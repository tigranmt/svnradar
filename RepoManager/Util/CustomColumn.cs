using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace RepoManager.Util
{
    /// <summary>
    /// Defines special, CustomColumn column
    /// </summary>
    public sealed class CustomColumn : GridViewColumn
    {


        /*Overwrite Width dependency property metadata*/
        static CustomColumn()
        {
            WidthProperty.OverrideMetadata(typeof(CustomColumn),
                new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceWidth)));
        }

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            /* On Width change always return the width initially installed by the user in the  FixedWidth property */
            CustomColumn customColumn = o as CustomColumn;
            if (customColumn != null && double.IsNaN((double)baseValue))
            {                
                return customColumn.Width; // ->
            }

            //customColumn.Width = (double)baseValue;
            return baseValue;
        }




    }
}
