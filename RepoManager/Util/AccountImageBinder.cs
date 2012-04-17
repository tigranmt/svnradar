using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;

namespace SvnRadar.Util
{
    internal class AccountImageBinder
    {
        /// <summary>
        /// Function searches for account defined image, if find it, loads 
        /// </summary>
        /// <param name="imageSource">The source have to be asssigned</param>
        /// <param name="accountName">Account name to search for</param>
        public static void BindToAccountImage(Image imageSource, string accountName)
        {
            // Image img = AppResourceManager.FindResource("/Images/systrayicon.ico");


            bool ok = false;
            if (!string.IsNullOrEmpty(accountName))
            {
                string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string imageFolder = Path.Combine(exeFolder, "Accounts");
                string accountImagePath = Path.Combine(imageFolder, accountName + ".jpg");
                if (File.Exists(accountImagePath))
                {
                    try
                    {

                        imageSource.Source = new BitmapImage(new Uri(accountImagePath));
                        ok = true;
                    }
                    catch
                    {
                        ok = false;
                    }
                }
            }

            if (!ok)
                imageSource.Source = new BitmapImage(new Uri("pack://application:,,/Images/systrayicon.ico"));
        }

    }
}
