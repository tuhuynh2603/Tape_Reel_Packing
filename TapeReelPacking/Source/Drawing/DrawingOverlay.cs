using TapeReelPacking.Source.Algorithm;
using TapeReelPacking.Source.Application;
using TapeReelPacking.UI.UserControls.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CvContourArray = Emgu.CV.Util.VectorOfVectorOfPoint;
using CvPointArray = Emgu.CV.Util.VectorOfPoint;
using Point = System.Drawing.Point;
using TapeReelPacking.UI.UserControls.ViewModel;

namespace TapeReelPacking.Source.DrawingOverlay
{
    class EDDrawingOverlay
    {
        //Main Drawing Functions
        #region Main Drawing Functions
        //Draw Line
        public static bool DrawLine(int nTrack, Canvas GridOverlay, System.Drawing.Point p1, System.Drawing.Point p2, System.Windows.Media.Brush color, double thickness = 2)
        {
            Line line = new Line();
            line.Stroke = color;
            line.StrokeThickness = thickness;
            double scaleWidth = 0;
            double scaleHeight = 0;

            GridOverlay.Dispatcher.Invoke(delegate
            {
                scaleWidth = GridOverlay.Width / Application.Application.m_Width[nTrack];
                scaleHeight = GridOverlay.Height / Application.Application.m_Height[nTrack];
            });

            line.X1 = p1.X * scaleWidth;
            line.Y1 = p1.Y * scaleHeight;

            line.X2 = p2.X * scaleWidth;
            line.Y2 = p2.Y * scaleHeight;

            GridOverlay.Dispatcher.Invoke(delegate
            {
                GridOverlay.Children.Add(line);
            });
            return true;
        }
        public static bool DrawLineFlap(int nTrack, Canvas GridOverlay, System.Drawing.Point p1, System.Drawing.Point p2, System.Windows.Media.Brush color, double thickness = 2)
        {
            Line line = new Line();
            line.Stroke = color;
            line.StrokeThickness = thickness;
            double scaleWidth = 0;
            double scaleHeight = 0;

            GridOverlay.Dispatcher.Invoke(delegate
            {
                scaleWidth = GridOverlay.Width / Application.Application.m_Width[nTrack];
                scaleHeight = GridOverlay.Height / Application.Application.m_Height[nTrack];
            });

            line.X1 = p1.X * scaleWidth;
            line.Y1 = p1.Y * scaleHeight;

            line.X2 = p2.X * scaleWidth;
            line.Y2 = p2.Y * scaleHeight;

            GridOverlay.Dispatcher.Invoke(delegate
            {
                GridOverlay.Children.Add(line);
            });
            return true;
        }
        //Draw Region
        public static bool DrawRegion(Canvas GridOverlay, int ntrackID, Emgu.CV.Mat Region, System.Windows.Media.Brush color, double thickness)
        {
            if (Region == null)
                return false;
            CvContourArray contourRegion = new CvContourArray();
            if (Region.Width > 2000)
            {
                MagnusOpenCVLib.OpeningCircle(ref Region, ref Region, 2);
                MagnusOpenCVLib.ClosingCircle(ref Region, ref Region, 2);
            }

            else
            {
                MagnusOpenCVLib.OpeningCircle(ref Region, ref Region, 1);
                MagnusOpenCVLib.ClosingCircle(ref Region, ref Region, 1);
            }

            MagnusOpenCVLib.GenContourRegion(ref Region, ref contourRegion, Emgu.CV.CvEnum.RetrType.Tree);
            for (int i = 0; i < contourRegion.Size; i++)
            {
                Polygon polygon = new Polygon();
                polygon.Stroke = color;
                polygon.StrokeThickness = thickness;

                double scaleWidth = 0;
                double scaleHeight = 0;
                GridOverlay.Dispatcher.Invoke(delegate
                {
                    int width_track = MainWindowVM.master.m_Tracks[ntrackID].m_imageViews[0]._imageWidth;
                    int height_track = MainWindowVM.master.m_Tracks[ntrackID].m_imageViews[0]._imageHeight;
                    scaleWidth = GridOverlay.Width / width_track;
                    scaleHeight = GridOverlay.Height / height_track;
                });

                CvPointArray contour = new CvPointArray();
                contour = contourRegion[i];
                foreach (System.Drawing.Point point in contour.ToArray().ToList())
                {
                    System.Windows.Point scaledPoint = new System.Windows.Point(point.X * scaleWidth, point.Y * scaleHeight);
                    polygon.Points.Add(scaledPoint);
                }

                GridOverlay.Dispatcher.Invoke(delegate
                {
                    GridOverlay.Children.Add(polygon);
                });
            }
            return true;
        }

