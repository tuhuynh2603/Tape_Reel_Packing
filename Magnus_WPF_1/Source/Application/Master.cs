using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.CvEnum;
using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Define;
using Magnus_WPF_1.UI.UserControls;
using static System.Net.Mime.MediaTypeNames;

namespace Magnus_WPF_1.Source.Application
{
    public class Master {
        //private int width, height, dpi;

        private MainWindow mainWindow;
        public Track[] m_Tracks;
        public static AutoResetEvent[] InspectEvent;
        public static AutoResetEvent[] InspectDoneEvent;
        public static AutoResetEvent[] m_hardwareTriggerSnapEvent;
        public Application applications = new Application();
        public TeachParametersUC teachParameter = new TeachParametersUC();
        public MappingSetingUC mappingParameter = new MappingSetingUC();

        // public AutoDeleteImagesDlg m_AutoDeleteImagesDlg = new AutoDeleteImagesDlg();

        public delegate void DelegateCameraStream();
        public DelegateCameraStream delegateCameraStream;

        public delegate void GrabDelegate();
        public GrabDelegate grabDelegate;

        public Thread threadGrabImageSimulateCycle;
        public Thread threadInspecOffline;

        public Thread m_SaveInspectImageThread;
        public static Queue<ImageSaveData> m_SaveInspectImageQueue = new Queue<ImageSaveData>(); // create a queue to hold messages
        public BitmapSource btmSource;

        #region Contructor Master
        public Master(MainWindow app)
        {
            mainWindow = app;
            ContructorDocComponent();
            InspectEvent = new AutoResetEvent[Application.m_nTrack];
            InspectDoneEvent = new AutoResetEvent[Application.m_nTrack];
            m_hardwareTriggerSnapEvent = new AutoResetEvent[Application.m_nTrack];
            for (int n = 0; n < Application.m_nTrack; n++)
            {
                InspectEvent[n] = new AutoResetEvent(false);
                InspectDoneEvent[n] = new AutoResetEvent(false);
                m_hardwareTriggerSnapEvent[n] = new AutoResetEvent(false);
            }

            Application.CheckRegistry();
            Application.LoadRegistry();
            LoadRecipe();


            if (m_SaveInspectImageThread == null)
            {
                m_SaveInspectImageThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => RunSaveInspectImageThread()));
                m_SaveInspectImageThread.Start();
            }


        }
        public void DeleteMaster()
        {
            m_Tracks[0].m_cap.Dispose();
            InspectEvent[0].Reset();
            InspectEvent[0].Dispose();
            InspectDoneEvent[0].Reset();
            InspectDoneEvent[0].Dispose();
        }
        public void LoadRecipe(bool isLoadRecipeManualy = false)
        {
            Application.LoadRecipe();
            #region Load Teach Paramter
            if (!teachParameter.UpdateTeachParameter(Application.dictTeachParam))
            {
                //MessageBox.Show("Can not load teach parameters", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {

                //DebugMessage.WriteToDebugViewer(2, string.Format("Load Teach Parameters Success "));
            }
            InspectionCore.SetTeachParameterToInspectionCore();
            InspectionCore.LoadTeachImageToInspectionCore();
            InspectionCore.AutoTeach();


            mappingParameter.UpdateMappingParameter(Application.dictMappingParam);
            #endregion
        }
        #endregion

        #region ContructorDocComponent


        private void ContructorDocComponent()
        {
            m_Tracks = new Track[Application.m_nTrack];
            for (int index_track = 0; index_track < Application.m_nTrack; index_track++)
            {
                //for(int index_doc = 0; index_doc < Application.m_nDoc; index_doc++)
                m_Tracks[index_track] = new Track(index_track, 1, "none", mainWindow);

            }
        }
        #endregion

        public static BitmapImage LoadBitmap(string pathFile)
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

        public void loadTeachImageToUI()
        {

            string image_top_view = System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_1.bmp");
            if (!File.Exists(image_top_view))
                return;
            else
            {
                string[] nameImageArray = image_top_view.Split('\\');
                int leght = nameImageArray.Count();
                string _nameImage = nameImageArray[leght - 1];
                m_Tracks[0].m_imageViews[0].nameImage = _nameImage;
                BitmapImage bitmap = new BitmapImage();
                bitmap = LoadBitmap(image_top_view);
                m_Tracks[0].m_imageViews[0].UpdateNewImage(bitmap);
                m_Tracks[0].m_imageViews[0].GridOverlay.Children.Clear();
                m_Tracks[0].m_imageViews[0].UpdateTextOverlay("", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                mainWindow.UpdateTitleDoc(0, string.Format("Name Image: {0}", " teachImage.bmp"), true);
            }
        }

        internal void Grab_Image_Testing_Thread(bool bSingleSnap = false)
        {
            //for (int index_track = 0; index_track < Application.m_nTrack; index_track++)
            //{
            if(bSingleSnap)
                m_Tracks[0].SingleSnap();
            else
                m_Tracks[0].Snap();

            //}
        }

        internal void RunSequenceThread()
        {
            mainWindow.ResetMappingResult();
            mainWindow.ResetStatisticResult();
            InspectDoneEvent[0].Reset();
            InspectEvent[0].Reset();
            m_hardwareTriggerSnapEvent[0].Reset();

            if (threadGrabImageSimulateCycle == null)
            {
                threadGrabImageSimulateCycle = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[0].GrabAndInspectionSequence()));
                threadGrabImageSimulateCycle.Start();
            }
            else if (!threadGrabImageSimulateCycle.IsAlive)
            {
                threadGrabImageSimulateCycle = new System.Threading.Thread(new System.Threading.ThreadStart(() => m_Tracks[0].GrabAndInspectionSequence()));
                threadGrabImageSimulateCycle.Start();
            }
        }

        internal void RunSaveInspectImageThread()
        {
            while(MainWindow.mainWindow!= null)
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
                    string path_image = Path.Combine(strLotIDFolder,"Device_" + (data.nDeviceID + 1).ToString() + ".bmp");
                    CvInvoke.Imwrite(path_image, data.imageSave);
                }
                catch (Exception)
                {
                }

                Thread.Sleep(10);

            }
        }

        public void UpdateResult()
        {
        }

        public void WriteTeachParam()
        {
            try
            {
                applications.WriteTeachParam();
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

        public void InspectOffline(string strFolder)
        {

            m_Tracks[0].InspectOffline( strFolder);
        }
    }

}
