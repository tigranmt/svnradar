using System.Windows;
using System.Windows.Controls;
using SvnRadar.Util;
using System.Collections.ObjectModel;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System;
using System.Collections.Generic;
using SvnRadar.DataBase;
using System.Linq.Expressions;
using SvnRadar.Common.Controls;
using System.Xml.XPath;

namespace SvnRadar
{
    /// <summary>
    /// Class contains the function set to operate over the chosen repository
    /// </summary>
    public sealed class SvnRadarExecutor : INotifyPropertyChanged
    {
        #region fields
        /// <summary>
        /// Determines if there is some command is being executed 
        /// </summary>
        bool executingCommand = false;


        /// <summary>
        /// Holds last command executed on  repo
        /// </summary>
        internal static string LastExecutedCommand = null;


        /// <summary>
        /// Holds last revision numbe investigated in repository
        /// </summary>
        internal static int LastRevisionNumber = -1;



        /// <summary>
        /// Holds last file name queried on repository
        /// </summary>
        internal static string LastFileName = null;


        /// <summary>
        /// Background worker member for listening the Repository manager process output
        /// </summary>
        static private BackgroundWorker _worker;


        /// <summary>
        /// Repository manager exe process object
        /// </summary>
        internal static RepositoryProcess RepoProcess = null;


        /// <summary>
        /// Repository manager exe process for background processing from the sys tray
        /// </summary>
        internal static RepositoryProcess RepoProcessForSysTray = null;



        /// <summary>
        /// Holds the name of the repository that the user now querying about the information
        /// </summary>
        static string currentRepositoryInQueryName = string.Empty;


        ///// <summary>
        ///// Holds the in query repository working copy folder information
        ///// </summary>
        internal static FolderRepoInfo currentRepoFolderIndormation = null;


        /// <summary>
        /// Holds the pointer to the current window of updating trace
        /// </summary>
        internal static UpdateTraceWindow currentUpdateTraceWindow = null;

        
        /// <summary>
        /// Property holds last executed process
        /// </summary>
        public static RepositoryProcess LastExecutedProcess { get; set; }



        /// <summary>
        /// Launched process list
        /// </summary>
        static List<RepositoryProcess> processes = new List<RepositoryProcess>();

        /// <summary>
        /// In case when the subversions path wasn't set upped, or doesn't already exists, the user 
        /// will be notified about that via message box. To not show every verification time this message box,
        /// set this boolean to TRUE, in order to skip showing of message
        /// </summary>
        bool notifiedAboutSubversionPathLack = false;

        /// <summary>
        /// After counter arrives to 50/RepoBrowserConfiguration.Instance.ControlRate , 
        /// the notifiedAboutSubversionPathLack will be reseted to FALSE.
        /// </summary>
        int notifyCounter = 0;

        #endregion

        #region properties
        /// <summary>
        /// Holds the name of the repositiory that application's engine querying now
        /// </summary>
        internal static string CurrentRepositoryInQueryName
        {
            get { return currentRepositoryInQueryName; }
            set
            {
                currentRepositoryInQueryName = value;

            }

        }

        /// <summary>
        /// Boolean value identifies if currently there is an command executing in the current selected repository.
        /// True if there is a command executing, False otherwise.
        /// </summary>
        public bool ExecutingCommand
        {
            get
            {
                return IsProcesStillAvailable();
            }
            set
            {
                executingCommand = value;
                FirePropertyChanged(() => ExecutingCommand);
            }
        }
        #endregion

        #region ctor
        /// <summary>
        /// ctor
        /// </summary>      
        public SvnRadarExecutor()
        {

        }
        #endregion

       

        #region Svn executors

        #region Repository commands



