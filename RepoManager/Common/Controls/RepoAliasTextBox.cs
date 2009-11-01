using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace RepoManager.Common.Controls
{
    internal sealed class RepoAliasTextBox : TextBox
    {

        #region fields

        /// <summary>
        /// Default text that will appear on text box
        /// </summary>
        string DEFAULT_TEXT = "Set alias for Tab";

        /// <summary>
        /// Parent RepoTabItem object
        /// </summary>
        RepoTabItem parentTabItem = null;


        /// <summary>
        /// The original content of RepoTabItem header
        /// </summary>
        static object headerOriginalContent = null;
        #endregion

        #region member functions
        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="repoTabItem">Parent RepoTabItem object where the text box should appear</param>
        private RepoAliasTextBox(RepoTabItem repoTabItem)
        {
            parentTabItem = repoTabItem;
            this.Text = DEFAULT_TEXT;
        }



        /// <summary>
        /// Attahces the text box to the header of the given RepoTabItem
        /// </summary>
        /// <param name="repoTabItem">RepoTabItem object to attach TextBox object to</param>
        public static void AttachToRepoTabItem(RepoTabItem repoTabItem)
        {
            /*Save original content of TabItem header*/
            headerOriginalContent = repoTabItem.Header;
            repoTabItem.Header = new RepoAliasTextBox(repoTabItem);
        }
        #endregion

        #region overrides 
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            /*If user pressed Escape, return to the RepoTabItem's 
             * Header property it's original content value
             */
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                parentTabItem.Header = headerOriginalContent;                
            }
             /*If user pressed Enter, set the RepoTabItem's Header 
              * property value to the TextBox text. If it's Null reste the Heaedr content
              * to it's original value
              */
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                if(!string.IsNullOrEmpty( this.Text))
                    parentTabItem.Header = this.Text;
                else
                    parentTabItem.Header = headerOriginalContent;   
            }
        }
        #endregion
    }
}
