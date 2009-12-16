/* SvnRadar.Commands.cs --------------------------------
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
using SvnRadar.Util;
using System.Windows.Input;
using SvnRadar.Common.Controls;
using System.Windows.Data;
using SvnRadar.DataBase;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ComponentModel;

namespace SvnRadar
{
    public partial class RepoBrowserWindow : Window
    {
        /// <summary>
        /// Repo executor object declared in resources of the applciation
        /// </summary>
        static SvnRadarExecutor svnRadarExecutor = null;

        /// <summary>
        /// Filter manager object declared in resources of the applciation
        /// </summary>
        static FilterManager filterManager = null;


        /// <summary>
        /// True if the program is processing a sequence of the repositories by update command
        /// </summary>
        bool bRepositorySequenceUpdating = false;


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
            else if (btn.Name == UIConstants.BTN_FIND_WINMERGE_PATH)
            {
                SetupWinMergePath();
            }
            else if (btn.Name == UIConstants.BTN_REMOVE_WINMERGE_PATH)
            {
                RemoveWinMergePath();
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
                    Repository repo = lbSvnPaths.SelectedItem as Repository;
                    RemoveSvnPath(repo.RepositoryCompletePath);
                }
            }


        }


        /// <summary>
        /// Handles the execution of Open working copy location command 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenWorkingCopyLocationCommand(object sender, ExecutedRoutedEventArgs e)
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

            if (string.IsNullOrEmpty(repoTabItem.RepositoryCompletePath))
                return;

            if (!System.IO.Directory.Exists(repoTabItem.RepositoryCompletePath))
                return;


            try
            {
                System.Diagnostics.Process.Start(repoTabItem.RepositoryCompletePath);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex,true);
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
        /// Retrives the RadarExecutor object from the Application resources
        /// </summary>
        /// <returns>Returns RadarExecutor object</returns>
        SvnRadarExecutor RadarExecutor
        {
            get
            {
                return svnRadarExecutor ??
                    (svnRadarExecutor = (SvnRadarExecutor)((ObjectDataProvider)FindResource("svnRadarExecutor")).ObjectInstance);
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

            FolderRepoInfo folderRepoInfo = svnRadarExecutor.GetFolderRepoInfo(selRepoString, false);

            if (folderRepoInfo != null)
            {
                /*Clear previously loadd information*/
                RepoInfoBase.ClearRepoInfo(selRepo.RepositoryCompletePath);


                /*Assign to the executor current folder repo info, to pass it to the executor process in the future*/
                SvnRadarExecutor.currentRepoFolderIndormation = folderRepoInfo;

                /*Set up RepoTabItem property in order to maintain the information related to the repository that TabItem rapresents*/
                selRepo.FolderRepoInformation = folderRepoInfo;

                /*Update parameter of the connected ObjectDataProvider*/
                selRepo.UpdateListViewBinding();
               

                /* Execute command **/
                RadarExecutor.GetRepositoryLog(selRepoString, folderRepoInfo, false);


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
        /// Handles execution of request to update working copy from the repository on specified file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateSingleFileCommand(object sender, ExecutedRoutedEventArgs e)
        {


            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;

            /*Only UI access, I must have some Tab selected in ordder to be able to slect any item from the list*/
            if (selRepo == null)
                return;

            RepoInfo selectedSingleInfo = RepoInfo.SelectedInfo;

            if (selectedSingleInfo == null)
                return;

            if (string.IsNullOrEmpty(selectedSingleInfo.Item))
                return;



            /*If there is already Update executing over specified repository do not call again, as the folder is alreday locked by Subversion.*/
            if (!string.IsNullOrEmpty(selRepo.RepositoryCompletePath))
            {

                RepositoryProcess availableRepoProcess = RadarExecutor.IsProcesStillAvailable(selRepo.RepositoryCompletePath);
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


            try
            {
                System.IO.Directory.SetCurrentDirectory(selRepo.RepositoryCompletePath);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
                return;
            }


            //Creating trace window
            UpdateTraceWindow utw = new UpdateTraceWindow();
            if (selRepo != null)
                utw.Title += "   /" + selRepo.RepositoryCompletePath;
            else if (!string.IsNullOrEmpty(selRepo.RepositoryCompletePath))
            {
                utw.Title += "   /" + System.IO.Path.GetDirectoryName(selRepo.RepositoryCompletePath);
            }
            else
                return;
            utw.Topmost = true;


            /*Assign to the executor current folder repo info, to pass it to the executor process in the future*/
            SvnRadarExecutor.currentUpdateTraceWindow = utw;

            /* Register window in the window manager */
            WindowsManager.AddNewWindow(utw);
            RadarExecutor.UpdateReposiotrySingleFile(selRepo.RepositoryCompletePath, selectedSingleInfo.Revision, selectedSingleInfo.Item);

            /*Initialize properties of the window */
            utw.Process = SvnRadarExecutor.LastExecutedProcess;
            utw.RelatedRepositoryName = SvnRadarExecutor.LastExecutedProcess.RelatedRepositoryName;
            utw.RelatedCommand = SvnRadarExecutor.LastExecutedProcess.Command;


            utw.Show();



        }


        /// <summary>
        /// Conditionize the Enable or Disable of the Update repository command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateRepositoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;

            RepoTabItem repoTabItem = mainTab.SelectedItem as RepoTabItem;
            if (repoTabItem == null)
                return;
            int count = RepoInfoBase.GetRepoInfoCount(repoTabItem.RepositoryCompletePath);
            e.CanExecute = (count > 0);
        }




        /// <summary>
        /// Updates all avalable repsotiory in sequence
        /// </summary>
        private void UpdateReporiotiesInSequence()
        {

            /*If I,m not finished updateing the sequence of the repositories, do not execute any kind of command*/
            if (bRepositorySequenceUpdating)
                return;

            FolderRepoInfo[] friArray = null;
            lock (TaskNotifierManager.notificationList)
            {
                friArray = TaskNotifierManager.notificationList.ToArray<FolderRepoInfo>();
            }

            /*Set global boolean value*/
            bRepositorySequenceUpdating = true;

            /*Begin new thread in order to not block UI interface.
             In the specified thread begin iteration over the repositories that are going to be updated by
             user request in sequence*/
            BackgroundWorker _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                foreach (FolderRepoInfo changedRepoInfo in friArray)
                {
                    UpdateTraceWindow utw = null;

                    Dispatcher.Invoke(new Action(() =>
                    {
                        utw = UpdateRepository(changedRepoInfo.FolderPath);
                        utw.SignalStarUpdate();
                    }));


                    if (utw == null)
                        continue;

                    /*If during the utw object creation the relative process is already distroyed or has exited, 
                     * continnue to the next item, if there is any*/
                    if (utw.Process == null || utw.Process.HasExited)
                        continue;

                    
                    /*Wait until process exits, in order to try to close automatically related  UpdateTrace window*/
                    utw.Process.Exited += delegate(object sender, System.EventArgs e)
                    {
                        try
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                /*Try forse the closing of the update window, only if there is no any
                                 conflict detected during update*/
                                if (utw.ConflictDetected == false)
                                {
                                    utw.Close();
                                    GC.SuppressFinalize(utw);
                                    utw = null;
                                }

                            }));

                        }
                        catch
                        {
                        }

                       
                    };




                    /*Waiting indefinitely untill the process completes it job*/
                    utw.WaitUpdateEnd();                      
                   
                }

                /*Worker execution competed so execute som GC stuff*/
                _worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
                {
                    Dispatcher.Invoke(new Action(() =>
                            {
                                GC.SuppressFinalize(friArray);
                                friArray = null;

                                GC.Collect();
                                GC.WaitForPendingFinalizers();

                            }
                            )
                    );
                };


                /*Reset global boolean value*/
                bRepositorySequenceUpdating = false;
            };



            _worker.RunWorkerAsync();




        }


        /// <summary>
        /// Executes updates specified repository .
        /// </summary>
        /// <param name="repositoryPath">Repository complete working copy path. If Null or Empty string is passed, the 
        /// current selected repository will be updated, if there is any.</param>
        private UpdateTraceWindow UpdateRepository(string repositoryPath)
        {


            string repoName = System.IO.Path.GetFileNameWithoutExtension(repositoryPath);


            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            string selRepoCompletePath = string.Empty;


            if (!string.IsNullOrEmpty(repositoryPath))
            {
                selRepoCompletePath = repositoryPath;
                if (string.IsNullOrEmpty(selRepoCompletePath))
                    return null;

            }
            else if (selRepo != null)
            {
                selRepoCompletePath = selRepo.RepositoryCompletePath;
            }
            else
                return null;



            if (string.IsNullOrEmpty(selRepoCompletePath))
                return null;

            if (!System.IO.Directory.Exists(selRepoCompletePath))
                return null;


            /*If there is already Update executing over specified repository do not call again, as the folder is alreday locked by Subversion.*/
            if (!string.IsNullOrEmpty(selRepoCompletePath))
            {

                RepositoryProcess availableRepoProcess = RadarExecutor.IsProcesStillAvailable(selRepoCompletePath);
                if (availableRepoProcess != null)
                {
                    if (CommandStringsManager.IsUpdateCommand(availableRepoProcess.Command))
                    {
                        string msg = FindResource("MSG_MULTIPLEUPDATECOMMANDS") as string;
                        if (!string.IsNullOrEmpty(msg))
                            MessageBox.Show(msg, App.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        return null;
                    }
                }
            }


            try
            {
                System.IO.Directory.SetCurrentDirectory(selRepoCompletePath);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
                return null;
            }


            //Creating trace window
            UpdateTraceWindow utw = new UpdateTraceWindow();
            if (selRepo != null)
                utw.Title += "    " + selRepoCompletePath;
            else if (!string.IsNullOrEmpty(selRepoCompletePath))
            {
                utw.Title += "    " + selRepoCompletePath;
            }
            else
                return null;
            utw.Topmost = true;


            /*Assign to the executor current folder repo info, to pass it to the executor process in the future*/
            SvnRadarExecutor.currentUpdateTraceWindow = utw;

            /* Register window in the window manager */
            WindowsManager.AddNewWindow(utw);
            RadarExecutor.UpdateRepository(selRepoCompletePath, false);

            /*Initialize properties of the window */
            utw.Process = SvnRadarExecutor.LastExecutedProcess;
            utw.RelatedRepositoryName = SvnRadarExecutor.LastExecutedProcess.RelatedRepositoryName;
            utw.RelatedCommand = SvnRadarExecutor.LastExecutedProcess.Command;


            utw.Show();
            return utw;

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

            return RadarExecutor.GetFolderRepoInfo(folderPath, isCallForSysTray);

        }


        private void OnShowRevisionInfoCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            if (selRepo == null)
                return;

            string selRepoString = selRepo.RepositoryCompletePath;
            if (string.IsNullOrEmpty(selRepoString))
                return;


            RepoInfo repositoryInfo = RepoInfo.SelectedInfo;
            if (repositoryInfo == null)
                return;

            /*If Batch file doesn't exist but the merge path defined , try to recreate batch file*/
            if (!RepoBrowserConfiguration.Instance.IsBatchFileExists && RepoBrowserConfiguration.Instance.IsWinMergeDefined)
            {
                GenerateBatchFile();

                /*If Batch file doesn't exist, means there is somethign wrong with the batch file generation so remove also WinMerge 
                 * definition from  the application configuration, and application will make diff via built in future*/
                if (!RepoBrowserConfiguration.Instance.IsBatchFileExists)
                {
                    /*Reset WinMerge path to emtpy*/
                    RepoBrowserConfiguration.Instance.WinMergePath = string.Empty;


                    /*  Show warning message about it...*/
                    string message = FindResource("BatchFileGenerationProblem") as string;
                    if (!string.IsNullOrEmpty(message))
                    {
                        MessageBox.Show(message, Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
            }


            /*If the mege path doesn't defined or doesn't exist*/
            if (!string.IsNullOrEmpty(RepoBrowserConfiguration.Instance.WinMergePath) && !System.IO.File.Exists(RepoBrowserConfiguration.Instance.WinMergePath))
            {
                /*  Show warning message about it...*/
                string message = FindResource("DiffToolPathProblem") as string;
                if (!string.IsNullOrEmpty(message))
                {

                    MessageBox.Show(message, Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }


                /*Reset WinMerge path to emtpy*/
                RepoBrowserConfiguration.Instance.WinMergePath = string.Empty;

            }

            /*If there is no any batch file in the system or any diff tol defined, manage in built in way*/
            if (!RepoBrowserConfiguration.Instance.IsBatchFileExists || !RepoBrowserConfiguration.Instance.IsWinMergeDefined)
            {
                /*Add revison object to the base, in order to populate it from the
                 commands output in the future. The strong key, in this case, is the Revision number*/
                RevisionInfo revi = RepoInfoBase.AddRevisionInfoString(selRepo.RepositoryCompletePath, repositoryInfo.Revision, repositoryInfo.Item, repositoryInfo.Date, string.Empty);
                RevisionInfoWindow reviwWnd = new RevisionInfoWindow(revi);
                reviwWnd.Topmost = true;
                reviwWnd.RelatedRepositoryName = selRepo.RepositoryCompletePath;

                bool bSuccess = svnRadarExecutor.GetRevisionInfo(selRepoString, repositoryInfo.Revision, repositoryInfo.Item, selRepo.FolderRepoInformation, false);
                if (bSuccess)
                {
                    WindowsManager.AddNewWindow(reviwWnd);

                    reviwWnd.Process = SvnRadarExecutor.LastExecutedProcess;
                    reviwWnd.RelatedCommand = SvnRadarExecutor.LastExecutedProcess.Command;
                    reviwWnd.Show();
                }
                else
                {
                    /* Something went wrong in the execution, so do not open window and clear the data*/
                    RepoInfoBase.RemoveRevisonInfoStringFromBase(selRepo.RepositoryCompletePath, repositoryInfo.Revision);
                    revi = null;
                    reviwWnd = null;
                }
            }
            else
            {
                /* Call batch file that will execute external Diff tool*/
                svnRadarExecutor.ShowDiffWithExternalDiffProgram(selRepo.RepositoryCompletePath, repositoryInfo.Revision, repositoryInfo.Item, selRepo.FolderRepoInformation);
            }

        }

        private void OnRemoveFilterFromColumnCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RepoTabItem selRepo = mainTab.SelectedItem as RepoTabItem;
            if (selRepo == null)
                return;

            System.Windows.Controls.GridViewColumnHeader gvch = e.Parameter as System.Windows.Controls.GridViewColumnHeader;


            string colName = gvch.Column.Header.ToString();
            if (string.IsNullOrEmpty(colName))
                return;

            /*remove filter from collection  */
            FilterManager.RemoveFilter(selRepo.RepositoryCompletePath, colName);


            gvch.Column.UpdateColumnHeaderBindings();

            /*update data provider*/
            UpdateObjectDataProvider();

            


        }

        List<GridViewColumnHeader> columnsWithFilters = new List<GridViewColumnHeader>();

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
            if (FilterManager.AddFilterToColumn(selRepo.RepositoryCompletePath, gvCh.Column.Header.ToString(), string.Empty))
            {
                gvCh.Column.UpdateColumnHeaderBindings();
                columnsWithFilters.Add(gvCh);
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

            FilterManager.UpdateFilterOnColumn(selRepo.RepositoryCompletePath, colName, tb.Text);


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
        /// Setups Group view UI on ListView
        /// </summary>
        private void SetUpGroupByRevision()
        {
            RepoBrowserConfiguration.Instance.ViewLayout = RepoBrowserConfiguration.ListViewLayoutEnum.RevisionView;
            CollectionViewSource colViewSource = FindResource("source") as CollectionViewSource;
            if (colViewSource == null)
                return;

            colViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Revision"));

        }


        /// <summary>
        /// Handles request on group available items list into groups by RevisionNumber
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnGroupByRevisionNumberCommand(object sender, ExecutedRoutedEventArgs args)
        {
            SetUpGroupByRevision();
        }




        /// <summary>
        /// Setups FlatView on UI
        /// </summary>
        private void SetUpFlatView()
        {
            RepoBrowserConfiguration.Instance.ViewLayout = RepoBrowserConfiguration.ListViewLayoutEnum.FlatView;
            CollectionViewSource colViewSource = FindResource("source") as CollectionViewSource;
            if (colViewSource == null)
                return;

            colViewSource.GroupDescriptions.Clear();
        }

        /// <summary>
        /// Handles request on visualize items in flat view  mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnFlatViewCommand(object sender, ExecutedRoutedEventArgs args)
        {
            SetUpFlatView();
        }






        /// <summary>
        /// Hanldes request on break the repository changes request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnBreakLogLoadingCommand(object sender, ExecutedRoutedEventArgs args)
        {
            RepoTabItem repoTabItem = RepoBrowserWindow.SelectedRepoTabItem;
            if (repoTabItem == null)
                return;


            RepositoryProcess repoProcess = svnRadarExecutor.IsProcesStillAvailable(repoTabItem.RepositoryCompletePath);


            try
            {
                repoProcess.Kill();
                repoProcess.Dispose();
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
            }
        }








    }
}
