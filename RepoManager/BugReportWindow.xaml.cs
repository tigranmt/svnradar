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
            be.UpdateSource();
            if (be.HasError)
            {
                string appExcetionString = AppResourceManager.FindResource("MSG_MESSAGEMANDATORY") as string;
                MessageBox.Show(appExcetionString, Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
                return;
            }

            be = txtReporterName.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            if (be.HasError)
            {
                string appExcetionString = AppResourceManager.FindResource("MSG_REPORTERNAMEMANDATORY") as string;
                MessageBox.Show(appExcetionString, Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
                return;
            }

            BugReportData reportData = ((ObjectDataProvider)FindResource("ErrorData")).ObjectInstance as BugReportData;
            if (reportData == null)
                return;


            try
            {
                MailMessage message = new MailMessage(reportData.ReporterName, toEmailAddress, reportData.Motivation.ToString(), reportData.Comment);
                SmtpClient smtpCl = new SmtpClient();
                smtpCl.Host = "";
                smtpCl.Send(message);
            }
            catch (InvalidOperationException ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
                return;
            }

            this.Close();
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


        /// <summary>
        /// Handles motivation button click. So the Data source property will vary, so the property changed event will be triggered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MotivationButton_Click(object sender, RoutedEventArgs e)
        {
            BugReportData reportData = ((ObjectDataProvider)FindResource("ErrorData")).ObjectInstance as BugReportData;
            if (reportData == null)
                return;
            if (reportData.Motivation == BugReportData.MessageMotivation.Error)
                reportData.Motivation = BugReportData.MessageMotivation.UserComment;
            else
                reportData.Motivation = BugReportData.MessageMotivation.Error;
        }

       
    }
}
