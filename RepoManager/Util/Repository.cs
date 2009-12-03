using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvnRadar.Util
{
    public sealed class Repository
    {
        public sealed class RepositoryCredentials {
            public string UserName {get;set;}
            public string Password {get;set;}
        }

        /// <summary>
        /// The repository wotking copy complete path
        /// </summary>
        public string RepositoryCompletePath { get; set; }
        public RepositoryCredentials RepoCredentials = null;
        
    }
}
