using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SvnRadar.Util;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;

namespace SvnRadar.DataBase
{
    

    #region delegates for handling the method calls in multithreading envirounment
    public delegate void AddRevisionInfoDelegate(string repoName, int revisionNum, string itemName,string dateTime, string madeChanges);
    public delegate void AddRepoListInfoDelegate(string repoName, List<RepoInfo> revision);
    public delegate void AddRepoInfoDelegate(string repoName, RepoInfo repo);
    public delegate void AddUpdateTraceInfo(string updateTraceString);
    #endregion

    /// <summary>
    /// Holds tha data base of application all available repositories information 
    /// </summary>
    public  sealed class RepoInfoBase
    {

        #region fields
        /// <summary>
        /// defines the number of the revision to ignore when try to add to the collection
        /// </summary>
        const short REV_NUM_TO_IGNORE = -1;


        private static  FilterManager filterManager = null;


        /// <summary>
        /// Global object of ObjectDataProvider 
        /// </summary>
        private static ObjectDataProvider provider = null;

        /// <summary>
        /// Global object for retrieving the count of the found object
        /// </summary>
        private static ObjectDataProvider countProvider = null;

        /// <summary>
        /// Dictionary for holding the repo information. 
        /// Key: RepoName-the name of repository 
        /// Value: List<RepoINfo>-list of available repository informations
        /// </summary>
        static Dictionary<string, ObservableCollection<RepoInfo>> DataBase = 
            new Dictionary<string, ObservableCollection<RepoInfo>>(StringComparer.InvariantCultureIgnoreCase);


        /// <summary>
        /// Holds the all necessary informatin for every requested Revision
        /// Key: Repository name
        /// Value: List of  information for every requested revision number
        /// </summary>
        static Dictionary<string, ObservableCollection<RevisionInfo>> RevisionInfoBase = new Dictionary<string, ObservableCollection<RevisionInfo>>();

        #endregion

        #region properties

        /// <summary>
        /// Returns global object data provider
        /// </summary>
        static ObjectDataProvider DataProvider
        {
            get
            {
                if (provider == null)
                    provider = FindResource("DataProviderForListView") as ObjectDataProvider;
                return provider;
            }
        }


        /// <summary>
        /// Returns global object data provider
        /// </summary>
        static ObjectDataProvider CountDataProvider
        {
            get
            {
                if (countProvider == null)
                    countProvider = FindResource("GetCurrentRepoInfoCount") as ObjectDataProvider;
                return countProvider;
            }
        }


        /// <summary>
        /// Retrives the FilterManager object from the Application resources
        /// </summary>
        /// <returns>Returns FilterManager object</returns>
        static FilterManager FilterManager
        {
            get
            {
                return filterManager ??
                    (filterManager = (FilterManager)((ObjectDataProvider)FindResource("filterManager")).ObjectInstance);
            }
        }

        #endregion

        #region member functions

        #region add functions
        /// <summary>
        /// Add row to database
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <param name="repo">RepoInfo object to add to the relational list</param>
        public static void AddRepoInfo(string repoName,RepoInfo repo) 
        {
            if (repo == null ||
                repo.Revision == REV_NUM_TO_IGNORE)
                return;

            ObservableCollection<RepoInfo> list = null;

            if (DataBase.TryGetValue(repoName, out list))
            {
                list.Add(repo);
            }
            else
            {
                list = new ObservableCollection<RepoInfo>();
                list.Add(repo);
                DataBase.Add(repoName, list);
            }

            
            DataProvider.Refresh();
        }

        /// <summary>
        /// Add row to database
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <param name="repoList">List of RepoInfo objects to add to the relational list</param>
        public static void AddRepoInfoList(string repoName, List<RepoInfo> repoList)
        {
            if (string.IsNullOrEmpty(repoName))
                return;

            if (repoList == null || repoList.Count == 0)
                return;


            foreach (RepoInfo rInfo in repoList)
                AddRepoInfo(repoName, rInfo);
        }


