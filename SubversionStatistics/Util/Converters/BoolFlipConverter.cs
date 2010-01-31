using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace SubversionStatistics.Util.Converters
{

    [ValueConversion(typeof(bool), typeof(bool))]
    public class FlipBoolConverter : IValueConverter
    {
        public static FlipBoolConverter SingleInstance = new FlipBoolConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }
    }

}
