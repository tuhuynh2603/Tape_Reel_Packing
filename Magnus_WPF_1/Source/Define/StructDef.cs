using Emgu.CV;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Magnus_WPF_1.Source.Define
{
    public enum TEACHSTEP :int
    {
        TEACH_LOADIMAGE = -1,
        TEACH_DEVICELOCATION,
        TEACH_DEVICELOCATION_TEACHED,
        TEACH_TEMPLATEROI,
        TEACH_TEMPLATEROI_TEACHED,
        TEACH_TOTALSTEP
    }

    public enum IMAGETYPE :int
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
        BOTTOM_LEFT
    };
    public enum COLOR : int
    {
        BLACK,
        WHITE,
        ANY_COLOR
    };
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
    public struct LocationReference
    {
        public int deltaX { get; set; }
        public int deltaY { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public static class DefautTeachingSequence
    {
        public static System.Windows.Media.Brush ColorContentTeahing = System.Windows.Media.Brushes.Cyan;
        public static System.Windows.Media.Brush ColorExplaintionTeahing = System.Windows.Media.Brushes.Crimson;
        public static System.Windows.Media.Brush ColorContentTeached = System.Windows.Media.Brushes.Lime;
        public static System.Windows.Media.Brush ColorRectangleTeached = System.Windows.Media.Brushes.DarkGreen;

        public static int StrokeThickness = 2;

    }
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
        public Emgu.CV.Mat imageSave;
    }
}
