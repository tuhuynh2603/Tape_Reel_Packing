using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Define;
using Magnus_WPF_1.UI.UserControls.View;
using static Magnus_WPF_1.Source.Algorithm.InspectionCore;
using CvImage = Emgu.CV.Mat;
using Line = Emgu.CV.Structure.LineSegment2D;
using LineArray = System.Collections.Generic.List<Emgu.CV.Structure.LineSegment2D>;
using CvContourArray = Emgu.CV.Util.VectorOfVectorOfPoint;
using CvPointArray = Emgu.CV.Util.VectorOfPoint;
using Org.BouncyCastle.Tsp;
using iTextSharp.text;
using Emgu.CV.WPF;
using System.Windows.Media.Imaging;
using Magnus_WPF_1.Source.Application;
using System.Windows.Controls;
using iTextSharp.text.pdf.fonts.cmaps;
using System.IO;
using System.Reflection;

namespace Magnus_WPF_1.Source.Algorithm
{
    public class InspectionCore
    {
        public static Size globalImageSize;
        static TemplateMatchingModel m_TemplateMatchingModel = new TemplateMatchingModel();

        public struct ImageTarget
        {
            public CvImage Gray;
            public CvImage Bgr;
        }

        public struct DeviceLocationParameter
        {
            public static Rectangles m_L_DeviceLocationRoi = new Rectangles();
            public static int m_L_lowerThreshold = 0;
            public static int m_L_upperThreshold = 255;
            public static int m_nOpeningMask = 11;
            public static int m_nMinWidthDevice = 50;
            public static int m_nMinHeightDevice = 50;
            public static int m_nDilationMask = 30;

            public static Rectangles m_L_TemplateRoi = new Rectangles();
            public static int m_nStepTemplate = 4;
            public static double m_dAngleResolutionTemplate = 90.0;
            public static double m_dMinScoreTemplate = 50.0;
            public static int m_nBlackCornerIndexTemplateImage = 0;

        }

        public static ImageTarget m_SourceImage;
        public static ImageTarget m_TeachImage;
        public static ImageTarget m_TemplateImage;

        public static DeviceLocationParameter m_DeviceLocationParameter;


        public static bool Initialize()
        {
            m_SourceImage = new ImageTarget();
            m_TeachImage = new ImageTarget();
            m_TemplateImage = new ImageTarget();
            m_DeviceLocationParameter = new DeviceLocationParameter();
            globalImageSize = new Size(Application.Track.m_Width, Application.Track.m_Height);

            return true;
        }

