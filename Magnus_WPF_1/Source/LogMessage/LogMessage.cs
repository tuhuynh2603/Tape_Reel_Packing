using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Magnus_WPF_1.Source.LogMessage
{
    public static class LogMessage
    {

        [DllImport("DebugMessage.dll")]
        static extern void OutputDebugLogTo(int tab, bool timestamp, StringBuilder messageText);

        public static void DebugMessageTo(int tab, bool timestamp, string messageText)
        {
            try
            {
                OutputDebugLogTo(tab, timestamp, new StringBuilder(messageText));
            }
            catch
            {
                //System.Windows.Forms.MessageBox.Show(e.Message);
                //OutputDebugLogTo(tab, timestamp, new StringBuilder(e.Message));
            };
        }

        public static void OutDebugMessage(string messageText)
        {
            try
            {
                OutputDebugLogTo(2, true, new StringBuilder(messageText));

            }
            catch
            {
                //System.Windows.Forms.MessageBox.Show(e.Message);
                //OutputDebugLogTo(2, true, new StringBuilder(e.Message));
            };
        }
        public static void WriteToDebugViewer(int tab, string messageText)
        {
            DateTime time = DateTime.Now;
            messageText = string.Format("{0}: {1}", time.ToString("HH:mm:ss.fff"), messageText);
            //MessageBox.Show(messageText);
            try
            {
                OutputDebugLogTo(tab, true, new StringBuilder(messageText));
            }
            catch
            {
                //OutputDebugLogTo(tab, true, new StringBuilder(e.Message));
                //System.Windows.Forms.MessageBox.Show(e.Message);
            };
        }

    }
}
