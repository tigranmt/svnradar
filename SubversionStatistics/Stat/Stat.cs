using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SubversionStatistics.Util;

using System.Xml.XPath;
using System.IO;
using System.Reflection;
using System.Data.SQLite;
using ViewModel;
using System.Collections.ObjectModel;

namespace SubversionStatistics.Stat
{
    #region delegates
    public delegate void BeginLoadingBase(object sender, System.EventArgs args);
    public delegate void EndLoadingBase(object sender, System.EventArgs args);
    public delegate void ErrorRecieved(object sender, ProcessErrorEventArgs errArgs);
    public delegate void BeginProcessingEntry(object sender, LogEntry processingEntry);
    public delegate void EndProcessingEntry(object sender, LogEntry processedEntry);
    public delegate void BeginProcessingFileEntry(object sender, FileEntry fe);
    public delegate void EndProcessingFileEntry(object sender, FileEntry fe);
    public delegate void BeginGettingChangedLinesCount(object sender, int revisonNumber, string fileName);
    public delegate void EndGettingChangedLinesCount(object sender, int revisionNumber, string fileName);
    #endregion


    /// <summary>
    /// Superclass for handling Statistics data
    /// </summary>
    public class SvnStat
    {

        #region events / launchers
        /// <summary>
        /// Raises when the library begins to load the data into data base
        /// </summary>
        public event BeginLoadingBase BeginLoadingBaseEvent;

        /// <summary>
        /// Raises when the loading into the data base is terminated
        /// </summary>
        public event EndLoadingBase EndLoadingBaseEvent;

        /// <summary>
        /// Raises when any type error burns
        /// </summary>
        public event ErrorRecieved ErrorRecievedEvent;

        /// <summary>
        /// Raises when library begin processing of a specific LogEntry
        /// </summary>
        public event BeginProcessingEntry BeginProcessingEntryEvent;

        /// <summary>
        /// Raises when library end processing of a specific LogEntry
        /// </summary>
        public event EndProcessingEntry EndProcessingEntryEvent;


        /// <summary>
        /// Raises when library begin processing of a specific LogEntry
        /// </summary>
        public event BeginProcessingFileEntry BeginProcessingFileEntryEvent;

        /// <summary>
        /// Raises when library end processing of a specific LogEntry
        /// </summary>
        public event EndProcessingFileEntry EndProcessingFileEntryEvent;


        /// <summary>
        /// Raises when library is going to begin to get changed lines count data from the specified revision's file
        /// </summary>
        public event BeginGettingChangedLinesCount BeginGettingChangedLinesCountEvent;


        /// <summary>
        /// Raises when library is end to get changed lines count data from the specified revision's file
        /// </summary>
        public event EndGettingChangedLinesCount EndGettingChangedLinesCountEvent;


        /// <summary>
        /// Notifies all subscribers about begining of  loading of central base
        /// </summary>
        void RaiseBeginLoadingBaseEvent()
        {
            if (BeginLoadingBaseEvent != null)
                BeginLoadingBaseEvent(this, null);
        }

        /// <summary>
        /// Notifies all subscribers about the end of  loading of central base
        /// </summary>
        void RaiseEndLoadingBaseEvent()
        {
            if (EndLoadingBaseEvent != null)
                EndLoadingBaseEvent(this, null);
        }


        /// <summary>
        /// Notifies to subscribers about recieved error 
        /// </summary>
        /// <param name="errorDescritpion">The error complete description </param>
        /// <param name="processID">Related process ID</param>
        void RaiseErrorRecieved(string errorDescritpion, int processID)
        {
            if (ErrorRecievedEvent != null)
            {
                ProcessErrorEventArgs errEventArgs = new ProcessErrorEventArgs
                {
                    Message = errorDescritpion,
                    ProcessID = processID
                };

                ErrorRecievedEvent(this, errEventArgs);
            }
        }


        /// <summary>
        ///  Notifies to subscribers about begin of processing of specified log entry
        /// </summary>
        /// <param name="entry"></param>
        void RaiseBeginProcessingEntry(LogEntry entry)
        {
            if (BeginProcessingEntryEvent != null)
                BeginProcessingEntryEvent(this, entry);
        }

