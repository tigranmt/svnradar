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
using RepoManager.Util;
using System.Collections.ObjectModel;
using RepoManager.DataBase;
using RepoManager.Common.Intefaces;

namespace RepoManager
{

    /// <summary>
    /// Interaction logic for UpdateTraceWindow.xaml
    /// </summary>
    public partial class UpdateTraceWindow : Window, IRepoWindow
    {
        /// <summary>
        /// Observable collection for holding  the update trace info data. Binded to the window's ListView control.
        /// </summary>
        ObservableCollection<UpdateTraceRow> updateTrace = new ObservableCollection<UpdateTraceRow>();

        //Ctor
        public UpdateTraceWindow()
        {
            InitializeComponent();


            InitUI();
            
        }


        private void InitUI()
        {

            string statusBarString = FindResource("UPDATINGREPOSITORYSTATUSBARMSG") as string;
            if (!string.IsNullOrEmpty(statusBarString))
                TextOnStatusBar.Text = new string(' ', 2) + statusBarString + new string(' ', 2);
            progressBar.Visibility = Visibility.Visible;

        }



        /// <summary>
        /// Adds the given string to the ListView.
        /// </summary>
        /// <param name="updateString"></param>
        public void AddString(string updateString)
        {
             


            if (string.IsNullOrEmpty(updateString))
                return;            

            /*First recover the state of the item */
            int spaceIndex = updateString.IndexOf(' ');
            if (spaceIndex <= 0)
                return;

            /*Get the state of the item in string*/
            string strState = updateString.Substring(0,spaceIndex).Trim();
            if (string.IsNullOrEmpty(strState))
                return;


            UpdateTraceRow utr = new UpdateTraceRow();
            /*State char */
            if (strState.Length == 1)
            {
                char stateChar = strState.ToCharArray()[0];
                utr.RepositoryItemState = RepoInfo.StateFromChar(stateChar);
                utr.Action = RepoInfo.StateDescriptionFromEnum(utr.RepositoryItemState);
            }
            else
            {
                utr.Action = strState;
            }

            if(UpdateTraceListView.ItemsSource == null)
                UpdateTraceListView.ItemsSource = updateTrace;
       
            utr.Item = updateString.Substring(spaceIndex, updateString.Length - spaceIndex);
            
            //Add object to the binded collection
            updateTrace.Add(utr);
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
                repoProcess = value;
            }
        }

        /// <summary>
        /// Related process name
        /// </summary>
        public string RelatedRepositoryName { get; set; }

        /// <summary>
        /// Related command name
        /// </summary>
        public string RelatedCommand { get; set; }

        /// <summary>
        /// Notifies window about relatve process exit
        /// </summary>
        public void ProcessExited()
        {
            SetDefaultUI();

        }
        #endregion


        #region process events handlers

        void SetDefaultUI()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                UpdateTraceListView.Cursor = Cursors.Arrow;
                TextOnStatusBar.Text = "";
                progressBar.Visibility = Visibility.Hidden;
            }));
        }




        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Process != null)
            {

             

                //try to kill process if it still alive
                try
                {
                    if (!this.Process.HasExited)
                    {

                        string warningMsg = FindResource("MSG_REPOUPDATEBREAK") as string;
                        if (!string.IsNullOrEmpty(warningMsg))
                        {
                            if (MessageBox.Show(warningMsg, Application.Current.MainWindow.Title, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) ==
                                MessageBoxResult.Cancel)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }


                        this.Process.CancelOutputRead();
                        this.Process.Kill();
                        this.Process.Dispose();
                    }



                    /*Delete myself from the available windows collection */
                    WindowsManager.RemoveWindow(this);
                }
                catch (Exception ex)
                {
                    //TODO: Log an exception, do not raise anythig here. It's not critical point.
                }

            }
        }
    }
}
