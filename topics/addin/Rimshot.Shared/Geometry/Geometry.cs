using Autodesk.Navisworks.Api;
using Objects.Geometry;
using Rimshot.ArrayExtensions;
using Rimshot.Conversions;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge; //

namespace Rimshot.Geometry {
  public class CallbackGeomListener : ComApi.InwSimplePrimitivesCB {
    public List<double> Coords { get; set; }

    public List<int> Faces { get; set; }
    public List<NavisDoubleTriangle> Triangles { get; set; }
    public double[] Matrix { get; set; }
    public CallbackGeomListener () {
      this.Coords = new List<double>();
      this.Faces = new List<int>();
      this.Triangles = new List<NavisDoubleTriangle>();
    }
    public void Line ( ComApi.InwSimpleVertex v1, ComApi.InwSimpleVertex v2 ) { }
    public void Point ( ComApi.InwSimpleVertex v1 ) { }
    public void SnapPoint ( ComApi.InwSimpleVertex v1 ) { }
    public void Triangle ( ComApi.InwSimpleVertex v1, ComApi.InwSimpleVertex v2, ComApi.InwSimpleVertex v3 ) {

      int indexPointer = this.Faces.Count;

      Array array_v1 = ( Array )v1.coord;
      double v1X = ( double )( float )array_v1.GetValue( 1 );
      double v1Y = ( double )( float )array_v1.GetValue( 2 );
      double v1Z = ( double )( float )array_v1.GetValue( 3 );

      Array array_v2 = ( Array )v2.coord;
      double v2X = ( double )( float )array_v2.GetValue( 1 );
      double v2Y = ( double )( float )array_v2.GetValue( 2 );
      double v2Z = ( double )( float )array_v2.GetValue( 3 );

      Array array_v3 = ( Array )v3.coord;
      double v3X = ( double )( float )array_v3.GetValue( 1 );
      double v3Y = ( double )( float )array_v3.GetValue( 2 );
      double v3Z = ( double )( float )array_v3.GetValue( 3 );

      //Matrix transformation
      double T1 = Matrix[ 3 ] * v1X + Matrix[ 7 ] * v1Y + Matrix[ 11 ] * v1Z + Matrix[ 15 ];
      double v1X_ = ( Matrix[ 0 ] * v1X + Matrix[ 4 ] * v1Y + Matrix[ 8 ] * v1Z + Matrix[ 12 ] ) / T1;
      double v1Y_ = ( Matrix[ 1 ] * v1X + Matrix[ 5 ] * v1Y + Matrix[ 9 ] * v1Z + Matrix[ 13 ] ) / T1;
      double v1Z_ = ( Matrix[ 2 ] * v1X + Matrix[ 6 ] * v1Y + Matrix[ 10 ] * v1Z + Matrix[ 14 ] ) / T1;

      double T2 = Matrix[ 3 ] * v2X + Matrix[ 7 ] * v2Y + Matrix[ 11 ] * v2Z + Matrix[ 15 ];
      double v2X_ = ( Matrix[ 0 ] * v2X + Matrix[ 4 ] * v2Y + Matrix[ 8 ] * v2Z + Matrix[ 12 ] ) / T2;
      double v2Y_ = ( Matrix[ 1 ] * v2X + Matrix[ 5 ] * v2Y + Matrix[ 9 ] * v2Z + Matrix[ 13 ] ) / T2;
      double v2Z_ = ( Matrix[ 2 ] * v2X + Matrix[ 6 ] * v2Y + Matrix[ 10 ] * v2Z + Matrix[ 14 ] ) / T2;

      double T3 = Matrix[ 3 ] * v3X + Matrix[ 7 ] * v3Y + Matrix[ 11 ] * v3Z + Matrix[ 15 ];
      double v3X_ = ( Matrix[ 0 ] * v3X + Matrix[ 4 ] * v3Y + Matrix[ 8 ] * v3Z + Matrix[ 12 ] ) / T3;
      double v3Y_ = ( Matrix[ 1 ] * v3X + Matrix[ 5 ] * v3Y + Matrix[ 9 ] * v3Z + Matrix[ 13 ] ) / T3;
      double v3Z_ = ( Matrix[ 2 ] * v3X + Matrix[ 6 ] * v3Y + Matrix[ 10 ] * v3Z + Matrix[ 14 ] ) / T3;


      this.Faces.Add( 3 ); //TRIANGLE FLAG

      // Triangle by 3 Vertices
      this.Coords.Add( v1X_ );
      this.Coords.Add( v1Y_ );
      this.Coords.Add( v1Z_ );
      this.Faces.Add( indexPointer + 0 );

      this.Coords.Add( v2X_ );
      this.Coords.Add( v2Y_ );
      this.Coords.Add( v2Z_ );
      this.Faces.Add( indexPointer + 1 );

      this.Coords.Add( v3X_ );
      this.Coords.Add( v3Y_ );
      this.Coords.Add( v3Z_ );
      this.Faces.Add( indexPointer + 2 );

      // Append all translated Triangle faces
      //this.Triangles.Add(
      //  new NavisTriangle(
      //    new NavisVertex( ( float )v1X_, ( float )v1Y_, ( float )v1Z_ ),
      //    new NavisVertex( ( float )v2X_, ( float )v2Y_, ( float )v2Z_ ),
      //    new NavisVertex( ( float )v3X_, ( float )v3Y_, ( float )v3Z_ )
      //  )
      //);
      this.Triangles.Add(
   new NavisDoubleTriangle(
     new NavisDoubleVertex( v1X_, v1Y_, v1Z_ ),
     new NavisDoubleVertex( v2X_, v2Y_, v2Z_ ),
     new NavisDoubleVertex( v3X_, v3Y_, v3Z_ )
   )
 );
    }
  }