        /// <summary>
        /// Gets the given working copy folder repository related information
        /// </summary>
        /// <param name="folderPath">Folder complete working copy path on the machine</param>
        /// <returns>FolderRepoInfo object, Null otherwise</returns>
        /// <param name="isCallForSysTray">If True the RepositoryProcess for sys tray will be used</param>
        public FolderRepoInfo GetFolderRepoInfo(string folderPath, bool isCallForSysTray)
        {
            if (string.IsNullOrEmpty(folderPath))
                return null;

            //if (!System.IO.Directory.Exists(folderPath))
            //    return null;


            /* Do not use Async call to the repo process exe, instead use particular code for this particular case */
            /* Run the process output listener in background. */
            /* As we get the information from the local copy, there is no any web access so this call, should be relatively 
               fast to execute, for this reason, in order to no add another level of complexity, we execte this command on the main UI
               blocking the program, usually for some seconds. */

            bool UrlPassed = folderPath.Trim().StartsWith("http://") || folderPath.Trim().StartsWith("https://");


            /*If for some reason RepoBrowserConfiguration.Instance.SubversionPath is emtpy, notify error and return */
            if (string.IsNullOrEmpty(RepoBrowserConfiguration.Instance.SubversionPath) || 
                !System.IO.File.Exists(RepoBrowserConfiguration.Instance.SubversionPath))
            {
                if (!notifiedAboutSubversionPathLack)
                {
                    ErrorManager.ShowCommonError("The subversion exe path is missed. Can not execute command", true);
                    notifiedAboutSubversionPathLack = true;
                }

                notifyCounter++;
                if (notifyCounter > (50/RepoBrowserConfiguration.Instance.ControlRate) )
                {
                    notifyCounter = 0;
                    notifiedAboutSubversionPathLack = false;
                }
                return null;
            }

            System.Diagnostics.ProcessStartInfo psi =
                          new System.Diagnostics.ProcessStartInfo(RepoBrowserConfiguration.Instance.SubversionPath);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.Arguments = " " + CommandStringsManager.CommonInfoCommand + " " + folderPath;
            psi.CreateNoWindow = true;

            RepositoryProcess process = RepoProcess;
            if (isCallForSysTray)
                process = RepoProcessForSysTray;

            process = new RepositoryProcess();

            process.StartInfo = psi;
            process.EnableRaisingEvents = true;



            /* Do not use Stack, for example, because we need to begins the scan of the results from the begining, in order 
             if in the future, the output of this command will be changed, that probabbly will be added some new  information 
             * on end, we by the way are able to get all neccessary for us information */
            List<string> diInfoStrings = new List<string>();


            process.ErrorDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {

            };

            process.OutputDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                diInfoStrings.Add(e.Data);
            };


            process.Exited += delegate(object sender, System.EventArgs e)
            {
                RepositoryProcess exitedProcess = sender as RepositoryProcess;
                if (exitedProcess != null)
                {
                    try
                    {
                        string erroMessage = exitedProcess.StandardError.ReadToEnd();
                        if (!string.IsNullOrEmpty(erroMessage))
                            ErrorManager.ShowCommonError(erroMessage, true);
                    }
                    catch
                    {
                    }
                }
            };


            process.Start();
            process.BeginOutputReadLine();

            /*wait approximately 20 seconds, after break the execution */
            process.WaitForExit();

            if (diInfoStrings.Count < 2)
            {
                if (process.ExitCode != 0)
                {
                    ErrorManager.ShowProcessError(process);
                }
                return null;
            }

            FolderRepoInfo frInfo = new FolderRepoInfo();
            frInfo.FolderPath = folderPath;

