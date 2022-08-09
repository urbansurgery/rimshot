using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Interop;
using Autodesk.Navisworks.Api.Interop.ComApi;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Props = Rimshot.Conversions.Properties;

namespace Rimshot.Conversions {
  internal class Properties {
    static public string SanitizePropertyName ( string name ) {
      if ( name == "Item" ) {
        return "$Item";
      }

      // Regex pattern from speckle-sharp/Core/Core/Models/DynamicBase.cs IsPropNameValid
      return Regex.Replace( name, @"[\.\/]", "_" );
    }

    static public void BuildPropertyCategory ( InwGUIAttribute2 propertyCategory, InwOaProperty property, ref Base propertyCategoryBase ) {
      string categoryName;
      string propertyName;
      try {
        categoryName = Props.SanitizePropertyName( propertyCategory.ClassUserName );
      } catch ( Exception err ) {
        Logging.ErrorLog( $"Category Name not converted. {err.Message}" );
        return;
      }

      try {
        propertyName = Props.SanitizePropertyName( property.UserName );
      } catch ( Exception err ) {
        Logging.ErrorLog( $"Category Name not converted. {err.Message}" );
        return;
      }

      dynamic propertyValue = property.value;

      //property.value

      dynamic t = propertyValue.GetType();

      Console.WriteLine( t.ToString() );

      //VariantDataType type = property.Value.DataType;

      //switch ( type ) {
      //  case VariantDataType.Boolean: propertyValue = property.Value.ToBoolean(); break;
      //  case VariantDataType.DisplayString: propertyValue = property.Value.ToDisplayString(); break;
      //  case VariantDataType.IdentifierString: propertyValue = property.Value.ToIdentifierString(); break;
      //  case VariantDataType.Int32: propertyValue = property.Value.ToInt32(); break;
      //  case VariantDataType.Double: propertyValue = property.Value.ToDouble(); break;
      //  case VariantDataType.DoubleAngle: propertyValue = property.Value.ToDoubleAngle(); break;
      //  case VariantDataType.DoubleArea: propertyValue = property.Value.ToDoubleArea(); break;
      //  case VariantDataType.DoubleLength: propertyValue = property.Value.ToDoubleLength(); break;
      //  case VariantDataType.DoubleVolume: propertyValue = property.Value.ToDoubleVolume(); break;
      //  case VariantDataType.DateTime: propertyValue = property.Value.ToDateTime().ToString(); break;
      //  case VariantDataType.NamedConstant: propertyValue = property.Value.ToNamedConstant().DisplayName; break;
      //  case VariantDataType.Point3D: propertyValue = property.Value.ToPoint3D(); break;
      //  case VariantDataType.None: break;
      //  case VariantDataType.Point2D:
      //    break;
      //  default:
      //    break;
      //}

      if ( propertyValue != null && propertyCategoryBase != null ) {
        object keyPropValue = propertyCategoryBase[ propertyName ];

        if ( keyPropValue == null ) {
          propertyCategoryBase[ propertyName ] = propertyValue;
        } else if ( keyPropValue is List<dynamic> ) {
          List<dynamic> arrayPropValue = ( List<dynamic> )keyPropValue;

          if ( !arrayPropValue.Contains( propertyValue ) ) {

            arrayPropValue.Add( propertyValue );
          }

          propertyCategoryBase[ propertyName ] = arrayPropValue;
        } else {
          dynamic existingValue = keyPropValue;

          if ( existingValue != propertyValue ) {
            List<dynamic> arrayPropValue = new List<dynamic> {
                  existingValue,
                  propertyValue
                };

            propertyCategoryBase[ propertyName ] = arrayPropValue;
          }
        }
      }
    }

    static public void BuildPropertyCategory ( PropertyCategory propertyCategory, DataProperty property, ref Base propertyCategoryBase ) {
      string categoryName;
      string propertyName;
      try {
        categoryName = Props.SanitizePropertyName( propertyCategory.DisplayName );
      } catch ( Exception err ) {
        Logging.ErrorLog( $"Category Name not converted. {err.Message}" );
        return;
      }

      try {
        propertyName = Props.SanitizePropertyName( property.DisplayName );
      } catch ( Exception err ) {
        Logging.ErrorLog( $"Category Name not converted. {err.Message}" );
        return;
      }

      dynamic propertyValue = null;

      VariantDataType type = property.Value.DataType;

      switch ( type ) {
        case VariantDataType.Boolean: propertyValue = property.Value.ToBoolean(); break;
        case VariantDataType.DisplayString: propertyValue = property.Value.ToDisplayString(); break;
        case VariantDataType.IdentifierString: propertyValue = property.Value.ToIdentifierString(); break;
        case VariantDataType.Int32: propertyValue = property.Value.ToInt32(); break;
        case VariantDataType.Double: propertyValue = property.Value.ToDouble(); break;
        case VariantDataType.DoubleAngle: propertyValue = property.Value.ToDoubleAngle(); break;
        case VariantDataType.DoubleArea: propertyValue = property.Value.ToDoubleArea(); break;
        case VariantDataType.DoubleLength: propertyValue = property.Value.ToDoubleLength(); break;
        case VariantDataType.DoubleVolume: propertyValue = property.Value.ToDoubleVolume(); break;
        case VariantDataType.DateTime: propertyValue = property.Value.ToDateTime().ToString(); break;
        case VariantDataType.NamedConstant: propertyValue = property.Value.ToNamedConstant().DisplayName; break;
        case VariantDataType.Point3D: propertyValue = property.Value.ToPoint3D(); break;
        case VariantDataType.None: break;
        case VariantDataType.Point2D:
          break;
        default:
          break;
      }

      if ( propertyValue != null && propertyCategoryBase != null ) {
        object keyPropValue = propertyCategoryBase[ propertyName ];

        if ( keyPropValue == null ) {
          propertyCategoryBase[ propertyName ] = propertyValue;
        } else if ( keyPropValue is List<dynamic> ) {
          List<dynamic> arrayPropValue = ( List<dynamic> )keyPropValue;

          if ( !arrayPropValue.Contains( propertyValue ) ) {

            arrayPropValue.Add( propertyValue );
          }

          propertyCategoryBase[ propertyName ] = arrayPropValue;
        } else {
          dynamic existingValue = keyPropValue;

          if ( existingValue != propertyValue ) {
            List<dynamic> arrayPropValue = new List<dynamic> {
                  existingValue,
                  propertyValue
                };

            propertyCategoryBase[ propertyName ] = arrayPropValue;
          }
        }
      }
    }

    public static List<Tuple<NamedConstant, NamedConstant>> LoadQuickProperties () {
      List<Tuple<NamedConstant, NamedConstant>> quickProperties_CategoryPropertyPairs = new List<Tuple<NamedConstant, NamedConstant>>();
      using ( LcUOptionLock optionLock = new LcUOptionLock() ) {
        LcUOptionSet set = LcUOption.GetSet( "interface.smart_tags.definitions", optionLock );
        int numOptions = set.GetNumOptions();
        if ( numOptions > 0 ) {
          for ( int index = 0; index < numOptions; ++index ) {
            LcUOptionSet lcUoptionSet = set.GetValue( index, null );
            NamedConstant cat = lcUoptionSet.GetName( "category" ).GetPtr();
            NamedConstant prop = lcUoptionSet.GetName( "property" ).GetPtr();
            quickProperties_CategoryPropertyPairs.Add( Tuple.Create( cat, prop ) );
          }
        }
      }
      return quickProperties_CategoryPropertyPairs;
    }
  }
}
