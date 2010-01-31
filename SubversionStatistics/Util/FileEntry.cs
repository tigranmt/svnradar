using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubversionStatistics.Util
{
    /// <summary>
    /// Provides model for the file enrty data in the single revision 
    /// </summary>
    public class FileEntry
    {
        /// <summary>
        /// File relative path in the repository
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Changed lines count in this entry
        /// </summary>
        public int ChangedLinesCount { get; set; }
    }
}