            try
            {
                for (int i = 0; i < diInfoStrings.Count; i++)
                {
                    int sepIndex = diInfoStrings[i].IndexOf(':');
                    if (sepIndex <= 0)
                        continue;

                    string value = diInfoStrings[i].Substring(sepIndex + 1);


                    if (i == 0)
                    {
                        frInfo.Path = value;
                    }

                    else if (i == 1)
                    {
                        frInfo.Url = value;
                    }

                    else if (i == 2)
                    {
                        frInfo.RepositoryRoot = value;
                    }

                    else if (i == 3)
                    {
                        frInfo.UUID = value;
                    }

                    else if (i == 4)
                    {
                        int revNum = -1;
                        Int32.TryParse(value, out revNum);
                        frInfo.Revision = revNum;
                    }

                    else if (i == 6 && UrlPassed)
                    {
                        frInfo.LastAuthor = value;
                    }
                    else if (i == 7)
                    {
                        if (!UrlPassed)
                            frInfo.LastAuthor = value;
                        else
                        {
                            int revNum = -1;
                            Int32.TryParse(value, out revNum);
                            frInfo.LastRevisionNumber = revNum;
                        }
                    }

                    else if (i == 8)
                    {
                        if (!UrlPassed)
                        {
                            int revNum = -1;
                            Int32.TryParse(value, out revNum);
                            frInfo.LastRevisionNumber = revNum;
                        }
                        else
                        {
                            frInfo.LastChangeDate = value;
                        }
                    }

                    else if (i == 9)
                    {
                        if (!UrlPassed)
                        {
                            frInfo.LastChangeDate = value;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#endif
            }


            return frInfo;
        }


        /// <summary>
        /// Get the log for the given repository. In difference of GetRepositoryLog it waits until command execution will terminate
        /// by blocking calling thread.
        /// </summary>
        /// <param name="repoPath">repository complete path </param>
        /// <param name="folderRepoInfo">FolderrepoInfo object, if Null no params will specified on command execution</param>
        /// <param name="isCallForSysTray">If True the RepositoryProcess for sys tray will be used</param>
        public List<RepoInfo> GetRepositoryLogImmediate(string repoPath, FolderRepoInfo folderRepoInfo, bool isCallForSysTray)
        {
            if (string.IsNullOrEmpty(RepoBrowserConfiguration.Instance.SubversionPath))
                return null;


            /*Svn special parameters for requesting the repository log*/
            string repoStatusRequestParams = " ";

            /*If object is not Null, pass Url like a parameter to reconver server side log of the repository*/
            if (folderRepoInfo != null)
                repoStatusRequestParams += folderRepoInfo.Url + " -r " + folderRepoInfo.Revision.ToString() + " -v --xml";

            //Assign last executed command
            LastExecutedCommand = CommandStringsManager.Repo_LogCommand;

            /*Assign Last revision number to invalid value, as we are not going to get info from entire repository,
             not from single revision.*/
            LastRevisionNumber = -1;

            LastFileName = string.Empty;


            StringBuilder unrecognizedData = new StringBuilder();





            System.Diagnostics.ProcessStartInfo psi =
                          new System.Diagnostics.ProcessStartInfo(RepoBrowserConfiguration.Instance.SubversionPath);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.Arguments = " " + CommandStringsManager.CommonLogCommand + " " + repoStatusRequestParams;
            psi.CreateNoWindow = true;

            RepositoryProcess process = RepoProcess;
            if (isCallForSysTray)
                process = RepoProcessForSysTray;

            process = new RepositoryProcess();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;


            /* Do not use Stack, for example, because we need to begin the scan of the results from the begining, in case, 
             if in the future, the output of this command will be changed, that probably will be added some new  information 
             * on end, we by the way are able to get all neccessary for us information */
            StringBuilder xmlLogStrings = new StringBuilder();


            process.ErrorDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {

            };

            process.OutputDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                if (e.Data.StartsWith("<logentry"))
                    xmlLogStrings.Append(e.Data);
                else if (e.Data.Contains("</logentry"))
                {
                    process.CancelOutputRead();

                    int endIndex = e.Data.IndexOf("</logentry");
                    xmlLogStrings.Append(e.Data.Substring(0, endIndex) + "</logentry>");

                    process.Close();
                    process.Dispose();
                }
                else if (xmlLogStrings.Length > 0)
                {
                    xmlLogStrings.Append(e.Data);
                }
                else
                {
                    unrecognizedData.Append(e.Data);
                }

            };


            process.Exited += delegate(object sender, System.EventArgs e)
            {

            };




            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();

            if (xmlLogStrings != null && xmlLogStrings.Length > 0)
            {
                bool breakPorcessing = false;
                return ProcessRepoLogCommandOutputLine(xmlLogStrings.ToString(), folderRepoInfo, false, out breakPorcessing);
            }
            else
            {
                if (process.ExitCode != 0)
                {
                    ErrorManager.ShowProcessError(process);
                }
                else
                {
                    RepoInfo rInfo = new RepoInfo();
                    rInfo.Account = folderRepoInfo.LastAuthor;
                    rInfo.Date = folderRepoInfo.LastChangeDate;
                    rInfo.Item = null;
                    rInfo.Revision = folderRepoInfo.LastRevisionNumber;
                    List<RepoInfo> list = new List<RepoInfo>();
                    list.Add(rInfo);
                    return list;
                }
            }


            return null;



        }




        /// <summary>
        /// Get the log for the given repository
        /// </summary>
        /// <param name="repoPath">repository complete path </param>
        /// <param name="folderRepoInfo">FolderrepoInfo object, if Null no params will specified on command execution</param>
        /// <param name="folderRepoInfo">If True, sys tray process object will be used</param>
        public void GetRepositoryLog(string repoPath, FolderRepoInfo folderRepoInfo, bool isCallForSysTray)
        {
            if (string.IsNullOrEmpty(RepoBrowserConfiguration.Instance.SubversionPath))
                return;


            try
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(repoPath);
                CurrentRepositoryInQueryName = dirInfo.Name;

                if (string.IsNullOrEmpty(CurrentRepositoryInQueryName))
                    return;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            /*Svn special parameters for requesting the repository log*/
            string repoStatusRequestParams = " ";

            /*If object is not Null, pass Url like a parameter to reconver server side log of the repository*/
            if (folderRepoInfo != null)
                repoStatusRequestParams += folderRepoInfo.Url + " -v --xml";

            //Assign last executed command
            LastExecutedCommand = CommandStringsManager.Repo_LogCommand;

            /*Assign Last revision number to invalid value, as we are not going to get info from entire repository,
             not from single revision.*/
            LastRevisionNumber = -1;

            LastFileName = string.Empty;

            System.IO.Directory.SetCurrentDirectory(repoPath);




            //Eecute command
            Execute(RepoBrowserConfiguration.Instance.SubversionPath, " " + CommandStringsManager.CommonLogCommand +
                " " + repoStatusRequestParams, isCallForSysTray);
        }

        /// <summary>
        /// Get the status of the given repository. No thread safe method.
        /// </summary>
        /// <param name="repoPath">Repositoory working cpy complete path</param>     
        /// <param name="folderRepoInfo">If True, sys tray process object will be used</param>
        public void GetRepositoryStatus(string repoPath, bool isCallForSysTray)
        {

            if (string.IsNullOrEmpty(RepoBrowserConfiguration.Instance.SubversionPath))
                return;


            try
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(repoPath);
                CurrentRepositoryInQueryName = dirInfo.Name;

                if (string.IsNullOrEmpty(CurrentRepositoryInQueryName))
                    return;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            /*Svn special parameters for requesting the repository status*/
            const string repoStatusRequestParams = " -uv";

            //Assign last executed command
            LastExecutedCommand = CommandStringsManager.Repo_StatusCommand;

            /*Assign Last revision number to invalid value, as we are not going to get info from entire repository,
             not from single revision.*/
            LastRevisionNumber = -1;

            LastFileName = string.Empty;

            System.IO.Directory.SetCurrentDirectory(repoPath);




            //Eecute command
            Execute(RepoBrowserConfiguration.Instance.SubversionPath, " " + CommandStringsManager.CommonStatusCommand +
                " " + repoStatusRequestParams, isCallForSysTray);
        }




        /// <summary>
        /// Updates specfied repository.
        /// </summary>
        /// <param name="workingCopyCompletePath"> Complete path to the working copy to update</param>
        /// <param name="isCallForSysTray"> Complete path to the working copy to update</param>
        public void UpdateRepository(string workingCopyCompletePath, bool isCallForSysTray)
        {


            if (string.IsNullOrEmpty(RepoBrowserConfiguration.Instance.SubversionPath))
                return;


            try
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(workingCopyCompletePath);
                CurrentRepositoryInQueryName = dirInfo.Name;

                if (string.IsNullOrEmpty(CurrentRepositoryInQueryName))
                    return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Assign last executed command
            LastExecutedCommand = CommandStringsManager.UpdateCommand;

            //Assign last revison number 
            LastRevisionNumber = -1;


            System.IO.Directory.SetCurrentDirectory(workingCopyCompletePath);

           
        

            //Execute command
            Execute(RepoBrowserConfiguration.Instance.SubversionPath,
                " " + CommandStringsManager.UpdateCommand, isCallForSysTray);
        }


        /// <summary>
        /// Get the information about given revision
        /// </summary>
        /// <param name="repoPath">Repositiory path</param>
        /// <param name="revisionNum">Revision number of interest</param>
        /// <param name="fileName">File name of interest</param>
        /// <param name="folderRepoInfo">Folder repository information object</param>
        /// <param name="isCallForSysTray">If True the RepositoryProcess for sys tray will be used</param>
        public void GetRevisionInfo(string repoPath, int revisionNum, string fileName, FolderRepoInfo folderRepoInfo, bool isCallForSysTray)
        {

            if (string.IsNullOrEmpty(RepoBrowserConfiguration.Instance.SubversionPath))
                return;


            try
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(repoPath);
                CurrentRepositoryInQueryName = dirInfo.Name;

                if (string.IsNullOrEmpty(CurrentRepositoryInQueryName))
                    return;
            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#endif
                return;
            }


            if (!System.IO.Path.IsPathRooted(fileName))
                fileName = System.IO.Path.GetFullPath(fileName);


            string logInfoParams = "  -r " + revisionNum.ToString();
            string fileRelativeUrl = fileName;
            if (folderRepoInfo != null)
            {
                try
                {
                    string relativeUrl = folderRepoInfo.RepoRelativeUrl;
                    if (!fileRelativeUrl.Equals(relativeUrl, StringComparison.InvariantCultureIgnoreCase))
                        fileRelativeUrl = fileName.Substring(fileName.IndexOf(relativeUrl) + relativeUrl.Length + 1);
                }
                catch (Exception ex)
                {
#if DEBUG
                    throw ex;
#endif
                    return;
                }
            }



            /*Svn special parameters for requesting the repository info*/
            string changesMadeInfoParams = " " + folderRepoInfo.Path + @"\" + fileRelativeUrl.Replace("/", @"\");

            if (!System.IO.File.Exists(changesMadeInfoParams))
            {
                ErrorManager.ShowCommonError("Can not lcate " + changesMadeInfoParams +
                    " file", true);
                return;
            }


            //Assign last executed command
            LastExecutedCommand = CommandStringsManager.RevisionInfoCommand;

            //Assign last revison number 
            LastRevisionNumber = revisionNum;

            //Assign last file name of interest
            LastFileName = fileName;

            /*Move OS pointer to the directory of interest*/
            string folderCompletePath = System.IO.Path.GetDirectoryName(changesMadeInfoParams);
            if (!string.IsNullOrEmpty(folderCompletePath))
                System.IO.Directory.SetCurrentDirectory(folderCompletePath);
            else
                throw new InvalidOperationException("Can not get the specofoed folder location. Path: " + changesMadeInfoParams);

            /*Get file name */
            string fileNaturalName = System.IO.Path.GetFileName(changesMadeInfoParams);

            /*Execute command*/
            Execute(RepoBrowserConfiguration.Instance.SubversionPath, " " + CommandStringsManager.CommonDiffCommand +
                " " + logInfoParams + " " + fileNaturalName, isCallForSysTray);
        }


