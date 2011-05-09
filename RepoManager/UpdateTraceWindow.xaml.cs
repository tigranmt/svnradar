/* UpdateTraceWindow.xaml.cs --------------------------------
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SvnRadar.Util;
using System.Collections.ObjectModel;
using SvnRadar.DataBase;
using SvnRadar.Common.Intefaces;

namespace SvnRadar
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

        bool bConflictDetected = false;
      

        //Ctor
        public UpdateTraceWindow()
        {
            InitializeComponent();


            InitUI();
            
        }

        System.Threading.ManualResetEvent manuevent = null;
        public void SignalStarUpdate()
        {
            /*Resets the event*/
            manuevent = new System.Threading.ManualResetEvent(false);
        }


        private void SignalEndUpdate()
        {
            if (manuevent != null)
                manuevent.Set();
        }


        public void WaitUpdateEnd()
        {
            if (manuevent != null)
                manuevent.WaitOne();
        }


        /// <summary>
        /// If TRUE there are some conflict were detected during the update operation, FALSE otherwise
        /// </summary>
        public bool ConflictDetected 
        {
            get { return bConflictDetected; } 
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

            if (!bConflictDetected)
            {
                /*If there is any conflict detected */
                bConflictDetected = (utr.RepositoryItemState == RepoInfo.RepositoryItemState.Conflict);
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
            SignalEndUpdate();

            Dispatcher.Invoke((Action)(() =>
            {
                UpdateTraceListView.Cursor = Cursors.Arrow;
                TextOnStatusBar.Text = "";
                progressBar.Visibility = Visibility.Hidden;
                btnOk.Visibility = Visibility.Visible;



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
                    ErrorManager.LogException(ex);
                }

            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
