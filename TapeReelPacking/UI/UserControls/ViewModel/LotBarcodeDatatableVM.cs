using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using TapeReelPacking.Source.Define;
using System.Collections.ObjectModel;
using System.IO;
using OfficeOpenXml;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class LotBarcodeDatatableVM:BaseVM, ICustomUserControl
    {

        public MainWindowVM _mainWindowVM { get; set; }
        private DragDropUserControlVM _dragDropVM { set; get; }
        public void RegisterUserControl()
        {
            _dragDropVM.RegisterMoveGrid();
            _dragDropVM.RegisterResizeGrid();
        }


        public LotBarcodeDatatableVM(DragDropUserControlVM dragDropVM, MainWindowVM mainVM)
        {
            _mainWindowVM = mainVM;
            _dragDropVM = dragDropVM;
            RegisterUserControl();
        }

        private DateTime selectedDate1 = DateTime.Now;

        public DateTime selectedDate
        {
            get => selectedDate1;
            set
            {
                selectedDate1 = value;
                OnPropertyChanged(nameof(selectedDate));
            }
        }

        private ActionCommand btn_Load_Lot_ClickCmd1;

        public ICommand btn_Load_Lot_ClickCmd
        {
            get
            {
                if (btn_Load_Lot_ClickCmd1 == null)
                {
                    btn_Load_Lot_ClickCmd1 = new ActionCommand(Performbtn_Load_Lot_ClickCmd);
                }

                return btn_Load_Lot_ClickCmd1;
            }
        }

        private void Performbtn_Load_Lot_ClickCmd()
        {
            if (lotSelected == null)
                return;
            m_ListLotBarcodeDataTable = new ObservableCollection<VisionResultDataExcel>();
            
            ReadLotResultFromExcel("Dummy", m_ListLotBarcodeDataTable, lotSelected);
        }

        private ActionCommand btn_Send_To_Server_ClickCmd1;

        public ICommand btn_Send_To_Server_ClickCmd
        {
            get
            {
                if (btn_Send_To_Server_ClickCmd1 == null)
                {
                    btn_Send_To_Server_ClickCmd1 = new ActionCommand(Performbtn_Send_To_Server_ClickCmd);
                }

                return btn_Send_To_Server_ClickCmd1;
            }
        }

        private void Performbtn_Send_To_Server_ClickCmd()
        {
            ObservableCollection<VisionResultDataExcel> list_DeviceID = new ObservableCollection<VisionResultDataExcel>();
            if (m_ListLotBarcodeDataTable.Count() == 0)
                return;

            string strWriteData = CombineReelIDStringSentToClient(m_ListLotBarcodeDataTable, lotSelected);
            ViewModel.SerialCommunicationVM.WriteSerialCom(strWriteData);


        }
        public static string CombineReelIDStringSentToClient(ObservableCollection<VisionResultDataExcel> list_DeviceID, string strLotID)
        {
            string strCombine = "";
            for (int n = 0; n < list_DeviceID.Count(); n++)
            {
                strCombine += list_DeviceID[n].str_BarcodeID + ",";
            }
            strCombine += $"{list_DeviceID.Count()}";
            strLotID = strLotID.Replace("_", "");
            strCombine += $",{strLotID}";
            return strCombine;
        }

        private ObservableCollection<VisionResultDataExcel> m_ListLotBarcodeDataTable1;

        public ObservableCollection<VisionResultDataExcel>  m_ListLotBarcodeDataTable
        {
            get => m_ListLotBarcodeDataTable1;
            set
            {
                m_ListLotBarcodeDataTable1 = value;
                OnPropertyChanged(nameof(m_ListLotBarcodeDataTable));
            }
        }

        private ObservableCollection<string> m_ListStrLotFullPath1;

        public ObservableCollection<string> m_ListStrLotFullPath
        {
            get => m_ListStrLotFullPath1;
            set
            {
                m_ListStrLotFullPath1 = value;
                OnPropertyChanged(nameof(m_ListStrLotFullPath));
            }
        }


        public static void ReadLotResultFromExcel(string strLotID, ObservableCollection<VisionResultDataExcel> result, string strfullpathInput = "")
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

        private ActionCommand dateSelectionChangedCmd;

        public ICommand DateSelectionChangedCmd
        {
            get
            {
                if (dateSelectionChangedCmd == null)
                {
                    dateSelectionChangedCmd = new ActionCommand(PerformDateSelectionChangedCmd);
                }

                return dateSelectionChangedCmd;
            }
        }

        private void PerformDateSelectionChangedCmd()
        {
            string strDayPicked = string.Format("{0}{1}{2}", selectedDate.ToString("yyyy"), selectedDate.ToString("MM"), selectedDate.ToString("dd"));

            string[] strTrackName = { "Camera", "Barcode" };
            string strRecipePath = System.IO.Path.Combine(
                Source.Application.Application.pathStatistics,
                Source.Application.Application.currentRecipe,
                strDayPicked,
                strTrackName[1]);

            if (!Directory.Exists(strRecipePath))
                return;

            //DirectoryInfo folder = new DirectoryInfo(strFolderPath);
            DirectoryInfo folder = new DirectoryInfo(strRecipePath);
            // Get a list of items (files and directories) inside the folder
            FileSystemInfo[] items = folder.GetFileSystemInfos();
            m_ListStrLotFullPath = new ObservableCollection<string>();
            foreach (FileSystemInfo item in items)
            {
                m_ListStrLotFullPath.Add(item.FullName);
            }

        }

        private string lotSelected1;

        public string lotSelected
        {
            get => lotSelected1;
            set
            {
                lotSelected1 = value;
                OnPropertyChanged(nameof(lotSelected));
            }
        }

    }
}
