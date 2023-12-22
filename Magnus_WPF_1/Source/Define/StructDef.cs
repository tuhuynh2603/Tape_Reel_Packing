//using System.Drawing;
using Emgu.CV;
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
        NO_LABEL,
        PROCESS_ERROR,
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
        public int bFail;
        public Emgu.CV.Mat imageSave;
    }
    public enum TRACK_TYPE :int
    {
        TRACK_CAM1 = 0,
        TRACK_CAM2 = 1,
        TRACK_ALL = 2
    }
}
