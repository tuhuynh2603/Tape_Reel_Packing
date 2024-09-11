using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Define;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using static TapeReelPacking.Source.Application.Master;
using TapeReelPacking.UI.UserControls.View;

namespace TapeReelPacking.UI.UserControls.View
{
    /// <summary>
    /// Interaction logic for WarningMessageBox.xaml
    /// </summary>
    public partial class WarningMessageBox : UserControl
    {


        public WarningMessageBox()
        {
            InitializeComponent();
        }

        private Point _startWarningPositionDlg;
        private System.Windows.Vector _startOffsetWarningPositionDlg;

        private void grd_Warning_Setting_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startWarningPositionDlg = e.GetPosition(this);
            if (_startWarningPositionDlg.X != 0 && _startWarningPositionDlg.Y != 0)
            {
                _startOffsetWarningPositionDlg = new Vector(tt_WarningSettings.X, tt_WarningSettings.Y);
                grd_Warning_Setting.CaptureMouse();
            }
        }

        private void grd_Warning_Setting_MouseMove(object sender, MouseEventArgs e)
        {
            if (grd_Warning_Setting.IsMouseCaptured)
            {
                Vector offset = Point.Subtract(e.GetPosition(this), _startWarningPositionDlg);
                tt_WarningSettings.X = _startOffsetWarningPositionDlg.X + offset.X;
                tt_WarningSettings.Y = _startOffsetWarningPositionDlg.Y + offset.Y;
            }
        }

        private void grd_Warning_Setting_MouseUp(object sender, MouseButtonEventArgs e)
        {
            grd_Warning_Setting.ReleaseMouseCapture();
        }


    }
}