        #endregion

        #region Main Executes

        /// <summary>
        /// Starts Repository checker process
        /// </summary>
        /// <param name="executablePath">The complete path to executable o the Svn exe</param>
        /// <param name="parameters">The parameters string to pass to the Svn process</param>
        /// <param name="isCallForSysTray">If True the RepositoryProcess for sys tray will be used</param>
        RepositoryProcess StartRepoProcess(string executablePath, string parameters, bool isCallForSysTray)
        {
            System.Diagnostics.ProcessStartInfo psi =
               new System.Diagnostics.ProcessStartInfo(executablePath);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.Arguments = parameters;
            psi.CreateNoWindow = true;

            RepositoryProcess process = RepoProcess;
            if (isCallForSysTray)
                process = RepoProcessForSysTray;

            process = new RepositoryProcess();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;


            process.ErrorDataReceived -= new System.Diagnostics.DataReceivedEventHandler(RepoProcess_ErrorDataReceived);
            process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(RepoProcess_ErrorDataReceived);


            process.OutputDataReceived -= new System.Diagnostics.DataReceivedEventHandler(RepoProcess_OutputDataReceived);
            process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(RepoProcess_OutputDataReceived);

            process.Exited -= new System.EventHandler(proc_Exited);
            process.Exited += new System.EventHandler(proc_Exited);

            process.Disposed -= new EventHandler(RepoProcess_Disposed);
            process.Disposed += new EventHandler(RepoProcess_Disposed);

            process.RelatedRepositoryName = CurrentRepositoryInQueryName;
            process.Command = LastExecutedCommand;
            process.RevisionNumber = LastRevisionNumber;
            process.FileName = LastFileName;
            process.FolderRepoInformation = currentRepoFolderIndormation;
            process.UpdateTraceWindowObject = currentUpdateTraceWindow;





            //Add the newly created process to the processes list
            processes.Add(process);

            process.Start();
            process.BeginOutputReadLine();
            return process;

        }

