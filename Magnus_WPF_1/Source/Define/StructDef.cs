//using System.Drawing;
using Emgu.CV;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Magnus_WPF_1.Source.Define
{

    public enum TEACHSTEP : int
    {
        TEACH_LOADIMAGE = -1,
        TEACH_DEVICELOCATION,
        TEACH_DEVICELOCATION_TEACHED,
        TEACH_TEMPLATEROI,
        TEACH_TEMPLATEROI_TEACHED,
        TEACH_TOTALSTEP
    }

    public enum IMAGETYPE : int
    {
        GRAY_TYPE,
        BGR_TYPE
    }
    public enum OPERATOR : int
    {
        AND,
        OR
    };
    public enum SIZE : int
    {
        WIDTH,
        HEIGHT
    };
    public enum POSITION : int
    {
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_RIGHT,
        BOTTOM_LEFT,
        _TOP,
        _LEFT,
        _BOTTOM,
        _RIGHT

    };
    public enum OBJECT_COLOR : int
    {
        BLACK = 0,
        WHITE = 1,
        ANY_COLOR
    };

    public enum AREA_INDEX: int
    {
        A1 = 0,
        A2,
        A3,
        A4,
        A5,
    }
    public enum WARNINGMESSAGE: int
    {
        MESSAGE_EMERGENCY,
        MESSAGE_IMIDIATESTOP,
        MESSAGE_STEPDEBUG,
        MESSAGE_INFORMATION
    }

    public enum DIRECTION : int
    {
        X,
        Y,
        ANY_DIRECTION
    };
    public enum EDGE_POSITION : int
    {
        NEGATIVE,
        POSITIVE
    };

    public enum THRESHOLD_TYPE :int
    {
        BINARY_THRESHOLD = 0,
        VAR_THRESHOLD = 1   
    };


    public enum AccessLevel
    {
        Engineer = 3,
        Operator = 2,
        User = 1,
        None = 0
    }
    public enum UISTate
    {
        IDLE_STATE,
        IDLE_NOCAM_STATE,
        LOGOUT,
        LOGOUT_STATE,
        STREAM_STATE,
        CAMERASETTING_STATE,
        LIGHTSETTING_STATE,
        IO_STATE,
        STOREIMAGE_STATE,
        OFFLINEINSPECT_STATE,
        ONLINEINSPECT_STATE,
        INSPECT_STATE,
        TRAIN_STATE,
        ENGINEER,
        ENGINEER_NO_CAM,
        OPERATOR,
        OPERATOR_NO_CAM,
        USER,
        USER_NO_CAM
    }

    public enum SignalFromRobot : int
    {
        Robot_Start_Sequence = 0,
        Robot_Stop_Sequence = 1,
        Robot_Trigger_Camera = 2,
        Robot_Swap_Recipe = 3,
        Robot_Move_Done = 4,
        Robot_Error = 5,
        Robot_Reset_Software = 6
    }
    public enum SignalFromVision : int
    {
        Vision_Ready = 0,
        Vision_Go_Home = 1,
        Vision_Absolute_Move = 2,
        Vision_Relative_Move = 3,
        Vision_Go_Pick = 4,
        Vision_Reset_Software_Done = 5
    }


    public enum ERROR_CODE : int
    {
        PASS,
        NO_PATTERN_FOUND,
        OPPOSITE_CHIP,
        LABEL_FAIL,
        CAPTURE_FAIL,
        PROCESS_ERROR,
        NOT_INSPECTED,
        NUM_DEFECTS
    };

    public struct LocationReference
    {
        public int deltaX { get; set; }
        public int deltaY { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public struct CameraSettingParam
    {
        public bool softwareTrigger;
        public float exposureTime;
        public float frameRate;
        public float gain;
    }

    public static class DefautTeachingSequence
    {
        public static System.Windows.Media.Brush ColorContentTeahing = System.Windows.Media.Brushes.Cyan;
        public static System.Windows.Media.Brush ColorExplaintionTeahing = System.Windows.Media.Brushes.Crimson;
        public static System.Windows.Media.Brush ColorContentTeached = System.Windows.Media.Brushes.Lime;
        public static System.Windows.Media.Brush ColorRectangleTeached = System.Windows.Media.Brushes.DarkGreen;

        public static int StrokeThickness = 2;

    }


    public class VisionResultData
    {
        public int m_nDeviceIndexOnReel = 0;
        public string m_strDeviceID = "";
        public int m_nResult = -(int)ERROR_CODE.NOT_INSPECTED;
        public string m_strFullImagePath = "";
        public VisionResultData(int nDeviceIndexOnReel = 0, string strDeviceID = "", int nResult = -(int)ERROR_CODE.NOT_INSPECTED, string strPath = "")
        {
            m_nDeviceIndexOnReel = nDeviceIndexOnReel;
            m_strDeviceID = strDeviceID;
            m_nResult = nResult;
            m_strFullImagePath = strPath;
        }

        public static void SaveSequenceResultToExcel(string strLotID,int nTrack, VisionResultData data)
        {

            try
            {
                string[] strTrackName = { "Camera", "Barcode" };
                string strRecipePath = Path.Combine(
                    Application.Application.pathStatistics,  
                    Application.Application.currentRecipe,
                    Application.Application.m_strStartLotDay,                  
                    strTrackName[nTrack]);

                if (!Directory.Exists(strRecipePath))
                    Directory.CreateDirectory(strRecipePath);

                string fullpath = Path.Combine(strRecipePath, $"{strLotID}.xlsx");


                FileInfo file = new FileInfo(fullpath);

                if (!file.Exists)
                {
                    file.Create();
                    Thread.Sleep(200);
                }

                //file.CopyTo(backup_fullpath);

                using (ExcelPackage package = new ExcelPackage(file))
                {

                    bool bCreated = false;
                    for (int n = 0; n < package.Workbook.Worksheets.Count; n++)
                        if (package.Workbook.Worksheets[n].Name == "Lot Result")
                        {
                            bCreated = true;
                            break;
                        }

                    if (!bCreated)
                        package.Workbook.Worksheets.Add("Lot Result");

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    worksheet.DefaultColWidth = 35;
                    worksheet.DefaultRowHeight = 35;
                    // Header
                    int ncol = 1;
                    worksheet.Cells[1, ncol++].Value = "Device Index";
                    worksheet.Cells[1, ncol++].Value = "Device ID";
                    worksheet.Cells[1, ncol++].Value = "Result";
                    worksheet.Cells[1, ncol++].Value = "Image Path";

                    // Data
                    int row = data.m_nDeviceIndexOnReel + 2;
                    ncol = 1;
                    worksheet.Cells[row, ncol++].Value = data.m_nDeviceIndexOnReel;
                    worksheet.Cells[row, ncol++].Value = data.m_strDeviceID;
                    worksheet.Cells[row, ncol++].Value = data.m_nResult;
                    worksheet.Cells[row, ncol++].Value = data.m_strFullImagePath;

                    //foreach (var item in data)
                    //{
                    //    ncol = 1;
                    //    worksheet.Cells[row, ncol++].Value = item.m_PointIndex;
                    //    worksheet.Cells[row, ncol++].Value = item.m_PointComment;
                    //    row++;
                    //}
                    LogMessage.LogMessage.WriteToDebugViewer(7 + nTrack, "Save 8!");
                    package.Save();
                    LogMessage.LogMessage.WriteToDebugViewer(7 + nTrack, "Save 9!");
                    //package.Dispose();

                }
            }
            catch(Exception e)
            {

                LogMessage.LogMessage.WriteToDebugViewer(7 + nTrack, $"Save To Excel Failed! {e}");

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {

                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Track{nTrack} Save To Excel Fail {e}!.", (int)ERROR_CODE.LABEL_FAIL);
                });
            }
        }

        public static void ReadLotResultFromExcel(string strLotID, int nTrack, ref VisionResultData[] result, ref int nCurrentDeviceID)
        {
            nCurrentDeviceID = 0;
            string[] strTrackName = { "Camera", "Barcode" };
            string strRecipePath = Path.Combine(
                Application.Application.pathStatistics,
                Application.Application.currentRecipe,
                Application.Application.m_strStartLotDay,
                strTrackName[nTrack]);

            if (!Directory.Exists(strRecipePath))
                Directory.CreateDirectory(strRecipePath);

            string fullpath = Path.Combine(strRecipePath, $"{strLotID}.xlsx");

            FileInfo file = new FileInfo(fullpath);
            if (!file.Exists)
            {
                file.Create();
            }


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

                int nSequenceDeviceTemp = 0;
                for (int row = 2; row <= rowCount; row++)
                {

                    result[row - 2] = new VisionResultData();

                    if (row - 2 >= result.Length)
                        return;

                    int ncol = 1;
                    object valueTemp = worksheet.Cells[row, ncol++].Value;
                    if (valueTemp != null)
                    {
                        result[row - 2].m_nDeviceIndexOnReel = Convert.ToInt32(valueTemp);
                        if (nSequenceDeviceTemp < result[row - 2].m_nDeviceIndexOnReel)
                            nSequenceDeviceTemp = result[row - 2].m_nDeviceIndexOnReel;
                    }
                    valueTemp = worksheet.Cells[row, ncol++].Value;
                    if (valueTemp != null)
                        result[row - 2].m_strDeviceID = valueTemp.ToString();
                    valueTemp = worksheet.Cells[row, ncol++].Value;
                    if (valueTemp != null)
                        result[row - 2].m_nResult = Convert.ToInt32(valueTemp);

                    valueTemp = worksheet.Cells[row, ncol++].Value;
                    if (valueTemp != null)
                        result[row - 2].m_strFullImagePath = valueTemp.ToString();
                }

                nCurrentDeviceID = nSequenceDeviceTemp;

                package.Dispose();
            }
        }
    }



    public struct ArrayOverLay
    {
        public Mat mat_Region { get; set; }
        public System.Windows.Media.Color _color { get; set; }
    };



    public struct Rectangles
    {
        private Point _topLeft;
        private Point _bottomRight;
        private double _width;
        private double _height;
        private double _angle;

        public Point TopLeft
        {
            get
            {
                return _topLeft;
            }

            set
            {
                _topLeft = value;
            }
        }

        public Point BottomRight
        {
            get
            {
                return _bottomRight;
            }

            set
            {
                _bottomRight = value;
            }
        }

        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                _width = value;
            }
        }

        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
            }
        }

        public double Angle
        {
            get
            {
                return _angle;
            }

            set
            {
                _angle = value;
            }
        }

        public Rectangles(Point topLeft, Point bottomRight)
        {
            _topLeft = topLeft;
            _bottomRight = bottomRight;
            _width = bottomRight.X - topLeft.X;
            _height = bottomRight.Y - topLeft.Y;
            _angle = 0;
        }
        public Rectangles(Point topLeft, double width, double height)
        {
            _topLeft = topLeft;
            _bottomRight = new Point(topLeft.X + width, topLeft.Y + height);
            _width = width;
            _height = height;
            _angle = 0;
        }
        public Rectangles(double left, double top, double width, double height)
        {
            _topLeft = new Point(left, top);
            _bottomRight = new Point(_topLeft.X + width, _topLeft.Y + height);
            _width = width;
            _height = height;
            _angle = 0;
        }
        public Rectangles(double left, double top, double width, double height, double angle)
        {
            _topLeft = new Point(left, top);
            _bottomRight = new Point(_topLeft.X + width, _topLeft.Y + height);
            _width = width;
            _height = height;
            _angle = angle;
        }

        public void SetAngle(double angle)
        {
            _angle = angle;
        }
        public struct CCStatsOp
        {
            public System.Drawing.Rectangle Rectangle;
            public int Area;
        }
    }

    public struct ImageSaveData
    {
        public int nDeviceID;
        public string strLotID;
        public int nTrackID;
        public int nResult;
        public string m_strPathImage;
        public Emgu.CV.Mat imageSave;
    }
    public enum TRACK_TYPE :int
    {
        TRACK_CAM1 = 0,
        TRACK_CAM2 = 1,
        TRACK_ALL = 2
    }
}