        // Draw Polygon
        public static bool DrawPolygon(int nTrack, Canvas GridOverlay, List<System.Drawing.Point> pointList, System.Windows.Media.Brush color, double thickness)
        {
            Polygon polygon = new Polygon();
            polygon.Stroke = color;
            polygon.StrokeThickness = thickness;
            double scaleWidth = 0;
            double scaleHeight = 0;
            GridOverlay.Dispatcher.Invoke(delegate
            {
                scaleWidth = GridOverlay.Width / Application.Application.m_Width[nTrack];
                scaleHeight = GridOverlay.Height / Application.Application.m_Height[nTrack];
            });

            foreach (System.Drawing.Point point in pointList)
            {
                System.Windows.Point scaledPoint = new System.Windows.Point(point.X * scaleWidth, point.Y * scaleHeight);
                polygon.Points.Add(scaledPoint);
            }

            System.Windows.Application.Current.MainWindow.Dispatcher.Invoke(delegate
            {
                GridOverlay.Children.Add(polygon);
            });
            return true;
        }

        // Draw Rectangle
        public static bool DrawRectangle(int nTrack, Canvas GridOverlay, System.Windows.Point leftTop, System.Windows.Point rightBottom, System.Windows.Media.Brush color, double thickness)
        {
            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = color;
            rect.StrokeThickness = thickness;
            double scaleWidth = 0;
            double scaleHeight = 0;
            GridOverlay.Dispatcher.Invoke(delegate
            {
                scaleWidth = GridOverlay.Width / Application.Application.m_Width[nTrack];
                scaleHeight = GridOverlay.Height / Application.Application.m_Height[nTrack];
            });
            Canvas.SetLeft(rect, leftTop.X * scaleWidth);
            Canvas.SetTop(rect, leftTop.Y * scaleHeight);
            rect.Width = (rightBottom.X - leftTop.X) * scaleWidth;
            rect.Height = (rightBottom.Y - leftTop.Y) * scaleHeight;
            System.Windows.Application.Current.MainWindow.Dispatcher.Invoke(delegate
            {
                GridOverlay.Children.Add(rect);
            });
            return true;
        }

        public static bool DrawRectangle(int nTrack, Canvas GridOverlay, System.Drawing.Rectangle rect, int Method, System.Windows.Media.Brush color, double thickness)
        {
            double scaleWidth = 0;
            double scaleHeight = 0;
            GridOverlay.Dispatcher.Invoke(delegate
            {
                //Method == 0: TopView
                //Method (otherwise): SideFlap
                if (Method == 0)
                {
                    scaleWidth = GridOverlay.Width / Application.Application.m_Width[nTrack];
                    scaleHeight = GridOverlay.Height / Application.Application.m_Height[nTrack];
                }
                else
                {
                    scaleWidth = GridOverlay.Width / Application.Application.m_Width[nTrack];
                    scaleHeight = GridOverlay.Height / Application.Application.m_Height[nTrack];
                }

            });

            Point[] PointRec = new Point[4];
            PointRec[0] = new Point(rect.X, rect.Y);
            PointRec[1] = new Point(rect.X + rect.Width, rect.Y);
            PointRec[2] = new Point(rect.X + rect.Width, rect.Y + rect.Height);
            PointRec[3] = new Point(rect.X, rect.Y + rect.Height);

            for (int i = 0; i < 4; i++)
            {
                Line l1 = new Line();
                l1.Stroke = color;
                l1.StrokeThickness = 2;
                if (i == 3)
                {
                    l1.X1 = PointRec[i].X * scaleWidth;
                    l1.Y1 = PointRec[i].Y * scaleHeight;

                    l1.X2 = PointRec[0].X * scaleWidth;
                    l1.Y2 = PointRec[0].Y * scaleHeight;
                    GridOverlay.Children.Add(l1);
                    break;
                }
                l1.X1 = PointRec[i].X * scaleWidth;
                l1.Y1 = PointRec[i].Y * scaleHeight;

                l1.X2 = PointRec[i + 1].X * scaleWidth;
                l1.Y2 = PointRec[i + 1].Y * scaleHeight;

                GridOverlay.Children.Add(l1);
            }
            return true;
        }