        /// <summary>
        /// Executes arbitrary command with Repository manager exe.
        /// </summary>
        /// <param name="executablePath">Executable complete path</param>
        /// <param name="parameters">The parameters string need to pass to the executable in order to execute some command</param>
        void Execute(string executablePath, string parameters, bool isCallForSysTray)
        {



            /*If for some reason RepoBrowserConfiguration.Instance.SubversionPath is emtpy, notify error and return */
            if (string.IsNullOrEmpty(RepoBrowserConfiguration.Instance.SubversionPath))
            {
                ErrorManager.ShowCommonError("The subversion exe path is missed. Can not execute command", true);
                return;
            }

            /*Run the process output listener in background*/
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            RepositoryProcess process = StartRepoProcess(executablePath, parameters, isCallForSysTray);
            LastExecutedProcess = process;

            _worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {

                ExecutingCommand = true;
                if (process != null)
                {

                    process.WaitForExit();
                    _worker.CancelAsync();
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
                ExecutingCommand = false;
            };

            _worker.RunWorkerAsync();

        }
        #endregion

        #region Repository process events listeners and processors
        /// <summary>
        /// Processes the output data from the last command executed over the repository
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RepoProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                /* As adding the repo info can impact user interface too, need to execute folowing conde on the main UI thread.
                 * So use the Dispathcer to achieve that goal.*/
                RepositoryProcess repoProc = sender as RepositoryProcess;
                if (repoProc == null)
                    return;

                /*Process STATUS command output*/
                if (repoProc.Command == CommandStringsManager.Repo_StatusCommand)
                {

                    Application.Current.Dispatcher.Invoke((AddRepoInfoDelegate)delegate(string rName, RepoInfo repoInfo)
                    {
                        DataBase.RepoInfoBase.AddRepoInfo(rName, repoInfo);
                    },
                        new object[] { repoProc.RelatedRepositoryName, ProcessRepoStatusCommandOutputLine(e.Data) });
                }
                /*Process REVISION INFO command*/
                else if (repoProc.Command == CommandStringsManager.RevisionInfoCommand)
                {
                    Application.Current.Dispatcher.Invoke((AddRevisionInfoDelegate)delegate(string rName, int revisonNumber, string itemName, string dateStr, string madeChanges)
                    {
                        DataBase.RepoInfoBase.AddRevisionInfoString(rName, revisonNumber, itemName, dateStr, madeChanges);
                    },
                    new object[] { repoProc.RelatedRepositoryName, repoProc.RevisionNumber, repoProc.FileName, string.Empty, e.Data });
                }


                 /*Process REVISION LOG command*/
                else if (repoProc.Command == CommandStringsManager.Repo_LogCommand)
                {

                    if (repoProc.repoLogInformation == null)
                        repoProc.repoLogInformation = new RepoInfo();

                    if (string.IsNullOrEmpty(e.Data))
                        return;

                    /*Start poulating single logentry xml data*/
                    if (e.Data.StartsWith("<logentry"))
                    {
                        repoProc.repoLogInformation.xmlData.Append(e.Data);
                        repoProc.repoLogInformation.StartPopulating = true;
                    }
                    /*Edn of log entry for sngle item*/
                    else if (e.Data.StartsWith("</logentry"))
                    {
                        /*reset state variables */

                        repoProc.repoLogInformation.xmlData.Append("</logentry>");
                        repoProc.repoLogInformation.StartPopulating = false;


                        /*If there is no any information available do not add it to the base*/
                        bool breakPorcessing = false;
                        List<RepoInfo> repoList = ProcessRepoLogCommandOutputLine(repoProc.repoLogInformation.xmlData.ToString(), repoProc.FolderRepoInformation, true, out breakPorcessing);

                        if (repoList == null && breakPorcessing)
                        {
                            if (repoProc != null)
                            {
                                /* Catch any kind of exception and proceed */
                                try
                                {
                                    repoProc.Close();
                                 //   repoProc.Kill();
                                  
                                }
                                catch
                                {
                                }
                                finally
                                {
                                    repoProc.Dispose();
                                    repoProc.repoLogInformation = null;
                                }
                            }

                            return;
                        }

                        repoProc.repoLogInformation.xmlData.Remove(0, repoProc.repoLogInformation.xmlData.Length);

                        if (repoList == null || repoList.Count == 0)
                        {
                            repoProc.repoLogInformation = null;
                            return;
                        }

                        Application.Current.Dispatcher.Invoke((AddRepoListInfoDelegate)delegate(string rName, List<RepoInfo> repoInfo)
                        {
                            DataBase.RepoInfoBase.AddRepoInfoList(rName, repoInfo);
                        },
                        new object[] { repoProc.RelatedRepositoryName, repoList });


                        repoProc.repoLogInformation = null;
                    }
                    else if (repoProc.repoLogInformation.StartPopulating)
                    {
                        repoProc.repoLogInformation.xmlData.Append(e.Data);
                    }

                }
                /*UPDATE command*/
                else if (repoProc.Command == CommandStringsManager.UpdateCommand)
                {
                    if (repoProc.UpdateTraceWindowObject == null)
                        return;

                    /*Add the string to show in the relative window*/
                    Application.Current.Dispatcher.Invoke((AddUpdateTraceInfo)delegate(string traceString)
                    {
                        repoProc.UpdateTraceWindowObject.AddString(traceString);
                    },
                    new object[] { e.Data });

                }
                else
                {
                    repoProc.notRecognizedData.AppendLine(e.Data);
                }



            }



        }


