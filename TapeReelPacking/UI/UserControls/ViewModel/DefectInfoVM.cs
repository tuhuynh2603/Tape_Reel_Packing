using Emgu.CV;
using Emgu.CV.Structure;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Define;
using TapeReelPacking.UI.UserControls.View;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class DefectInfoVM:BaseVM
    {
        public ICommand SelectionChangedCommand { set; get; }
        //public ICommand FocusableChangedCommand { set; get; }
        //public ICommand PreviewMouseWheelCommand { set; get; }
        //public ICommand MouseWheelCommand { set; get; }


        private DebugInfors _selectedItem;

        public DebugInfors selectedItem
        {
            set => SetProperty(ref _selectedItem, value);

            get => _selectedItem;
        }


        //private double _verticalOffsetValue;
        //public double verticalOffsetValue
        //{
        //    set => SetProperty(ref _verticalOffsetValue, value);

        //    get => _verticalOffsetValue;
        //}


        public DefectInfoVM()
        {
            SelectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(OnSelectionChanged);
            //FocusableChangedCommand = new DelegateCommand<MouseWheelEventArgs>(OnFocusableChanged);
            //MouseWheelCommand = new DelegateCommand<MouseWheelEventArgs>(OnMouseWheel);
        }

        //private void OnMouseWheel(MouseWheelEventArgs e)
        //{
        //    e.
        //    scv_StepDebugScrollView.ScrollToVerticalOffset(scv_StepDebugScrollView.VerticalOffset - e.Delta);
        //    e.Handled = true;
        //}

        //private void OnPreviewMouseWheel(MouseWheelEventArgs e)
        //{
        //    scv_StepDebugScrollView.ScrollToVerticalOffset(scv_StepDebugScrollView.VerticalOffset - e.Delta);
        //    e.Handled = true;
        //}

        //private void OnFocusableChanged(MouseWheelEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}


        bool bIsSelected = false;
        public int m_TrackDebugging = 0;
        private void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (bIsSelected)
                    return;

            if (selectedItem is DebugInfors item)
            {

                bIsSelected = true;
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    //master.trackSF[trackId].imageViewSF[0].image.Source = master.trackSF[trackId].imageViewSF[0].btmSource;
                    Mat matImage = item.mat_Image;
                    Image<Gray, byte> imgg = matImage.ToImage<Gray, byte>().Clone();
                    //m_imageViews[0].bufferImage = BitmapToByteArray(imgg.ToBitmap());
                    //byte[] buffer = Track.BitmapToByteArray(imgg.ToBitmap());
                    MainWindow.mainWindow.master.m_Tracks[m_TrackDebugging].m_imageViews[0].ClearOverlay();
                    MainWindow.mainWindow.master.m_Tracks[m_TrackDebugging].m_imageViews[0].UpdateUIImageMono(Track.BitmapToByteArray(imgg.ToBitmap()));
                    //MainWindow.mainWindow.master.m_Tracks[MainWindow.activeImageDock.trackID].m_imageViews[0].image.Source = imgg;
                    Mat matRegion = item.mat_Region;
                    SolidColorBrush color = new SolidColorBrush(Colors.Cyan);
                    MainWindow.mainWindow.master.m_Tracks[m_TrackDebugging].m_imageViews[0].DrawRegionOverlay(matRegion, color);
                });

                bIsSelected = false;
 
                return;
            }

            }

        }
}
