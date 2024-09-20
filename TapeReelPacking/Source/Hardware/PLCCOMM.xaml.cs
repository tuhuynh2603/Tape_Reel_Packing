using System;
using System.Collections.Generic;
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
using EasyModbus;
using TapeReelPacking.Source.Helper;
using TapeReelPacking.UI.UserControls.View;
using TapeReelPacking.UI.UserControls.ViewModel;
namespace TapeReelPacking.Source.Hardware
{
    /// <summary>
    /// Interaction logic for PLCCOMM.xaml
    /// </summary>
    public partial class PLCCOMM : UserControl
    {
        public enum PLC_ADDRESS
        {
            //PLC_BARCODE_TRIGGER = 100,

            //PLC_BARCODE_CAPTURE_DONE = 101,
            PLC_CURRENT_ROBOT_CHIP_COUNT = 104,
            PLC_READY_STATUS = 106,
            PLC_BARCODE_READY =107,
            PLC_EMERGENCY_STATUS = 108,
            PLC_IMIDIATE_STATUS = 109,
            PLC_RESET_STATUS = 110,
            PLC_ROBOT_RESULT = 111,
            PLC_BARCODE_RESULT = 112,
            PLC_AIR_PRESS_RESULT = 113,
            PLC_ROBOT_RUNNING_STATUS = 114,

            //For Manual mode
            PLC_MANUAL_BARCODE_ENABLE = 123,
            PLC_MANUAL_BARCODE_TRIGGER = 124,

            PLC_MANUAL_BARCODE_RESULT_PASS = 125,
            PLC_MANUAL_BARCODE_RESULT_FAIL = 126,
            PLC_MANUAL_BARCODE_CAPTURE_BUSY = 127,
            PLC_MANUAL_BARCODE_COUNT = 128,
            PLC_CURRENT_BARCODE_CHIP_COUNT = 129,


            PLC_RESET_LOT = 131,
            PLC_RESET_LOT_ACK = 132,


        }

        public ModbusClient m_modbusClient;
        public string m_strCommAddress = "127.0.0.1";
        int m_PLCPort = 502;


        async Task InitTask()
        {
            Task t3 = new Task(
                () =>
                {
                    m_strCommAddress = FileHelper.GetCommInfo("PLC Comm", m_strCommAddress, Application.Application.pathRegistry);
                    m_PLCPort = int.Parse(FileHelper.GetCommInfo("PLC Port", m_PLCPort.ToString(), Application.Application.pathRegistry));
                    m_modbusClient = new ModbusClient(m_strCommAddress, 502);
                    updateConnectionStatus();

                    m_modbusClient.ConnectionTimeout = 10000;
                    //m_modbusServer.LocalIPAddress = ;
                    //m_modbusServer.Port = 502;
                    m_modbusClient.ConnectedChanged += M_modbusClient_ConnectedChanged;
                    try
                    {
                        m_modbusClient.Connect();
                    }
                    catch
                    {


                    }
                }
                );

            t3.Start();
            await t3;
            Thread.Sleep(1000);
            combo_PLC_Comm_Function.Items.Add("Write Value");
            combo_PLC_Comm_Function.Items.Add("Read Value");
            m_modbusClient.ReceiveDataChanged += M_modbusClient_ReceiveDataChanged;
            label_PLC_IPAddress.Content = $"IP: {m_strCommAddress} Port: {m_PLCPort}";
        }

        public PLCCOMM()
        {
            InitializeComponent();
            Task t3 = InitTask();
            Console.WriteLine("bbb");

        }

