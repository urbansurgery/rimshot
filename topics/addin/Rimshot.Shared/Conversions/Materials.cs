using Autodesk.Navisworks.Api;
using System;
using Color = System.Drawing.Color;

namespace Rimshot.Conversions {
  class Materials {
    static public Objects.Other.RenderMaterial TranslateMaterial ( ModelItem geom ) {

      Color active = Colors.NavisColorToColor( geom.Geometry.ActiveColor );
      Color original = Colors.NavisColorToColor( geom.Geometry.OriginalColor );
      Color permanent = Colors.NavisColorToColor( geom.Geometry.PermanentColor );

      Color black = Color.FromArgb( Convert.ToInt32( 0 ), Convert.ToInt32( 0 ), Convert.ToInt32( 0 ) );

      var materialPropertyCategory = geom.PropertyCategories.FindCategoryByDisplayName( "Material" );
      if ( materialPropertyCategory != null ) {
        var material = ( object )materialPropertyCategory.Properties;
      }


      Objects.Other.RenderMaterial r = new Objects.Other.RenderMaterial( 1 - geom.Geometry.OriginalTransparency, 0, 1, original, black ) {
        name = "NavisMaterial"
      };

      return r;
    }
  }
}
