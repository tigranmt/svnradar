/* App.xaml.cs --------------------------------
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

        private void TextBox_Changed(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == System.Windows.Input.Key.Escape)
                return;
            else
            {
               
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



        /// <summary>
        /// ListViewItem Context menu ShowRevisionInfo item click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowRevisionInfo_Click(object sender, RoutedEventArgs e)
        {          

            //Show revison info to user
            AppCommands.ShowRevisionInfoCommand.Execute(sender);
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

        private void UpdateOnlyThisFile_Click(object sender, RoutedEventArgs e)
        {
            //Show revison info to user
            AppCommands.UpdateSingleFileCommand.Execute(sender);
        }

        private void RemoveFilterButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            System.Windows.Controls.ContentPresenter cp = (btn.Parent as System.Windows.Controls.StackPanel).TemplatedParent as System.Windows.Controls.ContentPresenter;
            System.Windows.Controls.GridViewColumnHeader gvch = cp.TemplatedParent as System.Windows.Controls.GridViewColumnHeader;
            AppCommands.RemoveFilterFromColumnCommand.Execute(gvch);
        }

        private void tbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            AppCommands.SetFilterOnColumnCommand.Execute(sender);
        }

      

     

     
   
    }
}
