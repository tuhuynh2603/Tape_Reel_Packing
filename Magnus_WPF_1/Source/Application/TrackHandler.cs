﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.WPF;
using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Define;
using Magnus_WPF_1.Source.Hardware;
using Magnus_WPF_1.UI.UserControls.View;
using Magnus_WPF_1.UI.UserControls;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Magnus_WPF_1.Source.Application
{
    using Magnus_WPF_1.Source.Hardware.SDKHrobot;
    using Magnus_WPF_1.Source.LogMessage;
    using System.Numerics;
    public class Track
    {
        private MainWindow mainWindow;
        public InspectionCore m_InspectionCore;
        public static int m_Width = 3840;
        public static int m_Height = 2748;
        public ImageView[] m_imageViews;
        public int[] m_nResult;
        public int m_nTrackID;
        public HIKControlCameraView hIKControlCameraView;
        public string m_strSeriCamera = "";
        Mat m_frame = new Mat();
        public VideoCapture m_cap;
        public Thread threadInspectOnline;

        public List<DefectInfor.DebugInfors> m_StepDebugInfors;
        public List<ArrayOverLay> m_ArrayOverLay;
        public Track(int indexTrack, int numdoc, string serieCam, MainWindow app)
        {
            m_StepDebugInfors = new List<DefectInfor.DebugInfors>();
            m_ArrayOverLay = new List<ArrayOverLay>();
            m_nTrackID = indexTrack;
            mainWindow = app;
            m_imageViews = new ImageView[numdoc];
            m_nResult = new int[10000];


            Application.LoadCamSetting(indexTrack);
            if (serieCam != "none")
                hIKControlCameraView = new HIKControlCameraView(serieCam, indexTrack);
            m_strSeriCamera = serieCam;
            //m_cap = new VideoCapture(0);
            //m_cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, m_Width);
            //m_cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, m_Height);
            //m_cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, 10);
            //m_cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autofocus, 1);
            //m_cap.SetCaptureProperty(CapProp.Focus, 65); // set the focus to the specified value

            int dpi = 96;
            m_imageViews[0] = new ImageView(m_Width, m_Height, dpi, indexTrack, 1);
            m_imageViews[0].bufferImage = new byte[m_Width * m_Height * 3];
            m_imageViews[0]._imageWidth = m_Width;
            m_imageViews[0]._imageHeight = m_Height;
            m_imageViews[0]._dpi = dpi;
            m_imageViews[0].SetBackgroundDoc(indexTrack);

            TransformImage transformImage = new TransformImage(m_imageViews[0].grd_Dock);
            m_imageViews[0].transform = transformImage;
            m_imageViews[0].docID = 0;
            m_imageViews[0].trackID = indexTrack;
            m_imageViews[0].dockPaneID = 0;
            m_imageViews[0].visibleRGB = /*indexTrack == 0 ?*/ Visibility.Visible /*: Visibility.Collapsed*/;
            System.Drawing.Size size = new System.Drawing.Size(m_Width, m_Height);
            m_InspectionCore = new InspectionCore(ref size);
            //InspectionCore.Initialize();

            CheckInspectionOnlineThread();

        }

        private void Video_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Array.Clear(m_imageViews[0].bufferImage, 0, m_imageViews[0].bufferImage.Length);

                m_cap.SetCaptureProperty(CapProp.Autofocus, 0);
                //m_cap.SetCaptureProperty(CapProp.Focus, InspectionCore.DeviceLocationParameter.m_nStepTemplate);
                m_cap.Retrieve(m_frame);
                Image<Bgr, byte> imgg = m_frame.ToImage<Bgr, byte>();
                m_imageViews[0].bufferImage = BitmapToByteArray(imgg.ToBitmap());
                m_imageViews[0].UpdateNewImageColor(m_imageViews[0].bufferImage, imgg.ToBitmap().Width, imgg.ToBitmap().Height, 96);
                CvInvoke.WaitKey(10);
            }
            catch
            {

            }
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {

            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }

        }

        //public int Snap()
        //{
        //    m_cap.ImageGrabbed += Video_ImageGrabbed;

        //    m_cap.Start();
        //    if (MainWindow.mainWindow == null)
        //        return -1;

        //    while (MainWindow.mainWindow.bEnableGrabCycle)
        //    {
        //        if (MainWindow.mainWindow == null)
        //            return -1;
        //    }
        //    m_cap.Stop();
        //    m_cap.ImageGrabbed -= Video_ImageGrabbed;

        //    //m_cap.Dispose();
        //    return 0;
        //}

        public int Stream_HIKCamera()
        {
            if (!hIKControlCameraView.m_MyCamera.MV_CC_IsDeviceConnected_NET())
                hIKControlCameraView.InitializeCamera(m_strSeriCamera);

            int nRet = hIKControlCameraView.m_MyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                hIKControlCameraView.m_bGrabbing = false;
                return 0;
            }
            while (MainWindow.mainWindow.bEnableGrabCycle)
            {
                if (MainWindow.mainWindow == null)
                    return -1;

                int nWidth = 0, nHeight = 0;

                nRet = hIKControlCameraView.m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                if (MyCamera.MV_OK != nRet)
                {
                    //OutputDe("Trigger Software Fail!", nRet);
                    nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
                    return 0;
                }

                hIKControlCameraView.CaptureAndGetImageBuffer(ref m_imageViews[0].bufferImage, ref nWidth, ref nHeight);
                m_imageViews[0].UpdateSourceImageMono();
                //m_imageViews[0].UpdateNewImageColor(m_imageViews[0].bufferImage, nWidth, nHeight, 96);
                if (MyCamera.MV_OK != nRet)
                {
                    hIKControlCameraView.m_bGrabbing = false;
                    nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
                    return 0;
                }
            }

            nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
            return 0;
        }


        public int SingleSnap_HIKCamera()
        {
            if (!hIKControlCameraView.m_MyCamera.MV_CC_IsDeviceConnected_NET())
                hIKControlCameraView.InitializeCamera(m_strSeriCamera);

            int nRet = hIKControlCameraView.m_MyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                hIKControlCameraView.m_bGrabbing = false;
                return 0;
            }

            int nWidth = 0, nHeight = 0;
            nRet = hIKControlCameraView.m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (MyCamera.MV_OK != nRet)
            {
                //OutputDe("Trigger Software Fail!", nRet);
                nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
                return 0;
            }
            hIKControlCameraView.CaptureAndGetImageBuffer(ref m_imageViews[0].bufferImage, ref nWidth, ref nHeight);
            m_imageViews[0].UpdateSourceImageMono();
            // m_imageViews[0].UpdateNewImageColor(m_imageViews[0].bufferImage, nWidth, nHeight, 96);
            nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                hIKControlCameraView.m_bGrabbing = false;
                return 0;
            }
            return 0;

        }
        //public int SingleSnap()
        //{
        //    m_cap.ImageGrabbed += SingleOffline_ImageGrabbed;

        //    m_cap.Start();
        //    if (MainWindow.mainWindow == null)
        //        return -1;
        //    while (MainWindow.mainWindow.bEnableSingleSnapImages == false)
        //    {
        //        if (MainWindow.mainWindow == null)
        //            return -1;
        //    }
        //    m_cap.Stop();
        //    m_cap.ImageGrabbed -= SingleOffline_ImageGrabbed;

        //    return 0;
        //}


        #region Calculate XYZ Shift and Transform Matrix between robot and camera

        public class MagnusMatrix
        {
            private float[,] data;

            public MagnusMatrix(int rows, int cols)
            {
                data = new float[rows, cols];
            }

            public float this[int i, int j]
            {
                get { return data[i, j]; }
                set { data[i, j] = value; }
            }

            public int Rows => data.GetLength(0);
            public int Cols => data.GetLength(1);


            static PointF[] AddHomogeneousCoordinate(PointF[] points)
            {
                PointF[] homogeneousPoints = new PointF[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    homogeneousPoints[i] = new PointF(points[i].X, points[i].Y);
                }
                return homogeneousPoints;
            }

            public static Mat CalculateTransformMatrix(PointF[] srcPnt, PointF[] dstPnt)
            {
                //float x1c = pC1.X;
                //float y1c = pC1.Y;
                //float x2c = pC2.X;
                //float y2c = pC2.Y;
                //float x1r = pR1.X;
                //float y1r = pR1.Y;
                //float x2r = pR2.X;
                //float y2r = pR2.Y;

                //MagnusMatrix A = new MagnusMatrix(6, 6);
                //A[0, 0] = x1c; A[0, 1] = y1c; A[0, 2] = 1; A[1, 3] = x1c; A[1, 4] = y1c; A[1, 5] = 1;
                //A[2, 0] = x2c; A[2, 1] = y2c; A[3, 3] = x2c; A[3, 4] = y2c;

                //float[] B = { x1r, x2r, y1r, y2r, 0, 0 };

                //float[] x = GaussElimination(A, B);
                //PointF[] srcPnt = new PointF[3];
                //srcPnt[0] = pC1;
                //srcPnt[1] = pC2;
                //srcPnt[2] = pC3;
                PointF[] srcPointsHomogeneous = AddHomogeneousCoordinate(srcPnt);

                //PointF[] dstPnt = new PointF[3];
                //dstPnt[0] = pR1;
                //dstPnt[1] = pR2;
                //srcPnt[2] = pC3;
                PointF[] dstPointsHomogeneous = AddHomogeneousCoordinate(dstPnt);
                Mat mat = new Mat(3, 3, DepthType.Cv32F, 1);
                mat.SetTo(new MCvScalar(10.5f));
                mat = CvInvoke.GetAffineTransform(srcPointsHomogeneous, dstPointsHomogeneous);
                return mat;
            }

            public static PointF ApplyTransformation(Mat transformMatrix, PointF point)
            {
                //System.Array transf = transformMatrix.GetData();
                //object a = transf.GetValue(0, 0);

                //float value = (float)transf.GetValue(0, 0);
                //Matrix<float> transf = new Matrix<float>(3, 3);
                //transformMatrix.CopyTo(transf);
                float x = GetMatValue(transformMatrix, 0, 0) * point.X + GetMatValue(transformMatrix,0, 1) * point.Y + GetMatValue(transformMatrix, 0, 2);
                float y = GetMatValue(transformMatrix, 1, 0) * point.X + GetMatValue(transformMatrix,1, 1) * point.Y + GetMatValue(transformMatrix, 1, 2);

                return new PointF(x, y);
            }

            static float GetMatValue(Mat mat, int row, int col)
            {
                // Ensure the indices are within bounds
                if (row < 0 || row >= mat.Rows || col < 0 || col >= mat.Cols)
                {
                    throw new ArgumentOutOfRangeException("Invalid row or column indices");
                }

                // Access the value at the specified row and column
                double dva = mat.GetValue(row, col);
                float value = (float)dva;

                return value;
            }


            public static double CalculateShiftXYAngle(PointF pCenter1, PointF pCorner1, PointF pCenter2, PointF pCorner2 )
            {
                double dX1 = pCorner1.X - pCenter1.X;
                double dY1 = pCorner1.Y - pCenter1.Y;
                double dX2 = pCorner2.X - pCenter2.X;
                double dY2 = pCorner2.Y - pCenter2.Y;
                return AngleBetweenVectors(dX1, dY1, dX2, dY2) * RotationDirection(dX1, dY1, dX2, dY2);
            }
            public static double AngleWithXAxis(double xa, double ya)
            {
                // Calculate the angle in radians
                double thetaRad = Math.Atan2(ya, xa);

                // Convert radians to degrees
                double thetaDeg = Math.Round((180 / Math.PI) * thetaRad, 2);

                return thetaDeg;
            }
            public static double AngleBetweenVectors(double xa, double ya, double xb, double yb)
            {
                // Calculate the dot product
                double dotProduct = xa * xb + ya * yb;

                // Calculate the magnitudes of the vectors
                double magnitudeA = Math.Sqrt(xa * xa + ya * ya);
                double magnitudeB = Math.Sqrt(xb * xb + yb * yb);

                // Calculate the angle in radians
                double thetaRad = Math.Acos(dotProduct / (magnitudeA * magnitudeB));

                // Convert radians to degrees
                double thetaDeg = Math.Round((180 / Math.PI) * thetaRad, 2);

                return thetaDeg;
            }
            public static int RotationDirection(double xa, double ya, double xb, double yb)
            {
                // Calculate the cross product
                double crossProduct = xa * yb - xb * ya;

                // Determine the rotation direction
                if (crossProduct > 0)
                {
                    return -1;
                }
                else if (crossProduct < 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }




        #endregion

        public void DrawInspectionResult(ref int nResult, ref PointF pCenter, ref double dAngle)
        {
            Stopwatch timeIns = new Stopwatch();

            timeIns.Restart();

            SolidColorBrush color = new SolidColorBrush(Colors.Yellow);
            foreach (ArrayOverLay overlay in m_ArrayOverLay)
            {
                SolidColorBrush c = new SolidColorBrush(overlay._color);
                m_imageViews[0].DrawRegionOverlay(overlay.mat_Region, c);

            }

            color = new SolidColorBrush(Colors.Yellow);
            m_imageViews[0].DrawStringOverlay("(X, Y, Angle) = (" + pCenter.X.ToString() + ", " + pCenter.Y.ToString() + ", " + ((int)dAngle).ToString() + ")", (int)pCenter.X + 10, (int)pCenter.Y, color, 20);

            if (nResult == -99)
            {
                color = new SolidColorBrush(Colors.Red);
                m_imageViews[0].DrawString("Device not found! ", 10, 10, color, 31);
            }
            else
            {
                if (nResult == -1)
                {
                    color = new SolidColorBrush(Colors.Red);
                    m_imageViews[0].DrawString("Not Good", 10, 10, color, 31);
                    //m_imageViews[0].DrawString("Score: " + ((int)dScoreOutput).ToString(), 10, 35, color, 31);

                }
                else
                {
                    color = new SolidColorBrush(Colors.Green);
                    m_imageViews[0].DrawString(/*m_cap.GetCaptureProperty(CapProp.Focus).ToString() */ "Good", 10, 10, color, 31);
                    color = new SolidColorBrush(Colors.Yellow);
                    //m_imageViews[0].DrawString("Delta: = (" + dShiftX.ToString() + ", " + dShiftY.ToString() + ", " + dDeltaAngle.ToString() + ")", 10, 40, color, 18);

                }

            }

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Draw Overlay time: " + timeIns.ElapsedMilliseconds.ToString(), (int)ERROR_CODE.NO_LABEL);

            });

            LogMessage.WriteToDebugViewer(1, "Draw Overlay time: " + timeIns.ElapsedMilliseconds.ToString());
        }

        public int AutoTeach(ref Track m_track, bool bEnableDisplay = false)
        {

            {
                //Track _track = m_track;

                if (MainWindow.mainWindow.m_bEnableDebug)
                    m_StepDebugInfors.Clear();

                m_ArrayOverLay.Clear();
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    m_imageViews[0].ClearOverlay();
                    m_imageViews[0].ClearText();
                });
                int nResult;
                PointF pCenter = new PointF();
                PointF pCorner = new PointF();

                nResult = m_InspectionCore.Inspect(ref m_InspectionCore.m_TeachImage, ref m_ArrayOverLay, ref pCenter, ref pCorner, ref m_StepDebugInfors, false);
                //Draw Result
                if (nResult == 0)
                {
                    m_InspectionCore.m_DeviceLocationResult.m_dCenterDevicePoint = pCenter;
                    m_InspectionCore.m_DeviceLocationResult.m_dCornerDevicePoint = pCorner;
                    //m_InspectionCore.m_DeviceLocationResult.m_dAngleOxDevice = MagnusMatrix.AngleWithXAxis(pCorner.X - pCenter.X, pCorner.Y - pCenter.Y);
                }



                if (bEnableDisplay)
                {
                    double dDeltaAngle = MagnusMatrix.CalculateShiftXYAngle(pCenter, pCorner, m_InspectionCore.m_DeviceLocationResult.m_dCenterDevicePoint, m_InspectionCore.m_DeviceLocationResult.m_dCornerDevicePoint);

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        DrawInspectionResult(ref nResult, ref pCenter, ref dDeltaAngle);
                    });
                }
                return nResult;
            }
        }

        public int DebugFunction(ref Track m_track)
        {
            PointF pCenter = new PointF();
            PointF pCorner = new PointF();


            int nResult = Inspect(ref m_track, out pCenter, out pCorner);
            double dDeltaAngle = MagnusMatrix.CalculateShiftXYAngle(pCenter, pCorner, m_InspectionCore.m_DeviceLocationResult.m_dCenterDevicePoint, m_InspectionCore.m_DeviceLocationResult.m_dCornerDevicePoint);
            //Todo need to later after adding the calib function to calculate the transform matrix
            PointF robotPoint = MagnusMatrix.ApplyTransformation(MainWindow.mainWindow.master.m_hiWinRobotInterface.m_hiWinRobotUserControl.m_MatCameraRobotTransform, pCenter);
            //Draw Result
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                DrawInspectionResult(ref nResult, ref pCenter, ref dDeltaAngle);
            });


            return nResult;
        }

        public int Inspect(ref Track m_track, out PointF pCenterOut, out PointF pCornerOut)
        {
            //Track _track = m_track;

            if (MainWindow.mainWindow.m_bEnableDebug)
                m_StepDebugInfors.Clear();

            m_ArrayOverLay.Clear();
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                m_imageViews[0].ClearOverlay();
                m_imageViews[0].ClearText();
            });
            int nResult;
            //double nAngleOutput = 0;
            PointF pCenter = new PointF(0, 0);
            PointF pCorner = new PointF(0,0);
            nResult = m_InspectionCore.Inspect(ref m_InspectionCore.m_SourceImage, ref m_ArrayOverLay, ref pCenter, ref pCorner, ref m_StepDebugInfors, MainWindow.mainWindow.m_bEnableDebug);

            pCenterOut = pCenter;
            pCornerOut = pCorner;
            return nResult;
        }

        private void InspectionOnlineThread()
        {
            Master.InspectEvent[m_nTrackID].Reset();
            while (MainWindow.mainWindow != null)
            {
                try
                {
                    Master.InspectEvent[m_nTrackID].Reset();
                    while (!Master.InspectEvent[m_nTrackID].WaitOne(10))
                    {
                        if (MainWindow.mainWindow == null)
                            return;
                        Thread.Sleep(5);
                    }
                    Master.InspectEvent[m_nTrackID].Reset();

                    if (m_CurrentSequenceDeviceID < 0 || m_CurrentSequenceDeviceID >= m_nResult.Length)
                        m_CurrentSequenceDeviceID = 0;

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        m_InspectionCore.LoadImageToInspection(m_imageViews[0].btmSource);

                        if (Application.m_bEnableSavingOnlineImage == true && MainWindow.mainWindow.bEnableRunSequence)
                        {
                            ImageSaveData imageSaveData = new ImageSaveData();
                            imageSaveData.nDeviceID = m_CurrentSequenceDeviceID;
                            imageSaveData.strLotID = m_strCurrentLot;
                            imageSaveData.imageSave = BitmapSourceConvert.ToMat(m_imageViews[0].btmSource).Clone();
                            lock (Master.m_SaveInspectImageQueue)
                            {
                                Master.m_SaveInspectImageQueue.Enqueue(imageSaveData);
                            }
                        }

                    });


                    Stopwatch timeIns = new Stopwatch();
                    timeIns.Start();
                    PointF pCenter, pCorner;
                    m_nResult[m_CurrentSequenceDeviceID] = Inspect(ref mainWindow.master.m_Tracks[m_nTrackID], out pCenter, out pCorner);
                    m_Center_Vision = pCenter;
                    m_dDeltaAngleInspection = MagnusMatrix.CalculateShiftXYAngle(m_Center_Vision, pCorner, m_InspectionCore.m_DeviceLocationResult.m_dCenterDevicePoint, m_InspectionCore.m_DeviceLocationResult.m_dCornerDevicePoint);

                    //MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveToPosition(HiWinRobotInterface.m_DeviceID, 0, dValue);

                    //MainWindow.mainWindow.master.m_hiWinRobotInterface.m_hiWinRobotUserControl.MoveToReadyPosition(HiWinRobotInterface.m_DeviceID);
                    //int Anglesign = RotationDirection(dX1, dY1, dX2, dY2);


                    ////
                    //Master.InspectEvent[m_nTrackID].Reset();
                    Master.InspectDoneEvent[m_nTrackID].Set();


                    timeIns.Stop();
                    Thread.Sleep(5);
                    LogMessage.WriteToDebugViewer(1, "Total inspection time: " + timeIns.ElapsedMilliseconds.ToString());

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        //MainWindow.mainWindow.UpdateDebugInfor();

                        m_imageViews[0].tbl_InspectTime.Text = timeIns.ElapsedMilliseconds.ToString();

                        // Update Statistics
                        if (m_CurrentSequenceDeviceID == 0)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                MainWindow.mainWindow.ResetMappingResult(m_nTrackID);

                            });
                        }

                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            MainWindow.mainWindow.UpdateMappingResult(m_CurrentSequenceDeviceID, m_nResult[m_CurrentSequenceDeviceID]);
                            MainWindow.mainWindow.UpdateStatisticResult(m_nResult[m_CurrentSequenceDeviceID]);
                        });

                    });
                }
                catch (Exception e)
                {
                    LogMessage.WriteToDebugViewer(1, "PROCESS ERROR: " + e.ToString());
                    //Master.InspectDoneEvent[m_nTrackID].Set();

                }
            }


        }


        public int m_CurrentSequenceDeviceID = -1;
        public int m_nCurrentClickMappingID = -1;
        public string m_strCurrentLot;
        public PointF m_Center_Vision = new PointF();
        public double m_dDeltaAngleInspection = 0.0;

        public void InspectOffline(string strFolderPath)
        {

            //if (mainWindow.bEnableOfflineInspection)
            //        return;

            CheckInspectionOnlineThread();

            MainWindow.mainWindow.bEnableOfflineInspection = true;

            DirectoryInfo folder = new DirectoryInfo(strFolderPath);

            // Get a list of items (files and directories) inside the folder
            FileSystemInfo[] items = folder.GetFileSystemInfos();

            // Loop through the items and print their names

            while (MainWindow.mainWindow.bEnableOfflineInspection && !mainWindow.bEnableRunSequence)
            {
                try
                {
                    while (!Master.m_hardwareTriggerSnapEvent[m_nTrackID].WaitOne(10))
                    {
                        if (MainWindow.mainWindow == null)
                            return;

                        if (!MainWindow.mainWindow.bEnableOfflineInspection || MainWindow.mainWindow.bEnableRunSequence)
                            return;

                    }
                    bool bDeviceIDFound = false;
                    foreach (FileSystemInfo item in items)
                    {
                        string strDeviceID = item.Name.Split('.')[0];
                        strDeviceID = strDeviceID.Split('_')[1];
                        int nDeviceID = Int32.Parse(strDeviceID);
                        if (nDeviceID < 0 || nDeviceID >= m_nResult.Length)
                            nDeviceID = 0;
                        if (m_nCurrentClickMappingID != nDeviceID - 1)
                            continue;
                        bDeviceIDFound = true;
                        Array.Clear(m_imageViews[0].bufferImage, 0, m_imageViews[0].bufferImage.Length);
                        // Mono Image
                        m_imageViews[0].UpdateNewImageMono(item.FullName);
                        break;
                    }
                    if (bDeviceIDFound == false)
                        continue;
                    //Color Image
                    //Mat img_temp = new Mat();
                    //img_temp = CvInvoke.Imread(item.FullName, ImreadModes.Color);
                    //Array.Clear(m_imageViews[0].bufferImage, 0, m_imageViews[0].bufferImage.Length);
                    //Image<Bgr, byte> imgg = img_temp.ToImage<Bgr, byte>();
                    //m_imageViews[0].bufferImage = BitmapToByteArray(imgg.ToBitmap());
                    //m_imageViews[0].UpdateNewImageColor(m_imageViews[0].bufferImage, imgg.ToBitmap().Width, imgg.ToBitmap().Height, 96);

                    Master.InspectEvent[m_nTrackID].Set();
                    //m_nResult[m_nCurrentClickMappingID] = Inspect();
                    Master.InspectDoneEvent[m_nTrackID].Reset();
                    while (!Master.InspectDoneEvent[m_nTrackID].WaitOne(10))
                    {
                        if (MainWindow.mainWindow == null)
                            return;

                        if (!MainWindow.mainWindow.bEnableOfflineInspection || MainWindow.mainWindow.bEnableRunSequence)
                            return;
                    }
                    Master.InspectDoneEvent[m_nTrackID].Reset();


                }
                catch (Exception e)
                {
                    LogMessage.WriteToDebugViewer(1, "PROCESS ERROR. Inspection Offline : " + e.ToString());
                }
            }
        }

        public void FullSequenceThread()
        {

            CheckInspectionOnlineThread();

            int nWidth = 0, nHeight = 0;
            //Todo If Reset lot ID, need to create new lot ID and reset current Device ID to 0
            m_CurrentSequenceDeviceID = -1;
            m_strCurrentLot = string.Format("TrayID_{0}{1}{2}_{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
            if (!hIKControlCameraView.m_MyCamera.MV_CC_IsDeviceConnected_NET())
                hIKControlCameraView.InitializeCamera(m_strSeriCamera);

            int nRet = hIKControlCameraView.m_MyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                hIKControlCameraView.m_bGrabbing = false;
                return ;
            }

            //m_cap.ImageGrabbed += Online_ImageGrabbed;
            //m_cap.Start();
            Stopwatch timeIns = new Stopwatch();
            timeIns.Start();
            while (MainWindow.mainWindow.bEnableRunSequence)
            {

                if (MainWindow.mainWindow == null)
                    return;

                while (!Master.m_hardwareTriggerSnapEvent[m_nTrackID].WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return;

                    if (!MainWindow.mainWindow.bEnableRunSequence)
                    {
                        nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
                        return;
                    }

                }
                timeIns.Restart();
                //double dScale = 3;
                //nError = FindDeviceLocation_Zoom(ref m_SourceImage.Gray,
                //                                 ref list_arrayOverlay, ref pCenter, ref nAngleOutput, ref debugInfors, bEnableDebug);

                //Snap camera
                nRet = hIKControlCameraView.m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                if (MyCamera.MV_OK != nRet)
                {
                    nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
                    return;
                }

                hIKControlCameraView.CaptureAndGetImageBuffer(ref m_imageViews[0].bufferImage, ref nWidth, ref nHeight);
                if (MyCamera.MV_OK != nRet)
                {
                    hIKControlCameraView.m_bGrabbing = false;
                    nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
                    return;
                }

                m_imageViews[0].UpdateSourceImageMono();
                // Update Current Device ID
                m_CurrentSequenceDeviceID++;
                if (m_CurrentSequenceDeviceID >= Source.Application.Application.categoriesMappingParam.M_NumberDeviceX * Source.Application.Application.categoriesMappingParam.M_NumberDeviceY)
                    m_CurrentSequenceDeviceID = 0;
                if (m_CurrentSequenceDeviceID == 0)
                {
                    m_strCurrentLot = string.Format("TrayID_{0}{1}{2}_{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
                }

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Capture and update Image time: " + timeIns.ElapsedMilliseconds.ToString(), (int)ERROR_CODE.NO_LABEL);

                });


                // Do Inspection
                Master.InspectDoneEvent[m_nTrackID].Reset();
                Master.InspectEvent[m_nTrackID].Set();
                //bool b = false;
                while (!Master.InspectDoneEvent[m_nTrackID].WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return;

                    if (!MainWindow.mainWindow.bEnableRunSequence)
                    {
                        //m_cap.Stop();
                        //m_cap.ImageGrabbed -= Online_ImageGrabbed;
                        nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
                        return;
                    }
                }

                // Send result to Robot
                //string strResult = m_nResult[m_CurrentSequenceDeviceID].ToString(); 
                //Master.commHIKRobot.CreateAndSendMessageToHIKRobot(SignalFromVision.Vision_Go_Pick, strResult);
                PointF robotPoint = MagnusMatrix.ApplyTransformation(MainWindow.mainWindow.master.m_hiWinRobotInterface.m_hiWinRobotUserControl.m_MatCameraRobotTransform, m_Center_Vision);
                // Move to Pre Pick position
                if(MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PRE_PICK_POSITION(robotPoint, m_dDeltaAngleInspection) !=0)
                    return;

                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;

                // Turn on vaccum
                HWinRobot.set_digital_output(HiWinRobotInterface.m_DeviceID, (int)HiWinRobotInterface.OUTPUT_IOROBOT.ROBOT_AIR_ONOFF, true);

                while (HWinRobot.get_digital_input(HiWinRobotInterface.m_DeviceID, (int)HiWinRobotInterface.INPUT_IOROBOT.AIR_PRESSURESTATUS) == 0)
                {
                    if (!MainWindow.mainWindow.bEnableRunSequence)
                    {
                        //m_cap.Stop();
                        //m_cap.ImageGrabbed -= Online_ImageGrabbed;
                        return;
                    }
                }

                // Move to Pick position (move Down Z motor)
                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PICK_POSITION(robotPoint, m_dDeltaAngleInspection);
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;

                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PRE_PICK_POSITION(robotPoint, m_dDeltaAngleInspection);
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;

                // Move to  Pre Pick position again (move Up Z motor)
                HWinRobot.get_digital_input(HiWinRobotInterface.m_DeviceID, (int)HiWinRobotInterface.INPUT_IOROBOT.AIR_PRESSURESTATUS);
                while (HWinRobot.get_digital_input(HiWinRobotInterface.m_DeviceID, (int)HiWinRobotInterface.INPUT_IOROBOT.PLC_READY) == 0)
                {
                    if (!MainWindow.mainWindow.bEnableRunSequence)
                    {
                        //m_cap.Stop();
                        //m_cap.ImageGrabbed -= Online_ImageGrabbed;
                        return;
                    }
                }

                Master.InspectDoneEvent[m_nTrackID].Reset();
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Full sequence time: " + timeIns.ElapsedMilliseconds.ToString(), (int)ERROR_CODE.NO_LABEL);

                });
                timeIns.Restart();

            }
            nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();

        }

        private void SaveBtmSource(BitmapSource btm, string path)
        {
            Task.Factory.StartNew(() =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    BitmapSource _bitmapImage = btm;

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        BitmapEncoder encoder = new BmpBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)_bitmapImage));
                        encoder.Save(stream);
                        _bitmapImage.Freeze();
                        stream.Dispose();
                        stream.Close();
                    }
                });
            });
        }

        public void ClearOverLay()
        {
            for (int index_doc = 0; index_doc < m_imageViews.Length; index_doc++)
            {
                m_imageViews[index_doc].ClearText();
                m_imageViews[index_doc].ClearOverlay();
            }
        }

        public void SetCameraResolution()
        {
        }


        public int CheckInspectionOnlineThread()
        {
            if (threadInspectOnline == null)
            {
                Master.InspectDoneEvent[m_nTrackID].Reset();
                Master.InspectEvent[m_nTrackID].Reset();
                threadInspectOnline = new System.Threading.Thread(new System.Threading.ThreadStart(() => InspectionOnlineThread()));
                threadInspectOnline.Name = m_nTrackID.ToString();
                threadInspectOnline.SetApartmentState(ApartmentState.STA);
                threadInspectOnline.IsBackground = true;
                threadInspectOnline.Start();
                threadInspectOnline.Priority = ThreadPriority.Normal;
            }
            else if (!threadInspectOnline.IsAlive)
            {
                Master.InspectDoneEvent[m_nTrackID].Reset();
                Master.InspectEvent[m_nTrackID].Reset();
                threadInspectOnline = new System.Threading.Thread(new System.Threading.ThreadStart(() => InspectionOnlineThread()));
                threadInspectOnline.Name = m_nTrackID.ToString();
                threadInspectOnline.SetApartmentState(ApartmentState.STA);
                threadInspectOnline.IsBackground = true;
                threadInspectOnline.Start();
                threadInspectOnline.Priority = ThreadPriority.Normal;
            }

            return 0;
        }

    }


}