  public class NavisGeometry {
    public ComApi.InwOpSelection ComSelection { get; set; }
    public ModelItem ModelItem { get; set; }
    public Stack<ComApi.InwOaFragment3> ModelFragments { get; set; }
    public Base Geometry { get; internal set; }
    public Base Base { get; internal set; }

    public NavisGeometry ( ModelItem modelItem ) {

      this.ModelItem = modelItem;

      // Add conversion geometry to oModelColl Property
      ModelItemCollection modelitemCollection = new ModelItemCollection {
        modelItem
      };

      //convert to COM selection
      this.ComSelection = ComBridge.ToInwOpSelection( modelitemCollection );
    }

    public List<CallbackGeomListener> GetUniqueFragments () {

      List<CallbackGeomListener> callbackListeners = new List<CallbackGeomListener>();

      foreach ( ComApi.InwOaPath path in this.ComSelection.Paths() ) {
        CallbackGeomListener callbackListener = new CallbackGeomListener();
        foreach ( ComApi.InwOaFragment3 fragment in this.ModelFragments ) {
          Array a1 = ( ( Array )fragment.path.ArrayData ).ToArray<int>();
          Array a2 = ( ( Array )path.ArrayData ).ToArray<int>();

          // This is now lots of duplicate code!!
          bool isSame = true;

          if ( a1.Length == a2.Length ) {

            for ( int i = 0; i < a1.Length; i += 1 ) {
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

          if ( isSame ) {
            ComApi.InwLTransform3f3 localToWorld = ( ComApi.InwLTransform3f3 )( object )fragment.GetLocalToWorldMatrix();

            //create Global Cordinate System Matrix
            object matrix = localToWorld.Matrix;
            Array matrix_array = ( Array )matrix;
            double[] elements = ConvertArrayToDouble( matrix_array );
            double[] elementsValue = new double[ elements.Length ];
            for ( int i = 0; i < elements.Length; i++ ) {
              elementsValue[ i ] = elements[ i ];
            }

            callbackListener.Matrix = elementsValue;
            fragment.GenerateSimplePrimitives( ComApi.nwEVertexProperty.eNORMAL, callbackListener );
          }
        }
        callbackListeners.Add( callbackListener );
      }
      return callbackListeners;
    }

    public List<CallbackGeomListener> GetFragments () {
      List<CallbackGeomListener> callbackListeners = new List<CallbackGeomListener>();
      // create the callback object

      foreach ( ComApi.InwOaPath3 path in ComSelection.Paths() ) {
        CallbackGeomListener callbackListener = new CallbackGeomListener();
        foreach ( ComApi.InwOaFragment3 fragment in path.Fragments() ) {
          ComApi.InwLTransform3f3 localToWorld = ( ComApi.InwLTransform3f3 )( object )fragment.GetLocalToWorldMatrix();

          //create Global Cordinate System Matrix
          object matrix = localToWorld.Matrix;
          Array matrix_array = ( Array )matrix;
          double[] elements = ConvertArrayToDouble( matrix_array );
          double[] elementsValue = new double[ elements.Length ];
          for ( int i = 0; i < elements.Length; i++ ) {
            elementsValue[ i ] = elements[ i ];
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

    public static double[] ConvertArrayToDouble ( Array arr ) {
      if ( arr.Rank != 1 ) {
        throw new ArgumentException();
      }

      double[] retval = new double[ arr.GetLength( 0 ) ];
      for ( int ix = arr.GetLowerBound( 0 ); ix <= arr.GetUpperBound( 0 ); ++ix ) {
        retval[ ix - arr.GetLowerBound( 0 ) ] = ( double )arr.GetValue( ix );
      }

      return retval;
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
  public class NavisDoubleTriangle {
    public NavisDoubleVertex Vertex1 { get; set; }
    public NavisDoubleVertex Vertex2 { get; set; }
    public NavisDoubleVertex Vertex3 { get; set; }
    public NavisDoubleTriangle ( NavisDoubleVertex v1, NavisDoubleVertex v2, NavisDoubleVertex v3 ) {
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
  public class NavisDoubleVertex {
    public NavisDoubleVertex ( double x, double y, double z ) {
      X = x;
      Y = y;
      Z = z;
    }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
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

  public class Geometry {

    public Vector3D TransformVector3D { get; set; }
    public Vector SettingOutPoint { get; set; }
    public Vector TransformVector { get; set; }

    public Dictionary<int[], Stack<ComApi.InwOaFragment3>> pathDictionary = new Dictionary<int[], Stack<ComApi.InwOaFragment3>>();
    public Dictionary<NavisGeometry, Stack<ComApi.InwOaFragment3>> modelGeometryDictionary =
      new Dictionary<NavisGeometry, Stack<ComApi.InwOaFragment3>>();

    public HashSet<NavisGeometry> GeometrySet = new HashSet<NavisGeometry>();

    public readonly ModelItemCollection selectedItems = new ModelItemCollection();
    public readonly ModelItemCollection selectedItemsAndDescendants = new ModelItemCollection();

    public Geometry () { }


    /// <summary>
    /// Parse all descendant nodes of the element that are visible, selected and geometry nodes.
    /// </summary>
    public List<ModelItem> CollectGeometryNodes ( ModelItem element ) {
      ModelItemEnumerableCollection descendants = element.DescendantsAndSelf;

      // if the descendant node isn't hidden, has geometry and is part of the original selection set.

      List<ModelItem> items = new List<ModelItem>();
      int dCount = descendants.Count();

      foreach ( ModelItem item in descendants ) {
        bool hasGeometry = item.HasGeometry;
        bool isVisible = !item.IsHidden;
        bool isSelected = selectedItemsAndDescendants.IsSelected( item );

        if ( hasGeometry && isVisible && isSelected ) {
          items.Add( item );
        }

        Logging.ConsoleLog( $"Collecting Geometry Nodes {items.Count} of possible {dCount}", ConsoleColor.DarkYellow );
      }

      return items;
    }

    public void AddFragments ( NavisGeometry geometry ) {

      geometry.ModelFragments = new Stack<ComApi.InwOaFragment3>();

      foreach ( ComApi.InwOaPath path in geometry.ComSelection.Paths() ) {
        foreach ( ComApi.InwOaFragment3 frag in path.Fragments() ) {

          int[] a1 = ( ( Array )frag.path.ArrayData ).ToArray<int>();
          int[] a2 = ( ( Array )path.ArrayData ).ToArray<int>();
          bool isSame = true;

          if ( a1.Length != a2.Length || !Enumerable.SequenceEqual( a1, a2 ) ) {
            isSame = false;
          }

          if ( isSame ) {
            geometry.ModelFragments.Push( frag );
          }
        }
      }
    }

    public void GetSortedFragments ( ModelItemCollection modelItems ) {
      ComApi.InwOpSelection oSel = ComBridge.ToInwOpSelection( modelItems );
      // To be most efficient you need to lookup an efficient EqualityComparer
      // for the int[] key
      foreach ( ComApi.InwOaPath3 path in oSel.Paths() ) {
        // this yields ONLY unique fragments
        // ordered by geometry they belong to
        foreach ( ComApi.InwOaFragment3 frag in path.Fragments() ) {
          int[] pathArr = ( ( Array )frag.path.ArrayData ).ToArray<int>();
          if ( !this.pathDictionary.TryGetValue( pathArr, out Stack<ComApi.InwOaFragment3> frags ) ) {
            frags = new Stack<ComApi.InwOaFragment3>();
            this.pathDictionary[ pathArr ] = frags;
          }
          frags.Push( frag );
        }
      }
    }

    public void TranslateGeometryElement ( NavisGeometry geometryElement ) {
      Base elementBase = new Base();

      if ( geometryElement.ModelItem.HasGeometry && geometryElement.ModelItem.Children.Count() == 0 ) {
        List<Base> speckleGeometries = TranslateFragmentGeometry( geometryElement );
        if ( speckleGeometries.Count > 0 ) {
          elementBase[ "displayValue" ] = speckleGeometries;
          elementBase[ "units" ] = "m";
          elementBase[ "bbox" ] = ToSpeckle.BoxToSpeckle( geometryElement.ModelItem.BoundingBox() );
        }
      }
      geometryElement.Geometry = elementBase;
    }

    public List<Base> TranslateFragmentGeometry ( NavisGeometry navisGeometry ) {
      List<CallbackGeomListener> callbackListeners = navisGeometry.GetUniqueFragments();

      List<Base> baseGeometries = new List<Base>();

      foreach ( CallbackGeomListener callback in callbackListeners ) {
        List<NavisDoubleTriangle> Triangles = callback.Triangles;
        // TODO: Additional Geometry Types
        //List<NavisDoubleLine> Lines = callback.Lines;
        //List<NavisDoublePoint> Points = callback.Points;

        List<double> vertices = new List<double>();
        List<int> faces = new List<int>();

        Vector3D move = TransformVector3D;

        int triangleCount = Triangles.Count;
        if ( triangleCount > 0 ) {

          for ( int t = 0; t < triangleCount; t += 1 ) {
            double scale = ( double )0.001; // TODO: This will need to relate to the ActiveDocument reality and the target units. Probably metres.

            // Apply the bounding box move.
            // The native API methods for overriding transforms are not thread safe to call from the CEF instance
            vertices.AddRange( new List<double>() {
              ( Triangles[ t ].Vertex1.X + move.X ) * scale,
              ( Triangles[ t ].Vertex1.Y + move.Y ) * scale,
              ( Triangles[ t ].Vertex1.Z + move.Z ) * scale
            } );
            vertices.AddRange( new List<double>() {
              ( Triangles[ t ].Vertex2.X + move.X ) * scale,
              ( Triangles[ t ].Vertex2.Y + move.Y ) * scale,
              ( Triangles[ t ].Vertex2.Z + move.Z ) * scale
            } );
            vertices.AddRange( new List<double>() {
              ( Triangles[ t ].Vertex3.X + move.X ) * scale,
              ( Triangles[ t ].Vertex3.Y + move.Y ) * scale,
              ( Triangles[ t ].Vertex3.Z + move.Z ) * scale
            } );

            // TODO: Move this back to Geometry.cs
            faces.Add( 0 );
            faces.Add( t * 3 );
            faces.Add( t * 3 + 1 );
            faces.Add( t * 3 + 2 );
          }
          Mesh baseMesh = new Mesh( vertices, faces );
          baseMesh[ "renderMaterial" ] = Materials.TranslateMaterial( navisGeometry.ModelItem );
          baseGeometries.Add( baseMesh );
        }
      }
      return baseGeometries; // TODO: Check if this actually has geometries before adding to DisplayValue
    }
  }
}


