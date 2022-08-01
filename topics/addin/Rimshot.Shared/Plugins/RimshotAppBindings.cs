using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.DocumentParts;
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
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using Navis = Autodesk.Navisworks.Api.Application;
using NavisworksApp = Autodesk.Navisworks.Api.Application;
using Props = Rimshot.Conversions.Properties;

namespace Rimshot.Bindings {
  public abstract class RimshotAppBindings : DefaultBindings {

    public RimshotAppBindings () => AppName = "Rimshot";

#if DEBUGUI
    const string rimshotUrl = "http://192.168.86.29:8080/issues";
#else
    const string rimshotUrl = "https://rimshot.app/issues";
#endif
    public new const string Url = rimshotUrl;
    private const int Modulo = 5;
    private string _tempFolder = "";

    public RimshotPane Window { get; set; }
    private string image;

    private readonly List<Tuple<NamedConstant, NamedConstant>> QuickPropertyDefinitions = new List<Tuple<NamedConstant, NamedConstant>>();
    private Base QuickProperties;
    private readonly SpeckleServer SpeckleServer = new SpeckleServer();

    // base64 encoding of the image
    public virtual string GetImage () => this.image;

    // base64 encoding of the image
    public virtual void SetImage ( string value ) => this.image = value;

    public virtual void AddImage () => SendIssueView();

    public virtual string UpdateView ( string camera ) {
      Logging.ConsoleLog( camera );
      return camera;
    }

    public Document ActiveDocument { get; set; }

