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
using Sres.Net.EEIP;

namespace TapeReelPacking.Source.Hardware
{
    /// <summary>
    /// Interaction logic for ETHERNETIP_View.xaml
    /// </summary>
    public partial class ETHERNETIP_View : UserControl
    {

        private EEIPClient eeipClient;
        public ETHERNETIP_View()
        {
            InitializeComponent();
            eeipClient = new EEIPClient();

            // Set PLC IP address and other parameters
            eeipClient.IPAddress = "192.168.40.10"; // Replace with your PLC's IP address
            eeipClient.TCPPort = 502; // Default Ethernet/IP port

            // Establish a connection to the PLC
            eeipClient.RegisterSession();
            eeipClient.ForwardOpen();
        }

        private void btn_Read(object sender, RoutedEventArgs e)
        {
            // Read a tag value from the PLC
            //string tagName = "TagName"; // Replace with your PLC tag name
            object value = eeipClient.GetAttributeSingle(0x01, 1, 1);
            LogMessage.LogMessage.WriteToDebugViewer(1, $"{value}");
            value = eeipClient.GetAttributeSingle(0x02, 1, 1);
            LogMessage.LogMessage.WriteToDebugViewer(1, $"{value}");
            value = eeipClient.GetAttributeSingle(0x01, 2, 1);
            LogMessage.LogMessage.WriteToDebugViewer(1, $"{value}");
            value = eeipClient.GetAttributeSingle(0x01, 1, 2);
            LogMessage.LogMessage.WriteToDebugViewer(1, $"{value}");

            // Display the value in the TextBox
            txtReadValue.Text = value.ToString();
        }

        private void btn_Write(object sender, RoutedEventArgs e)
        {
            string tagName = "WriteTag"; // Replace with your PLC tag name
            int writeValue;

            // Parse the value from the TextBox
            if (int.TryParse(txtWriteValue.Text, out writeValue))
            {
                // Use SetAttribute for writing values
                byte[] value = { 1, 2 };
                eeipClient.SetAttributeSingle(0x01, 1, 1, value);
                MessageBox.Show($"Wrote value {value} to {tagName}");
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a valid integer.");
            }
        }
    }
}