        public static bool LoadImage(BitmapSource btmSource)
        {
            try
            {
                CvImage imgBgr = BitmapSourceConvert.ToMat(btmSource);
                BitmapSource btmSourceGray = new FormatConvertedBitmap(btmSource, PixelFormats.Gray8, null, 0);
                CvImage imgGray = BitmapSourceConvert.ToMat(btmSourceGray);
                m_SourceImage.Gray = imgGray.Clone();
                m_SourceImage.Bgr = imgBgr.Clone();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool LoadOfflineImage(string strPath)
        {
            try {
                m_SourceImage.Gray = CvInvoke.Imread(strPath, Emgu.CV.CvEnum.ImreadModes.Grayscale);
                m_SourceImage.Bgr = CvInvoke.Imread(strPath, Emgu.CV.CvEnum.ImreadModes.Color);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SaveTemplateImage(string strTemplateImageFile)
        {
            try
            {
                CvInvoke.Imwrite(strTemplateImageFile, m_TemplateImage.Gray);
            }
            catch
            {

            }
        }

        public static void SaveInspectImage(string strPath, string strDeviceID)
        {
            try
            {
                if(Directory.Exists(strPath) == false)
                {
                    Directory.CreateDirectory(strPath);
                }

                string strImageName =System.IO.Path.Combine(strPath, "Device_" + strDeviceID + ".bmp");
                if (Directory.Exists(strPath))
                    CvInvoke.Imwrite(strImageName, m_SourceImage.Bgr);

            }
            catch { }
        }
        public static bool SetTeachParameterToInspectionCore()
        {
            DeviceLocationParameter.m_L_lowerThreshold = Source.Application.Application.categoriesTeachParam.L_lowerThreshold;
            DeviceLocationParameter.m_L_upperThreshold = Source.Application.Application.categoriesTeachParam.L_upperThreshold;
            DeviceLocationParameter.m_L_DeviceLocationRoi = Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi;
            DeviceLocationParameter.m_nOpeningMask = Source.Application.Application.categoriesTeachParam.L_OpeningMask;
            DeviceLocationParameter.m_nMinWidthDevice = Source.Application.Application.categoriesTeachParam.L_DilationMask;
            DeviceLocationParameter.m_nMinHeightDevice = Source.Application.Application.categoriesTeachParam.L_MinWidthDevice;
            DeviceLocationParameter.m_nDilationMask = Source.Application.Application.categoriesTeachParam.L_MinHeightDevice;

            DeviceLocationParameter.m_L_TemplateRoi = Source.Application.Application.categoriesTeachParam.L_TemplateRoi;
            DeviceLocationParameter.m_nStepTemplate = Source.Application.Application.categoriesTeachParam.L_NumberSide;
            DeviceLocationParameter.m_dAngleResolutionTemplate = Source.Application.Application.categoriesTeachParam.L_AngleResolution;
            DeviceLocationParameter.m_dMinScoreTemplate = Source.Application.Application.categoriesTeachParam.L_MinScore;
            DeviceLocationParameter.m_nBlackCornerIndexTemplateImage = Source.Application.Application.categoriesTeachParam.L_CornerIndex;
            //master.m_Tracks[0].m_imageViews[0].SaveTeachImage(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_1.bmp"));
            //master.m_Tracks[0].m_imageViews[0].saveTemplateImage(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "templateImage_1.bmp"));
            return true;
        }
        public static bool LoadTeachImageToInspectionCore()
        {
            try
            {
                m_TeachImage.Gray = CvInvoke.Imread(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_1.bmp"), Emgu.CV.CvEnum.ImreadModes.Grayscale);
                m_TemplateImage.Gray = CvInvoke.Imread(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "templateImage_1.bmp"), Emgu.CV.CvEnum.ImreadModes.Grayscale);

            }
            catch { }

            return true;
        }
        public static int SimpleInspection(ref List<Point> p_Regionpolygon, ref Point pCenter, ref Mat mat_DeviceLocationRegion,  ref double nAngleOutput, ref double dScoreOutput)
        {

            return FindDeviceLocation(ref m_SourceImage.Gray,
                                 ref p_Regionpolygon, ref pCenter, ref mat_DeviceLocationRegion,ref nAngleOutput, ref dScoreOutput);
        }

        public static void SetTemplateImage()
        {
            Image<Gray, Byte> imgCropped = new Image<Gray, byte>(m_TeachImage.Gray.Bitmap);
            System.Drawing.Rectangle rec = new System.Drawing.Rectangle();
            rec.X = (int)DeviceLocationParameter.m_L_TemplateRoi.TopLeft.X;
            rec.Y = (int)DeviceLocationParameter.m_L_TemplateRoi.TopLeft.Y;
            rec.Width = (int)DeviceLocationParameter.m_L_TemplateRoi.Width;
            rec.Height = (int)DeviceLocationParameter.m_L_TemplateRoi.Height;
            imgCropped.ROI = rec;
            m_TemplateImage.Gray = imgCropped.Mat.Clone();
            CvInvoke.Imshow("Template Image", m_TemplateImage.Gray);
            CvInvoke.WaitKey();
        }

        public static int FindDeviceLocation(ref CvImage imgSource,
            ref List<Point> p_Regionpolygon, ref Point pCenter, ref Mat mat_DeviceLocationRegion,ref double nAngleOutput, ref double dScoreOutput)
        {
            if (m_TemplateImage.Gray == null)
                return -99;

            CvImage img_thresholdRegion = new CvImage();
            CvImage img_openingRegionRegion = new CvImage();
            CvImage img_BiggestRegion = new CvImage();
            CvImage img_SelectRegion = new CvImage();
            CvImage img_DilationRegion = new CvImage();

            CvImage img_Crop = new CvImage();
            CvImage region_Crop = new CvImage();
            mat_DeviceLocationRegion = new CvImage();

            System.Drawing.Rectangle rectDeviceLocation = new System.Drawing.Rectangle((int)DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.X,
                                                                                        (int)DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.Y, 
                                                                                        (int)DeviceLocationParameter.m_L_DeviceLocationRoi.Width, 
                                                                                       (int)DeviceLocationParameter.m_L_DeviceLocationRoi.Height);
            //Image<Gray, Byte> Image_Source_Crop_Temp = new Image<Gray, Byte>(imgSource.Bitmap);
            CvImage rec_region = new CvImage();
            rec_region = CvImage.Zeros(imgSource.Height, imgSource.Width, DepthType.Cv8U, 1);
            CvInvoke.Rectangle(rec_region, rectDeviceLocation, new MCvScalar(255), -1);           
            MagnusOpenCVLib.Threshold2(ref imgSource, ref img_thresholdRegion, DeviceLocationParameter.m_L_lowerThreshold, DeviceLocationParameter.m_L_upperThreshold);
            CvInvoke.BitwiseAnd(img_thresholdRegion, rec_region, img_thresholdRegion);
            MagnusOpenCVLib.OpeningRectangle(ref img_thresholdRegion, ref img_openingRegionRegion, DeviceLocationParameter.m_nOpeningMask, DeviceLocationParameter.m_nOpeningMask);
            MagnusOpenCVLib.SelectBiggestRegion(ref img_openingRegionRegion, ref mat_DeviceLocationRegion);
            List<System.Drawing.Rectangle> rectLabel = new List<System.Drawing.Rectangle>();

            MagnusOpenCVLib.SelectRegion(ref mat_DeviceLocationRegion, ref img_SelectRegion, ref rectLabel, DeviceLocationParameter.m_nMinWidthDevice, DeviceLocationParameter.m_nMinHeightDevice);
            if (rectLabel == null)
                return -99;

            MagnusOpenCVLib.DilationRectangle(ref img_SelectRegion,ref img_DilationRegion, DeviceLocationParameter.m_nDilationMask, DeviceLocationParameter.m_nDilationMask);
            System.Drawing.Rectangle rectangleRoi = new System.Drawing.Rectangle();
            Image<Gray, Byte> ImageAfterDilationCrop = new Image<Gray, Byte>(imgSource.Bitmap);
            Image<Gray, Byte> Img = new Image<Gray, Byte>(imgSource.Bitmap);
            MagnusOpenCVLib.CropImage(ref ImageAfterDilationCrop, ref Img, img_DilationRegion, ref rectangleRoi);

            System.Drawing.Rectangle rectMatchingPosition = new System.Drawing.Rectangle();
            bool bIsTemplateFounded = false;
            if (ImageAfterDilationCrop.Width <= m_TemplateImage.Gray.Width || ImageAfterDilationCrop.Height <= m_TemplateImage.Gray.Height)
            {
                if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(imgSource, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
                else
                {
                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(imgSource, m_TemplateImage.Gray, 0, 12, 30, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(imgSource, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
               
                }
            }
            else
            {
                if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
                else
                {
                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, m_TemplateImage.Gray, 0, 12, 30, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(ImageAfterDilationCrop.Mat, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
                }
            }

            List<Point> pPolygon = new List<Point>();
            pCenter = new Point((rectMatchingPosition.Left + rectMatchingPosition.Right) / 2 + rectangleRoi.Left,
                          (rectMatchingPosition.Top + rectMatchingPosition.Bottom) / 2 + rectangleRoi.Top);

            //top bottom left right  x1 x2 y1 y2
            Point po1 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
            Point po2 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
            Point po3 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);
            Point po4 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);

            pPolygon.Add(po1);
            pPolygon.Add(po2);
            pPolygon.Add(po3);
            pPolygon.Add(po4);
            pPolygon.Add(pCenter);

            List<Point> p_Regionpolygon_temp = RotatePolygon(pPolygon, -nAngleOutput, pCenter.X, pCenter.Y);

            int nminIndex = FindNearestPoints(imgSource, ref mat_DeviceLocationRegion, rectMatchingPosition, p_Regionpolygon_temp, (float)nAngleOutput);

            nAngleOutput = nAngleOutput + (DeviceLocationParameter.m_nBlackCornerIndexTemplateImage - nminIndex) * 90;
            if (nAngleOutput <= -180)
                nAngleOutput = 360 - nAngleOutput;

            if (nAngleOutput >= 180)
                nAngleOutput = nAngleOutput - 360;
            p_Regionpolygon = RotatePolygon(pPolygon, -nAngleOutput, pCenter.X, pCenter.Y);
            p_Regionpolygon.Remove(pCenter);


            if (bIsTemplateFounded)
                return 0;
            else
                return -1;

        }

        public static int FindNearestPoints(CvImage imgSourceInput, ref CvImage deviceLocationThresholdRegion, System.Drawing.Rectangle rectMatchingPosition,List<Point> polygonInput, float fAngleInput)
        {

            RotatedRect rotateRect = new RotatedRect(polygonInput[polygonInput.Count()-1], new SizeF(rectMatchingPosition.Width - 30, rectMatchingPosition.Height - 30), -fAngleInput);
            CvImage rec_region2 = new CvImage();
            rec_region2 = CvImage.Zeros(imgSourceInput.Height, imgSourceInput.Width, DepthType.Cv8U, 1);
            MagnusOpenCVLib.GenRectangle2(rec_region2, rotateRect, new MCvScalar(255), 1);
            CvImage rec_regionFillup = new CvImage();
            MagnusOpenCVLib.FillUp(ref rec_region2, ref rec_regionFillup);

            CvImage XOR_Region = new CvImage();
            CvImage fillup_Region = new CvImage();
            MagnusOpenCVLib.FillUp(ref deviceLocationThresholdRegion, ref fillup_Region);

            CvInvoke.BitwiseXor(rec_regionFillup, fillup_Region, XOR_Region);
            CvImage And_Region = new CvImage();

            CvInvoke.BitwiseAnd(XOR_Region, rec_regionFillup, And_Region);
            CvImage opening_Region2 = new CvImage();
            MagnusOpenCVLib.OpeningCircle(ref And_Region, ref opening_Region2, 5);
            MagnusOpenCVLib.SelectBiggestRegion(ref opening_Region2, ref deviceLocationThresholdRegion);
            CvPointArray regionPoints = new CvPointArray();
            CvInvoke.FindNonZero(deviceLocationThresholdRegion, regionPoints);
            System.Drawing.Rectangle rect_temp = CvInvoke.BoundingRectangle(regionPoints);
            Point Center_Point = new Point(rect_temp.Left + rect_temp.Width / 2, rect_temp.Top + rect_temp.Height / 2);
            double minDistance = 9999999;
            int nminIndex = -1;
            for (int n = 0; n < polygonInput.Count() - 1; n++)
            {
                double distance_Square = (polygonInput[n].X - Center_Point.X) * (polygonInput[n].X - Center_Point.X) + (polygonInput[n].Y - Center_Point.Y) * (polygonInput[n].Y - Center_Point.Y);
                if (distance_Square < minDistance)
                {
                    nminIndex = n;
                    minDistance = distance_Square;
                }
            }

            return nminIndex;
        }

        public static int AutoTeachDatumLocation(ref List<Point> p_Regionpolygon, Rectangles rectDeviceLocationInput, Rectangles rectTemplateInput, ref Mat mat_DeviceLocationRegion, ref System.Drawing.Rectangle rectMatchingPosition, ref double nAngleOutput, ref double dScoreOutput, ref int nCornerIndex)
        {

            if (m_TeachImage.Gray == null)
                return -99;

            CvImage img_thresholdRegion = new CvImage();
            CvImage img_openingRegionRegion = new CvImage();
            CvImage img_BiggestRegion = new CvImage();
            CvImage img_SelectRegion = new CvImage();
            CvImage img_DilationRegion = new CvImage();

            CvImage img_Crop = new CvImage();
            CvImage region_Crop = new CvImage();
            mat_DeviceLocationRegion = new CvImage();


            Image<Gray, Byte> imgCropped = new Image<Gray, byte>(m_TeachImage.Gray.Bitmap);
            System.Drawing.Rectangle rec = new System.Drawing.Rectangle();
            CvImage templateImageCropped = new CvImage();
            rec.X = (int)rectTemplateInput.TopLeft.X;
            rec.Y = (int)rectTemplateInput.TopLeft.Y;
            rec.Width = (int)rectTemplateInput.Width;
            rec.Height = (int)rectTemplateInput.Height;
            imgCropped.ROI = rec;
            templateImageCropped = imgCropped.Mat.Clone();
            CvInvoke.Imshow("Template Image", templateImageCropped);
            CvInvoke.WaitKey();


            System.Drawing.Rectangle rectDeviceLocation = new System.Drawing.Rectangle((int)rectDeviceLocationInput.TopLeft.X,
                                                                                        (int)rectDeviceLocationInput.TopLeft.Y,
                                                                                        (int)rectDeviceLocationInput.Width,
                                                                                       (int)rectDeviceLocationInput.Height);
            //Image<Gray, Byte> Image_Source_Crop_Temp = new Image<Gray, Byte>(imgSource.Bitmap);
            CvImage rec_region = new CvImage();
            rec_region = CvImage.Zeros(m_TeachImage.Gray.Height, m_TeachImage.Gray.Width, DepthType.Cv8U, 1);
            CvInvoke.Rectangle(rec_region, rectDeviceLocation, new MCvScalar(255), -1);
            MagnusOpenCVLib.Threshold2(ref m_TeachImage.Gray, ref img_thresholdRegion, DeviceLocationParameter.m_L_lowerThreshold, DeviceLocationParameter.m_L_upperThreshold);
            CvInvoke.BitwiseAnd(img_thresholdRegion, rec_region, img_thresholdRegion);
            MagnusOpenCVLib.OpeningRectangle(ref img_thresholdRegion, ref img_openingRegionRegion, DeviceLocationParameter.m_nOpeningMask, DeviceLocationParameter.m_nOpeningMask);
            MagnusOpenCVLib.SelectBiggestRegion(ref img_openingRegionRegion, ref mat_DeviceLocationRegion);
            List<System.Drawing.Rectangle> rectLabel = new List<System.Drawing.Rectangle>();

            MagnusOpenCVLib.SelectRegion(ref mat_DeviceLocationRegion, ref img_SelectRegion, ref rectLabel, DeviceLocationParameter.m_nMinWidthDevice, DeviceLocationParameter.m_nMinHeightDevice);
            if (rectLabel == null)
                return -99;

            MagnusOpenCVLib.DilationRectangle(ref img_SelectRegion, ref img_DilationRegion, DeviceLocationParameter.m_nDilationMask, DeviceLocationParameter.m_nDilationMask);
            System.Drawing.Rectangle rectangleRoi = new System.Drawing.Rectangle();
            Image<Gray, Byte> ImageAfterDilationCrop = new Image<Gray, Byte>(m_TeachImage.Gray.Bitmap);
            Image<Gray, Byte> Img = new Image<Gray, Byte>(m_TeachImage.Gray.Bitmap);
            MagnusOpenCVLib.CropImage(ref ImageAfterDilationCrop, ref Img, img_DilationRegion, ref rectangleRoi);

            bool bIsTemplateFounded = false;
            if (ImageAfterDilationCrop.Width <= templateImageCropped.Width || ImageAfterDilationCrop.Height <= templateImageCropped.Height)
            {
                if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(m_TeachImage.Gray, templateImageCropped, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
                else
                {
                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(m_TeachImage.Gray, templateImageCropped, 0, 12, 30, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(m_TeachImage.Gray, templateImageCropped, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

                }
            }
            else
            {
                if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, templateImageCropped, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
                else
                {
                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, templateImageCropped, 0, 12, 30, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

                    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(ImageAfterDilationCrop.Mat, templateImageCropped, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
                }
            }

            List<Point> pPolygon = new List<Point>();
            Point pCenter = new Point((rectMatchingPosition.Left + rectMatchingPosition.Right) / 2 + rectangleRoi.Left,
                          (rectMatchingPosition.Top + rectMatchingPosition.Bottom) / 2 + rectangleRoi.Top);

            //top bottom left right  x1 x2 y1 y2
            Point po1 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
            Point po2 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
            Point po3 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);
            Point po4 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);

            pPolygon.Add(po1);
            pPolygon.Add(po2);
            pPolygon.Add(po3);
            pPolygon.Add(po4);
            pPolygon.Add(pCenter);
            List<Point> p_Regionpolygon_temp = RotatePolygon(pPolygon, -nAngleOutput, pCenter.X, pCenter.Y);

            nCornerIndex = FindNearestPoints(m_TeachImage.Gray, ref mat_DeviceLocationRegion, rectMatchingPosition, p_Regionpolygon_temp, -(float)nAngleOutput);
            return 0;
        }
        public static List<Point> RotatePolygon(List<Point> polygon, double angle, double midx, double midy)
        {
            List<Point> newPoints = new List<Point>();

            foreach (Point p in polygon)
            {
                Point newP = RotatePoint(p, angle, midx, midy);
                newPoints.Add(newP);
            }

            return new List<Point>(newPoints);
        }

        private static Point RotatePoint(Point p, double angle, double midx, double midy)
        {
            //if (angle >= 180)
            //    angle = 180 - angle;
            //if (angle <= -180)
            //    angle = 180 + angle;
            double radians = angle * Math.PI / 180.0;
            double cosRadians = Math.Cos(radians);
            double sinRadians = Math.Sin(radians);

            double x = p.X - midx;
            double y = p.Y - midy;

            double newX = x * cosRadians - y * sinRadians + midx;
            double newY = x * sinRadians + y * cosRadians + midy;

            return new Point((int)newX, (int)newY);
        }

        internal static void AutoTeach()
        {

            List<System.Drawing.Point> p_Regionpolygon = new List<System.Drawing.Point>();
            Mat mat_DeviceLocationRegion = new Mat();
            double nAngleOutput = 0.0;
            double dScoreOutput = 0.0;
            int nCornerIndex = 0;
            System.Drawing.Rectangle rectMatchingPosition = new System.Drawing.Rectangle();
            InspectionCore.AutoTeachDatumLocation(ref p_Regionpolygon, DeviceLocationParameter.m_L_DeviceLocationRoi, DeviceLocationParameter.m_L_TemplateRoi, ref mat_DeviceLocationRegion, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput, ref nCornerIndex);
            DeviceLocationParameter.m_nBlackCornerIndexTemplateImage = nCornerIndex;
        }
    }
}
