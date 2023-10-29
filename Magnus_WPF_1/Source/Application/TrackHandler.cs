using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.WPF;
using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Define;
using Magnus_WPF_1.Source.Hardware;
using Magnus_WPF_1.UI.UserControls.View;
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
    public class Track
    {
        private MainWindow mainWindow;

        public static int m_Width = 3840;
        public static int m_Height = 2748;
        public ImageView[] m_imageViews;
        public int[] m_nResult;
        public int m_nTrackID;
        public HIKControlCameraView hIKControlCameraView;
        public VideoCapture m_cap;
        public string m_strSeriCamera = "";
        Mat m_frame = new Mat();
        public Thread threadInspectOnline;


        public Track(int indexTrack, int numdoc, string serieCam, MainWindow app)
        {
            m_nTrackID = indexTrack;
            mainWindow = app;
            m_imageViews = new ImageView[numdoc];
            m_nResult = new int[10000];
            if(serieCam != "none")
                hIKControlCameraView = new HIKControlCameraView(serieCam);
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
            InspectionCore.Initialize();
        }

        private void Video_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Array.Clear(m_imageViews[0].bufferImage, 0, m_imageViews[0].bufferImage.Length);
                m_cap.SetCaptureProperty(CapProp.Autofocus, 0);
                m_cap.SetCaptureProperty(CapProp.Focus, InspectionCore.DeviceLocationParameter.m_nStepTemplate);
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

        private void SingleOffline_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                if (MainWindow.mainWindow.bEnableSingleSnapImages)
                    return;

                MainWindow.mainWindow.bEnableSingleSnapImages = true;
                Array.Clear(m_imageViews[0].bufferImage, 0, m_imageViews[0].bufferImage.Length);
                m_cap.Retrieve(m_frame);
                Image<Bgr, byte> imgg = m_frame.ToImage<Bgr, byte>();
                m_imageViews[0].bufferImage = BitmapToByteArray(imgg.ToBitmap());
                m_imageViews[0].UpdateNewImageColor(m_imageViews[0].bufferImage, imgg.ToBitmap().Width, imgg.ToBitmap().Height, 96);
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    InspectionCore.LoadImageToInspection(m_imageViews[0].btmSource);
                });

                m_nResult[m_nCurrentClickMappingID] = Inspect();

                Master.InspectEvent[m_nTrackID].Reset();
                Master.InspectDoneEvent[m_nTrackID].Set();


            }
            catch
            {
            }
        }

        public int Inspect()
        {
            List<Point> polygon = new List<Point>();
            Point pCenter = new Point();
            Mat mat_output = new Mat();
            double nAngleOutput = 0;
            double dScoreOutput = 0;
            int nResult = InspectionCore.SimpleInspection(ref polygon, ref pCenter, ref mat_output, ref nAngleOutput, ref dScoreOutput);


            //Draw Result
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                m_imageViews[0].ClearOverlay();
                m_imageViews[0].ClearText();
                SolidColorBrush color = new SolidColorBrush(Colors.Yellow);
                m_imageViews[0].DrawPolygonOverlay(ref polygon, color, 1);
                m_imageViews[0].DrawCrossPointOverlay(ref pCenter);
                color = new SolidColorBrush(Colors.Yellow);
                m_imageViews[0].DrawStringOverlay("(X, Y, Angle) = (" + pCenter.X.ToString() + ", " + pCenter.Y.ToString() + ", " + ((int)nAngleOutput).ToString() + ")", pCenter.X + 10, pCenter.Y, color, 20);

                //m_imageViews[0].DrawRegionOverlay(ref mat_output);

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
                        m_imageViews[0].DrawString("Score: " + ((int)dScoreOutput).ToString(), 10, 35, color, 31);

                    }
                    else
                    {
                        color = new SolidColorBrush(Colors.Green);
                        m_imageViews[0].DrawString(/*m_cap.GetCaptureProperty(CapProp.Focus).ToString() */ "Good", 10, 10, color, 31);
                        m_imageViews[0].DrawString("Score: " + ((int)dScoreOutput).ToString(), 10, 35, color, 31);

                    }

                }
            });

            return nResult;
        }

        public void InspectOffline(string strFolderPath)
        {
            try
            {
                if (MainWindow.mainWindow.bEnableOfflineInspection)
                    return;

                mainWindow.bEnableOfflineInspection = true;
                DirectoryInfo folder = new DirectoryInfo(strFolderPath);

                // Get a list of items (files and directories) inside the folder
                FileSystemInfo[] items = folder.GetFileSystemInfos();

                // Loop through the items and print their names
                foreach (FileSystemInfo item in items)
                {

                    while (!Master.InspectEvent[m_nTrackID].WaitOne(100))
                    {
                        if (mainWindow == null)
                            return;
                    }
                    Array.Clear(m_imageViews[0].bufferImage, 0, m_imageViews[0].bufferImage.Length);
                    // Mono Image
                    m_imageViews[0].UpdateNewImageMono(item.FullName);

                    //Color Image
                    //Mat img_temp = new Mat();
                    //img_temp = CvInvoke.Imread(item.FullName, ImreadModes.Color);
                    //Array.Clear(m_imageViews[0].bufferImage, 0, m_imageViews[0].bufferImage.Length);
                    //Image<Bgr, byte> imgg = img_temp.ToImage<Bgr, byte>();
                    //m_imageViews[0].bufferImage = BitmapToByteArray(imgg.ToBitmap());
                    //m_imageViews[0].UpdateNewImageColor(m_imageViews[0].bufferImage, imgg.ToBitmap().Width, imgg.ToBitmap().Height, 96);

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InspectionCore.LoadImageToInspection(m_imageViews[0].btmSource);
                    });


                    //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    //    InspectionCore.LoadImage(m_imageViews[0].btmSource);
                    //});

                    m_nCurrentClickMappingID = 0;
                    m_nResult[m_nCurrentClickMappingID] = Inspect();


                }

            }
            catch
            {
            }
        }


        //private void Online_ImageGrabbed(object sender, EventArgs e)
        //{
        //    try
        //    {

        //        if (MainWindow.mainWindow == null)
        //            return;

        //        if (MainWindow.mainWindow.bEnableRunSequence == false)
        //            return;

        //        if (!Master.InspectEvent[m_nTrackID].WaitOne(1))
        //            return;

        //        Array.Clear(m_imageViews[0].bufferImage, 0, m_imageViews[0].bufferImage.Length);
        //        m_cap.Retrieve(m_frame);
        //        Image<Bgr, byte> imgg = m_frame.ToImage<Bgr, byte>();
        //        m_imageViews[0].bufferImage = BitmapToByteArray(imgg.ToBitmap());
        //        m_imageViews[0].UpdateNewImageColor(m_imageViews[0].bufferImage, imgg.ToBitmap().Width, imgg.ToBitmap().Height, 96);
        //        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
        //        {
        //            InspectionCore.LoadImageToInspection(m_imageViews[0].btmSource);

        //            if (Application.m_bEnableSavingOnlineImage == true)
        //            {
        //                ImageSaveData imageSaveData = new ImageSaveData();
        //                imageSaveData.nDeviceID = m_CurrentSequenceDeviceID;
        //                imageSaveData.strLotID = m_strCurrentLot;
        //                imageSaveData.imageSave = BitmapSourceConvert.ToMat(m_imageViews[0].btmSource).Clone();
        //                lock (Master.m_SaveInspectImageQueue)
        //                {
        //                    Master.m_SaveInspectImageQueue.Enqueue(imageSaveData);
        //                }
        //            }

        //        });

        //        m_nResult[m_CurrentSequenceDeviceID] = Inspect();
        //        ////
        //        Master.InspectEvent[m_nTrackID].Reset();
        //        Master.InspectDoneEvent[m_nTrackID].Set();
        //    }
        //    catch
        //    {
        //    }
        //}

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

        public int Snap_HIKCamera()
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


        private void InspectionOnlineThread()
        {
            try
            {

                if (MainWindow.mainWindow == null)
                    return;

                if (MainWindow.mainWindow.bEnableRunSequence == false)
                    return;

                int nWidth = 0, nHeight = 0;

                while (MainWindow.mainWindow.bEnableRunSequence && MainWindow.mainWindow != null)
                {

                    while (!Master.InspectEvent[m_nTrackID].WaitOne(10))
                    {
                        if (MainWindow.mainWindow == null)
                            return;
                        else
                            if (!MainWindow.mainWindow.bEnableRunSequence)
                            return;
                    }
                    int nRet = hIKControlCameraView.m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                    if (MyCamera.MV_OK != nRet)
                    {
                        //OutputDe("Trigger Software Fail!", nRet);
                        nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
                        return ;
                    }

                    hIKControlCameraView.CaptureAndGetImageBuffer(ref m_imageViews[0].bufferImage, ref nWidth, ref nHeight);
                    m_imageViews[0].UpdateSourceImageMono();
                    //m_imageViews[0].UpdateNewImageColor(m_imageViews[0].bufferImage, nWidth, nHeight, 96);
                    if (MyCamera.MV_OK != nRet)
                    {
                        hIKControlCameraView.m_bGrabbing = false;
                        nRet = hIKControlCameraView.m_MyCamera.MV_CC_StopGrabbing_NET();
                        return ;
                    }

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InspectionCore.LoadImageToInspection(m_imageViews[0].btmSource);

                        if (Application.m_bEnableSavingOnlineImage == true)
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
                    m_nResult[m_CurrentSequenceDeviceID] = Inspect();
                    timeIns.Stop();
                    Thread.Sleep(5);
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        m_imageViews[0].tbl_InspectTime.Text = timeIns.ElapsedMilliseconds.ToString();
                    });

                    ////
                    Master.InspectEvent[m_nTrackID].Reset();
                    Master.InspectDoneEvent[m_nTrackID].Set();



                }

            }
            catch
            {
            }
        }


        public int m_CurrentSequenceDeviceID = -1;
        public int m_nCurrentClickMappingID = -1;
        public string m_strCurrentLot;
        public void FullSequenceThread()
        {

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


            if (threadInspectOnline == null)
            {
                threadInspectOnline = new System.Threading.Thread(new System.Threading.ThreadStart(() => InspectionOnlineThread()));
                threadInspectOnline.Start();
            }
            else if (!threadInspectOnline.IsAlive)
            {
                threadInspectOnline = new System.Threading.Thread(new System.Threading.ThreadStart(() => InspectionOnlineThread()));
                threadInspectOnline.Start();
            }


            //m_cap.ImageGrabbed += Online_ImageGrabbed;
            //m_cap.Start();
            while (MainWindow.mainWindow.bEnableRunSequence)
            {

                //if (MainWindow.mainWindow == null)
                //    return;

                while (!Master.m_hardwareTriggerSnapEvent[m_nTrackID].WaitOne(10))
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
                //Snap camera

                //


                // Update Current Device ID
                m_CurrentSequenceDeviceID++;
                if (m_CurrentSequenceDeviceID >= Source.Application.Application.categoriesMappingParam.M_NumberDeviceX * Source.Application.Application.categoriesMappingParam.M_NumberDeviceY)
                    m_CurrentSequenceDeviceID = 0;
                if (m_CurrentSequenceDeviceID == 0)
                {
                    m_strCurrentLot = string.Format("TrayID_{0}{1}{2}_{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
                }


                // Do Inspection
                Master.InspectEvent[m_nTrackID].Set();
                bool b = false;
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

                // Update Statistics
                if (m_CurrentSequenceDeviceID == 0)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        mainWindow.ResetMappingResult();

                    });
                }

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    mainWindow.UpdateMappingResult(m_CurrentSequenceDeviceID, m_nResult[m_CurrentSequenceDeviceID]);
                    mainWindow.UpdateStatisticResult(m_nResult[m_CurrentSequenceDeviceID]);
                });

            }
            //m_cap.Stop();
            //m_cap.ImageGrabbed -= Online_ImageGrabbed;
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

    }
}
