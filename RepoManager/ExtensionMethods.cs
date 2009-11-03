using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using SvnRadar.Util;
using System.Linq.Expressions;
using System.Reflection;

namespace SvnRadar
{
    /// <summary>
    /// Exetnsion methods class
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Extension method. Force binding templates to update ther's state, as the column's header vary.
        /// </summary>
        /// <param name="col">GridViewColumn object which header bindings need to be updated</param>
        public static void UpdateColumnHeaderBindings(this GridViewColumn col) 
        {
            string header = col.Header.ToString();
            col.ClearValue(GridViewColumn.HeaderProperty);
            col.SetValue(GridViewColumn.HeaderProperty, header);
        }



        public static IEnumerable<RepoInfo> Where(this IEnumerable<RepoInfo> sourceCollection, IEnumerable<FilterCriteria> filters)
        {
          

            IQueryable<RepoInfo> filteredCollection = sourceCollection.AsQueryable();
            Type repoInfoType = typeof(RepoInfo);
            if (filters != null)
            {
                foreach (FilterCriteria filter in filters)
                {
                    filteredCollection = filteredCollection.Where(p => repoInfoType.InvokeMember(filter.ColumnName, BindingFlags.GetProperty, null, p, null).ToString().StartsWith(filter.Filter));
                }

                return filteredCollection;
            }
            return sourceCollection;
        }
    }
}
