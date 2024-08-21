using ReadResultAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;
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
using TapeReelPacking.Source.Application;
using Path = System.IO.Path;

namespace TapeReelPacking.Source.Hardware
{
    /// <summary>
    /// Interaction logic for BarCodeReaderView.xaml
    /// </summary>
    public partial class BarCodeReaderView : UserControl
    {

        private const string MESSAGE_FILE_NO_EXIST = "< Historical data file doesn't exist. >";
        private const string MESSAGE_FILE_CORRECT = "< Find historical data file(s). >";
        private const string MESSAGE_FIND_NEW_FILE = "< Find new historical data file(s). >";
        private const string MESSAGE_JPEG_NO_EXIST = "< CANNOT find any image file in the historical data. >";

        public BarCodeReaderView()
        {
            LogMessage.LogMessage.WriteToDebugViewer(2, "Init Barcode UI");

            InitializeComponent();
            combo_BarcodeBrank.Items.Add("1");
            combo_BarcodeBrank.Items.Add("2");
            //combo_BarcodeBrank.SelectedItem = MainWindow.mainWindow.master.m_BarcodeReader.barcodeSetting.brankID;
            //combo_commandSendToBarCode.Items.Add("LOFF");

        }
        private void combo_commandSendToBarCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            
           //string strFullPathImageOut = "";
           //MainWindow.mainWindow.master.m_BarcodeReader.GetBarCodeStringAndImage(out strFullPathImageOut);
        }

        private void button_Clear_Click(object sender, RoutedEventArgs e)
        {
            label_DataReceived.Content = "";
        }

        int nDeviceIDTemp = 0;
        string strLot = string.Format("{0}{1}{2}+{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
        private void button_Trigger_Click(object sender, RoutedEventArgs e)
        {
            string strFullPathImageOut;
            nDeviceIDTemp++;
            if (nDeviceIDTemp > Application.Application.categoriesMappingParam.M_NumberDevicePerLot)
            {
                nDeviceIDTemp = 0;
                strLot = string.Format("{0}{1}{2}+{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
            }

            label_DataReceived.Content =  MainWindow.mainWindow.master.m_BarcodeReader.GetBarCodeStringAndImage(out strFullPathImageOut, nDeviceIDTemp, strLot);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.master.m_BarcodeReader.barcodeSetting.brankID = combo_BarcodeBrank.SelectedItem.ToString();
            MainWindow.mainWindow.master.m_BarcodeReader.WriteBarcodeSetting();
        }

    }
}
