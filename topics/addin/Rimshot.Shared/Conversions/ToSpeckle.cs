using Autodesk.Navisworks.Api;
using Objects.Geometry;
using Rimshot.Geometry;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using Props = Rimshot.Conversions.Properties;

namespace Rimshot.Conversions {
  internal class ToSpeckle {

    static public Box BoxToSpeckle ( BoundingBox3D boundingBox3D ) {
      Box boundingBox = new Box();

      double scale = 0.001; // TODO: Proper units support.

      Point3D min = boundingBox3D.Min;
      Point3D max = boundingBox3D.Max;

      boundingBox.xSize = new Objects.Primitive.Interval( min.X * scale, max.X * scale );
      boundingBox.ySize = new Objects.Primitive.Interval( min.Y * scale, max.Y * scale );
      boundingBox.zSize = new Objects.Primitive.Interval( min.Z * scale, max.Z * scale );

      return boundingBox;
    }

    public static Base BuildBaseObjectTree ( ModelItem element,
                                            NavisGeometry geometry,
                                            List<Tuple<NamedConstant, NamedConstant>> QuickPropertyDefinitions,
                                            ref Base QuickProperties ) {

      Base elementBase = new Base();
      Base propertiesBase = new Base();

      // GUI visible properties varies by a Global Options setting.
      PropertyCategoryCollection propertyCategories = element.GetUserFilteredPropertyCategories();

      // If Add QuickProperties is set.
      foreach ( Tuple<NamedConstant, NamedConstant> quickPropertyDef in QuickPropertyDefinitions ) {
        PropertyCategory foundCategory = propertyCategories.FindCategoryByCombinedName( quickPropertyDef.Item1 );

        if ( foundCategory != null ) {
          DataProperty foundProperty = propertyCategories.FindPropertyByCombinedName( quickPropertyDef.Item1, quickPropertyDef.Item2 );

          Base quickPropertiesCategoryBase;

          string foundCategoryName = Props.SanitizePropertyName( foundCategory.DisplayName );

          quickPropertiesCategoryBase = QuickProperties[ foundCategoryName ] == null ? new Base() : ( Base )QuickProperties[ foundCategoryName ];

          if ( foundProperty != null ) {
            Props.BuildPropertyCategory( foundCategory, foundProperty, ref quickPropertiesCategoryBase );
          }

          QuickProperties[ foundCategoryName ] = quickPropertiesCategoryBase;
        }
      }

      foreach ( PropertyCategory propertyCategory in propertyCategories ) {
        DataPropertyCollection properties = propertyCategory.Properties;
        Base propertyCategoryBase = new Base();

        properties.ToList().ForEach( property => Props.BuildPropertyCategory( propertyCategory, property, ref propertyCategoryBase ) );

        if ( propertyCategoryBase.GetDynamicMembers().Count() > 0 && propertyCategory.DisplayName != null ) {
          if ( propertiesBase != null ) {

            string propertyCategoryDisplayName = Props.SanitizePropertyName( propertyCategory.DisplayName );

            if ( propertyCategory.DisplayName == "Geometry" ) {
              continue;
            }

            if ( propertyCategory.DisplayName == "Item" ) {
              foreach ( string property in propertyCategoryBase.GetDynamicMembers() ) {
                elementBase[ property ] = propertyCategoryBase[ property ];
              }
            } else {
              propertiesBase[ propertyCategoryDisplayName ] = propertyCategoryBase;
            }
          }
        }
      }

      elementBase[ "QuickProperties" ] = QuickProperties;
      elementBase[ "Properties" ] = propertiesBase;

      if ( element == geometry.ModelItem ) {
        // Establish the node as the geometry node and copy through the existing conversion.
        foreach ( string baseProperty in geometry.Geometry.GetDynamicMembers() ) {
          elementBase[ baseProperty ] = geometry.Geometry[ baseProperty ];
        }
      } else {
        // Process the descendants.
        List<Base> children = new List<Base>();
        if ( element.Children.Count() > 0 ) {
          for ( int d = 0; d < element.Children.Count(); d++ ) {
            ModelItem child = element.Children.ElementAt( d );
            Base bbot = BuildBaseObjectTree( child, geometry, QuickPropertyDefinitions, ref QuickProperties );

            if ( ( bbot[ "displayValue" ] is null ) || bbot.GetDynamicMembers().Contains( "displayValue" ) == false ) {
              Console.WriteLine( child.DisplayName );
            } else {

              children.Add( bbot );

            }

          }
          elementBase[ "@Elements" ] = children;
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

      if ( element.InstanceGuid != null ) {
        if ( element.InstanceGuid.ToByteArray().Select( x => ( int )x ).Sum() > 0 ) {
          elementBase[ "InstanceGuid" ] = element.InstanceGuid;
          elementBase.applicationId = element.InstanceGuid.ToString();
        }
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
