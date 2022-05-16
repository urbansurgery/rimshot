using Autodesk.Navisworks.Api;
using System;
using System.Collections.Generic;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge; //

namespace Rimshot.Shared {
  public class CallbackGeomListener : ComApi.InwSimplePrimitivesCB {
    public List<float> Coords { get; set; }
    public float[] Matrix { get; set; }
    public CallbackGeomListener () {
      this.Coords = new List<float>();
    }
    public void Line ( ComApi.InwSimpleVertex v1, ComApi.InwSimpleVertex v2 ) { }
    public void Point ( ComApi.InwSimpleVertex v1 ) { }
    public void SnapPoint ( ComApi.InwSimpleVertex v1 ) { }

    public void Triangle ( ComApi.InwSimpleVertex v1, ComApi.InwSimpleVertex v2, ComApi.InwSimpleVertex v3 ) {



      Array array_v1 = ( Array )v1.coord;
      float v1_x = ( float )array_v1.GetValue( 1 );
      float v1_y = ( float )array_v1.GetValue( 2 );
      float v1_z = ( float )array_v1.GetValue( 3 );

      Array array_v2 = ( Array )v2.coord;
      float v2_x = ( float )array_v2.GetValue( 1 );
      float v2_y = ( float )array_v2.GetValue( 2 );
      float v2_z = ( float )array_v2.GetValue( 3 );

      Array array_v3 = ( Array )v3.coord;
      float v3_x = ( float )array_v3.GetValue( 1 );
      float v3_y = ( float )array_v3.GetValue( 2 );
      float v3_z = ( float )array_v3.GetValue( 3 );

      //Matrix

      float w1 = Matrix[ 3 ] * v1_x + Matrix[ 7 ] * v1_y + Matrix[ 11 ] * v1_z + Matrix[ 15 ];

      float v1__x = ( Matrix[ 0 ] * v1_x + Matrix[ 4 ] * v1_y + Matrix[ 8 ] * v1_z + Matrix[ 12 ] ) / w1;
      float v1__y = ( Matrix[ 1 ] * v1_x + Matrix[ 5 ] * v1_y + Matrix[ 9 ] * v1_z + Matrix[ 13 ] ) / w1;
      float v1__z = ( Matrix[ 2 ] * v1_x + Matrix[ 6 ] * v1_y + Matrix[ 10 ] * v1_z + Matrix[ 14 ] ) / w1;


      float w2 = Matrix[ 3 ] * v2_x + Matrix[ 7 ] * v2_y + Matrix[ 11 ] * v2_z + Matrix[ 15 ];

      float v2__x = ( Matrix[ 0 ] * v2_x + Matrix[ 4 ] * v2_y + Matrix[ 8 ] * v2_z + Matrix[ 12 ] ) / w2;
      float v2__y = ( Matrix[ 1 ] * v2_x + Matrix[ 5 ] * v2_y + Matrix[ 9 ] * v2_z + Matrix[ 13 ] ) / w2;
      float v2__z = ( Matrix[ 2 ] * v2_x + Matrix[ 6 ] * v2_y + Matrix[ 10 ] * v2_z + Matrix[ 14 ] ) / w2;

      float w3 = Matrix[ 3 ] * v3_x + Matrix[ 7 ] * v3_y + Matrix[ 11 ] * v3_z + Matrix[ 15 ];

      float v3__x = ( Matrix[ 0 ] * v3_x + Matrix[ 4 ] * v3_y + Matrix[ 8 ] * v3_z + Matrix[ 12 ] ) / w3;
      float v3__y = ( Matrix[ 1 ] * v3_x + Matrix[ 5 ] * v3_y + Matrix[ 9 ] * v3_z + Matrix[ 13 ] ) / w3;
      float v3__z = ( Matrix[ 2 ] * v3_x + Matrix[ 6 ] * v3_y + Matrix[ 10 ] * v3_z + Matrix[ 14 ] ) / w3;



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
    private ComApi.InwOpSelection ComSelection { get; set; }

    public NavisGeometry ( ModelItem modelItem ) {

      // Add conversion geometry to oModelColl Property
      ModelItemCollection modelitemCollection = new ModelItemCollection {
        modelItem
      };
      //modelitemCollection = NavisApplication.ActiveDocument.CurrentSelection.SelectedItems;

      //convert to COM selection
      this.ComSelection = ComBridge.ToInwOpSelection( modelitemCollection );

    }
    public List<CallbackGeomListener> GetFragments () {
      List<CallbackGeomListener> callbackListeners = new List<CallbackGeomListener>();
      // create the callback object

      foreach ( ComApi.InwOaPath3 path in ComSelection.Paths() ) {
        CallbackGeomListener callbackListener = new CallbackGeomListener();
        foreach ( ComApi.InwOaFragment3 fragment in path.Fragments() ) {
          ComApi.InwLTransform3f3 localToWorld = ( ComApi.InwLTransform3f3 )( object )fragment.GetLocalToWorldMatrix();

          //create Global Cordinate System Matrix
          Array array_v1 = ( Array )( object )localToWorld.Matrix;
          double[] elements = ToArray<double>( array_v1 );
          float[] elementsValue = new float[ elements.Length ];
          for ( int i = 0; i < elements.Length; i++ ) {
            elementsValue[ i ] = ( float )elements[ i ];
          }

          callbackListener.Matrix = elementsValue;
          fragment.GenerateSimplePrimitives( ComApi.nwEVertexProperty.eNORMAL, callbackListener );

        }
        callbackListeners.Add( callbackListener );
      }
      return callbackListeners;
    }
    public T[] ToArray<T> ( Array arr ) {
      T[] result = new T[ arr.Length ];
      Array.Copy( arr, result, result.Length );

      return result;
    }
  }


  public class NavisTriangle {
    public NavisVertex Vertex1 { get; set; }
    public NavisVertex Vertex2 { get; set; }
    public NavisVertex Vertex3 { get; set; }
    public NavisTriangle ( NavisVertex v1, NavisVertex v2, NavisVertex v3 ) {
      Vertex1 = v1;
      Vertex2 = v2;
      Vertex3 = v3;
    }
  }
  public class NavisVertex {
    public NavisVertex ( float x, float y, float z ) {
      X = x;
      Y = y;
      Z = z;
    }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
  }

  public class NavisMesh {
    public List<int> Indices { get; set; }
    public List<float> Vertices { get; set; }
    public List<NavisTriangle> Triangles { get; set; }
    public NavisMesh ( List<NavisTriangle> Triangles ) {
      this.Triangles = new List<NavisTriangle>();
      this.Triangles = Triangles;

      //Add indices and vertices
      Indices = new List<int>();
      Vertices = new List<float>();
      int index = 0;

      //create indices and vertices lists
      foreach ( NavisTriangle triangle in Triangles ) {
        Indices.Add( index++ );
        Indices.Add( index++ );
        Indices.Add( index++ );
        Vertices.Add( triangle.Vertex1.X );
        Vertices.Add( triangle.Vertex1.Y );
        Vertices.Add( triangle.Vertex1.Z );
        Vertices.Add( triangle.Vertex2.X );
        Vertices.Add( triangle.Vertex2.Y );
        Vertices.Add( triangle.Vertex2.Z );
        Vertices.Add( triangle.Vertex3.X );
        Vertices.Add( triangle.Vertex3.Y );
        Vertices.Add( triangle.Vertex3.Z );
      }
    }
  }
}



