using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SvnRadar
{

    /// <summary>
    /// Holds the information of a single filter criteria applied on the ListView column
    /// </summary>
    public class FilterCriteria
    {
        /// <summary>
        /// Repository name, thow the TabItem header name
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// Column name where the filter was applied
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// The filter string itself
        /// </summary>
        public string Filter { get; set; }


        /// <summary>
        /// Verifies object's validity
        /// </summary>
        /// <param name="criteria">FilterCriteria object that should be validated</param>
        /// <returns>True if the content of the object is valid, False otherwise</returns>
        public static bool IsValid(FilterCriteria criteria)
        {
            if (string.IsNullOrEmpty(criteria.RepositoryName) ||
                string.IsNullOrEmpty(criteria.ColumnName))
                return false;

            return true;
        }


        public bool EqualsIgnoreFilter(object obj)
        {
            if (obj is FilterCriteria == false)
                return false;

            FilterCriteria fc = (FilterCriteria)obj;
            if (!FilterCriteria.IsValid(fc) &&
                !FilterCriteria.IsValid(this))
                return true;


            if (fc.RepositoryName.Equals(this.RepositoryName, StringComparison.InvariantCultureIgnoreCase) &&
                fc.ColumnName.Equals(this.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                return true;


            return false;
        }



        #region object overrides
        public override bool Equals(object obj)
        {
            if (obj is FilterCriteria == false)
                return false;

            FilterCriteria fc = (FilterCriteria)obj;
            if (!FilterCriteria.IsValid(fc) &&
                !FilterCriteria.IsValid(this))
                return true;


            if (fc.RepositoryName.Equals(this.RepositoryName, StringComparison.InvariantCultureIgnoreCase) &&
                fc.ColumnName.Equals(this.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                return true;


            return false;
        }


        public override string ToString()
        {
            if (!IsValid(this))
                return base.ToString();

            return this.RepositoryName + ":" + this.ColumnName + ":" + this.Filter;

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

    }


    /// <summary>
    /// Manages all filters applied to the columns of ListView available in application
    /// </summary>
    public class FilterManager
    {
        /// <summary>
        /// Collects all filters availbale in the application
        /// </summary>
        LinkedList<FilterCriteria> filterCriteriaList = new LinkedList<FilterCriteria>();

     


        /// <summary>
        /// Verifies on existance of the filter in the given repository on the given column
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <param name="columnName">Column name</param>
        /// <returns>True if filter exists, False otherwise</returns>
        public bool ExistsFilterFor(string repoName, string columnName)
        {
            FilterCriteria fcToFind = new FilterCriteria { RepositoryName = repoName, ColumnName = columnName };

            fcToFind = Find(fcToFind, true);
            if (FilterCriteria.IsValid(fcToFind))
                return true;
            else
                return false;
        }

        #region Find
        /// <summary>
        /// Executes search over the available collection in order to find object that has equal properties to the given one
        /// </summary>
        /// <param name="criteria">FilterCriteria object that is the base of the search </param>        
        /// <returns>Returns FilterCriteria. In case if the object doesn't exist the execution of the static function IsValid will return False, thow not valid object description</returns>
        public FilterCriteria Find(FilterCriteria criteria)
        {
            return Find(criteria, false);
        }

        /// <summary>
        /// Executes search over the available collection in order to find object that has equal properties to the given one
        /// </summary>
        /// <param name="criteria">FilterCriteria object that is the base of the search </param>
        /// <param name="ignoreFilter">If False the Equality on Filter property will not be verified </param>
        /// <returns>Returns FilterCriteria. In case if the object doesn't exist the execution of the static function IsValid will return False, thow not valid object description</returns>
        public FilterCriteria Find(FilterCriteria criteria, bool ignoreFilter)
        {
            if (filterCriteriaList == null || filterCriteriaList.Count == 0)
                //Return empty struct
                return new FilterCriteria();

            foreach (FilterCriteria fc in filterCriteriaList)
            {
                if (!ignoreFilter && fc.Equals(criteria))
                    return fc;
                else if (ignoreFilter && fc.EqualsIgnoreFilter(criteria))
                    return fc;
                   
            }

            //Return empty struct
            return new FilterCriteria();
        }

        /// <summary>
        /// Determines if there is some filters applied to collection
        /// </summary>
        public bool HasFiltersApplied
        {
            get {                
                if (filterCriteriaList == null || filterCriteriaList.Count == 0)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Finds all available filter criterias applied to the given repository content
        /// </summary>
        /// <param name="repositoryName">Repository name</param>
        /// <returns>The list of found FilterCriteria object on the given repository, Null otherwise.</returns>
        public IEnumerable<FilterCriteria> FindAllFilterCriterias(string repositoryName)
        {
            if (string.IsNullOrEmpty(repositoryName))
                return null;

            if (filterCriteriaList == null || filterCriteriaList.Count == 0)
                return null;

            return filterCriteriaList.Where((x)=>x.RepositoryName.Equals(repositoryName,StringComparison.InvariantCultureIgnoreCase));

          
        }
        #endregion


        #region Add methods
        /// <summary>
        /// Adds requested filet to the collection if the same doesn't exist
        /// </summary>
        /// <param name="criteria">FilterCriteria object that should be add to the collection</param>
        /// <returns>True if add success, False otherwise</returns>
        public bool AddFilter(FilterCriteria criteria)
        {
            if (!FilterCriteria.IsValid(criteria))
            {
                throw new ArgumentException("Not all properties of the object criteria are valid for insertion in collection");
            }


            FilterCriteria fc = Find(criteria);
            if (!FilterCriteria.IsValid(fc))
                return false;


            return AddFilterToColumn(criteria.RepositoryName, criteria.ColumnName, criteria.Filter);

        }

        /// <summary>
        /// Adds requested filter to the column
        /// </summary>
        /// <param name="repoName">Repository name on which the filter should exist</param>
        /// <param name="columnName">Repository's column name on which filter should be applied</param>
        /// <param name="filter">the filter string</param>
        /// <returns>True if add was success, False otherwise.</returns>
        public bool AddFilterToColumn(string repoName, string columnName, string filter)
        {
            FilterCriteria fc = new FilterCriteria { RepositoryName = repoName, ColumnName = columnName, Filter = filter };
            if (!FilterCriteria.IsValid(fc))
                return false;
           
            filterCriteriaList.AddLast(fc);
            return true;

        }
        #endregion 

        #region Remove methods 
        /// <summary>
        /// Removes requested filet from the collection
        /// </summary>
        /// <param name="repositoryName">Repository name</param>
        /// <param name="columnName">Column name</param>
        /// <returns>True if remove success, False otherwise</returns>
        public bool RemoveFilter(string repositoryName, string columnName)
        {
            return RemoveFilter(new FilterCriteria { RepositoryName = repositoryName, ColumnName = columnName });
        }

        /// <summary>
        /// Removes requested filet from the collection
        /// </summary>
        /// <param name="criteria">FilterCriteria object that should be removed from the collection</param>        
        /// <returns>True if remove success, False otherwise</returns>
        public bool RemoveFilter(FilterCriteria criteria)
        {
            if (!FilterCriteria.IsValid(criteria))
            {
                throw new ArgumentException("Not all properties of the object criteria are valid for insertion in collection");

            }



            FilterCriteria fc = Find(criteria);
            if (!FilterCriteria.IsValid(fc))
                return false;

            filterCriteriaList.Remove(fc);

            return true;         

        }

       
        #endregion 


        #region Update methods
        /// <summary>
        /// Updates the given column filter in the given repository with desired string
        /// </summary>
        /// <param name="repoName">Repository name</param>
        /// <param name="columnName">Column name</param>
        /// <param name="newFilter">New filter string to be applied</param>
        /// <returns>True if the filter was found and updated, False otherwise</returns>
        public bool UpdateFilterOnColumn(string repoName, string columnName, string newFilter)
        {
           FilterCriteria fc = Find(new FilterCriteria { RepositoryName = repoName, ColumnName = columnName }, true);
           if(FilterCriteria.IsValid(fc))  
           {
               fc.Filter = newFilter;
               return true;
           }

           return false;
        }
        #endregion
    }

}
