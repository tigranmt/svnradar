using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using SvnRadar.Util;

namespace SvnRadar.Util
{
    /// <summary>
    /// Holds the complete repository informatino about given item.
    /// This class is directly finish on UI, as there is no any ViewModel implementation. As the
    /// data of a single record read from repository is constant and there is no way that it could be changed, so there
    /// is no any kind of "interactivity" between this object and UI.
    /// </summary>
    public class RepoInfo : ICloneable
    {

        #region base user comments static dictionary
        /// <summary>
        /// Static table for holding all user omments for all available revisions, as on the view we show the information to 
        /// the user in flat format. Every ro in on the view correspodn to a single item, so user will find multiple times same revision
        /// number with change on different item, but as the user comment is relative to revision, to not waste the memory for multiple
        /// rows putting same comment, we put them in the single shared central location. Look at UserComment property to see ho the value is recovered.
        /// 
        /// </summary>
        static Dictionary<int, string> revNumVsUserCommentDic = new Dictionary<int, string>();
        #endregion


        #region fields
        /// <summary>
        /// String contains XML output stream of the command
        /// </summary>
        internal StringBuilder xmlData = new StringBuilder();


        //Working copy revision number
        int wcRevision = -1;

        //Repository revison number
        int repoRevision = -1;

        /// <summary>
        /// Defines enumeration of the possible domains of the item.
        /// </summary>
        public enum Domain { WorkingCopy, Repository }

        /// <summary>
        /// Defines domain of the item
        /// </summary>
        public Domain ItemDomain = Domain.WorkingCopy;

        /// <summary>
        /// Enumertaion of the possible states of the Svn item.
        /// </summary>
        public enum RepoItemState : int { Normal = 0, Modified, Deleted, Add, Replaced, Conflict, ExternalDefinition, Ignored, NotVersioned, Missing, VersionedWithDifferentKindOfObject, NeedToBeUpdatedFromRepo, Merged };

        /// <summary>
        /// Repo item name
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Repository item state
        /// </summary>
        public RepoItemState RepositoryItemState { get; set; }


        /// <summary>
        /// String of the record DateTime
        /// </summary>
        string strDate = string.Empty;


        /// <summary>
        /// Is an object is a selecte itam in ListView
        /// </summary>
        bool isSelected = false;


        /// <summary>
        /// If the  data populating on the object starts
        /// </summary>
        internal bool StartPopulating = false;


        /// <summary>
        /// Lines count in the log comment
        /// </summary>
        internal int logLinesCount = -1;


        static RepoInfo selectedRepoInfo = null;

        #endregion


        #region properties
        public string StateDescription
        {
            get
            {
                return StateDescriptionFromEnum(this.RepositoryItemState);

            }
        }

        public string State
        {
            get { return RepositoryItemState.ToString(); }
        }


        /// <summary>
        /// Working copy item state
        /// </summary>
        public RepoItemState WcItemState { get; set; }

        /// <summary>
        /// Working copy revision number
        /// </summary>
        public int WCRevision
        {
            get { return wcRevision; }
            set { wcRevision = value; }
        }

        /// <summary>
        /// Repositiry revision number
        /// </summary>
        public int Revision
        {
            get { return repoRevision; }
            set { repoRevision = value; }
        }

        /// <summary>
        /// Account name 
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// User comment
        /// </summary>
        public string UserComment
        {
            get
            {
                if (revNumVsUserCommentDic.ContainsKey(this.Revision))
                {
                    return revNumVsUserCommentDic[this.Revision];
                }
                return string.Empty;
            }
            set
            {
                if (this.Revision < 0)
                    throw new ArgumentException("Can not set comment on revision, without associated number. Set furst the number of revision.");

                // revNumVsUserCommentDic[this.Revision] = value.Replace(".", System.Environment.NewLine);
                revNumVsUserCommentDic[this.Revision] = value;
            }
        }



        /// <summary>
        /// Change date time
        /// </summary>
        public string Date
        {
            get { return strDate; }
            set
            {


                DateTime dtResult;
                if (DateTime.TryParse(value, out dtResult))
                    strDate = dtResult.ToLongDateString() + "    " + dtResult.ToLongTimeString();
            }
        }

