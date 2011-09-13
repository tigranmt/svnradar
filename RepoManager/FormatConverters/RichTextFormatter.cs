using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Controls;
using System.Windows.Documents;

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
            new TextRange(document.ContentStart, document.ContentEnd).Text = text + System.Environment.NewLine;
        }
    }
}
