/* FixWidthListViewColumn.cs --------------------------------
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
using System.Windows.Controls;
using System.Windows;

namespace SvnRadar.Util
{
    /// <summary>
    /// Defines special, FixWidth column
    /// </summary>
    public sealed class FixWidthListViewColumn : GridViewColumn
    {


        /*Overwrite Width dependency property metadata*/
        static FixWidthListViewColumn()
        {
            WidthProperty.OverrideMetadata(typeof(FixWidthListViewColumn) ,
                new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceWidth)));
        }

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            /* On Width change always return the width initially installed by the user in the  FixedWidth property */
            FixWidthListViewColumn fwc = o as FixWidthListViewColumn;
            if (fwc != null)          
                return fwc.FixedWidth; // ->

            return baseValue;
        }


        /*Add new FixedWidth dependency property in order to set up initial value*/
        public static readonly DependencyProperty FixedWidthProperty =
                DependencyProperty.Register(
                    "FixedWidth",
                    typeof(double),
                    typeof(FixWidthListViewColumn),
                    new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnFixedWidthChanged)));


        public double FixedWidth
        {
            get { return (double)GetValue(FixedWidthProperty); }
            set { SetValue(FixedWidthProperty, value); }
        }



        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FixWidthListViewColumn fwc = o as FixWidthListViewColumn;
            if (fwc != null)
                fwc.CoerceValue(WidthProperty);
        }


    }
}