        /// <summary>
        /// Handles via data binding the item selection event
        /// </summary>
        public bool IsSelectedInfo
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                if (isSelected)
                {
                    if (selectedRepoInfo != null)
                        selectedRepoInfo.IsSelectedInfo = false;
                    selectedRepoInfo = this;
                }
            }
        }

        /// <summary>
        /// Returns current selected item's object 
        /// </summary>
        public static RepoInfo SelectedInfo
        {
            get
            {
                return selectedRepoInfo;
            }
        }
        #endregion

        #region methods
        /// <summary>
        /// Retruns revision related user comment from the global base
        /// </summary>
        /// <param name="revNumber">Revision number</param>
        /// <returns>User comment assigned to the specified revision, otherwise null</returns>
        public static string GetRevisionUserComment(int revNumber)
        {
            string userComment = null;
            revNumVsUserCommentDic.TryGetValue(revNumber, out userComment);
            return userComment;
        }


        /// <summary>
        /// Returns human readable string dscription of the state of the current item
        /// </summary>
        public static string StateDescriptionFromEnum(SvnRadar.Util.RepoInfo.RepoItemState repostate)
        {
            if (repostate == RepoItemState.Add)
                return AppResourceManager.FindResource("ADD_STR") as string;
            else if (repostate == RepoItemState.Conflict)
                return AppResourceManager.FindResource("CONFLICT_STR") as string;
            else if (repostate == RepoItemState.Deleted)
                return AppResourceManager.FindResource("DELETED_STR") as string;
            else if (repostate == RepoItemState.ExternalDefinition)
                return AppResourceManager.FindResource("EXTERNALDEF_STR") as string;
            else if (repostate == RepoItemState.Ignored)
                return AppResourceManager.FindResource("IGNORED_STR") as string;
            else if (repostate == RepoItemState.Missing)
                return AppResourceManager.FindResource("MISSING_STR") as string;
            else if (repostate == RepoItemState.Modified)
                return AppResourceManager.FindResource("MODIFIED_STR") as string;
             else if (repostate == RepoItemState.Merged)
                return AppResourceManager.FindResource("MERGED_STR") as string;                
            else if (repostate == RepoItemState.NeedToBeUpdatedFromRepo)
                return AppResourceManager.FindResource("NEEDTOBEUPDATED_STR") as string;
            else if (repostate == RepoItemState.Normal)
                return AppResourceManager.FindResource("NORMAL_STR") as string;
            else if (repostate == RepoItemState.NotVersioned)
                return AppResourceManager.FindResource("NOTVERSIONED_STR") as string;
            else if (repostate == RepoItemState.Replaced)
                return AppResourceManager.FindResource("REPLACED_STR") as string;
            else
                return AppResourceManager.FindResource("DIFFERENTKIND_STR") as string;
        }




        /// <summary>
        /// Returns the RepoItemState enumeration member parsed from the char symbol
        /// </summary>
        /// <param name="stateChar">The car that should be parsed</param>
        /// <returns>RepoInfo.RepoItemState enumeration member result</returns>
        public static RepoInfo.RepoItemState StateFromChar(char stateChar)
        {
            if (stateChar == ' ')
            {
                /*No changes on item*/
                return RepoInfo.RepoItemState.Normal;
            }
            else if (stateChar == 'A')
            {
                /*Item is scheduled for Addition*/
                return RepoInfo.RepoItemState.Add;
            }
            else if (stateChar == 'D')
            {
                /*Item is scheduled for Deletion.*/
                return RepoInfo.RepoItemState.Deleted;
            }
            else if (stateChar == 'M' || stateChar == 'U')
            {
                /*Item has been modified.*/
                return RepoInfo.RepoItemState.Modified;
            }
            else if (stateChar == 'R')
            {
                /*Item has been replaced in your working copy. 
                 * This means the file was scheduled for deletion, and then a new file with the same name was scheduled for addition in its place.*/
                return RepoInfo.RepoItemState.Replaced;
            }
            else if (stateChar == 'G')
            {
                /*Item has been replaced in your working copy. 
                 * This means the file was scheduled for deletion, and then a new file with the same name was scheduled for addition in its place.*/
                return RepoInfo.RepoItemState.Merged;
            }
            else if (stateChar == 'C')
            {
                /*The contents (as opposed to the properties) of the item conflict 
                 * with updates received from the repository.
                    */
                return RepoInfo.RepoItemState.Conflict;
            }
            else if (stateChar == 'X')
            {
                /*Item is related to an externals definition.*/
                return RepoInfo.RepoItemState.ExternalDefinition;
            }
            else if (stateChar == 'I')
            {
                /*Item is being ignored (e.g. with the svn:ignore property).*/
                return RepoInfo.RepoItemState.Ignored;
            }
            else if (stateChar == '?')
            {
                /*Item is not under version control.*/
                return RepoInfo.RepoItemState.NotVersioned;
            }
            else if (stateChar == '!')
            {
                /*Item is missing (e.g. you moved or deleted it without using svn). 
                 * This also indicates that a directory is incomplete (a checkout or update was interrupted).
                */
                return RepoInfo.RepoItemState.Missing;
            }
            else if (stateChar == '~')
            {
                /*Item is versioned as one kind of object (file, directory, link), 
                 * but has been replaced by different kind of object.
                    */
                return RepoInfo.RepoItemState.VersionedWithDifferentKindOfObject;
            }
            else if (stateChar == '*')
            {
                /*Item was changed on repository
                    */
                return RepoInfo.RepoItemState.NeedToBeUpdatedFromRepo;
            }

            return RepoInfo.RepoItemState.Normal;

        }


        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new RepoInfo
            {
                Revision = this.Revision,
                Item = this.Item,
                Date = this.Date,
                Account = this.Account,
                ItemDomain = this.ItemDomain,
                RepositoryItemState = this.RepositoryItemState,
                WcItemState = this.WcItemState,
                WCRevision = this.WCRevision
            };
        }

        #endregion

        #region overrides
        public override string ToString()
        {
            return this.RepositoryItemState + " " + Item;
        }
        #endregion
    }
}
