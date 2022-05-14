using System;
using System.Collections.Generic;
using System.Text;

using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge; //
using NavisApplication = Autodesk.Navisworks.Api.Application;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using Autodesk.Navisworks.Api;
using System.Linq;

namespace Rimshot.Shared {
  public class CallbackGeomListener : ComApi.InwSimplePrimitivesCB {
    public List<float> Coords { get; set; }
    public float[] matrix { get; set; }
    public CallbackGeomListener () {
      Coords = new List<float>();
    }
    public void Line ( ComApi.InwSimpleVertex v1,
                     ComApi.InwSimpleVertex v2 ) {

    }
    public void Point ( ComApi.InwSimpleVertex v1 ) {

    }
    public void SnapPoint ( ComApi.InwSimpleVertex v1 ) {

    }

    public void Triangle ( ComApi.InwSimpleVertex v1,
                         ComApi.InwSimpleVertex v2,
                         ComApi.InwSimpleVertex v3 ) {



      Array array_v1 = ( Array )( object )v1.coord;
      float v1_x = ( float )( array_v1.GetValue( 1 ) );
      float v1_y = ( float )( array_v1.GetValue( 2 ) );
      float v1_z = ( float )( array_v1.GetValue( 3 ) );

      Array array_v2 = ( Array )( object )v2.coord;
      float v2_x = ( float )( array_v2.GetValue( 1 ) );
      float v2_y = ( float )( array_v2.GetValue( 2 ) );
      float v2_z = ( float )( array_v2.GetValue( 3 ) );

      Array array_v3 = ( Array )( object )v3.coord;
      float v3_x = ( float )( array_v3.GetValue( 1 ) );
      float v3_y = ( float )( array_v3.GetValue( 2 ) );
      float v3_z = ( float )( array_v3.GetValue( 3 ) );

      //Matrix

      float w1 = matrix[ 3 ] * v1_x + matrix[ 7 ] * v1_y + matrix[ 11 ] * v1_z + matrix[ 15 ];

      var v1__x = ( matrix[ 0 ] * v1_x + matrix[ 4 ] * v1_y + matrix[ 8 ] * v1_z + matrix[ 12 ] ) / w1;
      var v1__y = ( matrix[ 1 ] * v1_x + matrix[ 5 ] * v1_y + matrix[ 9 ] * v1_z + matrix[ 13 ] ) / w1;
      var v1__z = ( matrix[ 2 ] * v1_x + matrix[ 6 ] * v1_y + matrix[ 10 ] * v1_z + matrix[ 14 ] ) / w1;


      float w2 = matrix[ 3 ] * v2_x + matrix[ 7 ] * v2_y + matrix[ 11 ] * v2_z + matrix[ 15 ];

      var v2__x = ( matrix[ 0 ] * v2_x + matrix[ 4 ] * v2_y + matrix[ 8 ] * v2_z + matrix[ 12 ] ) / w2;
      var v2__y = ( matrix[ 1 ] * v2_x + matrix[ 5 ] * v2_y + matrix[ 9 ] * v2_z + matrix[ 13 ] ) / w2;
      var v2__z = ( matrix[ 2 ] * v2_x + matrix[ 6 ] * v2_y + matrix[ 10 ] * v2_z + matrix[ 14 ] ) / w2;

      float w3 = matrix[ 3 ] * v3_x + matrix[ 7 ] * v3_y + matrix[ 11 ] * v3_z + matrix[ 15 ];

      var v3__x = ( matrix[ 0 ] * v3_x + matrix[ 4 ] * v3_y + matrix[ 8 ] * v3_z + matrix[ 12 ] ) / w3;
      var v3__y = ( matrix[ 1 ] * v3_x + matrix[ 5 ] * v3_y + matrix[ 9 ] * v3_z + matrix[ 13 ] ) / w3;
      var v3__z = ( matrix[ 2 ] * v3_x + matrix[ 6 ] * v3_y + matrix[ 10 ] * v3_z + matrix[ 14 ] ) / w3;



      Coords.Add( ( float )v1__x );
      Coords.Add( ( float )v1__y );
      Coords.Add( ( float )v1__z );

      Coords.Add( ( float )v2__x );
      Coords.Add( ( float )v2__y );
      Coords.Add( ( float )v2__z );

      Coords.Add( ( float )v3__x );
      Coords.Add( ( float )v3__y );
      Coords.Add( ( float )v3__z );
    }

  }

