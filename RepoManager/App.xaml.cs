using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using SvnRadar.Util;

namespace SvnRadar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

      
        private void repoView_Click(object sender, RoutedEventArgs e)
        {
            if(e.OriginalSource is System.Windows.Controls.GridViewColumnHeader)
                AppCommands.ShowFilterOnColumnCommand.Execute(e.OriginalSource);
        }

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox)
            {
                
                if(e.Key == System.Windows.Input.Key.Escape)
                    AppCommands.RemoveFilterFromColumnCommand.Execute(sender);
                else 
                {
                    AppCommands.SetFilterOnColumnCommand.Execute(sender);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void repoView_Loaded(object sender, RoutedEventArgs e)
        {
            SvnRadar.Common.Controls.RepoTabItem.MyListView = e.OriginalSource as System.Windows.Controls.ListView;
        }


        private void repoView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
          
               
        }

        /// <summary>
        /// ListViewItem Context menu ShowRevisionInfo item click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowRevisionInfo_Click(object sender, RoutedEventArgs e)
        {
            /*For some reasons in case when, for example, the user from the configuration Tab
           * choose new repository to monitor, so the new Tab added on view. In this case the ListView 
           items collection selected item is always Null. (???)*/


            if (SvnRadar.Common.Controls.RepoTabItem.MyListView.SelectedItem == null)
            {
                //RepoManager.Common.Controls.RepoTabItem.AttachListViewToTab(sender as System.Windows.Controls.ListView);
            }

            if (SvnRadar.Common.Controls.RepoTabItem.MyListView == null)
            {
                ErrorManager.ShowCommonError("Fata error occured in application. Please restart application in order to fix the problem.", true);
                ErrorManager.LogMessage("MyListView is Null. Method: ShowRevisionInfo_Click", true);

                return;
            }

            //Show revison info to user
            AppCommands.ShowRevisionInfoCommand.Execute(sender);
        }

        private void repoView_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {

        }

        /// <summary>
        /// ListView item double click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void ListViewItemDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            //Show revison info to user
            AppCommands.ShowRevisionInfoCommand.Execute(sender);

        }

     

     
   
    }
}
