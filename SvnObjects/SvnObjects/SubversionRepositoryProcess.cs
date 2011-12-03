/* SunbversionRepositoryProcess.cs --------------------------------
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
using System.Diagnostics;

using SvnObjects;
using SvnObjects.Objects;

namespace SvnObjects.Objects
{
    /// <summary>
    /// Repo process extends Process class in order to hold more properties for any type of command execution
    /// </summary>
    public class SubversionRepositoryProcess : Process
    {
        /// <summary>
        /// Holds the Nam eof the repository that the process is going to check
        /// </summary>
        public string RelatedRepositoryName { get; set; }

        /// <summary>
        /// Command which was executed with given process
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// In case when the command is related to the single revision, 
        /// this property holds the Revision number inforation of which the process is going to get.
        /// </summary>
        public int RevisionNumber { get; set; }


        /// <summary>
        /// In case when the command is related to the single File, 
        /// this property holds the File name inforation of which the process is going to get.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Folder working copy information
        /// </summary>
        public FolderRepoInfo FolderRepoInformation { get; set; }


      



        /// <summary>
        /// data string of the datat recieved form the standart outptu and not recognized like any valid command by application
        /// </summary>
        public StringBuilder notRecognizedData = new StringBuilder();


    }
}
