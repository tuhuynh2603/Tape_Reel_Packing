using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows;

namespace TapeReelPacking.Source.Behaviors
{
    public class ScrollToEndBehavior : Behavior<FlowDocumentScrollViewer>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.LayoutUpdated += OnLayoutUpdated;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.LayoutUpdated -= OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(this.AssociatedObject);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                {
                    return result;
                }

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }
    }
}
