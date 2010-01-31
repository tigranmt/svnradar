using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ViewModel;
using System.Xml.XPath;
using System.ComponentModel;
using System.Windows.Threading;

namespace SubversionStatistics.Util
{

    public delegate void OnStartLoad(object sender, string path);
    public delegate void OnEndLoad(object sender, string path);

    /// <summary>
    /// Manages the tree view populating in Asynchronious way
    /// </summary>
    public class SubversionTreeViewAsyncLoader
    {

        string subverionExePath = string.Empty;

        public static readonly int DEEPNESS = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pathToLoadInfo">Complete path to subversion executable</param>
        public SubversionTreeViewAsyncLoader(string subverionExec)
        {
            subverionExePath = subverionExec;
        }


        /// <summary>
        /// Starts asynch loading of the children of the specified repository
        /// </summary>
        /// <param name="tvi">Parent TreeItemViewModel object</param>
        public void StartLoadChilds(TreeItemViewModel tvi)
        {

            if (tvi == null)
                throw new ArgumentNullException("The tvi parameter is null or empty");

            if (string.IsNullOrEmpty(subverionExePath))
                throw new ArgumentNullException("The subverionExePath path is null or empty");

            if (string.IsNullOrEmpty(tvi.RepositoryPath))
                throw new ArgumentNullException("The tvi.RepositoryPath parameter is null or empty");

            /*Run tree loading on separated thread*/
            BackgroundWorker _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;


            tvi.IsLoading = true;
            tvi.IsLoadingCanceled = false;

            _worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                StartSvnProcess(tvi);
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
        /// Create TreeViewItem object from the specified xml string
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        private TreeItemViewModel TreeViewItemModelFromXml(StringBuilder xmlString)
        {
            if (xmlString == null || xmlString.Length == 0)
                return null;

            TreeItemViewModel tvi = new TreeItemViewModel();
            XPathDocument xPathDoc = new XPathDocument(new System.IO.StringReader(xmlString.ToString()));
            XPathNavigator navigator = xPathDoc.CreateNavigator();


            XPathNodeIterator nodeIterator = navigator.Evaluate("/entry/@kind") as XPathNodeIterator;
            nodeIterator.MoveNext();

            /*The type of the object (file, dir..)*/
            switch (nodeIterator.Current.Value.ToLowerInvariant())
            {
                case "file":
                    tvi.TreeItemType = TreeItemViewModel.TreeItemTypeEnum.File;
                    break;
                case "dir":
                    tvi.TreeItemType = TreeItemViewModel.TreeItemTypeEnum.Folder;
                    break;
            }

            /* Item name */
            nodeIterator = navigator.Evaluate("/entry/name") as XPathNodeIterator;
            nodeIterator.MoveNext();
            tvi.Name = nodeIterator.Current.TypedValue as string;

            /* File size*/
            nodeIterator = navigator.Evaluate("/entry/size") as XPathNodeIterator;
            nodeIterator.MoveNext();
            string strFileSize = nodeIterator.Current.TypedValue as string;

            /*Revision number*/
            nodeIterator = navigator.Evaluate("/entry/commit/@revision") as XPathNodeIterator;
            nodeIterator.MoveNext();
            int revision = 0;
            if (Int32.TryParse(nodeIterator.Current.Value, out revision))
            {
                tvi.Revision = revision;
            }

            /* Account name*/
            nodeIterator = navigator.Evaluate("/entry/commit/author") as XPathNodeIterator;
            nodeIterator.MoveNext();
            tvi.AccountName = nodeIterator.Current.TypedValue as string;


            /* Date */
            nodeIterator = navigator.Evaluate("/entry/commit/date") as XPathNodeIterator;
            nodeIterator.MoveNext();

            DateTime dt;
            if (DateTime.TryParse(nodeIterator.Current.TypedValue as string, out dt))
            {
                tvi.Date = dt;
            }



            return tvi;
        }

        /// <summary>
        /// Resets the specified TreeViewItemModel properties state
        /// </summary>
        /// <param name="tvi">TreeItemViewModel object that need to be reset</param>
        private void ResetItem(TreeItemViewModel tvi)
        {
            /*Reset boolean values*/
            tvi.IsLoading = false;
            tvi.IsLoadingCanceled = false;
            tvi.IsExpanded = false;

            /*Clear all children*/
            tvi.Children.Clear();

            /*If this is a folder add Dummy child*/
            if (tvi.TreeItemType == TreeItemViewModel.TreeItemTypeEnum.Folder)
                tvi.Children.Add(TreeItemViewModel.DummyChild);

        }

        /// <summary>
        /// Start async execution of the Subversion process with the requested parameters list
        /// </summary>
        /// <param name="tvi">Parent TreeItemViewModel object</param>
        private void StartSvnProcess(TreeItemViewModel tvi)
        {

            if (tvi == null)
                throw new ArgumentNullException("The tvi parameter is null or empty");

            if (string.IsNullOrEmpty(tvi.RepositoryPath))
                throw new ArgumentNullException("The tvi.RepositoryPath parameter is null or empty");


            System.Diagnostics.ProcessStartInfo psi =
                         new System.Diagnostics.ProcessStartInfo("\"" + Configuration.Configuration.SubversionExeCompletePath + "\"");

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;

            psi.Arguments = " " + SvnConstants.LIST_COMMAND + " \"" + tvi.RepositoryPath + "\" --xml";

            psi.CreateNoWindow = true;

            bool beginLoadingData = false;


            SvnLogProcess process = new SvnLogProcess();

            process.StartInfo = psi;
            process.EnableRaisingEvents = true;


            process.ErrorDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ResetItem(tvi);
                }));
            };

            process.OutputDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                if (tvi.IsLoadingCanceled)
                {
                    try
                    {

                        process.Kill();
                        process.Dispose();
                    }
                    catch (Exception ex)
                    {
                        ErrorManager.ShowExceptionError(ex, true);
                    }


                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ResetItem(tvi);
                    }));


                }

                /*Start populating data*/
                if (e.Data.StartsWith("<entry"))
                {
                    beginLoadingData = true;
                    process.AppendXmlData(e.Data);
                }
                /*End populating data*/
                else if (e.Data.StartsWith("</entry"))
                {
                    if (tvi.IsLoadingCanceled)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ResetItem(tvi);
                        }));
                    }

                    process.AppendXmlData(e.Data);
                    StringBuilder xmlString = process.GetXmlString();

                    beginLoadingData = false;

                    TreeItemViewModel tvil = TreeViewItemModelFromXml(xmlString);
                    if (tvil != null)
                    {
                        tvil.RepositoryPath = tvi.RepositoryPath + "/" + tvil.Name;

                        /*Invoke on the main thread as the Children.Add code leads us  to update databinding*/
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                       {
                           tvi.Children.Add(tvil);
                           if (tvil.TreeItemType == TreeItemViewModel.TreeItemTypeEnum.Folder)
                               tvil.Children.Add(TreeItemViewModel.DummyChild);
                           
                       }));
                    }


                    process.ClearData();
                }
                /*Populating data*/
                else if (beginLoadingData)
                    process.AppendXmlData(e.Data);
            };


            process.Exited += delegate(object sender, System.EventArgs e)
            {

                if (tvi.IsLoadingCanceled)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ResetItem(tvi);
                    }));
                }


                tvi.IsLoading = false;
                tvi.IsLoadingCanceled = false;

                SvnLogProcess exitedProcess = sender as SvnLogProcess;
                if (exitedProcess != null)
                {
                    try
                    {
                        string erroMessage = exitedProcess.StandardError.ReadToEnd();
                        if (!string.IsNullOrEmpty(erroMessage))
                        {
                            return;

                        }

                    }
                    catch
                    {
                    }
                }
            };


            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();

        }



    }
}
