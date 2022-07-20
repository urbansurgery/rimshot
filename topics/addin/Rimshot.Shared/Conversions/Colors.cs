using System;

namespace Rimshot.Conversions {
  class Colors {
    static public System.Drawing.Color NavisColorToColor ( Autodesk.Navisworks.Api.Color color ) {
      return System.Drawing.Color.FromArgb(
        Convert.ToInt32( color.R * 255 ),
        Convert.ToInt32( color.G * 255 ),
        Convert.ToInt32( color.B * 255 ) );
    }

  }
}