        /// <summary>
        ///Notifies to subscribers about end of processing of specified log entry
        /// </summary>
        /// <param name="entry"></param>
        void RaiseEndProcessingEntry(LogEntry entry)
        {
            if (EndProcessingEntryEvent != null)
                EndProcessingEntryEvent(this, entry);
        }


        /// <summary>
        ///Notifies to subscribers about begin of processing of specified file entry
        /// </summary>
        /// <param name="entry"></param>
        void RaiseBeginProcessingFileEntry(FileEntry fe)
        {
            if (BeginProcessingFileEntryEvent != null)
                BeginProcessingFileEntryEvent(this, fe);
        }


        /// <summary>
        ///Notifies to subscribers about end of processing of specified file entry
        /// </summary>
        /// <param name="entry"></param>
        void RaiseEndProcessingFileEntry(FileEntry fe)
        {
            if (EndProcessingFileEntryEvent != null)
                EndProcessingFileEntryEvent(this, fe);
        }



        /// <summary>
        /// Raises begin get changed lines count event 
        /// </summary>
        /// <param name="revisionNumber">Revision number</param>
        /// <param name="fileName">File name</param>
        void RaiseBeginGettingChangedLinesCount(int revisionNumber, string fileName)
        {
            if (BeginGettingChangedLinesCountEvent != null)
                BeginGettingChangedLinesCountEvent(this, revisionNumber, fileName);
        }

        /// <summary>
        /// Raises end get changed lines count event 
        /// </summary>
        /// <param name="revisionNumber">Revision number</param>
        /// <param name="fileName">File name</param>
        void RaiseEndGettingChangedLinesCount(int revisionNumber, string fileName)
        {
            if (EndGettingChangedLinesCountEvent != null)
                EndGettingChangedLinesCountEvent(this, revisionNumber, fileName);
        }

        #endregion

        #region Constants
        private static readonly string BASE_DIRECTORY_NAME = "DirBase";
        private static readonly string TEMPLATE_DB_RELATIVE_PATH = @"dbtemplate\template";
        private static readonly string STAT_FILE_EXTENSION = ".rsb";
        #endregion

