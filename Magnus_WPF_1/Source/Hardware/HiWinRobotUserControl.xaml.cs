using Magnus_WPF_1.Source.Hardware.SDKHrobot;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using static Magnus_WPF_1.Source.Hardware.SDKHrobot.HiWinRobotInterface;

namespace Magnus_WPF_1.Source.Hardware
{
    /// <summary>
    /// Interaction logic for HiWinRobotUserControl.xaml
    /// </summary>
    public partial class HiWinRobotUserControl : UserControl, INotifyPropertyChanged
    {
        public enum MOVETYPES
        {
            AbsoluteMove = 0,
            RelativeMove = 1
        }

        public static string m_strAlarmMessage = "";
        public HiWinRobotUserControl(string strIPAddress)
        {
            InitializeComponent();
            _m_txtRobotIPAddress = strIPAddress;

            combo_JogType.Items.Clear();
            //combo_JogType.Items.Add("Joint");
            combo_JogType.Items.Add("XYZ");
            combo_JogType.SelectedIndex = 1;
            SetMovingButtonLabel();

            combo_MoveTypes.Items.Add("Absolute");
            combo_MoveTypes.Items.Add("Relative");
            combo_MoveTypes.SelectedIndex = 1;

            int bServoOnOff = HWinRobot.get_motor_state(HiWinRobotInterface.m_DeviceID);
            //toggle_ServoOnOff.IsChecked = bServoOnOff == 0 ? false : true;

            //m_nAccRatioPercentValue = 10;
            //m_PTPSpeedPercentValue = 10;
            //m_nLinearSpeedValue = 10;
            //m_nOverridePercent = 10;
            //m_nStepRelativeValue = 1000;

            //slider_AccRatioPercent.Value = 10;
            //slider_LinearSpeed.Value = 100;
            //slider_PTPSpeedPercent.Value = 10;
            //slider_OverridePercent.Value = 10;
            //slider_StepRelative.Value = 1000;

            slider_AccRatioPercentShow.Text = "10";
            slider_LinearSpeedShow.Text = "100";
            slider_PTPSpeedPercentShow.Text = "10";
            slider_OverridePercentShow.Text = "10";
            slider_StepRelativeShow.Text = "1000";

            this.DataContext = this;
        }

        private string _m_txtRobotIPAddress = "127.0.0.1";

        public string txtRobotIPAddress
        {
            get { return _m_txtRobotIPAddress; }
            set
            {
                _m_txtRobotIPAddress = value;
                OnPropertyChanged("txtRobotIPAddress");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private int _m_nAccRatioPercentValue = 1;
        public int m_nAccRatioPercentValue
        {
            get { return _m_nAccRatioPercentValue; }
            set
            {
                _m_nAccRatioPercentValue = value;
                OnPropertyChanged("m_nAccRatioPercentValue");
            }
        }

        private int _m_PTPSpeedPercentValue = 1;
        public int m_PTPSpeedPercentValue
        {
            get { return _m_PTPSpeedPercentValue; }
            set
            {
                _m_PTPSpeedPercentValue = value;
                OnPropertyChanged("m_PTPSpeedPercentValue");
            }
        }


        private int _m_nLinearSpeedValue = 1;
        public int m_nLinearSpeedValue
        {
            get { return _m_nLinearSpeedValue; }
            set
            {
                _m_nLinearSpeedValue = value;
                OnPropertyChanged("m_nLinearSpeedValue");
            }
        }





        private int _m_nOverridePercent = 1;
        public int m_nOverridePercent
        {
            get { return _m_nOverridePercent; }
            set
            {
                _m_nOverridePercent = value;
                OnPropertyChanged("m_nOverridePercent");
            }
        }

        private int _m_nStepRelativeValue = 0;
        public int m_nStepRelativeValue
        {
            get { return _m_nStepRelativeValue; }
            set
            {
                _m_nStepRelativeValue = value;
                OnPropertyChanged("m_nStepRelativeValue");
            }
        }


        private void slider_AccRatioPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider_AccRatioPercent.Value < slider_AccRatioPercent.Minimum)
            { _m_nAccRatioPercentValue = (int)slider_AccRatioPercent.Minimum;
            }
            else if (slider_AccRatioPercent.Value <= slider_AccRatioPercent.Maximum)
            {
                _m_nAccRatioPercentValue = (int)slider_AccRatioPercent.Maximum;
            }
        }

