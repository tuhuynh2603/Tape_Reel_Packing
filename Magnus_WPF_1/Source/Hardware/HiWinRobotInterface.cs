using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magnus_WPF_1.Source.Hardware.SDKHrobot
{
    public class HiWinRobotInterface 
    {

        public Thread m_hikThread;

        public class positionData
        {
            public string m_field { get; set; }
            public double m_value { get; set; }
            public string m_unit { get; set; }
        }

        public class SequencePointData
        {
            public int m_PointIndex { get; set; }
            public string m_PointComment { get; set; }
            public double m_X { get; set; }
            public double m_Y { get; set; }
            public double m_Z { get; set; }
            public double m_Rx { get; set; }
            public double m_Ry { get; set; }
            public double m_Rz { get; set; }
            public int m_Base { get; set; }
            public int m_Tool { get; set; }
            public int m_AccRatio { get; set; }
            public int m_PTPSpeed { get; set; }
            public double m_LinearSpeed { get; set; }
            public int m_Override { get; set; }

            public SequencePointData(int pointIdex, double[] d_value, int AccRatio, int PTPSpeed, double LinearSpeed, int Override, string PointComment = "")
            {
                m_PointIndex = pointIdex;
                m_PointComment = PointComment;
                m_X = d_value[0];
                m_Y = d_value[1];
                m_Z = d_value[2];
                m_Rx = d_value[3];
                m_Ry = d_value[4];
                m_Rz = d_value[5];
                m_AccRatio = AccRatio;
                m_PTPSpeed = PTPSpeed;
                m_LinearSpeed = LinearSpeed;
                m_Override = Override;
            }
        }

        public HiWinRobotUserControl m_hiWinRobotUserControl;
        public string m_strAddress = "";


        public void InitRobotParameter()
        {
            m_strAddress = GetCommInfo("Comm::IpAddress", m_strAddress);
        }

        public static string GetCommInfo(string key, string defaults)
        {
            RegistryKey registerPreferences = Registry.CurrentUser.CreateSubKey(Application.Application.pathRegistry + "\\Comm", true);
            if ((string)registerPreferences.GetValue(key) == null)
            {
                registerPreferences.SetValue(key, defaults);
                return defaults;
            }
            else
                return (string)registerPreferences.GetValue(key);
        }


        public HiWinRobotInterface()
        {
            InitRobotParameter();
            m_hiWinRobotUserControl = new HiWinRobotUserControl(m_strAddress);
            ConnectoHIKRobot(m_strAddress);
        }
        public void ConnectoHIKRobot(string strAddress = "127.0.0.1")
        {

            m_strAddress = strAddress;
            int device_id = -1;
            try
            {
                device_id = HWinRobot.open_connection(strAddress, 1, callback);
                HWinRobot.clear_alarm(device_id);
            }
            catch (Exception e)
            {
                LogMessage.LogMessage.WriteToDebugViewer(1, "connect failed.");

            };

            if (device_id >= 0)
            {
                m_DeviceID = device_id;
                int client_L = -1, client_s = -1, client_rev = -1;
                int server_L = -1, server_s = -1, server_rev = -1;
                LogMessage.LogMessage.WriteToDebugViewer(1, "connect successful.");
                StringBuilder v = new StringBuilder(100);
                HWinRobot.get_hrsdk_version(v);
                LogMessage.LogMessage.WriteToDebugViewer(1, "HRSDK Release Ver:" + v);

                HWinRobot.get_hrsdk_sdkver(ref client_L, ref client_s, ref client_rev);
                HWinRobot.get_hrss_sdkver(device_id, ref server_L, ref server_s, ref server_rev);
                LogMessage.LogMessage.WriteToDebugViewer(1, $"HRSDK Connection Ver: {client_L}.{client_s}.{client_rev}");
                LogMessage.LogMessage.WriteToDebugViewer(1, $"HRSS Connection Ver: {server_L}.{server_s}.{server_rev}");
                if (client_L != server_L)
                {
                    LogMessage.LogMessage.WriteToDebugViewer(1, "Please update software.");
                }
                else if (client_s != server_s)
                {
                    LogMessage.LogMessage.WriteToDebugViewer(1, "Some APIs not support.");
                }

                int level = HWinRobot.get_connection_level(device_id);
                LogMessage.LogMessage.WriteToDebugViewer(1, "level:" + level);

                //HWinRobot.set_connection_level(device_id, 1);
                //level = HWinRobot.get_connection_level(device_id);
                //LogMessage.LogMessage.WriteToDebugViewer(1, "level:" + level);

                //Disconnect(device_id);
                InitDataGridview(device_id, true);

                System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
                dispatcherTimer.Start();

                //HWinRobot.set_connection_level(device_id, 1);
                //level = HWinRobot.get_connection_level(device_id);
                //LogMessage.LogMessage.WriteToDebugViewer(1, "level:" + level);

                m_hikThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => Thread_function(0)));
                m_hikThread.Start();
            }
            else
            {
                LogMessage.LogMessage.WriteToDebugViewer(1, "connect failure.");
            }
        }



        public List<positionData> m_ListPositionData = new List<positionData>();
        public List<positionData> m_ListVelocityData = new List<positionData>();
        public List<positionData> m_ListInputData = new List<positionData>();
        public List<positionData> m_ListOutputData = new List<positionData>();
        public static int m_DeviceID = -1;
        public void InitDataGridview(int device_id, bool breset = false)
        {
            if (breset)
            {
                m_ListPositionData.Clear();
                double[] d_value = new double[6];
                HWinRobot.get_current_position(device_id, d_value);
                m_ListPositionData.Add(new positionData() { m_field = "X", m_value = d_value[0], m_unit = "mm" });
                m_ListPositionData.Add(new positionData() { m_field = "Y", m_value = d_value[1], m_unit = "mm" });
                m_ListPositionData.Add(new positionData() { m_field = "Z", m_value = d_value[2], m_unit = "mm" });
                m_ListPositionData.Add(new positionData() { m_field = "A", m_value = d_value[3], m_unit = "mm" });
                m_ListPositionData.Add(new positionData() { m_field = "B", m_value = d_value[4], m_unit = "mm" });
                m_ListPositionData.Add(new positionData() { m_field = "C", m_value = d_value[5], m_unit = "mm" });


                int[] n_value = new int[6];
                HWinRobot.get_encoder_count(device_id, n_value);
                m_ListPositionData.Add(new positionData() { m_field = "Motor1", m_value = n_value[0], m_unit = "degree" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor2", m_value = n_value[1], m_unit = "degree" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor3", m_value = n_value[2], m_unit = "degree" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor4", m_value = n_value[3], m_unit = "degree" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor5", m_value = n_value[4], m_unit = "degree" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor6", m_value = n_value[5], m_unit = "degree" });

                m_ListInputData.Clear();

                for (int nIO = 1; nIO < 16; nIO++)
                {
                    m_ListInputData.Add(new positionData() { m_field = "DI " + nIO.ToString(), m_value = HWinRobot.get_digital_input(device_id, nIO), m_unit = "" });
                }

                for (int nIO = 1; nIO < 16; nIO++)
                {
                    m_ListOutputData.Add(new positionData() { m_field = "DO " + nIO.ToString(), m_value = HWinRobot.get_digital_output(device_id, nIO), m_unit = "" });
                }

            }
            else
            {

                double[] d_value = new double[6];
                HWinRobot.get_current_position(device_id, d_value);
                m_ListPositionData[0].m_value = d_value[0];
                m_ListPositionData[1].m_value = d_value[1];
                m_ListPositionData[2].m_value = d_value[2];
                m_ListPositionData[3].m_value = d_value[3];
                m_ListPositionData[4].m_value = d_value[4];
                m_ListPositionData[5].m_value = d_value[5];

                int[] n_value = new int[6];
                HWinRobot.get_encoder_count(device_id, n_value);
                m_ListPositionData[6].m_value = n_value[0];
                m_ListPositionData[7].m_value = n_value[1];
                m_ListPositionData[8].m_value = n_value[2];
                m_ListPositionData[9].m_value = n_value[3];
                m_ListPositionData[10].m_value = n_value[4];
                m_ListPositionData[11].m_value = n_value[5];
                for (int nIO = 1; nIO < 16; nIO++)
                {
                    m_ListInputData[nIO - 1].m_value = HWinRobot.get_digital_input(device_id, nIO);
                }

                for (int nIO = 1; nIO < 16; nIO++)
                {
                    m_ListOutputData[nIO - 1].m_value = HWinRobot.get_digital_output(device_id, nIO);
                }

            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (MainWindow.mainWindow.master == null)
                    return;
                m_hiWinRobotUserControl.dataGrid_robot_Position.ItemsSource = null;
                m_hiWinRobotUserControl.dataGrid_robot_Position.ItemsSource = m_ListPositionData;
                m_hiWinRobotUserControl.dataGrid_robot_Input.ItemsSource = null;
                m_hiWinRobotUserControl.dataGrid_robot_Input.ItemsSource = m_ListInputData;
                m_hiWinRobotUserControl.dataGrid_robot_Output.ItemsSource = null;
                m_hiWinRobotUserControl.dataGrid_robot_Output.ItemsSource = m_ListOutputData;
            });

        }

        public static void EventFun(UInt16 cmd, UInt16 rlt, ref UInt16 Msg, int len)
        {
            LogMessage.LogMessage.WriteToDebugViewer(1, "Command: " + cmd + " Resault: " + rlt);
        }
        private static HWinRobot.CallBackFun callback = new HWinRobot.CallBackFun(EventFun);

        public bool Thread_function(int device_id)
        {
            //while (true)
            //{
            //    if (MainWindow.mainWindow == null)
            //    {
            //        HWinRobot.disconnect(m_DeviceID);
            //        return false ;
            //    }

            //    if (MainWindow.isOpenCommLog)
            //    {
            //        wait_for_stop_motion(device_id);
            //        HWinRobot.jog_home(m_DeviceID);
            //        double[] cp1 = { -190, 400, -90, 0, 0, 90 };
            //        wait_for_stop_motion(device_id);
            //        MoveToPosition(device_id, 0, cp1);
            //        double[] cp2 = { -90, 350, -90, 0, 0, 0 };
            //        wait_for_stop_motion(device_id);
            //        MoveToPosition(device_id, 0, cp2);
            //        Thread.Sleep(5000);

            //    }
            //    Thread.Sleep(1000);

            //}
            return true;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (MainWindow.mainWindow == null)
                HWinRobot.disconnect(m_DeviceID);

            InitDataGridview(0);
            LogMessage.LogMessage.WriteToDebugViewer(1, "timer");
        }

        public void Disconnect(int device_id)
        {
            if (device_id >= 0)
            {
                HWinRobot.disconnect(device_id);
            }
        }



        public bool MoveToPosition(int ndeviceid, int nmode, double[] p)
        {

            HWinRobot.ptp_pos(ndeviceid, nmode, p);
            return wait_for_stop_motion(ndeviceid);
        }

        public bool MoveSingleJoint(int nDeviceid, int nmode, double[]p)
        {
            //HWinRobot.(ndeviceid, nmode, p);

            return wait_for_stop_motion(nDeviceid);
        }

        public static bool wait_for_stop_motion(int device_id)
        {
            while (HWinRobot.get_motion_state(device_id) != 1)
            {
                if (HWinRobot.get_connection_level(device_id) == -1)
                {
                    return false;  // The robot is not connected anymore
                }
                Thread.Sleep(10);
            }
            return true;
        }

        public static void wait_for_stop(int device_id)
        {
            while (HWinRobot.get_motion_state(device_id) != 1 && HWinRobot.get_connection_level(device_id) != -1)
            {
                Thread.Sleep(30);
            }
        }
        public static void SoftLimitExample(int device_id)
        {
            double[] joint_low_limit = { -20, -20, -35, -20, 0, 0 };
            double[] joint_high_limit = { 20, 20, 0, 0, 0, 0 };
            double[] cart_low_limit = { -100, 300, -100, 0, 0, 0 };
            double[] cart_high_limit = { 100, 450, -25, 0, 0, 0 };
            double[] cart_home = { 0, 400, 0, 0, -90, 0 };
            double[] joint_home = { 0, 0, 0, 0, -90, 0 };
            double[] now_pos = { 0, 0, 0, 0, 0, 0 };
            bool re_bool = false;
            HWinRobot.get_current_position(device_id, now_pos);

            // run joint softlimit
            HWinRobot.set_override_ratio(device_id, 100);
            HWinRobot.set_joint_soft_limit(device_id, joint_low_limit, joint_high_limit);
            HWinRobot.enable_joint_soft_limit(device_id, true);
            HWinRobot.enable_cart_soft_limit(device_id, false);
            HWinRobot.get_joint_soft_limit_config(device_id, ref re_bool, joint_low_limit, joint_high_limit);
            Console.WriteLine("Enable Joint SoftLimit: " + re_bool);
            HWinRobot.jog_home(device_id);
            wait_for_stop(device_id);
            Thread.Sleep(1000);
            for (int i = 0; i < 4; i++)
            {
                HWinRobot.jog(device_id, 1, i, -1);
                wait_for_stop(device_id);
                Console.WriteLine("On the limits of SoftLimit");
            }
            for (int i = 0; i < 4; i++)
            {
                HWinRobot.jog(device_id, 1, i, 1);
                wait_for_stop(device_id);
                Console.WriteLine("On the limits of SoftLimit");
            }
            HWinRobot.enable_joint_soft_limit(device_id, false);

            // run cartesian softlimit
            HWinRobot.ptp_axis(device_id, 0, joint_home);
            wait_for_stop(device_id);
            HWinRobot.set_cart_soft_limit(device_id, cart_low_limit, cart_high_limit);
            HWinRobot.enable_cart_soft_limit(device_id, true);
            HWinRobot.get_cart_soft_limit_config(device_id, ref re_bool, cart_low_limit, cart_high_limit);
            Console.WriteLine("Enable Cart SoftLimit: " + re_bool);
            HWinRobot.lin_pos(device_id, 0, 0, cart_home);
            wait_for_stop(device_id);
            for (int i = 0; i < 3; i++)
            {
                HWinRobot.jog(device_id, 0, i, -1);
                wait_for_stop(device_id);
                Console.WriteLine("On the limits of SoftLimit");
                Console.WriteLine("");
                HWinRobot.clear_alarm(device_id);
                Thread.Sleep(2000);
            }
            for (int i = 0; i < 3; i++)
            {
                HWinRobot.jog(device_id, 0, i, 1);
                wait_for_stop(device_id);
                Console.WriteLine("On the limits of SoftLimit");
                Console.WriteLine("");
                HWinRobot.clear_alarm(device_id);
                Thread.Sleep(2000);
            }
            Console.WriteLine("End of motion");

            HWinRobot.enable_joint_soft_limit(device_id, false);
            HWinRobot.enable_cart_soft_limit(device_id, false);
        }

        public static SequencePointData AddSequencePointInfo(int device_id, int nIndex = 0, string strComment = "")
        {
            //m_List_sequencePointData.Add(sequenceData);

            double[] d_value = new double[6];
            HWinRobot.get_current_position(device_id, d_value);
            return new SequencePointData(nIndex, d_value, 
                                                                    HWinRobot.get_acc_dec_ratio(device_id), 
                                                                    HWinRobot.get_ptp_speed(device_id), 
                                                                    HWinRobot.get_lin_speed(device_id), 
                                                                    HWinRobot.get_override_ratio(device_id), strComment);
        }


        //public static void EventFun(UInt16 cmd, UInt16 rlt, ref UInt16 Msg, int len)
        //{
        //    Console.WriteLine("Command: " + cmd + " Resault: " + rlt);
        //}






    }
}
