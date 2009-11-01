﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using RepoManager.Util;
using System.Windows.Input;
using RepoManager.Common.Controls;
using System.Windows.Data;
using RepoManager.DataBase;

namespace RepoManager
{
    public partial class RepoBrowserWindow : Window
    {
        /// <summary>
        /// Repo executor object declared in resources of the applciation
        /// </summary>
        static RepoExecutor repoExecutor = null;

        /// <summary>
        /// Filter manager object declared in resources of the applciation
        /// </summary>
        static FilterManager filterManager = null;


        /// <summary>
        /// Handles the command executed due the click on arbitrary button on configuration tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigurationTabButtonClicked(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            if (btn == null)
            {
                return;
            }
            if (btn.Name == UIConstants.BTN_FIND_SVN_PATH)
            {
                SetupSvnPath();
            }
            else if (btn.Name == UIConstants.BTN_NAME_DISCARDCHANGES)
            {
                ResetConfiguration();
            }
            else if (btn.Name == UIConstants.BTN_NAME_SAVE)
            {
                SaveConfiguration();
            }
            else if (btn.Name == UIConstants.BTN_ADDNEW_SVN_PATH)
            {
                AddNewSvnPath();
            }
            else if (btn.Name == UIConstants.BTN_DELETE_SVN_PATH)
            {
                if (lbSvnPaths.SelectedItem != null)
                {
                    string selectedPath = lbSvnPaths.SelectedItem as string;
                    RemoveSvnPath(selectedPath);
                }
            }


        }



        /// <summary>
        /// Handles the execution of Set alias command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSetAliasCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            TabControl SelTabControl = e.Source as TabControl;
            RepoTabItem repoTabItem = null;

            /*The message can be recieved from both controls, so control both conditions*/
            if (SelTabControl != null)
                repoTabItem = SelTabControl.SelectedItem as RepoTabItem;
            else
                repoTabItem = e.Source as RepoTabItem;


            if (repoTabItem == null)
                return;

