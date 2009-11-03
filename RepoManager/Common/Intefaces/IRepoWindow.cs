using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvnRadar.Common.Intefaces
{
    /// <summary>
    /// Common interface that every window in application that handles the repository command output
    ///is implement
    /// </summary>
    public interface IRepoWindow
    {
        /// <summary>
        /// Process object associated to window
        /// </summary>
        RepositoryProcess Process { get; set; }

        /// <summary>
        /// Related repository name
        /// </summary>
        string RelatedRepositoryName { get; set; }


        /// <summary>
        /// Related command executed ober repository
        /// </summary>
        string RelatedCommand { get; set; }

        /// <summary>
        /// Notifies the widnow abou relative process exit
        /// </summary>
        void ProcessExited();


    }
}
