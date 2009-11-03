using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SvnRadar
{
    public class AppCommands
    {
        /// <summary>
        /// Command applies specified alias to specified tab on TabControl.
        /// </summary>
        public static readonly ICommand SetAliasOnTabCommand = new RoutedCommand("SetAliasOnTabCommand", typeof(AppCommands));

        /// <summary>
        /// Command removes specified tab from view and internal model collection 
        /// </summary>
        public static readonly ICommand RemoveTabCommand = new RoutedCommand("RemoveTabCommand", typeof(AppCommands));

        /// <summary>
        /// Command for retrieving workign copy log information
        /// </summary>
        public static readonly ICommand GetWorkingCopyInfoCommand = new RoutedCommand("GetWorkingCopyInfoCommand", typeof(AppCommands));

        /// <summary>
        /// Commnd for retriveing repository log information
        /// </summary>
        public static readonly ICommand GetRepoInfoCommand = new RoutedCommand("GetRepoInfoCommand", typeof(AppCommands));

        /// <summary>
        /// Command shows filter textbox the filter on the specified column header of ListView
        /// </summary>
        public static readonly ICommand ShowFilterOnColumnCommand = new RoutedCommand("ShowFilterOnColumnCommand", typeof(AppCommands));

        /// <summary>
        /// Command applies the filter on the specified column header of ListView
        /// </summary>
        public static readonly ICommand SetFilterOnColumnCommand = new RoutedCommand("SetFilterOnColumnCommand", typeof(AppCommands));

        /// <summary>
        /// Command removes any filter applied from the specified column on ListView column header
        /// </summary>
        public static readonly ICommand RemoveFilterFromColumnCommand = new RoutedCommand("RemoveFilterFromColumnCommand", typeof(AppCommands));

        /// <summary>
        /// Command that shows detaild information about specified item
        /// </summary>
        public static readonly ICommand ShowRevisionInfoCommand = new RoutedCommand("ShowRevisionInfoCommand", typeof(AppCommands));
        
        /// <summary>
        /// Command that updtes only sleected item from repository 
        /// </summary>
        public static readonly ICommand UpdateSelectedCommand = new RoutedCommand("UpdateSelectedCommand", typeof(AppCommands));

        /// <summary>
        /// Command that updates entire repositry
        /// </summary>
        public static readonly ICommand UpdateRepositoryCommand = new RoutedCommand("UpdateRepositoryCommand", typeof(AppCommands));

        /// <summary>
        /// Commands that groups list view content by the revison number
        /// </summary>
        public static readonly ICommand GroupByRevisionNumberCommand = new RoutedCommand("GroupByRevisionNumberCommand", typeof(AppCommands));

        /// <summary>
        /// Command thta shows recovered information in simple flat GridView mode
        /// </summary>
        public static readonly ICommand FlatViewCommand = new RoutedCommand("FlatViewCommand", typeof(AppCommands));
        
    }


}