        #region ctor
        /// <summary>
        /// Constructs SubversionStatistics object based on the valid path to the 
        /// Subversion repository
        /// </summary>
        /// <param name="dbPath"></param>
        public SvnStat(string repositoryPath)
        {

            if (string.IsNullOrEmpty(repositoryPath))
                throw new ArgumentNullException("The parameter {repositoryPath} is Null or Empty");

            if (!Directory.Exists(repositoryPath))
                throw new ArgumentException("The file { " + repositoryPath + " } doesn't exist)");


            string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string templateLocation = assemblyLocation +
                System.IO.Path.DirectorySeparatorChar + TEMPLATE_DB_RELATIVE_PATH;

            if (!File.Exists(templateLocation))
                throw new ArgumentException("The file { " + templateLocation + " } doesn't exist). Please create the directory and copy template databse in it.");



            /*First of all control on presence of the template and 
             * relative repository db files*/
            try
            {

                string repositoryDirName = Path.GetFileName(repositoryPath);

                if (string.IsNullOrEmpty(repositoryDirName))
                    throw new ArgumentNullException("Can not get repository directoy name");

                string baseDirectoryPath = assemblyLocation + System.IO.Path.DirectorySeparatorChar +
                    BASE_DIRECTORY_NAME;
                if (!Directory.Exists(baseDirectoryPath))
                    Directory.CreateDirectory(baseDirectoryPath);

                string repositoryRelatedDirectory =
                    baseDirectoryPath + System.IO.Path.DirectorySeparatorChar + repositoryDirName;

                if (!Directory.Exists(repositoryRelatedDirectory))
                    Directory.CreateDirectory(repositoryRelatedDirectory);

                string repositoryRelatedFile = repositoryRelatedDirectory +
                    System.IO.Path.DirectorySeparatorChar + repositoryDirName + STAT_FILE_EXTENSION;
                if (!File.Exists(repositoryRelatedFile))
                    File.Copy(templateLocation, repositoryRelatedFile);


                RepositoryPath = repositoryPath;

                RepositoryConnectionString = "data source=" + repositoryRelatedFile;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            /**/


        }
        #endregion

        #region properties

        /// <summary>
        /// Repository complete path
        /// </summary>
        public string RepositoryPath { get; set; }


        /// <summary>
        /// Repository Url
        /// </summary>
        public string RepositoryUrl { get; set; }


        /// <summary>
        /// Repository connection string
        /// </summary>
        public string RepositoryConnectionString { get; set; }

     

        #endregion

        #region methods




        /// <summary>
        /// Constructs repository tree from the given working copy path
        /// </summary>
        /// <param name="workingCopyPath">Working copy complete path</param>
        /// <param name="result">Result list</param>   
        public void ConstructRepoTree(string workingCopyPath,ObservableCollection<TreeItemViewModel> result)
        {

        }


       /// <summary>
        ///  Constructs the tree from the given repository path
       /// </summary>
       /// <param name="repositoryPath">repository complete path</param>
        /// <param name="result">Result list</param>      
        public void ConstructTreeChilds(string repositoryPath, ObservableCollection<TreeItemViewModel> result)
        {

        }

        /// <summary>
        /// Populates the specified database with the data recovered from the specified repository
        /// </summary>
        public void BeginPopulateDatabaseFromSubversionRepository()
        {
            /* Control on necessary initial data presence */
            if (string.IsNullOrEmpty(RepositoryPath))
                throw new ArgumentOutOfRangeException("The {RepositoryPath} is Empty");

            if (string.IsNullOrEmpty(RepositoryConnectionString))
                throw new ArgumentOutOfRangeException("The {RepositoryConnectionString} parameter is Empty");


            if (string.IsNullOrEmpty(Configuration.Configuration.SubversionExeCompletePath) ||
                !System.IO.File.Exists(Configuration.Configuration.SubversionExeCompletePath))
            {
                throw new ArgumentNullException("Set up Subversion executable path");
            }


            /*Raise an event to notify the subscribers about
             * the begining of the load operation*/
            RaiseBeginLoadingBaseEvent();

            /*Launch subversion executable with  -v --xml parameter*/
            RepositoryUrl = GetFolderRepoUrl(RepositoryPath);
            if (string.IsNullOrEmpty(RepositoryUrl))
                throw new ArgumentException("Ca not get Url from path " + RepositoryPath);

            string paramters = " " + SvnConstants.LOG_COMMAND + " " + RepositoryUrl + SvnConstants.LOG_COMMAND_PARAMETERS;
            StartRepoProcessAsync(Configuration.Configuration.SubversionExeCompletePath, paramters);



        }

        /// <summary>
        /// Starts Repository  process asynchroniously
        /// </summary>
        /// <param name="executablePath">The complete path to executable o the Svn exe</param>
        /// <param name="parameters">The parameters string to pass to the Svn process</param>        
        void StartRepoProcessAsync(string executablePath, string parameters)
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


            SvnLogProcess process = new SvnLogProcess();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;


            /*Subscribe to events */
            process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
            process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            process.Exited += new EventHandler(process_Exited);

            process.Start();
            process.BeginOutputReadLine();

        }



