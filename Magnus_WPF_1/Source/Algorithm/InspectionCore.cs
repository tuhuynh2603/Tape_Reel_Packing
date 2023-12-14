using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.WPF;
using Magnus_WPF_1.Source.Define;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CvImage = Emgu.CV.Mat;
using CvPointArray = Emgu.CV.Util.VectorOfPoint;
using DefectInfor = Magnus_WPF_1.UI.UserControls.DefectInfor;
using LineArray = System.Collections.Generic.List<Emgu.CV.Structure.LineSegment2D>;
using CvContourArray = Emgu.CV.Util.VectorOfVectorOfPoint;

namespace Magnus_WPF_1.Source.Algorithm
{
    using Magnus_WPF_1.Source.Application;
    using Magnus_WPF_1.Source.LogMessage;
    public class InspectionCore
    {

        public Size globalImageSize;
        TemplateMatchingModel m_TemplateMatchingModel = new TemplateMatchingModel();
        public struct ImageTarget
        {
            public CvImage Gray;
            public CvImage Bgr;
        }

        public class DeviceLocationParameter
        {
            public/* static*/ Rectangles m_L_DeviceLocationRoi = new Rectangles();
            public THRESHOLD_TYPE m_L_ThresholdType = THRESHOLD_TYPE.BINARY_THRESHOLD;
            public OBJECT_COLOR m_L_ObjectColor = OBJECT_COLOR.BLACK;
            public/* static*/ int m_L_lowerThreshold = 0;
            public/* static*/ int m_L_upperThreshold = 255;

            public/* static*/ int m_L_lowerThresholdInnerChip = 0;
            public/* static*/ int m_L_upperThresholdInnerChip = 255;

            public/* static*/ int m_nOpeningMask = 11;
            public/* static*/ int m_nMinWidthDevice = 50;
            public/* static*/ int m_nMinHeightDevice = 50;
            public/* static*/ int m_nDilationMask = 30;

            public/* static*/ Rectangles m_L_TemplateRoi = new Rectangles();
            public/* static*/ int m_nStepTemplate = 4;
            public/* static*/ double m_dScaleImageRatio = 0.1;
            public/* static*/ double m_dMinScoreTemplate = 50.0;
            public/* static*/ int m_nBlackCornerIndexTemplateImage = 0;

        }

        public class DeviceLocationResult
        {
           public PointF m_dCenterDevicePoint = new PointF(0, 0);
            public PointF m_dCornerDevicePoint = new PointF(0, 0);
           public double m_dAngleOxDevice = 0.0;
        }

        public class SurfaceDefectParameter
        {

            public List<Rectangles> m_LD_LabelLocations = new List<Rectangles>();
            public/* static*/ int m_LD_NumberROILocation = 4;
            public/* static*/ double m_LD_lowerThreshold = 0.1;
            public/* static*/ double m_LD_upperThreshold = 50.0;
            public/* static*/ int m_LD_DilationMask = 0;
            public/* static*/ int m_LD_OpeningMask = 0;
        }

        public /*static*/ ImageTarget m_SourceImage;
        public /*static*/ ImageTarget m_TeachImage;
        public /*static*/ ImageTarget m_TemplateImage;

        public /*static*/ DeviceLocationParameter m_DeviceLocationParameter;
        public /*static*/ SurfaceDefectParameter m_SurfaceDefectParameter;
        public DeviceLocationResult m_DeviceLocationResult;
        public InspectionCore(ref Size mImageSize)
        {
            globalImageSize = new Size(mImageSize.Width, mImageSize.Height);
            m_SourceImage = new ImageTarget();
            m_TeachImage = new ImageTarget();
            m_TemplateImage = new ImageTarget();
            m_DeviceLocationParameter = new DeviceLocationParameter();
            m_SurfaceDefectParameter = new SurfaceDefectParameter();
            m_DeviceLocationResult = new DeviceLocationResult();
        }
        //public /*static*/ bool Initialize()
        //{
        //    m_SourceImage = new ImageTarget();
        //    m_TeachImage = new ImageTarget();
        //    m_TemplateImage = new ImageTarget();
        //    m_DeviceLocationParameter = new DeviceLocationParameter();
        //    globalImageSize = new Size(Application.Track.m_Width, Application.Track.m_Height);
        //    return true;
        //}

        public /*static*/ bool LoadImageToInspection(BitmapSource btmSource)
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

        public /*static*/ bool LoadOfflineImage(string strPath)
        {
            try
            {
                m_SourceImage.Gray = CvInvoke.Imread(strPath, Emgu.CV.CvEnum.ImreadModes.Grayscale);
                m_SourceImage.Bgr = CvInvoke.Imread(strPath, Emgu.CV.CvEnum.ImreadModes.Color);

                return true;
            }
            catch
            {
                return false;
            }
        }

        //public /*static*/ void SaveTemplateImage(string strTemplateImageFile)
        //{
        //    try
        //    {
        //        CvInvoke.Imwrite(strTemplateImageFile, m_TemplateImage.Gray);
        //    }
        //    catch
        //    {

        //    }
        //}

        //public /*static*/ void SaveInspectImage(string strPath, string strDeviceID)
        //{
        //    try
        //    {
        //        if (Directory.Exists(strPath) == false)
        //        {
        //            Directory.CreateDirectory(strPath);
        //        }

        //        string strImageName = System.IO.Path.Combine(strPath, "Device_" + strDeviceID + ".bmp");
        //        if (Directory.Exists(strPath))
        //            CvInvoke.Imwrite(strImageName, m_SourceImage.Gray);

