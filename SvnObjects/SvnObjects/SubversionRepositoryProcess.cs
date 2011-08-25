using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SvnObjects;
using SvnObjects.Objects;

namespace SvnObjects.Objects
{
    /// <summary>
    /// Repo process extends Process class in order to hold more properties for any type of command execution
    /// </summary>
    public class SubversionRepositoryProcess : Process
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
        /// data string of the datat recieved form the standart outptu and not recognized like any valid command by application
        /// </summary>
        public StringBuilder notRecognizedData = new StringBuilder();


    }
}
