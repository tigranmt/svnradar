using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RepoManager.Util
{
    /// <summary>
    /// Holds information about single row in repository update command output tracing. 
    /// </summary>
    public struct UpdateTraceRow
    {
        /// <summary>
        /// The description of the action applied to the specified item.
        /// </summary>
        public string Action {get;set;}

        /// <summary>
        /// The description of the item to be updated
        /// </summary>
        public string Item{get;set;}

        /// <summary>
        /// Repository item state
        /// </summary>
        public RepoInfo.RepoItemState RepositoryItemState { get; set; }
    }
}
