using Magnus_WPF_1.Source.Application;
using Magnus_WPF_1.Source.Define;
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
        private WARNINGMESSAGE _m_warningMessage = WARNINGMESSAGE.MESSAGE_INFORMATION;
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

        public void updateMessageString(string strMessage, WARNINGMESSAGE warningtype)
        {
            _m_warningMessage = warningtype;
            string strDateTime = DateTime.Now.ToString();
            ////m_SequenceWarningMessage = $"[{strDateTime}]:   {strMessage}" ;
            txtWarningMessage.Text = $"[{strDateTime}]:  {strMessage}";

            btn_Sequence_Abort.IsEnabled = false;
            btn_Sequence_Abort.Visibility = Visibility.Collapsed;

            btn_Sequence_Continue.IsEnabled = false;
            btn_Sequence_Continue.Visibility = Visibility.Collapsed;

            btn_Sequence_Previous.IsEnabled = false;
            btn_Sequence_Next.IsEnabled = false;

            btn_Sequence_Previous.Visibility = Visibility.Collapsed;
            btn_Sequence_Next.Visibility = Visibility.Collapsed;



            switch (warningtype)
                {
                case WARNINGMESSAGE.MESSAGE_EMERGENCY:
                    //txtWarningMessage.Text = $"[{strDateTime}]:  Emergency Button Clicked!";

                    btn_Sequence_Abort.IsEnabled = true;
                    btn_Sequence_Abort.Visibility = Visibility.Visible
                        ;
                    break;

                case WARNINGMESSAGE.MESSAGE_IMIDIATESTOP:
                    //txtWarningMessage.Text = $"[{strDateTime}]: Imidiate Button Clicked! Please click Continue/Abort to Continue/End Sequence";

                    btn_Sequence_Continue.IsEnabled = true;
                    btn_Sequence_Continue.Visibility = Visibility.Visible;

                    btn_Sequence_Abort.IsEnabled = true;
                    btn_Sequence_Abort.Visibility = Visibility.Visible;
                    break;

                case WARNINGMESSAGE.MESSAGE_STEPDEBUG:
                    btn_Sequence_Previous.IsEnabled = true;
                    btn_Sequence_Next.IsEnabled = true;

                    btn_Sequence_Previous.Visibility = Visibility.Visible;
                    btn_Sequence_Next.Visibility = Visibility.Visible;

                    btn_Sequence_Abort.IsEnabled = true;
                    btn_Sequence_Abort.Visibility = Visibility.Visible;
                    break;

                case WARNINGMESSAGE.MESSAGE_INFORMATION:
                    //txtWarningMessage.Text = $"[{strDateTime}]: {strMessage}";

                    btn_Sequence_Continue.IsEnabled = true;
                    btn_Sequence_Continue.Visibility = Visibility.Visible;
                    break;


            }

        }

        private void btn_Sequence_Continue_Click(object sender, RoutedEventArgs e)
        {

            if (MainWindow.mainWindow.master.m_ImidiateStatus + MainWindow.mainWindow.master.m_EmergencyStatus > 0)
                return;

            Source.Hardware.SDKHrobot.HWinRobot.set_motor_state(Source.Hardware.SDKHrobot.HiWinRobotInterface.m_RobotConnectID, 1);
            Master.m_NextStepSequenceEvent.Set();
            if (_m_warningMessage == WARNINGMESSAGE.MESSAGE_IMIDIATESTOP)
            {
                MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE;
                MainWindow.mainWindow.master.m_bNeedToImidiateStop = false;
            }
            else
                MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE;


            MainWindow.mainWindow.PopupWarningMessageBox("", WARNINGMESSAGE.MESSAGE_INFORMATION, false);

        }

        private void btn_Sequence_Abort_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.mainWindow.master.m_ImidiateStatus + MainWindow.mainWindow.master.m_EmergencyStatus > 0)
                return;

            MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_ABORT;
                Master.m_NextStepSequenceEvent.Set();

            MainWindow.mainWindow.PopupWarningMessageBox("", WARNINGMESSAGE.MESSAGE_INFORMATION, false);

        }

        private void btn_Sequence_Previous_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.mainWindow.master.m_ImidiateStatus + MainWindow.mainWindow.master.m_EmergencyStatus > 0)
                return;

            Source.Hardware.SDKHrobot.HWinRobot.set_motor_state(Source.Hardware.SDKHrobot.HiWinRobotInterface.m_RobotConnectID, 1);
            MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_GOBACK;
            Master.m_NextStepSequenceEvent.Set();

            MainWindow.mainWindow.PopupWarningMessageBox("", WARNINGMESSAGE.MESSAGE_INFORMATION, false);

        }

        private void btn_Sequence_Next_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.mainWindow.master.m_ImidiateStatus + MainWindow.mainWindow.master.m_EmergencyStatus > 0)
                return;

            Source.Hardware.SDKHrobot.HWinRobot.set_motor_state(Source.Hardware.SDKHrobot.HiWinRobotInterface.m_RobotConnectID, 1);
            MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE;
            Master.m_NextStepSequenceEvent.Set();

            MainWindow.mainWindow.PopupWarningMessageBox("", WARNINGMESSAGE.MESSAGE_INFORMATION, false);

        }

        private void btn_Retry_Current_Step_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.mainWindow.master.m_ImidiateStatus + MainWindow.mainWindow.master.m_EmergencyStatus > 0)
                return;

            MainWindow.mainWindow.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_RETRY;
            Master.m_NextStepSequenceEvent.Set();

            MainWindow.mainWindow.PopupWarningMessageBox("", WARNINGMESSAGE.MESSAGE_INFORMATION, false);
        }
    }
}
