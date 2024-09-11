using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows;

namespace TapeReelPacking.Source.Behaviors
{
    public class DragBehavior : Behavior<UIElement>
    {
        private Point _startPoint;
        private bool _isDragging;

        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register(nameof(HorizontalOffset), typeof(double), typeof(DragBehavior), new PropertyMetadata(0.0));

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register(nameof(VerticalOffset), typeof(double), typeof(DragBehavior), new PropertyMetadata(0.0));

        public double HorizontalOffset
        {
            get => (double)GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        public double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _startPoint = e.GetPosition(null);
            AssociatedObject.CaptureMouse();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPosition = e.GetPosition(null);
                double offsetX = currentPosition.X - _startPoint.X;
                double offsetY = currentPosition.Y - _startPoint.Y;

                HorizontalOffset += offsetX;
                VerticalOffset += offsetY;

                _startPoint = currentPosition; // Update the start point for smoother dragging
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            AssociatedObject.ReleaseMouseCapture();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }
    }
}
