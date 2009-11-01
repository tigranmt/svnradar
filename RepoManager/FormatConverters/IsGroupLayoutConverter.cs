using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace RepoManager.FormatConverters
{
    public class IsGroupLayoutConverter : IValueConverter
    {
        
        #region IValueConverter Members     



        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            RepoManager.Util.RepoBrowserConfiguration.ListViewLayoutEnum curValue = (RepoManager.Util.RepoBrowserConfiguration.ListViewLayoutEnum)Enum.Parse(typeof(RepoManager.Util.RepoBrowserConfiguration.ListViewLayoutEnum),
                 RepoManager.Util.RepoBrowserConfiguration.Instance.ViewLayout.ToString());

            return (curValue == RepoManager.Util.RepoBrowserConfiguration.ListViewLayoutEnum.RevisionView);
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
