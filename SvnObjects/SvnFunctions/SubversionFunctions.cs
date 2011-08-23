using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SvnObjects.Objects;
using System.Xml.XPath;

namespace SvnObjects.SvnFunctions
{
    public sealed class SubversionFunctions
    {
        /// <summary>
        /// Holds a complete path to the Subversion executable
        /// </summary>
        string sSubverisionPath = null;


        /// <summary>
        /// Holds last error message, if there is any
        /// </summary>
        public string LastErrorMessage { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sSubversionExeCompletePath">Pass complete path to the subverison executable file</param>
        public SubversionFunctions(string sSubversionExeCompletePath)
        {
            sSubverisionPath = sSubversionExeCompletePath;
        }



        /// <summary>
        /// Gets the given working copy folder repository related information
        /// </summary>
        /// <param name="folderPath">Folder complete working copy path on the machine</param>
        /// <returns>FolderRepoInfo object, Null otherwise</returns>      
        public FolderRepoInfo GetFolderRepoInfo(string folderPath, int milliseconds)
        {
            if (string.IsNullOrEmpty(folderPath))
                return null;


            /* As we get the information from the local copy, there is no any web access so this call, should be relatively 
               fast to execute, for this reason, in order to no add another level of complexity, we execte this command on the main UI
               blocking the program, usually for some seconds. */

            bool UrlPassed = folderPath.Trim().StartsWith("http://") || folderPath.Trim().StartsWith("https://");


            /*If for some reason sSubverisionPath is emtpy, notify error and return */
            if (string.IsNullOrEmpty(sSubverisionPath) ||
                !System.IO.File.Exists(sSubverisionPath))
            {
                LastErrorMessage = "The subversion exe path is missed. Can not execute command";
                return null;
            }

            System.Diagnostics.ProcessStartInfo psi =
                          new System.Diagnostics.ProcessStartInfo("\"" + sSubverisionPath + "\"");

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;

            /*Url based, or not */
            if (!UrlPassed)
                psi.Arguments = " " + CommandStringsManager.CommonInfoCommand + " \"" + folderPath.Trim() + "\"";
            else
                psi.Arguments = " " + CommandStringsManager.CommonInfoCommand + " \"" + folderPath.Trim() + "\"";
            psi.CreateNoWindow = true;

            SubversionRepositoryProcess process = new SubversionRepositoryProcess();

            process.StartInfo = psi;
            process.EnableRaisingEvents = true;



            /* Do not use Stack, for example, because we need to begins the scan of the results from the begining, in order 
             if in the future, the output of this command will be changed, that probabbly will be added some new  information 
             * on end, we by the way are able to get all neccessary for us information */
            List<string> diInfoStrings = new List<string>();


            process.ErrorDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {

            };

            process.Exited += delegate(object sender, EventArgs e)
            {

            };

            process.OutputDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                diInfoStrings.Add(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();

            /*Wait maximum 1 minute*/
            if (milliseconds > 0)
                process.WaitForExit(milliseconds);
            else
                process.WaitForExit();
          
            if (diInfoStrings.Count < 2)
            {
                if (process.ExitCode != 0)
                {
                           LastErrorMessage =  "Got problems retreiving  information for the folder " +
                         folderPath + "ErrorCode: " + process.ExitCode.ToString() + " " + process.StandardError.ReadToEnd();
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
        /// Parser the XML string into the list of RepoInfo objects
        /// </summary>
        /// <param name="strOutputLine">Xml string</param>
        /// <param name="foldeRepoInfo">If not Null, only revisions major then majorRevisionPresent will retrieved, 
        /// otherwise the entire log history will be dumped into the list. </param>
        /// <param name="skipLowerRevisions">If true the revisions lower then the specified by foldeRepoInfo object will be skipped</param> 
        /// <param name="breakProcessing">If true process break it's execution</param> 
        /// <returns></returns>
        List<RepositoryInfo> ProcessRepoLogCommandOutputLine(string strOutputLine, FolderRepoInfo foldeRepoInfo, bool skipLowerRevisions, out bool breakProcessing)
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


            List<RepositoryInfo> repoInfoList = new List<RepositoryInfo>();

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


                RepositoryInfo singleRow = new RepositoryInfo();
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

                    RepositoryInfo current = null;
                    if (count == 0)
                    {
                        /*If first row assign the parent object reference*/
                        current = singleRow;
                    }
                    else
                    {
                        /*If this is not the first row, clonethe parent object and create a new one*/
                        current = singleRow.Clone() as RepositoryInfo;
                    }
                    count++;

                    /*get action related path*/
                    string actionUrl = nodeIterator.Current.TypedValue.ToString();

                    //load all files, do not skip anything 
                    /*Skip the paths that are not in the specified directory URL*/
                    //if (!string.IsNullOrEmpty(relativeUrl))
                    //    if (!actionUrl.StartsWith(relativeUrl))
                    //        continue;


                    /*If path is ok, continue populating object data*/

                    current.Item = actionUrl;
                    current.ItemDomain = RepositoryInfo.Domain.Repository;


                    string action = nodeIterator.Current.GetAttribute("action", string.Empty);
                    if (!string.IsNullOrEmpty(action))
                        current.ItemState = RepositoryInfo.StateFromChar(action[0]);

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
        /// Get the log for the given repository. Blocking call, not async.
        /// </summary>       
        /// <param name="folderRepoInfo">FolderrepoInfo object, if Null no params will specified on command execution</param>   
       /// <param name="iFromRevision">Specifiy revision number whom log have to be found, or pass -1 to get everything</param>
       /// <returns></returns>   
        public List<RepositoryInfo> GetRepositoryLogImmediate(FolderRepoInfo folderRepoInfo, int iFromRevision)
        {
            if (string.IsNullOrEmpty(sSubverisionPath))
                return null;


            List<RepositoryInfo> result = new List<RepositoryInfo>();

            /*Svn special parameters for requesting the repository log*/
            string repoStatusRequestParams = " ";

            /*If object is not Null, pass Url like a parameter to reconver server side log of the repository*/
            if (folderRepoInfo != null) {
                if (iFromRevision>0)
                    repoStatusRequestParams += folderRepoInfo.Url + " -r " + iFromRevision.ToString() + " -v --xml";
                else
                    repoStatusRequestParams += folderRepoInfo.Url + " -v --xml";
            }

         
            StringBuilder unrecognizedData = new StringBuilder();
            System.Diagnostics.ProcessStartInfo psi =
                          new System.Diagnostics.ProcessStartInfo(sSubverisionPath);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.Arguments = " " + CommandStringsManager.CommonLogCommand + " " + repoStatusRequestParams;
            psi.CreateNoWindow = true;

            SubversionRepositoryProcess process = new SubversionRepositoryProcess();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;


            /* Do not use Stack, for example, because we need to begin the scan of the results from the begining, in case, 
             if in the future, the output of this command will be changed, that probably will be added some new  information 
             * on end, we by the way are able to get all neccessary for us information */
            StringBuilder xmlLogStrings = new StringBuilder();


            process.ErrorDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {
                if(!string.IsNullOrEmpty(e.Data )) {
                    LastErrorMessage = e.Data;                    
                }
            };

            process.OutputDataReceived += delegate(object sender, System.Diagnostics.DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                if (e.Data.StartsWith("<logentry"))
                    xmlLogStrings.Append(e.Data);
                else if (e.Data.Contains("</logentry"))
                {                  
                    int endIndex = e.Data.IndexOf("</logentry");
                    xmlLogStrings.Append(e.Data.Substring(0, endIndex) + "</logentry>");
                    bool breakPorcessing = false;
                    result.AddRange(ProcessRepoLogCommandOutputLine(xmlLogStrings.ToString(), folderRepoInfo, false, out breakPorcessing));
                    xmlLogStrings.Clear();
                    
                    //TEST ONLY!
                    return;
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



            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();

            return result;



        }


    }
    
}
