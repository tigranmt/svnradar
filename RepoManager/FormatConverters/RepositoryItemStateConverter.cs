using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace RepoManager.FormatConverters
{
    class RepositoryItemStateConverter : IValueConverter
    {

        static Dictionary<int, System.Windows.Controls.Image> ImageVsRepoItemState = new Dictionary<int, System.Windows.Controls.Image>();

        /// <summary>
        /// Finds the requested resource from the application's resource dictionary
        /// </summary>
        /// <param name="esourceKey">Resource key</param>
        /// <returns>Resource object if exists, Null otherwise</returns>
        static object FindResource(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey))
                return null;

            return Application.Current.FindResource(resourceKey);
        }



        char CharFromRepoItemState(RepoManager.Util.RepoInfo.RepoItemState itemState)
        {
            if (itemState == RepoManager.Util.RepoInfo.RepoItemState.Add)
                return '+';
            else if (itemState == RepoManager.Util.RepoInfo.RepoItemState.NeedToBeUpdatedFromRepo)
                return 'M';
            else if (itemState == RepoManager.Util.RepoInfo.RepoItemState.Modified)
                return '%';
            return
                'o';
        }


        object ImagefromRepoItemState(RepoManager.Util.RepoInfo.RepoItemState itemState)
        {
            if (ImageVsRepoItemState.ContainsKey((int)itemState))
                return ImageVsRepoItemState[(int)itemState];

            if (itemState == RepoManager.Util.RepoInfo.RepoItemState.Add)
                ImageVsRepoItemState[(int)itemState] =  FindResource("AddImage") as System.Windows.Controls.Image;
            else if (itemState == RepoManager.Util.RepoInfo.RepoItemState.NeedToBeUpdatedFromRepo)
                ImageVsRepoItemState[(int)itemState] = FindResource("ModifiedImage") as System.Windows.Controls.Image;
            else if (itemState == RepoManager.Util.RepoInfo.RepoItemState.Modified)
                ImageVsRepoItemState[(int)itemState] = FindResource("ModifiedImage") as System.Windows.Controls.Image;
            else if (itemState == RepoManager.Util.RepoInfo.RepoItemState.Deleted)
                ImageVsRepoItemState[(int)itemState] = FindResource("DeleteImage") as System.Windows.Controls.Image;
            else
                ImageVsRepoItemState[(int)itemState] = FindResource("EmptyImage") as System.Windows.Controls.Image;


            return ImageVsRepoItemState[(int)itemState];

        }



        #region IValueConverter Members     



        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            RepoManager.Util.RepoInfo.RepoItemState itemState = (RepoManager.Util.RepoInfo.RepoItemState)Enum.Parse(typeof(RepoManager.Util.RepoInfo.RepoItemState),
                value.ToString());
            System.Windows.Controls.Image im = ImagefromRepoItemState(itemState) as System.Windows.Controls.Image;

            return im.Source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

}
