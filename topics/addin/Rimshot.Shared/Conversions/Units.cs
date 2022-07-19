using System.Windows;

namespace Rimshot.Conversions {
  internal class Units {
    static public double GetUnits () {
      string units = Autodesk.Navisworks.Api.Application.ActiveDocument.Units.ToString();
      double factor;
      switch ( units ) {
        case "Centimeters":
          factor = 100;
          break;
        case "Feet":
          factor = 3.28084;
          break;
        case "Inches":
          factor = 39.3701;
          break;
        case "Kilometers":
          factor = 0.001;
          break;
        case "Meters":
          factor = 1;
          break;
        case "Micrometers":
          factor = 1000000;
          break;
        case "Miles":
          factor = 0.000621371;
          break;
        case "Millimeters":
          factor = 1000;
          break;
        case "Mils":
          factor = 39370.0787;
          break;
        case "Yards":
          factor = 1.09361;
          break;
        default:
          MessageBox.Show( "Units " + units + " not recognized." );
          factor = 1;
          break;
      }
      return factor;
    }
  }
}