        private void M_modbusClient_ConnectedChanged(object sender)
        {
            updateConnectionStatus();

        }
        public void updateConnectionStatus()
        {
            if (System.Windows.Application.Current == null)
                return;


            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (m_modbusClient.Connected)
                {
                    //MainWindow.mainWindow.label_PLCCOMM_Status.Background = new SolidColorBrush(Colors.Green);
                    //MainWindow.mainWindow.label_PLCCOMM_Status.Content = $"{m_strCommAddress}:{m_PLCPort}";
                    //MainWindow.mainWindow.label_PLCCOMM_Status.Foreground = new SolidColorBrush(Colors.Black);
                    button_PLC_Connect.Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    //MainWindow.mainWindow.label_PLCCOMM_Status.Background = new SolidColorBrush(Colors.Red);
                    //MainWindow.mainWindow.label_PLCCOMM_Status.Content = $"{m_strCommAddress}:{m_PLCPort}";
                    //MainWindow.mainWindow.label_PLCCOMM_Status.Foreground = new SolidColorBrush(Colors.Black);
                    button_PLC_Connect.Background = new SolidColorBrush(Colors.Red);

                }
            });
        }

        private void M_modbusClient_ReceiveDataChanged(object sender)
        {
            
            //throw new NotImplementedException();
        }

        private void btn_SendToPLC_Click(object sender, RoutedEventArgs e)
        {
            int iadr = -1;
            int nTrack = MainWindowVM.activeImageDock.trackID;

            if (text_MemoryAdress.Text.Length > 0)
            {
                bool success = Int32.TryParse(text_MemoryAdress.Text, out iadr);
                if (success == false)
                    MessageBox.Show("Wrong format input");

                if (iadr > 65535)
                {
                    MessageBox.Show("Input out of range (0-65536)");
                    return;
                }
            }
            else
                MessageBox.Show("Wrong format input");


            if (iadr >0)
            {
                if (combo_PLC_Comm_Function.SelectedIndex == 0)
                {

                    int nValue = 0;
                    if (text_MemoryAdress_Status.Text.Length > 0)
                    {
                        bool success = Int32.TryParse(text_MemoryAdress_Status.Text, out nValue);
                        if (success == false)
                            MessageBox.Show("Wrong format input");
                    }
                    else
                        MessageBox.Show("Wrong format input");
                    WritePLCRegister(iadr, nValue);
                }
                else
                {
                    text_MemoryAdress_Status.Text = ReadPLCRegister(iadr).ToString();

                }
            }

            //ModbusServer.HoldingRegisters regs = m_modbusServer.holdingRegisters;
            //modbusClient.WriteSingleCoil(4, !modbusClient.ReadCoils(4, 1)[0]);
            //bool[] status = modbusClient.ReadCoils(4, 1);
        }

        public int ReadPLCRegister(int nAddress)
        {
            lock (m_modbusClient)
            {
                int brepeat = 0;
            RetryRead:
                Thread.Sleep(10);
                if (m_modbusClient.Connected)
                    try
                    {
                        int[] a = new int[10];

                        a = m_modbusClient.ReadHoldingRegisters(nAddress, 5);

                        return a[0];
                    }

                    catch
                    {
                        brepeat++;
                        Thread.Sleep(10);
                        if (brepeat > 10)
                            return -1;
                        try
                        {
                            m_modbusClient.Disconnect();
                            m_modbusClient.Connect();
                        }
                        catch
                        {
                            goto RetryRead;
                        }
                        int a = m_modbusClient.ConnectionTimeout;

                        goto RetryRead;
                    }

                else
                    return -1;
            }
        }

        public int WritePLCMultiRegister(int nAddress, int[] ival)
        {
            lock (m_modbusClient)
            {
                //int[] ival = new int[1];
                //ival[0] = nValue;
                int brepeat = 0;
            RetryWrite:
                Thread.Sleep(10);
                if (m_modbusClient.Connected)
                {
                    try
                    {
                        m_modbusClient.WriteMultipleRegisters(nAddress, ival);
                        Thread.Sleep(10);
                    }
                    catch (Exception e)
                    {
                        brepeat++;
                        Thread.Sleep(10);
                        if (brepeat > 10)
                            return -1;
                        try
                        {
                            m_modbusClient.Disconnect();
                            m_modbusClient.Connect();
                        }
                        catch
                        {
                            goto RetryWrite;
                        }

                        int a = m_modbusClient.ConnectionTimeout;

                        goto RetryWrite;
                    }
                }
                else
                    return -1;
                return 0;
            }
        }



        public int WritePLCRegister(int nAddress, int nValue)
        {
            lock (m_modbusClient)
            {
                int[] ival = new int[1];
                ival[0] = nValue;
                int brepeat = 0;
            RetryWrite:
                Thread.Sleep(10);
                if (m_modbusClient.Connected)
                {
                    try
                    {
                        m_modbusClient.WriteMultipleRegisters(nAddress, ival);
                        Thread.Sleep(10);
                    }
                    catch (Exception e)
                    {
                        brepeat++;
                        Thread.Sleep(10);
                        if (brepeat > 10)
                            return -1;
                        try
                        {
                            m_modbusClient.Disconnect();
                            m_modbusClient.Connect();
                        }
                        catch
                        {
                            goto RetryWrite;
                        }

                        int a = m_modbusClient.ConnectionTimeout;

                        goto RetryWrite;
                    }
                }
                else
                    return -1;
                return 0;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_PLC_Connect_Click(object sender, RoutedEventArgs e)
        {
            int nTrack = MainWindowVM.activeImageDock.trackID;
            if (!m_modbusClient.Connected)
                try
                {
                    m_modbusClient.Connect();

                }
                catch
                {
                    return;
                }
            else
                m_modbusClient.Disconnect();

        }



    }
}
