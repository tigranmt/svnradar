using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace SvnRadar.FormatConverters
{
    public sealed class MotivationToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SvnRadar.Util.BugReportData.MessageMotivation motivationValue = (SvnRadar.Util.BugReportData.MessageMotivation)
                Enum.Parse(typeof(SvnRadar.Util.BugReportData.MessageMotivation),
                value.ToString());

            if (motivationValue == SvnRadar.Util.BugReportData.MessageMotivation.UserComment)
                return Brushes.DarkGreen;
            else
                return Brushes.DarkRed;
         
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
