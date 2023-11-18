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

namespace Magnus_WPF_1.Source.Hardware
{
    /// <summary>
    /// Interaction logic for BarCodeReaderView.xaml
    /// </summary>
    public partial class BarCodeReaderView : UserControl
    {
        public BarCodeReaderView()
        {
            InitializeComponent();
        }

        private void UserControl_ToolTipClosing(object sender, ToolTipEventArgs e)
        {

        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
