/* ListViewItemColorConverter.cs --------------------------------
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
using System.Windows.Data;
using System.Windows.Media;
using SvnRadar.Common.Controls;
using System.Windows.Controls;
using SvnRadar.Util;

namespace SvnRadar.FormatConverters
{
    public sealed class ListViewItemColorConverter : IValueConverter
    {

        #region IValueConverter Members

        List<int> revisions = new List<int>();
      

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (RepoBrowserConfiguration.Instance.ViewLayout == RepoBrowserConfiguration.ListViewLayoutEnum.GroupView)
                return true;

            int curRevNum = (int)value;

            int indexOfRev = revisions.IndexOf(curRevNum);
            if (indexOfRev < 0)
            {
                revisions.Add(curRevNum);
                indexOfRev = revisions.Count - 1;
            }

          
            if (indexOfRev % 2 == 0)
                return true;
            else
                return false;
           

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
