using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TapeReelPacking.Source.Application;
using TapeReelPacking.UI.UserControls.View;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using WindowState = System.Windows.WindowState;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    class TitleBarVM : BaseVM
    {

        #region ICommand
        public ICommand CloseWindowCommand { get; set; }
        public ICommand MinimizeWindowCommand { get; set; }
        public ICommand MaximizeWindowCommand { get; set; }
        #endregion

       // MainWindowVM _mainWindowVM { get; set; }
        public TitleBarVM(/*MainWindowVM mainWindowVM*/)
        {
            //_mainWindowVM = mainWindowVM;
            InitCommand();
        }
        public static void CloseWindow(Window w)
        {
            MainWindowVM.master = null;
            w.Close();
            System.Windows.Application.Current.Shutdown();
        }

        public static FrameworkElement GetWindowParent(UserControl p)
        {
            FrameworkElement parent = p;
            if (parent == null)
                return parent;
            while (parent.Parent != null)
            {
                parent = parent.Parent as FrameworkElement;
            }
            return parent;
        }


        private void InitCommand()
        {
            CloseWindowCommand = new RelayCommand<UserControl>((p) => { return true; },
                                                                    (p) =>
                                                                    {


                                                                        if (MessageBox.Show("Close App?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                                                        {
                                                                            if (MainWindowVM.m_bSequenceRunning)
                                                                            {
                                                                                MessageBox.Show("Machine is running, cannot close the application", "");
                                                                                return;
                                                                            }

                                                                            MainWindowVM.master.m_Tracks[0].m_hIKControlCameraView.m_MyCamera.MV_CC_ClearImageBuffer_NET();
                                                                            MainWindowVM.master.m_Tracks[0].m_hIKControlCameraView.m_MyCamera.MV_CC_CloseDevice_NET();
                                                                            MainWindowVM.master.m_Tracks[0].m_hIKControlCameraView.m_MyCamera.MV_CC_DestroyDevice_NET();
                                                                            MainWindowVM.master.m_BarcodeReader.CloseConnection();
                                                                            MainWindowVM.master.m_hiWinRobotInterface.CloseConnection();
                                                                            
                                                                            //MainWindow.m_IsWindowOpen = false;
                                                                            Master.ReleaseEventAndThread();
                                                                            Thread.Sleep(500);
                                                                            //MainWindowVM.master = null;
                                                                            MainWindow.mainWindow.Close();
                                                                            //MainWindow.mainWindow = null;
                                                                            System.Windows.Application.Current.Shutdown();
                                                                            MainWindowVM.master.applications.KillCurrentProcess();
                                                                        }
                                                                    });

            MinimizeWindowCommand = new RelayCommand<UserControl>((p) => { return true; },
                                               (p) =>
                                               {
                                                   MainWindow.mainWindow.WindowState = WindowState.Minimized;
                                               });
            MaximizeWindowCommand = new RelayCommand<UserControl>((p) => { return true; },
                                                               (p) =>   
                                                               {
                                                                   if (MainWindow.mainWindow.WindowState == WindowState.Maximized)
                                                                   {
                                                                       MainWindow.mainWindow.WindowState = WindowState.Normal;
                                                                   }
                                                                   else
                                                                       MainWindow.mainWindow.WindowState = WindowState.Maximized;
                                                               });
        }
    }
}
