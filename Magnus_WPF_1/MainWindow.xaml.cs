using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Application;
using Magnus_WPF_1.Source.Define;
//using System.Windows.Forms;
using Magnus_WPF_1.Source.LogMessage;
using Magnus_WPF_1.UI.UserControls;
using Magnus_WPF_1.UI.UserControls.View;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Wpf.AvalonDock.Layout;
using Application = Magnus_WPF_1.Source.Application.Application;

namespace Magnus_WPF_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public Master master;
        public bool IsFisrtLogin = false;
        public bool Logout = false;

        public static MainWindow mainWindow;
        public OutputLogView outputLogView;
        public StatisticView m_staticView;
        public int m_nDeviceX = 5;
        public int m_nDeviceY = 5;
        public int m_nTotalDevicePerLot = 1000;
        public DefectInfor defectInfor = new DefectInfor();
        public WarningMessageBox m_WarningMessageBoxUC = new WarningMessageBox();

        public static string[] titles = new string[] { "Top Camera", "Barcode Reader", "Flap Side 2 " };

        private int screenWidth = 2000;// System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        private int screenHeight = 2000;// System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

        private LayoutDocumentPaneGroup mainPanelGroup;
        public LayoutDocumentPaneGroup oldPanelGroup = new LayoutDocumentPaneGroup();

        private LayoutDocumentPaneGroup bigPanelGroup = new LayoutDocumentPaneGroup();
        private LayoutDocumentPane imageZoomViewPane = new LayoutDocumentPane();
        private LayoutDocument bigDoc = new LayoutDocument();

        private LayoutDocumentPaneGroup imagesViewPaneGroup;
        private LayoutDocumentPane[] imagesViewPane;
        private LayoutDocument[] imagesViewDoc;


        private LayoutAnchorablePaneGroup outPutLogPaneGroup;
        private LayoutAnchorablePane outPutLogViewPane;
        private LayoutAnchorable outPutLogViewDoc;

        private LayoutDocument teachViewDoc;
        public static ImageView activeImageDock;
        LayoutPanel m_layout;

        private LayoutAnchorablePaneGroup m_MappingPaneGroup;
        private LayoutAnchorablePane m_MappingViewPane;
        private LayoutAnchorable m_MappingViewDoc;
        LayoutPanel m_layOut_OutPutLog;
        LayoutPanel m_layOut_Mapping;


        private Point _startPositionDlg;
        private System.Windows.Vector _startOffsetPositionDlg;
        private LayoutRoot _layoutHPSF;
        public LayoutRoot layoutHPSF
        {
            get { return _layoutHPSF; }
            set
            {
                _layoutHPSF = value;
                OnPropertyChanged("layoutHPSF");
            }
        }

        private LayoutRoot _layout_OutputLog;
        public LayoutRoot layout_OutputLog
        {
            get { return _layout_OutputLog; }
            set
            {
                _layout_OutputLog = value;
                OnPropertyChanged("layout_OutputLog");
            }
        }

        private LayoutRoot _layout_Mapping;

        public LayoutRoot layout_Mapping
        {
            get { return _layout_Mapping; }
            set
            {
                _layout_Mapping = value;
                OnPropertyChanged("layout_Mapping");
            }
        }

        

        private double _dialogDefectHeight;
        private double _dialogDefectWidth;
        public double DialogDefectHeight
        {
            get { return _dialogDefectHeight; }
            set
            {
                if (value != _dialogDefectHeight)
                {
                    _dialogDefectHeight = value;
                    OnPropertyChanged("DialogDefectHeight");
                }
            }
        }
        public double DialogDefectWidth
        {
            get { return _dialogDefectWidth; }
            set
            {
                if (value != _dialogDefectWidth)
                {
                    _dialogDefectWidth = value;
                    OnPropertyChanged("DialogDefectWidth");
                }
            }
        }

        #region STATE OF CONNECTION PLC 
        //name port
        //private string _barcodeReaderStatus;
        //public string barcodeReaderStatus
        //{
        //    get { return _barcodeReaderStatus; }
        //    set
        //    {
        //        if (value != _barcodeReaderStatus)
        //        {
        //            _barcodeReaderStatus = value;
        //            OnPropertyChanged("barcodeReaderStatus");
        //        }
        //    }
        //}

        // Color
        private string _color_barcodeReaderStatus;
        public string color_barcodeReaderStatus
        {
            get { return _color_barcodeReaderStatus; }
            set
            {
                if (value != _color_barcodeReaderStatus)
                {
                    _color_barcodeReaderStatus = value;
                    OnPropertyChanged("color_barcodeReaderStatus");
                }
            }
        }

        //private string _text_RobotStatus;
        //public string text_RobotStatus
        //{
        //    get { return _text_RobotStatus; }
        //    set
        //    {
        //        if (value != _text_RobotStatus)
        //        {
        //            _text_RobotStatus = value;
        //            OnPropertyChanged("text_RobotStatus");
        //        }
        //    }
        //}

        private string _color_RobotStatus;
        public string color_RobotStatus
        {
            get { return _color_RobotStatus; }
            set
            {
                if (value != _color_RobotStatus)
                {
                    _color_RobotStatus = value;
                    OnPropertyChanged("color_RobotStatus");
                }
            }
        }


        #endregion



        private LayoutPanel m_layoutPanel;
        public delegate void StateWindow(WindowState state);
        public static StateWindow changeStateWindow;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            //ButtonNameFCN = "123";
            //LogMessage.LogDebugMessage(1, "Start Application");
            LogMessage.WriteToDebugViewer(0, string.Format("Start Application...."));

            mainWindow = this;
            master = new Master(this);
            outputLogView = new OutputLogView(this);
            m_staticView = new StatisticView(this);
            //master.m_SaveInspectImageThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Testing_Thread(true)));

            btn_enable_saveimage.IsChecked = Source.Application.Application.m_bEnableSavingOnlineImage;


            ContrucUIComponent();
            MappingImageDockToAvalonDock();
            StateChanged += delegate (object sender, EventArgs e)
            {
                if ((sender as Window) == null)
                    return;
                switch ((sender as Window).WindowState)
                {
                    case WindowState.Maximized:
                        changeStateWindow?.Invoke(WindowState.Maximized);
                        break;
                    case WindowState.Minimized:
                        changeStateWindow?.Invoke(WindowState.Minimized);
                        break;
                    case WindowState.Normal:
                        changeStateWindow?.Invoke(WindowState.Normal);
                        break;
                    default:
                        break;
                }
            };
            Deactivated += delegate (object sender, EventArgs e)
            {

            };

            //master.m_Tracks[0].m_imageViews[0].resultTeach.Children.Clear();
            //master.m_Tracks[0].m_imageViews[0].ClearOverlay();
            for (int nTrack = 0; nTrack < Source.Application.Application.m_nTrack; nTrack++)
                master.loadTeachImageToUI(nTrack);
            //Source.Application.Application.LoadTeachParam();

        }









        public bool bEnableOfflineInspection;

        private void btn_inspect_offline_Checked(object sender, RoutedEventArgs e)
        {

            bEnableOfflineInspection = (bool)inspect_offline_btn.IsChecked;
            //for (int n = 0; n < Application.m_nTrack; n++)
            //{
            //    master.RunOfflineSequenceThread(n);
            //}
            master.RunOfflineSequenceThread(activeImageDock.trackID);
            
            //master.RobotSequenceThread();

        }

        private void btn_inspect_offline_UnChecked(object sender, RoutedEventArgs e)
        {

            bEnableOfflineInspection = (bool)inspect_offline_btn.IsChecked;
        }

        public bool bEnableRunSequence = false;
        private void btn_run_sequence_Checked(object sender, RoutedEventArgs e)
        {
            Run_Sequence(activeImageDock.trackID);
        }
        public void Run_Sequence(int nTrack = (int)TRACK_TYPE.TRACK_ALL)
        {
            btn_run_sequence.IsChecked = true;
            bEnableRunSequence = (bool)btn_run_sequence.IsChecked;
            inspect_offline_btn.IsEnabled = false;
            m_staticView.ResetMappingResult(nTrack);
            m_staticView.ClearStatistic(nTrack);
            master.RunOnlineSequenceThread(nTrack);

            //master.RunOnlineSequenceThread(0);

            //if (nTrack == (int)TRACK_TYPE.TRACK_ALL)
            //{
            //    for (int n = 0; n < Application.m_nTrack; n++)
            //    {
            //        master.RunOnlineSequenceThread(n);
            //    }
            //}
            //else
            //    master.RunOnlineSequenceThread(nTrack);
            if(nTrack == 0)
                master.RobotSequenceThread();
            else
                master.RobotSequenceThread();

            //Master.commHIKRobot.CreateAndSendMessageToHIKRobot(SignalFromVision.Vision_Ready);
        }
        public void Stop_Sequence(int nTrack = (int)TRACK_TYPE.TRACK_ALL)
        {
            bEnableRunSequence = false;
            inspect_offline_btn.IsEnabled = true;
            btn_run_sequence.IsChecked = false;
        }

        private void btn_run_sequence_Unchecked(object sender, RoutedEventArgs e)
        {
            Stop_Sequence(activeImageDock.trackID);
        }




        public void InitBigDocPanel()
        {
            bigPanelGroup.Children.Add(imageZoomViewPane);
            imageZoomViewPane.DockWidth = new System.Windows.GridLength(screenWidth / 1.5);
            imageZoomViewPane.DockHeight = new System.Windows.GridLength(screenHeight / 1);
            imageZoomViewPane.DockMinHeight = screenHeight / 1;
            imageZoomViewPane.DockMinWidth = screenWidth / 2;
            //bigPanelGroup.Children.Add(new LayoutDocumentPane(new LayoutDocument()
            //{
            //    Content = outputLogView,
            //    CanMove = false,
            //    CanClose = false,
            //    CanFloat = false,
            //    Title = "Output Log"
            //}));

            imageZoomViewPane.Children.Add(bigDoc);
            bigDoc.Title = "Zoom Doc Panel";
            bigDoc.CanClose = false;
            bigDoc.CanFloat = false;
        }

        public void InitTeachDocument()
        {
            teachViewDoc = new LayoutDocument();
            teachViewDoc.CanClose = false;
            teachViewDoc.CanFloat = false;
            //teachViewDoc.CanMove = false;
            teachViewDoc.Title = "Camera View";
            teachViewDoc.ContentId = "Teach";
            // teachViewDoc.Content = master.teachView;
        }
        private void ContrucUIComponent()
        {
            InitBigDocPanel();

            // InitTeachDocument();

            m_layoutPanel = new LayoutPanel();
            mainPanelGroup = new LayoutDocumentPaneGroup();
            mainPanelGroup.Orientation = Orientation.Horizontal;
            outPutLogPaneGroup = new LayoutAnchorablePaneGroup();
            outPutLogPaneGroup.Orientation = Orientation.Vertical;
            m_MappingPaneGroup = new LayoutAnchorablePaneGroup();
            m_MappingPaneGroup.Orientation = Orientation.Vertical;

            int numTrack, num_Doc, total_doc;
            numTrack = Magnus_WPF_1.Source.Application.Application.m_nTrack;
            num_Doc = Magnus_WPF_1.Source.Application.Application.m_nDoc;
            total_doc = num_Doc * numTrack;// Application.Application.total_doc;

            #region Image Layout
            imagesViewPaneGroup = new LayoutDocumentPaneGroup();
            imagesViewPaneGroup.Orientation = Orientation.Horizontal;
            mainPanelGroup.Children.Add(imagesViewPaneGroup);

            imagesViewPane = new LayoutDocumentPane[numTrack];
            imagesViewDoc = new LayoutDocument[total_doc];

            for (int track_index = 0; track_index < numTrack; track_index++)
            {
                imagesViewPane[track_index] = new LayoutDocumentPane();
                imagesViewPane[track_index].CanRepositionItems = true;
                imagesViewPaneGroup.Children.Add(imagesViewPane[track_index]);
                for (int doc_index = 0; doc_index < num_Doc; doc_index++)
                {
                    imagesViewDoc[track_index * num_Doc + doc_index] = new LayoutDocument
                    {
                        Title = titles[track_index * num_Doc + doc_index],
                        Content = master.m_Tracks[track_index].m_imageViews[doc_index],
                        ContentId = "N/A ",
                        CanFloat = true,
                        CanClose = false,
                        //CanMove = false,
                    };
                    imagesViewPane[track_index].Children.Add(imagesViewDoc[track_index * num_Doc + doc_index]);
                }
            }
            #endregion
            // grd_Status_Offline.Children.Add(statusUC);
            // statusUC.UpdateStatus("IDLE");

            #region Output Log Contruction
            outPutLogViewDoc = new LayoutAnchorable();
            outPutLogViewDoc.Title = "Output Log View";
            outPutLogViewDoc.Content = outputLogView;
            outPutLogViewDoc.ContentId = "";

            outPutLogViewDoc.CanClose = false;
            outPutLogViewDoc.CanHide = true;
            outPutLogViewDoc.AutoHideMinWidth = screenWidth / 1;

            outPutLogViewPane = new LayoutAnchorablePane();
            outPutLogPaneGroup.Children.Add(outPutLogViewPane);

            outPutLogPaneGroup.DockWidth = new System.Windows.GridLength(screenWidth / 4);
            outPutLogPaneGroup.DockHeight = new System.Windows.GridLength(screenHeight / 5);
            outPutLogPaneGroup.DockMinHeight = screenHeight / 5;
            outPutLogPaneGroup.DockMinWidth = screenWidth / 10;
            outPutLogViewPane.Children.Add(outPutLogViewDoc);

            //m_layOut_OutPutLog = new LayoutPanel();
            //m_layOut_OutPutLog.Children.Add(outPutLogPaneGroup);
            //layout_OutputLog = new LayoutRoot();
            //layout_OutputLog.RootPanel = m_layOut_OutPutLog;
            #endregion


            #region Statistic Contruction
            m_MappingViewDoc = new LayoutAnchorable();
            m_MappingViewDoc.Title = "Mapping Results";
            m_MappingViewDoc.Content = m_staticView;
            m_MappingViewDoc.ContentId = "";

            m_MappingViewDoc.CanClose = false;
            m_MappingViewDoc.CanHide = false;
            m_MappingViewDoc.AutoHideMinWidth = screenWidth / 1;

            m_MappingViewPane = new LayoutAnchorablePane();
            m_MappingPaneGroup.Children.Add(m_MappingViewPane);

            m_MappingPaneGroup.DockWidth = new System.Windows.GridLength(screenWidth);
            m_MappingPaneGroup.DockHeight = new System.Windows.GridLength(screenHeight / 6);
            m_MappingPaneGroup.DockMinHeight = screenHeight / 10;
            m_MappingPaneGroup.DockMinWidth = screenWidth / 10;
            m_MappingViewPane.Children.Add(m_MappingViewDoc);

            //m_layOut_Mapping = new LayoutPanel();
            //m_layOut_Mapping.Children.Add(m_MappingPaneGroup);
            //layout_Mapping = new LayoutRoot();
            //layout_Mapping.RootPanel = m_layOut_Mapping;
            #endregion


            #region Show UI
            m_layout = new LayoutPanel();
            m_layout.Orientation = Orientation.Horizontal;
            m_layout.Children.Add(mainPanelGroup);
            m_layout.Children.Add(outPutLogPaneGroup);
            
            m_layoutPanel.Orientation = Orientation.Vertical;
            m_layoutPanel.Children.Add(m_layout);
            m_layoutPanel.Children.Add(m_MappingPaneGroup);
            
            layoutHPSF = new LayoutRoot();
            layoutHPSF.RootPanel = m_layoutPanel;
            #endregion
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }

        public void MappingImageDockToAvalonDock()
        {
            int z = 0;
            activeImageDock = master.m_Tracks[0].m_imageViews[0];
            for (int itrack = 0; itrack < Magnus_WPF_1.Source.Application.Application.m_nTrack; itrack++)
            {
                for (int j = 0; j < Magnus_WPF_1.Source.Application.Application.m_nDoc; j++)
                {
                    imagesViewDoc[z].Content = master.m_Tracks[itrack].m_imageViews[j];
                    UpdateTitleDoc(z, "");
                    z++;
                }
            }
        }
        public void UpdateTitleDoc(int docIdx, string name, bool isLoadTeachImage = false)
        {
            if (isLoadTeachImage)
            {
                this.Dispatcher.Invoke(() =>
                {
                    master.m_Tracks[docIdx].ClearOverLay();
                    imagesViewDoc[docIdx].ContentId = name;
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    imagesViewDoc[docIdx].ContentId = name;
                });
            }
        }

        bool isOneSpecificDocState = false;
        public void ZoomDocPanel(int trackID)
        {

            if (!isOneSpecificDocState)
            {
                oldPanelGroup = mainPanelGroup;
                imageZoomViewPane.ReplaceChild(bigDoc, imagesViewDoc[trackID]);
                imagesViewDoc[trackID].CanClose = false;
                imagesViewDoc[trackID].CanFloat = false;
                m_layoutPanel.ReplaceChildAt(0, bigPanelGroup);
                m_layoutPanel.RemoveChildAt(1);
                //m_layoutPanel.Orientation = Orientation.Horizontal;
            }
            else
            {
                mainPanelGroup = oldPanelGroup;
                imagesViewDoc[trackID].CanClose = false;
                imagesViewDoc[trackID].CanFloat = false;
                imageZoomViewPane.ReplaceChild(imagesViewDoc[trackID], bigDoc);
                imagesViewPane[trackID].Children.Add(imagesViewDoc[trackID]);
                m_layoutPanel.ReplaceChildAt(0, m_layout);
                m_layoutPanel.Children.Add(m_MappingPaneGroup);
            }
            isOneSpecificDocState = !isOneSpecificDocState;
            child_PreviewMouseRightButtonDown(activeImageDock, null);
        }



        private void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImageView im = sender as ImageView;
            int track = im.trackID;

            for (int i = 0; i < Magnus_WPF_1.Source.Application.Application.m_nTrack; i++)
            {
                im.transform.Reset(1);
            }
        }




        private void TabablzControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Inspect_Checked(object sender, RoutedEventArgs e)
        {

        }

        public bool bEnableGrabCycle = false;
        private void btn_stream_camera_Checked(object sender, RoutedEventArgs e)
        {
            bEnableGrabCycle = (bool)stream_camera_btn.IsChecked;

            if (master.thread_StreamCamera[activeImageDock.trackID] == null)
            {
                master.thread_StreamCamera[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.func_GrabImageThread()));
                master.thread_StreamCamera[activeImageDock.trackID].Start();
            }
            else if (!master.thread_StreamCamera[activeImageDock.trackID].IsAlive)
            {
                master.thread_StreamCamera[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.func_GrabImageThread()));
                master.thread_StreamCamera[activeImageDock.trackID].Start();
            }
        }

        private void btn_stream_camera_UnChecked(object sender, RoutedEventArgs e)
        {
            bEnableGrabCycle = (bool)stream_camera_btn.IsChecked;

        }

        private void btn_Camera_Setting_Checked(object sender, RoutedEventArgs e)
        {
            bool bEnable = (bool)camera_setting_btn.IsChecked;

            int currentTabIndex = tab_controls.SelectedIndex;
            //tt_DialogSettings.X = 0;
            //tt_DialogSettings.Y = 0;

            grd_PopupDialog.Children.Clear();
            if(master.m_Tracks[0].hIKControlCameraView != null)
                grd_PopupDialog.Children.Add(master.m_Tracks[0].hIKControlCameraView);

            grd_Dialog_Settings.Margin = new Thickness(0, 160, 0, 0);
            grd_Dialog_Settings.VerticalAlignment = VerticalAlignment.Top;
            grd_Dialog_Settings.HorizontalAlignment = HorizontalAlignment.Left;
            //master.teachParameter.Width = 300;
            //master.teachParameter.Height = 600;
            //master.m_Tracks[0].m_cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autofocus, 1);

            //grd_PopupDialog.Children.Add(master.teachParameter);
            //grd_PopupDialog.Children.Add(master.m_Tracks[]);
            //tab_controls.SelectedIndex = currentTabIndex;
            grd_Dialog_Settings.Visibility = Visibility.Visible;


        }

        private void btn_Camera_Setting_Unchecked(object sender, RoutedEventArgs e)
        {
            bool bEnable = (bool)camera_setting_btn.IsChecked;
            grd_PopupDialog.Children.Clear();

        }

        private void PreviewMouseDownInspectionBtn(object sender, MouseButtonEventArgs e)
        {

        }

        private void btn_Inspect_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void dockManager_ActiveContentChanged(object sender, EventArgs e)
        {

        }

        public void UpdateGrayValue(int trackID, string pos, string valueGray)
        {
            master.m_Tracks[trackID].m_imageViews[0].tbl_Pos.Text = pos;
            master.m_Tracks[trackID].m_imageViews[0].tbl_Value.Text = "[None]";
            master.m_Tracks[trackID].m_imageViews[0].tbl_Value_gray.Text = valueGray;
        }
        public void UpdateRGBValue(string pos, string valueRGB, string valueGray)
        {
            master.m_Tracks[0].m_imageViews[0].tbl_Pos.Text = pos;
            master.m_Tracks[0].m_imageViews[0].tbl_Value.Text = valueRGB;
            master.m_Tracks[0].m_imageViews[0].tbl_Value_gray.Text = valueGray;
        }
        private void btn_save_current_image_Click(object sender, RoutedEventArgs e)
        {
            //var result = MessageBox.Show("Do you want to save as teach image ?", "Save as Teach Image", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if (result == MessageBoxResult.Yes)
            //{
            master.SaveUIImage(activeImageDock.trackID);
            //master.m_Tracks[0].m_imageViews[0].SaveTeachImage(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_1.bmp"));
            //}

        }

        string m_strSelectionFolderFilePath = "";
        private void btn_load_image_File_Click(object sender, RoutedEventArgs e)
        {
            if (m_strSelectionFolderFilePath == @"C:\" || m_strSelectionFolderFilePath == "")
                m_strSelectionFolderFilePath = Path.Combine(Application.pathImageSave, "UI Image");
            // Set the initial directory for the dialog box
            System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog();

            FileDialog.FileName = m_strSelectionFolderFilePath;

            // Display the dialog box and wait for the user's response
            System.Windows.Forms.DialogResult result = FileDialog.ShowDialog();

            // If the user clicked the OK button, open the selected folder
            if ((int)result == 1)
            {
                // Get the path of the selected folder
                m_strSelectionFolderFilePath = FileDialog.FileName;

                master.loadImageFromFileToUI(activeImageDock.trackID, FileDialog.FileName);
                // Open the folder using a DirectoryInfo or other appropriate method
                // ...
            }
            else
                return;


        }

        private void Btn_load_teach_image_Click(object sender, RoutedEventArgs e)
        {
            master.loadTeachImageToUI(activeImageDock.trackID);
        }

        //public bool bEnableSingleSnapImages = true;
        private void btn_inspect_Click(object sender, RoutedEventArgs e)
        {

            //if (bEnableSingleSnapImages)
            //    bEnableSingleSnapImages = false;

            //if (m_bEnableDebug)
            //{
            //    InspectionCore.LoadImageToInspection(master.m_Tracks[activeImageDock.trackID].m_imageViews[0].btmSource);
            //    master.m_Tracks[activeImageDock.trackID].Inspect();
            //    UpdateDebugInfor();
            //    return;
            //}

            Master.InspectEvent[activeImageDock.trackID].Set();

            //if (master.thread_FullSequence[activeImageDock.trackID] == null)
            //{
            //    master.thread_FullSequence[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Thread(true)));
            //    master.thread_FullSequence[activeImageDock.trackID].Start();
            //}
            //else if (!master.thread_FullSequence[activeImageDock.trackID].IsAlive)
            //{
            //    master.thread_FullSequence[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Thread(true)));
            //    master.thread_FullSequence[activeImageDock.trackID].Start();
            //}

            //mainWindow.statisticView.UpdateValueStatistic(master.m_Tracks[0].m_nResult);
            //inspect_btn.IsEnabled = bEnableGrabImages;
        }


        //HIKControlCameraView cameraView = new HIKControlCameraView();
        private void btn_teach_parameters_Checked(object sender, RoutedEventArgs e)
        {
            teach_parameters_btn.IsChecked = true;
            //_BEnableSavingOnlineImage = !_BEnableSavingOnlineImage;
            int currentTabIndex = tab_controls.SelectedIndex;
            tt_DialogSettings.X = 0;
            tt_DialogSettings.Y = 0;

            grd_PopupDialog.Children.Clear();
            //DisableGroupButtonSettings();
            //DisbleDialogTeachParameter();
            //DialogUCHeight = master.inspectionParameter.Height;
            //DialogUCWidth = master.inspectionParameter.Width;
            grd_Dialog_Settings.Margin = new Thickness(0, 160, 0, 0);
            grd_Dialog_Settings.VerticalAlignment = VerticalAlignment.Top;
            grd_Dialog_Settings.HorizontalAlignment = HorizontalAlignment.Left;
            master.teachParameter.Width = 300;
            master.teachParameter.Height = 600;
            //master.m_Tracks[0].m_cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autofocus, 1);
            master.teachParameter.track_ComboBox.SelectedIndex = activeImageDock.trackID;
            master.teachParameter.ReloadParameterUI(activeImageDock.trackID);
            grd_PopupDialog.Children.Add(master.teachParameter);
            //grd_PopupDialog.Children.Add(master.m_Tracks[]);
            tab_controls.SelectedIndex = currentTabIndex;
            grd_Dialog_Settings.Visibility = Visibility.Visible;

        }

        private void btn_teach_parameters_Unchecked(object sender, RoutedEventArgs e)
        {
            //master.m_Tracks[0].m_cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autofocus, 0);

            teach_parameters_btn.IsChecked = false;
            grd_PopupDialog.Children.Clear();
            //DisbleButtonStandard();
        }

        #region DRAG DIALOG SETTING
        private void grd_Dialog_Settings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            grd_Dialog_Settings.ReleaseMouseCapture();
        }
        private void grd_Dialog_Settings_MouseMove(object sender, MouseEventArgs e)
        {
            if (grd_Dialog_Settings.IsMouseCaptured)
            {
                System.Windows.Vector offset = Point.Subtract(e.GetPosition(this), _startPositionDlg);
                tt_DialogSettings.X = _startOffsetPositionDlg.X + offset.X;
                tt_DialogSettings.Y = _startOffsetPositionDlg.Y + offset.Y;
            }
        }
        private void grd_Dialog_Settings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPositionDlg = e.GetPosition(this);
            if (_startPositionDlg.X != 0 && _startPositionDlg.Y != 0)
            {
                _startOffsetPositionDlg = new System.Windows.Vector(tt_DialogSettings.X, tt_DialogSettings.Y);
                grd_Dialog_Settings.CaptureMouse();
            }
        }
        #endregion

        private void GridSettingChangedSize(object sender, SizeChangedEventArgs e)
        {
            //if (grd_PopupDialog.Children.Contains(Source.Application.Application.loginUser) &&
            //     Source.Application.Application.loginUser.userName.Foreground == System.Windows.Media.Brushes.Gray)
            //{
            //    Source.Application.Application.loginUser.userName.Focus();
            //}
        }

        //int nCurrentTeachingStep = -1;
        private void btn_abort_teach_Click(object sender, RoutedEventArgs e)
        {
            //if (Master.m_bIsTeaching)
            //    Master.m_NextStepTeachEvent.Set();
            //nCurrentTeachingStep = -1;
            Master.m_bIsTeaching = false;
            SetDisableTeachButton();
            for (int nTrack = 0; nTrack < Application.m_nTrack; nTrack++)
            {
                master.loadTeachImageToUI(nTrack);
                master.m_Tracks[nTrack].m_imageViews[0].resultTeach.Children.Clear();
                master.m_Tracks[nTrack].m_imageViews[0].ClearOverlay();
                master.m_Tracks[nTrack].m_imageViews[0].controlWin.Visibility = Visibility.Collapsed;
            }



        }

        private void btn_next_teach_click(object sender, RoutedEventArgs e)
        {
            if (Master.m_bIsTeaching)
                Master.m_NextStepTeachEvent.Set();

        }


        private void btn_teach_click(object sender, RoutedEventArgs e)
        {
            if (Master.m_bIsTeaching)
                return;

            SetEnableTeachButton();
            //nCurrentTeachingStep = 0;
            //master.m_Tracks[0].m_imageViews[0].Teach(nCurrentTeachingStep);
            master.TeachThread();

        }

        public void SetEnableTeachButton()
        {
            btn_next_teach.IsEnabled = true;
            btn_abort_teach.IsEnabled = true;
            inspect_btn.IsEnabled = false;
            load_teach_image_btn.IsEnabled = false;
            stream_camera_btn.IsEnabled = false;
            teach_parameters_btn.IsEnabled = false;
            btn_save_teach_image.IsEnabled = false;

        }

        public void SetDisableTeachButton()
        {
            btn_next_teach.IsEnabled = false;
            btn_abort_teach.IsEnabled = false;
            inspect_btn.IsEnabled = true;
            load_teach_image_btn.IsEnabled = true;
            stream_camera_btn.IsEnabled = true;
            teach_parameters_btn.IsEnabled = true;
            btn_save_teach_image.IsEnabled = true;
        }

        private void btn_save_teach_image_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to save as teach image ?", "Save as Teach Image", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                master.SaveUITeachImage(activeImageDock.trackID);
                //master.m_Tracks[0].m_imageViews[0].SaveTeachImage(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_1.bmp"));
            }

        }

        private void btn_mapping_parameters_Unchecked(object sender, RoutedEventArgs e)
        {
            mapping_parameters_btn.IsChecked = false;
            grd_PopupDialog.Children.Clear();

            if (m_nDeviceX != Source.Application.Application.categoriesMappingParam.M_NumberDeviceX || m_nDeviceY != Source.Application.Application.categoriesMappingParam.M_NumberDeviceY)
            {
                master.LoadRecipe();
                m_staticView.InitCanvasMapping();
            }

            //DisbleButtonStandard();
        }

        private void btn_mapping_parameters_Checked(object sender, RoutedEventArgs e)
        {
            mapping_parameters_btn.IsChecked = true;

            int currentTabIndex = tab_controls.SelectedIndex;
            tt_DialogSettings.X = 0;
            tt_DialogSettings.Y = 0;

            grd_PopupDialog.Children.Clear();
            grd_Dialog_Settings.Margin = new Thickness(0, 160, 0, 0);
            grd_Dialog_Settings.VerticalAlignment = VerticalAlignment.Top;
            grd_Dialog_Settings.HorizontalAlignment = HorizontalAlignment.Left;
            master.mappingParameter.Width = 300;
            master.mappingParameter.Height = 300;

            grd_PopupDialog.Children.Add(master.mappingParameter);
            tab_controls.SelectedIndex = currentTabIndex;
            grd_Dialog_Settings.Visibility = Visibility.Visible;

        }

        private void btn_enable_saveimage_Unchecked(object sender, RoutedEventArgs e)
        {
            //master.applications.m_bEnableSavingOnlineImage = (bool) enable_saveimage_btn.IsChecked;
            Source.Application.Application.m_bEnableSavingOnlineImage = (bool)btn_enable_saveimage.IsChecked;
        }

        private void btn_enable_saveimage_Checked(object sender, RoutedEventArgs e)
        {
            //master.applications.m_bEnableSavingOnlineImage = (bool)enable_saveimage_btn.IsChecked;
            Source.Application.Application.m_bEnableSavingOnlineImage = (bool)btn_enable_saveimage.IsChecked;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            master.m_hiWinRobotInterface.m_hikThread.Interrupt();
            master.m_hiWinRobotInterface.m_hikThread.Abort();
            master.DeleteMaster();
            mainWindow = null;
            master = null;

        }

        private void btn_online_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void tbl_Recipe_Name_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isMouseOver = (bool)e.NewValue;
            if (!isMouseOver)
                return;
            TextBlock textBlock = (TextBlock)sender;
            ((ToolTip)textBlock.ToolTip).Visibility =
                 Visibility.Visible;
            string pathRecipe = Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe);
            tbl_recipe_name_tooltip.Text = pathRecipe;
        }
        private void tbl_Recipe_ID_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isMouseOver = (bool)e.NewValue;
            if (!isMouseOver)
                return;
            TextBlock textBlock = (TextBlock)sender;
            ((ToolTip)textBlock.ToolTip).Visibility =
                 Visibility.Visible;
            string pathRecipe = Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe);
        }


        public static bool isRobotControllerOpen = false;
        private void btn_Clear_Comm_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.outputLogView.ClearOutputLog();
        }


        //private void btn_CommLog_Checked(object sender, RoutedEventArgs e)
        //{
        //    isRobotControllerOpen = (bool)btn_CommLog.IsChecked;
        //    master.OpenHiwinRobotDialog(isRobotControllerOpen);
        //}

        //private void btn_CommLog_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    isRobotControllerOpen = (bool)btn_CommLog.IsChecked;
        //    master.OpenHiwinRobotDialog(isRobotControllerOpen);
        //}



        private void btn_LogIn_Unchecked(object sender, RoutedEventArgs e)
        {
            //Source.Application.Application.loginUser.PreviewKeyDown -= Source.Application.Application.loginUser.KeyShortcut;
            //tabItem_Production.IsSelected = true;
        }
        private void btn_LogIn_Checked(object sender, RoutedEventArgs e)
        {
            //Source.Application.Application.loginUser.AssignMainWindow();
            //tt_DialogSettings.X = 0;
            //tt_DialogSettings.Y = 0;
            //btnLogIn.IsEnabled = false;
            //grd_PopupDialog.Children.Clear();
            //DisableDialogLogin();
            //grd_PopupDialog.Children.Add(Source.Application.Application.loginUser);
            //CleanHotKey();

            //Source.Application.Application.loginUser.PreviewKeyDown += Source.Application.Application.loginUser.KeyShortcut;
            //foreach (COMMAND_CODE cmd in Master.cmdCode)
            //{
            //    if (cmd != COMMAND_CODE.IDLE)
            //    {
            //        Panel.SetZIndex(Application.Application.loginUser.panelCreateUser, 2);
            //        grd_Dialog_Settings.Margin = new Thickness(0, 0, 0, 0);
            //        DialogUCHeight = Source.Application.Application.loginUser.Height;
            //        DialogUCWidth = Source.Application.Application.loginUser.Width;
            //        grd_Dialog_Settings.VerticalAlignment = VerticalAlignment.Center;
            //        grd_Dialog_Settings.HorizontalAlignment = HorizontalAlignment.Center;
            //        return;
            //    }
            //}
            //Panel.SetZIndex(Source.Application.Application.loginUser.panelLogIn, 3);
            //DialogUCHeight = Source.Application.Application.loginUser.Height;
            //DialogUCWidth = Source.Application.Application.loginUser.Width;
            //grd_Dialog_Settings.VerticalAlignment = VerticalAlignment.Center;
            //grd_Dialog_Settings.HorizontalAlignment = HorizontalAlignment.Center;

        }

        public void btn_Online_click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_MinimizeApplicaton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btn_CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            //for (int indextrack = 0; indextrack < Application.Application.num_track; indextrack++)
            //{
            //    if (master.trackSF[indextrack].isAvailable == false)
            //        continue;
            //    master.trackSF[indextrack].camera.ReleaseCamera();
            //}
            //master.ReleaseAllThread();
            //master.applications.KillCurrentProcess();
        }

         public void AddLineOutputLog(string text, int nStyle = (int)ERROR_CODE.PASS)
        {
            if (outputLogView == null)
                return;
            outputLogView.AddLineOutputLog(text, nStyle);
        }

        #region PIXEL RULER
        private void btn_Pixel_Ruler_Click(object sender, RoutedEventArgs e)
        {
            popupRuler.IsOpen = !popupRuler.IsOpen;
            btn_Pixel_Ruler.IsChecked = popupRuler.IsOpen;
            if (!popupRuler.IsOpen)
            {
                pixelRuler.Finish();
                return;
            }
            Grid[] tempGrid = new Grid[Source.Application.Application.m_nTrack];
            for (int index_track = 0; index_track < Source.Application.Application.m_nTrack; index_track++)
            {
                tempGrid[index_track] = master.m_Tracks[index_track].m_imageViews[0].grd_Dock;
            }

            pixelRuler.SetUp(tempGrid, true);
        }
        #endregion

        bool m_bBinarizeStatus = false;
        private void btn_Binarize_Click(object sender, RoutedEventArgs e)
        {
            if (activeImageDock == null)
                return;
            m_bBinarizeStatus = !m_bBinarizeStatus;
            btn_Binarize.IsChecked = m_bBinarizeStatus;
            if (m_bBinarizeStatus)
                btn_Binarize_On();
            else btn_Binarize_Off();

        }
        private void btn_Binarize_On()
        {

            int trackID = activeImageDock.trackID;
            if (trackID == 0 || trackID == 1)
            {
                master.m_Tracks[trackID].m_imageViews[0].ClearOverlay();
                master.m_Tracks[trackID].m_imageViews[0].ClearText();
                master.m_Tracks[trackID].m_imageViews[0].enableGray = 2;
                master.m_Tracks[trackID].m_imageViews[0].panelSliderGray.Visibility = Visibility.Visible;
                master.m_Tracks[trackID].m_imageViews[0].UpdateSourceSliderChangeValue();
            }
        }

        public void btn_Binarize_Off()
        {
            int trackID = activeImageDock.trackID;
            if (trackID == 0 || trackID == 1)
            {
                master.m_Tracks[trackID].m_imageViews[0].enableGray = 0;
                master.m_Tracks[trackID].m_imageViews[0].panelSliderGray.Visibility = Visibility.Collapsed;
                master.m_Tracks[trackID].m_imageViews[0].image.Source = master.m_Tracks[trackID].m_imageViews[0].btmSource;
            }
        }

        private void Binarize_MouseLeave(object sender, MouseEventArgs e)
        {
            popup_uc.Visibility = Visibility.Collapsed;
            popup_uc.IsOpen = false;
        }
        private void Binarize_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Header.PopupText == null)
                return;

            Header.Margin = new Thickness(30, 0, 0, 0);
            Header.PopupText.HorizontalAlignment = HorizontalAlignment.Center;
            Header.PopupText.VerticalAlignment = VerticalAlignment.Center;
            popup_uc.PlacementTarget = btn_Binarize;
            popup_uc.IsOpen = true;
            Header.PopupText.Text = "Ctrl+B";
        }


        #region Grid Defect Setting

        public bool m_bEnableDebug;
        private void btn_debug_Checked(object sender, RoutedEventArgs e)
        {

            m_bEnableDebug = (bool)debug_btn.IsChecked;
            if (master.m_Tracks[activeImageDock.trackID].m_imageViews[0].btmSource.Width < 0)
                return;


            defectInfor.m_TrackDebugging = activeImageDock.trackID;
            master.m_Tracks[activeImageDock.trackID].m_InspectionCore.LoadImageToInspection(master.m_Tracks[activeImageDock.trackID].m_imageViews[0].btmSource);
            master.m_Tracks[activeImageDock.trackID].DebugFunction(ref master.m_Tracks[activeImageDock.trackID]);

            UpdateDebugInfor();
            return;
        }

        private void btn_debug_Unchecked(object sender, RoutedEventArgs e)
        {

            m_bEnableDebug = (bool)debug_btn.IsChecked;
            grd_Defect_Settings.Visibility = Visibility.Collapsed;

        }


        GridView gridView = new GridView();
        //private string _color_portReceive;

        public void UpdateDebugInfor()
        {
            if (!m_bEnableDebug)
            {
                grd_Defect_Settings.Visibility = Visibility.Collapsed;
                return;
            }
                //defectInfor.lvDefect.View = gridView;
                defectInfor.lvDefect.ItemsSource = null;
                defectInfor.lvDefect.ItemsSource = master.m_Tracks[activeImageDock.trackID].m_StepDebugInfors;
                DialogDefectHeight = defectInfor.Height;
                DialogDefectWidth = defectInfor.Width;

                grd_Defect.Children.Clear();
                grd_Defect.Children.Add(defectInfor);
                grd_Defect_Settings.Visibility = Visibility.Visible;

        }

        private void grd_Defect_Settings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPositionDlg = e.GetPosition(this);
            if (_startPositionDlg.X != 0 && _startPositionDlg.Y != 0)
            {
                _startOffsetPositionDlg = new Vector(tt_DefectSettings.X, tt_DefectSettings.Y);
                grd_Defect_Settings.CaptureMouse();
            }
        }

        private void grd_Defect_Settings_MouseMove(object sender, MouseEventArgs e)
        {
            if (grd_Defect_Settings.IsMouseCaptured)
            {
                Vector offset = Point.Subtract(e.GetPosition(this), _startPositionDlg);
                tt_DefectSettings.X = _startOffsetPositionDlg.X + offset.X;
                tt_DefectSettings.Y = _startOffsetPositionDlg.Y + offset.Y;
            }
        }

        private void grd_Defect_Settings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            grd_Defect_Settings.ReleaseMouseCapture();
        }

        #endregion

        private void btn_Robot_Controller_Checked(object sender, RoutedEventArgs e)
        {
            isRobotControllerOpen = (bool)btn_Robot_Controller.IsChecked;
            master.OpenHiwinRobotDialog(isRobotControllerOpen);
        }

        private void btn_Robot_Controller_Unchecked(object sender, RoutedEventArgs e)
        {
            isRobotControllerOpen = (bool)btn_Robot_Controller.IsChecked;
            master.OpenHiwinRobotDialog(isRobotControllerOpen);
        }

        private void btn_BarCodeReader_Setting_Checked(object sender, RoutedEventArgs e)
        {
            //master.m_BarcodeReader.sendCommandToAllReaders("LON");

            //defectInfor.lvDefect.View = gridView;
            //DialogDefectWidth = defectInfor.Width;
            grd_Dialog_Settings.Margin = new Thickness(0, 160, 0, 0);
            grd_Dialog_Settings.VerticalAlignment = VerticalAlignment.Top;
            grd_Dialog_Settings.HorizontalAlignment = HorizontalAlignment.Left;
            grd_Dialog_Settings.Width = master.m_BarcodeReader.m_BarcodeReader.Width;
            grd_Dialog_Settings.Height = master.m_BarcodeReader.m_BarcodeReader.Height;
            grd_PopupDialog.Children.Clear();
            grd_PopupDialog.Children.Add(master.m_BarcodeReader.m_BarcodeReader);
            //grd_PopupDialog.Visibility = Visibility.Visible;
            grd_PopupDialog.Visibility = Visibility.Visible;
            grd_Dialog_Settings.Visibility = Visibility.Visible;

        }

        private void btn_BarCodeReader_Setting_Unchecked(object sender, RoutedEventArgs e)
        {
            //grd_Defect_Settings.Visibility = Visibility.Collapsed;
            grd_PopupDialog.Children.Clear();
        }


        public bool m_bEnableDebugSequence = false;
        private void btn_Debug_sequence_Checked(object sender, RoutedEventArgs e)
        {
            m_bEnableDebugSequence = (bool)btn_Debug_sequence.IsChecked;
            //btn_Debug_sequence_NextStep.IsEnabled = true;
        }

        private void btn_Debug_sequence_Unchecked(object sender, RoutedEventArgs e)
        {
            m_bEnableDebugSequence = (bool)btn_Debug_sequence.IsChecked;
            //btn_Debug_sequence_NextStep.IsEnabled = false;
        }


        private void btn_Debug_sequence_PreviousStep_Click(object sender, RoutedEventArgs e)
        {
            //master.m_bNextStepSequence = false;
            //if (m_bEnableDebugSequence)
            //    Master.m_NextStepSequenceEvent.Set();
        }

        private void btn_Debug_sequence_NextStep_Click(object sender, RoutedEventArgs e)
        {
            //master.m_bNextStepSequence = true;
            //if (m_bEnableDebugSequence)
            //    Master.m_NextStepSequenceEvent.Set();
        }

        int m_nPLCGridViewIndex = 0;
        private void btn_PLCCOMM_Setting_Checked(object sender, RoutedEventArgs e)
        {
            grd_Dialog_Settings.Margin = new Thickness(0, 160, 0, 0);
            grd_Dialog_Settings.VerticalAlignment = VerticalAlignment.Top;
            grd_Dialog_Settings.HorizontalAlignment = HorizontalAlignment.Left;
            grd_Dialog_Settings.Width = master.m_plcComm.Width;
            grd_Dialog_Settings.Height = master.m_plcComm.Height;

            m_nPLCGridViewIndex = grd_PopupDialog.Children.Count;
            grd_PopupDialog.Children.Clear();
            grd_PopupDialog.Children.Add(master.m_plcComm);
            grd_PopupDialog.Visibility = Visibility.Visible;
            grd_Dialog_Settings.Visibility = Visibility.Visible;
        }

        private void btn_PLCCOMM_Setting_Unchecked(object sender, RoutedEventArgs e)
        {
            //grd_Defect.Children.RemoveAt(m_nPLCGridViewIndex);
            //if(grd_Defect.Children.Count == 0)
            grd_PopupDialog.Children.Clear();
                //grd_Defect_Settings.Visibility = Visibility.Collapsed;

        }




        private void btn_Emergency_Stop_Click(object sender, RoutedEventArgs e)
        {
            master.m_EmergencyStatus_Simulate = (bool)btn_Emergency_Stop.IsChecked == false ? 0 : 1;
            Master.m_EmergencyStopSequenceEvent.Set();

        }

        private void btn_Imidiate_Stop_Click(object sender, RoutedEventArgs e)
        {
            master.m_ImidiateStatus_Simulate = (bool)btn_Imidiate_Stop.IsChecked == false ? 0 : 1;
            Master.m_EmergencyStopSequenceEvent.Set();
        }

        private void btn_Reset_Machine_Click(object sender, RoutedEventArgs e)
        {
            if(master.m_ResetMachineStatus_Simulate == 0)
                master.m_ResetMachineStatus_Simulate = 1;

            if (master.thread_RobotSequence == null)
            {
                master.thread_RobotSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.ResetSequence()));
                master.thread_RobotSequence.Start();
            }
            else if (!master.thread_RobotSequence.IsAlive)
            {
                master.thread_RobotSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.ResetSequence()));
                master.thread_RobotSequence.Start();
            }

        }

        public bool m_bShowOverlay = true;


        private void btn_ShowOverlay_Click(object sender, RoutedEventArgs e)
        {
            m_bShowOverlay = !m_bShowOverlay;
            ShowOverlay(m_bShowOverlay);
        }

        public void ShowOverlay(bool bShow)
        {

            if (activeImageDock == null) return;
            int trackID = activeImageDock.trackID;
            if (master == null)
                return;

            if (master.m_Tracks[trackID].m_imageViews[0].bufferImage == null)
                return;

            if (master.m_Tracks[trackID].m_bInspecting)
                return;

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (bShow)
                {
                    int nResult = master.m_Tracks[trackID].m_nResult[master.m_Tracks[trackID].m_CurrentSequenceDeviceID];
                    master.m_Tracks[trackID].DrawInspectionResult(ref nResult, ref master.m_Tracks[trackID].m_Center_Vision, ref master.m_Tracks[trackID].m_dDeltaAngleInspection);
                }
                else
                {
                    master.m_Tracks[trackID].m_imageViews[0].ClearOverlay();
                    master.m_Tracks[trackID].m_imageViews[0].ClearText();
                }


            });
        }


        public void PopupWarningMessageBox(string strDebugMessage, bool bIsEmergencyImidiateClicked, bool bIsNeedToAbort, bool bIsPopup = true)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {

                grd_Dialog_Settings.Margin = new Thickness(0, 160, 0, 0);
                grd_Dialog_Settings.VerticalAlignment = VerticalAlignment.Center;
                grd_Dialog_Settings.HorizontalAlignment = HorizontalAlignment.Center;
                grd_Dialog_Settings.Width = 600;
                grd_Dialog_Settings.Height = 300;

                if (bIsPopup)
                {
                    grd_popup_WarningMessageBox.Children.Clear();
                    m_WarningMessageBoxUC.updateMessageString(strDebugMessage, bIsEmergencyImidiateClicked, bIsNeedToAbort);
                    grd_popup_WarningMessageBox.Children.Add(m_WarningMessageBoxUC);
                    grd_popup_WarningMessageBox.Visibility = Visibility.Visible;
                    grd_Dialog_Settings.Visibility = Visibility.Visible;
                }
                else
                {
                    grd_popup_WarningMessageBox.Children.Clear();
                    grd_popup_WarningMessageBox.Visibility = Visibility.Collapsed;
                    grd_Dialog_Settings.Visibility = Visibility.Collapsed;

                }
            });
        }

    }
}
