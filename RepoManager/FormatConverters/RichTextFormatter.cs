using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Controls;
using System.Windows.Documents;
using System.Text.RegularExpressions;

namespace SvnRadar.FormatConverters
{
    public class ToolTipRichTextFormatter : ITextFormatter
    {
        public string GetText(System.Windows.Documents.FlowDocument document)
        {
            return new TextRange(document.ContentStart, document.ContentEnd).Text;
        }

        public void SetText(System.Windows.Documents.FlowDocument document, string text)
        {            
            Regex regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
            MatchCollection col = regx.Matches(text);


            //if (col != null && col.Count > 0)
            //{
            //    foreach (Match match in col)
            //    {
            //        new TextRange(document.ContentStart, document.ContentEnd).ApplyPropertyValue( = text.Substring()
            //    }
            //}
            //else
             new TextRange(document.ContentStart, document.ContentEnd).Text = text;

        }
    }
}
