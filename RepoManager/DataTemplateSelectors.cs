/* DataTemplateSelectors.cs --------------------------------
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
using System.Windows;
using System.Windows.Controls;
using SvnRadar.Common.Controls;
using System.Windows.Data;

namespace SvnRadar
{


   



    /// <summary>
    /// Custom ListView header template selector
    /// </summary>
    public class ListViewHeaderDataTemplateSelector : DataTemplateSelector
    {

        static FilterManager filterManager = null;

        /// <summary>
        /// Retrives the FilterManager object from the Application resources
        /// </summary>
        /// <returns></returns>
        static FilterManager FilterManager
        {
            get
            {
                if (filterManager == null)
                    filterManager = (FilterManager)((ObjectDataProvider)FindResource("filterManager")).ObjectInstance;
                return filterManager;

                
            }
        }


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

        /// <summary>
        /// Selects neccesary template base on the application logic
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            RepoTabItem selTab = RepoBrowserWindow.SelectedRepoTabItem;
            if (selTab == null)
                return null;

            
            string columnName = item as string;
            if (string.IsNullOrEmpty(columnName))
                return null;

            return SelectTemplate(selTab.RepositoryCompletePath, columnName.Trim());      
        }


        /// <summary>
        /// Selects neccesary template base on the application logic
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static DataTemplate SelectTemplate(string repositoryName, string columnName)
        {
            if (!FilterManager.ExistsFilterFor(repositoryName, columnName))
                return FindResource("ListViewHeaderNormalTemplate") as DataTemplate;
            else
                return FindResource("ListViewHeaderFilteredTemplate") as DataTemplate;
        }

     }
    
}
