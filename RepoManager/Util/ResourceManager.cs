using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvnRadar.Util
{
    public static class AppResourceManager
    {

        /// <summary>
        /// Finds the requested resource from the application's resource dictionary
        /// </summary>
        /// <param name="esourceKey">Resource key</param>
        /// <returns>Resource object if exists, Null otherwise</returns>
        public static object FindResource(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey))
                return null;

            return System.Windows.Application.Current.FindResource(resourceKey);
        }

    }
}
