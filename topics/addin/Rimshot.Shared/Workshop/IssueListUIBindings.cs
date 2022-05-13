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

namespace Rimshot.Shared.Workshop {
  public abstract class UIBindings {
#if DEBUG
    const string rimshotUrl = "http://192.168.86.42:8080/issues";
#else
    const String rimshotUrl = "https://rimshot.app/issues";;Boolean debug = false;
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

    private void CommitSelectedObjectsToSpeckle ( object payload ) => throw new NotImplementedException();

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
