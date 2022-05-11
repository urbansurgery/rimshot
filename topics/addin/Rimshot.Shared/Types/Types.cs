using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using COMApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;

namespace Rimshot.Analysis {

  public partial class ClashElements {
    public Dictionary<Zone, List<ClashPoint>> Clashpoints = new Dictionary<Zone, List<ClashPoint>>();

    public Dictionary<string, Tuple<BoundingBox3D, List<Zone>>> Zones = new Dictionary<string, Tuple<BoundingBox3D, List<Zone>>>();
    public Dictionary<string, Tuple<BoundingBox3D, List<Result>>> Tests = new Dictionary<string, Tuple<BoundingBox3D, List<Result>>>();
  }

  public class Test {

  }

  public class Result {
    public ClashResultGroup _result;

    public ClashTest Test { get; private set; }

    public string TestName { get; private set; }

    public string Name { get; private set; }

    public BoundingBox3D BoundingBox { get; private set; }

    public List<ClashPoint> Clashes { get; } = new List<ClashPoint>();

    public Result ( ClashResultGroup group ) {

      if ( group.IsGroup ) {
        ClashTest clashtest = ( ClashTest )group.Parent;

        Test = clashtest;
        _result = group;
        TestName = Test.DisplayName;
        BoundingBox = group.BoundingBox;
        Name = group.DisplayName;

        foreach ( ClashResult clash in group.Children ) {
          if ( clash.Status < ClashResultStatus.Approved ) {
            Clashes.Add( new ClashPoint( clash, group, clashtest ) );
          }
        }
      }
    }
  }

  public class ClashPoint {

    public ClashPoint ( ClashResult clash, ClashResultGroup Clashresult, ClashTest Clashtest ) {
      Point = clash.Center;
      Test = Clashtest;
      Result = Clashresult;
    }

    public Point3D Point { get; set; }

    public ClashTest Test { get; set; }

    public ClashResultGroup Result { get; set; }
  }

  public class Zone {
    private string _name;

    // Name should be a concatenation of the selection tree branches.
    public Zone ( string Name, List<SelectionSet> selection ) {

      BoundingBox3D bb = new BoundingBox3D();

      foreach ( SelectionSet z in selection ) {
        GetSelectedItems.Add( z );

        ModelItemCollection elements = z.GetSelectedItems();

        foreach ( ModelItem m in elements.ToList() ) {
          ZoneObject zo = new ZoneObject( Name, m );
          bb.Extend( zo.BoundingBox );
          ZoneObjects.Add( zo );
        }
      }

      BoundingBox = bb;
      _name = Name;
    }

    public List<ZoneObject> ZoneObjects { get; } = new List<ZoneObject>();

    public List<SelectionSet> GetSelectedItems { get; } = new List<SelectionSet>();

    public BoundingBox3D BoundingBox { get; private set; }

    public string Name {
      get {
        return _name;
      }
      set {
        if ( _name != value ) {
          _name = value;
        }
      }
    }
  }

  public class ZoneObject {
    public ZoneObject ( string Name, ModelItem item ) {

      this.Name = Name;
      ModelItem = item;
      BoundingBox = ModelItem.BoundingBox();

      COMApi.InwOaPath path = ComBridge.ToInwOaPath( ModelItem );
      //var fragments = (IEnumerable<COMApi.InwOaFragment3>) path.Fragments();

      GeometryCallback geocallback = new GeometryCallback();

      foreach ( COMApi.InwOaFragment3 frag in path.Fragments() ) {
        if ( IsSameInstance( path, frag ) ) {
          frag.GenerateSimplePrimitives( COMApi.nwEVertexProperty.eNORMAL, geocallback );

          Faces.AddRange( geocallback.Faces );
        }
      }
    }

    public BoundingBox3D BoundingBox { get; private set; }

    public string Name { get; private set; }

    public List<Triangle> Faces { get; } = new List<Triangle>();

    public ModelItem ModelItem { get; private set; }

