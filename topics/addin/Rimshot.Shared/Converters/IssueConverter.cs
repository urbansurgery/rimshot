using System;
using System.Globalization;
using System.Windows.Data;

namespace Rimshot.Converters
{
  public class IssueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string c;
      if ((int)value == 1)
        c = "Export " + value.ToString() + " Issue Views";
      else
        c = "Export " + value.ToString() + " Issue Views";
      return c;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

  }
}