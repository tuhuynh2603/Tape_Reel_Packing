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
        public DefectInfor defectInfor = new DefectInfor();
        public static string[] titles = new string[] { "Top View ", "Flap Side 1 ", "Flap Side 2 " };

        private int screenWidth = 2000;// System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        private int screenHeight = 2000;// System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

        private LayoutDocumentPaneGroup mainPanelGroup;
        public LayoutDocumentPaneGroup oldPanelGroup = new LayoutDocumentPaneGroup();

        private LayoutDocumentPaneGroup bigPanelGroup = new LayoutDocumentPaneGroup();
        private LayoutDocumentPane bigTagPanel = new LayoutDocumentPane();
        private LayoutDocument bigDoc = new LayoutDocument();

        private LayoutDocumentPaneGroup[] imagesViewPaneGroup;
        private LayoutDocumentPane[] imagesViewPane;
        private LayoutDocument[] imagesViewDoc;


        private LayoutAnchorablePaneGroup outPutLogPaneGroup;
        private LayoutAnchorablePane outPutLogViewPane;
        private LayoutAnchorable outPutLogViewDoc;

        private LayoutDocument teachViewDoc;
        public static ImageView activeImageDock;
        public static
        LayoutPanel m_layout;


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
        private string _portReceive;
        private string _portSent;
        private string _color_portReceive;
        private string _color_portSent;
        public string PortReceive
        {
            get { return _portReceive; }
            set
            {
                if (value != _portReceive)
                {
                    _portReceive = value;
                    OnPropertyChanged("PortReceive");
                }
            }
        }

        public string PortSent
        {
            get { return _portSent; }
            set
            {
                if (value != _portSent)
                {
                    _portSent = value;
                    OnPropertyChanged("PortSent");
                }
            }
        }
        // Color
        public string Color_PortReceive
        {
            get { return _color_portReceive; }
            set
            {
                if (value != _color_portReceive)
                {
                    _color_portReceive = value;
                    OnPropertyChanged("Color_PortReceive");
                }
            }
        }
        public string Color_PortSent
        {
            get { return _color_portSent; }
            set
            {
                if (value != _color_portSent)
                {
                    _color_portSent = value;
                    OnPropertyChanged("Color_PortSent");
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
            //master.m_SaveInspectImageThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Testing_Thread(true)));

            btn_enable_saveimage.IsChecked = Source.Application.Application.m_bEnableSavingOnlineImage;


            ContrucUIComponent();
            MappingImageDockToAvalonDock();
            InitCanvasMapping();
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


        public Point m_CanvasMovePoint = new Point(0, 0);
        int m_nWidthMappingRect = 100;
        int m_nStepMappingRect = 102;
        int m_nDeviceX = 5;
        int m_nDeviceY = 5;
        Image[] arr_imageMapping;
        Label[] arr_textBlockMapping;
        private void InitCanvasMapping()
        {
            m_nDeviceX = Source.Application.Application.categoriesMappingParam.M_NumberDeviceX;
            m_nDeviceY = Source.Application.Application.categoriesMappingParam.M_NumberDeviceY;
            int nMaxDeviceStep = m_nDeviceX > m_nDeviceY ? m_nDeviceX : m_nDeviceY;
            if (canvas_Mapping.Children != null)
            {
                //canvas_Mapping.Children.RemoveRange(0, canvas_Mapping.Children.Count);
                canvas_Mapping.Children.Clear();

            }

            m_nWidthMappingRect = 500 / nMaxDeviceStep;
            m_nStepMappingRect = m_nWidthMappingRect + 10 / nMaxDeviceStep;
            string path = @"/Resources/gray-chip.png";
            arr_imageMapping = new Image[m_nDeviceX * m_nDeviceY];
            arr_textBlockMapping = new Label[m_nDeviceX * m_nDeviceY];
            int nID = 0;
            for (int nDeviceX = 0; nDeviceX < m_nDeviceX; nDeviceX++)
            {
                for (int nDeviceY = 0; nDeviceY < m_nDeviceY; nDeviceY++)
                {
                    nID = nDeviceX + nDeviceY * m_nDeviceX;
                    //Canvas canvas_temp = new Canvas();
                    arr_imageMapping[nID] = new Image();
                    arr_imageMapping[nID].Source = new BitmapImage(new Uri(path, UriKind.Relative));
                    arr_imageMapping[nID].Width = 0.95 * m_nWidthMappingRect;
                    arr_imageMapping[nID].Height = 0.95 * m_nWidthMappingRect;
                    arr_textBlockMapping[nID] = new Label();
                    arr_textBlockMapping[nID].Content = (nDeviceX + 1 + nDeviceY * m_nDeviceX);
                    arr_textBlockMapping[nID].FontSize = 0.95 * m_nWidthMappingRect / 3;
                    arr_textBlockMapping[nID].MinWidth = 0.95 * m_nWidthMappingRect;
                    arr_textBlockMapping[nID].Foreground = new SolidColorBrush(Colors.Yellow);
                    arr_textBlockMapping[nID].HorizontalContentAlignment = HorizontalAlignment.Center;
                    Canvas.SetLeft(arr_imageMapping[nID], m_nStepMappingRect * nDeviceX);
                    Canvas.SetTop(arr_imageMapping[nID], m_nStepMappingRect * nDeviceY);
                    canvas_Mapping.Children.Add(arr_imageMapping[nID]);

                    Canvas.SetLeft(arr_textBlockMapping[nID], m_nStepMappingRect * nDeviceX);
                    Canvas.SetTop(arr_textBlockMapping[nID], m_nStepMappingRect * nDeviceY + arr_textBlockMapping[nID].FontSize / 3);
                    canvas_Mapping.Children.Add(arr_textBlockMapping[nID]);


                }
            }

            border_boundingbox_focus.Width = m_nWidthMappingRect;
            border_boundingbox_focus.Height = m_nWidthMappingRect;
            border_boundingbox_focus.BorderThickness = new Thickness(0);
            border_boundingbox_focus.BorderBrush = new SolidColorBrush(Colors.WhiteSmoke);
            Canvas.SetLeft(border_boundingbox_focus, 0);
            Canvas.SetTop(border_boundingbox_focus, 0);
            canvas_Mapping.Children.Add(border_boundingbox_focus);



            border_boundingbox_clicked.Width = m_nWidthMappingRect;
            border_boundingbox_clicked.Height = m_nWidthMappingRect;
            border_boundingbox_clicked.BorderThickness = new Thickness(0);
            border_boundingbox_clicked.BorderBrush = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(border_boundingbox_clicked, 0);
            Canvas.SetTop(border_boundingbox_clicked, 0);
            canvas_Mapping.Children.Add(border_boundingbox_clicked);

        }

        int nDeviceID = -1;



        public bool bEnableOfflineInspection;

        private void btn_inspect_offline_Checked(object sender, RoutedEventArgs e)
        {

            bEnableOfflineInspection = (bool)inspect_offline_btn.IsChecked;
            master.RunOfflineSequenceThread(activeImageDock.trackID);
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
            if (nTrack == (int)TRACK_TYPE.TRACK_ALL)
            {
                for (int n = 0; n < Application.m_nTrack; n++)
                {
                    master.RunSequenceThread(n);
                }
            }
            else
                master.RunSequenceThread(nTrack);


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


        private int Check_mapping_Cursor_ID(Point cur_point, bool bIsclicked)
        {
            int nIDX = (int)(cur_point.X / m_nStepMappingRect);
            int nIDY = (int)(cur_point.Y / m_nStepMappingRect);

            if (nDeviceID != nIDX + nIDY * m_nDeviceX)
            {
                nDeviceID = nIDX + nIDY * m_nDeviceX;
                if (nIDX < m_nDeviceX && nIDY < m_nDeviceY)
                {
                    Canvas.SetLeft(border_boundingbox_focus, m_nStepMappingRect * nIDX);
                    Canvas.SetTop(border_boundingbox_focus, m_nStepMappingRect * nIDY);
                    border_boundingbox_focus.BorderThickness = new Thickness(2);
                }
                else
                {
                    border_boundingbox_focus.BorderThickness = new Thickness(0);
                    //border_boundingbox_clicked.BorderThickness = new Thickness(0);

                }
            }

            if (bIsclicked)
            {
                Canvas.SetLeft(border_boundingbox_clicked, m_nStepMappingRect * nIDX);
                Canvas.SetTop(border_boundingbox_clicked, m_nStepMappingRect * nIDY);
                border_boundingbox_clicked.BorderThickness = new Thickness(2);
            }

            //canvas_Mapping.Children.RemoveAt(nIDX + nIDY * 10);
            return nIDX + nIDY * m_nDeviceX;

        }
        private void canvas_Mapping_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            m_CanvasMovePoint = e.GetPosition(canvas_Mapping);
            int nID = Check_mapping_Cursor_ID(m_CanvasMovePoint, true);
            master.m_Tracks[activeImageDock.trackID].m_nCurrentClickMappingID = nID;

            //if (m_bEnableDebug) {
            //    InspectionCore.LoadImageToInspection(master.m_Tracks[activeImageDock.trackID].m_imageViews[0].btmSource);
            //    master.m_Tracks[activeImageDock.trackID].Inspect();
            //    UpdateDebugInfor();
            //        return;
            //}

            master.m_Tracks[activeImageDock.trackID].CheckInspectionOnlineThread();
            if (bEnableRunSequence || bEnableOfflineInspection)
                Master.m_hardwareTriggerSnapEvent[activeImageDock.trackID].Set();
            else
                Master.InspectEvent[activeImageDock.trackID].Set();
        }

        private void canvas_Mapping_MouseMove(object sender, MouseEventArgs e)
        {
            m_CanvasMovePoint = e.GetPosition(canvas_Mapping);
            Check_mapping_Cursor_ID(m_CanvasMovePoint, false);
        }

        private void canvas_Mapping_MouseLeave(object sender, MouseEventArgs e)
        {
            border_boundingbox_focus.BorderThickness = new Thickness(0);
            nDeviceID = -1;
        }

        public void InitBigDocPanel()
        {
            bigPanelGroup.Children.Add(bigTagPanel);
            bigTagPanel.DockWidth = new System.Windows.GridLength(screenWidth / 1.5);
            bigTagPanel.DockHeight = new System.Windows.GridLength(screenHeight / 1);
            bigTagPanel.DockMinHeight = screenHeight / 1;
            bigTagPanel.DockMinWidth = screenWidth / 2;
            //bigPanelGroup.Children.Add(new LayoutDocumentPane(new LayoutDocument()
            //{
            //    Content = outputLogView,
            //    CanMove = false,
            //    CanClose = false,
            //    CanFloat = false,
            //    Title = "Output Log"
            //}));

            bigTagPanel.Children.Add(bigDoc);
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
            // mainPanelGroup.Orientation = Orientation.Horizontal;
            outPutLogPaneGroup = new LayoutAnchorablePaneGroup();
            outPutLogPaneGroup.Orientation = Orientation.Vertical;

            int numTrack, num_Doc, total_doc;
            numTrack = Magnus_WPF_1.Source.Application.Application.m_nTrack;
            num_Doc = Magnus_WPF_1.Source.Application.Application.m_nDoc;
            total_doc = num_Doc * numTrack;// Application.Application.total_doc;

            #region Image Layout
            imagesViewPaneGroup = new LayoutDocumentPaneGroup[total_doc];
            imagesViewPane = new LayoutDocumentPane[total_doc];
            imagesViewDoc = new LayoutDocument[total_doc];

            for (int i = 0; i < total_doc; i++)
            {
                imagesViewPaneGroup[i] = new LayoutDocumentPaneGroup();
                //imagesViewPaneGroup[i].Orientation = Orientation.Horizontal;
                mainPanelGroup.Children.Add(imagesViewPaneGroup[i]);
            }

            for (int track_index = 0; track_index < numTrack; track_index++)
            {
                for (int doc_index = 0; doc_index < num_Doc; doc_index++)
                {
                    imagesViewPane[track_index * num_Doc + doc_index] = new LayoutDocumentPane();
                    imagesViewPane[track_index * num_Doc + doc_index].CanRepositionItems = false;
                    //if (track_index == 0)
                    //    imagesViewPaneGroup[0].Children.Add(imagesViewPane[track_index * num_Doc + doc_index]);

                    //else
                    imagesViewPaneGroup[track_index].Children.Add(imagesViewPane[track_index * num_Doc + doc_index]);

                    imagesViewDoc[track_index * num_Doc + doc_index] = new LayoutDocument
                    {
                        Title = titles[track_index * num_Doc + doc_index],
                        Content = master.m_Tracks[track_index].m_imageViews[doc_index],
                        ContentId = "N/A ",
                        CanFloat = false,
                        CanClose = false,
                        //CanMove = false,
                    };
                    imagesViewPane[track_index * num_Doc + doc_index].Children.Add(imagesViewDoc[track_index * num_Doc + doc_index]);
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
            outPutLogViewDoc.CanHide = false;
            outPutLogViewDoc.AutoHideMinWidth = screenWidth / 1;

            outPutLogViewPane = new LayoutAnchorablePane();
            outPutLogPaneGroup.Children.Add(outPutLogViewPane);

            outPutLogPaneGroup.DockWidth = new System.Windows.GridLength(screenWidth / 8);
            outPutLogPaneGroup.DockHeight = new System.Windows.GridLength(screenHeight / 5);
            outPutLogPaneGroup.DockMinHeight = screenHeight / 5;
            outPutLogPaneGroup.DockMinWidth = screenWidth / 10;

            outPutLogViewPane.Children.Add(outPutLogViewDoc);
            #endregion

            #region Show UI
            m_layout = new LayoutPanel();
            m_layout.Orientation = Orientation.Vertical;
            m_layout.Children.Add(mainPanelGroup);
            m_layout.Children.Add(outPutLogPaneGroup);
            m_layoutPanel.Orientation = Orientation.Horizontal;
            m_layoutPanel.Children.Add(m_layout);

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
                bigTagPanel.ReplaceChild(bigDoc, imagesViewDoc[trackID]);
                imagesViewDoc[trackID].CanClose = false;
                imagesViewDoc[trackID].CanFloat = false;
                //imagesViewDoc[trackID].CanMove = false;

                m_layoutPanel.ReplaceChildAt(0, bigPanelGroup);
            }
            else
            {
                mainPanelGroup = oldPanelGroup;
                imagesViewDoc[trackID].CanClose = false;
                imagesViewDoc[trackID].CanFloat = false;
                //imagesViewDoc[trackID].CanMove = false;
                bigTagPanel.ReplaceChild(imagesViewDoc[trackID], bigDoc);
                imagesViewPane[trackID].Children.Add(imagesViewDoc[trackID]);
                m_layoutPanel.ReplaceChildAt(0, m_layout);
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
                master.thread_StreamCamera[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Thread()));
                master.thread_StreamCamera[activeImageDock.trackID].Start();
            }
            else if (!master.thread_StreamCamera[activeImageDock.trackID].IsAlive)
            {
                master.thread_StreamCamera[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Thread()));
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

        private void btn_load_teach_image_Click(object sender, RoutedEventArgs e)
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
            master.teachParameter.ReloadParameterUI(activeImageDock.trackID);
            grd_PopupDialog.Children.Add(master.teachParameter);
            //grd_PopupDialog.Children.Add(master.m_Tracks[]);
            tab_controls.SelectedIndex = currentTabIndex;

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
                InitCanvasMapping();
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

        public void UpdateMappingResult(int nID, int nResult)
        {
            string path = @"/Resources/green-chip.png";

            if (nResult < 0)
                path = @"/Resources/red-chip.png";

            arr_imageMapping[nID].Source = new BitmapImage(new Uri(path, UriKind.Relative));
        }
        public void ResetMappingResult(int nTrackID = (int)TRACK_TYPE.TRACK_CAM1)
        {
            string path = @"/Resources/gray-chip.png";
            for (int nID = 0; nID < arr_imageMapping.Length; nID++)
                arr_imageMapping[nID].Source = new BitmapImage(new Uri(path, UriKind.Relative));
        }

        public void ResetStatisticResult(int nTrackID = (int)TRACK_TYPE.TRACK_CAM1)
        {
            statisticView.ClearStatistic(nTrackID);
        }

        internal void UpdateStatisticResult(int nResult)
        {
            statisticView.UpdateValueStatistic(nResult);

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            master.DeleteMaster();
            mainWindow = null;
            master = null;
            //mainPanelGroup = null;
            //oldPanelGroup = null;
            //bigPanelGroup = null;
            //bigTagPanel = null;
            //bigDoc = null;
            //imagesViewPaneGroup = null;
            //imagesViewPane = null;
            //imagesViewDoc = null;
            //teachViewDoc = null;
            //activeImageDock = null;
            //layoutRoot = null;
            //_layoutHPSF = null;

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


        public static bool isOpenCommLog = false;
        private void btn_Clear_Comm_Click(object sender, RoutedEventArgs e)
        {
            //master.commLog.ClearCommLog();
        }


        private void btn_CommLog_Checked(object sender, RoutedEventArgs e)
        {
            isOpenCommLog = (bool)btn_CommLog.IsChecked;
            master.OpenHiwinRobotDialog(isOpenCommLog);
        }

        private void btn_CommLog_Unchecked(object sender, RoutedEventArgs e)
        {
            isOpenCommLog = (bool)btn_CommLog.IsChecked;
            master.OpenHiwinRobotDialog(isOpenCommLog);
        }



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
            outputLogView.AddLineOutputLog(text, nStyle);
        }

        #region PIXEL RULER
        private void btn_Pixel_Ruler_Checked(object sender, RoutedEventArgs e)
        {
            popupRuler.IsOpen = true;
            Grid[] tempGrid = new Grid[Source.Application.Application.m_nTrack];
            for (int index_track = 0; index_track < Source.Application.Application.m_nTrack; index_track++)
            {
                tempGrid[index_track] = master.m_Tracks[index_track].m_imageViews[0].grd_Dock;
            }

            pixelRuler.SetUp(tempGrid, true);
        }

        private void btn_Pixel_Ruler_Unchecked(object sender, RoutedEventArgs e)
        {
            popupRuler.IsOpen = false;
            pixelRuler.Finish();
        }
        #endregion

        private void btn_Binarize_Checked(object sender, RoutedEventArgs e)
        {
            if (activeImageDock == null)
                return;
            int trackID = activeImageDock.trackID;
            //if (trackID == 0)
            //{
            //    master.m_Tracks[trackID].imageViewSF[0].Stamp_rangeSlider_H.HigherValue = 70;
            //    master.m_Tracks[trackID].imageViewSF[0].Stamp_rangeSlider_H.LowerValue = 0;
            //    master.m_Tracks[trackID].imageViewSF[0].Stamp_rangeSlider_S.HigherValue = 255;
            //    master.m_Tracks[trackID].imageViewSF[0].Stamp_rangeSlider_S.LowerValue = 0;
            //    master.m_Tracks[trackID].imageViewSF[0].Stamp_rangeSlider_V.HigherValue = 255;
            //    master.m_Tracks[trackID].imageViewSF[0].Stamp_rangeSlider_V.LowerValue = 0;
            //    master.m_Tracks[trackID].imageViewSF[0].panelSlider.Visibility = Visibility.Visible;


            //    master.m_Tracks[trackID].imageViewSF[0].enableGray = 2;
            //    master.m_Tracks[trackID].imageViewSF[0].ShowBinaryImage();
            //    master.m_Tracks[trackID].imageViewSF[0].ClearOverlay();
            //    master.m_Tracks[trackID].imageViewSF[0].ClearText();

            //}
            if (trackID == 0 || trackID == 1)
            {
                master.m_Tracks[trackID].m_imageViews[0].ClearOverlay();
                master.m_Tracks[trackID].m_imageViews[0].ClearText();
                master.m_Tracks[trackID].m_imageViews[0].enableGray = 2;
                master.m_Tracks[trackID].m_imageViews[0].panelSliderGray.Visibility = Visibility.Visible;
                master.m_Tracks[trackID].m_imageViews[0].UpdateSourceSliderChangeValue();
            }
        }
        public void btn_Binarize_Unchecked(object sender, RoutedEventArgs e)
        {
            if (activeImageDock == null)
                return;
            int trackID = activeImageDock.trackID;
            //if (trackID == 0)
            //{
            //    master.trackSF[0].imageViewSF[0].enableGray = 0;
            //    master.trackSF[0].imageViewSF[0].panelSlider.Visibility = Visibility.Collapsed;
            //    master.trackSF[0].imageViewSF[0].image.Source = master.trackSF[0].imageViewSF[0].btmSource;
            //}
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

            master.m_Tracks[activeImageDock.trackID].m_InspectionCore.LoadImageToInspection(master.m_Tracks[activeImageDock.trackID].m_imageViews[0].btmSource);
            master.m_Tracks[activeImageDock.trackID].Inspect(ref master.m_Tracks[activeImageDock.trackID]);
            UpdateDebugInfor();
            return;
        }

        private void btn_debug_Unchecked(object sender, RoutedEventArgs e)
        {

            m_bEnableDebug = (bool)debug_btn.IsChecked;
            //grd_Defect.Children.Clear();
            grd_Defect_Settings.Visibility = Visibility.Collapsed;
            //defectInfor.lvDefect.ItemsSource = null;

        }


        GridView gridView = new GridView();
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
                //defectInfor.SvDefect.CanContentScroll = true;
                //grd_Defect.VerticalAlignment = VerticalAlignment.Top;
                //grd_Defect.HorizontalAlignment = HorizontalAlignment.Left;
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


    }
}