        //    }
        //    catch { }
        //}
        public /*static*/ bool UpdateTeachParamFromUIToInspectionCore()
        {

            m_DeviceLocationParameter.m_L_ThresholdType = Source.Application.Application.categoriesTeachParam.L_ThresholdType;
            m_DeviceLocationParameter.m_L_ObjectColor = Source.Application.Application.categoriesTeachParam.L_ObjectColor;

            
            m_DeviceLocationParameter.m_L_lowerThreshold = Source.Application.Application.categoriesTeachParam.L_lowerThreshold;
            m_DeviceLocationParameter.m_L_upperThreshold = Source.Application.Application.categoriesTeachParam.L_upperThreshold;

            m_DeviceLocationParameter.m_L_lowerThresholdInnerChip = Source.Application.Application.categoriesTeachParam.L_lowerThresholdInnerChip;
            m_DeviceLocationParameter.m_L_upperThresholdInnerChip = Source.Application.Application.categoriesTeachParam.L_upperThresholdInnerChip;


            if (Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi.Width > globalImageSize.Width)
                Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi = new Rectangles(new System.Windows.Point(300,300), 300,300);

            m_DeviceLocationParameter.m_L_DeviceLocationRoi = Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi;

            m_DeviceLocationParameter.m_nOpeningMask = Source.Application.Application.categoriesTeachParam.L_OpeningMask;
            m_DeviceLocationParameter.m_nDilationMask = Source.Application.Application.categoriesTeachParam.L_DilationMask;
            m_DeviceLocationParameter.m_nMinWidthDevice = Source.Application.Application.categoriesTeachParam.L_MinWidthDevice;
            m_DeviceLocationParameter.m_nMinHeightDevice = Source.Application.Application.categoriesTeachParam.L_MinHeightDevice;

            if (Source.Application.Application.categoriesTeachParam.L_TemplateRoi.Width > globalImageSize.Width)
                Source.Application.Application.categoriesTeachParam.L_TemplateRoi = new Rectangles(new System.Windows.Point(300, 300), 300, 300);
            m_DeviceLocationParameter.m_L_TemplateRoi = Source.Application.Application.categoriesTeachParam.L_TemplateRoi;

            m_DeviceLocationParameter.m_nStepTemplate = Source.Application.Application.categoriesTeachParam.L_NumberSide;
            m_DeviceLocationParameter.m_dScaleImageRatio = Source.Application.Application.categoriesTeachParam.L_ScaleImageRatio;
            m_DeviceLocationParameter.m_dMinScoreTemplate = Source.Application.Application.categoriesTeachParam.L_MinScore;
            m_DeviceLocationParameter.m_nBlackCornerIndexTemplateImage = Source.Application.Application.categoriesTeachParam.L_CornerIndex;
            //master.m_Tracks[0].m_imageViews[0].SaveTeachImage(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_1.bmp"));
            //master.m_Tracks[0].m_imageViews[0].saveTemplateImage(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "templateImage_1.bmp"));

            m_SurfaceDefectParameter.m_LD_LabelLocations.Clear();
            for (int n = 0; n< 5; n++)
            {
                if (Source.Application.Application.categoriesTeachParam.LD_LabelLocations[n].Width > globalImageSize.Width)
                    Source.Application.Application.categoriesTeachParam.LD_LabelLocations[n] = new Rectangles(new System.Windows.Point(300, 300), 300, 300);

                m_SurfaceDefectParameter.m_LD_LabelLocations.Add(Source.Application.Application.categoriesTeachParam.LD_LabelLocations[n]);
            }
            m_SurfaceDefectParameter.m_LD_NumberROILocation = Source.Application.Application.categoriesTeachParam.LD_NumberROILocation;
            m_SurfaceDefectParameter.m_LD_lowerThreshold = Source.Application.Application.categoriesTeachParam.LD_lowerThreshold;
            m_SurfaceDefectParameter.m_LD_upperThreshold = Source.Application.Application.categoriesTeachParam.LD_upperThreshold;
            m_SurfaceDefectParameter.m_LD_DilationMask = Source.Application.Application.categoriesTeachParam.LD_DilationMask;
            m_SurfaceDefectParameter.m_LD_OpeningMask = Source.Application.Application.categoriesTeachParam.LD_OpeningMask;
            return true;
        }
        public /*static*/ bool LoadTeachImageToInspectionCore(int nTrack)
        {
            try
            {
                m_SourceImage.Gray =   CvInvoke.Imread(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_Track" + (nTrack +1).ToString() + ".bmp"), Emgu.CV.CvEnum.ImreadModes.Grayscale);
                m_TeachImage.Gray =    CvInvoke.Imread(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_Track" + (nTrack + 1).ToString() + ".bmp"), Emgu.CV.CvEnum.ImreadModes.Grayscale);
                m_TemplateImage.Gray = CvInvoke.Imread(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "templateImage_Track" + (nTrack + 1).ToString() + ".bmp"), Emgu.CV.CvEnum.ImreadModes.Grayscale);

            }
            catch { }

            return true;
        }

        public /*static*/ void PushBackDebugInfors(CvImage debugImage, CvImage debugRegion, string debugMessage, bool bEnableDebug, ref List<DefectInfor.DebugInfors> debugInfors)
        {
            if (!bEnableDebug)
            {
                return;
            }

            if (debugRegion.Width == 0)
                debugRegion = CvImage.Zeros(m_SourceImage.Gray.Height, m_SourceImage.Gray.Width, DepthType.Cv8U, 1);

            CvImage zoomedImage = new CvImage(m_SourceImage.Gray.Height, m_SourceImage.Gray.Width, DepthType.Cv8U, 3);
            CvImage zoomedRegion = new CvImage(m_SourceImage.Gray.Height, m_SourceImage.Gray.Width, DepthType.Cv8U, 1);
            CvInvoke.Resize(debugImage, zoomedImage, new System.Drawing.Size(zoomedImage.Width, zoomedImage.Height));
            CvInvoke.Resize(debugRegion, zoomedRegion, new System.Drawing.Size(zoomedImage.Width, zoomedImage.Height));
            debugInfors.Add(new DefectInfor.DebugInfors() { mat_Image = zoomedImage, mat_Region = zoomedRegion, str_Step = (debugInfors.Count() + 1).ToString(), str_Message = debugMessage });

        }

        public /*static*/ void AddRegionOverlay(ref List<ArrayOverLay> list_arrayOverlay, Mat mat_region, System.Windows.Media.Color color)
        {

            if (mat_region.Width == 0)
                mat_region = CvImage.Zeros(m_SourceImage.Gray.Height, m_SourceImage.Gray.Width, DepthType.Cv8U, 1);

            CvImage zoomedRegion = new CvImage(m_SourceImage.Gray.Height, m_SourceImage.Gray.Width, DepthType.Cv8U, 1);
            CvInvoke.Resize(mat_region, zoomedRegion, new System.Drawing.Size(zoomedRegion.Width, zoomedRegion.Height));
            list_arrayOverlay.Add(new ArrayOverLay() { mat_Region = zoomedRegion, _color = color });

        }

        public /*static*/ int Inspect(ref ImageTarget InspectImage, ref List<ArrayOverLay> list_arrayOverlay, ref PointF pCenter, ref PointF pCorner, ref List<DefectInfor.DebugInfors> debugInfors, bool bEnableDebug = false)
        {
            int nError;
            Stopwatch timeIns = new Stopwatch();
            timeIns.Start();
            //double dScale = 3;
            nError = FindDeviceLocation_Zoom(ref InspectImage.Gray,
                                             ref list_arrayOverlay, ref pCenter, ref pCorner, ref  debugInfors, bEnableDebug);

            //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("FindDeviceLocation_Zoom time: " + timeIns.ElapsedMilliseconds.ToString(), (int)ERROR_CODE.NO_LABEL);

            //});
            timeIns.Restart();

            return nError;
        }

        public /*static*/ void SetTemplateImage()
        {
            Image<Gray, Byte> imgCropped = new Image<Gray, byte>(m_TeachImage.Gray.Bitmap);
            System.Drawing.Rectangle rec = new System.Drawing.Rectangle();
            rec.X = (int)m_DeviceLocationParameter.m_L_TemplateRoi.TopLeft.X;
            rec.Y = (int)m_DeviceLocationParameter.m_L_TemplateRoi.TopLeft.Y;
            rec.Width = (int)m_DeviceLocationParameter.m_L_TemplateRoi.Width;
            rec.Height = (int)m_DeviceLocationParameter.m_L_TemplateRoi.Height;
            imgCropped.ROI = rec;
            m_TemplateImage.Gray = imgCropped.Mat.Clone();
            //CvInvoke.Imshow("Template Image", m_TemplateImage.Gray);
            //CvInvoke.WaitKey();
        }

        public void SaveCurrentSourceImage(string strFullFileName, int nType = 0)
        {
            if(nType == 0)
            {
                CvInvoke.Imwrite(strFullFileName, m_SourceImage.Gray);
            }
            else
            {
                //m_SourceImage.Gray.Save(strFullFileName);
                string strtemp = strFullFileName.Replace(".bmp", ".jpg");
                CvInvoke.Imwrite(strtemp, m_SourceImage.Gray, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, 20));


            }
        }

        //public static int FindDeviceLocation_Old(ref CvImage imgSource,
        //    ref List<Point> p_Regionpolygon, ref Point pCenter, ref Mat mat_DeviceLocationRegion, ref double nAngleOutput, ref double dScoreOutput)
        //{
        //    if (m_TemplateImage.Gray == null)
        //        return -99;

        //    Stopwatch timeIns = new Stopwatch();
        //    CvImage img_thresholdRegion = new CvImage();
        //    CvImage img_openingRegionRegion = new CvImage();
        //    CvImage img_BiggestRegion = new CvImage();
        //    CvImage img_SelectRegion = new CvImage();
        //    CvImage img_DilationRegion = new CvImage();

        //    CvImage region_Crop = new CvImage();
        //    mat_DeviceLocationRegion = new CvImage();

        //    System.Drawing.Rectangle rectDeviceLocation = new System.Drawing.Rectangle((int)DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.X,
        //                                                                                (int)DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.Y,
        //                                                                                (int)DeviceLocationParameter.m_L_DeviceLocationRoi.Width,
        //                                                                               (int)DeviceLocationParameter.m_L_DeviceLocationRoi.Height);
        //    CvImage region_SearchDeviceLocation = new CvImage();
        //    region_SearchDeviceLocation = CvImage.Zeros(imgSource.Height, imgSource.Width, DepthType.Cv8U, 1);
        //    CvInvoke.Rectangle(region_SearchDeviceLocation, rectDeviceLocation, new MCvScalar(255), -1);

        //    //CvInvoke.WaitKey(0);
        //    timeIns.Start();
        //    MagnusOpenCVLib.Threshold2(ref imgSource, ref img_thresholdRegion, DeviceLocationParameter.m_L_lowerThreshold, DeviceLocationParameter.m_L_upperThreshold);
        //    LogMessage.WriteToDebugViewer(1, "Threshold 1 time: " + timeIns.ElapsedMilliseconds.ToString());
        //    timeIns.Restart();

        //    CvInvoke.BitwiseAnd(img_thresholdRegion, region_SearchDeviceLocation, img_thresholdRegion);
        //    MagnusOpenCVLib.OpeningRectangle(ref img_thresholdRegion, ref img_openingRegionRegion, DeviceLocationParameter.m_nOpeningMask, DeviceLocationParameter.m_nOpeningMask);
        //    MagnusOpenCVLib.SelectBiggestRegion(ref img_openingRegionRegion, ref mat_DeviceLocationRegion);
        //    List<System.Drawing.Rectangle> rectLabel = new List<System.Drawing.Rectangle>();

        //    MagnusOpenCVLib.SelectRegion(ref mat_DeviceLocationRegion, ref img_SelectRegion, ref rectLabel, DeviceLocationParameter.m_nMinWidthDevice, DeviceLocationParameter.m_nMinHeightDevice);
        //    LogMessage.WriteToDebugViewer(1, "Select region time: " + timeIns.ElapsedMilliseconds.ToString());
        //    timeIns.Restart();
        //    if (rectLabel == null)
        //        return -99;