    private bool IsSameInstance ( COMApi.InwOaPath oPath, COMApi.InwOaFragment3 oFrag ) {

      bool isSame = true;

      Array a1 = ( Array )oFrag.path.ArrayData;
      Array a2 = ( Array )oPath.ArrayData;

      if ( a1.GetLength( 0 ) == a2.GetLength( 0 ) &&
          a1.GetLowerBound( 0 ) == a2.GetLowerBound( 0 ) &&
          a1.GetUpperBound( 0 ) == a2.GetUpperBound( 0 ) ) {

        int i = a1.GetLowerBound( 0 );

        for ( ; i <= a1.GetLength( 0 ); i++ ) {

          int a1_value = ( int )a1.GetValue( i );
          int a2_value = ( int )a2.GetValue( i );

          if ( a1_value != a2_value ) {
            isSame = false;
            break;
          }
        }
      } else {
        isSame = false;
      }

      return isSame;
    }
  }

  public class Triangle {

    private readonly Tuple<CoordPt, CoordPt, CoordPt> _triangle;

    public Triangle ( CoordPt vertex1, CoordPt vertex2, CoordPt vertex3 ) {
      _triangle = new Tuple<CoordPt, CoordPt, CoordPt>( vertex1, vertex2, vertex3 );
    }

    public Triangle ( COMApi.InwSimpleVertex v1, COMApi.InwSimpleVertex v2, COMApi.InwSimpleVertex v3 ) {

      CoordPt vertex1 = new CoordPt( v1 );
      CoordPt vertex2 = new CoordPt( v2 );
      CoordPt vertex3 = new CoordPt( v3 );

      _triangle = new Tuple<CoordPt, CoordPt, CoordPt>( vertex1, vertex2, vertex3 );
    }

    public Triangle ToXYPlane () {

      CoordPt v1 = this._triangle.Item1;
      CoordPt v2 = this._triangle.Item2;
      CoordPt v3 = this._triangle.Item3;

      v1.Z = 0;
      v2.Z = 0;
      v3.Z = 0;

      return new Triangle( v1, v2, v3 );
    }
    public Triangle ToXZPlane () {

      CoordPt v1 = this._triangle.Item1;
      CoordPt v2 = this._triangle.Item2;
      CoordPt v3 = this._triangle.Item3;

      v1.Y = 0;
      v2.Y = 0;
      v3.Y = 0;

      return new Triangle( v1, v2, v3 );
    }
    public Triangle ToYZPlane () {

      CoordPt v1 = this._triangle.Item1;
      CoordPt v2 = this._triangle.Item2;
      CoordPt v3 = this._triangle.Item3;

      v1.X = 0;
      v2.X = 0;
      v3.X = 0;

      return new Triangle( v1, v2, v3 );
    }

    public BoundingBox3D BoundingBox () {

      BoundingBox3D bb = new BoundingBox3D();

      bb.Extend( _triangle.Item1.ToPoint3D() );
      bb.Extend( _triangle.Item1.ToPoint3D() );
      bb.Extend( _triangle.Item1.ToPoint3D() );

      return bb;
    }
  }

  public class CoordPt {

    Tuple<double, double, double> _coord;

    public CoordPt ( double X = 0, double Y = 0, double Z = 0 ) {
      _coord = new Tuple<double, double, double>( X, Y, Z );
    }

    public CoordPt ( Point3D vertex ) {
      _coord = new Tuple<double, double, double>( vertex.X, vertex.Y, vertex.Z );
    }

    public CoordPt ( COMApi.InwSimpleVertex vertex ) {
      Array arrayVertex = ( Array )( object )vertex.coord;

      double vX = Convert.ToDouble( arrayVertex.GetValue( 1 ) );
      double vY = Convert.ToDouble( arrayVertex.GetValue( 2 ) );
      double vZ = Convert.ToDouble( arrayVertex.GetValue( 3 ) );

      Tuple<double, double, double> t = new Tuple<double, double, double>( vX, vY, vZ );

      _coord = t;
    }

    public double X {
      get { return _coord.Item1; }
      set { _coord = new Tuple<double, double, double>( value, _coord.Item2, _coord.Item3 ); }
    }
    public double Y {
      get { return _coord.Item2; }
      set { _coord = new Tuple<double, double, double>( _coord.Item1, value, _coord.Item3 ); }
    }
    public double Z {
      get { return _coord.Item3; }
      set { _coord = new Tuple<double, double, double>( _coord.Item1, _coord.Item2, value ); }
    }

    public Point3D ToPoint3D () => new Point3D( this.X, this.Y, this.Z );

    public BoundingBox3D BoundingBox () {
      BoundingBox3D bb = new BoundingBox3D();
      bb.Extend( ToPoint3D() );
      return bb;
    }
  }
}
