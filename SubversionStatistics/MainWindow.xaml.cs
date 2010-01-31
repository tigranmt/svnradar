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
        /// Ctor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();    
       
            
        }




        public static ObservableCollection<TreeItemViewModel> GetRepositoryTree()
        {
            return repositoryTree;
        }


        public static object GetStatResult()
        {
            return null;
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

        private void RepoTreeView_Expanded(object sender, RoutedEventArgs e)
        {
            //TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            //if (tvi == null)
            //    return;
            //if(tv
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelLoad_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            BeginLoadingTree("http://src.chromium.org/svn/trunk/src/net");
            RepoTreeView.DataContext = repositoryTree;
        }




    }
}
