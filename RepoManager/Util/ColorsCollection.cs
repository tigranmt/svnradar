using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Reflection;

namespace RepoManager.Util
{
    sealed class ColorsCollection
    {
        static Color[] colorList = new Color[] { Colors.Beige, Colors.Coral, Colors.DarkViolet, Colors.ForestGreen };
       
         static int counter = 0;

         public static Brush GetNextColor() {

             counter++;
             if(counter == colorList.Length - 1)
                 counter = 0;

             
             return new  SolidColorBrush(colorList[counter]);
         }


    }
}
