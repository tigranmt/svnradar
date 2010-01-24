/* ErrorsWindow.xaml.cs --------------------------------
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

        private void ClearErrorsButton_Click(object sender, RoutedEventArgs e)
        {
            lock (ErrorManager.SilentNotificationList)
            {
                ErrorManager.SilentNotificationList.Clear();
            }
        }

    }
}
