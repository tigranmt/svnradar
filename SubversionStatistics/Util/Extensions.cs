using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubversionStatistics.Util
{
    public static class Extensions
    {
        /// <summary>
        /// Concatenates two urls ba making diff with the parameter
        /// </summary>
        /// <param name="baseString">Left operand that is the base string</param>
        /// <param name="rightUrl">Right operand where the diff have to be found</param>
        /// <returns>The resulting string if success, Empty string otherwise</returns>
        public static string ConcatUrlsDiff(this string baseString, string rightUrl)
        {
            
            if (string.IsNullOrEmpty(baseString) ||
                string.IsNullOrEmpty(rightUrl))
                return string.Empty;

            char separator = '/';
            string[] splits = rightUrl.Split(new char[] { separator });
            if (splits == null || splits.Length == 0)
                return string.Empty;

            StringBuilder resultToAttach = new StringBuilder();
            int startIndex = 0;
            foreach (string sp in splits)
            {
                startIndex = baseString.IndexOf(sp, startIndex);
                if (startIndex < 0)
                {
                    resultToAttach.Append(separator + sp);
                    startIndex = 0;
                }

            }


            return baseString +  resultToAttach.ToString();
        }
    }
}
