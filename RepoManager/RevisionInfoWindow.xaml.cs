/* RevisionInfoWindow.xaml.cs --------------------------------
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
                    this.Title = revisionInformation.RevisionNumber.ToString() + " : " + revisionInformation.Item;
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

            ///*Subscribe to the property change notification. So we will recieve notification about a change in the property in the mothod and update UI .*/

            revisionInformation.PropertyChanged += new PropertyChangedEventHandler(revisionInformation_PropertyChanged);
        }

        /// <summary>
        /// Handles revison info arbitrary property change event notification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void revisionInformation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
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
                GoToSourceButton.Visibility = Visibility.Visible;
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
                    ErrorManager.LogException(ex);
                }


                /*Delete myself from the available windows collection */
                WindowsManager.RemoveWindow(this);

                /*Remove my information from the shared base, to not waste the memory*/
                if (this.Process != null && this.RevisionInformation != null)
                {
                    SvnRadar.DataBase.RepoInfoBase.RemoveRevisonInfoStringFromBase(this.RelatedRepositoryName, this.RevisionInformation.RevisionNumber);
                }


                /*Destroying object event handler*/
                revisionInformation.PropertyChanged -= new PropertyChangedEventHandler(revisionInformation_PropertyChanged);
                revisionInformation = null;
                ChangesView.Inlines.Clear();



                GC.SuppressFinalize(this);
                GC.Collect();


            }
        }


        /// <summary>
        /// Handles Go to source button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToSourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (Process == null || Process.FolderRepoInformation == null)
                return;

            try
            {
                /*Ger repository local path*/
                string repoPath = Process.RelatedRepositoryName;

                /*Get file relative URL on repository*/
                string fileRelativeUrl = Process.FileName;

                if (!string.IsNullOrEmpty(fileRelativeUrl))
                {
                    /*Try to compose complete local path to the specified source file*/
                    string repoRealtiveUrl = Process.FolderRepoInformation.RepoRelativeUrl;
                    if (fileRelativeUrl.IndexOf(repoRealtiveUrl) == 0)
                    {
                        if (repoRealtiveUrl.Length < fileRelativeUrl.Length)
                        {
                            fileRelativeUrl = fileRelativeUrl.Substring(repoRealtiveUrl.Length);
                            string fileIOPath = fileRelativeUrl.Replace("/", @"\");
                            string fileCompletePath = repoPath + fileIOPath;

                            SvnRadarExecutor svnExecutor = (SvnRadarExecutor)((ObjectDataProvider)AppResourceManager.FindResource("svnRadarExecutor")).ObjectInstance;
                            
                            if (System.IO.File.Exists(fileCompletePath))
                            {
                                svnExecutor.ExecuteExplorerProcess(fileCompletePath);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex,true);
            }



        }



    }
}
