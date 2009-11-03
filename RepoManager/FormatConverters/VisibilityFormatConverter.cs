using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SvnRadar.FormatConverters
{
    class VisibilityConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null || value is bool == false)
                return System.Windows.Visibility.Hidden;

            return ((bool)value) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
