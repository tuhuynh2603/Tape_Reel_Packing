using Emgu.CV;
using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Define;
using Magnus_WPF_1.Source.Hardware.SDKHrobot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Magnus_WPF_1.Source.Application
{
    using Magnus_WPF_1.Source.Hardware;
    using System.Diagnostics;
    using static Magnus_WPF_1.Source.Application.Track;
    using static HiWinRobotInterface;
    public class Master
    {
        //private int width, height, dpi;

        private MainWindow mainWindow;
        public Track[] m_Tracks;
        public int m_nActiveTrack;
        public Application applications = new Application();
        public BarCodeReaderInterface m_BarcodeReader ;
        public TeachParametersUC teachParameter = new TeachParametersUC();
        public MappingSetingUC mappingParameter = new MappingSetingUC();
        public static bool m_bIsTeaching;
        public static AutoResetEvent m_NextStepTeachEvent;
        //public CommLog commLog = new CommLog();

        public static ManualResetEvent[] InspectEvent;
        public static ManualResetEvent[] InspectDoneEvent;
        public static AutoResetEvent[] m_hardwareTriggerSnapEvent;
        public static ManualResetEvent[] VisionReadyEvent;

        // public AutoDeleteImagesDlg m_AutoDeleteImagesDlg = new AutoDeleteImagesDlg();

        //public delegate void DelegateCameraStream();
        //public DelegateCameraStream delegateCameraStream;

        //public delegate void GrabDelegate();
        //public GrabDelegate grabDelegate;
        public Thread thread_RobotSequence;
        public Thread[] thread_InspectSequence;
        public Thread[] thread_StreamCamera;
        public Thread threadInspecOffline;
        public Thread m_TeachThread;
        public Thread[] m_SaveInspectImageThread;
        public static List<ArrayOverLay>[] list_arrayOverlay;
        public static Queue<ImageSaveData> m_SaveInspectImageQueue = new Queue<ImageSaveData>(); // create a queue to hold messages
        public BitmapSource btmSource;
        public HiWinRobotInterface m_hiWinRobotInterface;

        #region Contructor Master
        public Master(MainWindow app)
        {
            mainWindow = app;
            m_NextStepTeachEvent = new AutoResetEvent(false);
            m_bIsTeaching = false;
            Application.CheckRegistry();
            Application.LoadRegistry();
            ContructorDocComponent();
            m_BarcodeReader = new BarCodeReaderInterface();

            LoadRecipe();
            m_hiWinRobotInterface = new HiWinRobotInterface();

            m_nActiveTrack = 0;

        }
        public void DeleteMaster()
        {
            for (int nTrack = 0; nTrack < Application.m_nTrack; nTrack++)
            {
                //m_Tracks[nTrack].m_cap.Dispose();
                InspectEvent[nTrack].Set();
                InspectEvent[nTrack].Dispose();
                InspectDoneEvent[nTrack].Set();
                InspectDoneEvent[nTrack].Dispose();
                VisionReadyEvent[nTrack].Dispose();
            }
        }
        public void LoadRecipe(bool isLoadRecipeManualy = false)
        {
            Application.dictMappingParam.Clear();
            Application.LoadMappingParamFromFile();
            mappingParameter.LoadMappingParamFromDictToUI(Application.dictMappingParam);

            #region Load Teach Paramter
            for(int nTrack = 0; nTrack < Application.m_nTrack; nTrack++)
            {
                Application.dictTeachParam.Clear();
                Application.LoadTeachParamFromFileToDict(ref nTrack);

                //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
                teachParameter.UpdateTeachParamFromDictToUI(Application.dictTeachParam);

                m_Tracks[nTrack].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
                m_Tracks[nTrack].AutoTeach(ref m_Tracks[nTrack]);
            }
            #endregion

        }
        #endregion

        #region ContructorDocComponent


        private void ContructorDocComponent()
        {
            m_Tracks = new Track[Application.m_nTrack];
            InspectEvent = new ManualResetEvent[Application.m_nTrack];
            InspectDoneEvent = new ManualResetEvent[Application.m_nTrack];
            VisionReadyEvent = new ManualResetEvent[Application.m_nTrack];
            m_hardwareTriggerSnapEvent = new AutoResetEvent[Application.m_nTrack];
            m_SaveInspectImageThread = new Thread[Application.m_nTrack];
            thread_InspectSequence = new Thread[Application.m_nTrack];
            thread_StreamCamera = new Thread[Application.m_nTrack];
            list_arrayOverlay = new List<ArrayOverLay>[Application.m_nTrack];
            //string[] nSeriCam = { "02C89933333", "none" };
            
            for (int index_track = 0; index_track < Application.m_nTrack; index_track++)
            {
                //for(int index_doc = 0; index_doc < Application.m_nDoc; index_doc++)
                InspectEvent[index_track] = new ManualResetEvent(false);
                InspectDoneEvent[index_track] = new ManualResetEvent(false);
                m_hardwareTriggerSnapEvent[index_track] = new AutoResetEvent(false);
                VisionReadyEvent[index_track] = new ManualResetEvent(false);

                if (m_SaveInspectImageThread[index_track] == null)
                {
                    m_SaveInspectImageThread[index_track] = new System.Threading.Thread(new System.Threading.ThreadStart(() => RunSaveInspectImageThread(index_track)));
                    m_SaveInspectImageThread[index_track].Start();
                }


                list_arrayOverlay[index_track] = new List<ArrayOverLay>();

                m_Tracks[index_track] = new Track(index_track, 1, Application.m_strCameraSerial[index_track], mainWindow);

            }
        }
        #endregion

        public BitmapImage LoadBitmap(string pathFile)
        {
            BitmapImage bitmap = new BitmapImage();
            try
            {
                if (!File.Exists(pathFile))
                    return bitmap;
                bitmap.BeginInit();
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(pathFile);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Image Invalid !!!", "Load Image", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            return bitmap;
        }

        public void loadTeachImageToUI(int nTrack)
        {

            string strImageFilePath = System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_Track" + (nTrack + 1).ToString() +".bmp");
            if (!File.Exists(strImageFilePath))
                return;
            else
            {
                string[] nameImageArray = strImageFilePath.Split('\\');
                int leght = nameImageArray.Count();
                string _nameImage = nameImageArray[leght - 1];
                m_Tracks[nTrack].m_imageViews[0].nameImage = _nameImage;

                //Color Image
                //BitmapImage bitmap = new BitmapImage();
                //bitmap = LoadBitmap(strImageFilePath);
                //m_Tracks[nTrack].m_imageViews[0].UpdateNewImage(bitmap);
                // Mono Image
                m_Tracks[nTrack].m_imageViews[0].UpdateNewImageMono(strImageFilePath);
                m_Tracks[nTrack].m_imageViews[0].GridOverlay.Children.Clear();
                m_Tracks[nTrack].m_imageViews[0].UpdateTextOverlay("", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                mainWindow.UpdateTitleDoc(0, string.Format("Name Image: {0}", " teachImage.bmp"), true);
            }
        }

        public void SaveUITeachImage(int nTrack)
        {
            string strImageFilePath = System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_Track" + (nTrack + 1).ToString() + ".bmp");
            try
            {
                if (File.Exists(strImageFilePath))
                    File.Delete(strImageFilePath);
                BitmapSource _bitmapImage = btmSource;
                string path_image = strImageFilePath;
                using (FileStream stream = new FileStream(path_image, FileMode.CreateNew))
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)_bitmapImage));
                    encoder.Save(stream);
                    _bitmapImage.Freeze();
                    stream.Dispose();
                    stream.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        public void SaveTemplateImage(int nTrackID)
        {
            string pathFileImage = System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "templateImage_Track"+ (nTrackID + 1).ToString() + ".bmp");
            CvInvoke.Imwrite(pathFileImage, m_Tracks[nTrackID].m_InspectionCore.m_TemplateImage.Gray);
        }


        internal void Grab_Image_Thread(bool bSingleSnap = false)
        {
            //for (int index_track = 0; index_track < Application.m_nTrack; index_track++)
            //{
                if (bSingleSnap)
                    //m_Tracks[0].SingleSnap();
                    m_Tracks[0].SingleSnap_HIKCamera();
                else
                    m_Tracks[0].Stream_HIKCamera();
                //m_Tracks[0].Snap();
           // }
        }
        string m_folderPath = @"C:\";
        internal void RunOfflineSequenceThread(int nTrackID)
        {
            if (!mainWindow.bEnableOfflineInspection)
                return;

            if (m_folderPath == @"C:\" || m_folderPath == "")
                m_folderPath = Application.pathImageSave;
            // Set the initial directory for the dialog box
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            folderBrowserDialog.SelectedPath = m_folderPath;

            // Display the dialog box and wait for the user's response
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

            // If the user clicked the OK button, open the selected folder
            if ((int)result == 1)
            {
                // Get the path of the selected folder
                m_folderPath = folderBrowserDialog.SelectedPath;

                // Open the folder using a DirectoryInfo or other appropriate method
                // ...
            }
            else
                return;

            //Master.InspectEvent[0].Set();

            if (thread_InspectSequence[nTrackID] == null)
            {
                thread_InspectSequence[nTrackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrackID].InspectOfflineThread(m_folderPath)));
                thread_InspectSequence[nTrackID].Start();
            }
            else if (!thread_InspectSequence[nTrackID].IsAlive)
            {
                thread_InspectSequence[nTrackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrackID].InspectOfflineThread(m_folderPath)));
                thread_InspectSequence[nTrackID].Start();
            }
        }
        internal void RunOnlineSequenceThread(int nTrack)
        {
            mainWindow.ResetMappingResult(nTrack);
            mainWindow.ResetStatisticResult(nTrack);

            InspectDoneEvent[nTrack].Reset();
            InspectEvent[nTrack].Reset();
            m_hardwareTriggerSnapEvent[nTrack].Reset();

            if (thread_InspectSequence[nTrack] == null)
            {
                thread_InspectSequence[nTrack] = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrack].InspectOnlineThread()));
                thread_InspectSequence[nTrack].Start();
            }
            else if (!thread_InspectSequence[nTrack].IsAlive)
            {
                thread_InspectSequence[nTrack] = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrack].InspectOnlineThread()));
                thread_InspectSequence[nTrack].Start();
            }

        }


        internal void RobotSequenceThread()
        {

            if (thread_RobotSequence == null)
            {
               thread_RobotSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_RobotSequence()));
                thread_RobotSequence.Start();
            }
            else if (!thread_RobotSequence.IsAlive)
            {
                thread_RobotSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_RobotSequence()));
                thread_RobotSequence.Start();
            }
        }

        void func_RobotSequence()
        {
            // Reset All motor

            // Home Move // init OutPut
            if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                return;
            if (MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(HiWinRobotInterface.SequencePointData.READY_POSITION) != 0)
                return;

            if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                return;

            Master.VisionReadyEvent[0].Reset();
            Master.VisionReadyEvent[1].Set();

            m_hardwareTriggerSnapEvent[0].Set();
            //
            Stopwatch timeIns = new Stopwatch();
            timeIns.Start();
            while (MainWindow.mainWindow.bEnableRunSequence || MainWindow.mainWindow.bEnableOfflineInspection)
            {


                if (MainWindow.mainWindow == null)
                    return;

                timeIns.Restart();


                while (!Master.VisionReadyEvent[0].WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return;

                    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                    {
                        return;
                    }
                }
                // Send result to Robot
                //string strResult = m_nResult[m_CurrentSequenceDeviceID].ToString(); 
                //Master.commHIKRobot.CreateAndSendMessageToHIKRobot(SignalFromVision.Vision_Go_Pick, strResult);
                System.Drawing.PointF robotPoint = MagnusMatrix.ApplyTransformation(MainWindow.mainWindow.master.m_hiWinRobotInterface.m_hiWinRobotUserControl.m_MatCameraRobotTransform, m_Tracks[0].m_Center_Vision);
                // Move to Pre Pick position
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PRE_PICK_POSITION(robotPoint, m_Tracks[0].m_dDeltaAngleInspection) != 0)
                    return;

                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;

                // Move to Pick position (move Down Z motor)
                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PICK_POSITION(robotPoint, m_Tracks[0].m_dDeltaAngleInspection);
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;

                // Turn on vaccum
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ONOFF, true);

                if (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.AIR_PRESSURESTATUS) == 0)
                {
                    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                    {
                        //m_cap.Stop();
                        //m_cap.ImageGrabbed -= Online_ImageGrabbed;
                        return;
                    }
                }

                // From now, if Air PressureStatus signal is 0, we consider it as the chip has been through, so we arlamp it 
                // Move to  Pre  position again (move Up Z motor)
                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PRE_PICK_POSITION(robotPoint, m_Tracks[0].m_dDeltaAngleInspection);
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;


                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(HiWinRobotInterface.SequencePointData.READY_POSITION) != 0)
                    return;

                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;

                Master.VisionReadyEvent[0].Reset();
                m_hardwareTriggerSnapEvent[0].Set();

                ///////////////// Barcode Sequence

                int nResult = m_Tracks[0].m_nResult[m_Tracks[0].m_CurrentSequenceDeviceID];


                if (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_READY) == 0)
                {
                    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                    {
                        return;
                    }
                }

                while (!Master.VisionReadyEvent[1].WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return;

                    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                    {
                        return;
                    }
                }


                if (MoveToPassFailedPosition(nResult) < 0)
                    return;


                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(HiWinRobotInterface.SequencePointData.READY_POSITION) != 0)
                    return;

                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                    return;

                Master.VisionReadyEvent[1].Reset();
                m_hardwareTriggerSnapEvent[1].Set();


                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Inspection sequence time: " + timeIns.ElapsedMilliseconds.ToString(), (int)ERROR_CODE.NO_LABEL);

                });
                timeIns.Restart();

            }
        }

        public int MoveToPassFailedPosition(int nResult)
        {
            string strPrePosition = nResult == 0?SequencePointData.PRE_PASS_PLACE_POSITION : SequencePointData.PRE_FAILED_PLACE_POSITION;
            string strPosition = nResult == 0 ? SequencePointData.PASS_PLACE_POSITION : SequencePointData.FAILED_PLACE_POSITION;

            MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(strPrePosition);

            if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                return -1;

            MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(strPosition);

            if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                return -1;

            HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ONOFF, false);

            if (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.AIR_PRESSURESTATUS) == 1)
            {
                if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                {
                return -1;
                }
            }

            MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(strPrePosition);

            if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion())
                return -1;

        return 0;
        }


        internal void RunSaveInspectImageThread(int nTrack)
        {
            while (MainWindow.mainWindow != null)
            {

                ImageSaveData data = new ImageSaveData();
                lock (m_SaveInspectImageQueue)
                {
                    if (m_SaveInspectImageQueue.Count > 0)
                    {
                        data = m_SaveInspectImageQueue.Dequeue();
                    }
                }
                try
                {
                    string strLotIDFolder = Path.Combine(Application.pathImageSave, data.strLotID);
                    if (!Directory.Exists(strLotIDFolder))
                        Directory.CreateDirectory(strLotIDFolder);
                    string path_image = Path.Combine(strLotIDFolder, "Device_" + (data.nDeviceID + 1).ToString() + ".bmp");
                    CvInvoke.Imwrite(path_image, data.imageSave);
                }
                catch (Exception)
                {
                }

                Thread.Sleep(10);

            }
        }

        public void TeachThread()
        {
            if (m_TeachThread == null)
            {
                m_TeachThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[MainWindow.activeImageDock.trackID].m_imageViews[0].TeachSequence(MainWindow.activeImageDock.trackID)));
                m_TeachThread.SetApartmentState(ApartmentState.STA);
            }
            else
            {
                m_TeachThread = null;
                m_TeachThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[MainWindow.activeImageDock.trackID].m_imageViews[0].TeachSequence(MainWindow.activeImageDock.trackID)));
                m_TeachThread.SetApartmentState(ApartmentState.STA);
            }
            m_TeachThread.Start();


        }

        public void UpdateResult()
        {
        }

        public void WriteTeachParam(int nTrack)
        {
            try
            {
                applications.WriteTeachParam(nTrack);
            }
            catch (Exception)
            {
            }
        }

        public void WriteMappingParam()
        {
            try
            {
                applications.WriteMappingParam();
            }
            catch (Exception)
            {
            }
        }

        internal void OpenHiwinRobotDialog(bool bIschecked = false)
        {
            //HiWinRobotInterface.m_hiWinRobotUserControl.Visibility = System.Windows.Visibility.Visible;
            //defectInfor.lvDefect.View = gridView;
            if (bIschecked)
            {
                m_hiWinRobotInterface.dispatcherTimer.Start();

                MainWindow.mainWindow.grd_Defect.Height = m_hiWinRobotInterface.m_hiWinRobotUserControl.Height;
                MainWindow.mainWindow.grd_Defect.Width = m_hiWinRobotInterface.m_hiWinRobotUserControl.Width;

                MainWindow.mainWindow.grd_Defect.Children.Clear();
                MainWindow.mainWindow.grd_Defect.Children.Add(m_hiWinRobotInterface.m_hiWinRobotUserControl);
                //defectInfor.SvDefect.CanContentScroll = true;
                //MainWindow.mainWindow.grd_Defect.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                //MainWindow.mainWindow.grd_Defect.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                MainWindow.mainWindow.grd_Defect_Settings.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                m_hiWinRobotInterface.dispatcherTimer.Stop();

                MainWindow.mainWindow.grd_Defect.Children.Clear();
                MainWindow.mainWindow.grd_Defect_Settings.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }

}
