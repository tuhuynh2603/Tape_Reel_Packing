using OfficeOpenXml;
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
using TapeReelPacking.Source.Define;

namespace TapeReelPacking.UI.UserControls
{
    /// <summary>
    /// Interaction logic for LotBarcodeDataTable.xaml
    /// </summary>
    public partial class LotBarcodeDataTable : UserControl
    {
        string m_strDayPicked = "None";
        public List<string> m_ListStrLotFullPath = new List<string>();
        public List<VisionResultDataExcel> m_ListLotBarcodeDataTable = new List<VisionResultDataExcel>();
        public LotBarcodeDataTable()
        {
            InitializeComponent();
        }

        private void DatePicker_Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime datetimePicked = (DateTime)DatePicker_Date.SelectedDate;
            m_strDayPicked = string.Format("{0}{1}{2}", datetimePicked.ToString("yyyy"), datetimePicked.ToString("MM"), datetimePicked.ToString("dd"));

            string[] strTrackName = { "Camera", "Barcode" };
            string strRecipePath = System.IO.Path.Combine(
                Source.Application.Application.pathStatistics,
                Source.Application.Application.currentRecipe,
                m_strDayPicked,
                strTrackName[1]);

            if (!Directory.Exists(strRecipePath))
                return;

            //DirectoryInfo folder = new DirectoryInfo(strFolderPath);
            DirectoryInfo folder = new DirectoryInfo(strRecipePath);
            // Get a list of items (files and directories) inside the folder
            FileSystemInfo[] items = folder.GetFileSystemInfos();
            m_ListStrLotFullPath.Clear();
            ccb_LotSelected_ComboBox.Items.Clear();
            foreach (FileSystemInfo item in items)
            {
                ccb_LotSelected_ComboBox.Items.Add(item.Name);
                m_ListStrLotFullPath.Add(item.FullName);
            }
        }


        public static void ReadLotResultFromExcel(string strLotID, ref List<VisionResultDataExcel> result, string strfullpathInput = "")
        {

            string strFileName = strLotID;
            string strStartLotDay = strLotID.Split('_')[0];

            string[] strTrackName = { "Camera", "Barcode" };
            string strRecipePath = System.IO.Path.Combine(
               Source.Application.Application.pathStatistics,
                 Source.Application.Application.currentRecipe,
                strStartLotDay,
                strTrackName[1]);

            //if (!Directory.Exists(strRecipePath))
            //    return;

            string strFullPath = System.IO.Path.Combine(strRecipePath, $"{strFileName}.xlsx");
            if (strfullpathInput != "")
                strFullPath = strfullpathInput;

            FileInfo file = new FileInfo(strFullPath);
            if (!file.Exists)
            {
                return;
            }

            result.Clear();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Use NonCommercial license if applicable
            using (ExcelPackage package = new ExcelPackage(file))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    package.Dispose();
                    return;
                }

                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                worksheet.DefaultColWidth = 35;
                worksheet.DefaultRowHeight = 35;

                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    VisionResultDataExcel dataTemp = new VisionResultDataExcel();

                    int ncol = 1;
                    object valueTemp = worksheet.Cells[row, ncol++].Value;
                    if (valueTemp != null)
                    {
                        dataTemp.str_NO = valueTemp.ToString();
                    }

                    valueTemp = worksheet.Cells[row, ncol++].Value;
                    if (valueTemp != null)
                    {
                        dataTemp.str_DateScan = valueTemp.ToString();
                    }

                    valueTemp = worksheet.Cells[row, ncol++].Value;
                    if (valueTemp != null)
                    {
                        dataTemp.str_BarcodeID = valueTemp.ToString();
                    }

                    valueTemp = worksheet.Cells[row, ncol++].Value;
                    if (valueTemp != null)
                    {
                        dataTemp.str_Result = valueTemp.ToString();
                    }

                    result.Add(dataTemp);
                }
                package.Dispose();
            }
        }


        private void ccb_LotSelected_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //VisionResultData[] visionTemp = new VisionResultData[10000];
            //ReadLotResultFromExcel(m_ListStrLotFullPath[ccb_LotSelected_ComboBox.SelectedIndex], ref m_ListLotBarcodeDataTable);
            //lvLotBarCodeData.ItemsSource = null;
            //lvLotBarCodeData.ItemsSource = m_ListLotBarcodeDataTable;
        }

        private void btn_Load_Lot_Click(object sender, RoutedEventArgs e)
        {
            if (ccb_LotSelected_ComboBox.SelectedIndex < 0 || ccb_LotSelected_ComboBox.Items.Count < 1)
                return;
            ReadLotResultFromExcel("Dummy", ref m_ListLotBarcodeDataTable, m_ListStrLotFullPath[ccb_LotSelected_ComboBox.SelectedIndex]);
            lvLotBarCodeData.ItemsSource = null;
            lvLotBarCodeData.ItemsSource = m_ListLotBarcodeDataTable;
        }

        private void btn_Send_To_Server_Click(object sender, RoutedEventArgs e)
        {
            List<VisionResultDataExcel> list_DeviceID = new List<VisionResultDataExcel>();
            if (m_ListLotBarcodeDataTable.Count() == 0)
                return;

            object selectLot = ccb_LotSelected_ComboBox.SelectedItem;
            string strWriteData = CombineReelIDStringSentToClient(ref m_ListLotBarcodeDataTable, selectLot.ToString());
            ViewModel.SerialCommunicationVM.WriteSerialCom(strWriteData);
            
        }

        public static string CombineReelIDStringSentToClient(ref List<VisionResultDataExcel> list_DeviceID, string strLotID)
        {
            string strCombine = "";
            for(int n = 0; n < list_DeviceID.Count(); n++ )
            {
                strCombine += list_DeviceID[n].str_BarcodeID + ",";
            }
            strCombine += $"{ list_DeviceID.Count()}";
            strLotID = strLotID.Replace("_", "");
            strCombine += $",{strLotID}";
            return strCombine;
        }

        public void UpdateLotDataTable()
        {
            //defectInfor.lvDefect.View = gridView;
            this.lvLotBarCodeData.ItemsSource = null;
            this.lvLotBarCodeData.ItemsSource = m_ListLotBarcodeDataTable;
        }

        private void lvLotBarCodeData_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            scv_LotBarcodeDataTableScrollView.ScrollToVerticalOffset(scv_LotBarcodeDataTableScrollView.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void lvLotBarCodeData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void lvLotBarCodeData_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void scv_LotBarcodeDataTableScrollView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            scv_LotBarcodeDataTableScrollView.ScrollToVerticalOffset(scv_LotBarcodeDataTableScrollView.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