        /// <summary>
        /// Finds the process in collection of available ones for the current selected tab item
        /// </summary>
        /// <returns>True, if the process still exists, False otherwise</returns>
        bool IsProcesStillAvailable()
        {
            if (RepoBrowserWindow.SelectedRepoTabItem != null &&
                    !string.IsNullOrEmpty(RepoBrowserWindow.SelectedRepoTabItem.RepositoryName))
                return (IsProcesStillAvailable(RepoBrowserWindow.SelectedRepoTabItem.RepositoryName) != null);

            return false;
        }

        /// <summary>
        /// Finds the process in collection of available ones for the specified repository name
        /// </summary>
        /// <param name="processRepositoryName">Repository name the process of which have to be found</param>
        /// <returns>True, if the process still exists, False otherwise</returns>
        public RepositoryProcess IsProcesStillAvailable(string processRepositoryName)
        {
            if (string.IsNullOrEmpty(processRepositoryName))
                return null;

            return (processes.Find((x) => x.RelatedRepositoryName.Equals(processRepositoryName,
                StringComparison.InvariantCultureIgnoreCase)));


        }


        /// <summary>
        /// Determines if the specified repository is up to date
        /// </summary>
        /// <param name="repositoryUrl">Repository Url to verify</param>
        /// <returns>FolderRepoInfo object of the specified repository, Null otherwise.</returns>
        public FolderRepoInfo IsRepositoryUpTodate(string repositoryUrl)
        {
            if (string.IsNullOrEmpty(repositoryUrl))
                return null;



            FolderRepoInfo wcInfo = GetFolderRepoInfo(repositoryUrl, true);
            if (wcInfo != null)
            {
                FolderRepoInfo repoInfo = GetFolderRepoInfo(wcInfo.Url, true);
                if (repoInfo == null)
                {
                    return null;// throw new NullReferenceException("Not able get the repository folder information");
                }

                if (repoInfo.LastRevisionNumber > wcInfo.LastRevisionNumber)
                {
                    repoInfo.FolderPath = wcInfo.FolderPath;
                    return repoInfo;
                }
            }


            return null;
        }