  public class NavisGeometry {
    private ModelItemCollection oModelColl { get; set; }
    private ComApi.InwOpSelection comSelection { get; set; }
    private ComApi.InwOpState oState = ComBridge.State;
    public NavisGeometry ( ModelItem modelItem ) {

      // Add conversion geometry to oModelColl Property
      ModelItemCollection modelitemCollection = new ModelItemCollection();
      modelitemCollection.Add( modelItem );
      //modelitemCollection = NavisApplication.ActiveDocument.CurrentSelection.SelectedItems;

      //convert to COM selection
      this.comSelection = ComBridge.ToInwOpSelection( modelitemCollection );

    }
    public List<CallbackGeomListener> getFragments () {
      List<CallbackGeomListener> callbackListeners = new List<CallbackGeomListener>();
      // create the callback object

      foreach ( ComApi.InwOaPath3 path in comSelection.Paths() ) {
        CallbackGeomListener cbListener = new CallbackGeomListener();
        foreach ( ComApi.InwOaFragment3 frag in path.Fragments() ) {
          ComApi.InwLTransform3f3 localToWorld = ( ComApi.InwLTransform3f3 )( object )frag.GetLocalToWorldMatrix();

          //create Global Cordinate System Matrix
          Array array_v1 = ( Array )( object )localToWorld.Matrix;
          double[] elements = ToArray<double>( array_v1 );
          float[] elementsFloat = new float[ elements.Length ];
          for ( int i = 0; i < elements.Length; i++ ) {
            elementsFloat[ i ] = ( float )elements[ i ];
          }

          cbListener.matrix = elementsFloat;
          frag.GenerateSimplePrimitives( ComApi.nwEVertexProperty.eNORMAL, cbListener );
        }
        callbackListeners.Add( cbListener );
      }
      return callbackListeners;
    }
    public T[] ToArray<T> ( Array arr ) {
      T[] result = new T[ arr.Length ];
      Array.Copy( arr, result, result.Length );

      return result;
    }
  }

  public class Buffer {
    public string URI { get; set; }
    public int byteLength { get; set; }
    public List<BufferView> BufferViews { get; set; }
    public List<Accessor> Accessors { get; set; }
  }
  public class BufferView {
    public int buffer { get; set; }
    public int byteOffset { get; set; }
    public int byteLength { get; set; }
    public int target { get; set; }
    public BufferView ( int byteOffset, int byteLength, int target ) {
      this.byteOffset = byteOffset;
      this.byteLength = byteLength;
      this.target = target;
    }
  }
  public class Accessor {
    public int byteOffset { get; set; }
    public int componentType { get; set; }
    public int count { get; set; }
    public string type { get; set; }
    public string max { get; set; }
    public string min { get; set; }
    public Accessor ( int byteOffset, int componentType, int count, string type, string max, string min ) {
      this.byteOffset = byteOffset;
      this.componentType = componentType;
      this.count = count;
      this.type = type;
      this.max = max;
      this.min = min;
    }
  }

  public class Triangle {
    public Vertex Vertex1 { get; set; }
    public Vertex Vertex2 { get; set; }
    public Vertex Vertex3 { get; set; }
    public Triangle ( Vertex v1, Vertex v2, Vertex v3 ) {
      Vertex1 = v1;
      Vertex2 = v2;
      Vertex3 = v3;
    }
  }
  public class Vertex {
    public Vertex ( float x, float y, float z ) {
      X = x;
      Y = y;
      Z = z;
    }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
  }

