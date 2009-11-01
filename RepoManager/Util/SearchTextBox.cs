using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;

namespace RepoManager.Util
{
    /// <summary>
    /// Custom class or managing search text box . Actaully is supprots surch on TexBlock object type content
    /// </summary>
    internal sealed class SearchTextBox : TextBox
    {
        #region fields 
        /// <summary>
        /// Default string content of the text box
        /// </summary>
        string defaultString = "Print here to search content";


        /// <summary>
        /// Control that shuld be assigned in order to have searchable content
        /// </summary>
        static TextBlock searchableTextBlock = null;
        #endregion

        #region SearchTarget dependency property

        public static readonly DependencyProperty SearchTargetProperty =
                DependencyProperty.RegisterAttached("SearchTarget", typeof(UIElement), typeof(SearchTextBox),
                                                        new PropertyMetadata(null, 
                                                            new PropertyChangedCallback(SearchableControlNameChanged)));



        public static UIElement GetSearchTarget(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(SearchTargetProperty);
        }

        public static void SetSearchTarget(DependencyObject obj, string value)
        {
            obj.SetValue(SearchTargetProperty, value);
        }

        private static void SearchableControlNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((e.NewValue as UIElement) == null)
                throw new ArgumentNullException("SearchTarget property is not assigned.");


            searchableTextBlock = e.NewValue as TextBlock;
            if (searchableTextBlock == null)
                throw new ArgumentNullException("SearchTarget property is enabled only for TextBlock type objects.");


        }

        #endregion

        #region overrides
        protected override void OnInitialized(EventArgs e)
        {
            /*Load default string from resource dictionary */
            if(string.IsNullOrEmpty(defaultString))
                defaultString = FindResource("SearchTextBoxDefaultText") as string;
            this.Text = defaultString;

            base.OnInitialized(e);

           
        }

        /// <summary>
        /// On got focus text box content becomes empty
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(System.Windows.RoutedEventArgs e)
        {
            this.Text = string.Empty;
            base.OnGotFocus(e);
        }


        /// <summary>
        /// On Lost focus if content is empty, default text will be assigned to the TextBox's content
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(defaultString))
                this.Text = defaultString;
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Searches the Data binded context on defined string 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            /*Text changed raise at the loading of the control and before on initilize*/
            if (string.IsNullOrEmpty(defaultString))
                defaultString = FindResource("SearchTextBoxDefaultText") as string;


            /* Execute search over data context content only if the control's string is not Null and
             is not default string*/
            if(!string.IsNullOrEmpty(this.Text) && 
                !this.Text.Equals(defaultString,StringComparison.InvariantCultureIgnoreCase))
                SearchInControl(this.Text);

        }
        #endregion


        #region private methods
        /// <summary>
        /// Searches specified string in defined via attached property control's object content
        /// </summary>
        /// <param name="strToFind">String to find in revisionInfo object's content</param>     
        void SearchInControl(string strToFind)
        {
            if (string.IsNullOrEmpty(strToFind) ||
                searchableTextBlock == null)
                return;


            foreach (Inline inl in searchableTextBlock.Inlines)
            {
                //inl.ElementStart.
                if ((inl as InlineUIContainer) == null)
                    continue;

                TextBox tb= ((inl as InlineUIContainer).Child as TextBox);
                if (tb == null) 
                    continue;

                int indx = tb.Text.IndexOf(strToFind);
                if (indx >= 0)
                {
                    tb.AutoWordSelection = true;
                    tb.SelectionStart = indx;
                    tb.SelectionLength = strToFind.Length;
                   
                }
                else
                    tb.SelectedText = string.Empty;
                
            }

          

        }


            /// <summary>
        /// Finds the requested resource from the application's resource dictionary
        /// </summary>
        /// <param name="esourceKey">Resource key</param>
        /// <returns>Resource object if exists, Null otherwise</returns>
        static object FindResource(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey))
                return null;

            return System.Windows.Application.Current.FindResource(resourceKey);
        }

        #endregion

    }
}
