using Magnus_WPF_1.Source.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static Magnus_WPF_1.Source.Application.Master;

namespace Magnus_WPF_1.UI.UserControls
{
    /// <summary>
    /// Interaction logic for WarningMessageBox.xaml
    /// </summary>
    public partial class WarningMessageBox : UserControl, INotifyPropertyChanged
    {

        private string _m_SequenceWarningMessage = "........";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }

        public string m_SequenceWarningMessage
        {
            get { return _m_SequenceWarningMessage; }
            set
            {
                if (value != _m_SequenceWarningMessage)
                {
                    _m_SequenceWarningMessage = value;
                    OnPropertyChanged("m_SequenceWarningMessage");
                }
            }
        }
        public WarningMessageBox()
        {
            InitializeComponent();
        }

        public void updateMessageString(string strMessage, bool bStopButtonClicked, bool bNeedToAbort)
        {
            string strDateTime = DateTime.Now.ToString();
            ////m_SequenceWarningMessage = $"[{strDateTime}]:   {strMessage}" ;
            txtWarningMessage.Text = $"[{strDateTime}]:  {strMessage}";
            if (bNeedToAbort)
            {
                txtWarningMessage.Text = $"[{strDateTime}]:  Emergency Button Clicked!";

                btn_Sequence_Continue.IsEnabled = false;
                btn_Sequence_Continue.Visibility = Visibility.Collapsed;

                btn_Sequence_Previous.IsEnabled = false;
                btn_Sequence_Next.IsEnabled = false;

                btn_Sequence_Previous.Visibility = Visibility.Collapsed;
                btn_Sequence_Next.Visibility = Visibility.Collapsed;
            }

            else if (bStopButtonClicked)
            {
                txtWarningMessage.Text = $"[{strDateTime}]: Imidiate Button Clicked! Please click Continue/Abort to Continue/End Sequence";

                btn_Sequence_Continue.IsEnabled = true;
                btn_Sequence_Continue.Visibility = Visibility.Visible;

                btn_Sequence_Previous.IsEnabled = false;
                btn_Sequence_Next.IsEnabled = false;

                btn_Sequence_Previous.Visibility = Visibility.Collapsed;
                btn_Sequence_Next.Visibility = Visibility.Collapsed;


            }
            else
            {
                btn_Sequence_Previous.IsEnabled = true;
                btn_Sequence_Next.IsEnabled = true;

                btn_Sequence_Previous.Visibility = Visibility.Visible;
                btn_Sequence_Next.Visibility = Visibility.Visible;

                btn_Sequence_Continue.IsEnabled = false;
                btn_Sequence_Continue.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_Sequence_Continue_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE;

            if (MessageBox.Show("Would you like to continue sequence ?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Master.m_EmergencyStopSequenceEvent.Set();

            else
                Master.m_EmergencyStopSequenceEvent.Reset();

            MainWindow.mainWindow.master.m_bNeedToImidiateStop = false;
            MainWindow.mainWindow.PopupWarningMessageBox("", false, false, false);

        }

        private void btn_Sequence_Abort_Click(object sender, RoutedEventArgs e)
        {
            
            MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_ABORT;
                Master.m_EmergencyStopSequenceEvent.Set();

            MainWindow.mainWindow.PopupWarningMessageBox("", false, false, false);

        }

        private void btn_Sequence_Previous_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_GOBACK;
            if (MainWindow.mainWindow.m_bEnableDebugSequence)
                Master.m_NextStepSequenceEvent.Set();
        }

        private void btn_Sequence_Next_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE;
            if (MainWindow.mainWindow.m_bEnableDebugSequence)
                Master.m_NextStepSequenceEvent.Set();
        }
    }
}
