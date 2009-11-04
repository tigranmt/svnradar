using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SvnRadar.Util;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using SvnRadar.Common.Controls;
using System.ComponentModel;

namespace SvnRadar
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class RepoBrowserWindow : Window
    {
        /// <summary>
        /// Holds the current slecetd tab item
        /// </summary>
        static RepoTabItem selectedRepoTabItem = null;

        /// <summary>
        /// Timer object taht will controls the all available repositories
        /// </summary>
        static Timer timer = new Timer();


        /// <summary>
        /// Application state enumeration
        /// </summary>
        public enum ApplicationStateEnum { NotifyIcon, MainWindowVisible }

        /// <summary>
        /// Defines application state
        /// </summary>
        public static ApplicationStateEnum ApplicationState = ApplicationStateEnum.NotifyIcon;

        /// <summary>
        /// Current repository name checked by timer 
        /// </summary>
        static string currentRepositoryCheckedByTimer = string.Empty;

        /// <summary>
        /// Background worker for managing background thread
        /// </summary>
        static BackgroundWorker _worker = new BackgroundWorker();


        /// <summary>
        /// Notifies the prgram about netowk status state
        /// </summary>
        static bool bNetworksAvailable = false;


        /// <summary>
        /// Static property to retrieve the current selected RepoTabItem in application
        /// </summary>
        public static RepoTabItem SelectedRepoTabItem
        {
            get { return selectedRepoTabItem; }
        }

        //ctor
        public RepoBrowserWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Shows the open file dialog to find the Subversion exe path
        /// </summary>
        internal void SetupSvnPath()
        {
            System.Windows.Forms.OpenFileDialog opfDlg = new System.Windows.Forms.OpenFileDialog();
            opfDlg.Filter = "*Exe files (*.exe)|*.exe";
            opfDlg.ValidateNames = true;

            if (opfDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtSubversionPath.Text = opfDlg.FileName;
                RepoBrowserConfiguration.Instance.SubversionPath = txtSubversionPath.Text;
            }
        }


        /// <summary>
        /// Reset the configuration to it's default values
        /// </summary>
        internal void ResetConfiguration()
        {
            RepoBrowserConfiguration.Instance.ResetConfiguration();
        }

        /// <summary>
        /// Save curent configuration to file
        /// </summary>
        internal void SaveConfiguration()
        {
            RepoBrowserConfiguration.Instance.Save();
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //Loading configuration saved, if there is any
            RepoBrowserConfiguration.Instance.Load();

            /*Set up notify icon object into it's manager*/
            TaskNotifierManager.SetTaskBarIconObject(MainNotifyIcon);


            if (TaskNotifierManager.Balloon == null)
                TaskNotifierManager.InitBalloon();
            /* Subscribing to balloon events */
            TaskNotifierManager.Balloon.RepositoryChangesViewRequested += new FancyBalloon.RepositoryRoutedEventHandler(Balloon_RepositoryChangesViewRequested);
            TaskNotifierManager.Balloon.RepositoryUpdateRequested += new FancyBalloon.RepositoryRoutedEventHandler(Balloon_RepositoryUpdateRequested);


            if (RepoBrowserConfiguration.Instance.ViewLayout == RepoBrowserConfiguration.ListViewLayoutEnum.RevisionView)
                SetUpGroupByRevision();

            /*Start timer*/
            StartTimer();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            //Setup the control Data binding
            SetupBindings();

            /*Set UI*/
            SetupUI();


            /*Hide the main window after that it becomes rendered and databinding was setupped*/
            this.Visibility = Visibility.Hidden;

        }



        /// <summary>
        /// Starts the repository control timer
        /// </summary>
        void StartTimer()
        {
            timer.Interval = RepoBrowserConfiguration.Instance.ControlRate * 10000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }


        /// <summary>
        /// Notifies about the changes in specified repository on system tray
        /// </summary>
        /// <param name="folderRepoInfo">FolderRepoInfo object that contains change information</param>
        void NotifyChanges(FolderRepoInfo folderRepoInfo)
        {
            TaskNotifierManager.AddToNitificationList(folderRepoInfo);
            TaskNotifierManager.SignalChangesOnSysTray(true);
        }


        /// <summary>
        /// Checks for network status
        /// </summary>
        /// <returns></returns>
        bool IsNetworkAvailable()
        {
            bNetworksAvailable = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            return bNetworksAvailable;
        }

        /// <summary>
        /// Control all available repositories in sequence
        /// </summary>
        void ControlRepositorySequence()
        {
            if (_worker != null && _worker.IsBusy)
                return;



            /*If for some reason RepoBrowserConfiguration.Instance.SubversionPath is emtpy, notify error and return */
            if (string.IsNullOrEmpty(RepoBrowserConfiguration.Instance.SubversionPath))
            {
                ErrorManager.ShowCommonError("The subversion exe path is missed. Can not execute command", true);
                return;
            }

            /* Begin new therad here in order to not block the UI */
            /* Run the process output listener in background */

            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                /*Fil an array of object , becaus until the loop checks for repository user can delete some of them*/
                string[] paths = RepoBrowserConfiguration.Instance.RepositoryPaths.ToArray<string>();

                List<string> upToDateRepositories = new List<string>();
                bool needUpdate = false;

                /*On every tick check all available repositories*/
                foreach (string repositoryPath in paths)
                {

                    /*May be the directory is on remvable device, or has been deleted time ago,
                     so we need to notify about that to the user and help to make a choice*/
                    if (!System.IO.Directory.Exists(repositoryPath))
                    {
                        string messageString = FindResource("MSG_SELECTED_PATH_NOLONGER_EXISTS") as string;
                        if (!string.IsNullOrEmpty(messageString))
                        {
                            messageString = string.Format(messageString, repositoryPath);
                            if (ErrorManager.ShowCommonErrorYesNo(messageString, true) == MessageBoxResult.Yes)
                            {
                                /*If yes , means that user request that we delete autoamtically the lost path from the 
                                 configuration list*/
                                lock (RepoBrowserConfiguration.Instance.RepositoryPaths)
                                {
                                    if (!upToDateRepositories.Contains(repositoryPath))
                                        upToDateRepositories.Add(repositoryPath);

                                    Dispatcher.Invoke(new Action(() =>
                                    {
                                        RepoBrowserConfiguration.Instance.RepositoryPaths.Remove(repositoryPath);
                                    }));
                                }
                            }
                        }

                        continue;
                    }

                    /*Verify if the repository is up to date*/
                    FolderRepoInfo repoInfo = RadarExecutor.IsRepositoryUpTodate(repositoryPath);

                    if (repoInfo != null)
                    {
                        /*If yes, notify about it on view*/
                        needUpdate = true;

                        Dispatcher.Invoke(new Action(() =>
                        {
                            NotifyChanges(repoInfo);
                        }));
                    }
                    else if (!upToDateRepositories.Contains(repositoryPath))
                        upToDateRepositories.Add(repositoryPath);


                }//foreach


                if (!needUpdate)
                    TaskNotifierManager.SignalChangesOnSysTray(false);
                else
                {
                    TaskNotifierManager.UpToDateRepositories(upToDateRepositories);
                }

            };

            // On work progress changed
            _worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                // some progress handling stuff
            };

            //On work completed
            _worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {

            };

            _worker.RunWorkerAsync();



        }


        /// <summary>
        /// timer tick handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e)
        {
            if (RepoBrowserConfiguration.Instance.RepositoryPaths == null ||
                RepoBrowserConfiguration.Instance.RepositoryPaths.Count == 0)
                return;

            /*Verifies the network status*/
            if (bNetworksAvailable)
            {
                if (!IsNetworkAvailable())
                {
                    string errMsg = FindResource("MSG_NETWORKSTATUS_PROBLEM") as string;
                    ErrorManager.ShowCommonError(errMsg, true);
                    return;
                }
            }

            bNetworksAvailable = true;
            ControlRepositorySequence();

        }




        /// <summary>
        /// Setupe UI on the user view
        /// </summary>
        private void SetupUI()
        {
            /*Removes all tabs skipping the first one, which is cofiguration tab */
            while (mainTab.Items.Count > 1)
                mainTab.Items.RemoveAt(1);

            if (RepoBrowserConfiguration.Instance.RepositoryPaths != null &&
                RepoBrowserConfiguration.Instance.RepositoryPaths.Count > 0)
            {
                foreach (string repoPath in RepoBrowserConfiguration.Instance.RepositoryPaths)
                {
                    AddTabFromPath(repoPath);
                }
            }
        }


        /// <summary>
        /// Setup all neccessary bindings
        /// </summary>
        private void SetupBindings()
        {
            /*Subscribe to repositories list change event*/
            RepoBrowserConfiguration.Instance.RepositoryPaths.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(RepositoryPaths_CollectionChanged);

            /*sign data context of the UI cotrols*/
            txtSubversionPath.DataContext = RepoBrowserConfiguration.Instance;
            frequencySlider.DataContext = RepoBrowserConfiguration.Instance;
            lbSvnPaths.DataContext = RepoBrowserConfiguration.Instance;

            /*sign data context of the Menu*/
            ApplicationMenu.DataContext = RepoBrowserConfiguration.Instance;


        }


        /// <summary>
        /// Add the new Tab control for given SVN repository path
        /// </summary>
        /// <param name="path">SVN repository path</param>
        private void AddTabFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            /*Add repository tab to main tab's collection*/
            mainTab.Items.Add(new RepoTabItem(path));
        }



        /// <summary>
        /// Removes tab item that refers to given repository 
        /// </summary>
        /// <param name="path">SVN repository path</param>
        void RemoveTabByPath(string path)
        {


            if (string.IsNullOrEmpty(path))
                return;

            string repoName = string.Empty;
            try
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(path);
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

            RepoTabItem repoTabToRemove = null;

            foreach (TabItem repo in mainTab.Items)
            {
                RepoTabItem repoTab = repo as RepoTabItem;
                if (repoTab == null)
                    continue;
                if (repoTab.RepositoryName.Equals(repoName, StringComparison.InvariantCultureIgnoreCase))
                {
                    repoTabToRemove = repoTab;
                    break;
                }
            }

            if (repoTabToRemove != null)
                mainTab.Items.Remove(repoTabToRemove);
        }


        /// <summary>
        /// Add new repository path to the collection of available ones
        /// </summary>
        private void AddNewSvnPath()
        {

            FolderBrowserDialog obd = new FolderBrowserDialog();
            System.Windows.Forms.DialogResult res = obd.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                RepoBrowserConfiguration.Instance.AddRepoPath(obd.SelectedPath);
            }

        }


        /// <summary>
        /// Remove given repository path from the collection of available ones
        /// </summary>
        /// <param name="path">Repositiory path</param>
        private void RemoveSvnPath(string path)
        {
            string warningMessage =
                FindResource("MSG_WARNING_DELETE_REPOPATH") as string;

            if (System.Windows.MessageBox.Show(warningMessage,
                 this.Title,
                 MessageBoxButton.YesNo,
                 MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                RepoBrowserConfiguration.Instance.RemoveRepoPath(path);
            }
        }



        /// <summary>
        /// Attempts to restore the state of the main window
        /// </summary>
        private void ShowMe()
        {
            this.WindowState = WindowState.Normal;
            this.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Attempts to hide the main window
        /// </summary>
        private void HideMe()
        {
            this.WindowState = WindowState.Maximized;
            this.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// Hanldes Bug signal report 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BugSignal_Click(object sender, RoutedEventArgs e)
        {
            /* show Bug report window */
            new BugReportWindow().ShowDialog();
        }

        /// <summary>
        /// handles drop of the folder(s) on the listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSvnPaths_Drop(object sender, System.Windows.DragEventArgs e)
        {
            System.Windows.DataObject dobj = e.Data as System.Windows.DataObject;
            if (dobj != null)
            {
                /* Get the collection of files, if there is*/
                System.Collections.Specialized.StringCollection folderCollection = dobj.GetFileDropList();
                if (folderCollection != null && folderCollection.Count > 0)
                {
                    try
                    {
                        foreach (string folderPath in folderCollection)
                        {
                            /*If IO obect is a folder*/
                            if (System.IO.Directory.Exists(folderPath))
                            {
                                /*Add to the configuration*/
                                RepoBrowserConfiguration.Instance.AddRepoPath(folderPath);
                            }
                        }
                    }
                    catch (System.IO.IOException ioExc)
                    {
                        ErrorManager.ShowExceptionError(ioExc, true);
                    }
                }
            }
        }





    }
}
