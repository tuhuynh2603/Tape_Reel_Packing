using Magnus_WPF_1.Source.Application;
using Magnus_WPF_1.Source.Define;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = Magnus_WPF_1.Source.Application.Application;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using Point = System.Windows.Point;

namespace Magnus_WPF_1.UI.UserControls.View
{
    /// <summary>
    /// Interaction logic for StatisticView.xaml
    /// </summary>
    public partial class StatisticView : System.Windows.Controls.UserControl
    {

        public ObservableCollection<StatisticData> listSummary = new ObservableCollection<StatisticData>();
        public StatisticView( MainWindow mainWindow)
        {
            InitializeComponent();

            
            listSummary.Add(new StatisticData() { nameSummary = "Checked", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.WhiteSmoke});
            listSummary.Add(new StatisticData() { nameSummary = "Passed", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0,  color = Brushes.Yellow });
            listSummary.Add(new StatisticData() { nameSummary = "Failed", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.Red });
            listSummary.Add(new StatisticData() { nameSummary = "Yield %", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.Lime });

            lboxStatistic.ItemsSource = listSummary;
            InitCanvasMapping();


        }
        public void UpdateValueStatistic(int result, int nTrack)
        {
            if (result == -(int)ERROR_CODE.NOT_INSPECTED)
                return;
            if (nTrack == 0)
            {
                listSummary[0].valueSummary_Camera1 += 1;
                if (result == 0)
                    listSummary[1].valueSummary_Camera1 += 1;
                else 
                      listSummary[2].valueSummary_Camera1 += 1;

                listSummary[3].valueSummary_Camera1 = Math.Round((listSummary[1].valueSummary_Camera1 / listSummary[0].valueSummary_Camera1) * 100, 2);

            }
            else
            {
                listSummary[0].valueSummary_Camera2 += 1;
                if (result == 0)
                    listSummary[1].valueSummary_Camera2 += 1;
                else
                    listSummary[2].valueSummary_Camera2 += 1;

                listSummary[3].valueSummary_Camera2 = Math.Round((listSummary[1].valueSummary_Camera2 / listSummary[0].valueSummary_Camera2) * 100, 2);

            }
            lboxStatistic.ItemsSource = null; lboxStatistic.ItemsSource = listSummary;
        }
        public void ClearStatistic()
        {
            foreach (var def in listSummary)
            {
                def.valueSummary_Camera1 = 0;
                def.valueSummary_Camera2 = 0;
            }
            lboxStatistic.ItemsSource = null;
            lboxStatistic.ItemsSource = listSummary;
        }

        int m_nWidthMappingRect = 100;
        int m_nStepMappingRect = 102;
        public Point m_CanvasMovePoint = new Point(0, 0);
        int m_nNumberMappingPage = 1;
        //int m_nWidthMappingPageRect = 100;
        //int m_nStepMappingPageRect = 102;
        public Point m_CanvasMovePagePoint = new Point(0, 0);
        int[] m_nPageID = {0,0 };

