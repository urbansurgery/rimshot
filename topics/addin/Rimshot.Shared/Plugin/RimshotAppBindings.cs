using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.DocumentParts;
using CefSharp;
using Rimshot.Conversions;
using Rimshot.Geometry;
using Rimshot.SpeckleApi;
using Rimshot.Views;
using Speckle.Core.Api;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using Navis = Autodesk.Navisworks.Api.Application;
using NavisworksApp = Autodesk.Navisworks.Api.Application;
using Props = Rimshot.Conversions.Properties;

namespace Rimshot {

  public abstract class UIBindings {
#if DEBUG
    //const string rimshotUrl = "http://192.168.86.29:8080/issues";
    const string rimshotUrl = "https://rimshot.app/issues";
#else
    const string rimshotUrl = "https://rimshot.app/issues";
#endif
    public const string Url = rimshotUrl;
    private const int Modulo = 5;
    private string _tempFolder = "";

    public IWebBrowser Browser { get; set; }

    public RimshotPane Window { get; set; }
    private string image;

    private readonly List<Tuple<NamedConstant, NamedConstant>> QuickPropertyDefinitions = new List<Tuple<NamedConstant, NamedConstant>>();
    private Base QuickProperties;

    // base64 encoding of the image
    public virtual string GetImage () => this.image;

    // base64 encoding of the image
    public virtual void SetImage ( string value ) => this.image = value;

    public virtual void NotifyUI ( string eventName, dynamic eventInfo ) {
      string script = string.Format( "window.EventBus.$emit('{0}',{1})", eventName, JsonConvert.SerializeObject( eventInfo ) );
      Browser.GetMainFrame().EvaluateScriptAsync( script );
    }

    public virtual void CommitStoreMutationUI ( string storeMutationName, string args = null ) {
      string script = string.Format( "window.Store.commit('{0}', '{1}')", storeMutationName, args );
      try {
        Browser.GetMainFrame().EvaluateScriptAsync( script );
      } catch ( Exception e ) {
        Logging.ErrorLog( e.Message );
      }
    }

    public virtual void DispatchStoreActionUI ( string storeActionName, string args = null ) {
      string script = string.Format( "window.Store.dispatch('{0}', '{1}')", storeActionName, args );
      try {
        Browser.GetMainFrame().EvaluateScriptAsync( script );
      } catch ( Exception e ) {
        Logging.ErrorLog( e.Message );
      }
    }

    public virtual void ShowDev () => this.Browser.ShowDevTools();
    public virtual void Refresh ( bool force = false ) {
      this.Browser.Reload( force );
      this.Browser.GetMainFrame().LoadUrl( Url );
    }

    public virtual void AddImage () => this.SendIssueView();

    public virtual void CommitSelection ( object payload ) => CommitSelectedObjectsToSpeckle( payload );

    public virtual string UpdateView ( string camera ) {
      Logging.ConsoleLog( camera );
      return camera;
    }

    public Document ActiveDocument { get; set; }

    private readonly Geometry.Geometry geometry = new Geometry.Geometry();
    private readonly SpeckleServer speckle = new SpeckleServer();