        /// <summary>
        /// Removes the specified process from the collection of available ones
        /// </summary>
        /// <param name="repoProcess">Process object to remove from collection</param>
        void RemoveProcessFromCollection(RepositoryProcess repoProcess)
        {
            /*Find in collection of the available process the secified one and remove it from the collection of the available ones*/
            lock (processes)
            {
                RepositoryProcess processToRemove = processes.Find((x) => x.RelatedRepositoryName.Equals(repoProcess.RelatedRepositoryName, StringComparison.InvariantCultureIgnoreCase));
                processes.Remove(processToRemove);
            }
        }


        void RepoProcess_Disposed(object sender, EventArgs e)
        {
            //TODO: Notify that process executed on some command is terminated
            RepositoryProcess proc = sender as RepositoryProcess;

            RemoveProcessFromCollection(proc);
            WindowsManager.NotifyRepositoryProcessExit(proc.RelatedRepositoryName, proc.Command);


            ExecutingCommand = false;
        }


        /// <summary>
        /// Handles the error event got from the launched Svn process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RepoProcess_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (_worker != null)
                _worker.CancelAsync();



            RepositoryProcess proc = sender as RepositoryProcess;
            

            if (proc != null)
                proc.Kill();

            RemoveProcessFromCollection(proc);

            WindowsManager.NotifyRepositoryProcessExit(proc.RelatedRepositoryName, proc.Command);

