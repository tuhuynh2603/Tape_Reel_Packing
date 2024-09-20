using TapeReelPacking.Source.Algorithm;
using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Define;
//using System.Windows.Forms;
using TapeReelPacking.Source.LogMessage;
using TapeReelPacking.UI.UserControls;
using TapeReelPacking.UI.UserControls.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Wpf.AvalonDock.Layout;
using Application = TapeReelPacking.Source.Application.Application;
using TapeReelPacking.UI.UserControls.ViewModel;
using static TapeReelPacking.UI.UserControls.ViewModel.InspectionTabVM;
using static TapeReelPacking.UI.UserControls.ViewModel.MappingCanvasVM;

namespace TapeReelPacking.UI.UserControls.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static MainWindow mainWindow;
        //public delegate void StateWindow(WindowState state);
        //public static StateWindow changeStateWindow;

        //List<KeyBinding> hotkey = new List<KeyBinding>();


        //public static string BaseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)  // Only allow left-click drag                     
            {
                this.DragMove();  // Moves the window
            }
        }
    }
}
