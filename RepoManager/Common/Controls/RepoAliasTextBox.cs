/* RepoAliasTextBox.cs --------------------------------
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
using System.Windows.Controls;

namespace SvnRadar.Common.Controls
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
        /// <summary>
        /// Handles OnKey down event
        /// </summary>
        /// <param name="e"></param>
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
