using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using SvnRadar.Util;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using SvnRadar.DataBase;

namespace SvnRadar.Common.Controls
{
    /// <summary>
    /// Common tab item for holding arbitrary repository content visualization
    /// </summary>
    public sealed class RepoTabItem : TabItem
    {
        #region fields
        /// <summary>
        /// Repositiory name
        /// </summary>
        string repoName = string.Empty;

        /// <summary>
        /// Alias assigned to the repository. As default equals to it's original name 
        /// </summary>
        string alias = string.Empty;

        /// <summary>
        /// Repository complete path
        /// </summary>
        string repoCompletePath = string.Empty;

        /// <summary>
        /// Static context menu for arbitrary tab item
        /// </summary>
        static ContextMenu contextMenu = null;


        static ListView myView = null;


    
        /// <summary>
        /// Data provider that provides update son the current ListView control
        /// </summary>
        static ObjectDataProvider dataProviderForListView = null;
        #endregion


        #region properties

        /// <summary>
        /// Holds tab item's folder repository information
        /// </summary>
        public FolderRepoInfo FolderRepoInformation { get; set; }


        /// <summary>
        /// Repository folder name
        /// </summary>
        public string RepositoryName
        {
            get { return repoName; }
        }

        /// <summary>
        /// Repository complete path
        /// </summary>
        public string RepositoryCompletePath
        {
            get { return repoCompletePath; }
        }


        /// <summary>
        /// ListView template object present on the current TAB
        /// </summary>
        public static ListView MyListView
        {
            get { return myView; }
            set
            {

                /*Assign this property only if myView is Null and the new value is not Null.
                 This means that myView assigned only ones during lifetime of application*/
                if (myView == null && value != null)
                {
                    myView = value;

                }
            }
        }

        #endregion

        #region ctor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repositoryName">The repository complete path</param>
        public RepoTabItem(string repositoryName)
        {

            if (string.IsNullOrEmpty(repositoryName))
                return;


            try
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(repositoryName);
                repoName = dirInfo.Name;
            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#endif

            }
            if (string.IsNullOrEmpty(repoName))
                return;


            alias = repoName;
            this.Name = "repoTab";
            repoCompletePath = repositoryName;
        }
        #endregion

        #region overrides
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.Header = repoName;

            this.Focusable = true;


            /*Assign control template*/
            this.ContentTemplate = FindResource("TabItemDataTemplate") as DataTemplate;
            this.Style = FindResource("TabItemStyle") as Style;

        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            this.ContextMenu = GetContextMenu();
        }


        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            this.ContextMenu = null;
        }

        #endregion


        #region member functions

        #region Context menu accessor
        /// <summary>
        /// Creates context nenu for Tab item
        /// </summary>
        /// <returns></returns>
        ContextMenu GetContextMenu()
        {

            if (contextMenu == null)
            {
                contextMenu = new ContextMenu();

                MenuItem assignAliasMenuItem = new MenuItem();
                assignAliasMenuItem.Header = FindResource(UIConstants.MENUTEM_SETALIASTOTABITEM) as string;
                assignAliasMenuItem.Command = AppCommands.SetAliasOnTabCommand;

                MenuItem removeTabItemMenuItem = new MenuItem();
                removeTabItemMenuItem.Header = FindResource(UIConstants.MENUTEM_REMOVETABITEM) as string;
                removeTabItemMenuItem.Command = AppCommands.RemoveTabCommand;

                contextMenu.Items.Add(assignAliasMenuItem);
                contextMenu.Items.Add(removeTabItemMenuItem);


            }

            return contextMenu;
        }
        #endregion

        #region update functions
        /// <summary>
        /// Updates the binding of the given column
        /// </summary>
        /// <param name="columnName">Column name to update the binding of</param>
        /// <returns> Returns True if the column was found and binding update was executed over it, False othrwise </returns>
        public bool UpdateColumnBinding(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                return false;


            if (MyListView == null || MyListView.View == null)
                return false;


            GridViewColumn col = (MyListView.View as GridView).Columns.First((x) => x.Header != null && x.Header.ToString().Equals(columnName));
            if (col != null)
            {
                col.UpdateColumnHeaderBindings();
                return true;
            }


            return false;

        }


        /// <summary>
        /// Updates the binding of the ListView's template
        /// </summary>
        public void UpdateListViewBinding()
        {
            //Get Data provider for the current ListView
            if (dataProviderForListView == null)
            {
                dataProviderForListView = FindResource("DataProviderForListView") as ObjectDataProvider;              
            }

            if (dataProviderForListView != null)
            {
                dataProviderForListView.MethodParameters.Clear();
                dataProviderForListView.MethodParameters.Add(this.RepositoryCompletePath.Trim());
            }

            //Update the columns bindings
            if (MyListView != null && MyListView.View != null)
            {
                foreach (GridViewColumn col in (MyListView.View as GridView).Columns)
                {
                    col.UpdateColumnHeaderBindings();          
                }
            }

        }

     
        #endregion

        #region event handlers

       
        #endregion

        #region UI tree navigator

        /// <summary>
        /// Find first visual child on dependency object
        /// </summary>
        /// <typeparam name="T">The type retrived, taht is DependencyObject itself</typeparam>
        /// <param name="depObject">Dependecy object on which execute search</param>
        /// <returns>Return first found visual child</returns>
        private static T FindVisualChild<T>(DependencyObject depObject)
            where T : DependencyObject
        {
            
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObject, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T foo = FindVisualChild<T>(child);
                    if (foo != null)
                        return foo;
                }
            }
            return null;
        }
        #endregion

        #region attach list view method
        /// <summary>
        /// Assigns the speciied ListView object to the current TAB
        /// </summary>
        /// <param name="listView">ListView object to attach to the TAB</param>
        public static void AttachListViewToTab(ListView listView)
        {
            myView = listView;
        }
        #endregion

        #endregion

    }
}
