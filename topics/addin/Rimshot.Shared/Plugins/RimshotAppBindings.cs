using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.DocumentParts;
using Autodesk.Navisworks.Api.Interop.ComApi;
using Rimshot.ArrayExtensions;
using Rimshot.Conversions;
using Rimshot.Geometry;
using Rimshot.SpeckleApi;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Application = Autodesk.Navisworks.Api.Application;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using Navis = Autodesk.Navisworks.Api.Application;
using NavisworksApp = Autodesk.Navisworks.Api.Application;
using Props = Rimshot.Conversions.Properties;

namespace Rimshot.Bindings {
  public abstract class RimshotAppBindings : DefaultBindings {
    protected RimshotAppBindings () => this.AppName = "Rimshot";

#if DEBUGUI
    private const string RimshotUrl = "http://192.168.86.29:8080/issues";
#else
    const string rimshotUrl = "https://rimshot.app/issues";
#endif
    public new const string Url = RimshotUrl;
    private const int Modulo = 5;
    private string _tempFolder = "";

    public RimshotPane Window { get; set; }
    private string _image;

    private readonly List<Tuple<NamedConstant, NamedConstant>> _quickPropertyDefinitions = new List<Tuple<NamedConstant, NamedConstant>>();
    private Base _quickProperties;
    private readonly SpeckleServer _speckleServer = new SpeckleServer();

    // base64 encoding of the image
    public virtual string GetImage () => this._image;

    // base64 encoding of the image
    public virtual void SetImage ( string value ) => this._image = value;

    public virtual void AddImage () => SendIssueView();

    public virtual string UpdateView ( string camera ) {
      Logging.ConsoleLog( camera );
      return camera;
    }



    public Document ActiveDocument { get; set; }

    public void GetElements () {

      this.ActiveDocument = NavisworksApp.ActiveDocument;

      ConcurrentStack<ModelItem> itemCollection = new ConcurrentStack<ModelItem>();
      ModelItemCollection selectedItems = this.ActiveDocument.CurrentSelection.SelectedItems;

      ForegroundWorker geometryWorker = new ForegroundWorker();

      //List<Task> geometryTasks = new List<Task>();
      foreach ( ModelItem modelItem in selectedItems ) {
        //Task task = Task.Run( () => {
        //this.Context.Send( o => {
        IEnumerable<ModelItem> selectedItemsChildren = modelItem.DescendantsAndSelf.Where( mi => mi.HasGeometry && !mi.Ancestors.Any( a => a.IsHidden ) );
        foreach ( ModelItem m in selectedItemsChildren ) {
          itemCollection.Push( m );
        }
        //}, null );
        //} );
        //geometryTasks.Add( task );
      }
      //Task.WhenAll( geometryTasks ).Wait();

      ConcurrentDictionary<string, InwOaPath> paths = new ConcurrentDictionary<string, InwOaPath>();

      //List<Task> firstObjectTasks = new List<Task>();

      foreach ( ModelItem modelItem in itemCollection ) {
        //Task task = Task.Run( () => {
        //this.Context.Send( o => {
        ModelItem firstObject = modelItem.FindFirstObjectAncestor();
        InwOaPath path = ComBridge.ToInwOaPath( firstObject );
        object arrayData = path.ArrayData;
        int[] pathAsArray = ( ( Array )arrayData ).ToArray<int>();
        string pathAsString = string.Join( ".", pathAsArray );
        paths.TryAdd( pathAsString, path );
        //}, null );
        //} );
        //firstObjectTasks.Add( task );
      }

      //Task.WhenAll( firstObjectTasks ).Wait();

      List<ModelItem> modelItemsHydrated = paths.Select( path => ComBridge.ToModelItem( path.Value ) ).ToList();

      SelectionSet s = new SelectionSet();
      s.CopyFrom( modelItemsHydrated );

      SavedItem si = s;
      si.DisplayName = "Rimshot";
      si.MakeDisplayNameUnique( this.ActiveDocument.SelectionSets.RootItem );

      this.Context.Send( o => {
        if ( this.ActiveDocument == null ) {
          return;
        }

        this.ActiveDocument.CurrentSelection.Clear();
        this.ActiveDocument.CurrentSelection.AddRange( modelItemsHydrated );
        this.ActiveDocument.SelectionSets.AddCopy( si );
      }, null );
    }

    public IEnumerable<ModelItem> ItemsFromRoot ( Model model ) => model.RootItem.Descendants.Where( modelItem => modelItem.HasGeometry && modelItem.IsHidden );


