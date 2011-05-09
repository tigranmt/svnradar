/* RepoInfo.cs --------------------------------
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
using System.Windows.Documents;
using SvnRadar.Util;
using SvnObjects;

namespace SvnRadar.Util
{
    /// <summary>
    /// Holds the complete repository informatino about given item.
    /// This class is directly finish on UI, as there is no any ViewModel implementation. As the
    /// data of a single record read from repository is constant and there is no way that it could be changed, so there
    /// is no any kind of "interactivity" between this object and UI.
    /// </summary>
    public class RepoInfo : RepositoryInfo
    {

        /// <summary>
        /// Holds selected repository information
        /// </summary>
        static RepoInfo selectedRepoInfo = null;

        /// <summary>
        /// Is an object is a selecte itam in ListView
        /// </summary>
        bool isSelected = false;



        /// <summary>
        /// If the  data populating on the object starts
        /// </summary>
        internal bool StartPopulating = false;





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


        public string StateDescription
        {
            get
            {
                return StateDescriptionFromEnum(this.ItemState);

            }
        }

        public string State
        {
            get { return ItemState.ToString(); }
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


        /// <summary>
        /// Returns human readable string dscription of the state of the current item
        /// </summary>
        public static string StateDescriptionFromEnum(RepositoryInfo.RepositoryItemState repostate)
        {
            if (repostate == RepositoryItemState.Add)
                return AppResourceManager.FindResource("ADD_STR") as string;
            else if (repostate == RepositoryItemState.Conflict)
                return AppResourceManager.FindResource("CONFLICT_STR") as string;
            else if (repostate == RepositoryItemState.Deleted)
                return AppResourceManager.FindResource("DELETED_STR") as string;
            else if (repostate == RepositoryItemState.ExternalDefinition)
                return AppResourceManager.FindResource("EXTERNALDEF_STR") as string;
            else if (repostate == RepositoryItemState.Ignored)
                return AppResourceManager.FindResource("IGNORED_STR") as string;
            else if (repostate == RepositoryItemState.Missing)
                return AppResourceManager.FindResource("MISSING_STR") as string;
            else if (repostate == RepositoryItemState.Modified)
                return AppResourceManager.FindResource("MODIFIED_STR") as string;
            else if (repostate == RepositoryItemState.Merged)
                return AppResourceManager.FindResource("MERGED_STR") as string;
            else if (repostate == RepositoryItemState.NeedToBeUpdatedFromRepo)
                return AppResourceManager.FindResource("NEEDTOBEUPDATED_STR") as string;
            else if (repostate == RepositoryItemState.Normal)
                return AppResourceManager.FindResource("NORMAL_STR") as string;
            else if (repostate == RepositoryItemState.NotVersioned)
                return AppResourceManager.FindResource("NOTVERSIONED_STR") as string;
            else if (repostate == RepositoryItemState.Replaced)
                return AppResourceManager.FindResource("REPLACED_STR") as string;
            else
                return AppResourceManager.FindResource("DIFFERENTKIND_STR") as string;
        }




        /// <summary>
        /// Returns the RepoItemState enumeration member parsed from the char symbol
        /// </summary>
        /// <param name="stateChar">The car that should be parsed</param>
        /// <returns>RepoInfo.RepoItemState enumeration member result</returns>
        public static RepoInfo.RepositoryItemState StateFromChar(char stateChar)
        {
            if (stateChar == ' ')
            {
                /*No changes on item*/
                return RepoInfo.RepositoryItemState.Normal;
            }
            else if (stateChar == 'A')
            {
                /*Item is scheduled for Addition*/
                return RepoInfo.RepositoryItemState.Add;
            }
            else if (stateChar == 'D')
            {
                /*Item is scheduled for Deletion.*/
                return RepoInfo.RepositoryItemState.Deleted;
            }
            else if (stateChar == 'M' || stateChar == 'U')
            {
                /*Item has been modified.*/
                return RepoInfo.RepositoryItemState.Modified;
            }
            else if (stateChar == 'R')
            {
                /*Item has been replaced in your working copy. 
                 * This means the file was scheduled for deletion, and then a new file with the same name was scheduled for addition in its place.*/
                return RepoInfo.RepositoryItemState.Replaced;
            }
            else if (stateChar == 'G')
            {
                /*Item has been replaced in your working copy. 
                 * This means the file was scheduled for deletion, and then a new file with the same name was scheduled for addition in its place.*/
                return RepoInfo.RepositoryItemState.Merged;
            }
            else if (stateChar == 'C')
            {
                /*The contents (as opposed to the properties) of the item conflict 
                 * with updates received from the repository.
                    */
                return RepoInfo.RepositoryItemState.Conflict;
            }
            else if (stateChar == 'X')
            {
                /*Item is related to an externals definition.*/
                return RepoInfo.RepositoryItemState.ExternalDefinition;
            }
            else if (stateChar == 'I')
            {
                /*Item is being ignored (e.g. with the svn:ignore property).*/
                return RepoInfo.RepositoryItemState.Ignored;
            }
            else if (stateChar == '?')
            {
                /*Item is not under version control.*/
                return RepoInfo.RepositoryItemState.NotVersioned;
            }
            else if (stateChar == '!')
            {
                /*Item is missing (e.g. you moved or deleted it without using svn). 
                 * This also indicates that a directory is incomplete (a checkout or update was interrupted).
                */
                return RepoInfo.RepositoryItemState.Missing;
            }
            else if (stateChar == '~')
            {
                /*Item is versioned as one kind of object (file, directory, link), 
                 * but has been replaced by different kind of object.
                    */
                return RepoInfo.RepositoryItemState.VersionedWithDifferentKindOfObject;
            }
            else if (stateChar == '*')
            {
                /*Item was changed on repository
                    */
                return RepoInfo.RepositoryItemState.NeedToBeUpdatedFromRepo;
            }

            return RepoInfo.RepositoryItemState.Normal;

        }

    }
}
