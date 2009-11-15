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
            imgSvnRadar.ToolTip = imgTitle;

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
