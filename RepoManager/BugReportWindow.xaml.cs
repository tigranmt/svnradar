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
using System.Net.Mail;
using SvnRadar.Util;

namespace SvnRadar
{
    /// <summary>
    /// Interaction logic for BugReportWindow.xaml
    /// </summary>
    public partial class BugReportWindow : Window
    {
        const string toEmailAddress = "tigranmt@gmail.com";
        public BugReportWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles report button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReportBug_Click(object sender, RoutedEventArgs e)
        {
            BindingExpression be = txtContent.GetBindingExpression(TextBox.TextProperty);
            if (be.HasError)
            {
                string appExcetionString = AppResourceManager.FindResource("MSG_MESSAGEMANDATORY") as string;
                MessageBox.Show(appExcetionString, Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            be = txtReporterName.GetBindingExpression(TextBox.TextProperty);
            if (be.HasError)
            {
                string appExcetionString = AppResourceManager.FindResource("MSG_REPORTERNAMEMANDATORY") as string;
                MessageBox.Show(appExcetionString, Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

           // MailMessage message = new MailMessage(txtReporterName.Text,toEmailAddress,
        }

        /// <summary>
        /// Hanldes Close button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //Close the window
            this.Close();
        }

        private void txtContent_KeyUp(object sender, KeyEventArgs e)
        {
            BindingExpression be = txtContent.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }

       
    }
}
