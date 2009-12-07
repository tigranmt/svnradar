/* BindableRichTextBox.cs --------------------------------
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
using System.Windows.Documents;
using System.Windows;

namespace SvnRadar.Util
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
