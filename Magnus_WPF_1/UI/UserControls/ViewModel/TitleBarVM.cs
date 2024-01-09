﻿using Magnus_WPF_1.Source.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Magnus_WPF_1.UI.UserControls.ViewModel
{
    class TitleBarVM: BaseVM
    {

        #region ICommand
        public ICommand CloseWindowCommand { get; set; }
        public ICommand MinimizeWindowCommand { get; set; }
        public ICommand MaximizeWindowCommand { get; set; }
        #endregion

        public TitleBarVM()
        {
            InitCommand();
        }
        public static void CloseWindow(Window w)
        {
            MainWindow.mainWindow.master = null;
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
                                                                        if (MessageBox.Show("Close App?","", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                                                        {
                                                                            //FrameworkElement window = GetWindowParent(p);
                                                                            //var w = window as Window;
                                                                            //if (w == null)
                                                                            //    return;
                                                                            //CloseWindow(MainWindow.mainWindow);
                                                                            MainWindow.mainWindow.master = null;
                                                                            MainWindow.mainWindow.Close();
                                                                            MainWindow.mainWindow = null;
                                                                            System.Windows.Application.Current.Shutdown();
                                                                        }
                                                                    });

            MinimizeWindowCommand = new RelayCommand<UserControl>((p) => { return true; },
                                               (p) =>
                                               {
                                                   //FrameworkElement window = GetWindowParent(p);
                                                   //var w = window as Window;
                                                   //if (w == null)
                                                   //    return;
                                                   MainWindow.mainWindow.WindowState = WindowState.Minimized;
                                                   //w.WindowState = WindowState.Minimized;
                                               });
            MaximizeWindowCommand = new RelayCommand<UserControl>((p) => { return true; },
                                                               (p) =>
                                                               {
                                                                   //FrameworkElement window = GetWindowParent(p);
                                                                   //var w = window as Window;
                                                                   //if (w == null)
                                                                   //    return;
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