    /// <summary>
    /// Adds an hierarchical object based on the current selection and commits to the selected issue Branch. If no branch exists, one is created.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="commitMessage">Allows for the calling UI to override this and include a custom commit message.</param>
    private async void CommitSelectedObjectsToSpeckle ( object payload, string commitMessage = "Rimshot commit." ) {

      Logging.ConsoleLog( "Negotiating with the Speckle Server." );

      dynamic commitPayload = payload;

      this.speckle.HostUrl = commitPayload.host;
      this.speckle.rimshotIssueId = commitPayload.issueId;

      if ( speckle.HostUrl != speckle.Client.ServerUrl ) {
        Logging.ErrorLog( $"Host server Url for the issue ({speckle.HostUrl}) does not match the client server Url ({speckle.Client.ServerUrl}). Please check your configuration." );
        return;
      }

      this.geometry.GeometrySet.Clear();

      UIBindings app = this;

      speckle.RimshotApp = app;

      string description = $"issueId:{speckle.rimshotIssueId}";

      if ( speckle.Client == null ) { return; }

      await speckle.TryGetStream( commitPayload.stream );
      await speckle.TryGetBranch( commitPayload.branch, description );

      if ( speckle.Branch == null || speckle.Stream == null ) { return; };

      Logging.ConsoleLog( $"Stream: {speckle.StreamId}, Host: {speckle.HostUrl}, Branch: {speckle.BranchName}, Issue: {speckle.rimshotIssueId}" );


      // Current document, models and selected elements.
      ActiveDocument = NavisworksApp.ActiveDocument;
      DocumentModels documentModels = ActiveDocument.Models;
      ModelItemCollection appSelectedItems = NavisworksApp.ActiveDocument.CurrentSelection.SelectedItems;

      // Were the selection to change mid-commit, accessing the selected set would fail.

      appSelectedItems.CopyTo( this.geometry.selectedItems );
      appSelectedItems.DescendantsAndSelf.CopyTo( this.geometry.selectedItemsAndDescendants );

      if ( this.geometry.selectedItems.IsEmpty ) {
        Logging.ConsoleLog( "Nothing Selected." );
        NotifyUI( "error", JsonConvert.SerializeObject( new { message = "Nothing Selected." } ) );
        NotifyUI( "commit_sent", new { commit = "", speckle.rimshotIssueId, stream = speckle.StreamId, objectId = "" } );
        return;
      }

      // Setup Setting Out Location and translation
      ModelItem firstItem = this.geometry.selectedItems.First();
      ModelItem root = firstItem.Parent ?? firstItem;
      ModelGeometry temp = root.FindFirstGeometry();

      BoundingBox3D modelBoundingBox = temp.BoundingBox;
      Point3D center = modelBoundingBox.Center;

      this.geometry.TransformVector3D = new Vector3D( -center.X, -center.Y, 0 ); // 2D translation parallel to world XY

      this.geometry.SettingOutPoint = new Objects.Geometry.Vector {
        x = modelBoundingBox.Min.X,
        y = modelBoundingBox.Min.Y,
        z = 0
      };

      this.geometry.TransformVector = new Objects.Geometry.Vector {
        x = 0 - geometry.TransformVector3D.X,
        y = 0 - geometry.TransformVector3D.Y,
        z = 0
      };

      // Gather the Quick Properties for multi branch persistance
      this.QuickPropertyDefinitions.AddRange( Props.LoadQuickProperties().Distinct().ToList() );

      // This will be the Elements for the commit.
      List<Base> translatedElements = new List<Base>();

      // Thread safe collections
      // Iterate the selected elements regardless of their position in the tree.
      HashSet<NavisGeometry> geometrySet = new HashSet<NavisGeometry>();

      HashSet<ModelItem> firstObjectsInSelection = new HashSet<ModelItem>();
      ConcurrentStack<Base> UniqueGeometryNodes = new ConcurrentStack<Base>();
      ConcurrentDictionary<NavisGeometry, bool> geometryDict = new ConcurrentDictionary<NavisGeometry, bool>();
      ConcurrentDictionary<ModelItem, NavisGeometry> geometryNodeMap = new ConcurrentDictionary<ModelItem, NavisGeometry>();

      int elementCount = this.geometry.selectedItems.Count;

      for ( int e = 0; e < elementCount; e++ ) {

        ModelItem element = this.geometry.selectedItems[ e ];
        List<ModelItem> geometryNodes = this.geometry.CollectGeometryNodes( element ); // All relevant geometry nodes as children of whatever is selected.

        foreach ( ModelItem n in geometryNodes ) {
          NavisGeometry g = new NavisGeometry( n );
          bool addedGeometry = geometryDict.TryAdd( g, true );
          geometryNodeMap.TryAdd( n, g );
        }
      }

      ConcurrentStack<bool> doneElements = new ConcurrentStack<bool>();

      List<Task> fragmentTasks = new List<Task>();
      foreach ( KeyValuePair<NavisGeometry, bool> entry in geometryDict ) {
        fragmentTasks.Add( Task.Run( () => {
          this.geometry.AddFragments( entry.Key );
          doneElements.Push( true );
          Logging.ConsoleLog( $"{doneElements.Count} of {geometryDict.Count}", ConsoleColor.DarkGreen );
        } ) );
      }
      Task t = Task.WhenAll( fragmentTasks );

      t.Wait();

      // Builds the geometries of interest.
      //geometrySet.UnionWith( geometryDict.Keys );

      //List<Task> translateGeometryTasks = new List<Task>();
      //doneElements.Clear();

      List<NavisGeometry> keyList = geometryDict.Keys.ToList();
      for ( int n = 0; n < keyList.Count; n++ ) {
        NavisGeometry navisGeometry = keyList[ n ];
        //foreach ( NavisGeometry navisGeometry in geometryDict.Keys ) {
        //translateGeometryTasks.Add( Task.Run( () => {
        // Do the geometry conversion work.
        this.geometry.TranslateGeometryElement( navisGeometry );

        QuickProperties = new Base();

        // Do the properties and hierarchy conversion work.
        TranslateHierarchyElement( navisGeometry );

        //doneElements.Push( true );
        NotifyUI( "element-progress", JsonConvert.SerializeObject( new { current = doneElements.Count, count = geometrySet.Count } ) );
        Logging.ConsoleLog( $"Element {n} of {geometryDict.Keys.Count}" );
        //} ) 
        //);
      }

      //t = Task.WhenAll( translateGeometryTasks );
      //t.Wait();
      // At this point we have a Dict with unique selected geometry nodes. These don't reflect the Object hierarchy.

      // For each Geometry node now:
      // Navigate up the tree to find the FirstObject Ancestor. (If none, the geometry node is the first object.
      // The First Objects can then be traversed downward, nesting objects as we go.
      // When a child is a geometry child, we already have that node isolated.
      // Properties are then populated per branch child. (Property Categories + Geometry + ???) to "Parameters"
      // Properties of the ancestors of a FirstObject can be added directly to the FirstObject Base aggregating into lists if necessary

      Base myCommit = new Base();

      List<Base> Elements = new List<Base>();
      foreach ( NavisGeometry e in geometryDict.Keys ) {
        Elements.Add( e.Base );
      }

      myCommit[ "@Elements" ] = Elements;

      this.geometry.selectedItems.Clear();

      string[] stringseparators = new string[] { "/" };
      myCommit[ "Issue Number" ] = speckle.BranchName.Split( stringseparators, StringSplitOptions.None )[ 1 ];
      myCommit[ "applicationId" ] = speckle.rimshotIssueId;

      ServerTransport transport = new ServerTransport( speckle.Account, speckle.StreamId );
      string hash = Operations.Send( myCommit, new List<ITransport> { transport } ).Result;

      string commitId = speckle.Client.CommitCreate( new CommitCreateInput() {
        branchName = this.speckle.BranchName,
        message = commitMessage,
        objectId = hash,
        streamId = this.speckle.StreamId,
        sourceApplication = "Rimshot"
      } ).Result;

      Commit commitObject = speckle.Client.CommitGet( speckle.StreamId, commitId ).Result;
      string referencedObject = commitObject.referencedObject;

      NotifyUI( "commit_sent", new { commit = commitId, this.speckle.rimshotIssueId, stream = this.speckle.StreamId, objectId = referencedObject } );
    }

