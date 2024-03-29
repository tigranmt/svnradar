﻿/* RepoInfo.cs --------------------------------
 * 
 * * Copyright (c) 2009 Tigran Martirosyan 
 * * Contact and Information: tigranmt@gmail.com 
 * * This application is free software; you can redistribute it and/or 
 * * Modify it under the terms of the GPL license
 * * THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, 
 * * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
 * * OTHER DEALINGS IN THE SOFTWARE. 
 * * THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE 
 *  */
// ---------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SvnObjects.Objects
{
    /// <summary>
    /// Holds the complete repository informatino about given item.
    /// This class is directly finish on UI, as there is no any ViewModel implementation. As the
    /// data of a single record read from repository is constant and there is no way that it could be changed, so there
    /// is no any kind of "interactivity" between this object and UI.
    /// </summary>
    public class RepositoryInfo : ICloneable
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


        //Working copy revision number
        int wcRevision = -1;

        //Repository revison number
        int repoRevision = -1;


        /// <summary>
        /// String of the record DateTime
        /// </summary>
        string strDate = string.Empty;



        /// <summary>
        /// Lines count in the log comment
        /// </summary>
        internal int logLinesCount = -1;

        /// <summary>
        /// String contains XML output stream of the command
        /// </summary>
        public StringBuilder xmlData = new StringBuilder();
   

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
        public enum RepositoryItemState : int { Normal = 0, Modified, Deleted, Add, Replaced, Conflict, ExternalDefinition, Ignored, NotVersioned, Missing, VersionedWithDifferentKindOfObject, NeedToBeUpdatedFromRepo, Merged };

        /// <summary>
        /// Repo item name
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Repository item state
        /// </summary>
        public RepositoryItemState ItemState { get; set; }
        #endregion


        #region properties
      

        /// <summary>
        /// Working copy item state
        /// </summary>
        public RepositoryItemState WcItemState { get; set; }

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
        /// Returns the RepoItemState enumeration member parsed from the char symbol
        /// </summary>
        /// <param name="stateChar">The car that should be parsed</param>
        /// <returns>RepoInfo.RepoItemState enumeration member result</returns>
        public static RepositoryInfo.RepositoryItemState StateFromChar(char stateChar)
        {
            if (stateChar == ' ')
            {
                /*No changes on item*/
                return RepositoryInfo.RepositoryItemState.Normal;
            }
            else if (stateChar == 'A')
            {
                /*Item is scheduled for Addition*/
                return RepositoryInfo.RepositoryItemState.Add;
            }
            else if (stateChar == 'D')
            {
                /*Item is scheduled for Deletion.*/
                return RepositoryInfo.RepositoryItemState.Deleted;
            }
            else if (stateChar == 'M' || stateChar == 'U')
            {
                /*Item has been modified.*/
                return RepositoryInfo.RepositoryItemState.Modified;
            }
            else if (stateChar == 'R')
            {
                /*Item has been replaced in your working copy. 
                 * This means the file was scheduled for deletion, and then a new file with the same name was scheduled for addition in its place.*/
                return RepositoryInfo.RepositoryItemState.Replaced;
            }
            else if (stateChar == 'G')
            {
                /*Item has been replaced in your working copy. 
                 * This means the file was scheduled for deletion, and then a new file with the same name was scheduled for addition in its place.*/
                return RepositoryInfo.RepositoryItemState.Merged;
            }
            else if (stateChar == 'C')
            {
                /*The contents (as opposed to the properties) of the item conflict 
                 * with updates received from the repository.
                    */
                return RepositoryInfo.RepositoryItemState.Conflict;
            }
            else if (stateChar == 'X')
            {
                /*Item is related to an externals definition.*/
                return RepositoryInfo.RepositoryItemState.ExternalDefinition;
            }
            else if (stateChar == 'I')
            {
                /*Item is being ignored (e.g. with the svn:ignore property).*/
                return RepositoryInfo.RepositoryItemState.Ignored;
            }
            else if (stateChar == '?')
            {
                /*Item is not under version control.*/
                return RepositoryInfo.RepositoryItemState.NotVersioned;
            }
            else if (stateChar == '!')
            {
                /*Item is missing (e.g. you moved or deleted it without using svn). 
                 * This also indicates that a directory is incomplete (a checkout or update was interrupted).
                */
                return RepositoryInfo.RepositoryItemState.Missing;
            }
            else if (stateChar == '~')
            {
                /*Item is versioned as one kind of object (file, directory, link), 
                 * but has been replaced by different kind of object.
                    */
                return RepositoryInfo.RepositoryItemState.VersionedWithDifferentKindOfObject;
            }
            else if (stateChar == '*')
            {
                /*Item was changed on repository
                    */
                return RepositoryInfo.RepositoryItemState.NeedToBeUpdatedFromRepo;
            }

            return RepositoryInfo.RepositoryItemState.Normal;

        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new RepositoryInfo
            {
                Revision = this.Revision,
                Item = this.Item,
                Date = this.Date,
                Account = this.Account,
                ItemDomain = this.ItemDomain,
                ItemState = this.ItemState,
                WcItemState = this.WcItemState,
                WCRevision = this.WCRevision
            };
        }

        #endregion

        #region overrides
        public override string ToString()
        {
            return this.ItemState + " " + Item;
        }
        #endregion
    }
}
