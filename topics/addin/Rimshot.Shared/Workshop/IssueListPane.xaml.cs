using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Autodesk.Navisworks.Api;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;

using Path = System.IO.Path;
using Navis = Autodesk.Navisworks.Api.Application;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge; //
using System.Runtime.ExceptionServices;
using System.Globalization;

namespace Rimshot.Shared.Workshop {
  public class Image {
    [JsonProperty]
    internal string name;
    [JsonProperty]
    internal Guid guid;
    [JsonProperty]
    internal string image = "";
    [JsonProperty]
    internal string thumbnail = "";
    [JsonProperty]
    internal ImageViewpoint viewpoint;

    public Image ( SavedViewpoint view ) {
      this.name = view.DisplayName;
      this.guid = view.Guid;

      var vp = new ImageViewpoint( view );
      var viewpoint = JsonConvert.SerializeObject( vp );

      this.viewpoint = vp;
    }
  }
  public class ImageViewpoint {

    [JsonProperty]
    internal Point3D CameraViewpoint { get; private set; }
    [JsonProperty]
    internal Vector3D CameraDirection { get; private set; }
    [JsonProperty]
    internal Vector3D CameraUpVector { get; private set; }
    [JsonProperty]
    internal double FieldOfView { get; private set; }
    [JsonProperty]
    internal double AspectRatio { get; private set; }
    [JsonProperty]
    internal double ViewToWorldScale { get; private set; }
    [JsonProperty]
    internal object ClippingPlanes { get; private set; }
    [JsonProperty]
    internal string CameraType { get; private set; }

    private readonly Document oDoc = Navis.ActiveDocument;

    public ImageViewpoint ( SavedViewpoint view ) {
      string type = "";
      string zoom = "";
      double zoomValue = 1;
      double units = GetUnits();

      Viewpoint vp = view.Viewpoint.CreateCopy();
      ViewpointProjection projection = vp.Projection;

      //this.Guid = view.Guid.ToString();

      this.CameraDirection = GetViewDir( vp );
      this.CameraUpVector = GetViewUp( vp );
      this.CameraViewpoint = new Point3D(
        vp.Position.X / units,
        vp.Position.Y / units,
        vp.Position.Z / units
        );

      if ( projection == ViewpointProjection.Orthographic ) {
        type = "OrthogonalCamera";
        zoom = "ViewToWorldScale";

        double dist = vp.VerticalExtentAtFocalDistance / 2 / units;
        zoomValue = 3.125 * dist / this.CameraUpVector.Length;
      } else if ( projection == ViewpointProjection.Perspective ) {
        type = "PerspectiveCamera";
        zoom = "FieldOfView";

        try { zoomValue = vp.FocalDistance; } catch ( Exception err ) {
          Console.WriteLine( "No Focal Distance, Are you looking at anything?" );
        }
      } else {
        MessageBox.Show( "No View" );
      }

      this.FieldOfView = vp.HeightField;
      this.AspectRatio = vp.AspectRatio;

      object ClippingPlanes = JsonConvert.DeserializeObject( Navis.ActiveDocument.ActiveView.GetClippingPlanes() );
      this.ClippingPlanes = ClippingPlanes;

      System.Reflection.PropertyInfo prop = GetType().GetProperty( zoom );
      if ( prop != null && prop.CanWrite ) {
        prop.SetValue( this, zoomValue, null );
      }

      this.CameraType = type;

    }

    private Vector3D GetViewDir ( Viewpoint oVP ) {
      //double units = GetGunits();

      Rotation3D oRot = oVP.Rotation;
      // calculate view direction
      Rotation3D oNegtiveZ = new Rotation3D( 0, 0, -1, 0 );
      Rotation3D otempRot = MultiplyRotation3D( oNegtiveZ, oRot.Invert() );
      Rotation3D oViewDirRot = MultiplyRotation3D( oRot, otempRot );
      // get view direction
      Vector3D oViewDir = new Vector3D( oViewDirRot.A, oViewDirRot.B, oViewDirRot.C );

      return oViewDir.Normalize();
    }