        Image[][] arr_imageMapping = new Image[2][];
        Label[][] arr_textBlockMapping = new Label[2][];
        Border[] border_boundingbox_clicked = new Border[2];
        public void InitCanvasMapping()
        {


            MainWindow.mainWindow.m_nDeviceX = Source.Application.Application.categoriesMappingParam.M_NumberDeviceX;
            MainWindow.mainWindow.m_nDeviceY = Source.Application.Application.categoriesMappingParam.M_NumberDeviceY;
            MainWindow.mainWindow.m_nTotalDevicePerLot = Source.Application.Application.categoriesMappingParam.M_NumberDevicePerLot;
            double dPage = Math.Ceiling((MainWindow.mainWindow.m_nTotalDevicePerLot *1.0) / (MainWindow.mainWindow.m_nDeviceX * 1.0 )/ MainWindow.mainWindow.m_nDeviceY * 1.0);
            m_nNumberMappingPage = (int)dPage;

            int nMaxDeviceStep = MainWindow.mainWindow.m_nDeviceX > MainWindow.mainWindow.m_nDeviceY ? MainWindow.mainWindow.m_nDeviceX : MainWindow.mainWindow.m_nDeviceY;



            int nWidthgrid = Grid_CanVas_Mapping.ActualWidth > 0 ? (int)Grid_CanVas_Mapping.ActualWidth : 500;



            m_nWidthMappingRect = (int)(nWidthgrid / nMaxDeviceStep / 2.2);
            if (m_nWidthMappingRect > 100)
                m_nWidthMappingRect = 100;
            if (m_nWidthMappingRect < 35)
                m_nWidthMappingRect = 35;

            m_nStepMappingRect = m_nWidthMappingRect + 1;
            string path = @"/Resources/gray-chip.png";

            if (canvas_Mapping.Children != null)
                canvas_Mapping.Children.Clear();

            for (int nTrack = 0; nTrack < 2; nTrack++)
            {


                arr_imageMapping[nTrack] = new Image[MainWindow.mainWindow.m_nDeviceX * MainWindow.mainWindow.m_nDeviceY];
                arr_textBlockMapping[nTrack] = new Label[MainWindow.mainWindow.m_nDeviceX * MainWindow.mainWindow.m_nDeviceY];
                int nID = 0;
                for (int nDeviceX = 0; nDeviceX < MainWindow.mainWindow.m_nDeviceX; nDeviceX++)
                {
                    for (int nDeviceY = 0; nDeviceY < MainWindow.mainWindow.m_nDeviceY; nDeviceY++)
                    {
                        nID = nDeviceX + nDeviceY * MainWindow.mainWindow.m_nDeviceX;
                        //Canvas canvas_temp = new Canvas();
                        arr_imageMapping[nTrack][nID] = new Image();
                        arr_imageMapping[nTrack][nID].Source = new BitmapImage(new Uri(path, UriKind.Relative));
                        arr_imageMapping[nTrack][nID].Width = 0.95 * m_nWidthMappingRect;
                        arr_imageMapping[nTrack][nID].Height = 0.95 * m_nWidthMappingRect;
                        arr_textBlockMapping[nTrack][nID] = new Label();
                        arr_textBlockMapping[nTrack][nID].Content = nDeviceX + 1 + nDeviceY * MainWindow.mainWindow.m_nDeviceX;
                        arr_textBlockMapping[nTrack][nID].FontSize = 0.95 * m_nWidthMappingRect / 3;
                        arr_textBlockMapping[nTrack][nID].MinWidth = 0.95 * m_nWidthMappingRect;
                        arr_textBlockMapping[nTrack][nID].Foreground = new SolidColorBrush(Colors.Yellow);
                        arr_textBlockMapping[nTrack][nID].HorizontalContentAlignment = HorizontalAlignment.Center;
                        Canvas.SetLeft(arr_imageMapping[nTrack][nID], m_nStepMappingRect * nDeviceX + nTrack * m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1));
                        Canvas.SetTop(arr_imageMapping[nTrack][nID], m_nStepMappingRect * nDeviceY);
                        canvas_Mapping.Children.Add(arr_imageMapping[nTrack][nID]);

                        Canvas.SetLeft(arr_textBlockMapping[nTrack][nID], m_nStepMappingRect * nDeviceX + nTrack * m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1));
                        Canvas.SetTop(arr_textBlockMapping[nTrack][nID], m_nStepMappingRect * nDeviceY + arr_textBlockMapping[nTrack][nID].FontSize / 3);                    
                        canvas_Mapping.Children.Add(arr_textBlockMapping[nTrack][nID]);


                    }
                }
                border_boundingbox_clicked[nTrack] = new Border();
                border_boundingbox_clicked[nTrack].Width = m_nWidthMappingRect;
                border_boundingbox_clicked[nTrack].Height = m_nWidthMappingRect;
                border_boundingbox_clicked[nTrack].BorderThickness = new Thickness(0);
                border_boundingbox_clicked[nTrack].BorderBrush = new SolidColorBrush(Colors.Yellow);
                Canvas.SetLeft(border_boundingbox_clicked[nTrack], 0 + nTrack * m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1));
                Canvas.SetTop(border_boundingbox_clicked[nTrack], 0);
                canvas_Mapping.Children.Add(border_boundingbox_clicked[nTrack]);

