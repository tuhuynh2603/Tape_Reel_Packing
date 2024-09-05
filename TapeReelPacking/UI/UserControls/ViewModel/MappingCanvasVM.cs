using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TapeReelPacking.Source.Define;
using TapeReelPacking.UI.UserControls.View;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class MappingCanvasVM : BaseVM
    {

        private ObservableCollection<MappingRectangleVM> _mappingRectangles;

        public ObservableCollection<MappingRectangleVM> mappingRectangles
        {
            get => _mappingRectangles;
            set
            {
                _mappingRectangles = value;
                OnPropertyChanged(nameof(mappingRectangles));
            }
        }


        public delegate void ResetMappingResultDelegate(int nTrack);
        public static ResetMappingResultDelegate ResetMappingResultDeleagate;

        public delegate void UpdateMappingResultPageDelegate(int nTrack);
        public static UpdateMappingResultPageDelegate updateMappingResultPageDelegate;


        public delegate void UpdateMappingResultDelegate(VisionResultData resultData, int nTrack, int nDeviceID);
        public static UpdateMappingResultDelegate updateMappingResultDelegate;

        public delegate void InitCanvasMappingDelegate();
        public static InitCanvasMappingDelegate initCanvasMappingDelegate;

        public delegate void SetMappingPageDelegate(int nTrack, int nPage);
        public static SetMappingPageDelegate setMappingPageDelegate;

        public delegate void SetMappingSizeDelegate(double w, double h);
        public static SetMappingSizeDelegate setMappingSizeDelegate;

        private void SetMappingSize(double w, double h)
        {
            mappingCanvasWidth = w;
            mappingCanvasHeight = h;
        }

        private void SetMappingPage(int nTrack, int nPage) => m_nPageID[nTrack] = nPage;
        public MappingCanvasVM()
        {
            InitCanvasMapping();
            ResetMappingResultDeleagate = ResetMappingResult;
            updateMappingResultPageDelegate = UpdateMappingResultPage;
            updateMappingResultDelegate = UpdateMappingResult;
            initCanvasMappingDelegate = InitCanvasMapping;
            setMappingPageDelegate = SetMappingPage;
            setMappingSizeDelegate = SetMappingSize;
            // Example data
        }

        public void InitCanvasMapping()
        {
            if (MainWindow.mainWindow == null)
                return;

            MainWindow.mainWindow.m_nDeviceX = Source.Application.Application.categoriesMappingParam.M_NumberDeviceX;
            MainWindow.mainWindow.m_nDeviceY = Source.Application.Application.categoriesMappingParam.M_NumberDeviceY;
            MainWindow.mainWindow.m_nTotalDevicePerLot = Source.Application.Application.categoriesMappingParam.M_NumberDevicePerLot;
            int nMaxDeviceStep = MainWindow.mainWindow.m_nDeviceX > MainWindow.mainWindow.m_nDeviceY ? MainWindow.mainWindow.m_nDeviceX : MainWindow.mainWindow.m_nDeviceY;
            double dPage = Math.Ceiling((MainWindow.mainWindow.m_nTotalDevicePerLot * 1.0) / (MainWindow.mainWindow.m_nDeviceX * 1.0) / MainWindow.mainWindow.m_nDeviceY * 1.0);
            m_nNumberMappingPage = (int)dPage;

            int nWidthgrid = MappingCanvasWidth > 0 ? (int)MappingCanvasWidth : 500;

            double m_nWidthMappingRect = (int)(nWidthgrid / nMaxDeviceStep / 2.2);
            if (m_nWidthMappingRect > 100)
                m_nWidthMappingRect = 100;
            if (m_nWidthMappingRect < 35)
                m_nWidthMappingRect = 35;

            double m_nStepMappingRect = m_nWidthMappingRect + 1;
            string path = @"/Resources/gray-chip.png";

            mappingRectangles = new ObservableCollection<MappingRectangleVM>();
            MappingRectangleVM rec;
            int nID = 0;
            for (int nTrack = 0; nTrack < 2; nTrack++)
            {

                for (int nDeviceX = 0; nDeviceX < MainWindow.mainWindow.m_nDeviceX; nDeviceX++)
                {
                    for (int nDeviceY = 0; nDeviceY < MainWindow.mainWindow.m_nDeviceY; nDeviceY++)
                    {

                        rec = new MappingRectangleVM();
                        rec.trackID = nTrack;
                        rec.imageSource = new BitmapImage(new Uri(path, UriKind.Relative));
                        rec.imageWidth = 0.95 * m_nWidthMappingRect;
                        rec.imageHeight = 0.95 * m_nWidthMappingRect;
                        rec.mappingID = nDeviceX + 1 + nDeviceY * MainWindow.mainWindow.m_nDeviceX + m_nPageID[nTrack] * MainWindow.mainWindow.m_nDeviceX * MainWindow.mainWindow.m_nDeviceY;
                        rec.fontMappingSize = 0.95 * m_nWidthMappingRect / 3;
                        rec.minMappingWidth = 0.95 * m_nWidthMappingRect;
                        rec.minMappingHeight = 0.95 * m_nWidthMappingRect;
                        rec.imageLeft = m_nStepMappingRect * nDeviceX + nTrack * m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1);
                        rec.imageTop = m_nStepMappingRect * nDeviceY;
                        rec.labelLeft = m_nStepMappingRect * nDeviceX + nTrack * m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1);
                        rec.labelTop = m_nStepMappingRect * nDeviceY + 0.95 * m_nWidthMappingRect / 9;
                        mappingRectangles.Add(rec);

                    }
                }
            }

            //mappingRectangles = null;
            //mappingRectangles = mappingRectangles;

            if (MainWindow.mainWindow != null)
                MainWindow.mainWindow.loadAllStatistic(false);

        }


        public void UpdateMappingResultPage(int nTrack)
        {
            string path = @"/Resources/gray-chip.png";
            string pathFail = @"/Resources/red-chip.png";
            string pathPass = @"/Resources/green-chip.png";

            int nResultTotal;
            //for(int nTrack = 0; nTrack < 2; nTrack++)
            //{
            int nDevice = MainWindow.mainWindow.m_nDeviceX * MainWindow.mainWindow.m_nDeviceY;
            for (int nID = 0; nID < MainWindow.mainWindow.m_nDeviceX * MainWindow.mainWindow.m_nDeviceY; nID++)
            {
                nResultTotal = MainWindow.mainWindow.master.m_Tracks[nTrack].m_VisionResultDatas[nID + m_nPageID[nTrack] * nDevice].m_nResult;

                switch (nResultTotal)
                {
                    case -(int)ERROR_CODE.NOT_INSPECTED:
                        mappingRectangles[nID].imageSource = new BitmapImage(new Uri(path, UriKind.Relative));
                        break;

                    case -(int)ERROR_CODE.PASS:
                        mappingRectangles[nID].imageSource = new BitmapImage(new Uri(pathPass, UriKind.Relative));
                        break;
                    default:
                        mappingRectangles[nID].imageSource = new BitmapImage(new Uri(pathFail, UriKind.Relative));
                        break;
                }

                mappingRectangles[nID].mappingID = (nID + m_nPageID[nTrack] * nDevice + 1).ToString();

            }

            //}

            if (nTrack == 0)
                txtPage1 = (m_nPageID[nTrack] + 1).ToString();
            else
                txtPage2 = (m_nPageID[nTrack] + 1).ToString();


        }



        public void UpdateMappingResult(VisionResultData resultData, int nTrack, int nDeviceID)
        {
            int nDevice = MainWindow.mainWindow.m_nDeviceX * MainWindow.mainWindow.m_nDeviceY;

            if (nDeviceID >= (m_nPageID[nTrack] + 1) * nDevice)
            {
                m_nPageID[nTrack]++;
                UpdateMappingResultPage(nTrack);
            }

            string path = @"/Resources/gray-chip.png";
            switch (resultData.m_nResult)
            {
                case -(int)ERROR_CODE.NOT_INSPECTED:
                    path = @"/Resources/gray-chip.png";
                    break;

                case -(int)ERROR_CODE.PASS:
                    path = @"/Resources/green-chip.png";
                    break;
                default:
                    path = @"/Resources/red-chip.png";
                    break;
            }
        }


        public void ResetMappingResult(int nTrackID = (int)TRACK_TYPE.TRACK_CAM1)
        {
            int nDevice = MainWindow.mainWindow.m_nDeviceX * MainWindow.mainWindow.m_nDeviceY;

            string path = @"/Resources/gray-chip.png";
            for (int nID = 0; nID < nDevice; nID++)
                mappingRectangles[nID].imageSource = new BitmapImage(new Uri(path, UriKind.Relative));
        }


        private ActionCommand btnPageClickCommand1;

        public ICommand btnPageClickCommand
        {
            get
            {
                if (btnPageClickCommand1 == null)
                {
                    btnPageClickCommand1 = new ActionCommand(btnPageClick);
                }

                return btnPageClickCommand1;
            }
        }

        private void btnPageClick()
        {
            previousPage(0);
        }

        private ActionCommand btnNextPageCommand1;

        public ICommand btnNextPageCommand
        {
            get
            {
                if (btnNextPageCommand1 == null)
                {
                    btnNextPageCommand1 = new ActionCommand(btnNextPage);
                }

                return btnNextPageCommand1;
            }
        }

        private void btnNextPage()
        {
            NextPage(0);

        }

        private void NextPage(int nTrack)
        {
            m_nPageID[nTrack]++;
            if (m_nPageID[nTrack] >= m_nNumberMappingPage)
            {
                m_nPageID[nTrack] = m_nNumberMappingPage - 1;
                return;
            }
            if (nTrack == 0)
                txtPage1 = (m_nPageID[nTrack] + 1).ToString();
            else
                txtPage2 = (m_nPageID[nTrack] + 1).ToString();

            UpdateMappingResultPage(nTrack);

        }

        private ActionCommand btnPage2ClickCommand1;

        public ICommand btnPage2ClickCommand
        {
            get
            {
                if (btnPage2ClickCommand1 == null)
                {
                    btnPage2ClickCommand1 = new ActionCommand(btnPage2Click);
                }

                return btnPage2ClickCommand1;
            }
        }

        private void btnPage2Click()
        {
            previousPage(1);
        }

        public void previousPage(int nTrack)
        {
            m_nPageID[nTrack]--;
            if (m_nPageID[nTrack] < 0)
            {
                m_nPageID[nTrack] = 0;
                return;
            }

            if (nTrack == 0)
                txtPage1 = (m_nPageID[nTrack] + 1).ToString();
            else
                txtPage2 = (m_nPageID[nTrack] + 1).ToString();

            UpdateMappingResultPage(nTrack);
        }

        private ActionCommand btnPage2NextCommand1;

        public ICommand btnPage2NextCommand
        {
            get
            {
                if (btnPage2NextCommand1 == null)
                {
                    btnPage2NextCommand1 = new ActionCommand(btnPage2Next);
                }

                return btnPage2NextCommand1;
            }
        }

        private void btnPage2Next()
        {
            NextPage(1);
        }

        private double borderLeft1;

        public double borderLeft { get => borderLeft1; set => SetProperty(ref borderLeft1, value); }

        private double borderTop1;

        public double borderTop { get => borderTop1; set => SetProperty(ref borderTop1, value); }

        private System.Windows.Visibility borderVisible1;

        public System.Windows.Visibility borderVisible { get => borderVisible1; set => SetProperty(ref borderVisible1, value); }

        private string txtPage11 = "1";

        public string txtPage1 { get => txtPage11; set => SetProperty(ref txtPage11, value); }

        private string txtPage21 = "1";
        private int[] m_nPageID { set; get; } = { 0, 0 };
        private int m_nNumberMappingPage;

        public string txtPage2 { get => txtPage21; set => SetProperty(ref txtPage21, value); }

        private double mappingCanvasWidth = 200;
        public double MappingCanvasWidth
        {
            get => mappingCanvasWidth;
            set => SetProperty(ref mappingCanvasWidth, value);
        }

        private double mappingCanvasHeight = 300;
        public double MappingCanvasHeight
        {
            get => mappingCanvasHeight;
            set => SetProperty(ref mappingCanvasHeight, value);
        }


    }
}
