using Autodesk.Navisworks.Api;
using System;
using COMApi = Autodesk.Navisworks.Api.Interop.ComApi;

namespace Rimshot.Analysis {
  public class Triangle {

    private readonly Tuple<CoordPt, CoordPt, CoordPt> triangle;

    public Triangle ( CoordPt vertex1, CoordPt vertex2, CoordPt vertex3 ) {
      triangle = new Tuple<CoordPt, CoordPt, CoordPt>( vertex1, vertex2, vertex3 );
    }

    public Triangle ( COMApi.InwSimpleVertex v1, COMApi.InwSimpleVertex v2, COMApi.InwSimpleVertex v3 ) {

      CoordPt vertex1 = new CoordPt( v1 );
      CoordPt vertex2 = new CoordPt( v2 );
      CoordPt vertex3 = new CoordPt( v3 );

      this.triangle = new Tuple<CoordPt, CoordPt, CoordPt>( vertex1, vertex2, vertex3 );
    }

    public Triangle ToXYPlane () {

      CoordPt v1 = this.triangle.Item1;
      CoordPt v2 = this.triangle.Item2;
      CoordPt v3 = this.triangle.Item3;

      v1.Z = 0;
      v2.Z = 0;
      v3.Z = 0;

      return new Triangle( v1, v2, v3 );
    }
    public Triangle ToXZPlane () {

      CoordPt v1 = this.triangle.Item1;
      CoordPt v2 = this.triangle.Item2;
      CoordPt v3 = this.triangle.Item3;

      v1.Y = 0;
      v2.Y = 0;
      v3.Y = 0;

      return new Triangle( v1, v2, v3 );
    }
    public Triangle ToYZPlane () {

      CoordPt v1 = this.triangle.Item1;
      CoordPt v2 = this.triangle.Item2;
      CoordPt v3 = this.triangle.Item3;

      v1.X = 0;
      v2.X = 0;
      v3.X = 0;

      return new Triangle( v1, v2, v3 );
    }

    public BoundingBox3D BoundingBox () {

      BoundingBox3D bb = new BoundingBox3D();

      bb.Extend( triangle.Item1.ToPoint3D() );
      bb.Extend( triangle.Item1.ToPoint3D() );
      bb.Extend( triangle.Item1.ToPoint3D() );

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
      Array arrayVertex = ( Array )vertex.coord;

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
