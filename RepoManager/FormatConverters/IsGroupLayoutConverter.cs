using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SvnRadar.FormatConverters
{
    public class IsGroupLayoutConverter : IValueConverter
    {
        
        #region IValueConverter Members     



        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SvnRadar.Util.RepoBrowserConfiguration.ListViewLayoutEnum curValue = (SvnRadar.Util.RepoBrowserConfiguration.ListViewLayoutEnum)Enum.Parse(typeof(SvnRadar.Util.RepoBrowserConfiguration.ListViewLayoutEnum),
                 SvnRadar.Util.RepoBrowserConfiguration.Instance.ViewLayout.ToString());

            return (curValue == SvnRadar.Util.RepoBrowserConfiguration.ListViewLayoutEnum.RevisionView);
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
