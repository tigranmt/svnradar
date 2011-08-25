/* CommandStringsManager.cs --------------------------------
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

namespace SvnObjects.Objects
{
    public static class CommandStringsManager
    {
        #region Svn Command costants
        /// <summary>
        /// Recovers the status of the repository folder
        /// </summary>
        const string WC_STATUS_COMMAND = "wc_status";
        const string REPO_STATUS_COMMAND = "repo_status";
        const string REPO_LOG_COMMAND = "repo_log";
        const string COMMON_STATUS_COMMAND = "status";
        const string REVISION_INFO_COMMAND = "revision_info";
        const string UPDATE_COMMAND = "update";
        const string COMMON_LOG_COMMAND = "log";
        const string COMMON_DIFF_COMMAND = "diff";
        const string COMMON_INFO_COMMAND = "info";
        const string COMMON_UPDATE_COMMAND = "update";

        #endregion

        #region working copy statuc command 
        public static string Wc_StatusCommand 
        {
            get { return WC_STATUS_COMMAND; }
        }

        public static bool IsWcStatusCommand(string commandString) 
        {
            if(string.IsNullOrEmpty(commandString)) 
                return false;
            return commandString.Equals(WC_STATUS_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion


        #region repository status command 
        public static string Repo_StatusCommand
        {
            get
            {
                return REPO_STATUS_COMMAND;
            }
        }

        public static bool IsRepoStatusCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString)) 
                return false;

            return commandString.Equals(REPO_STATUS_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion 


        #region repository log command 
        public static string Repo_LogCommand
        {
            get
            {
                return REPO_LOG_COMMAND;
            }
        }

        public static bool IsRepoLogCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return false;

            return commandString.Equals(REPO_LOG_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion 


        #region common status command
        public static string CommonStatusCommand
        {
            get
            {
                return COMMON_STATUS_COMMAND;
            }
        }

        public static bool IsCommonStatusCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return false;

            return commandString.Equals(COMMON_STATUS_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion 


        
        #region revision info command
        public static string RevisionInfoCommand
        {
            get
            {
                return REVISION_INFO_COMMAND;
            }
        }

        public static bool IsRevisionInfoCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return false;

            return commandString.Equals(REVISION_INFO_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion 



        #region update command
        public static string UpdateCommand
        {
            get
            {
                return UPDATE_COMMAND;
            }
        }

        public static bool IsUpdateCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return false;

            return commandString.Equals(UPDATE_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion 


        #region common log command
        public static string CommonLogCommand
        {
            get
            {
                return COMMON_LOG_COMMAND;
            }
        }

        public static bool IsCommonLogCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return false;

            return commandString.Equals(COMMON_LOG_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion 


        #region common diff command
        public static string CommonDiffCommand
        {
            get
            {
                return COMMON_DIFF_COMMAND;
            }
        }

        public static bool IsCommonDiffCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return false;

            return commandString.Equals(COMMON_DIFF_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion 

        #region common info command
        public static string CommonInfoCommand
        {
            get
            {
                return COMMON_INFO_COMMAND;
            }
        }

        public static bool IsCommonInfoCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return false;

            return commandString.Equals(COMMON_INFO_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion 

        #region common update command
        public static string CommonUpdateCommand
        {
            get
            {
                return COMMON_UPDATE_COMMAND;
            }
        }

        public static bool IsCommonUpdateCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return false;

            return commandString.Equals(COMMON_UPDATE_COMMAND, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion 
    }
}