            RepoAliasTextBox.AttachToRepoTabItem(repoTabItem);


        }

        /// <summary>
        /// Handles execution of remove tab command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRemoveTabCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }


        /// <summary>
        /// Handles execution of request on information for working copy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGetWorkingCopyInfoCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            if (selRepo == null)
                return;

            string selRepoString = selRepo.RepositoryCompletePath;
        }

        /// <summary>
        /// Retrives the RepoExecutor object from the Application resources
        /// </summary>
        /// <returns>Returns RepoExecutor object</returns>
        RepoExecutor RepoExecutor
        {
            get
            {
                return repoExecutor ??
                    (repoExecutor = (RepoExecutor)((ObjectDataProvider)FindResource("repoExecutor")).ObjectInstance);
            }
        }


        /// <summary>
        /// Retrives the FilterManager object from the Application resources
        /// </summary>
        /// <returns>Returns FilterManager object</returns>
        FilterManager FilterManager
        {
            get
            {
                return filterManager ??
                    (filterManager = (FilterManager)((ObjectDataProvider)FindResource("filterManager")).ObjectInstance);
            }
        }



        /// <summary>
        /// Conditionize the Enable or Disable of the Get repository info command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGetRepositoryInfoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        /// <summary>
        /// Gets the repository change log from the currently selected repository
        /// </summary>
        private void GetSelectedRepositoryInfo()
        {
            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            if (selRepo == null)
                return;

            string selRepoString = selRepo.RepositoryCompletePath;
            if (string.IsNullOrEmpty(selRepoString))
                return;

            FolderRepoInfo folderRepoInfo = GetFolderRepoInfo(selRepoString, false);

            if (folderRepoInfo != null)
            {
                /*Clear previously loadd information*/
                RepoInfoBase.ClearRepoInfo(selRepo.RepositoryName);


                /*Assign to the executor current folder repo info, to pass it to the executor process in the future*/
                RepoExecutor.currentRepoFolderIndormation = folderRepoInfo;

                /*Set up RepoTabItem property in order to maintain the information related to the repository that TabItem rapresents*/
                selRepo.FolderRepoInformation = folderRepoInfo;

                /*Update parameter of the connected ObjectDataProvider*/
                selRepo.UpdateListViewBinding();

                /* Execute command **/
                RepoExecutor.GetRepositoryLog(selRepoString, folderRepoInfo, false);


            }
            else
            {
                //TODO: notify an error to the user
            }
        }


        /// <summary>
        /// Handles execution of request on information for repository
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGetRepositoryInfoCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GetSelectedRepositoryInfo();
        }



        /// <summary>
        /// Handles execution of request to update working copy from the repository on specified items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateSelectedCommand(object sender, ExecutedRoutedEventArgs e)
        {



        }


        /// <summary>
        /// Conditionize the Enable or Disable of the Update repository command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateRepositoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (RepoTabItem.MyListView != null && RepoTabItem.MyListView.Items != null &&
                RepoTabItem.MyListView.Items.Count > 0)
                e.CanExecute = true;
        }



        /// <summary>
        /// Executes updates specified repository .
        /// </summary>
        /// <param name="repositoryPath">Repository complete working copy path. If Null or Empty string is passed, the 
        /// current selected repository will be updated, if there is any.</param>
        private void UpdateRepository(string repositoryPath)
        {


            string repoName = System.IO.Path.GetFileNameWithoutExtension(repositoryPath);
            if (!string.IsNullOrEmpty(repoName))
            {
                RepositoryProcess availableRepoProcess = RepoExecutor.IsProcesStillAvailable(repoName);
                if (availableRepoProcess != null)
                {
                    if (CommandStringsManager.IsUpdateCommand(availableRepoProcess.Command))
                    {
                        string msg = FindResource("MSG_MULTIPLEUPDATECOMMANDS") as string;
                        if (!string.IsNullOrEmpty(msg))
                            MessageBox.Show(msg, App.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
            }

            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            string selRepoCompletePath = string.Empty;


            if (!string.IsNullOrEmpty(repositoryPath))
            {
                selRepoCompletePath = repositoryPath;
                if (string.IsNullOrEmpty(selRepoCompletePath))
                    return;

            }
            else if (selRepo != null)
            {
                selRepoCompletePath = selRepo.RepositoryCompletePath;
            }
            else
                return;



            if (string.IsNullOrEmpty(selRepoCompletePath))
                return;

            if (!System.IO.Directory.Exists(selRepoCompletePath))
                return;


            try
            {
                System.IO.Directory.SetCurrentDirectory(selRepoCompletePath);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
                return;
            }


            //Creating trace window
            UpdateTraceWindow utw = new UpdateTraceWindow();
            if (selRepo != null)
                utw.Title += "   /" + selRepo.RepositoryName;
            else if (!string.IsNullOrEmpty(selRepoCompletePath))
            {
                utw.Title += "   /" + System.IO.Path.GetDirectoryName(selRepoCompletePath);
            }
            else
                return;
            utw.Topmost = true;


            /*Assign to the executor current folder repo info, to pass it to the executor process in the future*/
            RepoExecutor.currentUpdateTraceWindow = utw;

            /* Register window in the window manager */
            WindowsManager.AddNewWindow(utw);
            RepoExecutor.UpdateRepository(selRepoCompletePath, false);

            /*Initialize properties of the window */
            utw.Process = RepoExecutor.LastExecutedProcess;
            utw.RelatedRepositoryName = RepoExecutor.LastExecutedProcess.RelatedRepositoryName;
            utw.RelatedCommand = RepoExecutor.LastExecutedProcess.Command;


            utw.Show();

        }

        /// <summary>
        /// Handles execution of request to update working copy from the repository
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateRepositoryCommand(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateRepository(e.Parameter as string);
        }

        /// <summary>
        /// Gets the given working copy folder repository related information
        /// </summary>
        /// <param name="folderPath">Folder complete working copy path on the machine, or URL to repository folder</param>
        /// <param name="isCallForSysTray">If True the RepositoryProcess for sys tray will be used</param>
        /// <returns>FolderRepoInfo object, Null otherwise</returns>
        private FolderRepoInfo GetFolderRepoInfo(string folderPath, bool isCallForSysTray)
        {
            if (string.IsNullOrEmpty(folderPath))
                return null;

            if (!System.IO.Directory.Exists(folderPath))
                return null;

            return RepoExecutor.GetFolderRepoInfo(folderPath, isCallForSysTray);

        }


        private void OnShowRevisionInfoCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            if (selRepo == null)
                return;

            string selRepoString = selRepo.RepositoryCompletePath;
            if (string.IsNullOrEmpty(selRepoString))
                return;


            if (RepoTabItem.MyListView == null)
                return;

            /*Can not show information for more then one item at a time so do not to anything*/
            if (RepoTabItem.MyListView.SelectedItems.Count > 1)
                return;

            object SelObject = RepoManager.Common.Controls.RepoTabItem.MyListView.SelectedItem;

            if (SelObject == null)
                SelObject = e.OriginalSource;

            RepoInfo repositoryInfo = SelObject as RepoInfo;
            if (repositoryInfo == null)
                return;

            /*Add revison object to the base, in order to populate it from the
             commands output in the future. The strong key, in this case, is the Revision number*/
            RevisionInfo revi = RepoInfoBase.AddRevisionInfoString(selRepo.RepositoryName, repositoryInfo.Revision, repositoryInfo.Item, repositoryInfo.Date, string.Empty);
            RevisionInfoWindow reviwWnd = new RevisionInfoWindow(revi);
            reviwWnd.Topmost = true;
            reviwWnd.RelatedRepositoryName = selRepo.RepositoryName;


            WindowsManager.AddNewWindow(reviwWnd);

            repoExecutor.GetRevisionInfo(selRepoString, repositoryInfo.Revision, repositoryInfo.Item, selRepo.FolderRepoInformation, false);
            reviwWnd.Process = RepoExecutor.LastExecutedProcess;
            reviwWnd.RelatedCommand = RepoExecutor.LastExecutedProcess.Command;
            reviwWnd.Show();

        }

        private void OnRemoveFilterFromColumnCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            if (selRepo == null)
                return;

            TextBox tb = e.Parameter as TextBox;
            if (tb == null)
                return;

            if (tb.Tag == null)
                return;

            string colName = tb.Tag.ToString();
            if (string.IsNullOrEmpty(colName))
                return;

            if (FilterManager.RemoveFilter(selRepo.RepositoryName, colName))
            {
                selRepo.UpdateColumnBinding(colName);

                UpdateObjectDataProvider();
            }

        }


        private void OnShowFilterOnColumnCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            if (selRepo == null)
                return;

            GridViewColumnHeader gvCh = e.Parameter as GridViewColumnHeader;
            if (gvCh == null)
                return;

            if (gvCh.Column == null)
                return;

            if (gvCh.Column.Header == null)
                return;




            /*Enable search cappability on the column*/

            //TODO : Add textbox control to the column header

            /*Register requested filter in the database*/
            if (FilterManager.AddFilterToColumn(selRepo.RepositoryName, gvCh.Column.Header.ToString(), string.Empty))
            {
                gvCh.Column.UpdateColumnHeaderBindings();
            }

        }

        private void OnSetFilterOnColumnCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            if (selRepo == null)
                return;

            TextBox tb = e.Parameter as TextBox;
            if (tb == null)
                return;

            if (tb.Tag == null)
                return;

            string colName = tb.Tag.ToString();
            if (string.IsNullOrEmpty(colName))
                return;

            FilterManager.UpdateFilterOnColumn(selRepo.RepositoryName, colName, tb.Text);


            UpdateObjectDataProvider();
        }


        /// <summary>
        /// Forces update global ObjectDataProvider object
        /// </summary>
        private void UpdateObjectDataProvider()
        {

            ObjectDataProvider odp = FindResource("DataProviderForListView") as ObjectDataProvider;
            if (odp != null)
            {
                odp.Refresh();
            }
        }


        /// <summary>
        /// Handles request on group available items list into groups by RevisionNumber
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnGroupByRevisionNumberCommand(object sender, ExecutedRoutedEventArgs args)
        {
            RepoBrowserConfiguration.Instance.ViewLayout = RepoBrowserConfiguration.ListViewLayoutEnum.RevisionView;
            CollectionViewSource colViewSource = FindResource("source") as CollectionViewSource;
            if (colViewSource == null)
                return;

            colViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Revision"));

        }



        /// <summary>
        /// Handles request on visualize items in flat view  mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnFlatViewCommand(object sender, ExecutedRoutedEventArgs args)
        {
            RepoBrowserConfiguration.Instance.ViewLayout = RepoBrowserConfiguration.ListViewLayoutEnum.FlatView;
            CollectionViewSource colViewSource = FindResource("source") as CollectionViewSource;
            if (colViewSource == null)
                return;

            colViewSource.GroupDescriptions.Clear();
        }




    }
}
