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
namespace Magnus_WPF_1.Source.Hardware
{
    /// <summary>
    /// Interaction logic for PLCCOMM.xaml
    /// </summary>
    public partial class PLCCOMM : UserControl
    {
        public enum PLC_ADDRESS
        {
            PLC_BARCODE_TRIGGER = 100,
            PLC_BARCODE_CAPTURE_DONE = 101,
            PLC_BARCODE_RESULT = 102,
            PLC_CURRENT_BARCODE_CHIP_COUNT = 103,
            PLC_CURRENT_ROBOT_CHIP_COUNT = 104,
            PLC_RESET_LOT = 105,
            PLC_READY_STATUS = 106,
        }

        public ModbusClient[] m_modbusClient = new ModbusClient[2];
        public string m_strCommAddress = "127.0.0.1";
        int m_PLCPort = 502;
        public PLCCOMM()
        {
            InitializeComponent();
            m_strCommAddress = Application.Application.GetCommInfo("PLC Comm", m_strCommAddress);
            m_PLCPort = int.Parse(Application.Application.GetCommInfo("PLC Port", m_PLCPort.ToString()));

            m_modbusClient[0] = new ModbusClient(m_strCommAddress, 502);
            m_modbusClient[0].ConnectionTimeout = 5000;
            //m_modbusServer.LocalIPAddress = ;
            //m_modbusServer.Port = 502;
            m_modbusClient[0].ConnectedChanged += M_modbusClient_ConnectedChanged;
            try {
                m_modbusClient[0].Connect();
            }
            catch
            {
               
            }
            combo_PLC_Comm_Function.Items.Add("Write Value");
            combo_PLC_Comm_Function.Items.Add("Read Value");
            m_modbusClient[0].ReceiveDataChanged += M_modbusClient_ReceiveDataChanged;
            label_PLC_IPAddress.Content = $"IP: {m_strCommAddress} Port: {m_PLCPort}";

        }

        private void M_modbusClient_ConnectedChanged(object sender)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (m_modbusClient[0].Connected)
                {
                    MainWindow.mainWindow.label_PLCCOMM_Status.Background = new SolidColorBrush(Colors.Green);
                    MainWindow.mainWindow.label_PLCCOMM_Status.Content = $"{m_strCommAddress}:{m_PLCPort}";
                    MainWindow.mainWindow.label_PLCCOMM_Status.Foreground = new SolidColorBrush(Colors.Black);
                    button_PLC_Connect.Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    MainWindow.mainWindow.label_PLCCOMM_Status.Background = new SolidColorBrush(Colors.Gray);
                    MainWindow.mainWindow.label_PLCCOMM_Status.Content = $"{m_strCommAddress}:{m_PLCPort}";
                    MainWindow.mainWindow.label_PLCCOMM_Status.Foreground = new SolidColorBrush(Colors.Red);
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
            int nTrack = MainWindow.activeImageDock.trackID;

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
                    WritePLCRegister(iadr, nValue, nTrack);
                }
                else
                {
                    text_MemoryAdress_Status.Text = ReadPLCRegister(iadr, nTrack).ToString();

                }
            }

            //ModbusServer.HoldingRegisters regs = m_modbusServer.holdingRegisters;
            //modbusClient.WriteSingleCoil(4, !modbusClient.ReadCoils(4, 1)[0]);
            //bool[] status = modbusClient.ReadCoils(4, 1);
        }

        public int ReadPLCRegister(int nAddress, int nTrack = 0)
        {
            int brepeat = 0;
            RetryRead:
            Thread.Sleep(10);
            if (m_modbusClient[0].Connected)
                try
                {
                    int[] a = new int[10];

                    a = m_modbusClient[0].ReadHoldingRegisters(nAddress, 5);

                    return a[0];
                }

                catch (Exception e)
                {
                    brepeat++;
                    Thread.Sleep(10);
                    if (brepeat > 5)
                        return -1;
                    try
                    {
                        m_modbusClient[0].Disconnect();
                        m_modbusClient[0].Connect();
                    }
                    catch
                    {
                        goto RetryRead;
                    }
                    int a = m_modbusClient[0].ConnectionTimeout;

                    goto RetryRead;
                }

            else
                return -1;
        }

        public int WritePLCRegister(int nAddress, int nValue, int nTrack = 0)
        {
            int[] ival = new int[1];
            ival[0] = nValue;
            int brepeat = 0;
        RetryWrite:
            Thread.Sleep(10);
            if (m_modbusClient[0].Connected)
            {
                try
                {
                    m_modbusClient[0].WriteMultipleRegisters(nAddress, ival);
                    Thread.Sleep(10);
                }
                catch(Exception e)
                {
                    brepeat++;
                    Thread.Sleep(10);
                    if (brepeat > 5)
                        return -1;
                    try
                    {
                        m_modbusClient[0].Disconnect();
                        m_modbusClient[0].Connect();
                    }
                    catch
                    {
                        goto RetryWrite;
                    }
                    int a = m_modbusClient[0].ConnectionTimeout;

                    goto RetryWrite;
                }
            }
            else
                return -1;
            return 0;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_PLC_Connect_Click(object sender, RoutedEventArgs e)
        {
            int nTrack = MainWindow.activeImageDock.trackID;
            if (!m_modbusClient[0].Connected)
                try
                {
                    m_modbusClient[0].Connect();

                }
                catch
                {
                    return;
                }
            else
                m_modbusClient[0].Disconnect();

        }
    }
}
