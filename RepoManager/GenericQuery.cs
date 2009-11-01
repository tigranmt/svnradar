using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace RepoManager
{
    public static class GenericQuery
    {
        /// <summary>
        /// Creates generic filter expresson collection of FilterCriteria objects
        /// </summary>
        public static Expression<Func<TInput, bool>> CreateFilterExpression<TInput>(IEnumerable<FilterCriteria> filters)
        {
            /* TInput is a collction on which the call will be executed */

            ParameterExpression parameter = Expression.Parameter(typeof(TInput),"");
            Expression lambdaExpressionBody = null;
            if (filters != null)
            {
                foreach (FilterCriteria fc in filters)
                {
                    
                    Expression compareExpression =
                        Expression.Equal (Expression.Property(parameter, fc.ColumnName),
                                        Expression.Constant(fc.Filter));

                    if (lambdaExpressionBody == null)
                        lambdaExpressionBody = compareExpression;
                    else
                        lambdaExpressionBody = Expression.Or(lambdaExpressionBody, compareExpression);
                }
            }

            if (lambdaExpressionBody == null)
                return Expression.Lambda<Func<TInput, bool>>(Expression.Constant(false));
            else
                return Expression.Lambda<Func<TInput, bool>>(lambdaExpressionBody, parameter);


            


        }
    }
}
