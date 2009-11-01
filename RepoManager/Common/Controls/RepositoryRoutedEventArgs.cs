using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace RepoManager.Common.Controls
{
    public class RepositoryRoutedEventArgs  :RoutedEventArgs
    {
        /// <summary>
        /// Repository complete Url
        /// </summary>
        public string RepositoryUrl { get; set; }
    }
}