    /// <summary>
    /// Adds an hierarchical object based on the current selection and commits to the selected issue Branch. If no branch exists, one is created.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="commitMessage">Allows for the calling UI to override this and include a custom commit message.</param>
    public async void CommitSelection ( object payload, string commitMessage = "Rimshot commit." ) {

      GetElements();

      return;

      Geometry.Geometry geometry = new Geometry.Geometry();

      Logging.ConsoleLog( "Negotiating with the Speckle Server." );

      dynamic commitPayload = payload;

      this._speckleServer.HostUrl = commitPayload.host;
      this._speckleServer.rimshotIssueId = commitPayload.issueId;

      if ( this._speckleServer.HostUrl != this._speckleServer.Client.ServerUrl ) {
        Logging.ErrorLog( $"Host server Url for the issue ({this._speckleServer.HostUrl}) does not match the client server Url ({this._speckleServer.Client.ServerUrl}). Please check your configuration." );
        return;
      }

      RimshotAppBindings app = this;
      this._speckleServer.App = app;

      string description = $"issueId:{this._speckleServer.rimshotIssueId}";

      if ( this._speckleServer.Client == null ) { return; }

      await this._speckleServer.TryGetStream( commitPayload.stream );
      await this._speckleServer.TryGetBranch( commitPayload.branch, description );

      if ( this._speckleServer.Branch == null || this._speckleServer.Stream == null ) { return; };

      Logging.ConsoleLog( $"Stream: {this._speckleServer.StreamId}, Host: {this._speckleServer.HostUrl}, Branch: {this._speckleServer.BranchName}, Issue: {this._speckleServer.rimshotIssueId}" );

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
        NotifyUi( "error", JsonConvert.SerializeObject( new { message = "Nothing Selected." } ) );
        NotifyUi( "commit_sent", new {
          commitId = "",
          issueId = this._speckleServer.rimshotIssueId,
          streamId = this._speckleServer.StreamId,
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

      this._quickPropertyDefinitions.Clear();
      this._quickPropertyDefinitions.AddRange( Props.LoadQuickProperties().Distinct().ToList() );

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

            this._quickProperties = new Base();

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

      Base myCommit = new Base { [ "@Elements" ] = elementsToCommit.ToList() };

      string[] stringSeparators = new string[] { "/" };
      myCommit[ "Issue Number" ] = this._speckleServer.BranchName.Split( stringSeparators, StringSplitOptions.None )[ 1 ];
      myCommit[ "applicationId" ] = this._speckleServer.rimshotIssueId;

      ServerTransport transport = new ServerTransport( this._speckleServer.Account, this._speckleServer.StreamId );
      string hash = Operations.Send( myCommit, new List<ITransport> { transport } ).Result;

      string commitId = this._speckleServer.Client.CommitCreate( new CommitCreateInput() {
        branchName = this._speckleServer.BranchName,
        message = commitMessage,
        objectId = hash,
        streamId = this._speckleServer.StreamId,
        sourceApplication = "Rimshot"
      } ).Result;

      Commit commitObject = this._speckleServer.Client.CommitGet( this._speckleServer.StreamId, commitId ).Result;
      string referencedObject = commitObject.referencedObject;

      NotifyUi( "commit_sent", new {
        commitId,
        issueId = this._speckleServer.rimshotIssueId,
        streamId = this._speckleServer.StreamId,
        objectId = referencedObject
      } );

      // Cleanup - No idea if it is all necessary 
      try {
        geometry.selectedItems.Clear();
        geometry.selectedItemsAndDescendants.Clear();
        geometry.pathDictionary.Clear();
        uniqueGeometryNodes.Clear();
        this._quickPropertyDefinitions.Clear();
        elementsToCommit = new ConcurrentBag<Base>();
        uniqueGeometryNodes = new ConcurrentDictionary<NavisGeometry, bool>();
      } catch ( Exception e ) {
        Logging.ErrorLog( e.Message );
      }
    }
    [HandleProcessCorruptedStateExceptions, SecurityCritical]
    public void TranslateHierarchyElement ( NavisGeometry geometryNode ) {
      ModelItem firstObject;
      try {
        firstObject = geometryNode.ModelItem.FindFirstObjectAncestor();

      } catch ( Exception e ) {
        Console.WriteLine( e.Message );
        firstObject = null;
      }
      // Geometry Object may be the first object.
      // Process the Geometry Base object as the root object.
      geometryNode.Base = firstObject == null ? ToSpeckle.BuildBaseObjectTree( geometryNode.ModelItem, geometryNode, this._quickPropertyDefinitions, ref this._quickProperties ) : ToSpeckle.BuildBaseObjectTree( firstObject, geometryNode, this._quickPropertyDefinitions, ref this._quickProperties );
    }

    private async void SendIssueView () {
      Guid id = Guid.NewGuid();
      SavedViewpoint oNewViewPt1 = new SavedViewpoint( Navis.ActiveDocument.CurrentViewpoint.ToViewpoint() ) {
        Guid = id,
        DisplayName = $"View - {id}"
      };
      Navis.ActiveDocument.SavedViewpoints.AddCopy( oNewViewPt1 );
      Snapshot issueSnapshot = new Snapshot( oNewViewPt1 ) { name = "View" };

      InwOpState10 oState = ComBridge.State;
      InwOaPropertyVec options = oState.GetIOPluginOptions( "lcodpimage" );

      this._tempFolder = Path.Combine( Path.GetTempPath(), "Rimshot.ExportBCF", Path.GetRandomFileName() );

      string snapshotFolder = Path.Combine( this._tempFolder, issueSnapshot.guid.ToString() );
      string snapshotFile = Path.Combine( snapshotFolder, issueSnapshot.name + ".png" );

      if ( !Directory.Exists( this._tempFolder ) ) {
        Directory.CreateDirectory( this._tempFolder );
      }

      Directory.CreateDirectory( snapshotFolder );

      foreach ( InwOaProperty option in options.Properties() ) {
        switch ( option.name ) {
          case "export.image.format":
            option.value = "lcodpexpng";
            break;
          case "export.image.width":
            option.value = 1600;
            break;
          case "export.image.height":
            option.value = 900;
            break;
        }
      }
      oState.DriveIOPlugin( "lcodpimage", snapshotFile, options );

      try {
        MemoryStream stream = new MemoryStream();

        Bitmap oBitmap = new Bitmap( snapshotFile );
        Bitmap tBitmap = new Bitmap( snapshotFile );

        oBitmap.Save( stream, System.Drawing.Imaging.ImageFormat.Jpeg );
        byte[] imageBytes = stream.ToArray();
        issueSnapshot.image = Convert.ToBase64String( imageBytes );

        oBitmap.Dispose();
        tBitmap.Dispose();

      } catch ( Exception err ) {
        _ = MessageBox.Show( err.Message );
      }
      string imageString = JsonConvert.SerializeObject( issueSnapshot );

      SetImage( imageString );
      NotifyUi( "new-image", imageString );

      try {

        int threadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine( $"Set View thread {threadId}" );

        //if ( this.UiThread.InvokeRequired ) {
        //  this.UiThread.Invoke( new GoDelegate( AddSelectionSet ) );
        //} else {
        //  AddSelectionSet();
        //}

        List<Task> tasks = new List<Task>();
        ConcurrentStack<bool> stack = new ConcurrentStack<bool>();
        for ( int i = 0; i < 100; i++ ) {
          tasks.Add( Task.Run( () => {
            ForegroundWorker fw = new ForegroundWorker();

            _ = fw.Send( o => {
              stack.Push( true );
              DocumentSelectionSets dss = Application.ActiveDocument.SelectionSets;
              ModelItemCollection m = new ModelItemCollection();
              SelectionSet s = new SelectionSet( m ) {
                DisplayName = $"Set No: {stack.Count}"
              };

              dss.AddCopy( s );

              fw.Response = $"Set: {stack.Count}";
            }, this.Context );

            _ = fw.Send( o => {
              IntPtr hWnd = Application.Gui.MainWindow.Handle;
              UpdateWindow( hWnd );
            }, this.Context );

            Console.WriteLine( $"{fw.Response}" );
          } ) );
        }

        await Task.WhenAll( tasks );
        Console.WriteLine( "All Gone." );

        //Navis.ActiveDocument.SelectionSets.AddCopy( s );
      } catch ( Exception e ) {
        Console.WriteLine( e.Message );
      }
    }


  }

  class ForegroundWorker {
    internal object Response { get; set; }

    internal object Send ( SendOrPostCallback function, SynchronizationContext context ) {
      context.Send( function, this.Response );

      return this.Response;
    }
    internal object Post ( SendOrPostCallback function, SynchronizationContext context ) {
      context.Post( function, this.Response );

      return this.Response;
    }
  }
}
