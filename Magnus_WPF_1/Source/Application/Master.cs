using Emgu.CV;
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
    using System.Windows;
    using static HiWinRobotInterface;
    using static Magnus_WPF_1.Source.Application.Track;
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
        public static AutoResetEvent[] m_OfflineTriggerSnapEvent;
        public int m_bNextStepSequence = 0;

        public static AutoResetEvent m_NextStepSequenceEvent;
        public static AutoResetEvent m_EmergencyStopSequenceEvent;

        public static ManualResetEvent[] VisionReadyEvent;

        // public AutoDeleteImagesDlg m_AutoDeleteImagesDlg = new AutoDeleteImagesDlg();

        //public delegate void DelegateCameraStream();
        //public DelegateCameraStream delegateCameraStream;

        //public delegate void GrabDelegate();
        //public GrabDelegate grabDelegate;
        public Thread thread_RobotSequence;
        public Thread thread_BarcodeReaderSequence;
        public Thread[] thread_InspectSequence;
        public Thread[] thread_StreamCamera;
        public Thread threadInspecOffline;
        public Thread m_TeachThread;
        public Thread[] m_SaveInspectImageThread;


        public Thread m_IOStatusThread;

        public static List<ArrayOverLay>[] list_arrayOverlay;
        public static Queue<ImageSaveData>[] m_SaveInspectImageQueue = new Queue<ImageSaveData>[2]; // create a queue to hold messages
        public HiWinRobotInterface m_hiWinRobotInterface;
        public PLCCOMM m_plcComm;
        #region Contructor Master
        public Master(MainWindow app)
        {
            mainWindow = app;
            m_NextStepTeachEvent = new AutoResetEvent(false);
            m_bIsTeaching = false;

            for(int nTrack = 0; nTrack < Application.m_nTrack; nTrack++)
                m_SaveInspectImageQueue[nTrack] = new Queue<ImageSaveData>();

            Application.CheckRegistry();
            Application.LoadRegistry();
            ContructorDocComponent();
            LogMessage.LogMessage.WriteToDebugViewer(2, "BarCodeReaderInterface");

            m_BarcodeReader = new BarCodeReaderInterface();
            LoadRecipe();
            m_hiWinRobotInterface = new HiWinRobotInterface();
            m_plcComm = new PLCCOMM();
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
            mappingParameter.UpdateMappingParamFromDictToUI(Application.dictMappingParam);

            #region Load Teach Paramter
            for(int nTrack = 0; nTrack < Application.m_nTrack; nTrack++)
            {
                Application.dictTeachParam.Clear();
                Application.LoadTeachParamFromFileToDict(ref nTrack);
                //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
                teachParameter.UpdateTeachParamFromDictToUI(Application.dictTeachParam);
                m_Tracks[nTrack].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();


                for (int nArea = 0; nArea < Application.TOTAL_AREA; nArea++)
                {
                    Application.dictPVIAreaParam[nArea] = new Dictionary<string, string>();

                    Application.LoadAreaParamFromFileToDict(ref nTrack, nArea);
                    teachParameter.UpdateTeachParamFromDictToUI(Application.dictPVIAreaParam[nArea]);
                    m_Tracks[nTrack].m_InspectionCore.UpdateAreaParameterFromUIToInspectionCore(nArea);
                }


                m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
                m_Tracks[nTrack].AutoTeach(ref m_Tracks[nTrack]);
            }
            #endregion

        }
        #endregion

        #region ContructorDocComponent



        public int m_EmergencyStatus = 0;
        public int m_ImidiateStatus = 0;
        public int m_ResetMachineStatus = 0;


        public int m_EmergencyStatus_Simulate = 0;
        public int m_ImidiateStatus_Simulate = 0;
        public int m_ResetMachineStatus_Simulate = 0;
        public bool m_bMachineNotReadyNeedToReset = false;
        public bool m_bNeedToImidiateStop= false;

        private void ContructorDocComponent()
        {
            m_Tracks = new Track[Application.m_nTrack];
            InspectEvent = new ManualResetEvent[Application.m_nTrack];
            InspectDoneEvent = new ManualResetEvent[Application.m_nTrack];
            m_OfflineTriggerSnapEvent = new AutoResetEvent[Application.m_nTrack];
            VisionReadyEvent = new ManualResetEvent[Application.m_nTrack];
            m_hardwareTriggerSnapEvent = new AutoResetEvent[Application.m_nTrack];
            m_NextStepSequenceEvent = new AutoResetEvent(false);
            m_EmergencyStopSequenceEvent = new AutoResetEvent(false);
            m_SaveInspectImageThread = new Thread[Application.m_nTrack];
            thread_InspectSequence = new Thread[Application.m_nTrack];
            thread_StreamCamera = new Thread[Application.m_nTrack];
            list_arrayOverlay = new List<ArrayOverLay>[Application.m_nTrack];
            //string[] nSeriCam = { "02C89933333", "none" };
            if (m_IOStatusThread == null)
            {
                m_IOStatusThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_IOStatusThread()));
                m_IOStatusThread.Start();
            }

            for (int index_track = 0; index_track < Application.m_nTrack; index_track++)
            {
                //for(int index_doc = 0; index_doc < Application.m_nDoc; index_doc++)
                InspectEvent[index_track] = new ManualResetEvent(false);
                InspectDoneEvent[index_track] = new ManualResetEvent(false);
                m_hardwareTriggerSnapEvent[index_track] = new AutoResetEvent(false);
                m_OfflineTriggerSnapEvent[index_track] = new AutoResetEvent(false);

                VisionReadyEvent[index_track] = new ManualResetEvent(false);

                if (m_SaveInspectImageThread[index_track] == null)
                {
                    m_SaveInspectImageThread[index_track] = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_RunSaveInspectImageThread(index_track)));
                    m_SaveInspectImageThread[index_track].Start();
                }


                list_arrayOverlay[index_track] = new List<ArrayOverLay>();

                m_Tracks[index_track] = new Track(index_track, 1, Application.m_strCameraSerial[index_track], mainWindow, Application.m_Width[index_track], Application.m_Height[index_track]);

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
                //m_Tracks[nTrack].m_imageViews[0].UpdateTextOverlay("", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                //mainWindow.UpdateTitleDoc(0, "teachImage.bmp", true);
            }
        }

        public void loadImageFromFileToUI(int nTrack, string strFileName)
        {

            if (!File.Exists(strFileName))
                return;
            else
            {
                string[] nameImageArray = strFileName.Split('\\');
                int leght = nameImageArray.Count();
                string _nameImage = nameImageArray[leght - 1];
                m_Tracks[nTrack].m_imageViews[0].nameImage = _nameImage;

                //Color Image
                //BitmapImage bitmap = new BitmapImage();
                //bitmap = LoadBitmap(strImageFilePath);
                //m_Tracks[nTrack].m_imageViews[0].UpdateNewImage(bitmap);
                // Mono Image
                m_Tracks[nTrack].m_imageViews[0].UpdateNewImageMono(strFileName);
                m_Tracks[nTrack].m_InspectionCore.LoadImageToInspection(m_Tracks[nTrack].m_imageViews[0].btmSource);

                m_Tracks[nTrack].m_imageViews[0].GridOverlay.Children.Clear();
                //m_Tracks[nTrack].m_imageViews[0].UpdateTextOverlay("", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                //mainWindow.UpdateTitleDoc(0, "teachImage.bmp", true);
            }
        }


        public void SaveUIImage(int nTrack)
        {
            string strDateTime = string.Format("({0}.{1}.{2}_{3}.{4}.{5})", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
            string strPath = System.IO.Path.Combine(Source.Application.Application.pathImageSave, "UI Image");
            if (!Directory.Exists(strPath))
                Directory.CreateDirectory(strPath);
            strPath = System.IO.Path.Combine(strPath, $"Track {nTrack + 1}");

            if (!Directory.Exists(strPath))
                Directory.CreateDirectory(strPath);

            string strImageFilePath = System.IO.Path.Combine(strPath, $"{strDateTime}.bmp");
            try
            {

                m_Tracks[nTrack].m_InspectionCore.LoadImageToInspection(m_Tracks[nTrack].m_imageViews[0].btmSource);
                m_Tracks[nTrack].m_InspectionCore.SaveCurrentSourceImage(strImageFilePath, 0);

                m_Tracks[nTrack].m_InspectionCore.SaveCurrentSourceImage(strImageFilePath, 1);
                /*BitmapSource _bitmapImage = m_Tracks[nTrack].m_imageViews[0].btmSource;
                string path_image = strImageFilePath;
                using (FileStream stream = new FileStream(path_image, FileMode.CreateNew))
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)_bitmapImage));
                    encoder.Save(stream);
                    _bitmapImage.Freeze();
                    stream.Dispose();
                    stream.Close();
                }*/
            }
            catch (Exception)
            {
            }
        }

        public void SaveUITeachImage(int nTrack)
        {
            string strImageFilePath = System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_Track" + (nTrack + 1).ToString() + ".bmp");
            try
            {
                if (File.Exists(strImageFilePath))
                    File.Delete(strImageFilePath);
                BitmapSource _bitmapImage = m_Tracks[nTrack].m_imageViews[0]. btmSource;
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


        internal void func_GrabImageThread(bool bSingleSnap = false)
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
                thread_InspectSequence[nTrackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrackID].func_InspectOfflineThread(m_folderPath)));
                thread_InspectSequence[nTrackID].Start();
            }
            else if (!thread_InspectSequence[nTrackID].IsAlive)
            {
                thread_InspectSequence[nTrackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrackID].func_InspectOfflineThread(m_folderPath)));
                thread_InspectSequence[nTrackID].Start();
            }
        }
        internal void RunOnlineSequenceThread(int nTrack)
        {

            InspectDoneEvent[nTrack].Reset();
            InspectEvent[nTrack].Reset();
            m_hardwareTriggerSnapEvent[nTrack].Reset();

            if (thread_InspectSequence[nTrack] == null)
            {
                thread_InspectSequence[nTrack] = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrack].func_InspectOnlineThread()));
                thread_InspectSequence[nTrack].Start();
            }
            else if (!thread_InspectSequence[nTrack].IsAlive)
            {
                thread_InspectSequence[nTrack] = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrack].func_InspectOnlineThread()));
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

        public void func_ResetSequenceThread()
        {
            if (ResetSequence() < 0)
            {
                lock (this)
                {
                    m_bMachineNotReadyNeedToReset = true;
                }
                WaitForNextStepSequenceEvent(" Reset Machine Failed. Please try again!");
            }

        }
        public int ResetSequence(bool bEnableStepMode = true)
        {
            bool bMachineNotReadyNeedToReset = m_bMachineNotReadyNeedToReset;
            if (m_ResetMachineStatus > 0 && m_bMachineNotReadyNeedToReset)
            {
                lock(this)
                {
                    m_bMachineNotReadyNeedToReset = false;
                }
            }
        ResetSequence_Step1:
            int nError = (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE;
            Master.VisionReadyEvent[0].Reset();
            Master.VisionReadyEvent[1].Reset();
            if (HiWinRobotInterface.m_RobotConnectID < 0)
                return -1;
            HWinRobot.clear_alarm(HiWinRobotInterface.m_RobotConnectID);

            // Stop Motor
            m_hiWinRobotInterface.StopMotor();
            MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion();
            nError = WaitForNextStepSequenceEvent("Reset Sequence: Stop motor Done. Press Next to Move to place position");
            if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT) return -1;

            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
            {
                goto ResetSequence_Step1;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
            {
                goto ResetSequence_Step1;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
            {
                goto ResetSequence_Step1;

            }

        ResetSequence_Step2:
            MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(HiWinRobotInterface.SequencePointData.PRE_FAILED_PLACE_POSITION);
            MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion();
            nError = WaitForNextStepSequenceEvent("Reset Sequence: Moving done. Press Next to put the device to position");
            if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT) return -1;
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
            {
                goto ResetSequence_Step2;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
            {
                goto ResetSequence_Step1;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
            {
                goto ResetSequence_Step1;

            }

        ResetSequence_Step3:
            HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
            HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, true);
            Thread.Sleep(500);
            nError = WaitForNextStepSequenceEvent("Reset Sequence: Put device done. Press Next to turn off all the airs");
            if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT) return -1;
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
            {
                goto ResetSequence_Step3;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
            {
                goto ResetSequence_Step2;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
            {
                goto ResetSequence_Step1;

            }

        ResetSequence_Step4:
            HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
            HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, false);
            Thread.Sleep(500);
            nError = WaitForNextStepSequenceEvent("Reset Sequence: Press Next to Move to Ready position");
            if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT) return -1;
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
            {
                goto ResetSequence_Step4;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
            {
                goto ResetSequence_Step3;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
            {
                goto ResetSequence_Step1;

            }

        ResetSequence_Step5:
            MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(HiWinRobotInterface.SequencePointData.READY_POSITION);
            MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion();
            nError = WaitForNextStepSequenceEvent("Reset Sequence: Press Next to Complete Reset Sequence");
            if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT) return -1;
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
            {
                goto ResetSequence_Step5;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
            {
                goto ResetSequence_Step4;
            }
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
            {
                goto ResetSequence_Step1;

            }



            if (m_bMachineNotReadyNeedToReset == true)
            {
                lock (this)
                {
                    m_bMachineNotReadyNeedToReset = false;
                }
            }

            return 0;
        }

        public enum SEQUENCE_OPTION
        {
            SEQUENCE_ABORT = -1,
            SEQUENCE_CONTINUE = 0,
            SEQUENCE_IMIDIATE_BUTTON_CONTINUE = 1,
            SEQUENCE_GOBACK = 2,
            SEQUENCE_RETRY = 3
        }
        public int WaitForNextStepSequenceEvent(string strDebugMessage = "")
        {
            //if (strDebugMessage == "")
            //{
            //    MainWindow.mainWindow.PopupWarningMessageBox(strDebugMessage, false, m_bMachineNotReadyNeedToReset, false);
            //    return (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE;
            //}

            m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE;
            if (m_bMachineNotReadyNeedToReset || m_bNeedToImidiateStop)
            {
                m_EmergencyStopSequenceEvent.Reset();

                MainWindow.mainWindow.PopupWarningMessageBox(strDebugMessage, true, m_bMachineNotReadyNeedToReset);
                if(m_bMachineNotReadyNeedToReset)
                    return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;

                while (!m_EmergencyStopSequenceEvent.WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;
                }
                //Show Dialog
            }
            else if(MainWindow.mainWindow.m_bEnableDebugSequence)
            {
                m_NextStepSequenceEvent.Reset();
                MainWindow.mainWindow.PopupWarningMessageBox(strDebugMessage, false, m_bMachineNotReadyNeedToReset);
                while (!m_NextStepSequenceEvent.WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;

                    //if (!MainWindow.mainWindow.bEnableRunSequence)
                    //    return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;

                    if (!MainWindow.mainWindow.m_bEnableDebugSequence || m_bNeedToImidiateStop || m_bMachineNotReadyNeedToReset)
                        return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;

                }

            }

            MainWindow.mainWindow.PopupWarningMessageBox(strDebugMessage, false, m_bMachineNotReadyNeedToReset, false);

            return m_bNextStepSequence;
        }

        void func_RobotSequence()
        {
        RobotSequence_Step1:

            int nError = 0;
            // Reset All motor

            // Home Move // init OutPut
            if (ResetSequence() < 0)
            {
                lock (this)
                {
                    m_bMachineNotReadyNeedToReset = true;
                }
                nError = WaitForNextStepSequenceEvent(" Reset Machine Failed. End Sequence...");
                return;
            }
            //Wait for Signal from PLC when they ready for new Lot so we can create new lot ID
            while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_READY) == 0)
            {
                // Need popup Dialog if waiting too long 
                if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection || m_bMachineNotReadyNeedToReset)
                    return;
            }

            nError = WaitForNextStepSequenceEvent("Begin Sequence: Press Next to trigger camera 1");

            m_hardwareTriggerSnapEvent[0].Set();

            if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                return;
            else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
            {
                goto RobotSequence_Step1;
            }

            Stopwatch timeIns = new Stopwatch();
            Stopwatch timeIns_fullSequence = new Stopwatch();
            timeIns_fullSequence.Start();
            timeIns.Start();

            //Just for testing 

            //
            int nChipCount = 0;
            while (MainWindow.mainWindow.bEnableRunSequence || MainWindow.mainWindow.bEnableOfflineInspection || m_bMachineNotReadyNeedToReset)
            {

                if (MainWindow.mainWindow == null)
                    return;

                int nStep = 1;
                LogMessage.LogMessage.WriteToDebugViewer(9, "Begin Sequence");
                if(m_plcComm.ReadPLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_RESET_LOT) > 0)
                {
                    m_Tracks[0].m_strCurrentLot = string.Format("TrayID_{0}{1}{2}_{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
                }
                else
                    m_Tracks[0].m_CurrentSequenceDeviceID = m_plcComm.ReadPLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_CURRENT_ROBOT_CHIP_COUNT);

                // Step 1: wait for station 1 inspection
                timeIns.Restart();

                while (!Master.VisionReadyEvent[0].WaitOne(1))
                {
                    if (MainWindow.mainWindow == null)
                        return;

                    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection || m_bMachineNotReadyNeedToReset)
                    {
                        return;
                    }
                }

                int nCamera1InspectionResult = m_Tracks[0].m_nResult[m_Tracks[0].m_CurrentSequenceDeviceID];

                if (nCamera1InspectionResult == -(int)ERROR_CODE.NO_PATTERN_FOUND) // If no pattern found, popup Error 
                {
                    //nError = WaitForNextStepSequenceEvent("Inspection Failed (Device Not found). Press Next to trigger camera 1 again");

                    //if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    //    break;

                    LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Station 1 inspection No Device Found ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                    //Master.VisionReadyEvent[0].Reset();
                    //m_hardwareTriggerSnapEvent[0].Set();
                    HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
                    HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, false);
                    break;
                }

                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Station 1 inspection  ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                nError = WaitForNextStepSequenceEvent("Inspection done. Press Next to Move to  pre pick position");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;

                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }
            RobotSequence_Step2:
                if (m_bMachineNotReadyNeedToReset)
                    return;
                // Step 2: Transform point from camera position to robot position
                System.Drawing.PointF robotPoint = MagnusMatrix.ApplyTransformation(MainWindow.mainWindow.master.m_hiWinRobotInterface.m_hiWinRobotUserControl.m_MatCameraRobotTransform, m_Tracks[0].m_Center_Vision);
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Transform point from camera position to robot position ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();

                //Step 3: Move to Pre Pick position
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PRE_PICK_POSITION(robotPoint, -m_Tracks[0].m_dDeltaAngleInspection) != 0)
                    return;
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)
                    return;
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Move to Pre Pick position ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                nError = WaitForNextStepSequenceEvent("Move to Pre Pick position done. Press Next to turn on vaccum");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;

                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
                {
                    goto RobotSequence_Step2;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
                {
                    goto RobotSequence_Step1;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }

            RobotSequence_Step3:
                if (m_bMachineNotReadyNeedToReset)
                    return;
                //Step 4: Turn on vaccum
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, true);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, false);
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Turn on vaccum ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                nError = WaitForNextStepSequenceEvent("Press Next to Pick the device");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
                {
                    goto RobotSequence_Step3;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
                {
                    goto RobotSequence_Step2;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }

            RobotSequence_Step4:
                if (m_bMachineNotReadyNeedToReset)
                    return;
                // Move to Pick position (move Down Z motor)
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PICK_POSITION(robotPoint, -m_Tracks[0].m_dDeltaAngleInspection) != 0)
                    return;
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)
                    return;
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Move to Pick position (move Down Z motor) ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();

                //Step 5: Move to  Pre pick  position again (move Up Z motor)
                // From now, if Air PressureStatus signal is 0, we consider it as the chip has been through, so we arlamp it
                nError = WaitForNextStepSequenceEvent("Press Next to move up to pre pick position");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
                {
                    goto RobotSequence_Step4;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
                {
                    goto RobotSequence_Step3;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }

            RobotSequence_Step5:

                if (m_bMachineNotReadyNeedToReset)
                    return;
                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PRE_PICK_POSITION(robotPoint, -m_Tracks[0].m_dDeltaAngleInspection);

                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)
                    return;

                //Trigger conveyor on
                    HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_CONVEYER_ONOFF, true);
                //

                while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.AIR_PRESSURESTATUS) == 0)
                {
                    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                    {
                        return;
                    }
                }

                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Move to  Pre pick  position again ({timeIns.ElapsedMilliseconds} ms)");
                nError = WaitForNextStepSequenceEvent("Move to prepick position done. Press Next to move up to pre pick position");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
                {
                    goto RobotSequence_Step5;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
                {
                    goto RobotSequence_Step4;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }
            RobotSequence_Step6:
                if (m_bMachineNotReadyNeedToReset)
                    return;
                //Step 6: Move To Pass fail position
                string strPrePosition = nCamera1InspectionResult == 0 ? SequencePointData.PRE_PASS_PLACE_POSITION : SequencePointData.PRE_FAILED_PLACE_POSITION;
                string strPosition = nCamera1InspectionResult == 0 ? SequencePointData.PASS_PLACE_POSITION : SequencePointData.FAILED_PLACE_POSITION;
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Move To Pass fail position ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();

                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(strPrePosition);

                // Sleep 200ms to make sure the robot outside of camera region
                //Thread.Sleep(200);
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)
                    return;

                //Step 7: Trigger camera  1 

                nError = WaitForNextStepSequenceEvent("Move To Pass fail position Done. Press Next to trigger camera 1");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
                {
                    goto RobotSequence_Step6;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
                {
                    goto RobotSequence_Step5;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }
            RobotSequence_Step7:
                if (m_bMachineNotReadyNeedToReset)
                    return;
                //Trigger conveyor off
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_CONVEYER_ONOFF, false);
                //while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_READY) == 0)
                //{
                //    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                //    {
                //        return;
                //    }
                //}
                //
                Master.VisionReadyEvent[0].Reset();
                m_hardwareTriggerSnapEvent[0].Set();
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Trigger camera  1  ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                nError = WaitForNextStepSequenceEvent("Move to pre place position done. Press Next to wait for plc ready signal");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
                {
                    goto RobotSequence_Step7;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
                {
                    goto RobotSequence_Step6;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }

            RobotSequence_Step8:
                if (m_bMachineNotReadyNeedToReset)
                    return;
                //Step 8: Wait For PLC Ready
                while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_READY) == 0)
                {
                    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                    {
                        return;
                    }
                }

                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Wait For PLC Ready ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                nError = WaitForNextStepSequenceEvent("wait for plc ready signal done. Press Next to Put device to the tray");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
                {
                    goto RobotSequence_Step8;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
                {
                    goto RobotSequence_Step7;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }

            RobotSequence_Step9:
                if (m_bMachineNotReadyNeedToReset)
                    return;
                /////////////////Step 9: put device to the tray (turn off vaccum)
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, true);
                Thread.Sleep(50);
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nStep++} : Put device to the tray ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();

                //if(nEmptyTrayHoles >= 5)
                //    m_hardwareTriggerSnapEvent[1].Set();

                nError = WaitForNextStepSequenceEvent("Put device to the tray Done. Press next to Move to Ready position");

            RobotSequence_Step10:
                if (m_bMachineNotReadyNeedToReset)
                    return;
                // Step 11: Move to ready position and turn off vaccum
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(HiWinRobotInterface.SequencePointData.READY_POSITION) != 0)
                    return;

                nError = WaitForNextStepSequenceEvent("Motor is moving! Press next to turn off the air");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
                {
                    goto RobotSequence_Step10;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
                {
                    goto RobotSequence_Step9;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }

                if(nCamera1InspectionResult ==-(int)ERROR_CODE.PASS)
                    m_Tracks[0].m_CurrentSequenceDeviceID++;
                m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_CURRENT_ROBOT_CHIP_COUNT, m_Tracks[0].m_CurrentSequenceDeviceID);

            RobotSequence_Step11:

                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, false);
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)
                    return;
                LogMessage.LogMessage.WriteToDebugViewer(9, $"End sequence. Total time: ({timeIns_fullSequence.ElapsedMilliseconds} ms)");
                nError = WaitForNextStepSequenceEvent("Sequence completed! Press Next to continue sequence...");
                if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT)
                    break;
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE)
                {
                    goto RobotSequence_Step11;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_GOBACK)
                {
                    goto RobotSequence_Step10;
                }
                else if (nError == (int)SEQUENCE_OPTION.SEQUENCE_RETRY)
                {
                    goto RobotSequence_Step1;

                }

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Full sequence. Total time {timeIns_fullSequence.ElapsedMilliseconds} (ms): " );
                });
                timeIns_fullSequence.Restart();

            }

        }

        internal void BarcodeReaderSequenceThread()
        {

            if (thread_BarcodeReaderSequence == null)
            {
                thread_BarcodeReaderSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_BarcodeReaderSequence()));
                thread_BarcodeReaderSequence.Start();
            }
            else if (!thread_BarcodeReaderSequence.IsAlive)
            {
                thread_BarcodeReaderSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_BarcodeReaderSequence()));
                thread_BarcodeReaderSequence.Start();
            }
        }

        void func_BarcodeReaderSequence()
        {
            Master.m_hardwareTriggerSnapEvent[1].Reset();
            int nChipCount = m_plcComm.ReadPLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_CURRENT_BARCODE_CHIP_COUNT);
            if (nChipCount == 0)
            {
                m_Tracks[1].m_strCurrentLot = string.Format("TrayID_{0}{1}{2}_{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
            }

            while (MainWindow.mainWindow.bEnableRunSequence)
            {
                if (!m_plcComm.m_modbusClient.Connected)
                {
                    MessageBox.Show("Connect to PLC failed. Please check the connection and try again!");
                    return;
                }

                while (m_plcComm.ReadPLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_BARCODE_TRIGGER) == 0)
                {
                    if (MainWindow.mainWindow == null)
                        return;

                    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                        return;

                    if (!m_plcComm.m_modbusClient.Connected)
                    {
                        MessageBox.Show("Connect to PLC failed. Please check the connection and try again!");
                        return;
                    }
                }

                m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_BARCODE_TRIGGER, 0);
                Master.m_hardwareTriggerSnapEvent[1].Set();
                
                while (!Master.VisionReadyEvent[1].WaitOne(1))
                {
                    if (MainWindow.mainWindow == null)
                        return;

                    if (!MainWindow.mainWindow.bEnableRunSequence && !MainWindow.mainWindow.bEnableOfflineInspection)
                        return;

                    if (!m_plcComm.m_modbusClient.Connected)
                    {
                        MessageBox.Show("Connect to PLC failed. Please check the connection and try again!");
                        return;
                    }
                }
                Master.VisionReadyEvent[1].Reset();
                m_Tracks[1].m_CurrentSequenceDeviceID = m_plcComm.ReadPLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_CURRENT_BARCODE_CHIP_COUNT);
                m_Tracks[1].m_CurrentSequenceDeviceID++;
                m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_CURRENT_BARCODE_CHIP_COUNT, m_Tracks[1].m_CurrentSequenceDeviceID);

            }
        }

        private void func_IOStatusThread()
        {
            while (MainWindow.mainWindow != null)
            {
                lock (this)
                {
                    if (m_EmergencyStatus > 0 && !m_bMachineNotReadyNeedToReset)
                        m_bMachineNotReadyNeedToReset = true;

                    if (m_ImidiateStatus > 0 && !m_bNeedToImidiateStop)
                        m_bNeedToImidiateStop = true;
                }


                m_EmergencyStatus = m_EmergencyStatus_Simulate;
                m_ImidiateStatus = m_ImidiateStatus_Simulate;
                m_ResetMachineStatus =  m_ResetMachineStatus_Simulate;

                if (m_hiWinRobotInterface == null)
                {
                    Thread.Sleep(20);
                    continue;
                }
                if (HiWinRobotInterface.m_RobotConnectID < 0)
                {
                    Thread.Sleep(20);
                    continue;
                }

                m_EmergencyStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.EMERGENCY_STATUS) | m_EmergencyStatus_Simulate;
                m_ImidiateStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.IMIDIATE_STATUS) | m_ImidiateStatus_Simulate;
                m_ResetMachineStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.RESET_STATUS) | m_ResetMachineStatus_Simulate;

                if (m_EmergencyStatus + m_ImidiateStatus > 0)
                    m_hiWinRobotInterface.StopMotor();

                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.EMERGENCY_STATUS, m_EmergencyStatus > 0 ? true : false);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.IMIDIATE_STATUS, m_ImidiateStatus > 0 ? true : false);
                //HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.RESET_STATUS, m_ResetMachineStatus > 0 ? true : false);

                Thread.Sleep(20);
            }
        }

        string m_FailstrLotIDFolder = "";
        int nFailCount = 0;
        internal void func_RunSaveInspectImageThread(int nTrack)
        {
            while (MainWindow.mainWindow != null)
            {
                Thread.Sleep(10);
                ImageSaveData data = new ImageSaveData();
                lock (m_SaveInspectImageQueue)
                {
                    if (m_SaveInspectImageQueue[nTrack].Count == 0)
                        continue;

                        data = m_SaveInspectImageQueue[nTrack].Dequeue();
                }
                try
                {
                    if (data.imageSave == null)
                        continue;
                    if (data.strLotID == null)
                        data.strLotID = "DUMMY";
                    string strLotIDFolder = Path.Combine(Application.pathImageSave, data.strLotID + "\\");

                    if (!Directory.Exists(strLotIDFolder))
                    Directory.CreateDirectory(strLotIDFolder);

                    if (!Directory.Exists(strLotIDFolder + "\\PASS\\"))
                        Directory.CreateDirectory(strLotIDFolder + "\\PASS\\");

                    if (!Directory.Exists(strLotIDFolder + "\\FAIL\\"))
                        Directory.CreateDirectory(strLotIDFolder + "\\FAIL\\");

                    if (m_FailstrLotIDFolder != strLotIDFolder)
                    {
                        nFailCount = 0;
                    }

                    string strPassFail = "PASS";
                    m_FailstrLotIDFolder = strLotIDFolder;
                    string path_image = Path.Combine(strLotIDFolder, "PASS", $"Device_{data.nDeviceID + 1}" + ".bmp");

                    
                    if (data.bFail != -(int)ERROR_CODE.PASS)
                    { 
                        if(nTrack == 0)
                        {
                            nFailCount++;
                            path_image = Path.Combine(strLotIDFolder, "FAIL", $"{data.nDeviceID + 1}_{nFailCount}" + ".bmp");
                        }
                        else
                            path_image = Path.Combine(strLotIDFolder, "FAIL", $"Device_{data.nDeviceID + 1}" + ".bmp");

                    }


                    CvInvoke.Imwrite(path_image, data.imageSave);
                }
                catch (Exception)
                {
                }


            }
        }

        public void TeachThread()
        {
            if (m_TeachThread == null)
            {
                m_TeachThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[MainWindow.activeImageDock.trackID].m_imageViews[0].func_TeachSequence(MainWindow.activeImageDock.trackID)));
                m_TeachThread.SetApartmentState(ApartmentState.STA);
            }
            else
            {
                m_TeachThread = null;
                m_TeachThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[MainWindow.activeImageDock.trackID].m_imageViews[0].func_TeachSequence(MainWindow.activeImageDock.trackID)));
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
                MainWindow.mainWindow.grd_Defect.Children.Clear();
                MainWindow.mainWindow.grd_Defect_Settings.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }

}
