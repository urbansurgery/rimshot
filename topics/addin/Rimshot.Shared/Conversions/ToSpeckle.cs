using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.ComApi;
using Autodesk.Navisworks.Api.Interop.ComApi;
using Objects.Geometry;
using Rimshot.Geometry;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using Props = Rimshot.Conversions.Properties;

namespace Rimshot {
  class PathTree {

    private InwOaPath Path { get; set; }
    private List<PathTree> Children { get; set; }
    PathTree () {

    }


  }
}

namespace Rimshot.Conversions {




  internal class ToSpeckle {





    public static Box BoxToSpeckle ( BoundingBox3D boundingBox3D ) {
      Box boundingBox = new Box();

      const double scale = 0.001; // TODO: Proper units support.

      Point3D min = boundingBox3D.Min;
      Point3D max = boundingBox3D.Max;

      boundingBox.xSize = new Objects.Primitive.Interval( min.X * scale, max.X * scale );
      boundingBox.ySize = new Objects.Primitive.Interval( min.Y * scale, max.Y * scale );
      boundingBox.zSize = new Objects.Primitive.Interval( min.Z * scale, max.Z * scale );

      return boundingBox;
    }



    public static Dictionary<NamedConstant, InwGUIAttribute2> GetPropertyCategories ( InwOaPath itemPath ) {
      //COM state object
      InwOpState10 state = ComApiBridge.State;

      // Get Items PropertyCategoryCollection object
      InwGUIPropertyNode2 propertyNode = ( InwGUIPropertyNode2 )state.GetGUIPropertyNode( itemPath, true );

      // Get PropertyCategoryCollection data
      var allPropertyCategories = propertyNode.GUIAttributes().Cast<InwGUIAttribute2>();

      // loop property category

      Dictionary<NamedConstant, InwGUIAttribute2> namedPropertyCategories = new Dictionary<NamedConstant, InwGUIAttribute2>();

      foreach ( InwGUIAttribute2 propertyCategory in allPropertyCategories ) {
        try {
          NamedConstant categoryName = new NamedConstant( propertyCategory.ClassName, propertyCategory.ClassUserName );

          if ( namedPropertyCategories.ContainsKey( categoryName ) ) {
            continue;
          }
          namedPropertyCategories.Add( categoryName, propertyCategory );
        } catch ( Exception e ) {
          Console.WriteLine( $"COM Property Categories: {e.Message}" );
        }
      }

      GC.KeepAlive( propertyNode );
      GC.KeepAlive( namedPropertyCategories );

      return namedPropertyCategories;
    }

    public static Dictionary<NamedConstant, InwGUIAttribute2> GetPropertyCategories ( ModelItem item ) => GetPropertyCategories( ComApiBridge.ToInwOaPath( item ) );


    public static Dictionary<NamedConstant, InwOaProperty> GetProperties ( InwOaPropertyColl categoryProperties ) {

      Dictionary<NamedConstant, InwOaProperty> namedProperties = new Dictionary<NamedConstant, InwOaProperty>();

      foreach ( InwOaProperty property in categoryProperties ) {
        NamedConstant propertyName = new NamedConstant( property.name, property.UserName );
        if ( namedProperties.ContainsKey( propertyName ) ) {
          continue;
        }
        namedProperties.Add( propertyName, property );
      }

      return namedProperties;
    }