    /// <summary>
    /// Get View Normal
    /// </summary>
    /// <param name="oVP"></param>
    /// <returns></returns>
    private Vector3D GetViewUp ( Viewpoint oVP ) {

      Rotation3D oRot = oVP.Rotation;
      // calculate view direction
      Rotation3D oNegtiveZ = new Rotation3D( 0, 1, 0, 0 );
      Rotation3D otempRot = MultiplyRotation3D( oNegtiveZ, oRot.Invert() );
      Rotation3D oViewDirRot = MultiplyRotation3D( oRot, otempRot );
      // get view direction
      Vector3D oViewDir = new Vector3D( oViewDirRot.A, oViewDirRot.B, oViewDirRot.C );

      return oViewDir.Normalize();
    }

    /// <summary>
    /// help function: Multiply two Rotation3D
    /// </summary>
    /// <param name="r2"></param>
    /// <param name="r1"></param>
    /// <returns></returns>
    private Rotation3D MultiplyRotation3D ( Rotation3D r2, Rotation3D r1 ) {

      Rotation3D oRot =
          new Rotation3D( r2.D * r1.A + r2.A * r1.D +
                              r2.B * r1.C - r2.C * r1.B,
                          r2.D * r1.B + r2.B * r1.D +
                              r2.C * r1.A - r2.A * r1.C,
                          r2.D * r1.C + r2.C * r1.D +
                              r2.A * r1.B - r2.B * r1.A,
                          r2.D * r1.D - r2.A * r1.A -
                              r2.B * r1.B - r2.C * r1.C );

      oRot.Normalize();

      return oRot;

    }
    public double GetUnits () {
      string units = oDoc.Units.ToString();
      double factor;
      switch ( units ) {
        case "Centimeters":
          factor = 100;
          break;
        case "Feet":
          factor = 3.28084;
          break;
        case "Inches":
          factor = 39.3701;
          break;
        case "Kilometers":
          factor = 0.001;
          break;
        case "Meters":
          factor = 1;
          break;
        case "Micrometers":
          factor = 1000000;
          break;
        case "Miles":
          factor = 0.000621371;
          break;
        case "Millimeters":
          factor = 1000;
          break;
        case "Mils":
          factor = 39370.0787;
          break;
        case "Yards":
          factor = 1.09361;
          break;
        default:
          MessageBox.Show( "Units " + units + " not recognized." );
          factor = 1;
          break;
      }
      return factor;
    }
  }


  public partial class IssueListPane : UserControl {
    private string _tempFolder = "";
    readonly Document oDoc = Navis.ActiveDocument;

    public class Bindings : UIBindings {




    }

    public Bindings bindings;

    public IssueListPane ( string address = Bindings.Url ) {

      InitializeCef();
      InitializeComponent();

      this.bindings = new Bindings {
        Browser = this.Browser,
        Window = this,
      };


      this.Browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
      this.Browser.JavascriptObjectRepository.Register( "UIBindings", this.bindings, isAsync: true, options: BindingOptions.DefaultBinder );

      this.Browser.Address = address;
    }
    private void InitializeCef () {
      if ( Cef.IsInitialized ) {
        return;
      }

      Cef.EnableHighDPISupport();
      CefSettings settings = new CefSettings() {
        CachePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "CefSharp\\Cache" )
      };

      try {
        Cef.Initialize( settings, performDependencyCheck: true, browserProcessHandler: null );
      } catch ( Exception e ) {
        _ = MessageBox.Show( e.ToString() );
      }
    }
    private void SendIssueView ( object sender, RoutedEventArgs e ) => this.bindings.AddImage();

    private void ShowDevTools ( object sender, EventArgs e ) => this.Browser.ShowDevTools();

    private void Refresh ( object sender, EventArgs e ) => this.Browser.Reload( true );


    public void SubscribeCurrentViewEvents () {
      Document oDoc = Navis.ActiveDocument;
      oDoc.CurrentViewpoint.Changed += CurrentViewpoint_Changed;
    }

    public void UnsubscribeCurrentViewEvents () {
      Document oDoc = Navis.ActiveDocument;
      oDoc.CurrentViewpoint.Changed -= CurrentViewpoint_Changed;
    }

    [HandleProcessCorruptedStateExceptions]
    private void CurrentViewpoint_Changed ( object sender, EventArgs e ) {

      if ( sender != null ) {

        Document doc = sender as Document;
        Viewpoint p = doc.CurrentViewpoint;

        try {
          string camera = p.GetCamera();
          this.bindings.UpdateView( camera );

        } catch ( AccessViolationException err ) {
          Console.WriteLine( "View camera accessed prematurely" );
          Console.WriteLine( err.Message );
        }
      }
    }
  }
}
