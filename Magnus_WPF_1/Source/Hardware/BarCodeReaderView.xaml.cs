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
            InitializeComponent();
            combo_commandSendToBarCode.Items.Add("LON");
            combo_commandSendToBarCode.Items.Add("LOFF");

        }

        bool bOnOff = false;
        FileSystemWatcher watcher = null;
        private string m_ftpPath = String.Empty;


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
            folderBrowserDialog1.SelectedPath = @"C:\Users\Public\Documents\KEYENCE\SR-H7W\SRManagementTool\AppSetting\LiveTmp\0001FCDA0F7C\live";
            //System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

            System.Windows.Forms.DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                Label_FTP_Path.Content = folderBrowserDialog1.SelectedPath;
                m_ftpPath = folderBrowserDialog1.SelectedPath;

                watcher = new System.IO.FileSystemWatcher();

                watcher.Path = m_ftpPath;

                watcher.NotifyFilter =
                    (System.IO.NotifyFilters.LastAccess
                    | System.IO.NotifyFilters.LastWrite
                    | System.IO.NotifyFilters.FileName
                    | System.IO.NotifyFilters.DirectoryName);

                watcher.Filter = "*.json";

                //watcher.SynchronizingObject = (System.ComponentModel.ISynchronizeInvoke)this;

                watcher.Created += new FileSystemEventHandler(watcher_Changed);
                watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);

                watcher.EnableRaisingEvents = true;

                // Newest historicalDataFile Search
                LabelContent.Content = getLatestHistoricalFileName(m_ftpPath);

                // Check file
                if (LabelContent.Content == string.Empty)
                {
                    lbNewJpgFileName.Content = MESSAGE_FILE_NO_EXIST;
                }
                else
                {
                    lbNewJpgFileName.Content = MESSAGE_FILE_CORRECT;
                }
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

            {
                //initialization
                string latestFileName = string.Empty;

                // Check FTP path input
                if (m_ftpPath == string.Empty)
                {
                    MessageBox.Show("FTP server path is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // get the latest historical data file
                latestFileName = getLatestHistoricalFileName(m_ftpPath);
                //LabelContent.Content = latestFileName;

                //// check historical data file
                //if (LabelContent.Content == string.Empty)
                //{
                //    lbNewJpgFileName.Content = MESSAGE_FILE_NO_EXIST;
                //    MessageBox.Show("Historical data file doesn't exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                //string historicalDataFile = m_ftpPath + "\\" + latestFileName;
                //// Check file name
                //if (latestFileName.Contains(".json") != true)
                //{
                //    MessageBox.Show("File name error.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                //HistoricalData historical_data = new HistoricalData(historicalDataFile);
                //if (historical_data.isDataOK != true)
                //{
                //    MessageBox.Show("Historical data content is not correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                // Get Jpeg file name from historical data file
                string jpegFileName = "123.JPG";// historical_data.GetJpegFileName();

                //// No Jpeg file
                //if (jpegFileName == string.Empty)
                //{
                //    lbNewJpgFileName.Content = MESSAGE_JPEG_NO_EXIST;
                //    MessageBox.Show("Jpeg file does not exist in the historical data file.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                //// Get corner point from historical data file
                List<System.Drawing.Point> posList = new List<System.Drawing.Point>();

                //// Check list of center point
                //if (posList.Count() == 0)
                //{
                //    MessageBox.Show("\"code corner\" does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                // Draw rectangle 
                JpegOperate jpeg_operate = new JpegOperate();
                jpegFileName = m_ftpPath + "\\" + jpegFileName;
                int ret_mark = jpeg_operate.DrawAndSave(jpegFileName, posList);

                if (ret_mark != 0)
                {
                    switch (ret_mark)
                    {
                        case JpegOperate.NOT_JPG_FILE:
                            MessageBox.Show("Saved file was not a jpeg file.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        case JpegOperate.JPG_NOT_EXIST:
                            MessageBox.Show("Jpeg file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        case JpegOperate.NOT_MAKE_BMP:
                            MessageBox.Show("Failed to read image data.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        case JpegOperate.NOT_SAVE_FILE:
                            MessageBox.Show("CANNOT Save new JPEG file.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;

                        default:
                            break;
                    }
                    return;
                }

                // Show ReadData & DateTime
                //lbReadDate.Text = historical_data._date + historical_data._time;
                //lbReadData.Text = historical_data.GetReaddataString(historical_data._out_data);
                // Show Jpeg view
                //this.picBxJpegImageView.ImageLocation = jpegFileName + JpegOperate.EXTENDED_STRING;
                // Show jpgFileName
                lbNewJpgFileName.Content = jpegFileName + JpegOperate.EXTENDED_STRING; ;// System.IO.Path.GetFileName(jpegFileName + JpegOperate.EXTENDED_STRING);
            }
        }

        private void combo_commandSendToBarCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                MainWindow.mainWindow.master.m_BarcodeReader.sendCommandToAllReaders(combo_commandSendToBarCode.SelectedItem.ToString());
        }
    }
}