            ExecutingCommand = false;



        }


        /// <summary>
        /// Handles the process Exited event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proc_Exited(object sender, System.EventArgs e)
        {

            RepositoryProcess proc = sender as RepositoryProcess;

            RemoveProcessFromCollection(proc);

            try
            {
                string errorMessage = proc.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(errorMessage))
                    ErrorManager.ShowCommonError(errorMessage, true);
            }
            catch
            {
            }

            WindowsManager.NotifyRepositoryProcessExit(proc.RelatedRepositoryName, proc.Command);

            ExecutingCommand = false;


        }




        /// <summary>
        /// Parser the XML string into the list of RepoInfo objects
        /// </summary>
        /// <param name="strOutputLine">Xml string</param>
        /// <param name="foldeRepoInfo">If not Null, only revisions major then majorRevisionPresent will retrieved, 
        /// otherwise the entire log history will be dumped into the list. </param>
        /// <param name="skipLowerRevisions">If true the revisions lower then the specified by foldeRepoInfo object will be skipped</param> 
        /// <param name="breakProcessing">If true process break it's execution</param> 
        /// <returns></returns>
        List<RepoInfo> ProcessRepoLogCommandOutputLine(string strOutputLine, FolderRepoInfo foldeRepoInfo, bool skipLowerRevisions, out bool breakProcessing)
        {

            if (foldeRepoInfo == null)
            {
                breakProcessing = false;
                return null;
            }


            if (string.IsNullOrEmpty(strOutputLine))
            {
                breakProcessing = false;
                return null;
            }


            List<RepoInfo> repoInfoList = new List<RepoInfo>();

            try
            {
                XPathDocument xPathDoc = new XPathDocument(new System.IO.StringReader(strOutputLine));
                XPathNavigator navigator = xPathDoc.CreateNavigator();


                XPathNodeIterator nodeIterator = navigator.Evaluate("/logentry/@revision") as XPathNodeIterator;
                nodeIterator.MoveNext();

                /* Revision */

                int revI = nodeIterator.Current.ValueAsInt;

                /*If skipLowerRevisions is TRUE and majorRevisionPresent has a valid value, and found revision number is less then 
                 * revI, do not add it into the result list 
                */
                if (skipLowerRevisions && foldeRepoInfo.Revision > 0 && foldeRepoInfo.Revision >= revI)
                {
                    /*As we get the lower revision , there is no need process the rest, as the rest will be inferior too,
                     so break execution of the process*/
                    breakProcessing = true;
                    return null;
                }


                RepoInfo singleRow = new RepoInfo();
                singleRow.Revision = revI;

                /* Author */
                nodeIterator = navigator.Evaluate("/logentry/author") as XPathNodeIterator;
                nodeIterator.MoveNext();
                singleRow.Account = nodeIterator.Current.TypedValue.ToString();


                /* Date */
                nodeIterator = navigator.Evaluate("/logentry/date") as XPathNodeIterator;
                nodeIterator.MoveNext();
                singleRow.Date = nodeIterator.Current.TypedValue.ToString();


                /* Message */
                nodeIterator = navigator.Evaluate("/logentry/msg") as XPathNodeIterator;
                nodeIterator.MoveNext();
                singleRow.UserComment = nodeIterator.Current.TypedValue.ToString();

                /* Paths */
                nodeIterator = navigator.Evaluate("/logentry/paths/path") as XPathNodeIterator;
                int count = 0;


                //Folder relative url
                string relativeUrl = null;

                /*Skip the paths that are not in the specified directory URL*/
                if (foldeRepoInfo != null && foldeRepoInfo.RepoRelativeUrl != null)
                    relativeUrl = foldeRepoInfo.RepoRelativeUrl;


                while (nodeIterator.MoveNext())
                {

                    RepoInfo current = null;
                    if (count == 0)
                    {
                        /*If first row assign the parent object reference*/
                        current = singleRow;
                    }
                    else
                    {
                        /*If this is not the first row, clonethe parent object and create a new one*/
                        current = singleRow.Clone() as RepoInfo;
                    }
                    count++;

                    /*get action related path*/
                    string actionUrl = nodeIterator.Current.TypedValue.ToString();

                    /*Skip the paths that are not in the specified directory URL*/
                    if (!string.IsNullOrEmpty(relativeUrl))
                        if (!actionUrl.StartsWith(relativeUrl))
                            continue;


                    /*If path is ok, continue populating object data*/

                    current.Item = actionUrl;
                    current.ItemDomain = RepoInfo.Domain.Repository;


                    string action = nodeIterator.Current.GetAttribute("action", string.Empty);
                    if (!string.IsNullOrEmpty(action))
                        current.RepositoryItemState = RepoInfo.StateFromChar(action[0]);

                    repoInfoList.Add(current);


                }







            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#endif
            }
            breakProcessing = false;
            return repoInfoList;
        }

        /// <summary>
        /// Process Repository STATUS command output 
        /// </summary>
        /// <returns>The colleciton of RepoInfo objects</returns>
        RepoInfo ProcessRepoStatusCommandOutputLine(string strOutputLine)
        {
            if (string.IsNullOrEmpty(strOutputLine))
                return null;

            RepoInfo InfoCol = new RepoInfo();

            /*The columns count can vary, if  in the list of the items there 
             * are also items that were changed localy in working copy*/

            const int NOLOCALCHANGES_COLUMNS_COUNT = 5;
            const int LOCALCHANGES_COLUMNS_COUNT = 6;

            string[] content = strOutputLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (content != null)
            {
                if (content.Length == NOLOCALCHANGES_COLUMNS_COUNT)
                {
                    InfoCol.RepositoryItemState = RepoInfo.StateFromChar(content[0][0]);

                    int revNum = -1;
                    Int32.TryParse(content[1], out revNum);
                    InfoCol.WCRevision = revNum;

                    Int32.TryParse(content[2], out revNum);
                    InfoCol.Revision = revNum;

                    InfoCol.Account = content[3];
                    InfoCol.Item = content[4];


                }
                else if (content.Length == LOCALCHANGES_COLUMNS_COUNT)
                {
                    InfoCol.WcItemState = RepoInfo.StateFromChar(content[0][0]);

                    InfoCol.RepositoryItemState = RepoInfo.StateFromChar(content[1][0]);

                    int revNum = -1;
                    Int32.TryParse(content[2], out revNum);
                    InfoCol.Revision = revNum;

                    Int32.TryParse(content[3], out revNum);
                    InfoCol.WCRevision = revNum;

                    InfoCol.Account = content[4];
                    InfoCol.Item = content[5];
                }
            }

            return InfoCol;
        }


        #endregion



        #endregion

        #region helper methods
        private void FirePropertyChanged<T>(Expression<Func<T>> property)
        {
            /*Get expression tree base, recover property name at runtime and notify Binding neinge about that*/
            var expression = property.Body as MemberExpression;
            var member = expression.Member;
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(member.Name));
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}