        /// <summary>
        /// Add or updates, if already exists, revision info in the  base
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <param name="revisonNumber">Revison number</param>
        /// <param name="itemName">Item name string </param>
        /// <param name="dateStr">DateTime string </param>
        /// <param name="diffString">Diff string</param>            
        /// <returns>Revision info object that was added or updated</returns>
        public static RevisionInfo  AddRevisionInfoString(string repoName, int revisonNumber, string itemName, string dateStr,string diffString)
        {
            if (string.IsNullOrEmpty(repoName))
                return null;

            if (revisonNumber< 0)
                return null;

            ObservableCollection<RevisionInfo> list = null;
            if (RevisionInfoBase.TryGetValue(repoName, out list))
            {
                //We didn't find any revison information about this revision number so add it to base
                RevisionInfo revi = list.FirstOrDefault((x) => x.Revision == revisonNumber);
                if (revi == null)
                {
                    revi = new RevisionInfo();
                    revi.Revision = revisonNumber;
                    revi.Date = dateStr;
                    revi.Item = itemName;
                    list.Add(revi);
                }

                revi.TextChanged = diffString;
                return revi;
            }
            else
            {
                list = new ObservableCollection<RevisionInfo>();
                list.Add(new RevisionInfo { Revision = revisonNumber, Date= dateStr, Item = itemName, TextChanged=diffString });
                RevisionInfoBase.Add(repoName, list);

                return list[list.Count - 1];
            }

        }
        #endregion

        #region remove functions
        /// <summary>
        /// Removes all available information about the revison  from the base
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <param name="revisonNumber">Revision number</param>
        public static void RemoveRevisonInfoStringFromBase(string repoName, int revisonNumber)
        {
            /*Locate the list of the revison information related to the specified repository*/
            ObservableCollection<RevisionInfo> list = null;
            if (RevisionInfoBase.TryGetValue(repoName, out list))
            {
                /*Removes all available revison informatin with the specified repository number equal to specified one*/
                IEnumerable<RevisionInfo> revi = list.Where((x) => x.Revision == revisonNumber);

                foreach (RevisionInfo r in revi.ToArray())
                {
                    r.ClearData();
                    list.Remove(r);
                }
            }
        }


        /// <summary>
        /// Clears repository relative information from the database
        /// </summary>
        /// <param name="repoName">Repository name</param>      
        public static void ClearRepoInfo(string repoName)
        {
            ObservableCollection<RepoInfo> list = null;
            if (DataBase.TryGetValue(repoName, out list))
                list.Clear();

            GC.Collect();
            GC.WaitForFullGCComplete();

        }
        #endregion

        #region get functions
        /// <summary>
        /// Return count of available information for the given repository
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <returns>The count of the records available for the given repositiory.</returns>
        public static int GetRepoInfoCount(string repoName)
        {
            ObservableCollection<RepoInfo> list = null;
            if (DataBase.TryGetValue(repoName, out list))
                return list.Count;

            return 0;
        }


        /// <summary>
        /// Return count of available information for the current repository
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <returns>The count of the records available for the given repositiory.</returns>
        public static int GetCurrentRepoInfoCount()
        {
            if (RepoBrowserWindow.SelectedRepoTabItem == null)
                return 0;

            ObservableCollection<RepoInfo> list = null;
            if (DataBase.TryGetValue(RepoBrowserWindow.SelectedRepoTabItem.RepositoryName, out list))
                return list.Count;

            return 0;
        }



        /// <summary>
        /// Return RepoInfo object about the available record for the given repository's revision number.
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <param name="repositoryRevisionNumber">Repository revision number</param>
        /// <returns>RepoInfo object if there is. Null otherwise.</returns>
        public static RepoInfo GetRepoInfoFromRepoRevision(string repoName, int repositoryRevisionNumber)
        {
            ObservableCollection<RepoInfo> list = null;
            if (!DataBase.TryGetValue(repoName, out list))
                return null;

            return list.First(rObject => rObject.Revision == repositoryRevisionNumber);
        
        }


