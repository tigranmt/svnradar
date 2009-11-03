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
using System.Windows.Shapes;
using SvnRadar.Util;
using System.ComponentModel;
using SvnRadar.Common.Intefaces;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace SvnRadar
{
    /// <summary>
    /// Interaction logic for RevisionInfoWindow.xaml
    /// </summary>
    public partial class RevisionInfoWindow : Window, IRepoWindow
    {

     
        private RevisionInfo revisionInformation = null;

        private const int MAXIMUM_ALOWED_COUNT = 100;
        private static ObservableCollection<object> texts = new ObservableCollection<object>();

        
     

        /// <summary>
        /// Holds the revision object on which the information must be shown
        /// </summary>
        public RevisionInfo RevisionInformation
        {
            get { return revisionInformation; }
            set
            {
                revisionInformation = value;

                if (value != null)
                {
                    this.Title = revisionInformation.Revision.ToString() + " : " + revisionInformation.Item;
                    this.DataContext = value;    
                    
                }
            }
        }


        /// <summary>
        /// Initialize the new instance of the TextBox
        /// </summary>
        void InitTextBox()
        {         
          
            ChangesView.Cursor = Cursors.Wait;
            string statusBarString = FindResource("GETCHANGELOGSTATUSBARMSG") as string;
            if (!string.IsNullOrEmpty(statusBarString))
                TextOnStatusBar.Text = new string(' ', 2) + statusBarString + new string(' ', 2);
            progressBar.Visibility = Visibility.Visible;
             

        }



      

        public RevisionInfoWindow()
        {
            InitializeComponent();
            
            InitTextBox();         

        }


        public RevisionInfoWindow(RevisionInfo rInfo)
            : this()
        {
            RevisionInformation = rInfo;

            ///*Subscribe to the property chnage notification, as it's imposible to DataBind the Document of the RichText box
            // So we will recieve notification about a change in the property in the mothod and update rich text box content*/
            revisionInformation.PropertyChanged += new PropertyChangedEventHandler(revisionInformation_PropertyChanged);
        }

        /// <summary>
        /// Handles revison info arbitrary property change event notification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void revisionInformation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(new  Action(()=>
            {
                TextBlock block = RevisionInformation.ParseMadeChangesToBlock();
                if (block != null)
                {

                    ChangesView.Inlines.Add(block);
                    ChangesView.Inlines.Add(new LineBreak());
                    
                    //texts.Add(block);
                    //texts.Add(new LineBreak());
                }
            }));
        }



        #region IRepoWindow Members
        /*Repository process responsable for operation handled by the window*/
        RepositoryProcess repoProcess = null;
        public RepositoryProcess Process
        {
            get
            {
                return repoProcess;
            }
            set
            {
                //Set process only once per window
                if (repoProcess == null)
                {                  

                    repoProcess = value;     
                 
                }
            }
        }

        /// <summary>
        /// Related repository name
        /// </summary>
        public string RelatedRepositoryName { get; set; }


        /// <summary>
        /// Related command executed over repository
        /// </summary>
        public string RelatedCommand { get; set; }


        /// <summary>
        /// Notifies window about relatve process exit
        /// </summary>
        public void ProcessExited()
        {
            SetDefaultState();
           
        }

        #endregion

        #region process events handlers

        void SetDefaultState()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                ChangesView.Cursor = Cursors.Arrow;
                TextOnStatusBar.Text = "";
                progressBar.Visibility = Visibility.Hidden;
               // txtSearch.Visibility = Visibility.Visible;
            }));
        }

       
      

        #endregion

        

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.Process != null)
            {
                //try to kill process if it still alive
                try
                {
                    if (!this.Process.HasExited)
                    {
                        this.Process.CancelOutputRead();
                        this.Process.Kill();
                        this.Process.Dispose();
                    }

                    
                }
                catch (Exception ex)
                {
                    //TODO: Log an exception, do not raise anythig here. It's not critical point.
                }


                /*Delete myself from the available windows collection */
                WindowsManager.RemoveWindow(this);

                /*Remove my information from the shared base, to not waste the memory*/
                if(this.Process != null &&  this.RevisionInformation != null) 
                {
                    SvnRadar.DataBase.RepoInfoBase.RemoveRevisonInfoStringFromBase(this.RelatedRepositoryName, this.RevisionInformation.Revision);
                }


                /*Destroying object event handler*/
                revisionInformation.PropertyChanged -= new PropertyChangedEventHandler(revisionInformation_PropertyChanged);
                ChangesView.Inlines.Clear();



                GC.SuppressFinalize(true);


            }
        }


        /// <summary>
        /// Returns the list of available TextBlocks
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <returns></returns>
        public static ObservableCollection<Object> GetTextDataList()
        {
            return texts;
        }


    }
}
