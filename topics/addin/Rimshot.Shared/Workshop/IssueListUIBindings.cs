using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using Navis = Autodesk.Navisworks.Api.Application;
using System.IO;
using Autodesk.Navisworks.Api;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge; //

using Speckle.Core.Models;
using Speckle.Core.Credentials;
using Speckle.Core.Transports;
using Speckle.Core.Api;
using System.Reflection;

namespace Rimshot.Shared.Workshop {
  public abstract class UIBindings {
#if DEBUG
    const string rimshotUrl = "https://rimshot.app/issues";
#else
    const String rimshotUrl = "https://rimshot.app/issues";
#endif
    public const string Url = rimshotUrl;


    private string _tempFolder = "";

    public IWebBrowser Browser { get; set; }
    public IssueListPane Window { get; set; }


    private string image;

    // base64 encoding of the image
    public virtual string GetImage () => this.image;

    // base64 encoding of the image
    public virtual void SetImage ( string value ) => this.image = value;

    public virtual void NotifyUI ( string eventName, dynamic eventInfo ) {
      string script = string.Format( "window.EventBus.$emit('{0}',{1})", eventName, JsonConvert.SerializeObject( eventInfo ) );
      Browser.GetMainFrame().EvaluateScriptAsync( script );
    }

    public virtual void DispatchStoreActionUI ( string storeActionName, string args = null ) {
      string script = string.Format( "window.Store.dispatch('{0}', '{1}')", storeActionName, args );
      Browser.GetMainFrame().EvaluateScriptAsync( script );
    }

    public virtual void ShowDev () => this.Browser.ShowDevTools();
    public virtual void Refresh ( bool force = false ) {
      this.Browser.Reload( force );
      this.Browser.GetMainFrame().LoadUrl( Url );
    }

    public virtual void AddImage () => this.SendIssueView();

    public virtual void CommitSelection ( object payload ) => this.CommitSelectedObjectsToSpeckle( payload );


    public virtual string UpdateView ( string camera ) {
      Console.WriteLine( camera );

      return camera;
    }

    private async void CommitSelectedObjectsToSpeckle ( object payload ) {

      Type payloadType = payload.GetType();

      PropertyInfo streamProp = payloadType.GetProperty( "stream" );
      PropertyInfo hostProp = payloadType.GetProperty( "host" );
      PropertyInfo branchProp = payloadType.GetProperty( "branch" );
      PropertyInfo issueProp = payloadType.GetProperty( "issueId" );

      var commitPayload = ( dynamic )payload;

      string branchName = commitPayload.branch;
      string streamId = commitPayload.stream;
      string host = commitPayload.host;
      string issueId = commitPayload.issueId;


      //{ "branch":"issues/xyz-2","host":"https://speckle.xyz","issueId":"au6SugFWUtOtv601Gekj","stream":"fd1d44405d"}

      //Objects.Geometry.Mesh mesh = new Objects.Geometry.Mesh();

      Objects.Geometry.Plane basePlane = new Objects.Geometry.Plane();
      Objects.Primitive.Interval xSize = new Objects.Primitive.Interval( 1.0, 2.0 );
      Objects.Primitive.Interval ySize = new Objects.Primitive.Interval( 1.0, 2.0 );
      Objects.Primitive.Interval zSize = new Objects.Primitive.Interval( 1.0, 2.0 );
      Objects.Geometry.Box box = new Objects.Geometry.Box( basePlane, xSize, ySize, zSize );

      List<Objects.Geometry.Box> displayValue = new List<Objects.Geometry.Box>();

      displayValue.Add( box );

      Base myCommit = new Base();

      myCommit[ "displayValue" ] = displayValue;

      string description = $"issueId:{issueId}";

      Account defaultAccount = AccountManager.GetDefaultAccount();

      Client client = new Client( defaultAccount );

      Branch branch;
      try {
        branch = await client.BranchGet( streamId, branchName );
        if ( branch is null ) {
          branch = await CreateBranch( client, streamId, branchName, description );
        }
      } catch ( Exception e ) {
        Console.WriteLine( $"Error: {e.Message}" );
      }

      //var branch = client.BranchGet( streamId, branchName, 1 ).Result;




    }

    public Task<Branch> CreateBranch ( Client client, string streamId, string branchName, string description ) {

      try {
        var branchId = client.BranchCreate( new BranchCreateInput() {
          streamId = streamId,
          name = branchName,
          description = description
        } );
      } catch ( Exception e ) {
        Console.WriteLine( $"Error: {e.Message}" );
      }

      var branch = client.BranchGet( streamId, branchName );

      Console.WriteLine( value: $"Branch: {branch}" );

      return branch;
      //try {
      //  var branchId = await Operations.StreamState.Client.BranchCreate( new BranchCreateInput() {
      //    streamId = StreamState.Stream.id,
      //    name = BranchName,
      //    description = BranchDescription
      //  } );
      //} catch ( Exception e ) {
      //  Notifications.Enqueue( $"Error: {e.Message}" );
      //  return;
      //}

      //var branch = await StreamState.Client.BranchGet( StreamState.Stream.id, BranchName );

      //DialogHost.CloseDialogCommand.Execute( branch, null );
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
      Image image = new Image( oNewViewPt1 ) { name = "View" };

      ComApi.InwOpState10 oState = ComBridge.State;
      ComApi.InwOaPropertyVec options = oState.GetIOPluginOptions( "lcodpimage" );

      _tempFolder = Path.Combine( Path.GetTempPath(), "Rimshot.ExportBCF", Path.GetRandomFileName() );

      string imageFolder = Path.Combine( this._tempFolder, image.guid.ToString() );
      string snapshot = Path.Combine( imageFolder, image.name + ".png" );

      if ( !Directory.Exists( this._tempFolder ) ) Directory.CreateDirectory( this._tempFolder );
      _ = Directory.CreateDirectory( imageFolder );

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
      oState.DriveIOPlugin( "lcodpimage", snapshot, options );

      try {
        stream = new MemoryStream();

        System.Drawing.Bitmap oBitmap = new System.Drawing.Bitmap( snapshot );
        System.Drawing.Bitmap tBitmap = new System.Drawing.Bitmap( snapshot );

        oBitmap.Save( stream, System.Drawing.Imaging.ImageFormat.Jpeg );
        imageBytes = stream.ToArray();
        image.image = Convert.ToBase64String( imageBytes );

        var viewpoint = new ImageViewpoint( oNewViewPt1 );

      } catch ( Exception err ) {
        _ = MessageBox.Show( err.Message );
      }
      string imageString = JsonConvert.SerializeObject( image );

      SetImage( imageString );
      NotifyUI( "new-image", imageString );
    }
  }
}
