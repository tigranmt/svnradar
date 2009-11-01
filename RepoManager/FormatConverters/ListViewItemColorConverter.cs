using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using RepoManager.Common.Controls;
using RepoManager.DataBase;
using System.Windows.Controls;
using RepoManager.Util;

namespace RepoManager.FormatConverters
{
    public sealed class ListViewItemColorConverter : IValueConverter
    {

        #region IValueConverter Members

        List<int> revisions = new List<int>();
      

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (RepoBrowserConfiguration.Instance.ViewLayout == RepoBrowserConfiguration.ListViewLayoutEnum.RevisionView)
                return true;

            int curRevNum = (int)value;

            int indexOfRev = revisions.IndexOf(curRevNum);
            if (indexOfRev < 0)
            {
                revisions.Add(curRevNum);
                indexOfRev = revisions.Count - 1;
            }

          
            if (indexOfRev % 2 == 0)
                return true;
            else
                return false;
           

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
