using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvnRadar.Util
{
    /// <summary>
    /// Holds repository inormation for given folder. Information like URL, LastRevision number and so on.
    /// </summary>
    public sealed class FolderRepoInfo
    {

        #region fields
        public string wellFormatedDate = string.Empty;
        #endregion

        #region properties
        /// <summary>
        /// Folder working copy complete path on the machine 
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// Relative path of the folder in repository tree
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The Url that points to a location on the repositiory server
        /// </summary>
        public string Url { get; set; }


        /// <summary>
        /// Repository root Url
        /// </summary>
        public string RepositoryRoot { get; set; }


        /// <summary>
        /// Given repository uniques identifier
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// Bigest revision number of the repositiory present in the given folder
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// Author name of the last change on the given directory
        /// </summary>
        public string LastAuthor { get; set; }

        /// <summary>
        /// Lat revison changed number present in the given working copy folder
        /// </summary>
        public int LastRevisionNumber { get; set; }

        /// <summary>
        /// Last revision log
        /// </summary>
        public string LastRevisionLog { get; set; }

        /// <summary>
        /// Last revison user comment
        /// </summary>
        public string LastRevisionComment { get; set; }


        /// <summary>
        /// Last chage date in the given folder
        /// </summary>
        public string LastChangeDate
        {
            get
            {
                return wellFormatedDate;
            }
            set
            {
                DateTime result;
                if (DateTime.TryParse(value, out result))
                {
                    wellFormatedDate = result.ToLongDateString() + " : " + result.ToLongTimeString();
                }
                else
                    wellFormatedDate = value;

            }
        }



        /// <summary>
        /// Repository relative URL path
        /// </summary>
        public string RepoRelativeUrl
        {
            get
            {
                try
                {
                    return this.Url.Substring(this.Url.IndexOf(this.RepositoryRoot) + this.RepositoryRoot.Length);

                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion

    }
}
