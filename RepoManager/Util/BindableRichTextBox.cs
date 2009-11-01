using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;

namespace RepoManager.Util
{
    /// <summary>
    /// Rich text box that allow binding on DocumentContent property
    /// </summary>
    public sealed class BindableRichTextBox : RichTextBox
    {
        public static readonly System.Windows.DependencyProperty DocumentContentProperty = DependencyProperty.Register("RichDocumentContent", typeof(FlowDocument), typeof(BindableRichTextBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDocumentChanged)));

        /// <summary>
        /// Dependency property for rich documen content
        /// </summary>
        public  FlowDocument RichDocumentContent
        {
            get { return (FlowDocument)GetValue(DocumentContentProperty); }
            set { SetValue(DocumentContentProperty, value); }
        }

        static void OnDocumentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            BindableRichTextBox rtb = (BindableRichTextBox)obj;
            rtb.Document = (FlowDocument)args.NewValue;
            rtb.IsReadOnly = false;
        }
    }
}
