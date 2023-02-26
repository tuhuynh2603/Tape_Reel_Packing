using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Linq;

namespace Magnus_WPF_1.UI.UserControls.View
{
    public class TransformImage
    {
        private UIElement child;
        Point start;
        Point origin;
        MainWindow window;
        public TransformImage(UIElement element)
        {

            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                child.MouseWheel += child_MouseWheel;
                child.MouseLeftButtonDown += child_MouseLeftButtonDown;
                child.MouseLeftButtonUp += child_MouseLeftButtonUp;
                child.MouseMove += child_MouseMove;
                child.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                child_PreviewMouseRightButtonDown);
            }
        }
        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform).Children.First(tr => tr is TranslateTransform);
        }
        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform).Children.First(tr => tr is ScaleTransform);
        }
        public void Reset(int i)
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }
        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Grid gr = sender as Grid;
            var st = GetScaleTransform(child); //256
            var tt = GetTranslateTransform(child);
            if (child != null)
            {
                double zoom = e.Delta > 0 ? 0.5 : -0.5;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                Point relative = e.GetPosition(gr);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;
                if (st.ScaleX <= 0.5 || st.ScaleY <= 0.5)
                {
                    st.ScaleX = 0.5;
                    st.ScaleY = 0.5;
                }
                //st.ScaleX *= 2;
                //st.ScaleY *= 2;

                if (st.ScaleX <= 1 || st.ScaleY <= 1)
                {
                    tt.X = 0;
                    tt.Y = 0;
                }
                else
                {
                    tt.X = abosuluteX - relative.X * st.ScaleX > 0 ? 0 : abosuluteX - relative.X * st.ScaleX;
                    tt.Y = abosuluteY - relative.Y * st.ScaleY > 0 ? 0 : abosuluteY - relative.Y * st.ScaleY;
                }
            }
        }
        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid ad = sender as Grid;
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(window);
                origin = new Point(tt.X, tt.Y);
                window.Cursor = Cursors.Hand;
                child.CaptureMouse();

            }
        }
        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                window.Cursor = Cursors.Arrow;
            }
        }
        private void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset(0); this.Reset(1);
        }
        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            window = (MainWindow)System.Windows.Application.Current.MainWindow;
            Grid ad = sender as Grid;// check ad null
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    var st = GetScaleTransform(child);
                    Vector v = start - e.GetPosition(window);
                    tt.X = origin.X - v.X >= 0 ? 0 : origin.X - v.X < -(st.ScaleX * ad.ActualWidth - ad.ActualWidth) ? -(st.ScaleX * ad.ActualWidth - ad.ActualWidth) : origin.X - v.X;
                    tt.Y = origin.Y - v.Y >= 0 ? 0 : origin.Y - v.Y < -(st.ScaleY * ad.ActualHeight - ad.ActualHeight) ? -(st.ScaleY * ad.ActualHeight - ad.ActualHeight) : origin.Y - v.Y;
                }
            }
        }
    }
}