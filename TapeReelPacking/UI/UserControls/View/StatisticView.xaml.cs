using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Define;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = TapeReelPacking.Source.Application.Application;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using Point = System.Windows.Point;
using static TapeReelPacking.UI.UserControls.ViewModel.MappingCanvasVM;

namespace TapeReelPacking.UI.UserControls.View
{
    /// <summary>
    /// Interaction logic for StatisticView.xaml
    /// </summary>
    public partial class StatisticView : UserControl
    {

        public StatisticView()
        {
            InitializeComponent();


            //InitCanvasMapping();


        }
        private void StatisticSizeChanged(object sender, SizeChangedEventArgs e)
        {
            setMappingSizeDelegate?.Invoke(e.NewSize.Width, e.NewSize.Height);

            initCanvasMappingDelegate?.Invoke();
        }
    }


}
