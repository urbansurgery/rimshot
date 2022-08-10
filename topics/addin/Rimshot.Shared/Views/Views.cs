using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Interop;
using Speckle.Newtonsoft.Json;
using System;
using System.Windows;
using NavisworksApp = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Views {
  public class Views {
    /// Show only items in Selection Set
    internal static void ShowSelectionSet_COM ( SelectionSet selectionSet ) => LcOpSelectionSetsElement.MakeVisible( NavisworksApp.MainDocument.State, selectionSet );
    /// Show only items in Model Item Collection 
    internal static void ShowSelectionSet_COM ( ModelItemCollection modelItems ) => LcOpSelectionSetsElement.MakeVisible( NavisworksApp.MainDocument.State, new SelectionSet( modelItems ) );
  }

  public class ImageViewpoint {

    [JsonProperty]
    internal Point3D CameraViewpoint { get; }
    [JsonProperty]
    internal Vector3D CameraDirection { get; }
    [JsonProperty]
    internal Vector3D CameraUpVector { get; }
    [JsonProperty]
    internal double FieldOfView { get; }
    [JsonProperty]
    internal double AspectRatio { get; }

    [JsonProperty]
    internal object ClippingPlanes { get; }
    [JsonProperty]
    internal string CameraType { get; }

    public ImageViewpoint ( SavedViewpoint view ) {
      string type = "";
      string zoom = "";
      double zoomValue = 1;
      double units = Conversions.Units.GetUnits();

      Viewpoint vp = view.Viewpoint.CreateCopy();
      ViewpointProjection projection = vp.Projection;

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
          Console.WriteLine( $"No Focal Distance, Are you looking at anything?\n{err.Message}" );
        }
      } else {
        _ = MessageBox.Show( "No View" );
      }

      this.FieldOfView = vp.HeightField;
      this.AspectRatio = vp.AspectRatio;

      object passedClippingPlanes = JsonConvert.DeserializeObject( NavisworksApp.ActiveDocument.ActiveView.GetClippingPlanes() );
      this.ClippingPlanes = passedClippingPlanes;

      System.Reflection.PropertyInfo prop = GetType().GetProperty( zoom );
      if ( prop != null && prop.CanWrite ) {
        prop.SetValue( this, zoomValue, null );
      }

      this.CameraType = type;

    }

    private Vector3D GetViewDir ( Viewpoint oVp ) {
      Rotation3D oRot = oVp.Rotation;
      // calculate view direction
      Rotation3D oNegativeZ = new Rotation3D( 0, 0, -1, 0 );
      Rotation3D oTempRotation = MultiplyRotation3D( oNegativeZ, oRot.Invert() );
      Rotation3D oViewDirRot = MultiplyRotation3D( oRot, oTempRotation );
      // get view direction
      Vector3D oViewDir = new Vector3D( oViewDirRot.A, oViewDirRot.B, oViewDirRot.C );

      return oViewDir.Normalize();
    }
    private Vector3D GetViewUp ( Viewpoint oVp ) {

      Rotation3D oRot = oVp.Rotation;
      // calculate view direction
      Rotation3D oNegativeZ = new Rotation3D( 0, 1, 0, 0 );
      Rotation3D oTempRotation = MultiplyRotation3D( oNegativeZ, oRot.Invert() );
      Rotation3D oViewDirRot = MultiplyRotation3D( oRot, oTempRotation );
      // get view direction
      Vector3D oViewDir = new Vector3D( oViewDirRot.A, oViewDirRot.B, oViewDirRot.C );

      return oViewDir.Normalize();
    }

    private Rotation3D MultiplyRotation3D ( Rotation3D r2, Rotation3D r1 ) =>
      new Rotation3D(
        r2.D * r1.A + r2.A * r1.D + r2.B * r1.C - r2.C * r1.B,
        r2.D * r1.B + r2.B * r1.D + r2.C * r1.A - r2.A * r1.C,
        r2.D * r1.C + r2.C * r1.D + r2.A * r1.B - r2.B * r1.A,
        r2.D * r1.D - r2.A * r1.A - r2.B * r1.B - r2.C * r1.C
      ).Normalize();
  }
}
