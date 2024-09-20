using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using TapeReelPacking.UI.UserControls.ViewModel;
using static TapeReelPacking.UI.UserControls.ViewModel.MappingCanvasVM;

namespace TapeReelPacking.UI.UserControls.View
{
    /// <summary>
    /// Interaction logic for MappingCanvasView.xaml
    /// </summary>
    public partial class MappingCanvasView : UserControl
    {
        public MappingCanvasView()
        {
            InitializeComponent();
        }

        private async void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            setMappingSizeDelegate?.Invoke(e.NewSize.Width, e.NewSize.Height);


            //MappingCanvasVM vm = (MappingCanvasVM)this.DataContext;

            //// Update the mapping size
            //vm.SetMappingSize(e.NewSize.Width, e.NewSize.Height);

            //// Await the async operation properly
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Restart();
            //Console.WriteLine("start");
            //await vm.InitCanvasMappingAsync();
            //Console.WriteLine($"end {stopWatch.ElapsedMilliseconds}");

        }
    }
}
