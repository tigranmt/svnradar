/* FolderRepoInfoFactory.cs --------------------------------
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
    public static class FolderRepoInfoFactory
    {
        /// <summary>
        /// Base for folder relational information . Key: working copy complete path, Value: relative Folder info object
        /// </summary>
        static Dictionary<string, FolderRepoInfo> folderRepoInfoBase = new Dictionary<string, FolderRepoInfo>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Returns FolderRepoInfo object associated to the specified folder. If there is no one, new object will be created and associated to the specified path
        /// </summary>
        /// <param name="repositoryConpletePath">Repository complete path</param>
        /// <returns>FolderRepoInfo object</returns>
        public static FolderRepoInfo GetFolderRepoObject(string repositoryConpletePath)
        {
            FolderRepoInfo frepo = null;

            if (!folderRepoInfoBase.TryGetValue(repositoryConpletePath, out frepo))
            {
                frepo = new FolderRepoInfo();
                folderRepoInfoBase.Add(repositoryConpletePath, frepo);
            }

            return frepo;
        }

    }
}