        private void slider_AccRatioPercentShow_TextChanged(object sender, TextChangedEventArgs e)
        {
            double dvalue = 0.0;
            bool error = Double.TryParse(slider_AccRatioPercentShow.Text, out dvalue);
            slider_AccRatioPercentShow.Text = Math.Round(dvalue).ToString();

            Int16 val = Convert.ToInt16(slider_AccRatioPercentShow.Text);
            HWinRobot.set_acc_dec_ratio(HiWinRobotInterface.m_DeviceID, val);
        }

        private void slider_PTPSpeedPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider_PTPSpeedPercent.Value < slider_PTPSpeedPercent.Minimum)
            {
                _m_PTPSpeedPercentValue = (int)slider_PTPSpeedPercent.Minimum;
            }
            else if (slider_PTPSpeedPercent.Value <= slider_PTPSpeedPercent.Maximum)
            {
                _m_PTPSpeedPercentValue = (int)slider_PTPSpeedPercent.Maximum;
            }
        }

        private void slider_PTPSpeedPercentShow_TextChanged(object sender, TextChangedEventArgs e)
        {
            double dvalue = 0.0;
            bool error = Double.TryParse(slider_PTPSpeedPercentShow.Text, out dvalue);
            slider_PTPSpeedPercentShow.Text = Math.Round(dvalue).ToString();
            Int16 val = Convert.ToInt16(slider_PTPSpeedPercentShow.Text);
            HWinRobot.set_ptp_speed(HiWinRobotInterface.m_DeviceID, val);
        }

        private void slider_LinearSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider_LinearSpeed.Value < slider_LinearSpeed.Minimum)
            {
                _m_nLinearSpeedValue = (int)slider_LinearSpeed.Minimum;
            }
            else if (slider_LinearSpeed.Value <= slider_LinearSpeed.Maximum)
            {
                _m_nLinearSpeedValue = (int)slider_LinearSpeed.Maximum;
            }
        }

        private void slider_LinearSpeedShow_TextChanged(object sender, TextChangedEventArgs e)
        {
            double dvalue = 0.0;
            bool error = Double.TryParse(slider_LinearSpeedShow.Text, out dvalue);
            slider_LinearSpeedShow.Text = Math.Round(dvalue).ToString();
            Int16 val = Convert.ToInt16(slider_LinearSpeedShow.Text);
            HWinRobot.set_lin_speed(HiWinRobotInterface.m_DeviceID, val);
        }

        private void slider_OverridePercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider_OverridePercent.Value < slider_OverridePercent.Minimum)
            {
                _m_nOverridePercent = (int)slider_OverridePercent.Minimum;
            }
            else if (slider_OverridePercent.Value <= slider_OverridePercent.Maximum)
            {
                _m_nOverridePercent = (int)slider_OverridePercent.Maximum;
            }
        }

        private void slider_OverridePercentShow_TextChanged(object sender, TextChangedEventArgs e)
        {
            double dvalue = 0.0;
            bool error = Double.TryParse(slider_OverridePercentShow.Text, out dvalue);
            slider_OverridePercentShow.Text = Math.Round(dvalue).ToString();
            Int16 val = Convert.ToInt16(slider_OverridePercentShow.Text);
            HWinRobot.set_override_ratio(HiWinRobotInterface.m_DeviceID, val);
        }

        private void toggle_ServoOnOff_Click(object sender, RoutedEventArgs e)
        {
            HWinRobot.jog_stop(HiWinRobotInterface.m_DeviceID);
            LogMessage.LogMessage.WriteToDebugViewer(2, "Stop Move");

            int bServoOnOff = HWinRobot.get_motor_state(HiWinRobotInterface.m_DeviceID);
            if (bServoOnOff == 0)
            {
                HWinRobot.set_motor_state(HiWinRobotInterface.m_DeviceID, 1);
                toggle_ServoOnOff.Background = new SolidColorBrush(Colors.Green);
                toggle_ServoOnOff.Content = "Servo ON";

            }
            else
            {
                HWinRobot.set_motor_state(HiWinRobotInterface.m_DeviceID, 0);
                toggle_ServoOnOff.Background = new SolidColorBrush(Colors.Gray);
                toggle_ServoOnOff.Content = "Servo OFF";

            }
            //toggle_ServoOnOff.IsChecked = bServoOnOff == 0 ? false : true;
        }

        public void SetMotorSpeed()
        {
            HWinRobot.set_acc_dec_ratio(HiWinRobotInterface.m_DeviceID, Convert.ToInt16(m_nAccRatioPercentValue));
            HWinRobot.set_ptp_speed(HiWinRobotInterface.m_DeviceID, Convert.ToInt16(m_PTPSpeedPercentValue));
            HWinRobot.set_lin_speed(HiWinRobotInterface.m_DeviceID, Convert.ToInt16(m_nLinearSpeedValue));
            HWinRobot.set_override_ratio(HiWinRobotInterface.m_DeviceID, Convert.ToInt16(m_nOverridePercent));
        }

        public List<SequencePointData> m_List_sequencePointData = new List<SequencePointData>();

        private void button_Add_Point_To_Sequence_Click(object sender, RoutedEventArgs e)
        {
            SetMotorSpeed();
            m_List_sequencePointData.Add(HiWinRobotInterface.AddSequencePointInfo(HiWinRobotInterface.m_DeviceID, m_List_sequencePointData.Count, "123"));
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (MainWindow.mainWindow.master == null)
                    return;
                dataGrid_all_robot_Positions.ItemsSource = null;
                dataGrid_all_robot_Positions.ItemsSource = m_List_sequencePointData;
            });
        }
        private void button_Delete_Point_To_Sequence_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_all_robot_Positions.SelectedIndex >= m_List_sequencePointData.Count || dataGrid_all_robot_Positions.SelectedIndex < 0)
                return;

            m_List_sequencePointData.RemoveAt(dataGrid_all_robot_Positions.SelectedIndex);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (MainWindow.mainWindow.master == null)
                    return;
                dataGrid_all_robot_Positions.ItemsSource = null;
                dataGrid_all_robot_Positions.ItemsSource = m_List_sequencePointData;
            });
        }


        private void button_setTo_ChoosenPos_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_all_robot_Positions.SelectedIndex >= m_List_sequencePointData.Count || dataGrid_all_robot_Positions.SelectedIndex < 0)
                return;

            SetMotorSpeed();
            m_List_sequencePointData[dataGrid_all_robot_Positions.SelectedIndex] = HiWinRobotInterface.AddSequencePointInfo(HiWinRobotInterface.m_DeviceID, m_List_sequencePointData.Count, "123");
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (MainWindow.mainWindow.master == null)
                    return;
                dataGrid_all_robot_Positions.ItemsSource = null;
                dataGrid_all_robot_Positions.ItemsSource = m_List_sequencePointData;
            });
        }

        private void button_Save_Sequence_Click(object sender, RoutedEventArgs e)
        {

        }

        int mComboSelectedItem_Backup = 0;
        string[] m_strXYZMovingButtonLabel = { "X", "Y", "Z", "RTZ"};
        string[] m_strJointMovingButtonLabe = { "A1", "A2", "A3", "A4"};

        private void combo_JogType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(combo_JogType.SelectedIndex != mComboSelectedItem_Backup)
            {
                mComboSelectedItem_Backup = combo_JogType.SelectedIndex;
                SetMovingButtonLabel();
            }
        }

        public void SetMovingButtonLabel()
        {
            if (combo_JogType == null)
                return;

            if (combo_JogType.Items.Count == 0)
                return;

            if (combo_JogType.SelectedIndex < 0)
            {
                combo_JogType.SelectedIndex = 0;
            }

            if (combo_JogType.SelectedItem.ToString() == "XYZ")
            {
                label_move_Motor1.Content = m_strXYZMovingButtonLabel[0];
                label_move_Motor2.Content = m_strXYZMovingButtonLabel[1];
                label_move_Motor3.Content = m_strXYZMovingButtonLabel[2];
                label_move_Motor4.Content = m_strXYZMovingButtonLabel[3];


            }
            else
            {
                label_move_Motor1.Content = m_strJointMovingButtonLabe[0];
                label_move_Motor2.Content = m_strJointMovingButtonLabe[1];
                label_move_Motor3.Content = m_strJointMovingButtonLabe[2];
                label_move_Motor4.Content = m_strJointMovingButtonLabe[3];

            }

        }

        private void combo_MoveType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
     
        private void slider_StepRelativeShow_TextChanged(object sender, TextChangedEventArgs e)
        {
            double dvalue = 0.0;
            Double.TryParse(slider_StepRelativeShow.Text, out dvalue);
            m_nStepRelativeValue = (int)Math.Round(dvalue);
            slider_StepRelativeShow.Text = m_nStepRelativeValue.ToString();
        }

        private void dataGrid_all_robot_Positions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dataGrid_all_robot_Positions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void button_Stop_Moving_Click(object sender, RoutedEventArgs e)
        {
            HWinRobot.jog_stop(HiWinRobotInterface.m_DeviceID);
            LogMessage.LogMessage.WriteToDebugViewer(2, "Stop Move Clicked");
        }

        Thread m_Thread;
        private void button_negative_Move1_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            int nIndex = combo_MoveTypes.SelectedIndex;
            m_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => MoveMotor(0, nIndex, -1)));
            m_Thread.Start();
        }

        private void button_positive_Move1_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            int nIndex = combo_MoveTypes.SelectedIndex;
            m_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => MoveMotor(0, nIndex, 1)));
            m_Thread.Start();
        }

        private void button_negative_Move2_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            int nIndex = combo_MoveTypes.SelectedIndex;
            m_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => MoveMotor(1, nIndex, -1)));
            m_Thread.Start();
        }

        private void button_positive_Move2_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            int nIndex = combo_MoveTypes.SelectedIndex;
            m_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => MoveMotor(1, nIndex, 1)));
            m_Thread.Start();
        }


        private void button_negative_Move3_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            int nIndex = combo_MoveTypes.SelectedIndex;
            m_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => MoveMotor(2, nIndex, -1)));
            m_Thread.Start();
        }

        private void button_positive_Move3_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            int nIndex = combo_MoveTypes.SelectedIndex;
            m_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => MoveMotor(2, nIndex, 1)));
            m_Thread.Start();
        }

        private void button_negative_Move4_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            int nIndex = combo_MoveTypes.SelectedIndex;
            m_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => MoveMotor(5, nIndex, -1)));
            m_Thread.Start();
        }

        private void button_positive_Move4_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            int nIndex = combo_MoveTypes.SelectedIndex;
            m_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => MoveMotor(5, nIndex, 1)));
            m_Thread.Start();
        }

        public void MoveMotor(int nMotorID, int nType, int ndirection)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (HWinRobot.get_motion_state(HiWinRobotInterface.m_DeviceID) != 1)
                    return;

                //MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion(HiWinRobotInterface.m_DeviceID);
                if (combo_MoveTypes.SelectedIndex == (int)MOVETYPES.AbsoluteMove)
                {
                    double[] dValue = new double[6];
                    HWinRobot.get_current_position(HiWinRobotInterface.m_DeviceID, dValue);
                    dValue[nMotorID] = (Double)(m_nStepRelativeValue / 1000.0);
                    HWinRobot.ptp_pos(HiWinRobotInterface.m_DeviceID, 0, dValue);
                }
                else
                {
                    double[] dValue = { 0, 0, 0, 0, 0, 0 };
                    dValue[nMotorID] = (Double)(Math.Abs(m_nStepRelativeValue) / 1000.0) * ndirection;
                    HWinRobot.ptp_rel_pos(HiWinRobotInterface.m_DeviceID, 0, dValue);
                }
                //dValue[nMotorID] += (Double)(Math.Abs(m_nStepRelativeValue) / 1000.0) * ndirection;

                //MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion(HiWinRobotInterface.m_DeviceID);
                //double[] dValue2 = { 2, 0, 0, 0, 0, 1};
                //HWinRobot.ptp_rel_pos(HiWinRobotInterface.m_DeviceID, 0, dValue2);
            });
        }

        private void button_Home_Move_Click(object sender, RoutedEventArgs e)
        {
            int nIndex = combo_MoveTypes.SelectedIndex;
            m_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => MainWindow.mainWindow.master.m_hiWinRobotInterface.HomeMove()));
            m_Thread.Start();

        }

        private void button_ResetAlarm_Click(object sender, RoutedEventArgs e)
        {
            HWinRobot.clear_alarm(HiWinRobotInterface.m_DeviceID);
            label_Alarm.Content = "";

        }

        public bool b_button_RobotConnect = true;
        private void button_RobotConnect_Checked(object sender, RoutedEventArgs e)
        {
            b_button_RobotConnect = (bool)button_RobotConnect.IsChecked;
        }

        private void button_RobotConnect_Unchecked(object sender, RoutedEventArgs e)
        {
            b_button_RobotConnect = (bool)button_RobotConnect.IsChecked;

        }
    }
}
