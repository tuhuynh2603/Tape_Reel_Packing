using Microsoft.Expression.Interactivity.Core;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ServiceModel.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Define;
using TapeReelPacking.Source.Factory;
using TapeReelPacking.Source.Interface;
using TapeReelPacking.Source.LogMessage;
using TapeReelPacking.Source.Model;
using TapeReelPacking.Source.Repository;
using TapeReelPacking.UI.UserControls.RibbonTabItems;
using TapeReelPacking.UI.UserControls.View;
using Xceed.Wpf.AvalonDock.Layout;
using Application = TapeReelPacking.Source.Application.Application;
using Point = System.Windows.Point;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class MainWindowVM : BaseVM
    {
        public static Master master;
        public Application applications { set; get; }
        public static bool IsFisrtLogin = false;
        public bool Logout = false;

        public static MainWindowVM mainWindowVM;
        public static bool m_IsWindowOpen = true;

        public int m_nDeviceX = 5;
        public int m_nDeviceY = 5;
        public int m_nTotalDevicePerLot = 1000;

        internal static string accountUser;
        internal static AccessLevel accessLevel;



        public bool bNextStepSimulateSequence = false;

        public static bool m_bSequenceRunning { set; get; } = false;

        public static string[] titles = new string[] { "Top Camera", "Barcode Reader", "Flap Side 2 " };

        private int screenWidth = 2000;// System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        private int screenHeight = 2000;// System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

        public OutputLogView outputLogView;
        public StatisticView m_staticView;
        public StatisticVM m_staticVM;

        private LayoutDocumentPaneGroup mainPanelGroup;
        public LayoutDocumentPaneGroup oldPanelGroup = new LayoutDocumentPaneGroup();

        private LayoutDocumentPaneGroup zoomDocPaneGroup = new LayoutDocumentPaneGroup();
        private LayoutDocumentPane imageZoomViewPane = new LayoutDocumentPane();
        private LayoutDocument zoomDoc = new LayoutDocument();

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


        private Point _startPositionDlg;
        private System.Windows.Vector _startOffsetPositionDlg;

        private Point _startWarningPositionDlg;
        private System.Windows.Vector _startOffsetWarningPositionDlg;


        public static UISTate UICurrentState { get; internal set; }


        private LayoutPanel tempDefaultPanelHomeView;
        private LayoutPanel tempPanelZoomView;




        private LayoutRoot _layoutVision;
        public LayoutRoot layoutVision
        {
            get { return _layoutVision; }
            set
            {
                _layoutVision = value;
                OnPropertyChanged(nameof(layoutVision));
            }
        }


        private string _m_strCurrentLotID = "";
        public string m_strCurrentLotID
        {
            get { return _m_strCurrentLotID; }
            set
            {
                _m_strCurrentLotID = value;
                OnPropertyChanged(nameof(m_strCurrentLotID));
            }
        }



        private double _dialogDefectHeight;
        public double DialogDefectHeight
        {
            get { return _dialogDefectHeight; }
            set
            {
                if (value != _dialogDefectHeight)
                {
                    _dialogDefectHeight = value;
                    OnPropertyChanged(nameof(DialogDefectHeight));
                }
            }
        }

        private double _dialogDefectWidth;
        public double DialogDefectWidth
        {
            get { return _dialogDefectWidth; }
            set
            {
                if (value != _dialogDefectWidth)
                {
                    _dialogDefectWidth = value;
                    OnPropertyChanged(nameof(DialogDefectWidth));
                }
            }
        }



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


        public DatabaseContext databaseContext { set; get; } = new DatabaseContext();
        public void ConstructModels()
        {




            mTitleBarVM = new TitleBarVM(this);   
            mLoginUserVM = new DragDropUserControlVM((p, mainVM) => new LoginUserVM(p, mainVM), this);
            mSerialCommunicationVM = new DragDropUserControlVM((p, mainVM) => new SerialCommunicationVM(p, mainVM), this);
            mPixelRulerVM = new DragDropUserControlVM((p, mainVM) => new PixelRulerVM(p, mainVM), this);
            mPLCCOMMVM = new DragDropUserControlVM((p, mainVM) => new PLCCOMMVM(p, mainVM), this);
            mHiwinRobotVM = new DragDropUserControlVM((p) => new HIKRobotVM(p));
            mHIKControlCameraVM = new DragDropUserControlVM((p) => new HIKControlCameraVM(p));
            mMappingSettingUCVM = new DragDropUserControlVM((p, mainVM, databaseContext) => new MappingSetingUCVM(p, mainVM, databaseContext), this, databaseContext);
            mBarCodeReaderVM = new DragDropUserControlVM((p) => new BarcodeReaderVM(p));
            mLotBarcodeDataTableVM = new DragDropUserControlVM((p, mainVM) => new LotBarcodeDatatableVM(p, mainVM), this);
            mRecipeManageVM = new DragDropUserControlVM((p, mainVM) => new RecipeManageVM(p, mainVM), this);
            mWarningMessageBoxVM = new DragDropUserControlVM((p) => new WarningMessageBoxVM(p));
            mTeachParameterVM = new DragDropUserControlVM((p, databaseContext) => new TeachParameterVM(p, databaseContext), databaseContext);
            mVisionParameterVM = new DragDropUserControlVM((p, databaseContext) => new VisionParameterVM(p, databaseContext), databaseContext);
            mStepDebugVM = new DragDropUserControlVM((p) => new StepDebugVM(p));
            inspectionTabVM = new InspectionTabVM(this);

        }

        //public Func<DragDropUserControlVM, MainWindowVM, BaseVM> InitLoginUserVM = (p, mainVM) =>
        //{
        //    return new LoginUserVM(p, mainVM);
        //};

        //public Func<DragDropUserControlVM, MainWindowVM, BaseVM> InitSerialCommunicationVM = (p, mainVM) =>
        //{
        //    return new SerialCommunicationVM(p, mainVM);
        //};
        //public Func<DragDropUserControlVM, MainWindowVM, BaseVM> InitPixelRulerVM = (p, mainVM) =>
        //{
        //    return new PixelRulerVM(p, mainVM);
        //};
        //public Func<DragDropUserControlVM, MainWindowVM, BaseVM> InitPLCCOMMVM = (p, mainVM) =>
        //{
        //    return new PLCCOMMVM(p, mainVM);
        //};
        //public Func<DragDropUserControlVM, BaseVM> InitHIKRobotVM = (p) =>
        //{
        //    return new HIKRobotVM(p);
        //};
        //public Func<DragDropUserControlVM, BaseVM> InitHIKControlCameraVM = (p) =>
        //{
        //    return new HIKControlCameraVM(p);
        //};
        //public Func<DragDropUserControlVM, MainWindowVM, DatabaseContext, BaseVM> InitMappingSetingUCVM = (p, mainVM, databaseContext) =>
        //{
        //    return new MappingSetingUCVM(p, mainVM, databaseContext);
        //};
        //public Func<DragDropUserControlVM, MainWindowVM, BaseVM> InitLotBarcodeDatatableVM = (p, mainVM) =>
        //{
        //    return new LotBarcodeDatatableVM(p, mainVM);
        //};
        //public Func<DragDropUserControlVM, BaseVM> InitBarcodeReaderVM = (p) =>
        //{
        //    return new BarcodeReaderVM(p);
        //};
        //public Func<DragDropUserControlVM, MainWindowVM, BaseVM> InitRecipeManageVM = (mainVM, databaseContext) =>
        //{
        //    return new RecipeManageVM(mainVM, databaseContext);
        //};

        //public Func<DragDropUserControlVM, BaseVM> InitWarningBoxVM = (p) =>
        //{
        //    return new WarningMessageBoxVM(p);
        //};

        //public Func<DragDropUserControlVM, DatabaseContext, BaseVM> InitTeachParameterVM = (p, databaseContext) =>
        //{
        //    return new TeachParameterVM(p, databaseContext);
        //};

        //public Func<DragDropUserControlVM, DatabaseContext, BaseVM> InitVisionParameterVM = (p, databaseContext) =>
        //{
        //    return new VisionParameterVM(p, databaseContext);
        //};

        //public Func<DragDropUserControlVM, BaseVM> InitStepDebugVM = (p) =>
        //{
        //    return new StepDebugVM(p);
        //};



        MappingCanvasVM mappingCanvasVM { set; get; }


        public MainWindowVM()
        {
            LogMessage.WriteToDebugViewer(0, string.Format("Start Application...."));
            applications = new Application(this);

            enableButton(false);
            ConstructModels();

            mainWindowVM = this;
            master = new Master(this);

            ContrucUIComponent();
            MappingImageDockToAvalonDock();

            for (int nTrack = 0; nTrack < Source.Application.Application.m_nTrack; nTrack++)
                master.loadTeachImageToUI(nTrack);

            loadAllStatistic(true);
            updateCameraConnectionStatusDelegate = UpdateCameraConnectionStatus;
            zoomDocPanelDelegate = ZoomDocPanel;
            updateTitleDocDelegate = UpdateTitleDoc;
            loadAllStatisticDelegate = loadAllStatistic;
        }



        public void InitBigDocPanel()
        {
            zoomDocPaneGroup.Children.Add(imageZoomViewPane);
            imageZoomViewPane.DockWidth = new System.Windows.GridLength(screenWidth / 1.5);
            imageZoomViewPane.DockHeight = new System.Windows.GridLength(screenHeight / 1);
            imageZoomViewPane.DockMinHeight = screenHeight / 1;
            imageZoomViewPane.DockMinWidth = screenWidth / 2;

            imageZoomViewPane.Children.Add(zoomDoc);
            zoomDoc.Title = "Zoom Doc Panel";
            zoomDoc.CanClose = false;
            zoomDoc.CanFloat = false;
        }

        public void InitTeachDocument()
        {
            teachViewDoc = new LayoutDocument();
            teachViewDoc.CanClose = false;
            teachViewDoc.CanFloat = false;
            teachViewDoc.Title = "Camera View";
            teachViewDoc.ContentId = "Teach";
        }
        private void ContrucUIComponent()
        {

            InitBigDocPanel();

            // InitTeachDocument();

            tempDefaultPanelHomeView = new LayoutPanel();
            mainPanelGroup = new LayoutDocumentPaneGroup();
            mainPanelGroup.Orientation = System.Windows.Controls.Orientation.Horizontal;
            outPutLogPaneGroup = new LayoutAnchorablePaneGroup();
            outPutLogPaneGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
            m_MappingPaneGroup = new LayoutAnchorablePaneGroup();
            m_MappingPaneGroup.Orientation = System.Windows.Controls.Orientation.Vertical;

            int numTrack, num_Doc, total_doc;
            numTrack = TapeReelPacking.Source.Application.Application.m_nTrack;
            num_Doc = TapeReelPacking.Source.Application.Application.m_nDoc;
            total_doc = num_Doc * numTrack;// Application.Application.total_doc;

            #region Image Layout
            imagesViewPaneGroup = new LayoutDocumentPaneGroup();
            imagesViewPaneGroup.Orientation = System.Windows.Controls.Orientation.Horizontal;
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
                        CanClose = false
                    };
                    imagesViewPane[track_index].Children.Add(imagesViewDoc[track_index * num_Doc + doc_index]);

                }
            }
            #endregion


            #region Output Log Contruction

            outputLogView = new OutputLogView();


            outPutLogViewDoc = new LayoutAnchorable();
            outPutLogViewDoc.Title = "Output Log View";
            outPutLogViewDoc.Content = outputLogView;
            outPutLogViewDoc.ContentId = "";

            outPutLogViewDoc.CanClose = false;
            outPutLogViewDoc.CanHide = false;
            outPutLogViewDoc.AutoHideMinWidth = screenWidth / 1;

            outPutLogViewPane = new LayoutAnchorablePane();
            outPutLogViewPane.Children.Add(outPutLogViewDoc);
            outPutLogPaneGroup.Children.Add(outPutLogViewPane);

            outPutLogPaneGroup.DockWidth = new System.Windows.GridLength(screenWidth / 4);
            outPutLogPaneGroup.DockHeight = new System.Windows.GridLength(screenHeight / 5);
            outPutLogPaneGroup.DockMinHeight = screenHeight / 5;
            outPutLogPaneGroup.DockMinWidth = screenWidth / 10;
            #endregion


            #region Statistic Contruction
            m_staticView = new StatisticView();
            mappingCanvasVM = new MappingCanvasVM(((MappingSetingUCVM)mMappingSettingUCVM.CurrentViewModel).categoriesMappingParam);
            m_staticVM = new StatisticVM(mappingCanvasVM);
            m_staticView.DataContext = m_staticVM;

            m_MappingViewDoc = new LayoutAnchorable();
            m_MappingViewDoc.Title = "Mapping Results";
            m_MappingViewDoc.Content = m_staticView;
            m_MappingViewDoc.ContentId = "";

            m_MappingViewDoc.CanClose = true;
            m_MappingViewDoc.CanHide = true;
            m_MappingViewDoc.AutoHideMinWidth = screenWidth / 1;

            m_MappingViewPane = new LayoutAnchorablePane();
            m_MappingPaneGroup.Children.Add(m_MappingViewPane);

            m_MappingPaneGroup.DockWidth = new System.Windows.GridLength(screenWidth);
            m_MappingPaneGroup.DockHeight = new System.Windows.GridLength(screenHeight / 6);
            m_MappingPaneGroup.DockMinHeight = screenHeight / 10;
            m_MappingPaneGroup.DockMinWidth = screenWidth / 10;
            m_MappingViewPane.Children.Add(m_MappingViewDoc);
            #endregion


            #region Show UI
            m_layout = new LayoutPanel();
            m_layout.Orientation = System.Windows.Controls.Orientation.Horizontal;
            m_layout.Children.Add(mainPanelGroup);
            m_layout.Children.Add(outPutLogPaneGroup);
            tempDefaultPanelHomeView.Orientation = System.Windows.Controls.Orientation.Vertical;
            tempDefaultPanelHomeView.Children.Add(m_layout);
            tempDefaultPanelHomeView.Children.Add(m_MappingPaneGroup);
            tempPanelZoomView = new LayoutPanel();
            tempPanelZoomView.Children.Add(zoomDocPaneGroup);

            layoutVision = new LayoutRoot();
            layoutVision.RootPanel = tempDefaultPanelHomeView;
            #endregion
        }


        public void MappingImageDockToAvalonDock()
        {
            int z = 0;
            activeImageDock = MainWindowVM.master.m_Tracks[0].m_imageViews[0];
            for (int itrack = 0; itrack < TapeReelPacking.Source.Application.Application.m_nTrack; itrack++)
            {
                for (int j = 0; j < TapeReelPacking.Source.Application.Application.m_nDoc; j++)
                {
                    imagesViewDoc[z].Content = master.m_Tracks[itrack].m_imageViews[j];
                    UpdateTitleDoc(z, "");
                    z++;
                }
            }
        }

        public delegate void UpdateTitleDocDelegate(int docIdx, string name, bool isLoadTeachImage = false);
        public static UpdateTitleDocDelegate updateTitleDocDelegate;

        public void UpdateTitleDoc(int docIdx, string name, bool isLoadTeachImage = false)
        {
            if (isLoadTeachImage)
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    master.m_Tracks[docIdx].ClearOverLay();
                    imagesViewDoc[docIdx].ContentId = name;
                });
            }
            else
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    imagesViewDoc[docIdx].ContentId = name;
                });
            }
        }


        public delegate void ZoomDocPanelDelegate(int trackID);
        public static ZoomDocPanelDelegate zoomDocPanelDelegate;

        static bool isOneSpecificDocState = false;
        public void ZoomDocPanel(int trackID)
        {
            if (MainWindowVM.m_bSequenceRunning)
                return;

            if (!isOneSpecificDocState)
            {
                zoomDoc.Title = imagesViewDoc[trackID].Title;
                zoomDoc.Content = imagesViewDoc[trackID].Content;
                zoomDoc.ContentId = imagesViewDoc[trackID].ContentId;
                zoomDoc.CanFloat = true;
                zoomDoc.CanClose = true;
                zoomDoc.CanMove = true;
                layoutVision.ReplaceChild(tempDefaultPanelHomeView, tempPanelZoomView);
            }
            else
            {

                layoutVision.ReplaceChild(tempPanelZoomView, tempDefaultPanelHomeView);
            }

            isOneSpecificDocState = !isOneSpecificDocState;
            activeImageDock.transform.Reset(1);
        }




        private ActionCommand btn_LogIn_CheckedCmd1;

        public ICommand btn_LogIn_CheckedCmd
        {
            get
            {
                if (btn_LogIn_CheckedCmd1 == null)
                {
                    btn_LogIn_CheckedCmd1 = new ActionCommand(Performbtn_LogIn_CheckedCmd);
                }

                return btn_LogIn_CheckedCmd1;
            }
        }

        private void Performbtn_LogIn_CheckedCmd()
        {
            showLoginUser(true);

        }

        private ActionCommand btn_LogIn_UncheckedCmd1;

        public ICommand btn_LogIn_UncheckedCmd
        {
            get
            {
                if (btn_LogIn_UncheckedCmd1 == null)
                {
                    btn_LogIn_UncheckedCmd1 = new ActionCommand(Performbtn_LogIn_UncheckedCmd);
                }

                return btn_LogIn_UncheckedCmd1;
            }
        }

        private void Performbtn_LogIn_UncheckedCmd()
        {
            showLoginUser(false);

        }


        //public delegate void ShowLoginUserDelegate(bool bShow);
        //public static ShowLoginUserDelegate showLoginUserDelegate;
        public void showLoginUser(bool bShow)
        {
            if (bShow)
            {
                //CleanHotKey();
                mLoginUserVM.isVisible = Visibility.Visible;

            }
            else
            {
                mLoginUserVM.isVisible = Visibility.Collapsed;
            }
        }

        public delegate void LoadAllStatisticDelegate(bool bResetSummary);

        public static LoadAllStatisticDelegate loadAllStatisticDelegate;

        public void loadAllStatistic(bool bResetSummary)
        {
            if (master.m_UpdateMappingUIThread == null)
            {
                master.m_UpdateMappingUIThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_loadAllStatistic(bResetSummary)));
                master.m_UpdateMappingUIThread.Start();
            }
            else if (!master.m_UpdateMappingUIThread.IsAlive)
            {
                master.m_UpdateMappingUIThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => func_loadAllStatistic(bResetSummary)));
                master.m_UpdateMappingUIThread.Start();

            }
        }
        public void func_loadAllStatistic(bool bResetSummary)
        {
            for (int nT = 0; nT < 2; nT++)
            {
                LoadStatistic(nT, bResetSummary);
            }
        }

        public void LoadStatistic(int nT, bool bResetSummary)
        {

            if (bResetSummary)
            {
                for (int n = 0; n < ((MappingSetingUCVM)mMappingSettingUCVM.CurrentViewModel).categoriesMappingParam.M_NumberDevicePerLot; n++)
                {
                    master.m_Tracks[nT].m_VisionResultDatas[n] = new VisionResultData();
                    master.m_Tracks[nT].m_VisionResultDatas_Total[n] = new VisionResultData();
                }

                VisionResultData.ReadLotResultFromExcel(Application.m_strCurrentLot, nT, ref master.m_Tracks[nT].m_VisionResultDatas, ref master.m_Tracks[nT].m_CurrentSequenceDeviceID);
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    for (int n = 0; n < ((MappingSetingUCVM)mMappingSettingUCVM.CurrentViewModel).categoriesMappingParam.M_NumberDevicePerLot; n++)
                    {
                        StatisticVM.updateValueStatisticDelegate?.Invoke(master.m_Tracks[nT].m_VisionResultDatas[n].m_nResult, nT);
                    }
                });
            }

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {

                MappingCanvasVM.updateMappingResultPageDelegate?.Invoke(nT);

            });

        }
        public void Run_Sequence(int nTrack = (int)TRACK_TYPE.TRACK_ALL)
        {

            master.BarcodeReaderSequenceThread();
            master.m_bRobotSequenceStatus = master.RobotSequenceThread();

            if (master.m_bRobotSequenceStatus)
            {
                OutputLogVM.AddLineOutputLog("Machine is running. Please Stop it and try again!", (int)ERROR_CODE.LABEL_FAIL);
                return;
            }

            MainWindowVM.m_bSequenceRunning = true;
        }
        public void Stop_Sequence(int nTrack = (int)TRACK_TYPE.TRACK_ALL)
        {
            MainWindowVM.m_bSequenceRunning = false;
        }

        public void AddLineOutputLog(string text, int nStyle = (int)ERROR_CODE.PASS)
        {
            if (outputLogView == null)
                return;

            OutputLogVM.AddLineOutputLog(text, nStyle);

        }

        private ActionCommand btn_Serial_COMM_UncheckedCmd1;

        public ICommand btn_Serial_COMM_UncheckedCmd
        {
            get
            {
                if (btn_Serial_COMM_UncheckedCmd1 == null)
                {
                    btn_Serial_COMM_UncheckedCmd1 = new ActionCommand(Performbtn_Serial_COMM_UncheckedCmd);
                }

                return btn_Serial_COMM_UncheckedCmd1;
            }
        }

        private void Performbtn_Serial_COMM_UncheckedCmd()
        {
            showSerialCommunicationView(false);
        }

        private ActionCommand btn_Serial_COMM_CheckedCmd1;

        public ICommand btn_Serial_COMM_CheckedCmd
        {
            get
            {
                if (btn_Serial_COMM_CheckedCmd1 == null)
                {
                    btn_Serial_COMM_CheckedCmd1 = new ActionCommand(Performbtn_Serial_COMM_CheckedCmd);
                }

                return btn_Serial_COMM_CheckedCmd1;
            }
        }

        private void Performbtn_Serial_COMM_CheckedCmd()
        {
            showSerialCommunicationView(true);
        }


        public void showSerialCommunicationView(bool bShow)
        {
            if (bShow)
            {
                //CleanHotKey();
                mSerialCommunicationVM.isVisible = Visibility.Visible;


            }
            else
            {
                //AddHotKey();
                mSerialCommunicationVM.isVisible = Visibility.Collapsed;

            }
        }


        List<KeyBinding> hotkey = new List<KeyBinding>();

        //private void CleanHotKey()
        //{

        //    foreach (var obj in hotkey)
        //    {
        //        InputBindings.Remove(obj);
        //    }
        //}
        //private void AddHotKey()
        //{
        //    foreach (var obj in hotkey)
        //    {
        //        if (!InputBindings.Contains(obj))
        //            InputBindings.Add(obj);
        //    }
        //    //InputBindings.AddRange(hotkey);
        //}





        private ActionCommand btn_Binarize_ClickCmd1;

        public ICommand btn_Binarize_ClickCmd
        {
            get
            {
                if (btn_Binarize_ClickCmd1 == null)
                {
                    btn_Binarize_ClickCmd1 = new ActionCommand(Performbtn_Binarize_ClickCmd);
                }

                return btn_Binarize_ClickCmd1;
            }
        }


        private void Performbtn_Binarize_ClickCmd()
        {
            if (MainWindowVM.m_bSequenceRunning)
            {
                btn_Binarize_Off();
                return;
            }

            if (activeImageDock == null)
                return;
            isbtn_BinarizeChecked = !isbtn_BinarizeChecked;
            if (isbtn_BinarizeChecked)
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



        private ActionCommand btn_ShowOverlay_ClickCmd1;

        public ICommand btn_ShowOverlay_ClickCmd
        {
            get
            {
                if (btn_ShowOverlay_ClickCmd1 == null)
                {
                    btn_ShowOverlay_ClickCmd1 = new ActionCommand(Performbtn_ShowOverlay_ClickCmd);
                }

                return btn_ShowOverlay_ClickCmd1;
            }
        }

        private void Performbtn_ShowOverlay_ClickCmd()
        {
            m_bShowOverlay = !m_bShowOverlay;
            ShowOverlay(m_bShowOverlay);
        }


        public bool m_bShowOverlay = true;
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
                    int nResult = master.m_Tracks[trackID].m_InspectionOnlineThreadVisionResult.m_nResult;
                    master.m_Tracks[trackID].DrawInspectionResult(ref nResult, ref master.m_Tracks[trackID].m_Center_Vision, ref master.m_Tracks[trackID].m_dDeltaAngleInspection);
                }
                else
                {
                    master.m_Tracks[trackID].m_imageViews[0].ClearOverlay();
                    master.m_Tracks[trackID].m_imageViews[0].ClearText();
                }


            });
        }




        private ActionCommand btn_Pixel_Ruler_ClickCmd1;

        public ICommand btn_Pixel_Ruler_ClickCmd
        {
            get
            {
                if (btn_Pixel_Ruler_ClickCmd1 == null)
                {
                    btn_Pixel_Ruler_ClickCmd1 = new ActionCommand(Performbtn_Pixel_Ruler_ClickCmd);
                }

                return btn_Pixel_Ruler_ClickCmd1;
            }
        }

        private void Performbtn_Pixel_Ruler_ClickCmd()
        {

            if (mPixelRulerVM.isVisible != Visibility.Collapsed)
            {
                mPixelRulerVM.isVisible = Visibility.Collapsed;
            }
            else
                mPixelRulerVM.isVisible = Visibility.Visible;

            PixelRuler.initPixelRuleDelegate?.Invoke(mPixelRulerVM.isVisible);
        }

        private ActionCommand btn_mapping_parameters_UncheckedCmd1;

        public ICommand btn_mapping_parameters_UncheckedCmd
        {
            get
            {
                if (btn_mapping_parameters_UncheckedCmd1 == null)
                {
                    btn_mapping_parameters_UncheckedCmd1 = new ActionCommand(Performbtn_mapping_parameters_UncheckedCmd);
                }

                return btn_mapping_parameters_UncheckedCmd1;
            }
        }

        private void Performbtn_mapping_parameters_UncheckedCmd()
        {
            mMappingSettingUCVM.isVisible = Visibility.Collapsed;

        }

        private ActionCommand btn_mapping_parameters_CheckedCmd1;

        public ICommand btn_mapping_parameters_CheckedCmd
        {
            get
            {
                if (btn_mapping_parameters_CheckedCmd1 == null)
                {
                    btn_mapping_parameters_CheckedCmd1 = new ActionCommand(Performbtn_mapping_parameters_CheckedCmd);
                }

                return btn_mapping_parameters_CheckedCmd1;
            }
        }

        private void Performbtn_mapping_parameters_CheckedCmd()
        {

            mMappingSettingUCVM.isVisible = Visibility.Visible;
        }

        private ActionCommand btn_run_sequence_UncheckedCmd1;

        public ICommand btn_run_sequence_UncheckedCmd
        {
            get
            {
                if (btn_run_sequence_UncheckedCmd1 == null)
                {
                    btn_run_sequence_UncheckedCmd1 = new ActionCommand(Performbtn_run_sequence_UncheckedCmd);
                }

                return btn_run_sequence_UncheckedCmd1;
            }
        }

        private void Performbtn_run_sequence_UncheckedCmd()
        {
            Stop_Sequence(activeImageDock.trackID);

        }

        private ActionCommand btn_run_sequence_CheckedCmd1;
        public ICommand btn_run_sequence_CheckedCmd
        {
            get
            {
                if (btn_run_sequence_CheckedCmd1 == null)
                {
                    btn_run_sequence_CheckedCmd1 = new ActionCommand(Performbtn_run_sequence_CheckedCmd);
                }

                return btn_run_sequence_CheckedCmd1;
            }
        }


        public static bool btn_runChecked = false;

        private void Performbtn_run_sequence_CheckedCmd()
        {
            btn_runChecked = true;
            Run_Sequence(activeImageDock.trackID);
        }

        private ActionCommand btn_Debug_sequence_UncheckedCmd1;

        public ICommand btn_Debug_sequence_UncheckedCmd
        {
            get
            {
                if (btn_Debug_sequence_UncheckedCmd1 == null)
                {
                    btn_Debug_sequence_UncheckedCmd1 = new ActionCommand(Performbtn_Debug_sequence_UncheckedCmd);
                }

                return btn_Debug_sequence_UncheckedCmd1;
            }
        }

        public static bool m_bEnableDebugSequence = false;
        private void Performbtn_Debug_sequence_UncheckedCmd()
        {

            //m_bEnableDebugSequence = (bool)btn_Debug_sequence.IsChecked;
            m_bEnableDebugSequence = false;

        }

        private ActionCommand btn_Debug_sequence_CheckedCmd1;

        public ICommand btn_Debug_sequence_CheckedCmd
        {
            get
            {
                if (btn_Debug_sequence_CheckedCmd1 == null)
                {
                    btn_Debug_sequence_CheckedCmd1 = new ActionCommand(Performbtn_Debug_sequence_CheckedCmd);
                }

                return btn_Debug_sequence_CheckedCmd1;
            }
        }

        private void Performbtn_Debug_sequence_CheckedCmd()
        {
            m_bEnableDebugSequence = true;
        }

        private ActionCommand btn_Imidiate_Stop_ClickCmd1;

        public ICommand btn_Imidiate_Stop_ClickCmd
        {
            get
            {
                if (btn_Imidiate_Stop_ClickCmd1 == null)
                {
                    btn_Imidiate_Stop_ClickCmd1 = new ActionCommand(Performbtn_Imidiate_Stop_ClickCmd);
                }

                return btn_Imidiate_Stop_ClickCmd1;
            }
        }

        private void Performbtn_Imidiate_Stop_ClickCmd()
        {
            //btn_Imidiate_Stop.IsChecked = false;
            if (Master.RobotIOStatus.m_ImidiateStatus_Simulate == 0)
                Master.RobotIOStatus.m_ImidiateStatus_Simulate = 1;
            else
                Master.RobotIOStatus.m_ImidiateStatus_Simulate = 0;
        }

        private ActionCommand btn_Emergency_Stop_ClickCmd1;

        public ICommand btn_Emergency_Stop_ClickCmd
        {
            get
            {
                if (btn_Emergency_Stop_ClickCmd1 == null)
                {
                    btn_Emergency_Stop_ClickCmd1 = new ActionCommand(Performbtn_Emergency_Stop_ClickCmd);
                }

                return btn_Emergency_Stop_ClickCmd1;
            }
        }

        private void Performbtn_Emergency_Stop_ClickCmd()
        {
            Master.RobotIOStatus.m_EmergencyStatus_Simulate = Isbtn_Emergency_StopChecked ? 1:0;
        }

        private ActionCommand btn_Reset_Machine_ClickCmd1;

        public ICommand btn_Reset_Machine_ClickCmd
        {
            get
            {
                if (btn_Reset_Machine_ClickCmd1 == null)
                {
                    btn_Reset_Machine_ClickCmd1 = new ActionCommand(Performbtn_Reset_Machine_ClickCmd);
                }

                return btn_Reset_Machine_ClickCmd1;
            }
        }

        private void Performbtn_Reset_Machine_ClickCmd()
        {
            //btn_Reset_Machine.IsChecked = false;
            if (Master.RobotIOStatus.m_ResetMachineStatus_Simulate == 0)
                Master.RobotIOStatus.m_ResetMachineStatus_Simulate = 1;
            if (MainWindowVM.m_bSequenceRunning)
                return;
            if (master.thread_RobotSequence == null)
            {
                master.thread_RobotSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.ResetSequence()));
                //master.thread_RobotSequence.IsBackground = true;
                master.thread_RobotSequence.Start();
            }
            else if (!master.thread_RobotSequence.IsAlive)
            {
                master.thread_RobotSequence = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.ResetSequence()));
                //master.thread_RobotSequence.IsBackground = true;
                master.thread_RobotSequence.Start();
            }

        }

        private ActionCommand btn_Clear_CommCmd1;

        public ICommand btn_Clear_CommCmd
        {
            get
            {
                if (btn_Clear_CommCmd1 == null)
                {
                    btn_Clear_CommCmd1 = new ActionCommand(Performbtn_Clear_CommCmd);
                }

                return btn_Clear_CommCmd1;
            }
        }

        private void Performbtn_Clear_CommCmd()
        {
        }

        private ActionCommand btn_Lot_DataExcel_UncheckedCmd1;

        public ICommand btn_Lot_DataExcel_UncheckedCmd
        {
            get
            {
                if (btn_Lot_DataExcel_UncheckedCmd1 == null)
                {
                    btn_Lot_DataExcel_UncheckedCmd1 = new ActionCommand(Performbtn_Lot_DataExcel_UncheckedCmd);
                }

                return btn_Lot_DataExcel_UncheckedCmd1;
            }
        }

        private void Performbtn_Lot_DataExcel_UncheckedCmd()
        {
            mLotBarcodeDataTableVM.isVisible = Visibility.Collapsed;
        }

        private ActionCommand btn_Lot_DataExcel_CheckedCmd1;

        public ICommand btn_Lot_DataExcel_CheckedCmd
        {
            get
            {
                if (btn_Lot_DataExcel_CheckedCmd1 == null)
                {
                    btn_Lot_DataExcel_CheckedCmd1 = new ActionCommand(Performbtn_Lot_DataExcel_CheckedCmd);
                }

                return btn_Lot_DataExcel_CheckedCmd1;
            }
        }

        private void Performbtn_Lot_DataExcel_CheckedCmd()
        {
            mLotBarcodeDataTableVM.isVisible = Visibility.Visible;
        }

        private ActionCommand btn_PLCCOMM_Setting_UncheckedCmd1;

        public ICommand btn_PLCCOMM_Setting_UncheckedCmd
        {
            get
            {
                if (btn_PLCCOMM_Setting_UncheckedCmd1 == null)
                {
                    btn_PLCCOMM_Setting_UncheckedCmd1 = new ActionCommand(Performbtn_PLCCOMM_Setting_UncheckedCmd);
                }

                return btn_PLCCOMM_Setting_UncheckedCmd1;
            }
        }

        private void Performbtn_PLCCOMM_Setting_UncheckedCmd()
        {
            mPLCCOMMVM.isVisible = Visibility.Collapsed;

        }

        private ActionCommand btn_PLCCOMM_Setting_CheckedCmd1;

        public ICommand btn_PLCCOMM_Setting_CheckedCmd
        {
            get
            {
                if (btn_PLCCOMM_Setting_CheckedCmd1 == null)
                {
                    btn_PLCCOMM_Setting_CheckedCmd1 = new ActionCommand(Performbtn_PLCCOMM_Setting_CheckedCmd);
                }

                return btn_PLCCOMM_Setting_CheckedCmd1;
            }
        }

        private void Performbtn_PLCCOMM_Setting_CheckedCmd()
        {

            mPLCCOMMVM.isVisible = Visibility.Visible;
        }

        private ActionCommand btn_BarCodeReader_Setting_UncheckedCmd1;

        public ICommand btn_BarCodeReader_Setting_UncheckedCmd
        {
            get
            {
                if (btn_BarCodeReader_Setting_UncheckedCmd1 == null)
                {
                    btn_BarCodeReader_Setting_UncheckedCmd1 = new ActionCommand(Performbtn_BarCodeReader_Setting_UncheckedCmd);
                }

                return btn_BarCodeReader_Setting_UncheckedCmd1;
            }
        }

        private void Performbtn_BarCodeReader_Setting_UncheckedCmd()
        {

            mBarCodeReaderVM.isVisible = Visibility.Collapsed;

        }

        private ActionCommand btn_BarCodeReader_Setting_CheckedCmd1;

        public ICommand btn_BarCodeReader_Setting_CheckedCmd
        {
            get
            {
                if (btn_BarCodeReader_Setting_CheckedCmd1 == null)
                {
                    btn_BarCodeReader_Setting_CheckedCmd1 = new ActionCommand(Performbtn_BarCodeReader_Setting_CheckedCmd);
                }

                return btn_BarCodeReader_Setting_CheckedCmd1;
            }
        }

        private void Performbtn_BarCodeReader_Setting_CheckedCmd()
        {
            mBarCodeReaderVM.isVisible = Visibility.Visible;
        }

        public void UpdateCameraConnectionStatus(int nTrack, bool bIsconnected)
        {
            if (nTrack == 0)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    LogMessage.WriteToDebugViewer(5 + nTrack, $"{Application.LineNumber()}: {Application.PrintCallerName()}");

                    labelCameraStatus = $"{Application.m_strCameraSerial[nTrack]}";
                    if (bIsconnected)
                        labelCameraStatusBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                    else
                        labelCameraStatusBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);

                    LogMessage.WriteToDebugViewer(5 + nTrack, $"{Application.LineNumber()}: {Application.PrintCallerName()}");

                });
            }
        }

        public delegate void UpdateCameraConnectionStatusDelegate(int nTrack, bool bIsconnected);
        public static UpdateCameraConnectionStatusDelegate updateCameraConnectionStatusDelegate;

        private ActionCommand btn_Camera_Setting_UncheckedCmd1;

        public ICommand btn_Camera_Setting_UncheckedCmd
        {
            get
            {
                if (btn_Camera_Setting_UncheckedCmd1 == null)
                {
                    btn_Camera_Setting_UncheckedCmd1 = new ActionCommand(Performbtn_Camera_Setting_UncheckedCmd);
                }

                return btn_Camera_Setting_UncheckedCmd1;
            }
        }

        private void Performbtn_Camera_Setting_UncheckedCmd()
        {
            mHIKControlCameraVM.isVisible = Visibility.Collapsed;

        }

        private ActionCommand btn_Camera_Setting_CheckedCmd1;

        public ICommand btn_Camera_Setting_CheckedCmd
        {
            get
            {
                if (btn_Camera_Setting_CheckedCmd1 == null)
                {
                    btn_Camera_Setting_CheckedCmd1 = new ActionCommand(Performbtn_Camera_Setting_CheckedCmd);
                }

                return btn_Camera_Setting_CheckedCmd1;
            }
        }

        private void Performbtn_Camera_Setting_CheckedCmd()
        {
            if (MainWindowVM.m_bSequenceRunning)
            {
                return;
            }

            mHIKControlCameraVM.isVisible = Visibility.Visible;
        }

        private ActionCommand btn_stream_camera_UnCheckedCmd1;

        public ICommand btn_stream_camera_UnCheckedCmd
        {
            get
            {
                if (btn_stream_camera_UnCheckedCmd1 == null)
                {
                    btn_stream_camera_UnCheckedCmd1 = new ActionCommand(Performbtn_stream_camera_UnCheckedCmd);
                }

                return btn_stream_camera_UnCheckedCmd1;
            }
        }

        public static bool bEnableGrabCycle = false;
        private void Performbtn_stream_camera_UnCheckedCmd()
        {
            bEnableGrabCycle = false;

        }

        private ActionCommand btn_stream_camera_CheckedCmd1;

        public ICommand btn_stream_camera_CheckedCmd
        {
            get
            {
                if (btn_stream_camera_CheckedCmd1 == null)
                {
                    btn_stream_camera_CheckedCmd1 = new ActionCommand(Performbtn_stream_camera_CheckedCmd);
                }

                return btn_stream_camera_CheckedCmd1;
            }
        }

        private void Performbtn_stream_camera_CheckedCmd()
        {
            if (MainWindowVM.m_bSequenceRunning)
                return;

            bEnableGrabCycle = true;

            if (master.thread_StreamCamera[activeImageDock.trackID] == null)
            {
                master.thread_StreamCamera[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.func_GrabImageThread()));
                master.thread_StreamCamera[activeImageDock.trackID].Start();
            }
            else if (!master.thread_StreamCamera[activeImageDock.trackID].IsAlive)
            {
                master.thread_StreamCamera[activeImageDock.trackID] = new System.Threading.Thread(new System.Threading.ThreadStart(() => master.func_GrabImageThread()));
                //master.thread_StreamCamera[activeImageDock.trackID].IsBackground = true;
                master.thread_StreamCamera[activeImageDock.trackID].Start();
            }

        }


        public  void enableButton(bool bEnable)
        {
            bEnableGrabCycle = bEnable;

             InspectionTabVM.loginEnableButtonDelegate?.Invoke(bEnable);

        }


        public static void UpdateGrayValue(int trackID, string pos, string valueGray)
        {
            master.m_Tracks[trackID].m_imageViews[0].tbl_Pos.Text = pos;
            //master.m_Tracks[trackID].m_imageViews[0].tbl_Value.Text = "[None]";
            master.m_Tracks[trackID].m_imageViews[0].tbl_Value_gray.Text = valueGray;
        }
        public void UpdateRGBValue(string pos, string valueRGB, string valueGray)
        {
            master.m_Tracks[0].m_imageViews[0].tbl_Pos.Text = pos;
            //master.m_Tracks[0].m_imageViews[0].tbl_Value.Text = valueRGB;
            master.m_Tracks[0].m_imageViews[0].tbl_Value_gray.Text = valueGray;
        }

        private ActionCommand text_LotID_MouseLeftButtonDown1;

        public ICommand text_LotID_MouseLeftButtonDown
        {
            get
            {
                if (text_LotID_MouseLeftButtonDown1 == null)
                {
                    text_LotID_MouseLeftButtonDown1 = new ActionCommand(Performtext_LotID_MouseLeftButtonDown);
                }

                return text_LotID_MouseLeftButtonDown1;
            }
        }

        private void Performtext_LotID_MouseLeftButtonDown()
        {
        }

        private ActionCommand btn_Robot_Controller_UncheckedCmd1;

        public ICommand btn_Robot_Controller_UncheckedCmd
        {
            get
            {
                if (btn_Robot_Controller_UncheckedCmd1 == null)
                {
                    btn_Robot_Controller_UncheckedCmd1 = new ActionCommand(Performbtn_Robot_Controller_UncheckedCmd);
                }

                return btn_Robot_Controller_UncheckedCmd1;
            }
        }

        private void Performbtn_Robot_Controller_UncheckedCmd()
        {
            mHiwinRobotVM.isVisible = Visibility.Collapsed;
        }

        private ActionCommand btn_Robot_Controller_CheckedCmd1;

        public ICommand btn_Robot_Controller_CheckedCmd
        {
            get
            {
                if (btn_Robot_Controller_CheckedCmd1 == null)
                {
                    btn_Robot_Controller_CheckedCmd1 = new ActionCommand(Performbtn_Robot_Controller_CheckedCmd);
                }

                return btn_Robot_Controller_CheckedCmd1;
            }
        }

        private void Performbtn_Robot_Controller_CheckedCmd()
        {
            if (MainWindowVM.m_bSequenceRunning && false)
                return;
            mHiwinRobotVM.isVisible = Visibility.Visible;
        }

        private bool isbtn_BinarizeChecked1 = false;

        public bool isbtn_BinarizeChecked
        {
            get { return isbtn_BinarizeChecked1; }
            set
            {

                isbtn_BinarizeChecked1 = value;
                OnPropertyChanged(nameof(isbtn_BinarizeChecked));
            }
        }

        private bool isbtn_Emergency_StopChecked = false;

        public bool Isbtn_Emergency_StopChecked
        {
            get { return isbtn_Emergency_StopChecked; }
            set
            {

                isbtn_Emergency_StopChecked = value;
                OnPropertyChanged(nameof(Isbtn_Emergency_StopChecked));
            }
        }

        private bool isbtn_Imidiate_StopChecked = false;

        public bool Isbtn_Imidiate_StopChecked
        {
            get { return isbtn_Imidiate_StopChecked; }
            set
            {

                isbtn_Imidiate_StopChecked = value;
                OnPropertyChanged(nameof(Isbtn_Imidiate_StopChecked));
            }
        }

        private object labelCameraStatus1;

        public object labelCameraStatus
        {
            get { return labelCameraStatus1; }
            set
            {

                labelCameraStatus1 = value;
                OnPropertyChanged(nameof(labelCameraStatus));
            }
        }

        private object labelRobotStatus1;

        public object labelRobotStatus
        {
            get { return labelRobotStatus1; }
            set
            {

                labelRobotStatus1 = value;
                OnPropertyChanged(nameof(labelRobotStatus));
            }
        }
        private System.Windows.Media.Brush labelCameraStatusBackground1;

        public System.Windows.Media.Brush labelCameraStatusBackground
        {
            get { return labelCameraStatusBackground1; }
            set
            {

                labelCameraStatusBackground1 = value;
                OnPropertyChanged(nameof(labelCameraStatusBackground));
            }
        }


        private WindowState windowState1 = WindowState.Maximized;

        public WindowState windowState
        {
            get { return windowState1; }
            set
            {

                windowState1 = value;
                OnPropertyChanged(nameof(windowState));
            }
        }

        private InspectionTabVM inspectionTabVM1;

        public InspectionTabVM inspectionTabVM
        {
            get => inspectionTabVM1;
            set
            {
                inspectionTabVM1 = value;
                OnPropertyChanged(nameof(inspectionTabVM));
            }
        }

        private DragDropUserControlVM mHiwinRobotVM1;

        public DragDropUserControlVM mHiwinRobotVM
        {
            get => mHiwinRobotVM1;
            set
            {
                mHiwinRobotVM1 = value;
                OnPropertyChanged(nameof(mHiwinRobotVM));
            }
        }

        private DragDropUserControlVM mVisionParameterVM1;

        public DragDropUserControlVM mVisionParameterVM
        {
            get => mVisionParameterVM1;
            set
            {
                mVisionParameterVM1 = value;
                OnPropertyChanged(nameof(mVisionParameterVM));
            }
        }

        private DragDropUserControlVM mTeachParameterVM1;

        public DragDropUserControlVM mTeachParameterVM
        {
            get => mTeachParameterVM1;
            set
            {
                mTeachParameterVM1 = value;
                OnPropertyChanged(nameof(mTeachParameterVM));
            }
        }

        private DragDropUserControlVM mHIKControlCameraVM1;

        public DragDropUserControlVM mHIKControlCameraVM
        {
            get => mHIKControlCameraVM1;
            set
            {
                mHIKControlCameraVM1 = value;
                OnPropertyChanged(nameof(mHIKControlCameraVM));
            }
        }
        private DragDropUserControlVM mLoginUserVM1;

        public DragDropUserControlVM mLoginUserVM
        {
            get => mLoginUserVM1;
            set
            {
                mLoginUserVM1 = value;
                OnPropertyChanged(nameof(mLoginUserVM));
            }
        }
        private DragDropUserControlVM mPixelRulerVM1;

        public DragDropUserControlVM mPixelRulerVM
        {
            get => mPixelRulerVM1;
            set
            {
                mPixelRulerVM1 = value;
                OnPropertyChanged(nameof(mPixelRulerVM));
            }
        }
        private DragDropUserControlVM mPLCCOMMVM1;

        public DragDropUserControlVM mPLCCOMMVM
        {
            get => mPLCCOMMVM1;
            set
            {
                mPLCCOMMVM1 = value;
                OnPropertyChanged(nameof(mPLCCOMMVM));
            }
        }

        private DragDropUserControlVM mMappingSettingUCVM1;

        public DragDropUserControlVM mMappingSettingUCVM
        {
            get => mMappingSettingUCVM1;
            set
            {
                mMappingSettingUCVM1 = value;
                OnPropertyChanged(nameof(mMappingSettingUCVM));
            }
        }


        private DragDropUserControlVM mStepDebugVM1;

        public DragDropUserControlVM mStepDebugVM
        {
            get => mStepDebugVM1;
            set
            {
                mStepDebugVM1 = value;
                OnPropertyChanged(nameof(mStepDebugVM));
            }
        }

        private DragDropUserControlVM mBarCodeReaderVM1;
        public DragDropUserControlVM mBarCodeReaderVM
        {
            get => mBarCodeReaderVM1;
            set
            {
                mBarCodeReaderVM1 = value;
                OnPropertyChanged(nameof(mBarCodeReaderVM));
            }
        }

        private DragDropUserControlVM mLotBarcodeDataTableVM1;
        public DragDropUserControlVM mLotBarcodeDataTableVM
        {
            get => mLotBarcodeDataTableVM1;
            set
            {
                mLotBarcodeDataTableVM1 = value;
                OnPropertyChanged(nameof(mLotBarcodeDataTableVM));
            }
        }

        private DragDropUserControlVM mWarningMessageBoxVM1;

        public DragDropUserControlVM mWarningMessageBoxVM
        {
            get => mWarningMessageBoxVM1;
            set
            {
                mWarningMessageBoxVM1 = value;
                OnPropertyChanged(nameof(mWarningMessageBoxVM));
            }
        }

        private DragDropUserControlVM mSerialCommunicationVM1;

        public DragDropUserControlVM mSerialCommunicationVM
        {
            get => mSerialCommunicationVM1;
            set
            {
                mSerialCommunicationVM1 = value;
                OnPropertyChanged(nameof(mSerialCommunicationVM));
            }
        }

        private DragDropUserControlVM mRecipeManageVM1;

        public DragDropUserControlVM mRecipeManageVM
        {
            get => mRecipeManageVM1;
            set
            {
                mRecipeManageVM1 = value;
                OnPropertyChanged(nameof(mRecipeManageVM));
            }
        }

        private TitleBarVM mTitleBarVM1;

        public TitleBarVM mTitleBarVM
        {
            get => mTitleBarVM1;
            set
            {
                mTitleBarVM1 = value;
                OnPropertyChanged(nameof(mTitleBarVM));
            }
        }

        private ActionCommand btn_LoadRecipe_ClickCmd1;

        public ICommand btn_LoadRecipe_ClickCmd
        {
            get
            {
                if (btn_LoadRecipe_ClickCmd1 == null)
                {
                    btn_LoadRecipe_ClickCmd1 = new ActionCommand(Performbtn_LoadRecipe_ClickCmd);
                }

                return btn_LoadRecipe_ClickCmd1;
            }
        }

        private void Performbtn_LoadRecipe_ClickCmd()
        {
            if (mRecipeManageVM.isVisible == Visibility.Visible)
                mRecipeManageVM.isVisible = Visibility.Collapsed;
            else
                mRecipeManageVM.isVisible = Visibility.Visible;

        }
    }
}
