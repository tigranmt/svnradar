using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvnRadar.Util
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
