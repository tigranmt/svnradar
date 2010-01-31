using System;
using System.Collections.Generic;
namespace SubversionStatistics.Util
{
    /// <summary>
    /// Provides model for the single subversion's log entry
    /// </summary>
    public class LogEntry
    {

        public List<FileEntry> fileList = new List<FileEntry>();

        /// <summary>
        /// Revision number of the log entry 
        /// </summary>
        public int RevisionNumber { get; set; }

        /// <summary>
        /// Account name of the log entry
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// The date of the registration of the log entry 
        /// </summary>
        public DateTime Date { get; set; }


        public List<FileEntry> FileList
        {
            get
            {
                return fileList;
            }
        }


    }
}