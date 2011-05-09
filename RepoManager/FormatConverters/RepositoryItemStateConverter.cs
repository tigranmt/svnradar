/* RepositoryItemStateConverter.cs --------------------------------
 * 
 * * Copyright (c) 2009 Tigran Martirosyan 
 * * Contact and Information: tigranmt@gmail.com 
 * * This application is free software; you can redistribute it and/or 
 * * Modify it under the terms of the GPL license
 * * THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, 
 * * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
 * * OTHER DEALINGS IN THE SOFTWARE. 
 * * THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE 
 *  */
// ---------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace SvnRadar.FormatConverters
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



        char CharFromRepoItemState(SvnRadar.Util.RepoInfo.RepositoryItemState itemState)
        {
            if (itemState == SvnRadar.Util.RepoInfo.RepositoryItemState.Add)
                return '+';
            else if (itemState == SvnRadar.Util.RepoInfo.RepositoryItemState.NeedToBeUpdatedFromRepo)
                return 'M';
            else if (itemState == SvnRadar.Util.RepoInfo.RepositoryItemState.Modified)
                return '%';
            return
                'o';
        }


        object ImagefromRepoItemState(SvnRadar.Util.RepoInfo.RepositoryItemState itemState)
        {
            if (ImageVsRepoItemState.ContainsKey((int)itemState))
                return ImageVsRepoItemState[(int)itemState];

            if (itemState == SvnRadar.Util.RepoInfo.RepositoryItemState.Add)
                ImageVsRepoItemState[(int)itemState] =  FindResource("AddImage") as System.Windows.Controls.Image;
            else if (itemState == SvnRadar.Util.RepoInfo.RepositoryItemState.NeedToBeUpdatedFromRepo)
                ImageVsRepoItemState[(int)itemState] = FindResource("ModifiedImage") as System.Windows.Controls.Image;
            else if (itemState == SvnRadar.Util.RepoInfo.RepositoryItemState.Modified)
                ImageVsRepoItemState[(int)itemState] = FindResource("ModifiedImage") as System.Windows.Controls.Image;
            else if (itemState == SvnRadar.Util.RepoInfo.RepositoryItemState.Merged)
                ImageVsRepoItemState[(int)itemState] = FindResource("MergedImage") as System.Windows.Controls.Image;
            else if (itemState == SvnRadar.Util.RepoInfo.RepositoryItemState.Deleted)
                ImageVsRepoItemState[(int)itemState] = FindResource("DeleteIcon") as System.Windows.Controls.Image;
            else
                ImageVsRepoItemState[(int)itemState] = FindResource("EmptyImage") as System.Windows.Controls.Image;


            return ImageVsRepoItemState[(int)itemState];

        }



        #region IValueConverter Members     



        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            SvnRadar.Util.RepoInfo.RepositoryItemState itemState = (SvnRadar.Util.RepoInfo.RepositoryItemState)Enum.Parse(typeof(SvnRadar.Util.RepoInfo.RepositoryItemState),
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