        //    MagnusOpenCVLib.DilationRectangle(ref img_SelectRegion, ref img_DilationRegion, DeviceLocationParameter.m_nDilationMask, DeviceLocationParameter.m_nDilationMask);
        //    System.Drawing.Rectangle rectangleRoi = new System.Drawing.Rectangle();
        //    Image<Gray, Byte> ImageAfterDilationCrop = new Image<Gray, Byte>(imgSource.Bitmap);
        //    Image<Gray, Byte> Img = new Image<Gray, Byte>(imgSource.Bitmap);

        //    LogMessage.WriteToDebugViewer(1, "DilationRectangle time: " + timeIns.ElapsedMilliseconds.ToString());
        //    timeIns.Restart();

        //    //int nWidth = 0, nHeight = 0;
        //    //MagnusOpenCVLib.GetWidthHeightRegion(ref img_DilationRegion, ref nWidth, ref nHeight);
        //    //if(nWidth >= m_TemplateImage.Gray.Width && nHeight >= m_TemplateImage.Gray.Height)
        //    //    MagnusOpenCVLib.CropImage(ref ImageAfterDilationCrop, ref Img, img_DilationRegion, ref rectangleRoi);
        //    //else
        //        MagnusOpenCVLib.CropImage(ref ImageAfterDilationCrop, ref Img, region_SearchDeviceLocation, ref rectangleRoi);

        //    LogMessage.WriteToDebugViewer(1, "CropImage time: " + timeIns.ElapsedMilliseconds.ToString());
        //    timeIns.Restart();
        //    System.Drawing.Rectangle rectMatchingPosition = new System.Drawing.Rectangle();
        //    bool bIsTemplateFounded = false;
        //    //if (ImageAfterDilationCrop.Width <= m_TemplateImage.Gray.Width || ImageAfterDilationCrop.Height <= m_TemplateImage.Gray.Height)
        //    //{
        //    //    if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
        //    //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(imgSource, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
        //    //    else
        //    //    {
        //    //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(imgSource, m_TemplateImage.Gray, 0, 24, 15, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

        //    //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(imgSource, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

        //    //    }
        //    //}
        //    //else
        //    //{
        //        //if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
        //        //    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
        //        //else
        //        //{
        //            bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, m_TemplateImage.Gray, 0, 24, 15, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

        //            bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(ImageAfterDilationCrop.Mat, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dScaleImageRatio, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
        //        //}
        //    //}


        //    LogMessage.WriteToDebugViewer(1, "template Matching time: " + timeIns.ElapsedMilliseconds.ToString());
        //    timeIns.Restart();
        //    List<Point> pPolygon = new List<Point>();
        //    pCenter = new Point((rectMatchingPosition.Left + rectMatchingPosition.Right) / 2 + rectangleRoi.Left,
        //                  (rectMatchingPosition.Top + rectMatchingPosition.Bottom) / 2 + rectangleRoi.Top);

        //    //top bottom left right  x1 x2 y1 y2
        //    Point po1 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
        //    Point po2 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
        //    Point po3 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);
        //    Point po4 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);

        //    pPolygon.Add(po1);
        //    pPolygon.Add(po2);
        //    pPolygon.Add(po3);
        //    pPolygon.Add(po4);
        //    pPolygon.Add(pCenter);

        //    List<Point> p_Regionpolygon_temp = RotatePolygon(pPolygon, -nAngleOutput, pCenter.X, pCenter.Y);

        //    int nminIndex = FindNearestPoints(imgSource, ref mat_DeviceLocationRegion, rectMatchingPosition, p_Regionpolygon_temp, (float)nAngleOutput);

        //    nAngleOutput = nAngleOutput + (DeviceLocationParameter.m_nBlackCornerIndexTemplateImage - nminIndex) * 90;
        //    if (nAngleOutput <= -180)
        //        nAngleOutput = 360 - nAngleOutput;

        //    if (nAngleOutput >= 180)
        //        nAngleOutput = nAngleOutput - 360;
        //    p_Regionpolygon = RotatePolygon(pPolygon, -nAngleOutput, pCenter.X, pCenter.Y);
        //    p_Regionpolygon.Remove(pCenter);

        //    LogMessage.WriteToDebugViewer(1, "FindNearestPoints time: " + timeIns.ElapsedMilliseconds.ToString());
        //    timeIns.Stop();

        //    if (bIsTemplateFounded)
        //        return 0;
        //    else
        //        return -1;

        //}

        public int LabelInspection(ref CvImage imgSource, List<ArrayOverLay> list_arrayOverlay, ref List<DefectInfor.DebugInfors> debugInfors, bool bEnableDebug = false)
        {

            System.Drawing.Rectangle rectLabel1 = new System.Drawing.Rectangle((int)(m_DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.X * m_DeviceLocationParameter.m_dScaleImageRatio),
                                                                            (int)(m_DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.Y * m_DeviceLocationParameter.m_dScaleImageRatio),
                                                                            (int)(m_DeviceLocationParameter.m_L_DeviceLocationRoi.Width * m_DeviceLocationParameter.m_dScaleImageRatio),
                                                                           (int)(m_DeviceLocationParameter.m_L_DeviceLocationRoi.Height * m_DeviceLocationParameter.m_dScaleImageRatio));

            return -(int)ERROR_CODE.PASS;
        }