        /// <summary>
        ///  Gets the count of the lines changed on the specified file in specified revison
        /// </summary>
        /// <param name="revisionNumber">Revision number</param>
        /// <param name="fileUrl">File url</param>
        /// <returns>The count of changed lines</returns>
        int GetChangedLinesCountFromEntry(int revisionNumber, string fileUrl)
        {

            if (string.IsNullOrEmpty(Configuration.Configuration.SubversionExeCompletePath) ||
                !File.Exists(Configuration.Configuration.SubversionExeCompletePath))
                throw new ArgumentNullException("Subversion exe file path is not set upped");


            if (revisionNumber < 0)
                throw new ArgumentNullException("Not valid value for parameter {revisionNumber} : " + revisionNumber.ToString());


            if (string.IsNullOrEmpty(fileUrl))
                throw new ArgumentNullException("The {fleUrl} parameter is Null or empty");


            string completeUtl = RepositoryUrl.ConcatUrlsDiff(fileUrl);
            if (string.IsNullOrEmpty(completeUtl) ||
                completeUtl.Equals(RepositoryUrl))
                throw new ArgumentException("Can not find file path. Path: " + completeUtl + " in repository " + RepositoryUrl);

            System.Diagnostics.ProcessStartInfo psi =
               new System.Diagnostics.ProcessStartInfo(Configuration.Configuration.SubversionExeCompletePath);

            string parameters = " " + SvnConstants.DIFF_COMMAND + " -c " + revisionNumber.ToString() + " " + completeUtl;

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.Arguments = parameters;
            psi.CreateNoWindow = true;


            SvnLogProcess process = new SvnLogProcess();

            process.StartInfo = psi;
            process.EnableRaisingEvents = true;
            process.Start();


            string output = process.StandardOutput.ReadToEnd();

            int changedLinesCount = 0;
            if (!string.IsNullOrEmpty(output))
            {
                string[] contentSplits = output.Split(new char[] { '\r', '\n' });
                if (contentSplits != null && contentSplits.Length > 0)
                {
                    foreach (string content in contentSplits)
                    {
                        if (content.StartsWith("+ "))
                            changedLinesCount++;
                    }
                }
            }


            process.WaitForExit();
            return changedLinesCount;
        }

        /// <summary>
        /// Processes the log entry and injects it into the database
        /// </summary>
        /// <param name="buffer">String buffer with Xml </param>
        LogEntry ProcessEntry(StringBuilder buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return null;




            try
            {
                XPathDocument xPathDoc = new XPathDocument(new System.IO.StringReader(buffer.ToString()));
                XPathNavigator navigator = xPathDoc.CreateNavigator();


                XPathNodeIterator nodeIterator = navigator.Evaluate("/logentry/@revision") as XPathNodeIterator;
                nodeIterator.MoveNext();

                /* Revision */

                int revI = nodeIterator.Current.ValueAsInt;


                LogEntry entry = new LogEntry();

                entry.RevisionNumber = revI;

                /* Author */
                nodeIterator = navigator.Evaluate("/logentry/author") as XPathNodeIterator;
                nodeIterator.MoveNext();
                entry.Account = nodeIterator.Current.TypedValue.ToString();


                /* Date */
                nodeIterator = navigator.Evaluate("/logentry/date") as XPathNodeIterator;
                nodeIterator.MoveNext();
                entry.Date = DateTime.Parse(nodeIterator.Current.TypedValue.ToString(),
                    System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces);


                /* Paths */
                nodeIterator = navigator.Evaluate("/logentry/paths/path") as XPathNodeIterator;

                /*Populate the related file paths*/
                while (nodeIterator.MoveNext())
                {
                    string actionUrl = nodeIterator.Current.TypedValue.ToString();
                    if (!string.IsNullOrEmpty(actionUrl))
                    {
                        int chagedLinesCount = GetChangedLinesCountFromEntry(revI, actionUrl);
                        entry.FileList.Add(new FileEntry { FileName = actionUrl, ChangedLinesCount = chagedLinesCount });
                    }

                }



                return entry;



            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#endif

                return null;
            }


            return null;

           

        }


