/* SvnRadar.EventHadlers.cs --------------------------------
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
using SvnRadar.Util;
using SvnRadar.Common.Controls;
using System.Windows.Controls;

namespace SvnRadar
{
    public partial class RepoBrowserWindow : Window
    {
        #region Tray icon event handlers

        /// <summary>
        /// DoubleClick on sys tray icon event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {          
            ShowMe();
        }

        /// <summary>
        /// Left mouse up on systray icon event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainNotifyIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            TaskNotifierManager.ShowFirstChangeIfThereIs();
        }

        /// <summary>
        /// Show changes menu item click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowChanges_Click(object sender, RoutedEventArgs e)
        {
            // TaskNotifierManager.StartNotificationSequence();
            TaskNotifierManager.ShowFirstChangeIfThereIs();
        }


        /// <summary>
        /// Show error notifications, if there are
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowErrorNotification_Click(object sender, RoutedEventArgs e)
        {
            ErrorsWindow.ShowWindow();
        }


        /// <summary>
        /// Show main wondow menu item click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowMainWindow_Click(object sender, RoutedEventArgs e)
        {
            ShowMe();
        }

        private void UpdateAll_Click(object sender, RoutedEventArgs e)
        {

            UpdateReporiotiesInSequence();
            
        }


        /// <summary>
        /// Handles request from the balloon to complete update of the repository
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Balloon_RepositoryUpdateRequested(object sender, RepositoryRoutedEventArgs e)
        {

            string repositoryUrl = e.RepositoryUrl;



            if (string.IsNullOrEmpty(repositoryUrl))
                return;           

            UpdateRepository(repositoryUrl);
        }

        /// <summary>
        /// Handles request from the balloon to view repository changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Balloon_RepositoryChangesViewRequested(object sender, RepositoryRoutedEventArgs e)
        {

            string repositoryUrl = e.RepositoryUrl;

            if (string.IsNullOrEmpty(repositoryUrl))
                return;

            if (this.Visibility != Visibility.Visible)
                this.Visibility = Visibility.Visible;

            this.BringIntoView();


            /* Find repository tab and select it, if there is*/
            RepoTabItem tabItem = null;
            foreach (TabItem rItem in mainTab.Items)
            {
                RepoTabItem repoTabItem = rItem as RepoTabItem;
                if (repoTabItem == null)
                    continue;

                if (repoTabItem.RepositoryCompletePath.Equals(repositoryUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    tabItem = repoTabItem;
                    break;
                }
            }

            /* Select found tab */
            if (tabItem != null)
                mainTab.SelectedItem = tabItem;

            /* Get view log information on just selected tab */
            GetSelectedRepositoryInfo();
           
        }



        #endregion


        #region application event handlers
        private void SelectedTabItemChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tabControl =
                e.OriginalSource as System.Windows.Controls.TabControl;
            if (tabControl == null)
                return;

            e.Handled = true;

            RepoTabItem selectedTab = tabControl.SelectedItem as RepoTabItem;
            if (selectedTab == null)
                return;

           

            FilterManager.RemoveAll();

            columnsWithFilters.ForEach((x) => x.Column.UpdateColumnHeaderBindings());
            columnsWithFilters.Clear();
            

            selectedRepoTabItem = selectedTab;
            selectedRepoTabItem.UpdateListViewBinding();


            /*Updating executing property in order to refersh UI elements on the user view*/
            RadarExecutor.ExecutingCommand = false;

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /*Ask the user if he realy wants to exit the applcaiton, if not, just return */
            string msg = FindResource("MSG_MAIN_CLOSE") as string;
            if (MessageBox.Show(msg, Application.Current.MainWindow.Title, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            /*Disposing NotifyIcon framework element*/
            MainNotifyIcon.Dispose();

        }


        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
                HideMe();
        }


        /// <summary>
        /// Handles Exit applcation request from various menu items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitMI_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }      



        #endregion

        #region Menu handlers

        private void AboutMI_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        #endregion

        #region configuration event handlers

        /// <summary>
        /// Handles repository lists chage notification in order to update the information on UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RepositoryPaths_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    foreach (Repository repo in e.NewItems)
                        AddTabFromPath(repo.RepositoryCompletePath);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    foreach (Repository repo in e.OldItems)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            RemoveTabByPath(repo.RepositoryCompletePath);
                        }));
                    }
                }
            }
        }


        /// <summary>
        /// Frequence chakc slider valu change handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frequencySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RepoBrowserConfiguration.Instance.ControlRate = (int)e.NewValue;
        }



        /// <summary>
        /// Exports configuration data in file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportMI_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            
           
            if ((bool)sfd.ShowDialog())
            {
                RepoBrowserConfiguration.Instance.Export(sfd.FileName);

            }

        
        }


        /// <summary>
        /// Imports configuration data from file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportMI_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog sfd = new Microsoft.Win32.OpenFileDialog();


            if ((bool)sfd.ShowDialog())
            {
                RepoBrowserConfiguration.Instance.Import(sfd.FileName);

                SetupBindings();

                SetupUI();

            }
        }
        #endregion
    }
}