    /// <summary>
    /// Adds an hierarchical object based on the current selection and commits to the selected issue Branch. If no branch exists, one is created.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="commitMessage">Allows for the calling UI to override this and include a custom commit message.</param>
    public async void CommitSelection ( object payload, string commitMessage = "Rimshot commit." ) {

      Geometry.Geometry geometry = new Geometry.Geometry();

      Logging.ConsoleLog( "Negotiating with the Speckle Server." );

      dynamic commitPayload = payload;

      this.SpeckleServer.HostUrl = commitPayload.host;
      this.SpeckleServer.rimshotIssueId = commitPayload.issueId;

      if ( this.SpeckleServer.HostUrl != this.SpeckleServer.Client.ServerUrl ) {
        Logging.ErrorLog( $"Host server Url for the issue ({this.SpeckleServer.HostUrl}) does not match the client server Url ({this.SpeckleServer.Client.ServerUrl}). Please check your configuration." );
        return;
      }

      RimshotAppBindings app = this;
      this.SpeckleServer.App = app;

      string description = $"issueId:{this.SpeckleServer.rimshotIssueId}";

      if ( this.SpeckleServer.Client == null ) { return; }

      await this.SpeckleServer.TryGetStream( commitPayload.stream );
      await this.SpeckleServer.TryGetBranch( commitPayload.branch, description );

      if ( this.SpeckleServer.Branch == null || this.SpeckleServer.Stream == null ) { return; };

      Logging.ConsoleLog( $"Stream: {this.SpeckleServer.StreamId}, Host: {this.SpeckleServer.HostUrl}, Branch: {this.SpeckleServer.BranchName}, Issue: {this.SpeckleServer.rimshotIssueId}" );

      // Current document, models and selected elements.
      this.ActiveDocument = NavisworksApp.ActiveDocument;
      DocumentModels documentModels = this.ActiveDocument.Models;
      ModelItemCollection appSelectedItems = NavisworksApp.ActiveDocument.CurrentSelection.SelectedItems;

      // Were the selection to change mid-commit, accessing the selected set would fail.
      geometry.selectedItems.Clear();
      appSelectedItems.CopyTo( geometry.selectedItems );
      appSelectedItems.DescendantsAndSelf.CopyTo( geometry.selectedItemsAndDescendants );

      if ( geometry.selectedItems.IsEmpty ) {
        Logging.ConsoleLog( "Nothing Selected." );
        NotifyUI( "error", JsonConvert.SerializeObject( new { message = "Nothing Selected." } ) );
        NotifyUI( "commit_sent", new {
          commitId = "",
          issueId = this.SpeckleServer.rimshotIssueId,
          streamId = this.SpeckleServer.StreamId,
          objectId = ""
        } );
        return;
      }

      // Setup Setting Out Location and translation
      ModelItem firstItem = geometry.selectedItems.First();
      ModelItem root = firstItem.Parent ?? firstItem;
      ModelGeometry temp = root.FindFirstGeometry();

      BoundingBox3D modelBoundingBox = temp.BoundingBox;
      Point3D center = modelBoundingBox.Center;

      // 2D translation parallel to world XY
      geometry.TransformVector3D = new Vector3D( -center.X, -center.Y, 0 );

      geometry.SettingOutPoint = new Objects.Geometry.Vector {
        x = modelBoundingBox.Min.X,
        y = modelBoundingBox.Min.Y,
        z = 0
      };

      geometry.TransformVector = new Objects.Geometry.Vector {
        x = 0 - geometry.TransformVector3D.X,
        y = 0 - geometry.TransformVector3D.Y,
        z = 0
      };

      this.QuickPropertyDefinitions.Clear();
      this.QuickPropertyDefinitions.AddRange( Props.LoadQuickProperties().Distinct().ToList() );

      List<Base> translatedElements = new List<Base>();

      // Thread safe collections
      // Iterate the selected elements regardless of their position in the tree.
      ConcurrentDictionary<NavisGeometry, bool> uniqueGeometryNodes = new ConcurrentDictionary<NavisGeometry, bool>();
      ConcurrentBag<Base> elementsToCommit = new ConcurrentBag<Base>();

      ConcurrentStack<bool> doneSelectedElements = new ConcurrentStack<bool>();
      ConcurrentStack<bool> doneConvertedElements = new ConcurrentStack<bool>();

      int selectedElementCount = geometry.selectedItems.Count;

      List<Task> conversionTasks = new List<Task>();

      foreach ( ModelItem modelItemToConvert in geometry.selectedItems ) {

        conversionTasks.Add( Task.Run( () => {
          // All relevant geometry nodes as children of whatever is selected.
          List<ModelItem> geometryNodes = geometry.CollectGeometryNodes( modelItemToConvert );

          int nodeCount = geometryNodes.Count;
          foreach ( ModelItem geometryNode in geometryNodes ) {
            NavisGeometry nodeNavisGeometry = new NavisGeometry( geometryNode );
            bool addedGeometry = uniqueGeometryNodes.TryAdd( nodeNavisGeometry, true );

            if ( addedGeometry ) {
              geometry.AddFragments( nodeNavisGeometry );
            }

            geometry.TranslateGeometryElement( nodeNavisGeometry );

            this.QuickProperties = new Base();

            // Do the properties and hierarchy conversion work.
            TranslateHierarchyElement( nodeNavisGeometry );

            doneConvertedElements.Push( true );
            if ( doneConvertedElements.Count > 0 && doneConvertedElements.Count % 10 == 0 ) {
              Logging.ConsoleLog( $"Element {doneConvertedElements.Count} of {nodeCount} " +
                $"for Selected Element {doneSelectedElements.Count} of {selectedElementCount}",
                ConsoleColor.DarkBlue );
            }
            elementsToCommit.Add( nodeNavisGeometry.Base );
          }

          doneSelectedElements.Push( true );
        } ) );
      }

      Task.WhenAll( conversionTasks ).Wait();

      Base myCommit = new Base();

      myCommit[ "@Elements" ] = elementsToCommit.ToList();

      string[] stringseparators = new string[] { "/" };
      myCommit[ "Issue Number" ] = this.SpeckleServer.BranchName.Split( stringseparators, StringSplitOptions.None )[ 1 ];
      myCommit[ "applicationId" ] = this.SpeckleServer.rimshotIssueId;

      ServerTransport transport = new ServerTransport( this.SpeckleServer.Account, this.SpeckleServer.StreamId );
      string hash = Operations.Send( myCommit, new List<ITransport> { transport } ).Result;

      string commitId = this.SpeckleServer.Client.CommitCreate( new CommitCreateInput() {
        branchName = this.SpeckleServer.BranchName,
        message = commitMessage,
        objectId = hash,
        streamId = this.SpeckleServer.StreamId,
        sourceApplication = "Rimshot"
      } ).Result;

      Commit commitObject = this.SpeckleServer.Client.CommitGet( this.SpeckleServer.StreamId, commitId ).Result;
      string referencedObject = commitObject.referencedObject;

      NotifyUI( "commit_sent", new {
        commitId,
        issueId = this.SpeckleServer.rimshotIssueId,
        streamId = this.SpeckleServer.StreamId,
        objectId = referencedObject
      } );

      // Cleanup - No idea if it is all necessary 
      try {
        geometry.selectedItems.Clear();
        geometry.selectedItemsAndDescendants.Clear();
        geometry.pathDictionary.Clear();
        uniqueGeometryNodes.Clear();
        this.QuickPropertyDefinitions.Clear();
        elementsToCommit = new ConcurrentBag<Base>();
        uniqueGeometryNodes = new ConcurrentDictionary<NavisGeometry, bool>();
      } catch ( Exception e ) {
        Logging.ErrorLog( e.Message );
      }
    }
    [HandleProcessCorruptedStateExceptions, SecurityCritical]
    public void TranslateHierarchyElement ( NavisGeometry geometrynode ) {
      ModelItem firstObject;
      try {
        firstObject = geometrynode.ModelItem.FindFirstObjectAncestor();

      } catch ( Exception e ) {
        Console.WriteLine( e.Message );
        firstObject = null;
      }
      // Geometry Object may be the first object.
      if ( firstObject == null ) {
        // Process the Geometry Base object as the root object.
        geometrynode.Base = ToSpeckle.BuildBaseObjectTree( geometrynode.ModelItem, geometrynode, this.QuickPropertyDefinitions, ref this.QuickProperties );
      } else {
        geometrynode.Base = ToSpeckle.BuildBaseObjectTree( firstObject, geometrynode, this.QuickPropertyDefinitions, ref this.QuickProperties );
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

      foreach ( ComApi.InwOaProperty option in options.Properties() ) {
        if ( option.name == "export.image.format" ) {
          option.value = "lcodpexpng";
        }

        if ( option.name == "export.image.width" ) {
          option.value = 1600;
        }

        if ( option.name == "export.image.height" ) {
          option.value = 900;
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