        /// <summary>
        /// Gets the given working copy folder repository related Url
        /// </summary>
        /// <param name="folderPath">Folder complete working copy path on the machine</param>
        /// <returns>The url of the related repository</returns>      
        public static string GetFolderRepoUrl(string workingCopyPath)
        {
            if (string.IsNullOrEmpty(workingCopyPath))
                return null;


            bool UrlPassed = workingCopyPath.Trim().StartsWith("http://") || workingCopyPath.Trim().StartsWith("https://");


            /*If for some reason RepoBrowserConfiguration.Instance.SubversionPath is emtpy, notify error and return */
            if (string.IsNullOrEmpty(Configuration.Configuration.SubversionExeCompletePath) ||
                !System.IO.File.Exists(Configuration.Configuration.SubversionExeCompletePath))
            {

                return null;
            }

            System.Diagnostics.ProcessStartInfo psi =
                          new System.Diagnostics.ProcessStartInfo("\"" + Configuration.Configuration.SubversionExeCompletePath + "\"");

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;

            /*If the path is not Url based, means that we neeed o handle a cases when the path contains spaces*/
            if (!UrlPassed)
                psi.Arguments = " " + SvnConstants.INFO_COMMAND + " \"" + workingCopyPath + "\"";
            else
                psi.Arguments = " " + SvnConstants.INFO_COMMAND + " " + workingCopyPath;
            psi.CreateNoWindow = true;


            SvnLogProcess process = new SvnLogProcess();

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


            if (diInfoStrings.Count < 2)
            {
                if (process.ExitCode != 0)
                {
                    throw new ArgumentException("Can not get directory information from Subversion. Directory path : " + workingCopyPath);
                }
                return null;
            }


            try
            {
                for (int i = 0; i < diInfoStrings.Count; i++)
                {
                    int sepIndex = diInfoStrings[i].IndexOf(':');
                    if (sepIndex <= 0)
                        continue;

                    string value = diInfoStrings[i].Substring(sepIndex + 1);


                    /*Path*/
                    if (i == 0)
                    {

                    }

                    /*url*/
                    else if (i == 1)
                    {
                        return value;
                    }

                    /*repository root*/
                    else if (i == 2)
                    {

                    }

                    /*UUID*/
                    else if (i == 3)
                    {

                    }

                    /*Revision*/
                    else if (i == 4)
                    {
                    }

                    /*If Url passed, this is Last author*/
                    else if (i == 6 && UrlPassed)
                    {

                    }
                    else if (i == 7)
                    {
                        /*If Url NOT passed, this one os Last author*/
                        if (!UrlPassed)
                        {
                        }
                        else
                        {
                            /*Else, this one is revision number*/
                        }
                    }

                    else if (i == 8)
                    {
                        /*If url not passed, this one is Last revison number*/
                        if (!UrlPassed)
                        {

                        }
                        else
                        {
                            /*Else this one is Last change date*/
                        }
                    }

                    else if (i == 9)
                    {
                        /*If not Url passed, this one is Last change date*/
                        if (!UrlPassed)
                        {

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


            return string.Empty;
        }




        /// <summary>
        /// Inserts entry information in Data base
        /// </summary>
        /// <param name="entry"></param>
        void InsertEntry(LogEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("The {entry} object is Null. Can not insert Null entry");

            using (SQLiteConnection conn =
                new SQLiteConnection(RepositoryConnectionString))
            {
                conn.Open();

                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {

                        #region Insert data into the Revision table
                        SQLiteCommand insertRevisionCommand = new SQLiteCommand(conn);
                        insertRevisionCommand.CommandText = "Insert Into Revision (Revision,Account,Date) Values(@RevisionParam,@AccountParam,@DateParam)";
                        insertRevisionCommand.Parameters.Add(
                               new SQLiteParameter("@RevisionParam", entry.RevisionNumber));
                        insertRevisionCommand.Parameters.Add(
                            new SQLiteParameter("@AccountParam", entry.Account));

                        insertRevisionCommand.Parameters.Add(
                           new SQLiteParameter("@DateParam", entry.Date));
                        int countInserted = (int)insertRevisionCommand.ExecuteNonQuery();
                        if (countInserted <= 0)
                            throw new InvalidOperationException("Can not insert row into the Revision table.");

                        #endregion

                        /*Files injection*/
                        if (entry.FileList != null && entry.FileList.Count > 0)
                        {
                            long currentFileID = -1;
                            foreach (FileEntry fe in entry.FileList)
                            {
                                RaiseBeginProcessingFileEntry(fe);

                                #region verify if the file already exists and
                                SQLiteCommand selectCommand = new SQLiteCommand(conn);
                                selectCommand.CommandText = "Select FileID from FileDetails Where FileName=@FileNameParameter";
                                selectCommand.Parameters.Add(new SQLiteParameter("@FileNameParameter", fe.FileName));
                                object fileid = selectCommand.ExecuteScalar();
                                if (fileid == null)
                                {
                                    /*File doesn't exist*/
                                    #region Insert File into db

                                    SQLiteCommand insertCommand = new SQLiteCommand(conn);
                                    insertCommand.CommandText = "Insert into FileDetails(FileName) Values(@FileNameParam); Select Max(FileID) from FileDetails";
                                    insertCommand.Parameters.Add(
                                        new SQLiteParameter("@FileNameParam", fe.FileName));
                                   
                                    currentFileID = (long)insertCommand.ExecuteScalar();

                                    #endregion
                                }
                                else
                                    currentFileID = (long)fileid;
                                #endregion


                                #region Insert data into the Revision Details
                                SQLiteCommand inertRevisionDetailsCommand = new SQLiteCommand(conn);
                                inertRevisionDetailsCommand.CommandText = "Insert into RevisionDetails(Revision,FileID) Values(@Revision,@FileID)";
                                inertRevisionDetailsCommand.Parameters.Add(
                                       new SQLiteParameter("@Revision", entry.RevisionNumber));
                                inertRevisionDetailsCommand.Parameters.Add(
                                    new SQLiteParameter("@FileID", currentFileID));

                                countInserted = (int)inertRevisionDetailsCommand.ExecuteNonQuery();
                                if (countInserted <= 0)
                                    throw new InvalidOperationException("Can not insert row into the RevisionDetails table.");
                                #endregion


                                RaiseEndProcessingFileEntry(fe);
                            }


                            transaction.Commit();
                        }

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }


            }


        }
        #endregion

        #region event handlers
        void process_Exited(object sender, EventArgs e)
        {
            SvnLogProcess process = sender as SvnLogProcess;

            if (process != null)
            {
                /*Unsubscribe to events */
                process.ErrorDataReceived -= new DataReceivedEventHandler(process_ErrorDataReceived);
                process.OutputDataReceived -= new DataReceivedEventHandler(process_OutputDataReceived);
                process.Exited -= new EventHandler(process_Exited);
            }

            RaiseEndLoadingBaseEvent();

        }

        /// <summary>
        /// Hanldes the output from the subverison process and injects it into the DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            SvnLogProcess logProcess = sender as SvnLogProcess;

            if (string.IsNullOrEmpty(e.Data))
                return;

            /*Start poulating single logentry xml data*/
            if (e.Data.ToLowerInvariant().StartsWith("<logentry"))
            {
                logProcess.AppendXmlData(e.Data);
                logProcess.StartPopulating = true;
            }
            /*Edn of log entry for sngle item*/
            else if (e.Data.ToLowerInvariant().StartsWith("</logentry"))
            {
                /*reset state variables */

                logProcess.AppendXmlData("</logentry>");
                logProcess.StartPopulating = false;

                LogEntry entry = ProcessEntry(logProcess.GetXmlString());
                logProcess.ClearData();
                if (entry != null)
                {
                    RaiseBeginProcessingEntry(entry);

                    if (!IsEntryExists(entry))
                        InsertEntry(entry);
                    else
                    {
                        /* If the entry already registered in db, stop processing and break process,
                         * as log entries are in sorted order */
                        logProcess.Kill();
                        logProcess.Dispose();
                        logProcess = null;
                        RaiseEndProcessingEntry(entry);
                        return;


                    }

                    RaiseEndProcessingEntry(entry);
                }


            }
            else if (logProcess.StartPopulating)
            {
                logProcess.AppendXmlData(e.Data);
            }


        }


        /// <summary>
        /// Verifies if the entry already reginstered in Data base
        /// </summary>
        /// <param name="entry">LogEntry object</param>
        /// <returns>True if entry already exists, False otherwise</returns>
        bool IsEntryExists(LogEntry entry)
        {

            using (SQLiteConnection conn =
               new SQLiteConnection(RepositoryConnectionString))
            {
                conn.Open();

                SQLiteCommand selectCommand = new SQLiteCommand(conn);
                selectCommand.CommandText = "Select Revision from Revision where Revision=@RevisionParameter";
                selectCommand.Parameters.Add(new SQLiteParameter("@RevisionParameter", entry.RevisionNumber));
                object result = selectCommand.ExecuteScalar();
                if (result != null)
                    return true;
            }


            return false;
        }


        /// <summary>
        /// Handles any type error notification from the process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            int processID = -1;
            SvnLogProcess svnProcess = sender as SvnLogProcess;
            if (svnProcess != null)
                processID = svnProcess.Id;

            RaiseErrorRecieved(e.Data, processID);
        }
        #endregion

    }

}
