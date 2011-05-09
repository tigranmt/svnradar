using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvnObjects.Objects
{
   
    /// <summary>
    /// Holds information about single revision
    /// </summary>
    public  class Revision
    {

        /// <summary>
        /// Date of revision
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Revision number
        /// </summary>
        public int RevisionNumber { get; set; }
        

        /// <summary>
        /// File changed by this revision
        /// </summary>
        public string Item { get; set; }


        /// <summary>
        /// Retruns the User comment related to the current revison
        /// </summary>
        public string UserComment
        {
            get { return RepositoryInfo.GetRevisionUserComment(this.RevisionNumber); }
        }



    }
}