        public int FindBlackChip(ref CvImage imgSource, ref CvImage mat_RegionInspectionROI, ref PointF pCenter, ref List<ArrayOverLay> list_arrayOverlay, ref List<DefectInfor.DebugInfors> debugInfors, bool bEnableDebug = false)
        {
            Stopwatch timeIns = new Stopwatch();

            //if (m_TemplateImage.Gray == null)
            //    return -99;
            timeIns.Start();
            int m_nLowerBlackChipThreshold = 0;
            int m_nUpperBlackChipThreshold = 30;
            int m_nOpeningBlackChipThreshold = 5;
            int m_nOpeningBlackChipThreshold2 = 15;

            CvImage img_thresholdRegion = new CvImage();
            MagnusOpenCVLib.Threshold2(ref imgSource, ref img_thresholdRegion, m_nLowerBlackChipThreshold, m_nUpperBlackChipThreshold);
            CvInvoke.BitwiseAnd(mat_RegionInspectionROI, img_thresholdRegion, img_thresholdRegion);
            PushBackDebugInfors(imgSource, img_thresholdRegion, "Check Black Chip Region: img_thresholdRegion. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage img_FillUpRegionWithSize = new CvImage();

            //MagnusOpenCVLib.FillUpSmallShapes(ref img_thresholdRegion, ref img_FillUpRegionWithSize, 0, 3);
            //PushBackDebugInfors(imgSource, img_FillUpRegionWithSize, "Check Black Chip Region: FillUp with size. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            //timeIns.Restart();

            CvImage img_OpeningRegion = new CvImage();
            MagnusOpenCVLib.OpeningCircle(ref img_thresholdRegion, ref img_OpeningRegion, m_nOpeningBlackChipThreshold);
            PushBackDebugInfors(imgSource, img_OpeningRegion, "Check Black Chip Region: img_OpeningRegion. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();
            CvImage img_FillUpRegion = new CvImage();

            MagnusOpenCVLib.FillUp(ref img_OpeningRegion, ref img_FillUpRegion);
            PushBackDebugInfors(imgSource, img_FillUpRegion, "Check Black Chip Region: FillUp. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage img_OpeningRegion2 = new CvImage();
            MagnusOpenCVLib.OpeningCircle(ref img_FillUpRegion, ref img_OpeningRegion2, m_nOpeningBlackChipThreshold2);
            PushBackDebugInfors(imgSource, img_OpeningRegion2, "Check Black Chip Region: img_OpeningRegion. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();


            CvImage img_BiggestRegion = new CvImage();
            MagnusOpenCVLib.SelectBiggestRegion(ref img_OpeningRegion2, ref img_BiggestRegion);
            PushBackDebugInfors(imgSource, img_BiggestRegion, "Check Black Chip Region: img_BiggestRegion. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();
            CvPointArray point_regions = new CvPointArray();

            CvInvoke.FindNonZero(img_BiggestRegion, point_regions);
            if (point_regions.Size == 0)
                return -(int)ERROR_CODE.PASS;

            CvContourArray contours2 = new CvContourArray();
            MagnusOpenCVLib.GenContourRegion(ref img_BiggestRegion, ref contours2, RetrType.External);
            RotatedRect rotateRect_Device = new RotatedRect();
            rotateRect_Device = CvInvoke.MinAreaRect(contours2[0]);
            if (rotateRect_Device.Size.Width < (int)(m_DeviceLocationParameter.m_nMinWidthDevice * m_DeviceLocationParameter.m_dScaleImageRatio/2)
                || rotateRect_Device.Size.Height < (int)(m_DeviceLocationParameter.m_nMinHeightDevice * m_DeviceLocationParameter.m_dScaleImageRatio/2))
                return -(int)ERROR_CODE.PASS;

            AddRegionOverlay(ref list_arrayOverlay, img_BiggestRegion, Colors.Red);


            CvImage distanceTransform = new CvImage();
            CvInvoke.DistanceTransform(img_BiggestRegion, distanceTransform, null, DistType.L2, 5);
            PushBackDebugInfors(distanceTransform, distanceTransform, "Check Black Chip Region: distanceTransform. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage normalizedDistanceTransform = new CvImage();
            CvInvoke.Normalize(distanceTransform, normalizedDistanceTransform, 0, 255, NormType.MinMax, DepthType.Cv8U);

            PushBackDebugInfors(normalizedDistanceTransform, normalizedDistanceTransform, "Check Black Chip Region: Normalize. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage img_thresholdTransformRegion = new CvImage();
            MagnusOpenCVLib.Threshold2(ref normalizedDistanceTransform, ref img_thresholdTransformRegion, 245, 255);
            PushBackDebugInfors(imgSource, img_thresholdTransformRegion, "Check Black Chip Region: img_thresholdTransformRegion. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();


            CvInvoke.FindNonZero(img_thresholdTransformRegion, point_regions);
            if (point_regions.Size == 0)
                return -(int)ERROR_CODE.PASS;


            List<Rectangle> corner = new List<Rectangle>();
            CvImage result = new CvImage();
            CvContourArray contourArray = new CvContourArray();
            //CvContourArray contourArrayHull = new CvContourArray();

            MagnusOpenCVLib.ExtractExternalContours(ref img_thresholdTransformRegion, ref result, ref contourArray);
            //MagnusOpenCVLib.ConvexHull(contourArray, ref contourArrayHull, ref result, imgSource.Size);
            PushBackDebugInfors(imgSource, result, "Check Black Chip Region: ConvexHull. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();
            List<PointF> listContourPoints = new List<PointF>();
            for (int n = 0; n < contourArray.Size; n++)
            {
                CvPointArray PointArrayTemp = contourArray[n];
                for (int m = 0; m < PointArrayTemp.Size; m++)
                {
                    listContourPoints.Add(PointArrayTemp[m]);

                }
            }
            //rect = CvInvoke.BoundingRectangle(PointArray);
            //corner.Add(rect);
            //CvImage region_Rect = new CvImage();
            //region_Rect = CvImage.Zeros(imgSource.Height, imgSource.Width, DepthType.Cv8U, 1);
            //foreach(Rectangle r in corner)
            //{
            //    CvInvoke.Rectangle(region_Rect, r, new MCvScalar(255), -1);
            //}

            //PushBackDebugInfors(imgSource, region_Rect, "Check Black Chip Region: region_Rect. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            //timeIns.Restart();


            PointF pTemp = new PointF(0, 0);
            MagnusOpenCVLib.SelectPointBased_Top_Left_Bottom_Right(ref listContourPoints, ref pTemp, (int)POSITION._TOP);

            //CvContourArray contourArrayHullTLBR = new CvContourArray();
            //CvImage center_BlackChipRegion =  CvImage.Zeros(imgSource.Rows, imgSource.Cols, DepthType.Cv8U, 1);
            //MCvScalar sColor = new MCvScalar(255);
            //bool bFound = false;
            //for (int n = 0; n < contourArray.Size; n++)
            //{
            //    CvPointArray PointArrayTemp = contourArray[n];
            //    for (int m = 0; m < PointArrayTemp.Size; m++)
            //    {
            //        if(PointArrayTemp[m] != pTemp)
            //            continue;

            //        contourArrayHullTLBR.Push(contourArray[n]);
            //        MagnusOpenCVLib.DrawContours(ref center_BlackChipRegion, ref contourArrayHullTLBR, imgSource.Size, sColor, 1, -1);


            //        bFound = true;
            //        break;
            //    }
            //    if (bFound)
            //        break;
            //}

            //if (!bFound)
            //    return -(int)ERROR_CODE.PROCESS_ERROR;

            //RotatedRect rotateRectContour = new RotatedRect();
            //rotateRectContour = CvInvoke.MinAreaRect(contours2[0]);
            //if (rotateRectContour.Size.Width <  rotateRectContour.Size.Height)
            //    return -(int)ERROR_CODE.PASS;

            //List<PointF> listContourPointsTLBR = new List<PointF>();
            //for (int n = 0; n < contourArrayHullTLBR[0].Size; n++)
            //{
            //    listContourPointsTLBR.Add(contourArrayHullTLBR[0][n]);
            //}



            CvImage mat_point = CvImage.Zeros(imgSource.Height, imgSource.Width, DepthType.Cv8U, 1);
            System.Drawing.Rectangle rect_center = new System.Drawing.Rectangle((int)(pTemp.X),
                                                                            (int)(pTemp.Y),
                                                                            (int)(4),
                                                                           (int)(4));

            CvInvoke.Rectangle(mat_point, rect_center, new MCvScalar(255), -1);
            PushBackDebugInfors(imgSource, mat_point, "Center Chip Region . (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();
            AddRegionOverlay(ref list_arrayOverlay, mat_point, Colors.Yellow);

            pCenter = new Point((int)(pTemp.X / m_DeviceLocationParameter.m_dScaleImageRatio), (int)(pTemp.Y / m_DeviceLocationParameter.m_dScaleImageRatio));

            return -(int)ERROR_CODE.OPPOSITE_CHIP;
        }

        public /*static*/ int FindDeviceLocation_Zoom(ref CvImage imgSource, ref List<ArrayOverLay> list_arrayOverlay, ref PointF pCenter, ref PointF pCorner, ref List<DefectInfor.DebugInfors> debugInfors, bool bEnableDebug = false)
        {
            Stopwatch timeIns = new Stopwatch();

            //if (m_TemplateImage.Gray == null)
            //    return -99;
            timeIns.Start();

            CvImage zoomedInImage = new CvImage((int)(imgSource.Height * m_DeviceLocationParameter.m_dScaleImageRatio), (int)(imgSource.Width * m_DeviceLocationParameter.m_dScaleImageRatio), DepthType.Cv8U, 3);
            CvInvoke.Resize(imgSource, zoomedInImage, new System.Drawing.Size(zoomedInImage.Width, zoomedInImage.Height));        
            CvImage zoomedInImage_Scale = new CvImage((int)(imgSource.Height * m_DeviceLocationParameter.m_dScaleImageRatio), (int)(imgSource.Width * m_DeviceLocationParameter.m_dScaleImageRatio), DepthType.Cv8U, 3);
            CvImage img_thresholdRegion = new CvImage();
            CvImage img_BiggestRegion = new CvImage();
            CvImage img_SelectRegion = new CvImage();
            CvImage img_DilationRegion = new CvImage();
            CvPointArray point_regions = new CvPointArray();

            CvImage region_Crop = new CvImage();
            //mat_DeviceLocationRegion = new CvImage();
            System.Drawing.Rectangle rectDeviceLocation = new System.Drawing.Rectangle((int)(m_DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.X * m_DeviceLocationParameter.m_dScaleImageRatio),
                                                                                        (int)(m_DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.Y * m_DeviceLocationParameter.m_dScaleImageRatio),
                                                                                        (int)(m_DeviceLocationParameter.m_L_DeviceLocationRoi.Width * m_DeviceLocationParameter.m_dScaleImageRatio),
                                                                                       (int)(m_DeviceLocationParameter.m_L_DeviceLocationRoi.Height * m_DeviceLocationParameter.m_dScaleImageRatio));
            CvImage region_SearchDeviceLocation = new CvImage();
            region_SearchDeviceLocation = CvImage.Zeros(zoomedInImage.Height, zoomedInImage.Width, DepthType.Cv8U, 1);
            CvInvoke.Rectangle(region_SearchDeviceLocation, rectDeviceLocation, new MCvScalar(255), -1);

            PushBackDebugInfors(zoomedInImage, region_SearchDeviceLocation, "Region_SearchDeviceLocationt. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            // Find black Chip first
            int nError = FindBlackChip(ref zoomedInImage, ref region_SearchDeviceLocation, ref pCenter, ref list_arrayOverlay, ref debugInfors, bEnableDebug);
            //if (nError < (int)ERROR_CODE.PASS)
            //    return nError;

            //CvInvoke.WaitKey(0);
            timeIns.Restart();

            if (m_DeviceLocationParameter.m_L_ThresholdType.GetHashCode() == (int)THRESHOLD_TYPE.BINARY_THRESHOLD)
            {

                MagnusOpenCVLib.Threshold2(ref zoomedInImage, ref img_thresholdRegion, m_DeviceLocationParameter.m_L_lowerThreshold, m_DeviceLocationParameter.m_L_upperThreshold);
                PushBackDebugInfors(imgSource, img_thresholdRegion, "img_thresholdRegion. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
                timeIns.Restart();

                //CvImage mat_IntersectionRegion = new CvImage();
                CvInvoke.BitwiseAnd(img_thresholdRegion, region_SearchDeviceLocation, img_thresholdRegion);
                PushBackDebugInfors(imgSource, img_thresholdRegion, "region after Intersection with Device Location ROI (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
                timeIns.Restart();
            }
            else//if (m_DeviceLocationParameter.m_L_ThresholdType.GetHashCode() == (int)THRESHOLD_TYPE.VAR_THRESHOLD)
            {
                //CvImage region_VarThreshold = new CvImage();
                MagnusOpenCVLib.VarThresholding(ref zoomedInImage, ref img_thresholdRegion, (int)m_DeviceLocationParameter.m_L_ObjectColor, 2 * ((int)(m_DeviceLocationParameter.m_nMinWidthDevice / 10)) + 3, ref region_SearchDeviceLocation, m_DeviceLocationParameter.m_L_upperThreshold);
                PushBackDebugInfors(imgSource, img_thresholdRegion, "VarThresholding. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
                timeIns.Restart();
            }

            CvImage img_MophoRegion = new CvImage();
            MagnusOpenCVLib.OpeningCircle(ref img_thresholdRegion, ref img_MophoRegion, 2);
            PushBackDebugInfors(imgSource, img_MophoRegion, "Outer region after Opening. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();


            CvImage img_MophoRegion2 = new CvImage();
            MagnusOpenCVLib.ClosingCircle(ref img_MophoRegion, ref img_MophoRegion2, (int)(m_DeviceLocationParameter.m_nDilationMask ) + 1);
            PushBackDebugInfors(imgSource, img_MophoRegion2, "Outer region after ClosingCircle. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            List<Rectangle> rect_SelectRectangle = new List<Rectangle>();
            CvImage mat_selectedtRegions = new CvImage();

            MagnusOpenCVLib.SelectRegion(ref img_MophoRegion2, ref mat_selectedtRegions, ref rect_SelectRectangle, (int)(m_DeviceLocationParameter.m_nMinWidthDevice * m_DeviceLocationParameter.m_dScaleImageRatio), (int)(m_DeviceLocationParameter.m_nMinHeightDevice * m_DeviceLocationParameter.m_dScaleImageRatio));
            PushBackDebugInfors(imgSource, mat_selectedtRegions, "Outer region after SelectRegion with Width and Height. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvInvoke.FindNonZero(mat_selectedtRegions, point_regions);
            if (point_regions.Size == 0)
                return - (int)ERROR_CODE.NO_PATTERN_FOUND;

            CvImage mat_FillUpBlackRegion = new CvImage();
            MagnusOpenCVLib.FillUp(ref mat_selectedtRegions, ref mat_FillUpBlackRegion);
            PushBackDebugInfors(imgSource, mat_FillUpBlackRegion, "Outer region after FillUp Select Region. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            //////////////////////////////////////////////////
            ////  segment single chip region
            CvImage img_thresholdInnerRegion = new CvImage();
            MagnusOpenCVLib.Threshold2(ref zoomedInImage, ref img_thresholdInnerRegion, m_DeviceLocationParameter.m_L_lowerThresholdInnerChip, m_DeviceLocationParameter.m_L_upperThresholdInnerChip);
            PushBackDebugInfors(imgSource, img_thresholdInnerRegion, "Inner threshold Region. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();
            CvImage mat_InnerChipRegion = new CvImage();
            CvInvoke.BitwiseAnd(img_thresholdInnerRegion, mat_FillUpBlackRegion, mat_InnerChipRegion);
            PushBackDebugInfors(imgSource, mat_InnerChipRegion, "Inner threshold region after Intersection with Black FillUp Region (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage mat_InnerChipFillUpRegion = new CvImage();
            MagnusOpenCVLib.FillUp(ref mat_InnerChipRegion, ref mat_InnerChipFillUpRegion);
            PushBackDebugInfors(imgSource, mat_InnerChipFillUpRegion, "Inner Chip Region after FillUp. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();
            
            CvImage mat_InnerChipOpeningRegion = new CvImage();
            MagnusOpenCVLib.OpeningCircle(ref mat_InnerChipFillUpRegion, ref mat_InnerChipOpeningRegion, (int)(m_DeviceLocationParameter.m_nOpeningMask) + 1 );
            PushBackDebugInfors(imgSource, mat_InnerChipOpeningRegion, "Inner Chip Region after Opening. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage mat_BiggestInnerChipRegion;

            RotatedRect rotateRect_Device = new RotatedRect();
            List<PointF> listCornerPoints = new List<PointF>();
            List<PointF> listCenterPoints = new List<PointF>();
            PointF cornerPointTemp = new PointF();
            bool bIsChipFound = false;
            while (true)
            {
                mat_BiggestInnerChipRegion = new CvImage();
                MagnusOpenCVLib.SelectBiggestRegion(ref mat_InnerChipOpeningRegion, ref mat_BiggestInnerChipRegion);
                PushBackDebugInfors(imgSource, mat_BiggestInnerChipRegion, "Inner Chip Region after select BiggestRegion. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
                timeIns.Restart();

                CvInvoke.FindNonZero(mat_BiggestInnerChipRegion, point_regions);
                if (point_regions.Size == 0)
                    break;
                /////////////
                CvContourArray contours = new CvContourArray();
                MagnusOpenCVLib.GenContourRegion(ref mat_BiggestInnerChipRegion, ref contours, RetrType.External);
                rotateRect_Device = CvInvoke.MinAreaRect(contours[0]);

                if (rotateRect_Device.Size.Width < (int)(m_DeviceLocationParameter.m_nMinWidthDevice * m_DeviceLocationParameter.m_dScaleImageRatio)
                    || rotateRect_Device.Size.Height < (int)(m_DeviceLocationParameter.m_nMinHeightDevice * m_DeviceLocationParameter.m_dScaleImageRatio))
                    break;


                CvInvoke.BitwiseXor(mat_InnerChipOpeningRegion, mat_BiggestInnerChipRegion, mat_InnerChipOpeningRegion);
                PushBackDebugInfors(imgSource, mat_InnerChipOpeningRegion, "Inner Chip Region after Remove Biggest region. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
                timeIns.Restart();
                if (Math.Abs(rotateRect_Device.Size.Width / rotateRect_Device.Size.Height - m_DeviceLocationParameter.m_L_TemplateRoi.Width / m_DeviceLocationParameter.m_L_TemplateRoi.Height) > 0.2)
                {
                   
                    continue;
                }

                cornerPointTemp = FindNearestPoints_Debug(zoomedInImage, rotateRect_Device, ref list_arrayOverlay, ref debugInfors, bEnableDebug);
                if (cornerPointTemp.X + cornerPointTemp.Y == 0)
                    continue;

                bIsChipFound = true;
                listCenterPoints.Add(new Point((int)rotateRect_Device.Center.X, (int)rotateRect_Device.Center.Y));
                listCornerPoints.Add(cornerPointTemp);

            }

            if(!bIsChipFound)
                return -(int)ERROR_CODE.NO_PATTERN_FOUND;


            int nIndexOut = MagnusOpenCVLib.SelectPointBased_Top_Left_Bottom_Right(ref listCenterPoints, ref pCenter, (int)POSITION._TOP);
            if(nIndexOut <0)
                return -(int)ERROR_CODE.NO_PATTERN_FOUND;

            pCenter.X = pCenter.X / (float)m_DeviceLocationParameter.m_dScaleImageRatio;
            pCenter.Y = pCenter.Y / (float)m_DeviceLocationParameter.m_dScaleImageRatio;
            pCorner = listCornerPoints[nIndexOut];
            pCorner.X = pCorner.X / (float)m_DeviceLocationParameter.m_dScaleImageRatio;
            pCorner.Y = pCorner.Y / (float)m_DeviceLocationParameter.m_dScaleImageRatio;

            CvImage mat_point = new CvImage();
            mat_point = CvImage.Zeros(zoomedInImage.Height, zoomedInImage.Width, DepthType.Cv8U, 1);
            System.Drawing.Rectangle rect_center = new System.Drawing.Rectangle((int)(rotateRect_Device.Center.X),
                                                                            (int)(rotateRect_Device.Center.Y),
                                                                            (int)(4),
                                                                           (int)(4));

            CvInvoke.Rectangle(mat_point, rect_center, new MCvScalar(255), -1);
            PushBackDebugInfors(imgSource, mat_point, "Center Chip Region . (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();
            AddRegionOverlay(ref list_arrayOverlay, mat_point, Colors.Yellow);
            return -(int)ERROR_CODE.PASS;
        }


        //public static int FindDeviceLocation(ref CvImage imgSource, ref List<Point> p_Regionpolygon, ref Point pCenter, ref Mat mat_DeviceLocationRegion, ref double nAngleOutput, ref double dScoreOutput)
        //{
        //    if (m_TemplateImage.Gray == null)
        //        return -99;

        //    CvImage zoomedInImage = new CvImage(m_SourceImage.Gray.Height / 3, m_SourceImage.Gray.Width / 3, DepthType.Cv8U, 3);


        //    Stopwatch timeIns = new Stopwatch();
        //    CvImage img_thresholdRegion = new CvImage();
        //    CvImage img_openingRegionRegion = new CvImage();
        //    CvImage img_BiggestRegion = new CvImage();
        //    CvImage img_SelectRegion = new CvImage();
        //    CvImage img_DilationRegion = new CvImage();

        //    CvImage region_Crop = new CvImage();
        //    mat_DeviceLocationRegion = new CvImage();
        //    System.Drawing.Rectangle rectDeviceLocation = new System.Drawing.Rectangle((int)DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.X,
        //                                                                                (int)DeviceLocationParameter.m_L_DeviceLocationRoi.TopLeft.Y,
        //                                                                                (int)DeviceLocationParameter.m_L_DeviceLocationRoi.Width,
        //                                                                               (int)DeviceLocationParameter.m_L_DeviceLocationRoi.Height);
        //    CvImage region_SearchDeviceLocation = new CvImage();
        //    region_SearchDeviceLocation = CvImage.Zeros(imgSource.Height, imgSource.Width, DepthType.Cv8U, 1);
        //    CvInvoke.Rectangle(region_SearchDeviceLocation, rectDeviceLocation, new MCvScalar(255), -1);
        //    //CvInvoke.WaitKey(0);
        //    timeIns.Start();
        //    MagnusOpenCVLib.Threshold2(ref imgSource, ref img_thresholdRegion, DeviceLocationParameter.m_L_lowerThreshold, DeviceLocationParameter.m_L_upperThreshold);
        //    //LogMessage.WriteToDebugViewer(1, "Threshold 1 time: " + timeIns.ElapsedMilliseconds.ToString());
        //    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
        //    {
        //        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Threshold 1 time: " + timeIns.ElapsedMilliseconds.ToString());

        //    });
        //    timeIns.Restart();

        //    // Intersection
        //    CvInvoke.BitwiseAnd(img_thresholdRegion, region_SearchDeviceLocation, img_thresholdRegion);

        //    MagnusOpenCVLib.OpeningRectangle(ref img_thresholdRegion, ref img_openingRegionRegion, DeviceLocationParameter.m_nOpeningMask, DeviceLocationParameter.m_nOpeningMask);
        //    MagnusOpenCVLib.SelectBiggestRegion(ref img_openingRegionRegion, ref mat_DeviceLocationRegion);
        //    List<System.Drawing.Rectangle> rectLabel = new List<System.Drawing.Rectangle>();

        //    MagnusOpenCVLib.SelectRegion(ref mat_DeviceLocationRegion, ref img_SelectRegion, ref rectLabel, DeviceLocationParameter.m_nMinWidthDevice, DeviceLocationParameter.m_nMinHeightDevice);
        //    LogMessage.WriteToDebugViewer(1, "Select region time: " + timeIns.ElapsedMilliseconds.ToString());
        //    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
        //    {
        //        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Select region time: " + timeIns.ElapsedMilliseconds.ToString());

        //    });

        //    timeIns.Restart();
        //    if (rectLabel == null)
        //        return -99;

        //    MagnusOpenCVLib.DilationRectangle(ref img_SelectRegion, ref img_DilationRegion, DeviceLocationParameter.m_nDilationMask, DeviceLocationParameter.m_nDilationMask);
        //    System.Drawing.Rectangle rectangleRoi = new System.Drawing.Rectangle();
        //    Image<Gray, Byte> ImageAfterDilationCrop = new Image<Gray, Byte>(imgSource.Bitmap);
        //    Image<Gray, Byte> Img = new Image<Gray, Byte>(imgSource.Bitmap);

        //    //LogMessage.WriteToDebugViewer(1, "DilationRectangle time: " + timeIns.ElapsedMilliseconds.ToString());
        //    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
        //    {
        //        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("DilationRectangle time: " + timeIns.ElapsedMilliseconds.ToString());

        //    });
        //    timeIns.Restart();

        //    MagnusOpenCVLib.CropImage(ref ImageAfterDilationCrop, ref Img, region_SearchDeviceLocation, ref rectangleRoi);


        //    System.Drawing.Rectangle rectMatchingPosition = new System.Drawing.Rectangle();
        //    bool bIsTemplateFounded = false;
        //    //LogMessage.WriteToDebugViewer(1, "CropImage time: " + timeIns.ElapsedMilliseconds.ToString());
        //    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
        //    {
        //        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("CropImage time: " + timeIns.ElapsedMilliseconds.ToString());

        //    });
        //    timeIns.Restart();


        //    List<Point> pPolygon = new List<Point>();
        //    pCenter = new Point((rectMatchingPosition.Left + rectMatchingPosition.Right) / 2 + rectangleRoi.Left,
        //                  (rectMatchingPosition.Top + rectMatchingPosition.Bottom) / 2 + rectangleRoi.Top);

        //    //top bottom left right  x1 x2 y1 y2
        //    Point po1 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
        //    Point po2 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
        //    Point po3 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);
        //    Point po4 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);

        //    pPolygon.Add(po1);
        //    pPolygon.Add(po2);
        //    pPolygon.Add(po3);
        //    pPolygon.Add(po4);
        //    pPolygon.Add(pCenter);

        //    List<Point> p_Regionpolygon_temp = RotatePolygon(pPolygon, -nAngleOutput, pCenter.X, pCenter.Y);

        //    int nminIndex = FindNearestPoints(imgSource, ref mat_DeviceLocationRegion, rectMatchingPosition, p_Regionpolygon_temp, (float)nAngleOutput);

        //    nAngleOutput = nAngleOutput + (DeviceLocationParameter.m_nBlackCornerIndexTemplateImage - nminIndex) * 90;
        //    if (nAngleOutput <= -180)
        //        nAngleOutput = 360 - nAngleOutput;

        //    if (nAngleOutput >= 180)
        //        nAngleOutput = nAngleOutput - 360;
        //    p_Regionpolygon = RotatePolygon(pPolygon, -nAngleOutput, pCenter.X, pCenter.Y);
        //    p_Regionpolygon.Remove(pCenter);

        //    //LogMessage.WriteToDebugViewer(1, "FindNearestPoints time: " + timeIns.ElapsedMilliseconds.ToString());
        //    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
        //    {
        //        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("FindNearestPoints time: " + timeIns.ElapsedMilliseconds.ToString());

        //    });
        //    timeIns.Stop();

        //    if (bIsTemplateFounded)
        //        return 0;
        //    else
        //        return -1;

        //}

        public PointF FindNearestPoints_Debug(CvImage imgSourceInput, RotatedRect rotateRect_Device, ref List<ArrayOverLay> list_arrayOverlay, ref List<DefectInfor.DebugInfors> debugInfors, bool bEnableDebug)
        {

            Stopwatch timeIns = new Stopwatch();
            CvPointArray regionPoints = new CvPointArray();

            //RotatedRect rotateRect = new RotatedRect(polygonInput[polygonInput.Count() - 1], new SizeF(rectMatchingPosition.Width - 30, rectMatchingPosition.Height - 30), -fAngleInput);
            CvImage rec_region2 = new CvImage();
            rec_region2 = CvImage.Zeros(imgSourceInput.Height, imgSourceInput.Width, DepthType.Cv8U, 1);
            MagnusOpenCVLib.GenRectangle2(rec_region2, rotateRect_Device, new MCvScalar(255), 1);
            PushBackDebugInfors(imgSourceInput, rec_region2, "Inner region after GenRectangle2. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            AddRegionOverlay(ref list_arrayOverlay, rec_region2, Colors.Yellow);

            CvImage rec_regionChipFillup = new CvImage();
            MagnusOpenCVLib.FillUp(ref rec_region2, ref rec_regionChipFillup);
            PushBackDebugInfors(imgSourceInput, rec_regionChipFillup, "Inner region after FillUp. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);

            ////////////////////////////////
            // Begin Attract corner of chip///

            CvImage fillup_Region = new CvImage();
            CvImage mat_Thresholdregion = new CvImage();
            timeIns.Restart();
            MagnusOpenCVLib.Threshold2(ref imgSourceInput, ref mat_Thresholdregion, m_DeviceLocationParameter.m_L_lowerThresholdInnerChip, m_DeviceLocationParameter.m_L_upperThresholdInnerChip);
            CvInvoke.BitwiseAnd(mat_Thresholdregion, rec_regionChipFillup, mat_Thresholdregion);
            PushBackDebugInfors(imgSourceInput, mat_Thresholdregion, "Inner rect Threshold region . (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage mat_ThresholdFillupRegion = new CvImage();
            MagnusOpenCVLib.FillUp(ref mat_Thresholdregion, ref mat_ThresholdFillupRegion);
            PushBackDebugInfors(imgSourceInput, mat_ThresholdFillupRegion, "Inner Threshold Region after FillUp. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage mat_openingRegionTemp = new CvImage();
            MagnusOpenCVLib.OpeningCircle(ref mat_ThresholdFillupRegion, ref mat_openingRegionTemp, (int)(m_DeviceLocationParameter.m_nOpeningMask * m_DeviceLocationParameter.m_dScaleImageRatio) + 1);
            PushBackDebugInfors(imgSourceInput, mat_openingRegionTemp, "Inner Region after OpeningCircle. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage mat_BiggestRegion = new CvImage();
            MagnusOpenCVLib.SelectBiggestRegion(ref mat_openingRegionTemp, ref mat_BiggestRegion);
            PushBackDebugInfors(imgSourceInput, mat_BiggestRegion, "Inner Region after SelectBiggestRegion. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvInvoke.FindNonZero(mat_BiggestRegion, regionPoints);
            if (regionPoints.Size == 0)
                return new PointF(0,0);

            AddRegionOverlay(ref list_arrayOverlay, mat_BiggestRegion, Colors.Cyan);



            //Detect Special Chip corner
            CvImage mat_CornerChipRegion = new CvImage();
            CvInvoke.BitwiseXor(rec_regionChipFillup, mat_BiggestRegion, mat_CornerChipRegion);
            PushBackDebugInfors(imgSourceInput, mat_CornerChipRegion, "Corner Chip Region after sub inner region and inner rect. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage rec_regionChipFillup_Offset = new CvImage();
            MagnusOpenCVLib.ErodeRectangle(ref rec_regionChipFillup, ref rec_regionChipFillup_Offset, 5, 5);
            PushBackDebugInfors(imgSourceInput, rec_regionChipFillup_Offset, "Rectangle Chip after offset 5 pixel. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();
            CvInvoke.BitwiseAnd(rec_regionChipFillup_Offset, mat_CornerChipRegion, mat_CornerChipRegion);
            PushBackDebugInfors(imgSourceInput, mat_CornerChipRegion, "Corner Chip Region after intersection with offset rectangle. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage mat_openingCornerRegion = new CvImage();
            MagnusOpenCVLib.OpeningCircle(ref mat_CornerChipRegion, ref mat_openingCornerRegion, (int)(m_DeviceLocationParameter.m_nOpeningMask * m_DeviceLocationParameter.m_dScaleImageRatio/ 2 + 1));
            PushBackDebugInfors(imgSourceInput, mat_openingCornerRegion, "Corner Chip Region after OpeningCircle. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();

            CvImage mat_CornerBiggestRegion = new CvImage();
            MagnusOpenCVLib.SelectBiggestRegion(ref mat_openingCornerRegion, ref mat_CornerBiggestRegion);
            PushBackDebugInfors(imgSourceInput, mat_CornerBiggestRegion, "Corner Chip Region after SelectBiggestRegion. (" + timeIns.ElapsedMilliseconds.ToString() + " ms)", bEnableDebug, ref debugInfors);
            timeIns.Restart();



            CvInvoke.FindNonZero(mat_CornerBiggestRegion, regionPoints);
            if (regionPoints.Size == 0)
                return new PointF(0, 0);
            AddRegionOverlay(ref list_arrayOverlay, mat_CornerBiggestRegion, Colors.Blue);

            System.Drawing.Rectangle rect_temp = CvInvoke.BoundingRectangle(regionPoints);
            Point Center_Point = new Point(rect_temp.Left + rect_temp.Width / 2, rect_temp.Top + rect_temp.Height / 2);
            PointF[] points = rotateRect_Device.GetVertices();
            double minDistance = 9999999;
            int nminIndex = -1;
            for (int n = 0; n < rotateRect_Device.GetVertices().Count(); n++)
            {
                double distance_Square = (points[n].X - Center_Point.X) * (points[n].X - Center_Point.X) + (points[n].Y - Center_Point.Y) * (points[n].Y - Center_Point.Y);
                if (distance_Square < minDistance)
                {
                    nminIndex = n;
                    minDistance = distance_Square;
                }
            }

            return points[nminIndex];
        }


        public void LabelMarking_Inspection(CvImage imgSourceInput, RotatedRect rotateRect_Device, ref int nResultOutput)
        {

        }
        public int Calibration_Get3Points(CvImage imgSourceInput, out PointF[] points, ref List<ArrayOverLay> list_arrayOverlay)
        {
            points = new PointF[4];
            CvImage thresholdImage = new CvImage();
            MagnusOpenCVLib.Threshold2(ref imgSourceInput, ref thresholdImage, m_DeviceLocationParameter.m_L_lowerThreshold, m_DeviceLocationParameter.m_L_upperThreshold);

            CvImage OpeningImage = new CvImage();

            MagnusOpenCVLib.OpeningCircle(ref thresholdImage, ref OpeningImage, 1);
            CvImage BiggestRegion = new CvImage();

            MagnusOpenCVLib.SelectBiggestRegion(ref OpeningImage, ref BiggestRegion);
            CvPointArray point_regions = new CvPointArray();

            CvInvoke.FindNonZero(BiggestRegion, point_regions);
            if (point_regions.Size == 0)
                return -1;

            CvContourArray contours = new CvContourArray();
            MagnusOpenCVLib.GenContourRegion(ref BiggestRegion, ref contours, RetrType.External);
            RotatedRect rotateRect_Device = CvInvoke.MinAreaRect(contours[0]);
            //if (rotateRect_Device.Size.Width < (int)(m_DeviceLocationParameter.m_nMinWidthDevice)
            //    || rotateRect_Device.Size.Height < (int)(m_DeviceLocationParameter.m_nMinHeightDevice))
            //    return -(int)ERROR_CODE.NO_PATTERN_FOUND;

            points = rotateRect_Device.GetVertices();



            CvImage rec_region2 = new CvImage();
            rec_region2 = CvImage.Zeros(imgSourceInput.Height, imgSourceInput.Width, DepthType.Cv8U, 1);

            MagnusOpenCVLib.GenRectangle2(rec_region2, rotateRect_Device, new MCvScalar(255), 1);
            CvImage rec_regionFillup = new CvImage();

            MagnusOpenCVLib.FillUp(ref rec_region2, ref  rec_regionFillup);

            AddRegionOverlay(ref list_arrayOverlay, rec_regionFillup, Colors.YellowGreen);
            return 0;
        }
        //public static int FindNearestPoints(CvImage imgSourceInput, ref CvImage deviceLocationThresholdRegion, System.Drawing.Rectangle rectMatchingPosition, List<Point> polygonInput, float fAngleInput)
        //{

        //    RotatedRect rotateRect = new RotatedRect(polygonInput[polygonInput.Count() - 1], new SizeF(rectMatchingPosition.Width - 30, rectMatchingPosition.Height - 30), -fAngleInput);
        //    CvImage rec_region2 = new CvImage();
        //    rec_region2 = CvImage.Zeros(imgSourceInput.Height, imgSourceInput.Width, DepthType.Cv8U, 1);
        //    MagnusOpenCVLib.GenRectangle2(rec_region2, rotateRect, new MCvScalar(255), 1);

        //    CvImage rec_regionFillup = new CvImage();
        //    MagnusOpenCVLib.FillUp(ref rec_region2, ref rec_regionFillup);

        //    CvImage fillup_Region = new CvImage();
        //    MagnusOpenCVLib.FillUp(ref deviceLocationThresholdRegion, ref fillup_Region);

        //    CvImage And_Region = new CvImage();

        //    //MagnusOpenCVLib.Different(rec_regionFillup, fillup_Region, ref Difference_Region);
        //    CvImage XOR_Region = new CvImage();
        //    CvInvoke.BitwiseXor(rec_regionFillup, fillup_Region, XOR_Region);
        //    CvInvoke.BitwiseAnd(XOR_Region, rec_regionFillup, And_Region);

        //    //CvInvoke.BitwiseXor(rec_regionFillup, fillup_Region, XOR_Region);
        //    //CvInvoke.BitwiseAnd(XOR_Region, rec_regionFillup, Difference_Region);



        //    CvImage opening_Region2 = new CvImage();
        //    MagnusOpenCVLib.OpeningCircle(ref And_Region, ref opening_Region2, 5);

        //    MagnusOpenCVLib.SelectBiggestRegion(ref opening_Region2, ref deviceLocationThresholdRegion);
        //    CvPointArray regionPoints = new CvPointArray();
        //    CvInvoke.FindNonZero(deviceLocationThresholdRegion, regionPoints);
        //    System.Drawing.Rectangle rect_temp = CvInvoke.BoundingRectangle(regionPoints);
        //    Point Center_Point = new Point(rect_temp.Left + rect_temp.Width / 2, rect_temp.Top + rect_temp.Height / 2);
        //    double minDistance = 9999999;
        //    int nminIndex = -1;
        //    for (int n = 0; n < polygonInput.Count() - 1; n++)
        //    {
        //        double distance_Square = (polygonInput[n].X - Center_Point.X) * (polygonInput[n].X - Center_Point.X) + (polygonInput[n].Y - Center_Point.Y) * (polygonInput[n].Y - Center_Point.Y);
        //        if (distance_Square < minDistance)
        //        {
        //            nminIndex = n;
        //            minDistance = distance_Square;
        //        }
        //    }

        //    return nminIndex;
        //}


        public int AutoTeachDatumLocation(ref List<Point> p_Regionpolygon, Rectangles rectDeviceLocationInput, Rectangles rectTemplateInput, ref Mat mat_DeviceLocationRegion, ref System.Drawing.Rectangle rectMatchingPosition, ref double nAngleOutput, ref double dScoreOutput, ref int nCornerIndex)
        {

            //if (m_TeachImage.Gray == null)
            //    return -99;

            //CvImage img_thresholdRegion = new CvImage();
            //CvImage img_openingRegionRegion = new CvImage();
            //CvImage img_BiggestRegion = new CvImage();
            //CvImage img_SelectRegion = new CvImage();
            //CvImage img_DilationRegion = new CvImage();

            //CvImage img_Crop = new CvImage();
            //CvImage region_Crop = new CvImage();
            //mat_DeviceLocationRegion = new CvImage();


            //Image<Gray, Byte> imgCropped = new Image<Gray, byte>(m_TeachImage.Gray.Bitmap);
            //System.Drawing.Rectangle rec = new System.Drawing.Rectangle();
            //CvImage templateImageCropped = new CvImage();
            //rec.X = (int)rectTemplateInput.TopLeft.X;
            //rec.Y = (int)rectTemplateInput.TopLeft.Y;
            //rec.Width = (int)rectTemplateInput.Width;
            //rec.Height = (int)rectTemplateInput.Height;
            //imgCropped.ROI = rec;
            //templateImageCropped = imgCropped.Mat.Clone();
            ////CvInvoke.Imshow("Template Image", templateImageCropped);
            ////CvInvoke.WaitKey();


            //System.Drawing.Rectangle rectDeviceLocation = new System.Drawing.Rectangle((int)rectDeviceLocationInput.TopLeft.X,
            //                                                                            (int)rectDeviceLocationInput.TopLeft.Y,
            //                                                                            (int)rectDeviceLocationInput.Width,
            //                                                                           (int)rectDeviceLocationInput.Height);
            ////Image<Gray, Byte> Image_Source_Crop_Temp = new Image<Gray, Byte>(imgSource.Bitmap);
            //CvImage region_SearchDeviceLocation = new CvImage();
            //region_SearchDeviceLocation = CvImage.Zeros(m_TeachImage.Gray.Height, m_TeachImage.Gray.Width, DepthType.Cv8U, 1);
            //CvInvoke.Rectangle(region_SearchDeviceLocation, rectDeviceLocation, new MCvScalar(255), -1);
            //MagnusOpenCVLib.Threshold2(ref m_TeachImage.Gray, ref img_thresholdRegion, m_DeviceLocationParameter.m_L_lowerThreshold, m_DeviceLocationParameter.m_L_upperThreshold);
            //CvInvoke.BitwiseAnd(img_thresholdRegion, region_SearchDeviceLocation, img_thresholdRegion);
            //MagnusOpenCVLib.OpeningRectangle(ref img_thresholdRegion, ref img_openingRegionRegion, m_DeviceLocationParameter.m_nOpeningMask, m_DeviceLocationParameter.m_nOpeningMask);
            //MagnusOpenCVLib.SelectBiggestRegion(ref img_openingRegionRegion, ref mat_DeviceLocationRegion);
            //List<System.Drawing.Rectangle> rectLabel = new List<System.Drawing.Rectangle>();

            //MagnusOpenCVLib.SelectRegion(ref mat_DeviceLocationRegion, ref img_SelectRegion, ref rectLabel, m_DeviceLocationParameter.m_nMinWidthDevice, m_DeviceLocationParameter.m_nMinHeightDevice);
            //if (rectLabel == null)
            //    return -99;

            //MagnusOpenCVLib.DilationRectangle(ref img_SelectRegion, ref img_DilationRegion, m_DeviceLocationParameter.m_nDilationMask, m_DeviceLocationParameter.m_nDilationMask);
            //System.Drawing.Rectangle rectangleRoi = new System.Drawing.Rectangle();
            //Image<Gray, Byte> ImageAfterDilationCrop = new Image<Gray, Byte>(m_TeachImage.Gray.Bitmap);
            //Image<Gray, Byte> Img = new Image<Gray, Byte>(m_TeachImage.Gray.Bitmap);


            //int nWidth = 0, nHeight = 0;
            //MagnusOpenCVLib.GetWidthHeightRegion(ref img_DilationRegion, ref nWidth, ref nHeight);
            //if (nWidth >= m_TemplateImage.Gray.Width && nHeight >= m_TemplateImage.Gray.Height)
            //    MagnusOpenCVLib.CropImage(ref ImageAfterDilationCrop, ref Img, img_DilationRegion, ref rectangleRoi);
            //else
            //    MagnusOpenCVLib.CropImage(ref ImageAfterDilationCrop, ref Img, region_SearchDeviceLocation, ref rectangleRoi);

            //System.Drawing.Rectangle rectMatchingPosition = new System.Drawing.Rectangle();
            //bool bIsTemplateFounded = false;
            //if (ImageAfterDilationCrop.Width <= m_TemplateImage.Gray.Width || ImageAfterDilationCrop.Height <= m_TemplateImage.Gray.Height)
            //{
            //    if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
            //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(imgSource, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
            //    else
            //    {
            //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(imgSource, m_TemplateImage.Gray, 0, 24, 15, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

            //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(imgSource, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

            //    }
            //}
            //else
            //{
            //if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
            //    bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
            //else
            //{
            ////bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, m_TemplateImage.Gray, 0, 24, 15, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

            ////bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(ImageAfterDilationCrop.Mat, m_TemplateImage.Gray, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dScaleImageRatio, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
            //}
            //}

            //MagnusOpenCVLib.CropImage(ref ImageAfterDilationCrop, ref Img, img_DilationRegion, ref rectangleRoi);

            //bool bIsTemplateFounded = false;
            //if (ImageAfterDilationCrop.Width <= templateImageCropped.Width || ImageAfterDilationCrop.Height <= templateImageCropped.Height)
            //{
            //    if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
            //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(m_TeachImage.Gray, templateImageCropped, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
            //    else
            //    {
            //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(m_TeachImage.Gray, templateImageCropped, 0, 12, 30, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

            //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(m_TeachImage.Gray, templateImageCropped, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

            //    }
            //}
            //else
            //{
            //    if (DeviceLocationParameter.m_dAngleResolutionTemplate >= 30)
            //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, templateImageCropped, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_nStepTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
            //    else
            //    {
            //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_TemplateMatching(ImageAfterDilationCrop.Mat, templateImageCropped, 0, 12, 30, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);

            //        bIsTemplateFounded = m_TemplateMatchingModel.MAgnus_KdTreeTemplateMatching(ImageAfterDilationCrop.Mat, templateImageCropped, DeviceLocationParameter.m_dMinScoreTemplate, DeviceLocationParameter.m_dAngleResolutionTemplate, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput);
            //    }
            //}

            ////List<Point> pPolygon = new List<Point>();
            ////Point pCenter = new Point((rectMatchingPosition.Left + rectMatchingPosition.Right) / 2 + rectangleRoi.Left,
            ////              (rectMatchingPosition.Top + rectMatchingPosition.Bottom) / 2 + rectangleRoi.Top);

            //////top bottom left right  x1 x2 y1 y2
            ////Point po1 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
            ////Point po2 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Top + rectangleRoi.Top);
            ////Point po3 = new Point(rectMatchingPosition.Right + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);
            ////Point po4 = new Point(rectMatchingPosition.Left + rectangleRoi.Left, rectMatchingPosition.Bottom + rectangleRoi.Top);

            ////pPolygon.Add(po1);
            ////pPolygon.Add(po2);
            ////pPolygon.Add(po3);
            ////pPolygon.Add(po4);
            ////pPolygon.Add(pCenter);
            ////List<Point> p_Regionpolygon_temp = RotatePolygon(pPolygon, -nAngleOutput, pCenter.X, pCenter.Y);
            //FindNearestPoints_Debug(m_TeachImage.Gray, ref mat_DeviceLocationRegion, rotateRect_Device, ref debugInfors, bEnableDebug);

            //nCornerIndex = FindNearestPoints_Debug(m_TeachImage.Gray, ref mat_DeviceLocationRegion, rectMatchingPosition, p_Regionpolygon_temp, -(float)nAngleOutput, null, false);
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
    }
}