    public void TranslateHierarchyElement ( NavisGeometry geometrynode ) {

      ModelItem firstObject = geometrynode.ModelItem.FindFirstObjectAncestor();

      // Geometry Object may be the first object.
      if ( firstObject == null ) {
        // Process the Geometry Base object as the root object.
        geometrynode.Base = ToSpeckle.BuildBaseObjectTree( geometrynode.ModelItem, geometrynode, this.QuickPropertyDefinitions, ref QuickProperties );
      } else {
        geometrynode.Base = ToSpeckle.BuildBaseObjectTree( firstObject, geometrynode, this.QuickPropertyDefinitions, ref QuickProperties );
      }
    }

    private void SendIssueView () {
      MemoryStream stream;
      byte[] imageBytes;
      Guid id = Guid.NewGuid();
      SavedViewpoint oNewViewPt1 = new SavedViewpoint( Navis.ActiveDocument.CurrentViewpoint.ToViewpoint() ) {
        Guid = id,
        DisplayName = string.Format( "View - {0}", id )
      };
      Navis.ActiveDocument.SavedViewpoints.AddCopy( oNewViewPt1 );
      Snapshot issueSnapshot = new Snapshot( oNewViewPt1 ) { name = "View" };

      ComApi.InwOpState10 oState = ComBridge.State;
      ComApi.InwOaPropertyVec options = oState.GetIOPluginOptions( "lcodpimage" );

      this._tempFolder = Path.Combine( Path.GetTempPath(), "Rimshot.ExportBCF", Path.GetRandomFileName() );

      string snapshotFolder = Path.Combine( this._tempFolder, issueSnapshot.guid.ToString() );
      string snapshotFile = Path.Combine( snapshotFolder, issueSnapshot.name + ".png" );

      if ( !Directory.Exists( this._tempFolder ) ) {
        Directory.CreateDirectory( this._tempFolder );
      }

      Directory.CreateDirectory( snapshotFolder );

      foreach ( ComApi.InwOaProperty opt in options.Properties() ) {
        if ( opt.name == "export.image.format" ) {
          opt.value = "lcodpexpng";
        }

        if ( opt.name == "export.image.width" ) {
          opt.value = 1600;
        }

        if ( opt.name == "export.image.height" ) {
          opt.value = 900;
        }
      }
      oState.DriveIOPlugin( "lcodpimage", snapshotFile, options );

      try {
        stream = new MemoryStream();

        Bitmap oBitmap = new Bitmap( snapshotFile );
        Bitmap tBitmap = new Bitmap( snapshotFile );

        oBitmap.Save( stream, System.Drawing.Imaging.ImageFormat.Jpeg );
        imageBytes = stream.ToArray();
        issueSnapshot.image = Convert.ToBase64String( imageBytes );

        ImageViewpoint viewpoint = new ImageViewpoint( oNewViewPt1 );

      } catch ( Exception err ) {
        _ = MessageBox.Show( err.Message );
      }
      string imageString = JsonConvert.SerializeObject( issueSnapshot );

      SetImage( imageString );
      NotifyUI( "new-image", imageString );
    }
  }
}
