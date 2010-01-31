using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SubversionStatistics;
using SubversionStatistics.Util;


namespace ViewModel
{
   
    

    /// <summary>
    /// View model for the TreeView file system
    /// </summary>
    public class TreeItemViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The dummy object to add to the folder node to notify to tree view that specified element could have potential children.
        /// </summary>
        public static TreeItemViewModel DummyChild = new TreeItemViewModel { Name = "Dummy", TreeItemType = TreeItemTypeEnum.File };

        bool bIsSelected = false;
        bool bIsExpanded = false;
        bool bIsLoadingCanceled = false;
        bool bIsLoading = false;

        /// <summary>
        /// Enumeration that describes speified entity type
        /// </summary>
        public enum TreeItemTypeEnum { File, Folder, Root };

        /// <summary>
        /// Describes specified entity type
        /// </summary>
        public TreeItemTypeEnum TreeItemType { get; set; }

        /// <summary>
        /// Item's repository path
        /// </summary>
        public string RepositoryPath { get; set; }


        /// <summary>
        /// Children items collection
        /// </summary>
        ObservableCollection<TreeItemViewModel> children = null;

        /// <summary>
        /// Revision number
        /// </summary>
        public int Revision { get; set; }


        /// <summary>
        /// Date 
        /// </summary>
        public DateTime Date { get; set; }


        /// <summary>
        /// Accoun name 
        /// </summary>
        public string AccountName { get; set; }


        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Defines if the node is selected
        /// </summary>
        public bool IsSelected
        {
            get { return bIsSelected; }
            set
            {
                bIsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }



        /// <summary>
        /// Defines if the node is expanded
        /// </summary>
        public bool IsExpanded
        {
            get { return bIsExpanded; }
            set
            {
                bIsExpanded = value;
                OnPropertyChanged("IsExpanded");

                if (value == true)
                {
                    if (this.Children.Count == 1 && this.Children[0].Equals(TreeItemViewModel.DummyChild))
                    {
                        this.Children.Clear();                        
                        MainWindow.asyncTreeLoader.StartLoadChilds(this);
                    }
                }
                
            }
        }


        /// <summary>
        /// Determines if the loading of the child nodes was canceled by user
        /// </summary>
        public bool IsLoadingCanceled
        {
            get { return bIsLoadingCanceled; }
            set
            {
                bIsLoadingCanceled = value;
                OnPropertyChanged("IsLoadingCanceled");
            }
        }


          /// <summary>
        /// Determines if the loading of the child is still processing
        /// </summary>
        public bool IsLoading
        {
            get { return bIsLoading; }
            set
            {
                bIsLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }



        

        /// <summary>
        /// Children nodes collection
        /// </summary>
        public ObservableCollection<TreeItemViewModel> Children 
        {

            get
            {
                if (children == null)
                    children = new ObservableCollection<TreeItemViewModel>();
                return children;
            }
          
        }
       



        

        #region INotifyPropertyChanged Members


        /// <summary>
        /// OnPropertyChanged for raising INotifyPropertyChanged inetrface notification
        /// </summary>
        /// <param name="name"></param>
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
