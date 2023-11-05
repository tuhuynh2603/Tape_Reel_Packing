using Emgu.CV;
using Emgu.CV.Structure;
using Magnus_WPF_1.Source.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Magnus_WPF_1.UI.UserControls
{
    /// <summary>
    /// Interaction logic for DefectInfor.xaml
    /// </summary>
    public partial class DefectInfor : UserControl
    {
        public DefectInfor()
        {
            InitializeComponent();
        }

        public struct DebugInfors
        {
            public Mat mat_Image { get; set; }
            public Mat mat_Region { get; set; }
            public string str_Message { get; set; }
            public string str_Step { get; set; }
            //public DebugInfors(Mat image, Mat region, string strMessage)
            //{
            //    mat_Image = image.Clone();
            //    mat_Region = region.Clone();
            //    str_Message = strMessage;
            //}
        };

        bool bIsSelected = false;
        private void lvDefect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (bIsSelected)
                return;


            if (this.lvDefect.SelectedItems.Count > 0)
            {
                // Access the selected item(s)
                foreach (var selectedItem in this.lvDefect.SelectedItems)
                {
                    // Check the type of the item and cast it as necessary
                    
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
                            MainWindow.mainWindow.master.m_Tracks[MainWindow.activeImageDock.trackID].m_imageViews[0].ClearOverlay();
                            MainWindow.mainWindow.master.m_Tracks[MainWindow.activeImageDock.trackID].m_imageViews[0].UpdateUIImageMono(Track.BitmapToByteArray(imgg.ToBitmap()));
                            //MainWindow.mainWindow.master.m_Tracks[MainWindow.activeImageDock.trackID].m_imageViews[0].image.Source = imgg;
                            Mat matRegion = item.mat_Region;
                            MainWindow.mainWindow.master.m_Tracks[MainWindow.activeImageDock.trackID].m_imageViews[0].DrawRegionOverlay(ref matRegion);
                        });
                                                   
                        bIsSelected = false;

                        return;
                    }
                }
            }

        }

        private void lvDefect_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
