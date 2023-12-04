using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Magnus_WPF_1.Source.Hardware.SDKHrobot
{
    public class HiWinRobotInterface 
    {
        public const int NUMBER_AXIS  = 4;
        public enum ROBOT_OPERATION_MODE
        {
            MODE_MANUAL = 0,
            MODE_AUTO = 1,
            MODE_NUMBER
        }
        public Thread m_hikThread;
        public bool m_bIsStop = false;
        public bool m_bMustConnectAgain = true;
        public int m_HRSSMode = -1;
        public class positionData
        {
            public string m_field { get; set; }
            public double m_value { get; set; }
            public string m_unit { get; set; }
        }
        public enum INPUT_IOROBOT
        {
            PLC_READY = 1,
            CHIPDETECT_SENSOR = 2,
            AIR_PRESSURESTATUS = 3,
            EMERGENCY_STATUS = 4,
            QUICK_STOP_STATUS = 5,
            RESET_STATUS = 6,
            IN_CONVEYER_ONOFF = 7

        }

        public enum OUTPUT_IOROBOT
        {
            ROBOT_AIR_ON = 1,
            ROBOT_AIR_OFF = 5,

            ROBOT_READY = 2,
            ROBOT_PLACE_DONE = 3,
            ROBOT_CONVEYER_ONOFF = 4,
            ROBOT_ARLAMP = 6,
            ROBOT_RESET = 7,
            ROBOT_HEART_BEAT = 8,
        }


        public class SequencePointData
        {
            public const string HOME_POSITION = "Home Position";
            public const string READY_POSITION = "Ready Position";
            public const string PRE_PICK_POSITION = "Pre Pick Position";
            public const string PICK_POSITION = "Pick Position";
            public const string PRE_PASS_PLACE_POSITION = "Pre Pass Place Position";
            public const string PASS_PLACE_POSITION = "Pass Place Position";
            public const string PRE_FAILED_PLACE_POSITION = "Pre Failed Place Position";
            public const string FAILED_PLACE_POSITION = "Failed Place Position";
            public const string CALIB_ROBOT_POSITION_1 = "Calib Robot Point 1";
            public const string CALIB_ROBOT_POSITION_2 = "Calib Robot Point 2";
            public const string CALIB_ROBOT_POSITION_3 = "Calib Robot Point 3";
            public const string CALIB_Vision_POSITION_1 = "Calib Vision Point 1";
            public const string CALIB_Vision_POSITION_2 = "Calib Vision Point 2";
            public const string CALIB_Vision_POSITION_3 = "Calib Vision Point 3";

            public int m_PointIndex { get; set; }
            public string m_PointComment { get; set; }
            public double m_X { get; set; }
            public double m_Y { get; set; }
            public double m_Z { get; set; }
            public double m_Rx { get; set; }
            public double m_Ry { get; set; }
            public double m_Rz { get; set; }

            public double m_Joint1{ get; set; }
            public double m_Joint2{ get; set; }
            public double m_Joint3{ get; set; }
            public double m_Joint4 { get; set; }
            public double m_Joint5 { get; set; }
            public double m_Joint6 { get; set; }

            public int m_Base { get; set; }
            public int m_Tool { get; set; }
            public int m_AccRatio { get; set; }
            public int m_PTPSpeed { get; set; }
            public double m_LinearSpeed { get; set; }
            public int m_Override { get; set; }

            public SequencePointData(int pointIdex = 0, double[] d_XYZvalue = null, double[] d_Jointvalue = null, int AccRatio = 0, int PTPSpeed = 0, double LinearSpeed = 0, int Override = 0, string PointComment = "")
            {
                if (d_XYZvalue == null)
                    return;
                m_PointIndex = pointIdex;
                m_PointComment = PointComment;
                m_X =  d_XYZvalue[0];
                m_Y =  d_XYZvalue[1];
                m_Z =  d_XYZvalue[2];
                m_Rx = d_XYZvalue[3];
                m_Ry = d_XYZvalue[4];
                m_Rz = d_XYZvalue[5];

                m_Joint1 = d_Jointvalue[0];
                m_Joint2 = d_Jointvalue[1];
                m_Joint3 = d_Jointvalue[2];
                m_Joint4 = d_Jointvalue[3];
                m_Joint5 = d_Jointvalue[4];
                m_Joint6 = d_Jointvalue[5];

                m_AccRatio = AccRatio;
                m_PTPSpeed = PTPSpeed;
                m_LinearSpeed = LinearSpeed;
                m_Override = Override;
            }
            public void GetXYZPoint(ref double[] dpos)
            {
                dpos[0] = m_X ;
                dpos[1] = m_Y ;
                dpos[2] = m_Z ;
                dpos[3] = m_Rx;
                dpos[4] = m_Ry;
                dpos[5] = m_Rz;
            }

            public void SetXYZPoint(double[] dpos)
            {
               m_X  = dpos[0];
               m_Y  = dpos[1];
               m_Z  = dpos[2];
               m_Rx = dpos[3];
               m_Ry = dpos[4];
               m_Rz = dpos[5];
            }

            public void GetJointPoint(ref double[] npos)
            {
                npos[0] = m_Joint1;
                npos[1] = m_Joint2;
                npos[2] = m_Joint3;
                npos[3] = m_Joint4;
                npos[4] = m_Joint5;
                npos[5] = m_Joint6;
            }

            public void SetJointPoint(double[] npos)
            {
               m_Joint1 = npos[0];
               m_Joint2 = npos[1];
               m_Joint3 = npos[2];
               m_Joint4 = npos[3];
               m_Joint5 = npos[4];
               m_Joint6 = npos[5];
            }


            public static void SaveRobotPointsToExcel(List<SequencePointData> data)
            {
                string strRecipePath = Path.Combine(Application.Application.pathRecipe, Application.Application.currentRecipe);
                string fullpath = Path.Combine(strRecipePath, "Robot Points" + ".cfg");

                string strDateTime = string.Format("({0}.{1}.{2}_{3}.{4}.{5})", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
                string backup_path = Path.Combine(strRecipePath, "Backup_Robot Points");
                if (!Directory.Exists(backup_path))
                    Directory.CreateDirectory(backup_path);
                
                string backup_fullpath = Path.Combine(backup_path, $"Robot Points {strDateTime}" + ".cfg");

                FileInfo file = new FileInfo(fullpath);

                if (!file.Exists)
                    file.Create();

                file.CopyTo(backup_fullpath);

                using (ExcelPackage package = new ExcelPackage(file))
                {

                    bool bCreated = false;
                    for (int n = 0; n < package.Workbook.Worksheets.Count; n++)
                        if (package.Workbook.Worksheets[n].Name == "Sheet_RobotPoints")
                        {
                            bCreated = true;
                            break;
                        }

                    if (!bCreated)
                        package.Workbook.Worksheets.Add("Sheet_RobotPoints");

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    // Header
                    int ncol = 1;
                    worksheet.Cells[1, ncol++].Value = "PointIndex";
                    worksheet.Cells[1, ncol++].Value = "PointComment";
                    worksheet.Cells[1, ncol++].Value = "X";
                    worksheet.Cells[1, ncol++].Value = "Y";
                    worksheet.Cells[1, ncol++].Value = "Z";
                    worksheet.Cells[1, ncol++].Value = "Rx";
                    worksheet.Cells[1, ncol++].Value = "Ry";
                    worksheet.Cells[1, ncol++].Value = "Rz";
                    worksheet.Cells[1, ncol++].Value = "Joint1";
                    worksheet.Cells[1, ncol++].Value = "Joint2";
                    worksheet.Cells[1, ncol++].Value = "Joint3";
                    worksheet.Cells[1, ncol++].Value = "Joint4";
                    worksheet.Cells[1, ncol++].Value = "Joint5";
                    worksheet.Cells[1, ncol++].Value = "Joint6";
                    worksheet.Cells[1, ncol++].Value = "Base";
                    worksheet.Cells[1, ncol++].Value = "Tool";
                    worksheet.Cells[1, ncol++].Value = "AccRatio";
                    worksheet.Cells[1, ncol++].Value = "PTPSpeed";
                    worksheet.Cells[1, ncol++].Value = "LinearSpeed";
                    worksheet.Cells[1, ncol++].Value = "Override";

                    // Data
                    int row = 2;
                    foreach (var item in data)
                    {
                        ncol = 1;
                        worksheet.Cells[row, ncol++].Value =item.m_PointIndex;
                        worksheet.Cells[row, ncol++].Value =item.m_PointComment;
                        worksheet.Cells[row, ncol++].Value =item.m_X;
                        worksheet.Cells[row, ncol++].Value =item.m_Y;
                        worksheet.Cells[row, ncol++].Value =item.m_Z;
                        worksheet.Cells[row, ncol++].Value =item.m_Rx;
                        worksheet.Cells[row, ncol++].Value =item.m_Ry;
                        worksheet.Cells[row, ncol++].Value =item.m_Rz;
                        worksheet.Cells[row, ncol++].Value =item.m_Joint1;
                        worksheet.Cells[row, ncol++].Value =item.m_Joint2;
                        worksheet.Cells[row, ncol++].Value =item.m_Joint3;
                        worksheet.Cells[row, ncol++].Value =item.m_Joint4;
                        worksheet.Cells[row, ncol++].Value =item.m_Joint5;
                        worksheet.Cells[row, ncol++].Value =item.m_Joint6;
                        worksheet.Cells[row, ncol++].Value =item.m_Base;
                        worksheet.Cells[row, ncol++].Value =item.m_Tool;
                        worksheet.Cells[row, ncol++].Value =item.m_AccRatio;
                        worksheet.Cells[row, ncol++].Value =item.m_PTPSpeed;
                        worksheet.Cells[row, ncol++].Value =item.m_LinearSpeed;
                        worksheet.Cells[row, ncol++].Value =item.m_Override;
                        row++;
                    }
                    package.Save();
                }
            }

            public static void ReadRobotPointsFromExcel(ref List<SequencePointData> result)
            {
                string strRecipePath = Path.Combine(Application.Application.pathRecipe, Application.Application.currentRecipe);
                string fullpath = Path.Combine(strRecipePath, "Robot Points" + ".cfg");

                FileInfo file = new FileInfo(fullpath);
                if (!file.Exists)
                    file.Create();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Use NonCommercial license if applicable
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    if (package.Workbook.Worksheets.Count == 0)
                        return;
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        SequencePointData item = new SequencePointData();
                        int ncol = 1;
                        item.m_PointIndex = Convert.ToInt32(worksheet.Cells[row, ncol++].Value);
                        item.m_PointComment = worksheet.Cells[row, ncol++].Value.ToString();
                        item.m_X = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Y = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Z = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Rx = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Ry = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Rz = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Joint1  = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Joint2  = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Joint3  = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Joint4  = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Joint5  = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Joint6 = Convert.ToDouble(worksheet.Cells[row, ncol++].Value);
                        item.m_Base = Convert.ToInt32(worksheet.Cells[row, ncol++].Value);
                        item.m_Tool = Convert.ToInt32(worksheet.Cells[row, ncol++].Value);
                        item.m_AccRatio = Convert.ToInt32(worksheet.Cells[row, ncol++].Value);
                        item.m_PTPSpeed = Convert.ToInt32(worksheet.Cells[row, ncol++].Value);
                        item.m_LinearSpeed = Convert.ToInt32(worksheet.Cells[row, ncol++].Value);
                        item.m_Override = Convert.ToInt32(worksheet.Cells[row, ncol++].Value);

                        int nIndexTemp = HiWinRobotUserControl.CheckPointExistOnList(ref result, item.m_PointComment);
                        if(nIndexTemp >=0)
                        {
                            result[nIndexTemp] = item;
                        }
                        else
                            result.Add(item);

                    }
                }
            }
        }

        public HiWinRobotUserControl m_hiWinRobotUserControl;
        public string m_strRobotIPAddress = "";


        public HiWinRobotInterface()
        {
            m_strRobotIPAddress = Application.Application.GetCommInfo("Robot Comm::IpAddress", m_strRobotIPAddress);
            ConnectoHIKRobot(m_strRobotIPAddress);
            m_hiWinRobotUserControl = new HiWinRobotUserControl(m_strRobotIPAddress);
            InitDataGridview(m_RobotConnectID, true);
            m_hikThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => Thread_function()));
            m_hikThread.Start();
            //dispatcherTimer.Tick += dispatcherTimer_Tick;
            //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            //dispatcherTimer.Start();
        }
        public void ConnectoHIKRobot(string strAddress = "127.0.0.1")
        {

            m_strRobotIPAddress = strAddress;
            m_RobotConnectID = -1;
            try
            {
                HWinRobot.disconnect(m_RobotConnectID);
                m_RobotConnectID = HWinRobot.open_connection(strAddress, 1, callback);
            }
            catch
            {
                LogMessage.LogMessage.WriteToDebugViewer(1, "connect failed.");

            };

            if (m_RobotConnectID >= 0)
            {
                HWinRobot.clear_alarm(m_RobotConnectID);

                int client_L = -1, client_s = -1, client_rev = -1;
                int server_L = -1, server_s = -1, server_rev = -1;
                LogMessage.LogMessage.WriteToDebugViewer(1, "connect successful.");
                StringBuilder v = new StringBuilder(100);
                HWinRobot.get_hrsdk_version(v);
                LogMessage.LogMessage.WriteToDebugViewer(1, "HRSDK Release Ver:" + v);

                HWinRobot.get_hrsdk_sdkver(ref client_L, ref client_s, ref client_rev);
                HWinRobot.get_hrss_sdkver(m_RobotConnectID, ref server_L, ref server_s, ref server_rev);
                // if HRSSMode != 3 it mean we cannot control robot by software
                // Mode 2: Auto
                // Mode 3: External control
                //Mode 0, mode 1: dont know
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

                int level = HWinRobot.get_connection_level(m_RobotConnectID);
                LogMessage.LogMessage.WriteToDebugViewer(1, "level:" + level);

                HWinRobot.set_connection_level(m_RobotConnectID, 1);
                HWinRobot.set_operation_mode(m_RobotConnectID, (int)ROBOT_OPERATION_MODE.MODE_AUTO);
                int nmode = HWinRobot.get_operation_mode(m_RobotConnectID);
                string strMode = nmode == (int)ROBOT_OPERATION_MODE.MODE_AUTO ? "AUTO" : "MANUAL";
                LogMessage.LogMessage.WriteToDebugViewer(1, "operation mode:" + strMode);
                //Disconnect(device_id);
                //HWinRobot.set_connection_level(device_id, 1);
                //level = HWinRobot.get_connection_level(device_id);
                //LogMessage.LogMessage.WriteToDebugViewer(1, "level:" + level);

                //m_hikThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => Thread_function(0)));
                //m_hikThread.Start();
            }
            else
            {
                LogMessage.LogMessage.WriteToDebugViewer(1, "connect failure.");
            }
        }

        public void ReconnectToHIKRobot()
        {
            //dispatcherTimer.Stop();
            try
            {
                HWinRobot.disconnect(m_RobotConnectID);               
                m_RobotConnectID = HWinRobot.open_connection(m_strRobotIPAddress, 1, callback);

            }
            catch
            {
                //LogMessage.LogMessage.WriteToDebugViewer(1, "connect failed.");

            };

            if (m_RobotConnectID >= 0)
            {

                HWinRobot.clear_alarm(m_RobotConnectID);
                int nHRSS = HWinRobot.get_hrss_mode(m_RobotConnectID);
                //LogMessage.LogMessage.WriteToDebugViewer(1, "HRSS mode " + nHRSS.ToString());

                if (nHRSS != 3)
                {
                    HWinRobot.disconnect(m_RobotConnectID);
                    m_RobotConnectID = -1;
                    LogMessage.LogMessage.WriteToDebugViewer(1, "connect failed.");
                    return;
                }

                LogMessage.LogMessage.WriteToDebugViewer(1, "connect successful.");
                StringBuilder v = new StringBuilder(100);
                HWinRobot.get_hrsdk_version(v);
                HWinRobot.set_connection_level(m_RobotConnectID, 1);
                int level = HWinRobot.get_connection_level(m_RobotConnectID);

                // Turn on Motor to allow Moving
                HWinRobot.set_motor_state(m_RobotConnectID, 1);
                //dispatcherTimer.Start();

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
        public static int m_RobotConnectID = -1;
        public void InitDataGridview(int device_id, bool breset = false)
        {
            bool[] bEnableUpddate = { false, false, false };
            if (breset)
            {
                bEnableUpddate[0] = true;
                bEnableUpddate[1] = true;
                bEnableUpddate[2] = true;
                m_ListPositionData.Clear();
                double[] d_value = new double[6];
                HWinRobot.get_current_position(device_id, d_value);
                m_ListPositionData.Add(new positionData() { m_field = "X", m_value = d_value[0], m_unit = "mm" });
                m_ListPositionData.Add(new positionData() { m_field = "Y", m_value = d_value[1], m_unit = "mm" });
                m_ListPositionData.Add(new positionData() { m_field = "Z", m_value = d_value[2], m_unit = "mm" });
                m_ListPositionData.Add(new positionData() { m_field = "Rx", m_value = d_value[3], m_unit = "deg" });
                m_ListPositionData.Add(new positionData() { m_field = "Ry", m_value = d_value[4], m_unit = "deg" });
                m_ListPositionData.Add(new positionData() { m_field = "Rz", m_value = d_value[5], m_unit = "deg" });


                double[] d_Jointvalue = new double[6];
                HWinRobot.get_current_joint(device_id, d_Jointvalue);
                m_ListPositionData.Add(new positionData() { m_field = "Motor1", m_value = d_Jointvalue[0], m_unit = "deg" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor2", m_value = d_Jointvalue[1], m_unit = "deg" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor3", m_value = d_Jointvalue[2], m_unit = "deg" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor4", m_value = d_Jointvalue[3], m_unit = "deg" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor5", m_value = d_Jointvalue[4], m_unit = "deg" });
                m_ListPositionData.Add(new positionData() { m_field = "Motor6", m_value = d_Jointvalue[5], m_unit = "deg" });

                m_ListInputData.Clear();

                for (int nIO = 0; nIO < 16; nIO++)
                {
                    m_ListInputData.Add(new positionData() { m_field = "DI " + (nIO + 1).ToString(), m_value = HWinRobot.get_digital_input(device_id, nIO + 1), m_unit = "" });
                }

                for (int nIO = 0; nIO < 16; nIO++)
                {
                    m_ListOutputData.Add(new positionData() { m_field = "DO " + (nIO + 1).ToString(), m_value = HWinRobot.get_digital_output(device_id, nIO + 1), m_unit = "" });
                }

            }
            else
            {

                double[] d_value = new double[6];
                HWinRobot.get_current_position(device_id, d_value);

                for (int n = 0; n < 6; n++)
                    if (m_ListPositionData[n].m_value != d_value[n])
                        bEnableUpddate[0] = true;

                m_ListPositionData[0].m_value = d_value[0];
                m_ListPositionData[1].m_value = d_value[1];
                m_ListPositionData[2].m_value = d_value[2];
                m_ListPositionData[3].m_value = d_value[3];
                m_ListPositionData[4].m_value = d_value[4];
                m_ListPositionData[5].m_value = d_value[5];



                double[] d_Jointvalue = new double[6];
                HWinRobot.get_current_joint(device_id, d_Jointvalue);

                for (int n = 0; n < 6; n++)
                    if (m_ListPositionData[n + 6].m_value != d_Jointvalue[n])
                        bEnableUpddate[0] = true;

                m_ListPositionData[6].m_value =  d_Jointvalue[0];
                m_ListPositionData[7].m_value =  d_Jointvalue[1];
                m_ListPositionData[8].m_value =  d_Jointvalue[2];
                m_ListPositionData[9].m_value =  d_Jointvalue[3];
                m_ListPositionData[10].m_value = d_Jointvalue[4];
                m_ListPositionData[11].m_value = d_Jointvalue[5];

                for (int nIO = 0; nIO < 16; nIO++)
                {
                    int nvalue = HWinRobot.get_digital_input(device_id, nIO + 1);
                    if ((int)m_ListInputData[nIO].m_value != nvalue)
                        bEnableUpddate[1] = true;

                    m_ListInputData[nIO].m_value = nvalue;
                }

                for (int nIO = 0; nIO < 16; nIO++)
                {

                    int nvalue = HWinRobot.get_digital_output(device_id, nIO + 1);
                    if ((int)m_ListOutputData[nIO].m_value != nvalue)
                        bEnableUpddate[2] = true;

                    m_ListOutputData[nIO].m_value = nvalue;
                }

            }


            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (MainWindow.mainWindow == null)
                    return; 
                if (MainWindow.mainWindow.master == null)
                    return;
                if (bEnableUpddate[0] || m_hiWinRobotUserControl.dataGrid_robot_Position.ItemsSource == null)
                {
                    m_hiWinRobotUserControl.dataGrid_robot_Position.ItemsSource = null;
                    m_hiWinRobotUserControl.dataGrid_robot_Position.ItemsSource = m_ListPositionData;
                }
                if (bEnableUpddate[1] || m_hiWinRobotUserControl.dataGrid_robot_Input.ItemsSource == null)
                {
                    m_hiWinRobotUserControl.dataGrid_robot_Input.ItemsSource = null;
                    m_hiWinRobotUserControl.dataGrid_robot_Input.ItemsSource = m_ListInputData;
                }
                if (bEnableUpddate[2] || m_hiWinRobotUserControl.dataGrid_robot_Output.ItemsSource == null)
                {
                    m_hiWinRobotUserControl.dataGrid_robot_Output.ItemsSource = null;
                    m_hiWinRobotUserControl.dataGrid_robot_Output.ItemsSource = m_ListOutputData;
                }


                if(HiWinRobotUserControl.m_strAlarmMessage.Length >0)
                {
                    lock (m_hiWinRobotUserControl.label_Alarm)
                    {
                        m_hiWinRobotUserControl.label_Alarm.Text = HiWinRobotUserControl.m_strAlarmMessage;

                    }
                }    
            });

        }

        public static void EventFun(UInt16 cmd, UInt16 rlt, ref UInt16 Msg, int len)
        {
            //Console.Clear();
            String info_p = "";
            unsafe
            {
                fixed (UInt16* p = &Msg)
                {
                    for (int i = 0; i < len; i++)
                    {
                        info_p += (char)p[i];
                    }
                }
            }

            if (rlt != 4702)
            {

                //LogMessage.LogMessage.WriteToDebugViewer(1,"Command: " + cmd + "  Result: " + rlt + "  Msg: " + info_p + "  len: " + len);
                HiWinRobotUserControl.m_strAlarmMessage = info_p;
            }
            if (cmd == 0)
            {
                string[] info = info_p.Split(',');
                switch (rlt)
                {
                    case 4030:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] HRSS alarm notify: " + info_p);

                        break;
                    case 4145:
                        LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] System Output" + info[0] + " : " + info[1]);
                        break;
                    case 4702:
                             //LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] Robot Information");

                        //Todo: Need to move to Main thread to prevent crash on Child 

                        //int nHRSSModeTemp = -1;
                        //int.TryParse(info[0], out nHRSSModeTemp);
                        ////if (MainWindow.mainWindow.master != null && nHRSSModeTemp != MainWindow.mainWindow.master.m_hiWinRobotInterface.m_HRSSMode)
                        ////{
                        ////    MainWindow.mainWindow.master.m_hiWinRobotInterface.m_HRSSMode = nHRSSModeTemp;
                        ////    if (MainWindow.mainWindow.master.m_hiWinRobotInterface.m_HRSSMode == 3)
                        ////    {
                        ////        MainWindow.mainWindow.master.m_hiWinRobotInterface.m_bMustConnectAgain = true;
                        ////    }
                        ////}
                        //LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] HRSS Mode: " + info[0]);
                        //if (info[0] == "3")
                        //{
                        //    LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Operation Mode: " + info[1]);
                        //    LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Override Ratio: " + info[2]);
                        //    LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Motor State: " + info[3]);
                        //    LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Exe File Name: " + info[4]);
                        //    LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Function Output: " + info[5]);
                        //    if (info[6] != "0")
                        //        LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Alarm Count: " + info[6]);
                        //    LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Keep Alive: " + info[7]);
                        //    if (info[8] == "2")
                        //        LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Motion Status: " + info[8]);

                        //    LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] payload: " + info[9]);
                        //    LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Speed: " + info[10]); // safe: 0   normal: 1
                        //    if (info[14] != "")
                        //    {
                        //        //LogMessage.LogMessage.WriteToDebugViewer(1, "[Notify] Move Done!. Position: " + info[11]);
                        //        LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] Move Done! Coor: {0}, {1}, {2}, {3}, {4}, {5} ", info[14], info[15], info[16], info[17], info[18], info[19]));
                        //        LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] Joint: {0}, {1}, {2}, {3}, {4}, {5} ", info[20], info[21], info[22], info[23], info[24], info[25]));
                        //    }
                        //    LogMessage.LogMessage.WriteToDebugViewer(1, "");
                        //}
                        break;
                    case 4703:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] Timer {0}: {1} ", Int32.Parse(info[0]) + 1, info[1]));
                        break;
                    case 4704:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] Counter {0}: {1}", Int32.Parse(info[0]) + 1, info[1]));
                        break;
                    case 4705:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] MI {0}: {1}", Int32.Parse(info[0]) + 1, info[1]));
                        break;
                    case 4706:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] MO {0}: {1}", Int32.Parse(info[0]) + 1, info[1]));
                        break;
                    case 4707:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] SI {0}: {1}", Int32.Parse(info[0]) + 1, info[1]));
                        break;
                    case 4708:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] SO {0}: {1}", Int32.Parse(info[0]) + 1, info[1]));
                        break;
                    case 4710:
                        ShowPRNotification(info);
                        break;
                    case 4711:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] DI {0}: {1}", Int32.Parse(info[0]), info[1]));
                        break;
                    case 4712:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] DO {0}: {1}", Int32.Parse(info[0]), info[1]));
                        break;
                    case 4714:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] Utilization start notify: " + info_p));
                        break;
                    case 4715:
                         LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] Utilization end notify: " + info_p));
                        break;
                }
            }
            else if (cmd == 13)
            {
                 LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] HRSS Disconnected!");
            }
            else if (cmd == 1450)
            {
                switch (rlt)
                {
                    case 4028:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] HRSS start clear alarm");
                        break;
                    case 4029:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] HRSS finish clear alarm");
                        break;
                }
            }
            else if (cmd == 1456)
            {
                 LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] SET_SPEED_LIMIT: " + Msg);
            }
            else if (cmd == 2161)
            {
                switch (rlt)
                {
                    case 0:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] DOWNLOAD_LOG_FILE: " + Msg);
                        break;
                    case 201:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] FILE_IS_NOT_EXIST: " + Msg);
                        break;
                }
            }
            else if (cmd == 4000)
            {
                switch (rlt)
                {
                    case 0:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] run ext task start cmd: ");
                        break;
                    case 201:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] ext task already exist.");
                        break;
                }
            }
            else if (cmd == 4001)
            {
                switch (rlt)
                {
                    case 2006:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] task start motion already exist.");
                        break;
                    case 4012:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] task start file name error.");
                        break;
                    case 4013:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] task start already exist.");
                        break;
                    case 4014:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] task start Run.");
                        break;
                }

            }
            else if (cmd == 4004)
            {
                switch (rlt)
                {
                    case 4018:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] task abort finish.");
                        break;
                }
            }
            else if (cmd == 4009)
            {
                switch (rlt)
                {
                    case 0:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] Download file.");
                        break;
                    case 201:
                         LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] File is not exist.");
                        break;
                }
            }
            else if (cmd == 4010)
            {
                 LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] Send file.");
            }
            else if (cmd == 4018)
            {
                 LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] SAVE_DATABASE");
            }
            else if (cmd == 4019)
            {
                 LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] LOAD_DATABASE");
            }
            else if (cmd == 4709)
            {
                 LogMessage.LogMessage.WriteToDebugViewer(1,"[Notify] Save module IO.");
            }

            void ShowPRNotification(string[] info)
            {
                int pr_len = Convert.ToInt32(info[0]);
                int pr_type = -1, pr_num = -1;

                for (int i = 0; i < pr_len; i++)
                {
                    pr_num = Convert.ToInt32(info[1 + 11 * i]);
                    pr_type = Convert.ToInt32(info[2 + 11 * i]);
                     LogMessage.LogMessage.WriteToDebugViewer(1, String.Format("[Notify] PR {0}: {1} \n" +
                                       "Pos: {2}, {3}, {4}, {5}, {6}, {7} Ext: {8}, {9}, {10}", pr_num, pr_type, // Type  1:Degree  2:Cartesian
                                       info[3 + 11 * i], info[4 + 11 * i], info[5 + 11 * i], info[6 + 11 * i], info[7 + 11 * i], info[8 + 11 * i],
                                       info[9 + 11 * i], info[10 + 11 * i], info[11 + 11 * i]));
                }
            }
        }
        private static HWinRobot.CallBackFun callback = new HWinRobot.CallBackFun(EventFun);

        public void Thread_function()
        {
            while (MainWindow.mainWindow != null)
            {
                //Connect button
                if (m_hiWinRobotUserControl.b_button_RobotConnect == true)
                {
                    if (m_RobotConnectID < 0)
                    {
                        ReconnectToHIKRobot();
                        Thread.Sleep(1000);

                    }
                    else if (MainWindow.isRobotControllerOpen)
                    {
                        InitDataGridview(m_RobotConnectID);

                    }

                    // Change mode during using software
                    if (HWinRobot.get_hrss_mode(m_RobotConnectID) != 3 && m_RobotConnectID >= 0)
                    {
                        HWinRobot.disconnect(m_RobotConnectID);
                        m_RobotConnectID = -1;

                    }
                }
                //Disconnect button
                else
                {
                    if (m_RobotConnectID >= 0)
                    {
                        HWinRobot.disconnect(m_RobotConnectID);
                        m_RobotConnectID = -1;
                    }
                            
                }

                if (System.Windows.Application.Current == null)
                    return;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (m_RobotConnectID >= 0)
                    {
                        m_hiWinRobotUserControl.button_RobotConnect.Content = "Connected";
                        m_hiWinRobotUserControl.button_RobotConnect.Background = new SolidColorBrush(Colors.Green);
                        MainWindow.mainWindow.label_Robot_Status.Background = new SolidColorBrush(Colors.Green);
                        MainWindow.mainWindow.label_Robot_Status.Content = m_strRobotIPAddress;
                        MainWindow.mainWindow.color_RobotStatus = "Black";
                    }
                    else
                    {
                        m_hiWinRobotUserControl.button_RobotConnect.Content = "Disconnected";
                        m_hiWinRobotUserControl.button_RobotConnect.Background = new SolidColorBrush(Colors.Gray);
                        MainWindow.mainWindow.color_RobotStatus = "Black";
                        MainWindow.mainWindow.label_Robot_Status.Content = m_strRobotIPAddress;
                        MainWindow.mainWindow.label_Robot_Status.Background = new SolidColorBrush(Colors.Gray);

                    }
                });




                Thread.Sleep(100);
            }
            HWinRobot.disconnect(m_RobotConnectID);
            return;
        }

        #region Pick Place Movement
        public int MoveTo_PRE_PICK_POSITION(System.Drawing.PointF robotPoint, double dDeltaAngle, bool bSetSpeed = false, int nmode = 0)
        {

            double[] dValue = new double[6];
            SequencePointData pData = m_hiWinRobotUserControl.GetPointData(SequencePointData.PRE_PICK_POSITION);
            pData.GetXYZPoint(ref dValue);
            if (bSetSpeed)
            {
                HWinRobot.set_acc_dec_ratio(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_AccRatio));
                HWinRobot.set_ptp_speed(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_PTPSpeed));
                HWinRobot.set_lin_speed(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_LinearSpeed));
                HWinRobot.set_override_ratio(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_Override));
            }

            dValue[0] = robotPoint.X;
            dValue[1] = robotPoint.Y;
            dValue[5] = dDeltaAngle + dValue[5];
            //wait_for_stop_motion(ndeviceid);
            if (CheckSoftLimit(HiWinRobotInterface.m_RobotConnectID, nmode, dValue) != 0)
            {
                LogMessage.LogMessage.WriteToDebugViewer(3, "Soft Limit Error at Position (X,Y,Z,Angle):  " + dValue[0].ToString() + ", " + dValue[1].ToString() + ", " + dValue[2].ToString() + ", " + dValue[5].ToString());
                return -1;
            }
            LogMessage.LogMessage.WriteToDebugViewer(2, $"Move to {SequencePointData.PRE_PICK_POSITION} (X Y Z Angle) = " + dValue[0].ToString() + ", " + dValue[1].ToString() + ", " + dValue[2].ToString() + ", " + dValue[5].ToString());

            return HWinRobot.ptp_pos(HiWinRobotInterface.m_RobotConnectID, nmode, dValue);

            //return wait_for_stop_motion(ndeviceid);
        }

        public int MoveTo_PICK_POSITION(System.Drawing.PointF robotPoint, double dDeltaAngle, bool bSetSpeed = false, int nmode = 0)
        {

            double[] dValue = new double[6];
            SequencePointData pData = m_hiWinRobotUserControl.GetPointData(SequencePointData.PICK_POSITION);
            pData.GetXYZPoint(ref dValue);
            if (bSetSpeed)
            {
                HWinRobot.set_acc_dec_ratio(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_AccRatio));
                HWinRobot.set_ptp_speed(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_PTPSpeed));
                HWinRobot.set_lin_speed(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_LinearSpeed));
                HWinRobot.set_override_ratio(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_Override));
            }
            dValue[0] = robotPoint.X;
            dValue[1] = robotPoint.Y;
            dValue[5] = dDeltaAngle + dValue[5];
            if (CheckSoftLimit(HiWinRobotInterface.m_RobotConnectID, nmode, dValue) != 0)
            {
                LogMessage.LogMessage.WriteToDebugViewer(3, "Soft Limit Error at Position (X,Y,Z,Angle):  " + dValue[0].ToString() + ", " + dValue[1].ToString() + ", " + dValue[2].ToString() + ", " + dValue[5].ToString());
                return -1;
            }
            LogMessage.LogMessage.WriteToDebugViewer(2, $"Move to {SequencePointData.PICK_POSITION} (X Y Z Angle) = " + dValue[0].ToString() + ", " + dValue[1].ToString() + ", " + dValue[2].ToString() + ", " + dValue[5].ToString());
            return HWinRobot.ptp_pos(HiWinRobotInterface.m_RobotConnectID, nmode, dValue);

        }

        public int MoveTo_STATIC_POSITION(string strPosition, bool bSetSpeed = false, int nmode = 0)
        {

            double[] dValue = new double[6];
            SequencePointData pData = m_hiWinRobotUserControl.GetPointData(strPosition);
            pData.GetJointPoint(ref dValue);
            if (bSetSpeed)
            {
                HWinRobot.set_acc_dec_ratio(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_AccRatio));
                HWinRobot.set_ptp_speed(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_PTPSpeed));
                HWinRobot.set_lin_speed(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_LinearSpeed));
                HWinRobot.set_override_ratio(HiWinRobotInterface.m_RobotConnectID, Convert.ToInt16(pData.m_Override));
            }
            LogMessage.LogMessage.WriteToDebugViewer(2, $"Move to {strPosition} (X Y Z Angle) = " + dValue[0].ToString() + ", " + dValue[1].ToString() + ", " + dValue[2].ToString() + ", " + dValue[5].ToString());
            if (CheckSoftLimit(HiWinRobotInterface.m_RobotConnectID, nmode, dValue) != 0)
            {
                LogMessage.LogMessage.WriteToDebugViewer(3, "Soft Limit Error at Position (X,Y,Z,Angle):  " + dValue[0].ToString() + ", " + dValue[1].ToString() + ", " + dValue[2].ToString() + ", " + dValue[5].ToString());
                return -1;
            }
            return HWinRobot.ptp_axis(HiWinRobotInterface.m_RobotConnectID, nmode, dValue);
        }

        #endregion

        public int wait_for_stop_motion()
        {
            while (HWinRobot.get_motion_state(HiWinRobotInterface.m_RobotConnectID) != 1 )
            {
                //robot connection changed => return false
                if (HWinRobot.get_connection_level(HiWinRobotInterface.m_RobotConnectID) <0)
                    return -1;
                if (m_bIsStop)
                {
                    
                    lock (this)
                    {
                        m_bIsStop = false;
                    }

                    return -1;

                }
                Thread.Sleep(5);
            }
            return 0;
        }


        public static void SetOnOffSoftLimit(int device_id, bool bEnable)
        {
            HWinRobot.enable_joint_soft_limit(device_id, bEnable);
            HWinRobot.enable_cart_soft_limit(device_id, bEnable);
        }
        public enum JOG_TYPE
        {
            JOG_XYZ,
            JOG_JOINT
        }

        public static int CheckSoftLimit(int nDevice_id, int nMode, double[] dMovingPoint)
        {
            bool re_bool = false;
            double[] low_limit = new double[6];
            double[] high_limit = new double[6];
            HWinRobot.get_cart_soft_limit_config(nDevice_id, ref re_bool, low_limit, high_limit);

            if (dMovingPoint[0] < low_limit[0] || dMovingPoint[0] > high_limit[0] ||
                dMovingPoint[1] < low_limit[1] || dMovingPoint[1] > high_limit[1] ||
                dMovingPoint[2] < low_limit[2] || dMovingPoint[2] > high_limit[2])
                return -1;

            return 0;
        }

        public static void GetSoftLimit (int device_id, int nMode, ref bool re_bool, ref double[] low_limit, ref double[] high_limit)
        {
            if (nMode == (int)JOG_TYPE.JOG_XYZ)
                HWinRobot.get_cart_soft_limit_config(device_id, ref re_bool, low_limit, high_limit);
            else if (nMode == (int)JOG_TYPE.JOG_JOINT)
                HWinRobot.get_joint_soft_limit_config(device_id, ref re_bool, low_limit, high_limit);
        }
        public static void SetSoftLimit(int device_id, double[] low_limit, double[] high_limit, int nMode)
        {
            if (nMode == (int)JOG_TYPE.JOG_XYZ)
                HWinRobot.set_cart_soft_limit(device_id, low_limit, high_limit);
            else if(nMode == (int)JOG_TYPE.JOG_JOINT)
                HWinRobot.set_joint_soft_limit(device_id, low_limit, high_limit);
        }

        public static bool SendTaskToRobotController(int device_id, string taskPath, string taskName)
        {

            if(taskName.Split('.').Length <=1)
                taskName = taskName + ".hrb";


            if (!Directory.Exists(taskPath))           
            {
                Directory.CreateDirectory(taskPath);
            }
            if (File.Exists(taskPath + taskName))
            {
                HWinRobot.send_file(device_id, taskPath + taskName, taskName);
                LogMessage.LogMessage.WriteToDebugViewer(1, "Send file to Robot Successfull");
                return false;
            }
            else
                LogMessage.LogMessage.WriteToDebugViewer(1, "File is not exist!");

            return true;
        }

        public static bool StartTask(int device_id, string taskName)
        {

            if (taskName.Split('.').Length <= 1)
                taskName = taskName + ".hrb";
            HWinRobot.task_start(device_id, taskName);

            return true;
        }

        public static bool EndTask(int device_id)
        {
            HWinRobot.task_abort(device_id);
            return true;
        }

        public static SequencePointData AddSequencePointInfo(int device_id, double[] d_XYZvalue, double[] d_Jointvalue, int nIndex = 0, string strComment = "")
        {
            //m_List_sequencePointData.Add(sequenceData);

            return new SequencePointData(nIndex, d_XYZvalue, d_Jointvalue,
                                                                    HWinRobot.get_acc_dec_ratio(device_id), 
                                                                    HWinRobot.get_ptp_speed(device_id), 
                                                                    HWinRobot.get_lin_speed(device_id), 
                                                                    HWinRobot.get_override_ratio(device_id), strComment);
        }


        public void StopMotor()
        {
            lock (this)
            {
                m_bIsStop = true;
            }
            HWinRobot.jog_stop(HiWinRobotInterface.m_RobotConnectID);
        }


        public static void HomeMove()
        {
            HWinRobot.jog_stop(m_RobotConnectID);
            MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion();
            HWinRobot.jog_home(m_RobotConnectID);
        }

        //public static void EventFun(UInt16 cmd, UInt16 rlt, ref UInt16 Msg, int len)
        //{
        //    Console.WriteLine("Command: " + cmd + " Resault: " + rlt);
        //}

    }
}
