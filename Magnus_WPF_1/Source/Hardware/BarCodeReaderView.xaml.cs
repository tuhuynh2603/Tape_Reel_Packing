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

namespace Magnus_WPF_1.Source.Hardware
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
            LogMessage.LogMessage.WriteToDebugViewer(2, "4");

            InitializeComponent();
            LogMessage.LogMessage.WriteToDebugViewer(2, "5");

            combo_commandSendToBarCode.Items.Add("LON");
            combo_commandSendToBarCode.Items.Add("LOFF");

        }

        //bool bOnOff = false;
        //FileSystemWatcher watcher = null;
        public string m_ftpPath = @"C:\Wisely\Barcode Reader\FTP";


        private void Connect_Click(object sender, RoutedEventArgs e)
        {

            if (MainWindow.mainWindow.master == null)
                return;

            //bOnOff = !bOnOff;
            //if (bOnOff)
            //    MainWindow.mainWindow.master.m_BarcodeReader.sendCommandToAllReaders("LON");
            //else
            //    MainWindow.mainWindow.master.m_BarcodeReader.sendCommandToAllReaders("LOFF");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ReceivedBarCodeFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select FTP path";
            folderBrowserDialog1.SelectedPath = @"C:\Wisely\Barcode Reader\FTP";
            //System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

            System.Windows.Forms.DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                Label_FTP_Path.Content = folderBrowserDialog1.SelectedPath;
                m_ftpPath = folderBrowserDialog1.SelectedPath;
            }
        }

        private void SendBarCodeFile_Click(object sender, RoutedEventArgs e)
        {

        }


        private void btnFolderSelect_Click_1(object sender, EventArgs e)
        {

        }

        private void watcher_Changed(System.Object source, System.IO.FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    string fileName = System.IO.Path.GetFileName(e.FullPath);
                    LabelContent.Content = fileName;
                    lbNewJpgFileName.Content = MESSAGE_FIND_NEW_FILE;
                    break;
                default:
                    break;
            }
        }

        private void watcher_Deleted(System.Object source, System.IO.FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Deleted:
                    LabelContent.Content = string.Empty;
                    lbNewJpgFileName.Content = MESSAGE_FILE_NO_EXIST;
                    break;

                default:
                    break;
            }
        }

        private string getLatestHistoricalFileName(string folderName)
        {
            string[] historicalDataFiles;

            try
            {
                historicalDataFiles = System.IO.Directory.GetFiles(folderName, "*.json", System.IO.SearchOption.TopDirectoryOnly);
            }
            catch (Exception)
            {
                return string.Empty;
            }

            string newesthistoricalDataFileName = string.Empty;
            System.DateTime updateTime = System.DateTime.MinValue;

            // Newest historicalDataFile Searching.
            foreach (string file in historicalDataFiles)
            {
                // Get historicalDataFile Infomation
                System.IO.FileInfo fi = new System.IO.FileInfo(file);

                // Judge DateTime
                if (fi.LastWriteTime > updateTime)
                {
                    updateTime = fi.LastWriteTime;
                    newesthistoricalDataFileName = file;
                }
            }

            return System.IO.Path.GetFileName(newesthistoricalDataFileName);
        }

        private void ReceivedFTPLastImage_Click(object sender, RoutedEventArgs e)
        {
                MainWindow.mainWindow.master.m_BarcodeReader.GetBarCodeStringAndImage("LON");
        }

        private void combo_commandSendToBarCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

           MainWindow.mainWindow.master.m_BarcodeReader.GetBarCodeStringAndImage(combo_commandSendToBarCode.SelectedItem.ToString());
        }
    }
}
