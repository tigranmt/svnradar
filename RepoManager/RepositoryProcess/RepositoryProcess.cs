using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SvnObjects.Objects;
using SvnRadar.Util;

namespace SvnRadar
{
    public class RepositoryProcess : SubversionRepositoryProcess
    {
        /// <summary>
        /// Holds single objectfor population Log command output data population
        /// </summary>
        internal RepoInfo repoLogInformation = null;


        /// <summary>
        /// When the updated requested over the property will be hold the Window object that will be trace the all output 
        /// comming from the standart output of the update command execution.
        /// </summary>
        public UpdateTraceWindow UpdateTraceWindowObject = null;

    }
}
