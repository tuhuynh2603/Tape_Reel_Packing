using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TapeReelPacking.Source.Define;
using static TapeReelPacking.Source.Application.Master;
using TapeReelPacking.UI.UserControls.View;
using TapeReelPacking.Source.Application;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class WarningMessageBoxVM:BaseVM
    {

        private MainWindowVM _mainWindowVM { set; get; }
        private WARNINGMESSAGE _m_warningMessage = WARNINGMESSAGE.MESSAGE_INFORMATION;
        private string _strWarningMessage = "........";

        public string strWarningMessage
        {
            get { return _strWarningMessage; }
            set
            {
                _strWarningMessage = value;
                OnPropertyChanged(nameof(strWarningMessage));
            }
        }


        private Visibility _isVisible = Visibility.Collapsed;
        public Visibility isVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(isVisible));
            }
        }

        public WarningMessageBoxVM(MainWindowVM mainWindowVM)
        {
            _mainWindowVM = mainWindowVM;
            popupWarningMessageBoxDelegate = PopupWarningMessageBox;
            continueSequenceButtonClickedDelegate = ContinueSequenceButtonClicked;        
        }




        public void updateMessageString(string strMessage, WARNINGMESSAGE warningtype)
        {
            _m_warningMessage = warningtype;
            string strDateTime = DateTime.Now.ToString();
            strWarningMessage = $"[{strDateTime}]:  {strMessage}";

            //btn_Sequence_Abort.IsEnabled = false;
            btn_Sequence_AbortVisible = Visibility.Collapsed;

            //btn_Sequence_Continue.IsEnabled = false;
            btn_Sequence_ContinueVisible = Visibility.Collapsed;

            //btn_Sequence_Previous.IsEnabled = false;
            //btn_Sequence_Next.IsEnabled = false;

            btn_Sequence_PreviousVisible = Visibility.Collapsed;
            btn_Sequence_NextVisible = Visibility.Collapsed;

            btn_Retry_Current_StepVisible = Visibility.Collapsed;
            //btn_Retry_Current_Step.IsEnabled = false;

            switch (warningtype)
            {
                case WARNINGMESSAGE.MESSAGE_EMERGENCY:
                    //txtWarningMessage.Text = $"[{strDateTime}]:  Emergency Button Clicked!";

                    //btn_Sequence_Abort.IsEnabled = true;
                    btn_Sequence_AbortVisible = Visibility.Visible
                        ;
                    break;

                case WARNINGMESSAGE.MESSAGE_IMIDIATESTOP:
                    //txtWarningMessage.Text = $"[{strDateTime}]: Imidiate Button Clicked! Please click Continue/Abort to Continue/End Sequence";

                    //btn_Sequence_Continue.IsEnabled = true;
                    btn_Sequence_ContinueVisible = Visibility.Visible;

                    //btn_Sequence_Abort.IsEnabled = true;
                    btn_Sequence_AbortVisible = Visibility.Visible;

                    //btn_Retry_Current_Step.Visibility = Visibility.Visible;
                    //btn_Retry_Current_Step.IsEnabled = true;


                    break;

                case WARNINGMESSAGE.MESSAGE_STEPDEBUG:
                    //btn_Sequence_Previous.IsEnabled = true;
                    //btn_Sequence_Next.IsEnabled = true;

                    //btn_Sequence_Previous.Visibility = Visibility.Visible;
                    btn_Sequence_NextVisible = Visibility.Visible;

                    //btn_Sequence_Abort.IsEnabled = true;
                    btn_Sequence_AbortVisible = Visibility.Visible;
                    break;

                case WARNINGMESSAGE.MESSAGE_INFORMATION:
                    //txtWarningMessage.Text = $"[{strDateTime}]: {strMessage}";

                    //btn_Sequence_Continue.IsEnabled = true;
                    btn_Sequence_ContinueVisible = Visibility.Visible;

                    //btn_Sequence_Abort.IsEnabled = true;
                    btn_Sequence_AbortVisible = Visibility.Visible;

                    break;


            }

        }
        private ActionCommand btn_Sequence_Continue_ClickCmd1;

        public ICommand btn_Sequence_Continue_ClickCmd
        {
            get
            {
                if (btn_Sequence_Continue_ClickCmd1 == null)
                {
                    btn_Sequence_Continue_ClickCmd1 = new ActionCommand(btn_Sequence_Continue_Click);
                }

                return btn_Sequence_Continue_ClickCmd1;
            }
        }


        public void btn_Sequence_Continue_Click()
        {
            ContinueSequenceButtonClicked(_m_warningMessage);
        }

        public delegate void ContinueSequenceButtonClickedDelegate(WARNINGMESSAGE nWarningMessges);
        public static ContinueSequenceButtonClickedDelegate continueSequenceButtonClickedDelegate;

        private void ContinueSequenceButtonClicked(WARNINGMESSAGE nWarningMessges)
        {
            if (RobotIOStatus.m_EmergencyStatus == 1)
                return;

            Source.Hardware.SDKHrobot.HWinRobot.set_motor_state(Source.Hardware.SDKHrobot.HiWinRobotInterface.m_RobotConnectID, 1);
            PopupWarningMessageBox("", WARNINGMESSAGE.MESSAGE_INFORMATION, false);
            if (RobotIOStatus.m_ImidiateStatus_Simulate == 1)
                RobotIOStatus.m_ImidiateStatus_Simulate = 0;
            MainWindowVM.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE;
            Master.m_bNeedToImidiateStop = false;
            Thread.Sleep(500);
            Master.m_NextStepSequenceEvent.Set();
        }

        private Visibility btn_Sequence_ContinueVisible1;

        public Visibility btn_Sequence_ContinueVisible
        {
            get => btn_Sequence_ContinueVisible1;
            set
            {
                btn_Sequence_ContinueVisible1 = value;
                OnPropertyChanged(nameof(btn_Sequence_ContinueVisible));
            }
        }
        private ActionCommand btn_Sequence_Abort_ClickCmd1;

        public ICommand btn_Sequence_Abort_ClickCmd
        {
            get
            {
                if (btn_Sequence_Abort_ClickCmd1 == null)
                {
                    btn_Sequence_Abort_ClickCmd1 = new ActionCommand(Performbtn_Sequence_Abort_ClickCmd);
                }

                return btn_Sequence_Abort_ClickCmd1;
            }
        }

        private void Performbtn_Sequence_Abort_ClickCmd()
        {
            if (Master.RobotIOStatus.m_EmergencyStatus == 1)
                return;

            MainWindowVM.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_ABORT;


            PopupWarningMessageBox("", WARNINGMESSAGE.MESSAGE_INFORMATION, false);
            Thread.Sleep(500);
            Master.m_NextStepSequenceEvent.Set();
        }

        private Visibility btn_Sequence_AbortVisible1;

        public Visibility btn_Sequence_AbortVisible
        {
            get => btn_Sequence_AbortVisible1;
            set
            {
                btn_Sequence_AbortVisible1 = value;
                OnPropertyChanged(nameof(btn_Sequence_AbortVisible));
            }
        }
        private ActionCommand btn_Sequence_Previous_ClickCmd1;

        public ICommand btn_Sequence_Previous_ClickCmd
        {
            get
            {
                if (btn_Sequence_Previous_ClickCmd1 == null)
                {
                    btn_Sequence_Previous_ClickCmd1 = new ActionCommand(Performbtn_Sequence_Previous_ClickCmd);
                }

                return btn_Sequence_Previous_ClickCmd1;
            }
        }

        private void Performbtn_Sequence_Previous_ClickCmd()
        {
        }

        private Visibility btn_Sequence_PreviousVisible1;

        public Visibility btn_Sequence_PreviousVisible
        {
            get => btn_Sequence_PreviousVisible1;
            set
            {
                btn_Sequence_PreviousVisible1 = value;
                OnPropertyChanged(nameof(btn_Sequence_PreviousVisible));
            }
        }
        private ActionCommand btn_Sequence_Next_ClickCmd1;

        public ICommand btn_Sequence_Next_ClickCmd
        {
            get
            {
                if (btn_Sequence_Next_ClickCmd1 == null)
                {
                    btn_Sequence_Next_ClickCmd1 = new ActionCommand(Performbtn_Sequence_Next_ClickCmd);
                }

                return btn_Sequence_Next_ClickCmd1;
            }
        }

        private void Performbtn_Sequence_Next_ClickCmd()
        {
            if (Master.RobotIOStatus.m_EmergencyStatus == 1)
                return;

            Source.Hardware.SDKHrobot.HWinRobot.set_motor_state(Source.Hardware.SDKHrobot.HiWinRobotInterface.m_RobotConnectID, 1);
            MainWindowVM.master.m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE;

            PopupWarningMessageBox("", WARNINGMESSAGE.MESSAGE_INFORMATION, false);
            Thread.Sleep(500);
            Master.m_NextStepSequenceEvent.Set();
        }

        private Visibility btn_Sequence_NextVisible1;

        public Visibility btn_Sequence_NextVisible
        {
            get => btn_Sequence_NextVisible1;
            set
            {
                btn_Sequence_NextVisible1 = value;
                OnPropertyChanged(nameof(btn_Sequence_NextVisible));
            }
        }
        private ActionCommand btn_Retry_Current_Step_ClickCmd1;

        public ICommand btn_Retry_Current_Step_ClickCmd
        {
            get
            {
                if (btn_Retry_Current_Step_ClickCmd1 == null)
                {
                    btn_Retry_Current_Step_ClickCmd1 = new ActionCommand(Performbtn_Retry_Current_Step_ClickCmd);
                }

                return btn_Retry_Current_Step_ClickCmd1;
            }
        }

        private void Performbtn_Retry_Current_Step_ClickCmd()
        {

        }

        private Visibility btn_Retry_Current_StepVisible1;

        public Visibility btn_Retry_Current_StepVisible
        { 
            get => btn_Retry_Current_StepVisible1; 
            set {
                btn_Retry_Current_StepVisible1 = value;
                OnPropertyChanged(nameof(btn_Retry_Current_StepVisible));
            }
        }

        public delegate void PopupWarningMessageBoxDelegate(string strDebugMessage, WARNINGMESSAGE warningtype, bool bIsPopup = true);
        public static PopupWarningMessageBoxDelegate popupWarningMessageBoxDelegate;

        public void PopupWarningMessageBox(string strDebugMessage, WARNINGMESSAGE warningtype, bool bIsPopup = true)
        {

            if (bIsPopup)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    updateMessageString(strDebugMessage, warningtype);

                    isVisible = Visibility.Visible;
                });
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    isVisible = Visibility.Collapsed;

                });
            }
        }



    }
}
