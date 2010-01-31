using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubversionStatistics.Util
{
    /// <summary>
    /// Subversion basic commands and parameters constants
    /// </summary>
    public static class SvnConstants
    {
        /// <summary>
        /// Subversion log command parameter
        /// </summary>
        public static readonly string LOG_COMMAND = "log";


        /// <summary>
        /// Subversion log command parameters (-v --xml)
        /// </summary>
        public static readonly string LOG_COMMAND_PARAMETERS = " -v --xml";

        /// <summary>
        /// Subversion info command parameter
        /// </summary>
        public static readonly string INFO_COMMAND = "info";

        /// <summary>
        /// Subversion Diff command paramater
        /// </summary>
        public static readonly string DIFF_COMMAND = "diff";


        /// <summary>
        /// Subversion List command paramater
        /// </summary>
        public static readonly string LIST_COMMAND = "list";


        
    }
}
