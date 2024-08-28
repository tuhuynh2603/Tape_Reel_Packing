using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using TapeReelPacking.Source.Define;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class OutputLogVM : BaseVM
    {

        private static Paragraph _paragraphOutputLog = new Paragraph();


        private FlowDocument _outputLog;
        public FlowDocument OutputLog
        {
            get => _outputLog;
            set
            {
                _outputLog = value;
                OnPropertyChanged(nameof(OutputLog));
            }
        }

        private ActionCommand _DeleteAllCommand;
        public ICommand DeleteAllCommand
        {
            get
            {
                if (_DeleteAllCommand == null)
                {
                    _DeleteAllCommand = new ActionCommand(DeleteAll);
                }

                return _DeleteAllCommand;
            }
        }

        public OutputLogVM()
        {
            OutputLog = new FlowDocument();

            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem { Header = "Copy All", Command = ApplicationCommands.Copy });
            contextMenu.Items.Add(new MenuItem { Header = "Delete All", Command = DeleteAllCommand });

            OutputLog.ContextMenu = contextMenu;

            OutputLog.Blocks.Add(_paragraphOutputLog);

        }

        private void DeleteAll()
        {
            ClearOutputLog();
        }

        public void ClearOutputLog()
        {
            OutputLog.Blocks.Clear();
            _paragraphOutputLog = new Paragraph();
            OutputLog.Blocks.Add(_paragraphOutputLog);
        }


        public static void AddLineOutputLog(string text, int nStyle)
        {
            DateTime currentTime = DateTime.Now;
            string strOutputLog = currentTime.ToString("HH:mm:ss.fff") + "  " + text;

            if (_paragraphOutputLog.Inlines.Count > 1000)
            {
                _paragraphOutputLog.Inlines.Remove(_paragraphOutputLog.Inlines.FirstInline);
            }

            if (nStyle == (int)ERROR_CODE.PASS)
            {
                Run errorText = new Run(strOutputLog + '\n')
                {
                    Foreground = Brushes.White,
                    Background = Brushes.Transparent
                };

                _paragraphOutputLog.Inlines.Add(errorText);
            }
            else
            {
                Run errorText = new Run(strOutputLog)
                {
                    Foreground = Brushes.White,
                    Background = Brushes.Red
                };
                _paragraphOutputLog.Inlines.Add(errorText);
                _paragraphOutputLog.Inlines.Add(new LineBreak());
            }
        }

    }
}
