using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using SvnRadar.Util;

namespace SvnRadar.FormatConverters
{
    public sealed class MotivationToTextConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            SvnRadar.Util.BugReportData.MessageMotivation motivationValue = (SvnRadar.Util.BugReportData.MessageMotivation)
             Enum.Parse(typeof(SvnRadar.Util.BugReportData.MessageMotivation),
             value.ToString());

            
            string title = string.Empty;

            try
            {
                if (motivationValue == SvnRadar.Util.BugReportData.MessageMotivation.UserComment)
                    title = AppResourceManager.FindResource("UserCommentTitle") as string;
                else
                    title = AppResourceManager.FindResource("BugTitle") as string;
            }
            catch
            {
            }

            if (string.IsNullOrEmpty(title))
                title=  string.Empty;


            return title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
