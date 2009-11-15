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
using SvnRadar.Util;

namespace SvnRadar
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {

        private SvnRadarExecutor svnExecutor = null;

        public AboutWindow()
        {
            InitializeComponent();
        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            tbVersionString.Text = VersionInfo.AssemblyVersion;          
            tbAuthorString.Text = VersionInfo.Author;
                
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        SvnRadarExecutor Executor
        {
            get
            {
                if (svnExecutor == null)
                    svnExecutor = (SvnRadarExecutor)((ObjectDataProvider)AppResourceManager.FindResource("svnRadarExecutor")).ObjectInstance;

                return svnExecutor;
            }
        }

        private void image2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Executor.ExecuteExplorerProcess(VersionInfo.AuthorSite);      
        }

        private void image3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Executor.ExecuteExplorerProcess(VersionInfo.ProjectHttpAddress);               
        }
    }
}
