using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;

namespace TapeReelPacking.Source.Behaviors
{
    public static class ThumbBehavior
    {
        public static readonly DependencyProperty DragDeltaCommandProperty =
            DependencyProperty.RegisterAttached(
                "DragDeltaCommand",
                typeof(ICommand),
                typeof(ThumbBehavior),
                new PropertyMetadata(null, OnDragDeltaCommandChanged));

        public static ICommand GetDragDeltaCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DragDeltaCommandProperty);
        }

        public static void SetDragDeltaCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DragDeltaCommandProperty, value);
        }

        private static void OnDragDeltaCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Thumb thumb)
            {
                thumb.DragDelta -= OnThumbDragDelta;
                thumb.DragDelta += OnThumbDragDelta;
            }
        }

        private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                var command = GetDragDeltaCommand(thumb);
                if (command != null && command.CanExecute(e))
                {
                    command.Execute(e);
                }
            }
        }
    }
}