    public static Base BuildBaseObjectTree ( ModelItem element,
                                            NavisGeometry geometry,
                                            List<Tuple<NamedConstant, NamedConstant>> quickPropertyDefinitions,
                                            ref Base quickProperties ) {

      Base elementBase = new Base();
      Base propertiesBase = new Base();

      Dictionary<NamedConstant, InwGUIAttribute2> propertyCategories = GetPropertyCategories( element );

      foreach ( Tuple<NamedConstant, NamedConstant> quickPropertyDef in quickPropertyDefinitions ) {
        KeyValuePair<NamedConstant, InwGUIAttribute2> foundCategory = propertyCategories.FirstOrDefault( p => p.Key.Equals( quickPropertyDef.Item1 ) );

        if ( foundCategory.Key != null ) {
          Dictionary<NamedConstant, InwOaProperty> categoryProperties;
          try {
            if ( foundCategory.Value != null ) {
              InwOaPropertyColl foundCategoryProperties = foundCategory.Value.Properties();
              categoryProperties = GetProperties( foundCategoryProperties );
            } else {
              categoryProperties = null;
            }
          } catch ( Exception e ) {
            Console.WriteLine( e.Message );
            categoryProperties = null;
          }

          if ( categoryProperties != null && categoryProperties.Keys.Count > 0 ) {
            KeyValuePair<NamedConstant, InwOaProperty> foundProperty = categoryProperties.FirstOrDefault( p => p.Value.Equals( quickPropertyDef.Item2 ) );

            if ( foundProperty.Key != null ) {
              string foundCategoryName = Props.SanitizePropertyName( foundCategory.Key.DisplayName );
              Base qb = ( Base )quickProperties[ foundCategoryName ];
              bool v = quickProperties[ foundCategoryName ] == null;
              Base quickPropertiesCategoryBase = v ? new Base() : qb;
              Props.BuildPropertyCategory( foundCategory.Value, foundProperty.Value, ref quickPropertiesCategoryBase );
              quickProperties[ foundCategoryName ] = quickPropertiesCategoryBase;
            }
          }
        }
      }

      GC.KeepAlive( propertyCategories );
      GC.KeepAlive( quickPropertyDefinitions );

      //foreach ( InwGUIAttribute2 propertyCategory in propertyCategories.Values ) {
      //  List<InwOaProperty> properties;

      //  properties = new List<InwOaProperty>();

      //  foreach ( InwOaProperty property in propertyCategory.Properties() ) {
      //    properties.Add( property );
      //  }

      //  if ( properties.Count > 0 ) {

      //    Base propertyCategoryBase = new Base();
      //    properties.ForEach( property => Props.BuildPropertyCategory( propertyCategory, property, ref propertyCategoryBase ) );

      //    if ( propertyCategoryBase.GetDynamicMembers().Count() > 0 && propertyCategory.ClassUserName != null ) {
      //      if ( propertiesBase != null ) {

      //        string propertyCategoryDisplayName = Props.SanitizePropertyName( propertyCategory.ClassUserName );

      //        if ( propertyCategory.ClassUserName == "Geometry" ) {
      //          continue;
      //        }

      //        if ( propertyCategory.ClassUserName == "Item" ) {
      //          foreach ( string property in propertyCategoryBase.GetDynamicMembers() ) {
      //            elementBase[ property ] = propertyCategoryBase[ property ];
      //          }
      //        } else {
      //          propertiesBase[ propertyCategoryDisplayName ] = propertyCategoryBase;
      //        }
      //      }
      //    }
      //  }
      //}

      elementBase[ "Properties" ] = propertiesBase;
      elementBase[ "QuickProperties" ] = quickProperties;

      if ( element == geometry.ModelItem ) {
        // Establish the node as the geometry node and copy through the existing conversion.
        foreach ( string baseProperty in geometry.Geometry.GetDynamicMembers() ) {
          elementBase[ baseProperty ] = geometry.Geometry[ baseProperty ];
        }
      } else {
        // Process the descendants.
        List<Base> children = new List<Base>();
        if ( element.Children.Any() ) {
          for ( int d = 0; d < element.Children.Count(); d++ ) {
            ModelItem child = element.Children.ElementAt( d );
            Base buildBaseObjectTree = BuildBaseObjectTree( child, geometry, quickPropertyDefinitions, ref quickProperties );

            if ( buildBaseObjectTree != null ) {
              if ( child.Geometry == null || child == geometry.ModelItem ) {
                children.Add( buildBaseObjectTree );
              }
            }
          }

          if ( children.Count > 0 ) {
            elementBase[ "@Elements" ] = children;
          }

        }
      }

      if ( element.IsCollection ) {
        elementBase[ "NodeType" ] = "Collection";
      }

      if ( element.IsComposite ) {
        elementBase[ "NodeType" ] = "Composite Object";
      }

      if ( element.IsInsert ) {
        elementBase[ "NodeType" ] = "Geometry Insert";
      }

      if ( element.IsLayer ) {
        elementBase[ "NodeType" ] = "Layer";
      }

      if ( element.Model != null ) {
        elementBase[ "Source" ] = element.Model.SourceFileName;
      }

      if ( element.Model != null ) {
        elementBase[ "Filename" ] = element.Model.FileName;
      }

      if ( element.Model != null ) {
        elementBase[ "Source Guid" ] = element.Model.SourceGuid;
      }

      if ( element.Model != null ) {
        elementBase[ "Creator" ] = element.Model.Creator;
      }

      if ( element.InstanceGuid.ToByteArray().Select( x => ( int )x ).Sum() > 0 ) {
        elementBase[ "InstanceGuid" ] = element.InstanceGuid;
        elementBase.applicationId = element.InstanceGuid.ToString();
      }

      if ( element.ClassDisplayName != null ) {
        elementBase[ "ClassDisplayName" ] = element.ClassDisplayName;
      }

      if ( element.ClassName != null ) {
        elementBase[ "ClassName" ] = element.ClassName;
      }

      if ( element.DisplayName != null ) {
        elementBase[ "DisplayName" ] = element.DisplayName;
      }

      return elementBase;
    }
  }
}
