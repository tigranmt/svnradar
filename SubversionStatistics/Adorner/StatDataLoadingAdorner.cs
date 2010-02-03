using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Threading;

namespace SubversionStatistics
{
    /// <summary>
    /// Implements Adorner that appears over the stat UI area untill the data is loading into the data base
    /// </summary>
    public class StatDataLoadingAdorner : Adorner
    {
        FormattedText formattedText = new FormattedText("Loading", Thread.CurrentThread.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface("Verdana"), 20.0, System.Windows.Media.Brushes.WhiteSmoke);

        
        public StatDataLoadingAdorner(UIElement uiElement)
            : base(uiElement)
        {
        }


        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
           
            drawingContext.DrawText(formattedText, new System.Windows.Point(10, 10));

            drawingContext.DrawRectangle((System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#7F4047F7"),
                new System.Windows.Media.Pen(System.Windows.Media.Brushes.Gray, 1), new Rect(new System.Windows.Point(2,2), DesiredSize));
            base.OnRender(drawingContext);
        }
    }
}
