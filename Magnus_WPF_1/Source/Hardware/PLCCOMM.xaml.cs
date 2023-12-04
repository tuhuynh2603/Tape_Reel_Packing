using System;
using System.Collections.Generic;
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
using EasyModbus;
namespace Magnus_WPF_1.Source.Hardware
{
    /// <summary>
    /// Interaction logic for PLCCOMM.xaml
    /// </summary>
    public partial class PLCCOMM : UserControl
    {
        ModbusClient m_modbusClient;
        public string m_strCommAddress = "127.0.0.1";
        int m_PLCPort = 502;
        public PLCCOMM()
        {
            InitializeComponent();
            m_strCommAddress = Application.Application.GetCommInfo("PLC Comm", m_strCommAddress);
            m_PLCPort = int.Parse(Application.Application.GetCommInfo("PLC Port", m_PLCPort.ToString()));

            m_modbusClient = new ModbusClient(m_strCommAddress, 502);
            //m_modbusServer.LocalIPAddress = ;
            //m_modbusServer.Port = 502;
            m_modbusClient.ConnectedChanged += M_modbusClient_ConnectedChanged;
            m_modbusClient.Connect();
            combo_PLC_Comm_Function.Items.Add("Write Value");
            combo_PLC_Comm_Function.Items.Add("Read Value");
            m_modbusClient.ReceiveDataChanged += M_modbusClient_ReceiveDataChanged;
            label_PLC_IPAddress.Content = $"IP: {m_strCommAddress} Port: {m_PLCPort}";

        }

        private void M_modbusClient_ConnectedChanged(object sender)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (m_modbusClient.Connected)
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

                    int[] ival = new int[1];
                    if (text_MemoryAdress_Status.Text.Length > 0)
                    {
                        bool success = Int32.TryParse(text_MemoryAdress_Status.Text, out ival[0]);
                        if (success == false)
                            MessageBox.Show("Wrong format input");
                    }
                    else
                        MessageBox.Show("Wrong format input");
                    m_modbusClient.WriteMultipleRegisters(iadr, ival);
                }
                else
                {
                    text_MemoryAdress_Status.Text = m_modbusClient.ReadHoldingRegisters(iadr, 1)[0].ToString();

                }
            }

            //ModbusServer.HoldingRegisters regs = m_modbusServer.holdingRegisters;
            //modbusClient.WriteSingleCoil(4, !modbusClient.ReadCoils(4, 1)[0]);
            //bool[] status = modbusClient.ReadCoils(4, 1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_PLC_Connect_Click(object sender, RoutedEventArgs e)
        {
            if(!m_modbusClient.Connected)
                 m_modbusClient.Connect();
            else
                m_modbusClient.Disconnect();

        }
    }
}
