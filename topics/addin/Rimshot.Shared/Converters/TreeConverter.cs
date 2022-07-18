using System;
using System.Globalization;
using System.Windows.Data;

namespace Rimshot.Converters {
  public class TreeConverter : IValueConverter {
    public object Convert ( object value, Type targetType, object parameter, CultureInfo culture ) {

      var c = value as string;
      if ( string.IsNullOrEmpty( c ) ) {
        return "null";
      } else {

        return c;
      }

    }

    public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture ) => throw new NotImplementedException();

  }
}