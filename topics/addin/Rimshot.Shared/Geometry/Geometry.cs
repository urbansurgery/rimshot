using Autodesk.Navisworks.Api;
using Objects.Geometry;
using Rimshot.ArrayExtensions;
using Rimshot.Conversions;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;

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
    [SuppressMessage( "ReSharper", "IdentifierTypo" )]
    public void Triangle ( ComApi.InwSimpleVertex v1, ComApi.InwSimpleVertex v2, ComApi.InwSimpleVertex v3 ) {

      int indexPointer = this.Faces.Count;

      Array arrayV1 = ( Array )v1.coord;
      double v1X = ( float )arrayV1.GetValue( 1 );
      double v1Y = ( float )arrayV1.GetValue( 2 );
      double v1Z = ( float )arrayV1.GetValue( 3 );

      Array arrayV2 = ( Array )v2.coord;
      double v2X = ( float )arrayV2.GetValue( 1 );
      double v2Y = ( float )arrayV2.GetValue( 2 );
      double v2Z = ( float )arrayV2.GetValue( 3 );

      Array arrayV3 = ( Array )v3.coord;
      double v3X = ( float )arrayV3.GetValue( 1 );
      double v3Y = ( float )arrayV3.GetValue( 2 );
      double v3Z = ( float )arrayV3.GetValue( 3 );

      //Matrix transformation
      double t1 = this.Matrix[ 3 ] * v1X + this.Matrix[ 7 ] * v1Y + this.Matrix[ 11 ] * v1Z + this.Matrix[ 15 ];
      double v1Xprime = ( this.Matrix[ 0 ] * v1X + this.Matrix[ 4 ] * v1Y + this.Matrix[ 8 ] * v1Z + this.Matrix[ 12 ] ) / t1;
      double v1Yprime = ( this.Matrix[ 1 ] * v1X + this.Matrix[ 5 ] * v1Y + this.Matrix[ 9 ] * v1Z + this.Matrix[ 13 ] ) / t1;
      double v1Zprime = ( this.Matrix[ 2 ] * v1X + this.Matrix[ 6 ] * v1Y + this.Matrix[ 10 ] * v1Z + this.Matrix[ 14 ] ) / t1;

      double t2 = this.Matrix[ 3 ] * v2X + this.Matrix[ 7 ] * v2Y + this.Matrix[ 11 ] * v2Z + this.Matrix[ 15 ];
      double v2Xprime = ( this.Matrix[ 0 ] * v2X + this.Matrix[ 4 ] * v2Y + this.Matrix[ 8 ] * v2Z + this.Matrix[ 12 ] ) / t2;
      double v2Yprime = ( this.Matrix[ 1 ] * v2X + this.Matrix[ 5 ] * v2Y + this.Matrix[ 9 ] * v2Z + this.Matrix[ 13 ] ) / t2;
      double v2Zprime = ( this.Matrix[ 2 ] * v2X + this.Matrix[ 6 ] * v2Y + this.Matrix[ 10 ] * v2Z + this.Matrix[ 14 ] ) / t2;

      double t3 = this.Matrix[ 3 ] * v3X + this.Matrix[ 7 ] * v3Y + this.Matrix[ 11 ] * v3Z + this.Matrix[ 15 ];
      double v3Xprime = ( this.Matrix[ 0 ] * v3X + this.Matrix[ 4 ] * v3Y + this.Matrix[ 8 ] * v3Z + this.Matrix[ 12 ] ) / t3;
      double v3Yprime = ( this.Matrix[ 1 ] * v3X + this.Matrix[ 5 ] * v3Y + this.Matrix[ 9 ] * v3Z + this.Matrix[ 13 ] ) / t3;
      double v3Zprime = ( this.Matrix[ 2 ] * v3X + this.Matrix[ 6 ] * v3Y + this.Matrix[ 10 ] * v3Z + this.Matrix[ 14 ] ) / t3;


      this.Faces.Add( 3 ); //TRIANGLE FLAG

      // Triangle by 3 Vertices
      this.Coords.Add( v1Xprime );
      this.Coords.Add( v1Yprime );
      this.Coords.Add( v1Zprime );
      this.Faces.Add( indexPointer + 0 );

      this.Coords.Add( v2Xprime );
      this.Coords.Add( v2Yprime );
      this.Coords.Add( v2Zprime );
      this.Faces.Add( indexPointer + 1 );

      this.Coords.Add( v3Xprime );
      this.Coords.Add( v3Yprime );
      this.Coords.Add( v3Zprime );
      this.Faces.Add( indexPointer + 2 );

      this.Triangles.Add( new NavisDoubleTriangle( v1: new NavisDoubleVertex( v1Xprime, v1Yprime, v1Zprime ),
                                                   v2: new NavisDoubleVertex( v2Xprime, v2Yprime, v2Zprime ),
                                                   v3: new NavisDoubleVertex( v3Xprime, v3Yprime, v3Zprime ) ) );
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

      ModelItemCollection modelItemCollection = new ModelItemCollection {
        modelItem
      };

      this.ComSelection = ComBridge.ToInwOpSelection( modelItemCollection );
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
              int a1Value = ( int )a1.GetValue( i );
              int a2Value = ( int )a2.GetValue( i );

              if ( a1Value != a2Value ) {
                isSame = false;
                break;
              }
            }
          } else {
            isSame = false;
          }

          if ( isSame ) {
            ComApi.InwLTransform3f3 localToWorld = ( ComApi.InwLTransform3f3 )fragment.GetLocalToWorldMatrix();

            //create Global Coordinate System Matrix
            object matrix = localToWorld.Matrix;
            Array matrixArray = ( Array )matrix;
            double[] elements = ConvertArrayToDouble( matrixArray );
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

      foreach ( ComApi.InwOaPath3 path in this.ComSelection.Paths() ) {
        CallbackGeomListener callbackListener = new CallbackGeomListener();
        foreach ( ComApi.InwOaFragment3 fragment in path.Fragments() ) {
          ComApi.InwLTransform3f3 localToWorld = ( ComApi.InwLTransform3f3 )fragment.GetLocalToWorldMatrix();

          //create Global Coordinate System Matrix
          object matrix = localToWorld.Matrix;
          Array matrixArray = ( Array )matrix;
          double[] elements = ConvertArrayToDouble( matrixArray );
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

      double[] arrayDoubles = new double[ arr.GetLength( 0 ) ];
      for ( int ix = arr.GetLowerBound( 0 ); ix <= arr.GetUpperBound( 0 ); ++ix ) {
        arrayDoubles[ ix - arr.GetLowerBound( 0 ) ] = ( double )arr.GetValue( ix );
      }

      return arrayDoubles;
    }
  }

  public class NavisTriangle {
    public NavisVertex Vertex1 { get; set; }
    public NavisVertex Vertex2 { get; set; }
    public NavisVertex Vertex3 { get; set; }
    public NavisTriangle ( NavisVertex v1, NavisVertex v2, NavisVertex v3 ) {
      this.Vertex1 = v1;
      this.Vertex2 = v2;
      this.Vertex3 = v3;
    }
  }
  public class NavisDoubleTriangle {
    public NavisDoubleVertex Vertex1 { get; set; }
    public NavisDoubleVertex Vertex2 { get; set; }
    public NavisDoubleVertex Vertex3 { get; set; }
    public NavisDoubleTriangle ( NavisDoubleVertex v1, NavisDoubleVertex v2, NavisDoubleVertex v3 ) {
      this.Vertex1 = v1;
      this.Vertex2 = v2;
      this.Vertex3 = v3;
    }
  }
  public class NavisVertex {
    public NavisVertex ( float x, float y, float z ) {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
  }
  public class NavisDoubleVertex {
    public NavisDoubleVertex ( double x, double y, double z ) {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
  }

  public class NavisMesh {
    public List<int> Indices { get; set; }
    public List<float> Vertices { get; set; }
    public List<NavisTriangle> Triangles { get; set; }
    public NavisMesh ( List<NavisTriangle> triangles ) {
      this.Triangles = new List<NavisTriangle>();
      this.Triangles = triangles;

      //Add indices and vertices
      this.Indices = new List<int>();
      this.Vertices = new List<float>();
      int index = 0;

      //create indices and vertices lists
      foreach ( NavisTriangle triangle in triangles ) {
        this.Indices.Add( index++ );
        this.Indices.Add( index++ );
        this.Indices.Add( index++ );
        this.Vertices.Add( triangle.Vertex1.X );
        this.Vertices.Add( triangle.Vertex1.Y );
        this.Vertices.Add( triangle.Vertex1.Z );
        this.Vertices.Add( triangle.Vertex2.X );
        this.Vertices.Add( triangle.Vertex2.Y );
        this.Vertices.Add( triangle.Vertex2.Z );
        this.Vertices.Add( triangle.Vertex3.X );
        this.Vertices.Add( triangle.Vertex3.Y );
        this.Vertices.Add( triangle.Vertex3.Z );
      }
    }
  }

  public class Geometry {

    public Vector3D TransformVector3D { get; set; }
    public Vector SettingOutPoint { get; set; }
    public Vector TransformVector { get; set; }

    public Dictionary<int[], Stack<ComApi.InwOaFragment3>> pathDictionary = new Dictionary<int[], Stack<ComApi.InwOaFragment3>>();

    public readonly ModelItemCollection selectedItems = new ModelItemCollection();
    public readonly ModelItemCollection selectedItemsAndDescendants = new ModelItemCollection();

    /// <summary>
    /// Parse all descendant nodes of the element that are visible, selected and geometry nodes.
    /// </summary>
    public List<ModelItem> CollectGeometryNodes ( ModelItem element ) {
      ModelItemEnumerableCollection descendants = element.DescendantsAndSelf;

      // if the descendant node isn't hidden, has geometry and is part of the original selection set.
      List<ModelItem> modelItems = new List<ModelItem>();
      int dCount = descendants.Count();

      foreach ( ModelItem item in descendants ) {
        bool hasGeometry = item.HasGeometry;
        bool isVisible = !item.IsHidden;
        bool isSelected = this.selectedItemsAndDescendants.IsSelected( item );

        if ( hasGeometry && isVisible && isSelected ) {
          modelItems.Add( item );
        }
        if ( modelItems.Count > 0 && modelItems.Count % 10 == 0 ) {
          Logging.ConsoleLog( $"Collecting Geometry Nodes {modelItems.Count} of possible {dCount}", ConsoleColor.DarkYellow );
        }
      }

      return modelItems;
    }

    public void AddFragments ( NavisGeometry geometry ) {

      geometry.ModelFragments = new Stack<ComApi.InwOaFragment3>();

      foreach ( ComApi.InwOaPath path in geometry.ComSelection.Paths() ) {
        foreach ( ComApi.InwOaFragment3 frag in path.Fragments() ) {

          int[] a1 = ( ( Array )frag.path.ArrayData ).ToArray<int>();
          int[] a2 = ( ( Array )path.ArrayData ).ToArray<int>();
          bool isSame = !( a1.Length != a2.Length || !a1.SequenceEqual( a2 ) );

          if ( isSame ) {
            geometry.ModelFragments.Push( frag );
          }
        }
      }
    }

    public void GetSortedFragments ( ModelItemCollection modelItems ) {
      ComApi.InwOpSelection pseudoSelection = ComBridge.ToInwOpSelection( modelItems );

      foreach ( ComApi.InwOaPath3 objectPath in pseudoSelection.Paths() ) {
        foreach ( ComApi.InwOaFragment3 fragment in objectPath.Fragments() ) {
          int[] pathStore = ( ( Array )fragment.path.ArrayData ).ToArray<int>();

          if ( !this.pathDictionary.TryGetValue( pathStore, out Stack<ComApi.InwOaFragment3> fragments ) ) {
            fragments = new Stack<ComApi.InwOaFragment3>();
            this.pathDictionary[ pathStore ] = fragments;
          }
          fragments.Push( fragment );
        }
      }
    }

    public void TranslateGeometryElement ( NavisGeometry geometryElement ) {
      Base elementBase = new Base();

      if ( geometryElement.ModelItem.HasGeometry && !geometryElement.ModelItem.Children.Any() ) {
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
        List<NavisDoubleTriangle> triangles = callback.Triangles;
        // TODO: Additional Geometry Types
        //List<NavisDoubleLine> Lines = callback.Lines;
        //List<NavisDoublePoint> Points = callback.Points;

        List<double> vertices = new List<double>();
        List<int> faces = new List<int>();

        Vector3D move = this.TransformVector3D;

        int triangleCount = triangles.Count;
        if ( triangleCount <= 0 ) {
          continue;
        }

        for ( int t = 0; t < triangleCount; t += 1 ) {
          const double scale = 0.001; // TODO: This will need to relate to the ActiveDocument reality and the target units. Probably meters.

          // Apply the bounding box move.
          // The native API methods for overriding transforms are not thread safe to call from the CEF instance
          vertices.AddRange( new List<double>() {
            ( triangles[ t ].Vertex1.X + move.X ) * scale,
            ( triangles[ t ].Vertex1.Y + move.Y ) * scale,
            ( triangles[ t ].Vertex1.Z + move.Z ) * scale
          } );
          vertices.AddRange( new List<double>() {
            ( triangles[ t ].Vertex2.X + move.X ) * scale,
            ( triangles[ t ].Vertex2.Y + move.Y ) * scale,
            ( triangles[ t ].Vertex2.Z + move.Z ) * scale
          } );
          vertices.AddRange( new List<double>() {
            ( triangles[ t ].Vertex3.X + move.X ) * scale,
            ( triangles[ t ].Vertex3.Y + move.Y ) * scale,
            ( triangles[ t ].Vertex3.Z + move.Z ) * scale
          } );

          // TODO: Move this back to Geometry.cs
          faces.Add( 0 );
          faces.Add( t * 3 );
          faces.Add( t * 3 + 1 );
          faces.Add( t * 3 + 2 );
        }
        Mesh baseMesh = new Mesh( vertices, faces ) { [ "renderMaterial" ] = Materials.TranslateMaterial( navisGeometry.ModelItem ) };
        baseGeometries.Add( baseMesh );
      }
      return baseGeometries; // TODO: Check if this actually has geometries before adding to DisplayValue
    }
  }
}


