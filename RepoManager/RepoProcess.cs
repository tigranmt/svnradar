using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using RepoManager.Util;

namespace RepoManager
{
    /// <summary>
    /// Repo process extends Process class in order to hold more properties for any type of command execution
    /// </summary>
    public class RepositoryProcess : Process
    {
        /// <summary>
        /// Holds the Nam eof the repository that the process is going to check
        /// </summary>
        public string RelatedRepositoryName { get; set; }

        /// <summary>
        /// Command which was executed with given process
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// In case when the command is related to the single revision, 
        /// this property holds the Revision number inforation of which the process is going to get.
        /// </summary>
        public int RevisionNumber { get; set; }


        /// <summary>
        /// In case when the command is related to the single File, 
        /// this property holds the File name inforation of which the process is going to get.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Folder working copy information
        /// </summary>
        public FolderRepoInfo FolderRepoInformation { get; set; }


        /// <summary>
        /// Holds single objectfor population Log command output data population
        /// </summary>
        internal RepoInfo repoLogInformation = null;


        /// <summary>
        /// When the updated requested over the property will be hold the Window object that will be trace the all output 
        /// comming from the standart output of the update command execution.
        /// </summary>
        public UpdateTraceWindow UpdateTraceWindowObject = null;


        /// <summary>
        /// data string of the datat recieved form the standart outptu and not recognized like any valid command by application
        /// </summary>
        public StringBuilder notRecognizedData = new StringBuilder();





    }
}