                Border borderTemp = new Border();
                borderTemp.Width = (m_nStepMappingRect) * MainWindow.mainWindow.m_nDeviceX;
                borderTemp.Height = (m_nStepMappingRect) * MainWindow.mainWindow.m_nDeviceY;
                borderTemp.BorderThickness = new Thickness(1);
                borderTemp.BorderBrush = new SolidColorBrush(Colors.Yellow);
                Canvas.SetLeft(borderTemp, 0 + nTrack * m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1));
                Canvas.SetTop(borderTemp, 0);
                canvas_Mapping.Children.Add(borderTemp);

                //canvas_Mapping[nTrack].MouseLeftButtonDown += StatisticView_MouseLeftButtonDown;
            }

            border_boundingbox_moving.Width = m_nWidthMappingRect;
            border_boundingbox_moving.Height = m_nWidthMappingRect;
            border_boundingbox_moving.BorderThickness = new Thickness(0);
            border_boundingbox_moving.BorderBrush = new SolidColorBrush(Colors.WhiteSmoke);
            Canvas.SetLeft(border_boundingbox_moving, 0);
            Canvas.SetTop(border_boundingbox_moving, 0);
            canvas_Mapping.Children.Add(border_boundingbox_moving);



            canvas_Mapping.Width = m_nStepMappingRect * MainWindow.mainWindow.m_nDeviceX + m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1);
            canvas_Mapping.Height = m_nStepMappingRect * MainWindow.mainWindow.m_nDeviceY;

            //Canvas.SetTop(canvas_Mapping_NextPage, canvas_Mapping.Height);

        }

        //private void StatisticView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        int nDeviceID = -1;

        private int Check_mapping_Cursor_ID(Point cur_point, bool bIsclicked, ref int nTrackID)
        {

            nTrackID = 0;
            if (cur_point.X >= m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1))
                nTrackID = 1;

            int nIDX = (int)((cur_point.X - (nTrackID * m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 2))) / m_nStepMappingRect);
            int nIDY = (int)(cur_point.Y / m_nStepMappingRect);

            if (nDeviceID != nIDX + nIDY * MainWindow.mainWindow.m_nDeviceX || bIsclicked)
            {
                nDeviceID = nIDX + nIDY * MainWindow.mainWindow.m_nDeviceX;
                if (nIDX < MainWindow.mainWindow.m_nDeviceX && nIDY < MainWindow.mainWindow.m_nDeviceY)
                {
                    Canvas.SetLeft(border_boundingbox_moving, m_nStepMappingRect * nIDX +  nTrackID * m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1));
                    Canvas.SetTop(border_boundingbox_moving, m_nStepMappingRect * nIDY);
                    border_boundingbox_moving.BorderThickness = new Thickness(2);
                }
                else
                {
                    border_boundingbox_moving.BorderThickness = new Thickness(0);
                    //border_boundingbox_clicked.BorderThickness = new Thickness(0);

                }
            }

            if (bIsclicked)
            {
                Canvas.SetLeft(border_boundingbox_clicked[nTrackID], m_nStepMappingRect * nIDX + nTrackID * m_nWidthMappingRect * (MainWindow.mainWindow.m_nDeviceX + 1));
                Canvas.SetTop(border_boundingbox_clicked[nTrackID], m_nStepMappingRect * nIDY);
                border_boundingbox_clicked[nTrackID].BorderThickness = new Thickness(2);
            }

            //canvas_Mapping.Children.RemoveAt(nIDX + nIDY * 10);
            return nIDX + nIDY * MainWindow.mainWindow.m_nDeviceX;

        }
        private void canvas_Mapping_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int nTrackId = 0;
            m_CanvasMovePoint = e.GetPosition(canvas_Mapping);
            int nID = Check_mapping_Cursor_ID(m_CanvasMovePoint, true, ref nTrackId);
            MainWindow.mainWindow.master.m_Tracks[nTrackId].m_nCurrentClickMappingID = nID + m_nPageID[nTrackId] * MainWindow.mainWindow.m_nDeviceX * MainWindow.mainWindow.m_nDeviceY;

            MainWindow.mainWindow.master.m_Tracks[nTrackId].CheckInspectionOnlineThread();
            if (MainWindow.mainWindow.m_bSequenceRunning || MainWindow.mainWindow.bEnableOfflineInspection)
                Master.m_OfflineTriggerSnapEvent[nTrackId].Set();
            else
                Master.InspectEvent[nTrackId].Set();
        }

        private void canvas_Mapping_MouseMove(object sender, MouseEventArgs e)
        {
            m_CanvasMovePoint = e.GetPosition(canvas_Mapping);
            int nTrack = 0;
            Check_mapping_Cursor_ID(m_CanvasMovePoint, false, ref nTrack);
        }

        private void canvas_Mapping_MouseLeave(object sender, MouseEventArgs e)
        {
            border_boundingbox_moving.BorderThickness = new Thickness(0);
            nDeviceID = -1;
        }

        //private void InitCanvasMappingPage()
        //{


        //    double dPage = MainWindow.mainWindow.m_nTotalDevicePerLot * 1.0 / MainWindow.mainWindow.m_nDeviceX * 1.0 / MainWindow.mainWindow.m_nDeviceY * 1.0;
        //    m_nNumberMappingPage = (int)dPage;
        //    if (m_nNumberMappingPage < dPage)
        //        m_nNumberMappingPage = (int)dPage + 1;
        //    int nMaxDeviceStep = m_nNumberMappingPage;
        //    if (canvas_Mapping_2.Children != null)
        //    {
        //        //canvas_Mapping.Children.RemoveRange(0, canvas_Mapping.Children.Count);
        //        canvas_Mapping_2.Children.Clear();

        //    }


        //    int nWidthgrid = Grid_CanVas_Mapping.ActualWidth > 0 ? (int)Grid_CanVas_Mapping.ActualWidth : 500;


        //    m_nWidthMappingPageRect = nWidthgrid / (int)(m_nNumberMappingPage * 1.3);
        //    if (m_nWidthMappingPageRect > 100)
        //        m_nWidthMappingPageRect = 100;
        //    if (m_nWidthMappingPageRect < 10)
        //        m_nWidthMappingPageRect = 10;

        //    m_nStepMappingPageRect = m_nWidthMappingPageRect + 5;
        //    string path = @"/Resources/blue-chip.png";
        //    arr_imageMapping_2 = new Image[m_nNumberMappingPage];
        //    arr_textBlockMapping_2 = new Label[m_nNumberMappingPage];
        //    int nID = 0;
        //    for (int nPage = 0; nPage < m_nNumberMappingPage; nPage++)
        //    {
        //        nID = nPage;
        //        //Canvas canvas_temp = new Canvas();
        //        arr_imageMapping_2[nID] = new Image();
        //        arr_imageMapping_2[nID].Source = new BitmapImage(new Uri(path, UriKind.Relative));
        //        arr_imageMapping_2[nID].Width = 0.95 * m_nWidthMappingPageRect;
        //        arr_imageMapping_2[nID].Height = 0.95 * m_nWidthMappingPageRect;
        //        arr_textBlockMapping_2[nID] = new Label();
        //        arr_textBlockMapping_2[nID].Content = nID + 1;
        //        arr_textBlockMapping_2[nID].FontSize = 0.95 * m_nWidthMappingPageRect / 3;
        //        arr_textBlockMapping_2[nID].MinWidth = 0.95 * m_nWidthMappingPageRect;
        //        arr_textBlockMapping_2[nID].Foreground = new SolidColorBrush(Colors.Yellow);
        //        arr_textBlockMapping_2[nID].HorizontalContentAlignment = HorizontalAlignment.Center;
        //        Canvas.SetLeft(arr_imageMapping_2[nID], m_nStepMappingPageRect * nPage);
        //        Canvas.SetTop(arr_imageMapping_2[nID], 0);
        //        canvas_Mapping_2.Children.Add(arr_imageMapping_2[nID]);

        //        Canvas.SetLeft(arr_textBlockMapping_2[nID], m_nStepMappingPageRect * nPage);
        //        Canvas.SetTop(arr_textBlockMapping_2[nID], 0 + arr_textBlockMapping_2[nID].FontSize / 3);
        //        canvas_Mapping_2.Children.Add(arr_textBlockMapping_2[nID]);
        //    }

        //    border_boundingbox_focus_2.Width = m_nWidthMappingPageRect;
        //    border_boundingbox_focus_2.Height = m_nWidthMappingPageRect;
        //    border_boundingbox_focus_2.BorderThickness = new Thickness(0);
        //    border_boundingbox_focus_2.BorderBrush = new SolidColorBrush(Colors.WhiteSmoke);
        //    Canvas.SetLeft(border_boundingbox_focus_2, 0);
        //    Canvas.SetTop(border_boundingbox_focus_2, 0);
        //    canvas_Mapping_2.Children.Add(border_boundingbox_focus_2);


        //    border_boundingbox_clicked_2.Width = m_nWidthMappingPageRect;
        //    border_boundingbox_clicked_2.Height = m_nWidthMappingPageRect;
        //    border_boundingbox_clicked_2.BorderThickness = new Thickness(0);
        //    border_boundingbox_clicked_2.BorderBrush = new SolidColorBrush(Colors.Yellow);
        //    Canvas.SetLeft(border_boundingbox_clicked_2, 0);
        //    Canvas.SetTop(border_boundingbox_clicked_2, 0);
        //    canvas_Mapping_2.Children.Add(border_boundingbox_clicked_2);
        //    canvas_Mapping_2.Width = m_nStepMappingPageRect * m_nNumberMappingPage;
        //    canvas_Mapping_2.Height = m_nWidthMappingPageRect;
        //}

        //private int Check_mapping_Page_Cursor_ID(Point cur_point, bool bIsclicked)
        //{
        //    int nIDX = (int)(cur_point.X / m_nStepMappingPageRect);
        //    //int nIDY = (int)(cur_point.Y / m_nStepMappingPageRect);

        //    if (m_nPageID != nIDX)
        //    {
        //        if (nIDX < m_nNumberMappingPage)
        //        {
        //            Canvas.SetLeft(border_boundingbox_focus_2, m_nStepMappingPageRect * nIDX);
        //            Canvas.SetTop(border_boundingbox_focus_2, 0);
        //            border_boundingbox_focus_2.BorderThickness = new Thickness(2);
        //        }
        //        else
        //        {
        //            border_boundingbox_focus_2.BorderThickness = new Thickness(0);
        //            //border_boundingbox_clicked.BorderThickness = new Thickness(0);

        //        }
        //    }

        //    if (bIsclicked)
        //    {
        //        Canvas.SetLeft(border_boundingbox_clicked_2, m_nStepMappingPageRect * nIDX);
        //        Canvas.SetTop(border_boundingbox_clicked_2, 0);
        //        border_boundingbox_clicked_2.BorderThickness = new Thickness(2);
        //    }

        //    //canvas_Mapping.Children.RemoveAt(nIDX + nIDY * 10);
        //    return nIDX;

        //}

        //private void canvas_Mapping_NextPage_MouseMove(object sender, MouseEventArgs e)
        //{
        //    m_CanvasMovePagePoint = e.GetPosition(canvas_Mapping_2);
        //    Check_mapping_Page_Cursor_ID(m_CanvasMovePagePoint, false);
        //}

        //private void canvas_Mapping_NextPage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    m_CanvasMovePagePoint = e.GetPosition(canvas_Mapping_2);
        //    int nCurrentPageID = Check_mapping_Page_Cursor_ID(m_CanvasMovePagePoint, true);

        //    if (m_nPageID == nCurrentPageID)
        //        return;

        //    m_nPageID = nCurrentPageID;
        //    UpdateMappingResultPage();
        //}


        private void btn_Previous_Page_Click(object sender, RoutedEventArgs e)
        {
            previousPage(0);
        }

        private void btn_Next_Page_Click(object sender, RoutedEventArgs e)
        {

            NextPage(0);
        }

        private void btn_Previous_Page2_Click(object sender, RoutedEventArgs e)
        {
            previousPage(1);
        }

        private void btn_Next_Page2_Click(object sender, RoutedEventArgs e)
        {

            NextPage(1);
        }

        public void previousPage(int nTrack)
        {
            m_nPageID[nTrack]--;
            if (m_nPageID[nTrack] < 0)
            {
                m_nPageID[nTrack] = 0;
                return;
            }

            if(nTrack == 0)
                text_Current_Page.Text = (m_nPageID[nTrack] + 1).ToString();
            else
                text_Current_Page2.Text = (m_nPageID[nTrack] + 1).ToString();

            UpdateMappingResultPage(nTrack);
        }

        public void NextPage(int nTrack)
        {
            m_nPageID[nTrack]++;
            if (m_nPageID[nTrack] >= m_nNumberMappingPage)
            {
                m_nPageID[nTrack] = m_nNumberMappingPage - 1;
                return;
            }
            if (nTrack == 0)
                text_Current_Page.Text = (m_nPageID[nTrack] + 1).ToString();
            else
                text_Current_Page2.Text = (m_nPageID[nTrack] + 1).ToString();

            UpdateMappingResultPage(nTrack);
        }

        public void UpdateMappingResultPage(int nTrack)
        {
            string path = @"/Resources/gray-chip.png";
            string pathFail = @"/Resources/red-chip.png";
            string pathPass = @"/Resources/green-chip.png";

            int nResultTotal;
            //for(int nTrack = 0; nTrack < 2; nTrack++)
            //{
                for (int nID = 0; nID < arr_imageMapping[nTrack].Length; nID++)
                {
                    nResultTotal = MainWindow.mainWindow.master.m_Tracks[nTrack].m_VisionResultDatas[nID + m_nPageID[nTrack] * arr_imageMapping[nTrack].Length].m_nResult;

                    switch (nResultTotal)
                    {
                        case -(int)ERROR_CODE.NOT_INSPECTED:
                            arr_imageMapping[nTrack][nID].Source = new BitmapImage(new Uri(path, UriKind.Relative));
                            break;

                        case -(int)ERROR_CODE.PASS:
                            arr_imageMapping[nTrack][nID].Source = new BitmapImage(new Uri(pathPass, UriKind.Relative));
                            break;
                        default:
                            arr_imageMapping[nTrack][nID].Source = new BitmapImage(new Uri(pathFail, UriKind.Relative));
                            break;
                    }

                    arr_textBlockMapping[nTrack][nID].Content = (nID + m_nPageID[nTrack] * arr_imageMapping[nTrack].Length + 1).ToString();

                }

            //}

            if(nTrack == 0)
                text_Current_Page.Text = (m_nPageID[nTrack] + 1).ToString();
            else
                text_Current_Page2.Text = (m_nPageID[nTrack] + 1).ToString();


        }

        //private void canvas_Mapping_NextPage_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    border_boundingbox_focus_2.BorderThickness = new Thickness(0);
        //    //m_nPageID = -1;
        //}


        public void UpdateMappingResult(VisionResultData resultData, int nTrack, int nDeviceID)
        {
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


            //if (resultData.m_nResult < 0)
            //    path = @"/Resources/red-chip.png";

            if (nDeviceID >= (m_nPageID[nTrack] + 1) * arr_imageMapping[nTrack].Length)
            {
                return;

                m_nPageID[nTrack] = (int)Math.Round((float)nDeviceID / arr_imageMapping[nTrack].Length);
                //m_nPageID[nTrack]++;
                //UpdateMappingResultPage(nTrack);
                if (nTrack == 0)
                    text_Current_Page.Text = (m_nPageID[nTrack] + 1).ToString();
                else
                    text_Current_Page2.Text = (m_nPageID[nTrack] + 1).ToString();
            } 

            arr_imageMapping[nTrack][nDeviceID % arr_imageMapping[nTrack].Length].Source = new BitmapImage(new Uri(path, UriKind.Relative));

        }
        public void ResetMappingResult(int nTrackID = (int)TRACK_TYPE.TRACK_CAM1)
        {
            string path = @"/Resources/gray-chip.png";
            for (int nID = 0; nID < arr_imageMapping[nTrackID].Length; nID++)
                arr_imageMapping[nTrackID][nID].Source = new BitmapImage(new Uri(path, UriKind.Relative));
        }

        private void Grid_CanVas_Mapping_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitCanvasMapping();
        }
    }

    public class StatisticData
    {
        public string nameSummary { get; set; }
        public double valueSummary_Camera1 { get; set; }
        public double valueSummary_Camera2 { get; set; }

        public System.Windows.Media.Brush color { get; set; }
    }
}
