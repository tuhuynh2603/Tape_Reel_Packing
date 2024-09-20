using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Expression.Interactivity.Core;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Define;
using TapeReelPacking.UI.UserControls.View;
using static TapeReelPacking.Source.Hardware.SDKHrobot.Global;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class StepDebugVM:BaseVM, ICustomUserControl
    {


        private Visibility _isVisible = Visibility.Collapsed;
        public Visibility isVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(isVisible));
            }
        }



        private ObservableCollection<DebugInfors> listStepDebugInfors1;

        public ObservableCollection<DebugInfors> listStepDebugInfors
        {
            get => listStepDebugInfors1;
            set
            {
                listStepDebugInfors1 = value;
                OnPropertyChanged(nameof(listStepDebugInfors));
            }
        }

        private DebugInfors _selectedItem;

        public DebugInfors selectedItem
        {
            set => SetProperty(ref _selectedItem, value);

            get => _selectedItem;
        }

        public ICommand SelectionChangedCommand { set; get; }


        private DragDropUserControlVM _dragDropVM { set; get; }

        public void RegisterUserControl()
        {
            //_dragDropVM.RegisterMoveGrid();
            //_dragDropVM.RegisterResizeGrid();
        }


        public StepDebugVM(DragDropUserControlVM dragDropVM)
        {
            _dragDropVM = dragDropVM;
            _dragDropVM.RegisterMoveGrid();
            _dragDropVM.RegisterResizeGrid();

            SelectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(OnSelectionChanged);
            listStepDebugInfors = new ObservableCollection<DebugInfors>();
            doStepDebugDelegate = DoStepDebug;
            stepDebugSizeChangedCmd = new DelegateCommand<Size?>(PerformstepDebugSizeChangedCmd);
        }

        public int m_TrackDebugging = 0;
        private void OnSelectionChanged(SelectionChangedEventArgs e)
        {

            if (selectedItem is DebugInfors item)
            {

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Mat matImage = item.mat_Image;
                    Image<Gray, byte> imgg = matImage.ToImage<Gray, byte>().Clone();
                    MainWindowVM.activeImageDock.ClearOverlay();
                    MainWindowVM.activeImageDock.UpdateUIImageMono(Track.BitmapToByteArray(imgg.ToBitmap()));
                    Mat matRegion = item.mat_Region;
                    SolidColorBrush color = new SolidColorBrush(Colors.Cyan);
                    MainWindowVM.activeImageDock.DrawRegionOverlay(matRegion, color);
                });
 
                return;
            }

        }

        public delegate void DoStepDebugDelegate(int nTrack);
        public static DoStepDebugDelegate doStepDebugDelegate;

        public void DoStepDebug(int nTrack)
        {


            m_TrackDebugging = MainWindowVM.activeImageDock.trackID;
            if (MainWindowVM.activeImageDock.btmSource.Width < 0)
                return;

            MainWindowVM.master.m_Tracks[m_TrackDebugging].m_InspectionCore.LoadImageToInspection(MainWindowVM.activeImageDock.btmSource);
            MainWindowVM.master.m_Tracks[m_TrackDebugging].DebugFunction();
            listStepDebugInfors = null;
            listStepDebugInfors = new ObservableCollection<DebugInfors> (MainWindowVM.master.m_Tracks[m_TrackDebugging].m_StepDebugInfors);
            return;
        }






        //Size change property
        private double _gridWidth;
        public double GridWidth
        {
            get => _gridWidth;
            set
            {
                _gridWidth = value;
                OnPropertyChanged(nameof(GridWidth));
            }
        }

        private double _gridHeight;
        public double GridHeight
        {
            get => _gridHeight;
            set
            {
                _gridHeight = value;
                OnPropertyChanged(nameof(GridHeight));
            }
        }

        public DelegateCommand<Size?> stepDebugSizeChangedCmd { get; set; }
        private void PerformstepDebugSizeChangedCmd(Size? newSize)
        {
            GridWidth = newSize.Value.Width;
            GridHeight = newSize.Value.Height;
        }


    }
}
