using Magnus_WPF_1.Source.Application;
//using System.Windows.Forms;
using Magnus_WPF_1.Source.LogMessage;
using Magnus_WPF_1.UI.UserControls.View;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Wpf.AvalonDock.Layout;

namespace Magnus_WPF_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public Master master;
        public static MainWindow mainWindow;
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

            //master.m_SaveInspectImageThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Testing_Thread(true)));

            enable_saveimage_btn.IsChecked = Source.Application.Application.m_bEnableSavingOnlineImage;


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

        }

        int nDeviceID = -1;
        private int Check_mapping_Cursor_ID(Point cur_point)
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
                }
            }

            //canvas_Mapping.Children.RemoveAt(nIDX + nIDY * 10);
            return nIDX + nIDY * m_nDeviceX;

        }

        private void canvas_Mapping_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_CanvasMovePoint = e.GetPosition(canvas_Mapping);
            int nID = Check_mapping_Cursor_ID(m_CanvasMovePoint);
            master.m_Tracks[activeImageDock.trackID].m_nCurrentClickMappingID = nID;
            if (btn_run_sequence.IsChecked == true)
                Master.m_hardwareTriggerSnapEvent[activeImageDock.trackID].Set();
            else
                Master.InspectEvent[0].Set();
        }

        private void canvas_Mapping_MouseMove(object sender, MouseEventArgs e)
        {
            m_CanvasMovePoint = e.GetPosition(canvas_Mapping);
            Check_mapping_Cursor_ID(m_CanvasMovePoint);
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

            #region Show UI
            m_layout = new LayoutPanel();
            m_layout.Orientation = Orientation.Vertical;
            m_layout.Children.Add(mainPanelGroup);

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
        private void Inspect_Cycle_Checked(object sender, RoutedEventArgs e)
        {
            bEnableGrabCycle = (bool)inspect_cycle_btn.IsChecked;

            if (master.thread_FullSequence[activeImageDock.trackID] == null)
            {
                master.thread_FullSequence[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Testing_Thread()));
                master.thread_FullSequence[activeImageDock.trackID].Start();
            }
            else if (!master.thread_FullSequence[activeImageDock.trackID].IsAlive)
            {
                master.thread_FullSequence[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Testing_Thread()));
                master.thread_FullSequence[activeImageDock.trackID].Start();
            }
        }

        private void Inspect_Cycle_Unchecked(object sender, RoutedEventArgs e)
        {
            bEnableGrabCycle = (bool)inspect_cycle_btn.IsChecked;

        }

        private void Camera_Setting_Checked(object sender, RoutedEventArgs e)
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

        private void Camera_Setting_Unchecked(object sender, RoutedEventArgs e)
        {
            bool bEnable = (bool)camera_setting_btn.IsChecked;
            grd_PopupDialog.Children.Clear();

        }

        private void PreviewMouseDownInspectionBtn(object sender, MouseButtonEventArgs e)
        {

        }

        private void Inspect_Unchecked(object sender, RoutedEventArgs e)
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

        private void load_teach_image_btn_Click(object sender, RoutedEventArgs e)
        {
            master.loadTeachImageToUI();

        }

        public bool bEnableSingleSnapImages = true;
        private void inspect_btn_Click(object sender, RoutedEventArgs e)
        {

            if (bEnableSingleSnapImages)
                bEnableSingleSnapImages = false;

            Master.InspectEvent[master.m_nActiveTrack].Set();

            if (master.thread_FullSequence[activeImageDock.trackID] == null)
            {
                master.thread_FullSequence[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Testing_Thread(true)));
                master.thread_FullSequence[activeImageDock.trackID].Start();
            }
            else if (!master.thread_FullSequence[activeImageDock.trackID].IsAlive)
            {
                master.thread_FullSequence[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.Grab_Image_Testing_Thread(true)));
                master.thread_FullSequence[activeImageDock.trackID].Start();
            }

            //mainWindow.statisticView.UpdateValueStatistic(master.m_Tracks[0].m_nResult);
            //inspect_btn.IsEnabled = bEnableGrabImages;
        }

        public bool bEnableOfflineInspection;
        string m_folderPath = @"C:\Wisely\C#\Magnus_WPF_1\DataBase\ImageSave";
        private void inspect_offline_btn_Click(object sender, RoutedEventArgs e)
        {

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


            if (bEnableOfflineInspection)
                bEnableOfflineInspection = false;

            Master.InspectEvent[0].Set();

            if (master.thread_FullSequence[activeImageDock.trackID] == null)
            {
                master.thread_FullSequence[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.InspectOffline(m_folderPath)));
                master.thread_FullSequence[activeImageDock.trackID].Start();
            }
            else if (!master.thread_FullSequence[activeImageDock.trackID].IsAlive)
            {
                master.thread_FullSequence[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.InspectOffline(m_folderPath)));
                master.thread_FullSequence[activeImageDock.trackID].Start();
            }

        }


        //HIKControlCameraView cameraView = new HIKControlCameraView();
        private void teach_parameters_btn_Checked(object sender, RoutedEventArgs e)
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

            grd_PopupDialog.Children.Add(master.teachParameter);
            //grd_PopupDialog.Children.Add(master.m_Tracks[]);
            tab_controls.SelectedIndex = currentTabIndex;

        }

        private void teach_parameters_btn_Unchecked(object sender, RoutedEventArgs e)
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
            master.loadTeachImageToUI();
            master.m_Tracks[0].m_imageViews[0].resultTeach.Children.Clear();
            master.m_Tracks[0].m_imageViews[0].ClearOverlay();
            master.m_Tracks[0].m_imageViews[0].controlWin.Visibility = Visibility.Collapsed;

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
            inspect_cycle_btn.IsEnabled = false;
            teach_parameters_btn.IsEnabled = false;
            save_teach_image_btn.IsEnabled = false;

        }

        public void SetDisableTeachButton()
        {
            btn_next_teach.IsEnabled = false;
            btn_abort_teach.IsEnabled = false;
            inspect_btn.IsEnabled = true;
            load_teach_image_btn.IsEnabled = true;
            inspect_cycle_btn.IsEnabled = true;
            teach_parameters_btn.IsEnabled = true;
            save_teach_image_btn.IsEnabled = true;
        }

        private void save_teach_image_btn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to save as teach image ?", "Save as Teach Image", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                master.m_Tracks[0].m_imageViews[0].SaveTeachImage(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_1.bmp"));
            }

        }

        private void mapping_parameters_btn_Unchecked(object sender, RoutedEventArgs e)
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

        private void mapping_parameters_btn_Checked(object sender, RoutedEventArgs e)
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

        private void enable_saveimage_btn_Unchecked(object sender, RoutedEventArgs e)
        {
            //master.applications.m_bEnableSavingOnlineImage = (bool) enable_saveimage_btn.IsChecked;
            Source.Application.Application.m_bEnableSavingOnlineImage = (bool)enable_saveimage_btn.IsChecked;
        }

        private void enable_saveimage_btn_Checked(object sender, RoutedEventArgs e)
        {
            //master.applications.m_bEnableSavingOnlineImage = (bool)enable_saveimage_btn.IsChecked;
            Source.Application.Application.m_bEnableSavingOnlineImage = (bool)enable_saveimage_btn.IsChecked;

        }

        public bool bEnableRunSequence = false;
        private void btn_run_sequence_Checked(object sender, RoutedEventArgs e)
        {
            bEnableRunSequence = (bool)btn_run_sequence.IsChecked;

            master.RunSequenceThread(activeImageDock.trackID);


        }

        private void btn_run_sequence_Unchecked(object sender, RoutedEventArgs e)
        {
            bEnableRunSequence = (bool)btn_run_sequence.IsChecked;

        }

        public void UpdateMappingResult(int nID, int nResult)
        {
            string path = @"/Resources/green-chip.png";

            if (nResult < 0)
                path = @"/Resources/red-chip.png";

            arr_imageMapping[nID].Source = new BitmapImage(new Uri(path, UriKind.Relative));
        }
        public void ResetMappingResult()
        {
            string path = @"/Resources/gray-chip.png";
            for (int nID = 0; nID < arr_imageMapping.Length; nID++)
                arr_imageMapping[nID].Source = new BitmapImage(new Uri(path, UriKind.Relative));
        }

        public void ResetStatisticResult()
        {
            statisticView.ClearStatistic();
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


    }
}
