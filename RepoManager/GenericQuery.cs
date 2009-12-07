/* GenericQuery.cs --------------------------------
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
using System.Linq.Expressions;

namespace SvnRadar
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
