/* FolderDialog.cs --------------------------------
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
using System.Windows.Forms;

namespace SvnRadar.Util
{
    /// <summary>
    /// Folder browser dialog wrapper class
    /// </summary>
    public static class FolderDialog
    {
        static FolderBrowserDialog browserDialog = new FolderBrowserDialog();

        /// <summary>
        /// Last selected path by the user
        /// </summary>
        static string selectedpath = string.Empty;

        /// <summary>
        /// If TRUE last selected location by the user will be selected like a default one after dialog is opened.
        /// False otherwise.
        /// </summary>
        static bool maintainLastSelectedLocation = true;

        /// <summary>
        /// If TRUE last selected location by the user will be selected like a default one after dialog is opened.
        /// False otherwise.
        /// </summary>
        public static bool MaintainLastSelectedLocation
        {
            get { return maintainLastSelectedLocation; }
            set { maintainLastSelectedLocation = value; }
        }

        /// <summary>
        /// Last selected path by the user
        /// </summary>
        public static string SelectedPath
        {
            get { return selectedpath; }
        }

        /// <summary>
        /// Shows folder slector system dialog
        /// </summary>
        /// <returns></returns>
        public static DialogResult ShowDialog()
        {
            if (maintainLastSelectedLocation)
                browserDialog.SelectedPath = selectedpath;

            DialogResult result = browserDialog.ShowDialog();

           
            if (result == DialogResult.OK)
                selectedpath = browserDialog.SelectedPath;
            return result;
        }
    }
}
