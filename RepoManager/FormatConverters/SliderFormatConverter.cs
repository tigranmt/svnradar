using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace SvnRadar.FormatConverters
{
    class SliderFormatConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string nullString = "00";
            if (value == null)
                return nullString;
            decimal decOut = 0;
            if (decimal.TryParse(value.ToString(), out decOut))
                return decimal.Truncate(decOut).ToString("00");
           

           return nullString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
    
}
