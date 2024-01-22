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
    using Microsoft.Win32;
    using System.Diagnostics;
    using System.Windows;
    using static HiWinRobotInterface;
    using static Magnus_WPF_1.Source.Application.Track;
    using static Magnus_WPF_1.Source.Hardware.SDKHrobot.Global;
    using static OfficeOpenXml.ExcelErrorValue;

    public class Master
    {
        //private int width, height, dpi;

        private MainWindow mainWindow;
        public Track[] m_Tracks;
        public int m_nActiveTrack;
        public Application applications = new Application();
        public BarCodeReaderInterface m_BarcodeReader;
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
        public static ManualResetEvent[] StartWaitPLCToTriggerCameraEvent;

        // public AutoDeleteImagesDlg m_AutoDeleteImagesDlg = new AutoDeleteImagesDlg();

        //public delegate void DelegateCameraStream();
        //public DelegateCameraStream delegateCameraStream;

        //public delegate void GrabDelegate();
        //public GrabDelegate grabDelegate;
        public Thread thread_RobotSequence;
        public Thread thread_BarcodeReaderSequence;
        public Thread[] thread_InspectSequence;
        public Thread threadInspecOffline;
        public Thread[] thread_StreamCamera;
        public Thread m_TeachThread;
        public Thread[] m_SaveInspectImageThread;
        public Thread[] m_StartWaitPLCReadyToTriggerCameraThread;

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



            Application.CheckRegistry();
            Application.LoadRegistry();

            ContructorDocComponent();
            LogMessage.LogMessage.WriteToDebugViewer(2, "BarCodeReaderInterface");

            LoadRecipe();
            ReadLogAccount();


            m_nActiveTrack = 0;


            MainWindow.mainWindow.EnableMotorFunction();
            InitThread();
        }

        public void InitThread()
        {
            //string[] nSeriCam = { "02C89933333", "none" };
            if (m_IOStatusThread == null)
            {
                m_IOStatusThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_IOStatusThread()));
                m_IOStatusThread.IsBackground = true;
                m_IOStatusThread.Start();
            }

            for (int index_track = 0; index_track < Application.m_nTrack; index_track++)
            {
                int n = index_track;
                if (m_SaveInspectImageThread[n] == null)
                {
                    m_SaveInspectImageThread[n] = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_RunSaveInspectImageThread(n)));
                    m_SaveInspectImageThread[n].IsBackground = true;
                    m_SaveInspectImageThread[n].Start();
                }
                RunOnlineSequenceThread(n);
            }


            if (!m_bBarcodeReaderSequenceStatus)
            {
                m_bBarcodeReaderSequenceStatus = true;
                BarcodeReaderSequenceThread();
            }
        }

        public void ReadLogAccount()
        {
            applications.acountDefault();
            try
            {
                applications.ReadLogAccount();
                LogMessage.LogMessage.WriteToDebugViewer(2, string.Format("Read Log Account Success "));
            }
            catch (Exception)
            {
                LogMessage.LogMessage.WriteToDebugViewer(2, string.Format("Read Log Account Failed "));
            }
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
        public void LoadRecipe(string strRecipe = "")
        {
            if (strRecipe != "")
            {
                Application.setRecipeToRegister(strRecipe);
            }

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                MainWindow.mainWindow.btn_LoadRecipe.Content = Application.currentRecipe;
            });



            Application.dictMappingParam.Clear();
            Application.LoadMappingParamFromFile();
            mappingParameter.UpdateMappingParamFromDictToUI(Application.dictMappingParam);

            #region Load Teach Paramter
            for (int nTrack = 0; nTrack < Application.m_nTrack; nTrack++)
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
        private void ContructorDocComponent()
        {
            m_Tracks = new Track[Application.m_nTrack];
            InspectEvent = new ManualResetEvent[Application.m_nTrack];
            InspectDoneEvent = new ManualResetEvent[Application.m_nTrack];
            m_OfflineTriggerSnapEvent = new AutoResetEvent[Application.m_nTrack];
            VisionReadyEvent = new ManualResetEvent[Application.m_nTrack];
            StartWaitPLCToTriggerCameraEvent = new ManualResetEvent[Application.m_nTrack];
            m_hardwareTriggerSnapEvent = new AutoResetEvent[Application.m_nTrack];
            m_NextStepSequenceEvent = new AutoResetEvent(false);
            m_EmergencyStopSequenceEvent = new AutoResetEvent(false);
            m_SaveInspectImageThread = new Thread[Application.m_nTrack];
            m_StartWaitPLCReadyToTriggerCameraThread = new Thread[Application.m_nTrack];
            thread_InspectSequence = new Thread[Application.m_nTrack];
            thread_StreamCamera = new Thread[Application.m_nTrack];
            list_arrayOverlay = new List<ArrayOverLay>[Application.m_nTrack];


            m_hiWinRobotInterface = new HiWinRobotInterface();
            m_plcComm = new PLCCOMM();
            m_BarcodeReader = new BarCodeReaderInterface();
            for (int index_track = 0; index_track < Application.m_nTrack; index_track++)
            {
                m_SaveInspectImageQueue[index_track] = new Queue<ImageSaveData>();
                //for(int index_doc = 0; index_doc < Application.m_nDoc; index_doc++)
                InspectEvent[index_track] = new ManualResetEvent(false);
                InspectDoneEvent[index_track] = new ManualResetEvent(false);
                m_hardwareTriggerSnapEvent[index_track] = new AutoResetEvent(false);
                m_OfflineTriggerSnapEvent[index_track] = new AutoResetEvent(false);

                VisionReadyEvent[index_track] = new ManualResetEvent(false);
                StartWaitPLCToTriggerCameraEvent[index_track] = new ManualResetEvent(false);

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

            string strImageFilePath = System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_Track" + (nTrack + 1).ToString() + ".bmp");
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
                BitmapSource _bitmapImage = m_Tracks[nTrack].m_imageViews[0].btmSource;
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
            string pathFileImage = System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "templateImage_Track" + (nTrackID + 1).ToString() + ".bmp");
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

            if (threadInspecOffline == null)
            {
                threadInspecOffline = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrackID].func_InspectOfflineThread(m_folderPath)));
                threadInspecOffline.IsBackground = true;
                threadInspecOffline.Start();
            }
            else if (!threadInspecOffline.IsAlive)
            {
                threadInspecOffline = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrackID].func_InspectOfflineThread(m_folderPath)));
                threadInspecOffline.IsBackground = true;
                threadInspecOffline.Start();
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
                thread_InspectSequence[nTrack].IsBackground = true;
                thread_InspectSequence[nTrack].Start();
            }
            else if (!thread_InspectSequence[nTrack].IsAlive)
            {
                thread_InspectSequence[nTrack] = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[nTrack].func_InspectOnlineThread()));
                thread_InspectSequence[nTrack].IsBackground = true;
                thread_InspectSequence[nTrack].Start();
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
            lock (this)
            {
                m_bMachineNotReadyNeedToReset = true;
            }

            m_hiWinRobotInterface.StopMotor();
            if (m_EmergencyStatus + m_ImidiateStatus == 0)
                HWinRobot.set_motor_state(HiWinRobotInterface.m_RobotConnectID, 1);

            MainWindow.mainWindow.EnableMotorFunction();
        ResetSequence_Step1:
            int nError = (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE;
            Master.VisionReadyEvent[0].Reset();
            Master.VisionReadyEvent[1].Reset();
            if (HiWinRobotInterface.m_RobotConnectID < 0)
                return -1;
            HWinRobot.clear_alarm(HiWinRobotInterface.m_RobotConnectID);

            // Stop Motor
            m_hiWinRobotInterface.StopMotor();
            if (MainWindow.mainWindow.m_bEnableDebugSequence)
                HWinRobot.set_operation_mode(m_RobotConnectID, (int)ROBOT_OPERATION_MODE.MODE_MANUAL);
            else
                HWinRobot.set_operation_mode(m_RobotConnectID, (int)ROBOT_OPERATION_MODE.MODE_AUTO);

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
            MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(HiWinRobotInterface.SequencePointData.PRE_FAILED_PLACE_POSITION, true);
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
            MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(HiWinRobotInterface.SequencePointData.READY_POSITION, true);
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

            MainWindow.mainWindow.EnableMotorFunction();
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


        public int m_PLC_Ready_Status = 0;
        public int m_EmergencyStatus = 0;
        public int m_ImidiateStatus = 0;
        public int m_ResetMachineStatus = 0;
        public int m_RunMachineStatus = 0;
        public int m_DoorOpennedStatus = 0;
        public int m_EndLotStatus = 0;
        public int m_CreateNewLotStatus = 0;

        public int m_EmergencyStatus_Simulate = 0;
        public int m_ImidiateStatus_Simulate = 0;
        public int m_ResetMachineStatus_Simulate = 0;
        public bool m_bMachineNotReadyNeedToReset = true;
        public bool m_bNeedToImidiateStop = false;
        public bool m_IsDoorOpennedAction = false;
        public bool m_bIsReleasePopupMessage = false;

        private void func_IOStatusThread()
        {
            int bEmergencyStatus_Backup = -1;
            int bImidiateStatus_Backup = -1;
            int bResetStatus_Backup = -1;
            int bDoorStatus_Backup = m_DoorOpennedStatus;// HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_DOOR_STATUS);
            int bEndLotStatus_Backup = m_EndLotStatus;// HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_END_LOT);
            int bCreateNewLotStatus_Backup = m_CreateNewLotStatus;// HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_CREATE_NEW_LOT);
            while (MainWindow.mainWindow != null)
            {

                Thread.Sleep(10);
                if (HiWinRobotInterface.m_RobotConnectID < 0 || m_hiWinRobotInterface == null)
                {

                    m_EmergencyStatus = m_EmergencyStatus_Simulate;
                    m_ImidiateStatus = m_ImidiateStatus_Simulate;
                    m_ResetMachineStatus = m_ResetMachineStatus_Simulate;
                }
                else
                {
                    m_PLC_Ready_Status = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_PACKING_PROCESS_READY);
                    m_EmergencyStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.EMERGENCY_STATUS) | m_EmergencyStatus_Simulate;
                    m_ImidiateStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.IMIDIATE_STOP_STATUS) | m_ImidiateStatus_Simulate;
                    m_ResetMachineStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.RESET_STATUS) | m_ResetMachineStatus_Simulate;
                    m_RunMachineStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.RUNSEQUENCE_STATUS);
                    m_DoorOpennedStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_DOOR_STATUS);
                    m_CreateNewLotStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_CREATE_NEW_LOT);
                    m_EndLotStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_END_LOT);
                }

                if (m_ImidiateStatus == 1)
                {
                    m_bNeedToImidiateStop = true;
                    string strMess = "Imidiate Stop Button clicked!";
                    m_hiWinRobotInterface.StopMotor();
                    HiWinRobotInterface.SetMotorState(0);
                    MainWindow.mainWindow.PopupWarningMessageBox(strMess, WARNINGMESSAGE.MESSAGE_IMIDIATESTOP);
                    //Thread.Sleep(100);
                }

                if (!MainWindow.mainWindow.m_bEnableDebugSequence)
                {
                    if (m_DoorOpennedStatus == 1 && bDoorStatus_Backup != m_DoorOpennedStatus)
                    {
                        //m_IsDoorOpennedAction = true;

                        if (MainWindow.mainWindow.m_bSequenceRunning)
                        {
                            m_bNeedToImidiateStop = true;
                            Thread.Sleep(1000);
                            //MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion();
                            string strMess = "Door is openned!";
                            m_hiWinRobotInterface.StopMotor();
                            HiWinRobotInterface.SetMotorState(0);
                            MainWindow.mainWindow.PopupWarningMessageBox(strMess, WARNINGMESSAGE.MESSAGE_IMIDIATESTOP);
                        }
                    }
                    //bDoorStatus_Backup = m_DoorOpennedStatus;
                }

                if (m_EmergencyStatus == 1)
                {
                    m_bMachineNotReadyNeedToReset = true;
                    m_hiWinRobotInterface.StopMotor();
                    HiWinRobotInterface.SetMotorState(0);
                    string strMess = "Emergency Button clicked, please release them  then reset the sequence!";
                    MainWindow.mainWindow.PopupWarningMessageBox(strMess, WARNINGMESSAGE.MESSAGE_EMERGENCY);
                    //Thread.Sleep(100);
                    // Disable all motor function;
                }

                if (m_RunMachineStatus == 1)
                {
                    Thread.Sleep(1000);
                    m_RunMachineStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.RUNSEQUENCE_STATUS);
                    if (m_RunMachineStatus == 1)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            MainWindow.mainWindow.Run_Sequence();
                        });
                    }
                }

                if (m_plcComm == null)
                    continue;

                if (!m_plcComm.m_modbusClient.Connected)
                    continue;

                if (bEmergencyStatus_Backup != m_EmergencyStatus)
                {
                    //m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_EMERGENCY_STATUS, m_EmergencyStatus);
                    bEmergencyStatus_Backup = m_EmergencyStatus;
                    LogMessage.LogMessage.WriteToDebugViewer(9, $"Emergency Status changed Status = {m_EmergencyStatus}!");

                    if (System.Windows.Application.Current == null)
                        return;
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Emergency Status changed Status = {m_EmergencyStatus}!", (int)ERROR_CODE.LABEL_FAIL);
                    });
                }

                if (bImidiateStatus_Backup != m_ImidiateStatus)
                {
                    //m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_IMIDIATE_STATUS, m_ImidiateStatus);
                    bImidiateStatus_Backup = m_ImidiateStatus;

                    LogMessage.LogMessage.WriteToDebugViewer(9, $"Imidiate button Status changed Status = {m_ImidiateStatus}!");

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Imidiate button Status changed Status = {m_ImidiateStatus}!", (int)ERROR_CODE.LABEL_FAIL);
                    });

                }
                if (bResetStatus_Backup != m_ResetMachineStatus)
                {
                    //m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_RESET_STATUS, m_ResetMachineStatus);

                    if (m_ResetMachineStatus == 1)
                    {
                        MainWindow.mainWindow.m_WarningMessageBoxUC.ContinueSequenceButtonClicked(WARNINGMESSAGE.MESSAGE_IMIDIATESTOP);
                    }

                    LogMessage.LogMessage.WriteToDebugViewer(9, $"Reset button Status changed Status = {m_ResetMachineStatus}!");

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Reset button Status changed Status = {m_ResetMachineStatus}!", (int)ERROR_CODE.LABEL_FAIL);
                    });

                }
                bResetStatus_Backup = m_ResetMachineStatus;


                //m_CreateNewLotStatus = m_plcComm.ReadPLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_RESET_LOT); //  HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_CREATE_NEW_LOT);
                //m_CreateNewLotStatus = m_plcComm.ReadPLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_RESET_LOT);
                if (m_CreateNewLotStatus == 1 && bCreateNewLotStatus_Backup != m_CreateNewLotStatus)  /*(m_plcComm.ReadPLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_RESET_LOT) > 0)*/
                {
                    LogMessage.LogMessage.WriteToDebugViewer(9, $"Reset Lot");
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Reset Lot", (int)ERROR_CODE.LABEL_FAIL);
                        Application.m_strCurrentLot = string.Format("{0}{1}{2}_{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
                        Application.m_strStartLotDay = Application.m_strCurrentLot.Split('_')[0];

                        Application.SetStringRegistry(Application.m_strCurrentLot_Registry, Application.m_strCurrentLot);
                        //MainWindow.mainWindow.m_staticView.ClearStatistic();
                        Thread.Sleep(250);
                        lock (MainWindow.mainWindow.master.m_Tracks[0])
                        {
                            MainWindow.mainWindow.LoadStatistic(0);
                        }
                        Thread.Sleep(250);

                        lock (MainWindow.mainWindow.master.m_Tracks[1])
                        {
                            MainWindow.mainWindow.LoadStatistic(1);
                        }
                        Thread.Sleep(250);

                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Create New Lot ID: {Application.m_strCurrentLot} ", (int)ERROR_CODE.LABEL_FAIL);

                        if (true)
                        {
                            MainWindow.mainWindow.m_strCurrentLotID = Application.m_strCurrentLot;
                        }

                        else
                        {

                            MessageBox.Show("Please KeyIn the Lot ID (on the top) then press Continue");
                        }
                    });
                    //m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_RESET_LOT, 0);
                    Thread.Sleep(250);

                    //m_CreateNewLotStatus = 0;
                }
                bCreateNewLotStatus_Backup = m_CreateNewLotStatus;


            }
        }


        public int WaitForNextStepSequenceEvent(string strDebugMessage = "", bool bPopUpInformation = false, int nTimeout = 0)
        {
            m_NextStepSequenceEvent.Reset();
            m_bNextStepSequence = (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE;
            if (m_EmergencyStatus > 0)
                return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;

            if (m_bNeedToImidiateStop)
            {
                //MainWindow.mainWindow.PopupWarningMessageBox(strDebugMessage, WARNINGMESSAGE.MESSAGE_IMIDIATESTOP);
                while (!m_NextStepSequenceEvent.WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;

                    if (m_EmergencyStatus == 1)
                        return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;
                }
                //Show Dialog
            }
            else if (MainWindow.mainWindow.m_bEnableDebugSequence)
            {
                MainWindow.mainWindow.PopupWarningMessageBox(strDebugMessage, WARNINGMESSAGE.MESSAGE_STEPDEBUG);
                while (!m_NextStepSequenceEvent.WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;

                    if (m_EmergencyStatus == 1 || m_ImidiateStatus == 1)
                        return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;
                }

            }
            else if (bPopUpInformation)
            {
                MainWindow.mainWindow.PopupWarningMessageBox(strDebugMessage, WARNINGMESSAGE.MESSAGE_INFORMATION);
                while (!m_NextStepSequenceEvent.WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;

                    if (m_EmergencyStatus == 1 || m_ImidiateStatus == 1)
                        return (int)SEQUENCE_OPTION.SEQUENCE_ABORT;
                }
                return m_bNextStepSequence;
            }

            return m_bNextStepSequence;
        }


        public bool m_bRobotSequenceStatus = false;

        internal bool RobotSequenceThread()
        {

            if (thread_RobotSequence == null)
            {
                thread_RobotSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_RobotSequence()));
                thread_RobotSequence.IsBackground = true;
                thread_RobotSequence.Start();
                return false;
            }
            else if (!thread_RobotSequence.IsAlive)
            {
                thread_RobotSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_RobotSequence()));
                thread_RobotSequence.IsBackground = true;
                thread_RobotSequence.Start();
                return false;
            }
            return true;
        }

        void func_RobotSequence()
        {
            LogMessage.LogMessage.WriteToDebugViewer(9, "Run Robot Sequence Thread....");

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Run Sequence Thead...", (int)ERROR_CODE.LABEL_FAIL);
                MainWindow.mainWindow.LoadStatistic(0);

            });


            RobotSequence();
            m_bRobotSequenceStatus = false;
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                MainWindow.mainWindow.btn_run_sequence.IsChecked = false;
                MainWindow.mainWindow.m_bSequenceRunning = false;
            });

            m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_ROBOT_RUNNING_STATUS, -1);

        }
        void RobotSequence()
        {

        _Start_Lot:
            int nCamera1InspectionResult = -(int)ERROR_CODE.NOT_INSPECTED;
            VisionResultData visionResultDataTemp = m_Tracks[0].m_SequenceThreadVisionResult;
            m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_ROBOT_RUNNING_STATUS, 1);
            int IsLastChipStatus = 0;
            int nCurrentSequenceStep = 0;
            string strLotID = Application.m_strCurrentLot;
            System.Drawing.PointF robotPoint = new System.Drawing.PointF(0, 0);
        // feedback running to plc


            int nError = 0;

            // Home Move // init OutPut
            if (ResetSequence() < 0)
            {
                WaitForNextStepSequenceEvent(" Reset Machine Failed. End Sequence...", true);
                return;
            }

            while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_PACKING_PROCESS_READY) == 0)
            {
                // Need popup Dialog if waiting too long 
                if (!MainWindow.mainWindow.m_bSequenceRunning && !MainWindow.mainWindow.bEnableOfflineInspection || m_EmergencyStatus > 0 || m_EndLotStatus > 0)
                    return;

                LogMessage.LogMessage.WriteToDebugViewer(9, "Waiting for PLC ready Signal....");
                Thread.Sleep(500);
            }

            int nRetry = 0;
        StartSequence:
            m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_ROBOT_RESULT, 0);

            HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_READY_CONVEYOR_ON, true);
            //Wait for Signal from PLC when they ready for new Lot so we can create new lot ID
            //HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_READY_CONVEYOR_ON, false);

            nError = WaitForNextStepSequenceEvent("Begin Sequence: Press Next to trigger camera 1");
            if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE || m_EndLotStatus > 0)
                return;



            //Application.SetIntRegistry(Application.m_strCurrentDeviceID_Registry[0], m_Tracks[0].m_CurrentSequenceDeviceID);


            if (func_CameraTriggerThread() < 0 || m_EndLotStatus > 0)
            {
                return;
            }


            //m_hardwareTriggerSnapEvent[0].Set();

            Stopwatch timeIns = new Stopwatch();
            Stopwatch timeIns_fullSequence = new Stopwatch();
            timeIns_fullSequence.Start();
            timeIns.Start();

            //int nChipCount = 0;
            while (MainWindow.mainWindow.m_bSequenceRunning || MainWindow.mainWindow.bEnableOfflineInspection || m_bMachineNotReadyNeedToReset)
            {

                if (MainWindow.mainWindow == null || m_EndLotStatus > 0)
                    return;

                LogMessage.LogMessage.WriteToDebugViewer(9, "Begin Sequence");
                //m_Tracks[0].m_CurrentSequenceDeviceID
                // Step 1: wait for station 1 inspection
                timeIns.Restart();
                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");
                nCamera1InspectionResult = -(int)ERROR_CODE.NOT_INSPECTED;


                while (!Master.VisionReadyEvent[0].WaitOne(10))
                {
                    if (MainWindow.mainWindow == null)
                        return;

                    if ((!MainWindow.mainWindow.m_bSequenceRunning && !MainWindow.mainWindow.bEnableOfflineInspection) || m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    {
                        return;
                    }

                    if (m_RunMachineStatus == 1)
                    {

                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {

                            ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("PLC Button Run is pressed. Restarting the sequence....", (int)ERROR_CODE.LABEL_FAIL);
                        });
                        Thread.Sleep(3000);
                        goto _Start_Lot;
                    }

                }

                if (m_RunMachineStatus == 1)
                {

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {

                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("PLC Button Run is pressed. Restarting the sequence....", (int)ERROR_CODE.LABEL_FAIL);
                    });
                    Thread.Sleep(3000);
                    goto _Start_Lot;
                }


                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Station 1 inspection No Device Found ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                Thread.Sleep(10);
                nCamera1InspectionResult = m_Tracks[0].m_SequenceVisionResult;

                if (nCamera1InspectionResult == -(int)ERROR_CODE.NO_PATTERN_FOUND) // If no pattern found, popup Error 
                {
                    nRetry++;

                    LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Station 1 inspection No Device Found ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {

                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Device not Found. Image processing Failed!.", (int)ERROR_CODE.LABEL_FAIL);
                    });


                    if (nRetry < 1)
                        goto StartSequence;

                    nRetry = 0;
                    m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_ROBOT_RESULT, -1);
                    HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
                    HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, false);

                    nError = WaitForNextStepSequenceEvent("Inspection Failed (Device Not found)! Please press 'RUN' on UI or PLC  to restart the sequence!", true);
                    if (nError == (int)SEQUENCE_OPTION.SEQUENCE_ABORT || m_EndLotStatus > 0)
                        return;
                    goto StartSequence;
                    //return;
                }



                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Station 1 inspection  ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                nError = WaitForNextStepSequenceEvent("Inspection done. Press Next to Move to  pre pick position");
                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;

                _Step_1:
                nCurrentSequenceStep = 1;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;

                // Transform point from camera position to robot position
                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                visionResultDataTemp = m_Tracks[0].m_SequenceThreadVisionResult;

                robotPoint = MagnusMatrix.ApplyTransformation(MainWindow.mainWindow.master.m_hiWinRobotInterface.m_hiWinRobotUserControl.m_MatCameraRobotTransform, m_Tracks[0].m_Center_Vision);
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Transform point from camera position to robot position ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();

                // Move to Pre Pick position
                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PRE_PICK_POSITION(robotPoint, -m_Tracks[0].m_dDeltaAngleInspection);
                //{
                //    nError = WaitForNextStepSequenceEvent($"Step {nCurrentSequenceStep} FAILED when move motor");
                //    if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                //        goto Check_StepSequence;
                //}

                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)
                {
                    nError = WaitForNextStepSequenceEvent($"Step {nCurrentSequenceStep} FAILED when move motor");
                    if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                        goto Check_StepSequence;
                }

                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Move to Pre Pick position ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                nError = WaitForNextStepSequenceEvent("Move to Pre Pick position done. Press Next to turn on vaccum");
                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;


                _Step_2:
                nCurrentSequenceStep = 2;


                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });


                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;
                // Turn on vaccum

                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, true);

                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, false);

                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Turn on vaccum ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                nError = WaitForNextStepSequenceEvent("Press Next to Pick the device");
                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;

                _Step_3:
                nCurrentSequenceStep = 3;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;
                // Move to Pick position (move Down Z motor)
                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PICK_POSITION(robotPoint, -m_Tracks[0].m_dDeltaAngleInspection);
                //if(MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PICK_POSITION(robotPoint, -m_Tracks[0].m_dDeltaAngleInspection) != 0)
                //{
                //    nError = WaitForNextStepSequenceEvent($"Step {nCurrentSequenceStep} FAILED when move motor");
                //    if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                //        goto Check_StepSequence;
                //}
                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)
                {
                    nError = WaitForNextStepSequenceEvent($"Step {nCurrentSequenceStep} FAILED when move motor");
                    if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                        goto Check_StepSequence;
                }
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Move to Pick position (move Down Z motor) ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();

                // Move to  Pre pick  position again (move Up Z motor)
                // From now, if Air PressureStatus signal is 0, we consider it as the chip has been through, so we arlamp it
                nError = WaitForNextStepSequenceEvent("Press Next to move up to pre pick position");
                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;

                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");


            _Step_4:
                nCurrentSequenceStep = 4;
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;
                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_PRE_PICK_POSITION(robotPoint, -m_Tracks[0].m_dDeltaAngleInspection);

                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)
                {
                    nError = WaitForNextStepSequenceEvent($"Step {nCurrentSequenceStep} FAILED when move motor");
                    if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                        goto Check_StepSequence;
                }

                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                //Trigger conveyor on
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_READY_CONVEYOR_ON, true);
                //
                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                int nCountSleep = 0;

                m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_AIR_PRESS_RESULT, 0);

                while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.AIR_PRESSURESTATUS) == 0)
                {
                    if (!MainWindow.mainWindow.m_bSequenceRunning && !MainWindow.mainWindow.bEnableOfflineInspection || m_EndLotStatus > 0)
                    {
                        return;
                    }

                    nCountSleep++;
                    if (nCountSleep > 300)
                    {

                        m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_AIR_PRESS_RESULT, -1);
                        HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
                        HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, false);

                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {

                            ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Air Pressure got problem!", (int)ERROR_CODE.LABEL_FAIL);
                        });

                        //WaitForNextStepSequenceEvent("Air Pressure got problem!",true);
                        return;
                    }
                    Thread.Sleep(10);
                }

                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Move to  Pre pick  position again ({timeIns.ElapsedMilliseconds} ms)");
                nError = WaitForNextStepSequenceEvent("Move to prepick position done. Press Next to move up to pre pick position");
                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;

                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

            _Step_5:
                nCurrentSequenceStep = 5;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;
                // Move To Pass fail position
                //nCamera1InspectionResult = (int)ERROR_CODE.PASS;
                string strPrePosition;
                if (nCamera1InspectionResult == -(int)ERROR_CODE.PASS)
                    strPrePosition = SequencePointData.PRE_PASS_PLACE_POSITION;
                else if (nCamera1InspectionResult == -(int)ERROR_CODE.OPPOSITE_CHIP)
                    strPrePosition = SequencePointData.PRE_FAILED_BLACK_PLACE_POSITION;
                else
                    strPrePosition = SequencePointData.PRE_FAILED_PLACE_POSITION;

                //string strPosition = nCamera1InspectionResult == 0 ? SequencePointData.PASS_PLACE_POSITION : SequencePointData.FAILED_PLACE_POSITION;
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Move To Pass fail position ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();

                MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(strPrePosition);

                // Sleep 200ms to make sure the robot outside of camera region
                //Thread.Sleep(200);
                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)
                {
                    nError = WaitForNextStepSequenceEvent($"Step {nCurrentSequenceStep} FAILED when move motor");
                    if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                        goto Check_StepSequence;
                }

                // Trigger camera  1 

                nError = WaitForNextStepSequenceEvent("Move To Pass fail position Done. Press Next to trigger camera 1");
                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;

                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");


            _Step_6:
                nCurrentSequenceStep = 6;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;


                //if (strLotID != Application.m_strCurrentLot)
                //{
                //    strLotID = Application.m_strCurrentLot;
                //    VisionResultData.ReadLotResultFromExcel(Application.m_strCurrentLot, 0, ref m_Tracks[0].m_VisionResultDatas, ref m_Tracks[0].m_CurrentSequenceDeviceID);

                //    if (m_Tracks[0].m_CurrentSequenceDeviceID < 0 || m_Tracks[0].m_CurrentSequenceDeviceID >= Application.categoriesMappingParam.M_NumberDevicePerLot)
                //        m_Tracks[0].m_CurrentSequenceDeviceID = 0;


                //}

                //Application.SetIntRegistry(Application.m_strCurrentDeviceID_Registry[0], m_Tracks[0].m_CurrentSequenceDeviceID);
                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");


                // If not yet trigger camera 1, trigger again
                m_StartWaitPLCReadyToTriggerCameraThread[0] = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_CameraTriggerThread(nCurrentSequenceStep, 0)));
                m_StartWaitPLCReadyToTriggerCameraThread[0].IsBackground = true;
                m_StartWaitPLCReadyToTriggerCameraThread[0].Start();

                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;

                _Step_7:
                nCurrentSequenceStep = 7;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;
                // Wait For PLC 2 Ready
                //m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_ROBOT_RESULT, nCamera1InspectionResult);


                if (nCamera1InspectionResult == -(int)ERROR_CODE.PASS)
                {
                    HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_PLACE_DONE, false);
                    LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");
                    while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_ALLOW_TO_PLACE) == 0)
                    {
                        if (!MainWindow.mainWindow.m_bSequenceRunning && !MainWindow.mainWindow.bEnableOfflineInspection || m_EndLotStatus > 0)
                        {
                            return;
                        }

                        if (m_RunMachineStatus == 1)
                        {

                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {

                                ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("PLC Button Run is pressed. Restarting the sequence....", (int)ERROR_CODE.LABEL_FAIL);
                            });
                            Thread.Sleep(3000);
                            goto _Start_Lot;
                        }
                    }
                }

                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Wait For PLC Ready ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();
                nError = WaitForNextStepSequenceEvent("wait for plc ready signal done. Press Next to Put device to the tray");
                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;



                _Step_8:
                nCurrentSequenceStep = 8;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;
                /////////////////Step 9: put device to the tray (turn off vaccum)
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, true);
                Thread.Sleep(50);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, false);
                LogMessage.LogMessage.WriteToDebugViewer(9, $"Last Chip Status: {IsLastChipStatus}");
                //End 30 End sequencd if pass

                if (nCamera1InspectionResult == -(int)ERROR_CODE.PASS)
                {
                    if (IsLastChipStatus > 0)
                    {

                        LogMessage.LogMessage.WriteToDebugViewer(9, $" Check Last Chip Status to Move or wait: {IsLastChipStatus}");
                        HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_PLACE_DONE, true);

                        if (MainWindow.mainWindow.m_bSequenceRunning)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {

                                // Update Statistics
                                //if (m_Tracks[1].m_CurrentSequenceDeviceID == 0)
                                //    MainWindow.mainWindow.m_staticView.ResetMappingResult(1);

                                LogMessage.LogMessage.WriteToDebugViewer(9, $"Robot Sequence: Update result");
                                visionResultDataTemp.m_nResult = nCamera1InspectionResult;
                                MainWindow.mainWindow.m_staticView.UpdateMappingResult(m_Tracks[0].m_InspectionOnlineThreadVisionResult, 0, m_Tracks[0].m_CurrentSequenceDeviceID);
                                MainWindow.mainWindow.m_staticView.UpdateValueStatistic(nCamera1InspectionResult, 0);
                                LogMessage.LogMessage.WriteToDebugViewer(9, $"Robot Sequence: Update result Done. Device {m_Tracks[0].m_CurrentSequenceDeviceID + 1}");
                            });
                        }

                        goto _Start_Lot;
                    }

                    //get Chip signal at end 29
                    IsLastChipStatus = HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_LAST_CHIP);
                    HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_PLACE_DONE, true);

                    //PLC start trigger last chip at begin 29
                }



                LogMessage.LogMessage.WriteToDebugViewer(9, $"Step {nCurrentSequenceStep} : Put device to the tray ({timeIns.ElapsedMilliseconds} ms)"); timeIns.Restart();

                //if(nEmptyTrayHoles >= 5)
                //    m_hardwareTriggerSnapEvent[1].Set();

                nError = WaitForNextStepSequenceEvent("Put device to the tray Done. Press next to Move to Ready position");
                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;



                _Step_9:
                nCurrentSequenceStep = 9;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;



                _Step_10:
                nCurrentSequenceStep = 10;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                if (m_bMachineNotReadyNeedToReset || m_EndLotStatus > 0)
                    return;
                // Move to ready position and turn off vaccum

                if (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_CHIPFOUND) == 0)
                {
                    if (MainWindow.mainWindow.master.m_hiWinRobotInterface.MoveTo_STATIC_POSITION(HiWinRobotInterface.SequencePointData.READY_POSITION) != 0)
                    {
                        nError = WaitForNextStepSequenceEvent($"Step {nCurrentSequenceStep} FAILED when move motor");
                        if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                            goto Check_StepSequence;
                    }

                    LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                    nError = WaitForNextStepSequenceEvent("Motor is moving! Press next to turn off the air");
                    if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                        goto Check_StepSequence;
                }

            _Step_11:
                nCurrentSequenceStep = 11;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                });

                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_ON, false);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_AIR_OFF, false);



                if (MainWindow.mainWindow.m_bSequenceRunning)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {

                        // Update Statistics
                        //if (m_Tracks[1].m_CurrentSequenceDeviceID == 0)
                        //    MainWindow.mainWindow.m_staticView.ResetMappingResult(1);

                        LogMessage.LogMessage.WriteToDebugViewer(9, $"Robot Sequence: Update result");
                        visionResultDataTemp.m_nResult = nCamera1InspectionResult;
                        MainWindow.mainWindow.m_staticView.UpdateMappingResult(m_Tracks[0].m_InspectionOnlineThreadVisionResult, 0, m_Tracks[0].m_CurrentSequenceDeviceID);
                        MainWindow.mainWindow.m_staticView.UpdateValueStatistic(nCamera1InspectionResult, 0);
                        LogMessage.LogMessage.WriteToDebugViewer(9, $"Robot Sequence: Update result Done. Device {m_Tracks[0].m_CurrentSequenceDeviceID + 1}");
                    });
                }


                if (MainWindow.mainWindow.master.m_hiWinRobotInterface.wait_for_stop_motion() != 0)

                {
                    nError = WaitForNextStepSequenceEvent($"Step {nCurrentSequenceStep} FAILED when move motor");
                    if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                        goto Check_StepSequence;
                }

                LogMessage.LogMessage.WriteToDebugViewer(9, $"End sequence. Total time: ({timeIns_fullSequence.ElapsedMilliseconds} ms)");
                nError = WaitForNextStepSequenceEvent("Sequence completed! Press Next to continue sequence...");
                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                    goto Check_StepSequence;

                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");

                if (nCamera1InspectionResult == -(int)ERROR_CODE.PASS)
                    m_Tracks[0].m_CurrentSequenceDeviceID++;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Pass Count {m_Tracks[0].m_CurrentSequenceDeviceID} Full sequence. Total time {timeIns_fullSequence.ElapsedMilliseconds} (ms): ");
                });
                timeIns_fullSequence.Restart();

                LogMessage.LogMessage.WriteToDebugViewer(9, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");


            Check_StepSequence:
                if (nError != (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE)
                {
                    switch (nError)
                    {
                        case (int)SEQUENCE_OPTION.SEQUENCE_ABORT:
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                            });
                            return;

                        case (int)SEQUENCE_OPTION.SEQUENCE_IMIDIATE_BUTTON_CONTINUE:
                            nCurrentSequenceStep++;
                            break;

                        case (int)SEQUENCE_OPTION.SEQUENCE_GOBACK:
                            nCurrentSequenceStep--;
                            break;

                        case (int)SEQUENCE_OPTION.SEQUENCE_RETRY:
                            nCurrentSequenceStep = 0;
                            break;

                        case (int)SEQUENCE_OPTION.SEQUENCE_CONTINUE:
                            break;

                        default:
                            break;
                    }

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Current Step {nCurrentSequenceStep} ");
                    });


                    switch (nCurrentSequenceStep)
                    {
                        case 0:
                            goto _Step_1;
                        case 1:
                            goto _Step_1;
                        case 2:
                            goto _Step_2;
                        case 3:
                            goto _Step_3;
                        case 4:
                            goto _Step_4;
                        case 5:
                            goto _Step_5;
                        case 6:
                            goto _Step_6;
                        case 7:
                            goto _Step_7;
                        case 8:
                            goto _Step_8;
                        case 9:
                            goto _Step_9;
                        case 10:
                            goto _Step_10;
                        case 11:
                            goto _Step_11;

                    }
                }

            }

        }
        public int func_CameraTriggerThread(int nCurrentSequenceStep = -1, int nTrack = 0)
        {
            //int nError;
            LogMessage.LogMessage.WriteToDebugViewer(9, $"Waiting for Chip found Signal from PLC");
            VisionReadyEvent[0].Reset();
            m_hardwareTriggerSnapEvent[0].Reset();

            while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_CHIPFOUND) == 0)
            {

                if (!MainWindow.mainWindow.m_bSequenceRunning && !MainWindow.mainWindow.bEnableOfflineInspection || m_EmergencyStatus > 0)
                {
                    return -1;
                }
                Thread.Sleep(5);
            }
            LogMessage.LogMessage.WriteToDebugViewer(9, $"Set PLC Conveyor ({ (int)OUTPUT_IOROBOT.ROBOT_READY_CONVEYOR_ON} ON");
            HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.ROBOT_READY_CONVEYOR_ON, false);
            LogMessage.LogMessage.WriteToDebugViewer(9, $"Set Camera Trigger Event ");
            m_hardwareTriggerSnapEvent[0].Set();
            return 0;
        }
        internal void BarcodeReaderSequenceThread()
        {


            if (thread_BarcodeReaderSequence == null)
            {
                thread_BarcodeReaderSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_BarcodeReaderSequence()));
                thread_BarcodeReaderSequence.IsBackground = true;
                thread_BarcodeReaderSequence.Start();
            }
            else if (!thread_BarcodeReaderSequence.IsAlive)
            {
                thread_BarcodeReaderSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_BarcodeReaderSequence()));
                thread_BarcodeReaderSequence.IsBackground = true;
                thread_BarcodeReaderSequence.Start();
            }
        }

        public bool m_bBarcodeReaderSequenceStatus = false;
        void func_BarcodeReaderSequence()
        {

            //while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_PACKING_PROCESS_READY) == 0)
            //{
            //    // Need popup Dialog if waiting too long 
            //    if (!MainWindow.mainWindow.m_bSequenceRunning && !MainWindow.mainWindow.bEnableOfflineInspection || m_EmergencyStatus > 0)
            //        return;

            //    LogMessage.LogMessage.WriteToDebugViewer(8, "Waiting for PLC ready Signal....");
            //    Thread.Sleep(500);

            //}
            LogMessage.LogMessage.WriteToDebugViewer(9, "Run Barcode Sequence Thread....");

            BarcodeReaderSequence();
            m_bBarcodeReaderSequenceStatus = false;
        }

        void BarcodeReaderSequence()
        {
            //startBarcodeSequence:
            HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_RESULT_PASS, false);
            HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_RESULT_FAIL, false);
            //int nTimeout = 0;
            Master.m_hardwareTriggerSnapEvent[1].Reset();
            Master.VisionReadyEvent[1].Reset();
            //m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_BARCODE_TRIGGER, 0);

            //string strLotID = Application.m_strCurrentLot;

            //VisionResultData.ReadLotResultFromExcel(Application.m_strCurrentLot, 1, ref m_Tracks[1].m_VisionResultDatas, ref m_Tracks[1].m_CurrentSequenceDeviceID);

            //if (m_Tracks[1].m_CurrentSequenceDeviceID < 0 || m_Tracks[1].m_CurrentSequenceDeviceID >= Application.categoriesMappingParam.M_NumberDevicePerLot)
            //    m_Tracks[1].m_CurrentSequenceDeviceID = 0;
            // PC will reset the new lot status 
            //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Barcode Reader Read PLC Chip Count!", (int)ERROR_CODE.NO_LABEL);

            //});
            //m_Tracks[1].m_CurrentSequenceDeviceID = Application.GetIntRegistry(Application.m_strCurrentDeviceID_Registry[1], 0);

            //m_Tracks[1].m_CurrentPLCRegisterDeviceID = m_plcComm.ReadPLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_CURRENT_BARCODE_CHIP_COUNT);
            //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Emergency! Barcode Reader Sequence Ended!", (int)ERROR_CODE.NO_LABEL);

            //});
            //if (m_Tracks[1].m_CurrentPLCRegisterDeviceID < 0 || m_Tracks[1].m_CurrentPLCRegisterDeviceID >= Application.categoriesMappingParam.M_NumberDevicePerLot)
            //    m_Tracks[1].m_CurrentSequenceDeviceID = Application.GetIntRegistry(Application.m_strCurrentDeviceID_Registry[1], 0);
            //else
            //    m_Tracks[1].m_CurrentSequenceDeviceID = m_Tracks[1].m_CurrentPLCRegisterDeviceID;


            //if (m_Tracks[1].m_CurrentSequenceDeviceID < 0 || m_Tracks[1].m_CurrentSequenceDeviceID >= Application.categoriesMappingParam.M_NumberDevicePerLot)
            //    m_Tracks[1].m_CurrentSequenceDeviceID = 0;

            //Application.SetIntRegistry(Application.m_strCurrentDeviceID_Registry[1], m_Tracks[1].m_CurrentSequenceDeviceID);
            if (m_plcComm == null)
                return;

            m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_BARCODE_READY, 1);

            while (true/*MainWindow.mainWindow.m_bSequenceRunning*/)
            {
                LogMessage.LogMessage.WriteToDebugViewer(8, $"Barcode Reader: Wait PLC Barcode Trigger {(int)INPUT_IOROBOT.PLC_BARCODE_TRIGGER}!");




                while (HWinRobot.get_digital_input(HiWinRobotInterface.m_RobotConnectID, (int)INPUT_IOROBOT.PLC_BARCODE_TRIGGER) != 1)
                {
                    if (MainWindow.mainWindow == null)
                        return;
                    Thread.Sleep(30);
                }

                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_RESULT_PASS, false);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_RESULT_FAIL, false);


                if (m_EmergencyStatus > 0)
                {

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Emergency! Barcode Reader Sequence Restart!", (int)ERROR_CODE.LABEL_FAIL);

                    });
                    Thread.Sleep(100);
                    continue;

                }

                int nDeviceID = m_Tracks[1].m_CurrentSequenceDeviceID;

                //if (strLotID != Application.m_strCurrentLot)
                //{
                //    strLotID = Application.m_strCurrentLot;

                //    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                //    {
                //        MainWindow.mainWindow.ResetStatistic(1);
                //        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog($"Barcode Reader: Change lot ID: {strLotID} ", (int)ERROR_CODE.LABEL_FAIL);

                //    });
                //    if (m_Tracks[1].m_CurrentSequenceDeviceID < 0 || m_Tracks[1].m_CurrentSequenceDeviceID >= Application.categoriesMappingParam.M_NumberDevicePerLot)
                //        m_Tracks[1].m_CurrentSequenceDeviceID = 0;
                //}



                //if (m_EmergencyStatus > 0)
                //{
                //    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                //    {
                //        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Emergency! Barcode Reader Sequence Ended!", (int)ERROR_CODE.LABEL_FAIL);

                //    });
                //    return;

                //}

                LogMessage.LogMessage.WriteToDebugViewer(8, $"Barcode Reader: Set BARCODE_CAPTURE_BUSY {(int)OUTPUT_IOROBOT.BARCODE_CAPTURE_BUSY} ON ");
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_CAPTURE_BUSY, true);
                VisionReadyEvent[1].Reset();

                m_hardwareTriggerSnapEvent[1].Set();
                LogMessage.LogMessage.WriteToDebugViewer(8, $"Barcode Reader: Waiting for vision done.... ");
                int nVisionResult = m_Tracks[1].m_SequenceThreadVisionResult.m_nResult;
                if (!VisionReadyEvent[1].WaitOne(1500))
                {
                    if (MainWindow.mainWindow == null)
                        return;
                    m_hardwareTriggerSnapEvent[1].Reset();
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Barcode Reader: TIME OUT!", (int)ERROR_CODE.LABEL_FAIL);

                    });
                    //VisionReadyEvent[1].Reset();
                    //m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_BARCODE_RESULT, -1);
                    //HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_RESULT, true);
                    //Thread.Sleep(1000);
                    //HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_CAPTURE_BUSY, false);
                    RunOnlineSequenceThread(1);
                    //goto startBarcodeSequence;
                    nVisionResult = -(int)ERROR_CODE.CAPTURE_FAIL;
                }
                else
                    nVisionResult = m_Tracks[1].m_InspectionOnlineThreadVisionResult.m_nResult;

                m_Tracks[1].m_VisionResultDatas[nDeviceID] = m_Tracks[1].m_InspectionOnlineThreadVisionResult;
                m_Tracks[1].m_VisionResultDatas[nDeviceID].m_nResult = nVisionResult;
                if (m_EmergencyStatus > 0)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Barcode Reader Sequence Ended!", (int)ERROR_CODE.LABEL_FAIL);

                    });
                    continue;

                }

                LogMessage.LogMessage.WriteToDebugViewer(8, $"Barcode Reader: Vision Done! ");

                bool bFailSendToPLC = false;
                if (nVisionResult != -(int)ERROR_CODE.PASS)
                {

                    bFailSendToPLC = true;
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).AddLineOutputLog("Barcode Reader: Result FAILED!", (int)ERROR_CODE.LABEL_FAIL);

                    });
                }

                LogMessage.LogMessage.WriteToDebugViewer(8, $"Barcode Reader: Send result to PLC {(int)PLCCOMM.PLC_ADDRESS.PLC_BARCODE_RESULT}  . Result: {bFailSendToPLC} ");
                m_plcComm.WritePLCRegister((int)PLCCOMM.PLC_ADDRESS.PLC_BARCODE_RESULT, nVisionResult);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_RESULT_PASS, !bFailSendToPLC);
                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_RESULT_FAIL, bFailSendToPLC);

                HWinRobot.set_digital_output(HiWinRobotInterface.m_RobotConnectID, (int)OUTPUT_IOROBOT.BARCODE_CAPTURE_BUSY, false);
                LogMessage.LogMessage.WriteToDebugViewer(8, $"Barcode Reader: Send result to PLC Done {(int)PLCCOMM.PLC_ADDRESS.PLC_BARCODE_RESULT}  . Result: {bFailSendToPLC} ");

                //LogMessage.LogMessage.WriteToDebugViewer(8, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");
                if (MainWindow.mainWindow.m_bSequenceRunning)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {

                        // Update Statistics
                        //if (m_Tracks[1].m_CurrentSequenceDeviceID == 0)
                        //    MainWindow.mainWindow.m_staticView.ResetMappingResult(1);
                        m_Tracks[1].m_VisionResultDatas[nDeviceID] = m_Tracks[1].m_InspectionOnlineThreadVisionResult;
                        m_Tracks[1].m_VisionResultDatas[nDeviceID].m_nResult = nVisionResult;

                        LogMessage.LogMessage.WriteToDebugViewer(8, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");
                        MainWindow.mainWindow.m_staticView.UpdateMappingResult(m_Tracks[1].m_VisionResultDatas[nDeviceID], 1, nDeviceID);
                        LogMessage.LogMessage.WriteToDebugViewer(8, $"{ Application.LineNumber()}: {Application.PrintCallerName()}");
                        MainWindow.mainWindow.m_staticView.UpdateValueStatistic(nVisionResult, 1);

                    });

                    if (nVisionResult == (int)ERROR_CODE.PASS)
                        m_Tracks[1].m_CurrentSequenceDeviceID++;
                }
                LogMessage.LogMessage.WriteToDebugViewer(8, $"Barcode Reader: Scan Done!");

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
                lock (m_SaveInspectImageQueue[nTrack])
                {
                    if (m_SaveInspectImageQueue[nTrack].Count == 0)
                        continue;

                    data = m_SaveInspectImageQueue[nTrack].Dequeue();
                }
                try
                {
                    if (data.imageSave == null)
                        continue;

                    string strDeviceIDTime = string.Format("{0}{1}{2}+{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));

                    //string strLotIDFolder = Path.Combine(Application.pathImageSave, data.strLotID + "\\");
                    string strDay = string.Format("{0}{1}{2}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"));
                    string strPassFolder = Path.Combine(Application.pathImageSave,"Camera",Application.currentRecipe, strDay, data.strLotID, "PASS IMAGE");
                    if (!Directory.Exists(strPassFolder))
                        Directory.CreateDirectory(strPassFolder);
                    string path_image = Path.Combine(strPassFolder, $"{strDeviceIDTime}_{data.nDeviceID + 1}" + ".bmp");

                    string strFailFolder = Path.Combine(Application.pathImageSave, "Camera", Application.currentRecipe, strDay, data.strLotID, "FAIL IMAGE");
                    if (!Directory.Exists(strFailFolder))
                        Directory.CreateDirectory(strFailFolder);



                    if (m_FailstrLotIDFolder != strPassFolder)
                    {
                        nFailCount = 0;
                    }


                    //string strPassFail = "PASS";
                    //m_FailstrLotIDFolder = strPassFolder;


                    if (data.nResult != -(int)ERROR_CODE.PASS)
                    {
                        //nFailCount++;
                        path_image = Path.Combine(strFailFolder, $"{strDeviceIDTime}_{nFailCount + 1}" + ".bmp");
                    }

                    //string pathTemp = path_image;
                    //strDeviceIDTime =  string.Format("{0}{1}{2}+{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
                    //if (File.Exists(pathTemp))
                    //    pathTemp.Replace($"_{data.nDeviceID + 1}", $"{strDeviceIDTime}_{data.nDeviceID + 1}");

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
            m_TeachThread.IsBackground = true;
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
            if (HiWinRobotInterface.m_RobotConnectID >= 0)
                HWinRobot.set_operation_mode(HiWinRobotInterface.m_RobotConnectID, (int)ROBOT_OPERATION_MODE.MODE_MANUAL);
            m_hiWinRobotInterface.m_hiWinRobotUserControl.check_Manual.IsChecked = true;
            m_hiWinRobotInterface.m_hiWinRobotUserControl.check_Auto.IsChecked = false;

            if (bIschecked)
            {
                MainWindow.mainWindow.grd_Defect.Height = 650;// m_hiWinRobotInterface.m_hiWinRobotUserControl.Height;
                MainWindow.mainWindow.grd_Defect.Width = 800;// m_hiWinRobotInterface.m_hiWinRobotUserControl.Width;

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