  public class Mesh {
    public Buffer Buffer { get; set; }
    public List<int> Indices { get; set; }
    public List<float> vertices { get; set; }
    public List<Triangle> Triangles { get; set; }
    public Mesh ( List<Triangle> Triangles ) {
      this.Triangles = new List<Triangle>();
      this.Triangles = Triangles;

      //Add indices and vertices
      Indices = new List<int>();
      vertices = new List<float>();
      int indice = 0;

      //create buffer, bufferViews and accessors 
      Buffer = new Buffer();
      Buffer.Accessors = new List<Accessor>();
      Buffer.BufferViews = new List<BufferView>();
      var offset = 0;


      //create indices and vertices lists

      foreach ( var triangle in Triangles ) {
        Indices.Add( indice++ );
        Indices.Add( indice++ );
        Indices.Add( indice++ );
        vertices.Add( triangle.Vertex1.X );
        vertices.Add( triangle.Vertex1.Y );
        vertices.Add( triangle.Vertex1.Z );
        vertices.Add( triangle.Vertex2.X );
        vertices.Add( triangle.Vertex2.Y );
        vertices.Add( triangle.Vertex2.Z );
        vertices.Add( triangle.Vertex3.X );
        vertices.Add( triangle.Vertex3.Y );
        vertices.Add( triangle.Vertex3.Z );
      }

      //to get max and min coords
      var verXs = new List<float>();
      var verYs = new List<float>();
      var verZs = new List<float>();
      for ( int i = 0; i < vertices.Count(); i++ ) {
        if ( i % 3 == 0 ) {
          verXs.Add( vertices[ i ] );
        } else if ( i % 3 == 1 ) {
          verYs.Add( vertices[ i ] );
        } else {
          verZs.Add( vertices[ i ] );
        }
      }
      //create accessors
      //accessor indices
      var accessor1 = new Accessor( 0, 5123, Indices.Count(), "SCALAR", Convert.ToString( Indices.Count() - 1 ), "0" );
      var max = $"[{verXs.Max()},{verYs.Max()},{verZs.Max()}]".Replace( '/', '.' );
      var min = $"[{verXs.Min()},{verYs.Min()},{verZs.Min()}]".Replace( '/', '.' );

      //accessor vertices
      var accessor2 = new Accessor( 0, 5126, vertices.Count(), "VEC3", max, min );
      Buffer.Accessors.Add( accessor1 );
      Buffer.Accessors.Add( accessor2 );

      //create buffer byte array
      var Bytes = new List<byte>();
      foreach ( var i in Indices ) {
        if ( i < 256 ) {
          Bytes.Add( Convert.ToByte( i ) );
          Bytes.Add( Convert.ToByte( 0 ) );
        } else {
          Bytes.Add( Convert.ToByte( i % 256 ) );
          Bytes.Add( Convert.ToByte( i / 256 ) );
        }
      }
      var indicesByteLength = Bytes.Count();
      var BufferView = new BufferView( 0, indicesByteLength, 34963 );
      Buffer.BufferViews.Add( BufferView );
      if ( indicesByteLength % 4 != 0 ) {
        Bytes.Add( Convert.ToByte( 0 ) );
        Bytes.Add( Convert.ToByte( 0 ) );
        offset = 2;
      }
      foreach ( var v in vertices ) {
        var bytes = BitConverter.GetBytes( v );
        Bytes.AddRange( bytes );
      }
      Buffer.byteLength = Bytes.Count();

      var verticesByteLength = Buffer.byteLength - ( BufferView.byteLength + offset );
      var BufferView2 = new BufferView( indicesByteLength + offset, verticesByteLength, 34962 );
      Buffer.BufferViews.Add( BufferView2 );

      //create buffer URI String
      string base64 = Convert.ToBase64String( Bytes.ToArray() );
      var BufferPrefix = " \"uri\" : \"data:application/gltf-buffer;base64,";
      Buffer.URI = BufferPrefix + base64;

    }
  }
}



