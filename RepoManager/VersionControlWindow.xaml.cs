/* VersionControlWindow.xaml.cs --------------------------------
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Resources;
using SvnRadar.Util;

namespace SvnRadar
{
    /// <summary>
    /// Interaction logic for VersionControlWindow.xaml
    /// </summary>
    public partial class VersionControlWindow : Window
    {

        string GoToPath = string.Empty;
        Version newVersion = null;
        SvnRadarExecutor svnExecutor = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="newServerVersion">The new version available on the server</param>     
        /// <param name="downloadPath">The path that should be shown on the window to download from</param>        
        public VersionControlWindow(Version newServerVersion, string downloadPath)
        {
            if (!string.IsNullOrEmpty(downloadPath))
                GoToPath = downloadPath;

            newVersion = newServerVersion;

            InitializeComponent();
        }


        SvnRadarExecutor Executor
        {
            get
            {
                if (svnExecutor == null)
                    svnExecutor =(SvnRadarExecutor)((ObjectDataProvider)AppResourceManager.FindResource("svnRadarExecutor")).ObjectInstance;

                return svnExecutor;
            }
        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            string imgTitle = ((string)AppResourceManager.FindResource("GoToTitle")) + " " + GoToPath;
          
            string infoString = AppResourceManager.FindResource("NewVersionApplicationString") as string;
            
            if(newVersion!=null)
                infoString = string.Format(infoString,newVersion.ToString());
            
            infoTextBlock.Text = infoString;
            
        }



        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void imgSvnRadar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Executor.ExecuteExplorerProcess(GoToPath);
        }

     
    }
}
