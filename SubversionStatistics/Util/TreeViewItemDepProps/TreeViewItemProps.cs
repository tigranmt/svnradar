using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SubversionStatistics.Util
{
    /// <summary>
    /// Defines Tree View item dependency properties for handling of the Children async loading 
    /// </summary>
    public static class TreeViewItemProps
    {

        #region dependency property Get/Set methods
        public static bool GetIsLoading(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLoadingProperty);
        }

        public static void SetIsLoading(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLoadingProperty, value);
        }

        public static bool GetIsLoaded(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLoadedProperty);
        }

        public static void SetIsLoaded(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLoadedProperty, value);
        }

        public static bool GetIsCanceled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCanceledProperty);
        }

        public static void SetIsCanceled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCanceledProperty, value);
        }
        #endregion

        #region properties declarations
        public static readonly DependencyProperty IsLoadingProperty;
        public static readonly DependencyProperty IsLoadedProperty;
        public static readonly DependencyProperty IsCanceledProperty;
        #endregion

        #region ctor
        static TreeViewItemProps()
        {            

            ///True if the child of the specified TreeViewItem has begun to load, False otherwise
            IsLoadingProperty = DependencyProperty.RegisterAttached("IsLoading",
                                                                    typeof(bool), typeof(TreeViewItemProps),
                                                                    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
            
            ///True, if the children loading of the specified TreeViewItem has finished, False otherwise
            IsLoadedProperty = DependencyProperty.RegisterAttached("IsLoaded",
                                                                    typeof(bool), typeof(TreeViewItemProps),
                                                                    new FrameworkPropertyMetadata(false));

            ///If True, the loading of children of the specified TreeVieItem was canceled, False otherwise
            IsCanceledProperty = DependencyProperty.RegisterAttached("IsCanceled",
                                                                    typeof(bool), typeof(TreeViewItemProps),
                                                                    new FrameworkPropertyMetadata(false));
        }
        #endregion
    }
}
