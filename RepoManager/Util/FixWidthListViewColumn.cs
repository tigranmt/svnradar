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
