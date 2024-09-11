using Emgu.CV;
using Emgu.CV.Structure;
using TapeReelPacking.Source.Application;
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
using TapeReelPacking.UI.UserControls.ViewModel;

namespace TapeReelPacking.UI.UserControls.View
{
    /// <summary>
    /// Interaction logic for DefectInfor.xaml
    /// </summary>
    public partial class StepDebugView : UserControl
    {

        public StepDebugView()
        {
            InitializeComponent();
        }
        private void lvDefect_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //ScrollViewer scv = (ScrollViewer)sender;
            scv_StepDebugScrollView.ScrollToVerticalOffset(scv_StepDebugScrollView.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void scv_StepDebugScrollView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            scv_StepDebugScrollView.ScrollToVerticalOffset(scv_StepDebugScrollView.VerticalOffset - e.Delta);
            e.Handled = true;
        }


    }
}