        /// <summary>
        /// Returns  revision number present at the moment for the specified repository at speified index
        /// </summary>
        /// <param name="repoName">Repository name </param>
        /// <param name="index">Index of the element to be retrieved from the repository's collection </param>
        /// <returns>The number of the last found revision in the specified repository</returns>
        public static int GetRevisionNumberAtIndex(string repoName, int index)
        {
            if (index < 0)
                return -1;

            ObservableCollection<RepoInfo> list = null;
            if (!DataBase.TryGetValue(repoName, out list))
                return -1;

            if (list == null || list.Count == 0)
                return -1;

            if (index >= list.Count)
                return -1;

            return list[index].Revision;
        }




        /// <summary>
        /// Return RepoInfo object about the available record for the given working copy's revision number.
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <param name="repositoryRevisionNumber">Working copy revision number</param>
        /// <returns>RepoINfo object if there is. Null otherwise.</returns>
        public static RepoInfo GetWcInfoFromRepoRevision(string repoName, int workingCopyRevisionNumber)
        {
            ObservableCollection<RepoInfo> list = null;
            if (!DataBase.TryGetValue(repoName, out list))
                return null;

            return list.First(rObject => rObject.Revision == workingCopyRevisionNumber);

        }



        /// <summary>
        /// Returns a list of the objects associated to the given repository
        /// </summary>
        /// <param name="repositoryName">Repository name</param>
        /// <returns>The observable collection of the RepoInfo objects</returns>
        public static ObservableCollection<RepoInfo> GetRepoInfoList(string repositoryName)
        {

            const string NULL_PARAMETER = "Empty";

            if(string.IsNullOrEmpty(repositoryName.Trim()) ||
                repositoryName.Equals(NULL_PARAMETER, StringComparison.InvariantCultureIgnoreCase))
                return new ObservableCollection<RepoInfo>();

            ObservableCollection<RepoInfo> list = null;
            if (!DataBase.TryGetValue(repositoryName, out list))
            {
                list = new ObservableCollection<RepoInfo>();
                DataBase.Add(repositoryName, list);               
            }

            if (FilterManager.HasFiltersApplied)
            {

                IEnumerable<RepoInfo> filtered = FilterList(repositoryName, list);
                try
                {
                    if (filtered != null)
                        return new ObservableCollection<RepoInfo>(filtered);
                }
                catch (NullReferenceException nulRefEx)
                {
                    return new ObservableCollection<RepoInfo>(list);
                }
            }
            
            return list;

        }


        #endregion

        #region filter functions

        /// <summary>
        /// Filters the given repository content
        /// </summary>
        /// <param name="RepositoryName">Repository name on which content the ffilter must be executed</param>
        /// <param name="list">The initial content of the repository</param>       
        /// <returns>Returns the filterd content of the repository, if any filter exists, otherwise the original list will be returned.</returns>
        static IEnumerable<RepoInfo> FilterList(string RepositoryName, IEnumerable<RepoInfo> list) 
        {
            if (string.IsNullOrEmpty(RepositoryName))
                return null;

            if (list == null)
                return null;

            IEnumerable<FilterCriteria> criteriaEnum = FilterManager.FindAllFilterCriterias(RepositoryName);

            if (criteriaEnum == null || criteriaEnum.Count() == 0)
                return list;


            return list.Where(criteriaEnum);

        }
        #endregion

        #region resource navigators
        /// <summary>
        /// Finds the requested resource from the application's resource dictionary
        /// </summary>
        /// <param name="esourceKey">Resource key</param>
        /// <returns>Resource object if exists, Null otherwise</returns>
        static object FindResource(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey))
                return null;

            return Application.Current.FindResource(resourceKey);
        }

        #endregion

        #endregion

    }

}
