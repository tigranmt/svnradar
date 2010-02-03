using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubversionStatistics.Exceptions
{
    /// <summary>
    /// Raises when the path of the xpecified working copy is not valid
    /// </summary>
    public class InvalidWorkingCopyPath : ArgumentException
    {
        string _workingCopyPath = string.Empty;

        /// <summary>
        /// Working copy path
        /// </summary>
        public string WorkingCopyPath { get { return _workingCopyPath; } }

        public InvalidWorkingCopyPath(string workingCopyPath)
            : base("Not valid working copy path. " + workingCopyPath)
        {
            _workingCopyPath = workingCopyPath;
             
        }
    }
}
