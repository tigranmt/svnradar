/* ExtensionMethods.cs --------------------------------
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
                    filteredCollection = filteredCollection.Where(p => repoInfoType.InvokeMember(filter.ColumnName, BindingFlags.GetProperty, null, p, null).ToString().ToLowerInvariant().Contains(filter.Filter.ToLowerInvariant()));
                }

                return filteredCollection;
            }
            return sourceCollection;
        }
    }
}
