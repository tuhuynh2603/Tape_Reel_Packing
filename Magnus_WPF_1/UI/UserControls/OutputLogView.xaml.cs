using Magnus_WPF_1.Source.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Magnus_WPF_1.UI.UserControls
{
	/// <summary>
	/// Interaction logic for OutputLogView.xaml
	/// </summary>
	public partial class OutputLogView : UserControl
	{
        MainWindow m_MainWindow;
		public OutputLogView(MainWindow mainWindow)
		{
            m_MainWindow = mainWindow;

            InitializeComponent();
		}

		private void SentDelete_Click(object sender, RoutedEventArgs e)
		{
			ClearOutputLog();
		}

        private Paragraph paragraphOutputLog;
        public void AddLineOutputLog(string text, int nStyle)
        {
            DateTime currentTime = DateTime.Now;
            string strOutputLog = currentTime.ToString("HH:mm:ss.fff") + "  " + text;
            if (paragraphOutputLog == null)
            {
                paragraphOutputLog = new Paragraph();
                m_MainWindow.outputLogView.outputLog.Blocks.Add(paragraphOutputLog);
            }

            else
            {
                //paragraphOutputLog.Inlines.Add(text + '\n');
                if (nStyle == (int)ERROR_CODE.PASS)
                {
                    paragraphOutputLog.Inlines.Add(strOutputLog + '\n');
                }
                else
                {
                    Label a = new Label();
                    a.Content = strOutputLog;
                    a.Foreground = System.Windows.Media.Brushes.White;
                    a.Background = System.Windows.Media.Brushes.Red;
                    paragraphOutputLog.Inlines.Add(a);
                    paragraphOutputLog.Inlines.Add("\n");
                }
            }
            ScrollToLastRow();
        }

        private void ScrollToLastRow()
        {
            if (m_MainWindow.outputLogView.flowDocumentSVOutputlog != null)
            {
                // Find the ScrollViewer within the FlowDocumentScrollViewer.
                ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(m_MainWindow.outputLogView.flowDocumentSVOutputlog);

                if (scrollViewer != null)
                {
                    // Scroll to the end (bottom) of the ScrollViewer.
                    scrollViewer.ScrollToBottom();
                }
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


        public void ClearOutputLog()
        {
            if (m_MainWindow.outputLogView.outputLog.Blocks.Count > 0)
                m_MainWindow.outputLogView.outputLog.Blocks.Clear();

            try
            {
                m_MainWindow.outputLogView.Dispatcher.Invoke(() => {
                    paragraphOutputLog = new Paragraph();
                    m_MainWindow.outputLogView.outputLog.Blocks.Add(paragraphOutputLog);
                    paragraphOutputLog.Inlines.Add("");
                });
            }

            catch (Exception ex)
            {
                string error = ex.ToString();
            }
        }



    }
}
