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
    /// Interaction logic for ErrorsWindow.xaml
    /// </summary>
    public partial class ErrorsWindow : Window
    {

        static ErrorsWindow singleton = null;


        private ErrorsWindow()
        {
            InitializeComponent();
        }


        public static void ShowWindow()
        {
            if (singleton == null)
            {
                singleton = new ErrorsWindow();
                singleton.Closed += new EventHandler(singleton_Closed);
            }
            singleton.Show();
            singleton.Activate();
        }

        static void singleton_Closed(object sender, EventArgs e)
        {
            singleton.Closed -= new EventHandler(singleton_Closed);
            singleton = null;
        }



        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            errorsView.ItemsSource = ErrorManager.SilentNotificationList;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
