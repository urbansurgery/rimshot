using Autodesk.Navisworks.Api;
using System;
using COMApi = Autodesk.Navisworks.Api.Interop.ComApi;

namespace Rimshot.Analysis {
  public class Triangle {

    private readonly Tuple<CoordinatePt, CoordinatePt, CoordinatePt> _triangle;

    public Triangle ( CoordinatePt vertex1, CoordinatePt vertex2, CoordinatePt vertex3 ) {
      this._triangle = new Tuple<CoordinatePt, CoordinatePt, CoordinatePt>( vertex1, vertex2, vertex3 );
    }

    public Triangle ( COMApi.InwSimpleVertex v1, COMApi.InwSimpleVertex v2, COMApi.InwSimpleVertex v3 ) {

      CoordinatePt vertex1 = new CoordinatePt( v1 );
      CoordinatePt vertex2 = new CoordinatePt( v2 );
      CoordinatePt vertex3 = new CoordinatePt( v3 );

      this._triangle = new Tuple<CoordinatePt, CoordinatePt, CoordinatePt>( vertex1, vertex2, vertex3 );
    }

    public Triangle ToXyPlane () {

      CoordinatePt v1 = this._triangle.Item1;
      CoordinatePt v2 = this._triangle.Item2;
      CoordinatePt v3 = this._triangle.Item3;

      v1.Z = 0;
      v2.Z = 0;
      v3.Z = 0;

      return new Triangle( v1, v2, v3 );
    }
    public Triangle ToXzPlane () {

      CoordinatePt v1 = this._triangle.Item1;
      CoordinatePt v2 = this._triangle.Item2;
      CoordinatePt v3 = this._triangle.Item3;

      v1.Y = 0;
      v2.Y = 0;
      v3.Y = 0;

      return new Triangle( v1, v2, v3 );
    }
    public Triangle ToYzPlane () {

      CoordinatePt v1 = this._triangle.Item1;
      CoordinatePt v2 = this._triangle.Item2;
      CoordinatePt v3 = this._triangle.Item3;

      v1.X = 0;
      v2.X = 0;
      v3.X = 0;

      return new Triangle( v1, v2, v3 );
    }

    public BoundingBox3D BoundingBox () {

      BoundingBox3D bb = new BoundingBox3D();

      bb.Extend( this._triangle.Item1.ToPoint3D() );
      bb.Extend( this._triangle.Item1.ToPoint3D() );
      bb.Extend( this._triangle.Item1.ToPoint3D() );

      return bb;
    }
  }

  public class CoordinatePt {

    Tuple<double, double, double> _coordinate;

    public CoordinatePt ( double x = 0, double y = 0, double z = 0 ) {
      this._coordinate = new Tuple<double, double, double>( x, y, z );
    }

    public CoordinatePt ( Point3D vertex ) {
      this._coordinate = new Tuple<double, double, double>( vertex.X, vertex.Y, vertex.Z );
    }

    public CoordinatePt ( COMApi.InwSimpleVertex vertex ) {
      Array arrayVertex = ( Array )vertex.coord;

      double vX = Convert.ToDouble( arrayVertex.GetValue( 1 ) );
      double vY = Convert.ToDouble( arrayVertex.GetValue( 2 ) );
      double vZ = Convert.ToDouble( arrayVertex.GetValue( 3 ) );

      Tuple<double, double, double> t = new Tuple<double, double, double>( vX, vY, vZ );

      this._coordinate = t;
    }

    public double X {
      get => this._coordinate.Item1;
      set => this._coordinate = new Tuple<double, double, double>( value, this._coordinate.Item2, this._coordinate.Item3 );
    }
    public double Y {
      get => this._coordinate.Item2;
      set => this._coordinate = new Tuple<double, double, double>( this._coordinate.Item1, value, this._coordinate.Item3 );
    }
    public double Z {
      get => this._coordinate.Item3;
      set => this._coordinate = new Tuple<double, double, double>( this._coordinate.Item1, this._coordinate.Item2, value );
    }

    public Point3D ToPoint3D () => new Point3D( this.X, this.Y, this.Z );

    public BoundingBox3D BoundingBox () {
      BoundingBox3D bb = new BoundingBox3D();
      bb.Extend( ToPoint3D() );
      return bb;
    }
  }
}
