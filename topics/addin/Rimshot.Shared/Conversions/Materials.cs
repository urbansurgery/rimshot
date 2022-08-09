using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Interop.ComApi;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Security;
using Color = System.Drawing.Color;

using ComApiBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;

namespace Rimshot.Conversions {
  class Materials {

    [HandleProcessCorruptedStateExceptions, SecurityCritical]
    static public Objects.Other.RenderMaterial TranslateMaterial ( ModelItem geom ) {

      var Settings = new { Mode = "original" };

      Color renderColor;

      switch ( Settings.Mode ) {
        case "original":
          renderColor = Colors.NavisColorToColor( geom.Geometry.OriginalColor );
          break;
        case "active":
          renderColor = Colors.NavisColorToColor( geom.Geometry.ActiveColor );
          break;
        case "permanent":
          renderColor = Colors.NavisColorToColor( geom.Geometry.PermanentColor );
          break;
        default:
          renderColor = new Color();
          break;
      }

      string materialName = $"NavisMaterial_{Math.Abs( renderColor.ToArgb() )}";

      Color black = Color.FromArgb( Convert.ToInt32( 0 ), Convert.ToInt32( 0 ), Convert.ToInt32( 0 ) );

      //COM state object
      InwOpState10 cdoc = ComApiBridge.State;


      // convert ModelItem to COM Path
      InwOaPath itemPath = ComApiBridge.ToInwOaPath( geom );

      // Get Items PropertyCategoryCollection object
      InwGUIPropertyNode2 propertyNode = ( InwGUIPropertyNode2 )cdoc.GetGUIPropertyNode( itemPath, true );

      // Get PropertyCategoryCollection data
      InwGUIAttributesColl allPropertyCategories = propertyNode.GUIAttributes();

      // loop propertycategory

      Dictionary<string, InwOaPropertyColl> namedPropertyCategories = new Dictionary<string, InwOaPropertyColl>();

      foreach ( InwGUIAttribute2 propertyCategory in allPropertyCategories ) {
        try {
          string propertyCategoryName = propertyCategory.ClassUserName;
          if ( propertyCategoryName.Equals( "Item" ) ) {
            namedPropertyCategories.Add( "Item", propertyCategory.Properties() );
          }
          if ( propertyCategoryName.Equals( "Material" ) ) {
            namedPropertyCategories.Add( "Material", propertyCategory.Properties() );
          }
        } catch ( Exception e ) {
          Console.WriteLine( $"COM Property Categories: {e.Message}" );
        }
      }

      InwOaPropertyColl properties = null;
      try {
        if ( namedPropertyCategories.ContainsKey( "Material" ) ) {
          properties = namedPropertyCategories[ "Material" ];
        } else if ( namedPropertyCategories.ContainsKey( "Item" ) ) {
          properties = namedPropertyCategories[ "Item" ];
        }

        if ( properties != null ) {
          foreach ( InwOaProperty prop in properties ) {
            if ( prop.name.Equals( "Name" ) && prop.value != null && prop.value.ToString() != "" ) {
              materialName = prop.value.ToString();
            }
          }
        }
      } catch ( Exception e ) {
        Console.WriteLine( $"COM Material Name: {e.Message}" );
      }
      ////List<PropertyCategory> propertyCategories;
      ////try {
      ////  PropertyCategoryCollection c = geom.GetUserFilteredPropertyCategories();
      ////  propertyCategories = c.ToList();

      ////} catch ( Exception e ) {
      ////  Console.WriteLine( e.Message );
      ////  propertyCategories = new List<PropertyCategory>();
      ////}
      ////PropertyCategory itemCategory = propertyCategories.Where( p => p.DisplayName == "Item" ).FirstOrDefault();
      ////if ( itemCategory != null ) {
      ////  DataPropertyCollection itemProperties = itemCategory.Properties;
      ////  DataProperty itemMaterial = itemProperties.FindPropertyByDisplayName( "Material" );
      ////  if ( itemMaterial != null && itemMaterial.DisplayName != "" ) {
      ////    materialName = itemMaterial.Value.ToDisplayString();
      ////  }
      ////}

      //PropertyCategory materialPropertyCategory;
      //try {
      //  materialPropertyCategory = propertyCategories.Where( p => p.DisplayName == "Material" ).FirstOrDefault();
      //} catch ( Exception e ) {
      //  Console.WriteLine( $"MaterialProperty > {e.Message}" );
      //  materialPropertyCategory = null;
      //}

      //if ( materialPropertyCategory != null ) {
      //  DataPropertyCollection material = materialPropertyCategory.Properties;
      //  DataProperty name = material.FindPropertyByDisplayName( "Name" );
      //  if ( name != null && name.DisplayName != "" ) {
      //    materialName = name.Value.ToDisplayString();
      //  };
      //}

      Objects.Other.RenderMaterial r = new Objects.Other.RenderMaterial( 1 - geom.Geometry.OriginalTransparency, 0, 1, renderColor, black ) {
        name = materialName
      };

      return r;
    }
  }
}
