﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SubversionStatistics.Util.Converters
{
    [ValueConversion(typeof(bool), typeof(ViewModel.TreeItemViewModel.TreeItemTypeEnum))]
    class TreeItemImageConverter : IValueConverter 
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ViewModel.TreeItemViewModel tvi = value as ViewModel.TreeItemViewModel;


            if (tvi.TreeItemType == ViewModel.TreeItemViewModel.TreeItemTypeEnum.Root)
                return "Images/root.png";
            else if (tvi.TreeItemType == ViewModel.TreeItemViewModel.TreeItemTypeEnum.Folder)
                return "Images/folder.png";
            else
                return "Images/file.png";
          
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