        // Draw Cross
        public static bool DrawCross(int nTrack, Canvas GridOverlay, System.Drawing.PointF pointCross, int Angle, double Length, System.Windows.Media.Brush color, double Thickness)
        {
            double scaleWidth = 0;
            double scaleHeight = 0;
            GridOverlay.Dispatcher.Invoke(delegate
            {
                scaleWidth = GridOverlay.Width / Application.Application.m_Width[nTrack];
                scaleHeight = GridOverlay.Height / Application.Application.m_Height[nTrack];
            });
            Line line1 = new Line();
            line1.Stroke = color;
            line1.StrokeThickness = 2;

            line1.X1 = (pointCross.X - Length * Math.Cos(Math.PI * Angle / 180)) * scaleWidth;
            line1.Y1 = (pointCross.Y - Length * Math.Sin(Math.PI * Angle / 180)) * scaleHeight;
            line1.X2 = (2 * pointCross.X - (pointCross.X - Length * Math.Cos(Math.PI * Angle / 180))) * scaleWidth;
            line1.Y2 = (2 * pointCross.Y - (pointCross.Y - Length * Math.Sin(Math.PI * Angle / 180))) * scaleHeight;

            GridOverlay.Children.Add(line1);

            Line line2 = new Line();
            line2.Stroke = color;
            line2.StrokeThickness = 2;

            line2.X1 = (pointCross.X - Length * Math.Cos(Math.PI * Angle / 180)) * scaleWidth;
            line2.Y1 = (pointCross.Y + Length * Math.Sin(Math.PI * Angle / 180)) * scaleHeight;
            line2.X2 = (2 * pointCross.X - (pointCross.X - Length * Math.Cos(Math.PI * Angle / 180))) * scaleWidth;
            line2.Y2 = (2 * pointCross.Y - (pointCross.Y + Length * Math.Sin(Math.PI * Angle / 180))) * scaleHeight;

            GridOverlay.Children.Add(line2);
            return true;
        }


        public static bool DrawString(int nTrack, Canvas GridOverlay, string text, int X, int Y, SolidColorBrush brushColor, int fontsize = 21)
        {

            double scaleWidth = 0;
            double scaleHeight = 0;
            GridOverlay.Dispatcher.Invoke(delegate
            {
                scaleWidth = GridOverlay.Width / Application.Application.m_Width[nTrack];
                scaleHeight = GridOverlay.Height / Application.Application.m_Height[nTrack];
            });

            Results result = new Results(text, brushColor, fontsize);
            //if (results.Count == 0)
            //    result.y = 45;
            //else
            //    result.y = results.Last().y + 28;
            //results.Add(result);
            TextBlock textBlock = new TextBlock();
            textBlock.Text = result.text;
            textBlock.FontSize = result.fontSize;
            //textBlock.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Bold");
            textBlock.Foreground = result.color;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.FontWeight = FontWeights.ExtraBold;
            Canvas.SetLeft(textBlock, X * scaleWidth);
            Canvas.SetTop(textBlock, Y * scaleHeight);
            GridOverlay.Dispatcher.Invoke(delegate
            {
                GridOverlay.Children.Add(textBlock);
            });
            return true;
        }





        #endregion
    }
}
