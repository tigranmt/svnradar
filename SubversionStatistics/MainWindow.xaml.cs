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
using System.Data.SQLite;
using SubversionStatistics.Stat;
using ViewModel;
using SubversionStatistics.Util;
using System.Collections.ObjectModel;
using SubversionStatistics.Exceptions;
using System.ComponentModel;

namespace SubversionStatistics
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Object of the SubversionTreeViewAsyncLoader responsable for the asynchronious loading of the tree
        /// </summary>
        public static SubversionTreeViewAsyncLoader asyncTreeLoader = new SubversionTreeViewAsyncLoader(Configuration.Configuration.SubversionExeCompletePath);

        /// <summary>
        /// Holds the specified repository tree structure
        /// </summary>
        public static ObservableCollection<TreeItemViewModel> repositoryTree = new ObservableCollection<TreeItemViewModel>();

        /// <summary>
        /// SvnStat objects list
        /// </summary>
        List<SvnStat> statistics = new List<SvnStat>();

        /// <summary>
        /// Ctor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }



        /// <summary>
        /// Gets the tree view of the repository
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<TreeItemViewModel> GetRepositoryTree()
        {
            return repositoryTree;
        }



        public static object GetStatResult()
        {
            return null;
        }

        /// <summary>
        /// Begins loading of statistical data for the specified object in DataBase
        /// </summary>
        /// <param name="stat">SvnStat object</param>
        private void BeginLoadingStatData(SvnStat stat)
        {
            if (stat == null)
                throw new ArgumentNullException("The stat object's reference is null");
            if (string.IsNullOrEmpty(stat.RepositoryPath))
                throw new InvalidWorkingCopyPath(stat.RepositoryPath);

            /*Run tree loading on separated thread*/
            BackgroundWorker _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            _worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                stat.BeginPopulateDatabaseFromSubversionRepository();
            };

            // On work progress changed
            _worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                // some progress handling stuff
            };


            _worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {

            };

            _worker.RunWorkerAsync();

        }

        /// <summary>
        /// Begin loading of the tree
        /// </summary>
        /// <param name="rootPath">The path to the root of the repository that will become the root of the tree</param>
        private void BeginLoadingTree(string rootPath)
        {
            this.Title += "     " + rootPath;

            TreeItemViewModel rootNode = new TreeItemViewModel { Name = rootPath, TreeItemType = TreeItemViewModel.TreeItemTypeEnum.Root, RepositoryPath = rootPath };
            repositoryTree.Add(rootNode);
            asyncTreeLoader.StartLoadChilds(rootNode);
        }



        private void btnReload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelLoad_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Handles load button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {

            string workingCopyPath = txtRepositoryPath.Text;

            if (string.IsNullOrEmpty(workingCopyPath) ||
                !System.IO.Directory.Exists(workingCopyPath))
            {
                new InvalidWorkingCopyPath(workingCopyPath);
            }

            //Retrieve url from the repository
            string url = SvnStat.GetFolderRepoUrl(workingCopyPath);

            //Load tree of the repository in separate thread
            BeginLoadingTree(url);
            RepoTreeView.DataContext = repositoryTree;

            //Delete stat info if it exists/
            SvnStat found = statistics.Find((x) => x.RepositoryPath.Equals(txtRepositoryPath.Text));
            if (found != null)
            {
                //Unsubscribe from the object's events
                UnSubscribeToEvents(found);
                statistics.Remove(found);
            }

            //Load db statistical data in separate thread
            SvnStat stat = new SvnStat(txtRepositoryPath.Text);

            //Subscribe to events
            SubscribeToEvents(stat);

            BeginLoadingStatData(stat);

        }

        /// <summary>
        /// Subscribe to events of the SvnStat object
        /// </summary>
        /// <param name="stat"></param>
        void SubscribeToEvents(SvnStat stat)
        {
            if (stat == null)
                throw new ArgumentNullException("The stat object's refernce is Null ");

            stat.BeginGettingChangedLinesCountEvent += new BeginGettingChangedLinesCount(stat_BeginGettingChangedLinesCountEvent);
            stat.BeginLoadingBaseEvent += new BeginLoadingBase(stat_BeginLoadingBaseEvent);
            stat.BeginProcessingEntryEvent += new BeginProcessingEntry(stat_BeginProcessingEntryEvent);
            stat.BeginProcessingFileEntryEvent += new BeginProcessingFileEntry(stat_BeginProcessingFileEntryEvent);
            stat.EndGettingChangedLinesCountEvent += new EndGettingChangedLinesCount(stat_EndGettingChangedLinesCountEvent);
            stat.EndLoadingBaseEvent += new EndLoadingBase(stat_EndLoadingBaseEvent);
            stat.EndProcessingEntryEvent += new EndProcessingEntry(stat_EndProcessingEntryEvent);
            stat.EndProcessingFileEntryEvent += new EndProcessingFileEntry(stat_EndProcessingFileEntryEvent);
            stat.ErrorRecievedEvent += new ErrorRecieved(stat_ErrorRecievedEvent);

        }


        /// <summary>
        /// Unsubscribe to events of the SvnStat object
        /// </summary>
        /// <param name="stat"></param>
        void UnSubscribeToEvents(SvnStat stat)
        {
            if (stat == null)
                throw new ArgumentNullException("The stat object's refernce is Null ");

            stat.BeginGettingChangedLinesCountEvent -= new BeginGettingChangedLinesCount(stat_BeginGettingChangedLinesCountEvent);
            stat.BeginLoadingBaseEvent -= new BeginLoadingBase(stat_BeginLoadingBaseEvent);
            stat.BeginProcessingEntryEvent -= new BeginProcessingEntry(stat_BeginProcessingEntryEvent);
            stat.BeginProcessingFileEntryEvent -= new BeginProcessingFileEntry(stat_BeginProcessingFileEntryEvent);
            stat.EndGettingChangedLinesCountEvent -= new EndGettingChangedLinesCount(stat_EndGettingChangedLinesCountEvent);
            stat.EndLoadingBaseEvent -= new EndLoadingBase(stat_EndLoadingBaseEvent);
            stat.EndProcessingEntryEvent -= new EndProcessingEntry(stat_EndProcessingEntryEvent);
            stat.EndProcessingFileEntryEvent -= new EndProcessingFileEntry(stat_EndProcessingFileEntryEvent);
            stat.ErrorRecievedEvent -= new ErrorRecieved(stat_ErrorRecievedEvent);

        }



        #region Statistical object eevnts handlers
        void stat_BeginGettingChangedLinesCountEvent(object sender, int revisonNumber, string fileName)
        {

        }

        /// <summary>
        /// Handles begin loading DB data event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void stat_BeginLoadingBaseEvent(object sender, EventArgs args)
        {
            //Show adorner in order to notify to the user about possibly long term operation
            Dispatcher.Invoke(new Action(() =>
            {
                Grid _gr = FindName("StatGrid") as Grid;
                AdornerManager.AddAdorner(_gr, new StatDataLoadingAdorner(_gr));

            }));


        }


        void stat_ErrorRecievedEvent(object sender, ProcessErrorEventArgs errArgs)
        {

        }

        void stat_EndProcessingFileEntryEvent(object sender, FileEntry fe)
        {

        }

        void stat_EndProcessingEntryEvent(object sender, LogEntry processedEntry)
        {

        }

        void stat_EndLoadingBaseEvent(object sender, EventArgs args)
        {
            //Loading of the data is finished so user can begin to view data, so remove adorner from the screen
            Dispatcher.Invoke(new Action(() =>
            {
                AdornerManager.RemoveAdorner();
            }));
        }

        void stat_EndGettingChangedLinesCountEvent(object sender, int revisionNumber, string fileName)
        {

        }

        void stat_BeginProcessingFileEntryEvent(object sender, FileEntry fe)
        {

        }

        void stat_BeginProcessingEntryEvent(object sender, LogEntry processingEntry)
        {

        }



        #endregion



    }
}
