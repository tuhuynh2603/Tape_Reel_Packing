using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TapeReelPacking.Source.Application;
using static TapeReelPacking.Source.Hardware.SDKHrobot.Global;
using TapeReelPacking.UI.UserControls.View;
using System.IO;
using System.Windows;
using Master = TapeReelPacking.Source.Application.Master;
using Application = TapeReelPacking.Source.Application.Application;
using static TapeReelPacking.UI.UserControls.ViewModel.StepDebugVM;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class InspectionTabVM:BaseVM
    {


        private bool inspect_btnEnable1;

        public bool inspect_btnEnable
        {
            get => inspect_btnEnable1;

            set
            {
                inspect_btnEnable1 = value;
                OnPropertyChanged(nameof(inspect_btnEnable));
            }
        }
        private bool debug_btnEnable1;

        public bool debug_btnEnable
        {
            get => debug_btnEnable1;

            set
            {
                debug_btnEnable1 = value;
                OnPropertyChanged(nameof(debug_btnEnable));
            }
        }
        public static bool bEnableOfflineInspection = false; // need to check more

        //private bool _bEnableOfflineInspection = false;

        public bool bEnableOfflineInspectionUI
        { 
            get => bEnableOfflineInspection; 

            set 
            {
                bEnableOfflineInspection = value;
                OnPropertyChanged(nameof(bEnableOfflineInspectionUI));
            } 
        }

        private bool btn_load_image_FileEnable1;

        public bool btn_load_image_FileEnable
        {
            get => btn_load_image_FileEnable1;

            set
            {
                btn_load_image_FileEnable1 = value;
                OnPropertyChanged(nameof(btn_load_image_FileEnable));
            }
        }
        private bool load_teach_image_btnEnable1;

        public bool load_teach_image_btnEnable
        {
            get => load_teach_image_btnEnable1;

            set
            {
                load_teach_image_btnEnable1 = value;
                OnPropertyChanged(nameof(load_teach_image_btnEnable));
            }
        }
        private bool btn_save_teach_imageEnable1;

        public bool btn_save_teach_imageEnable
        {
            get => btn_save_teach_imageEnable1;

            set
            {
                btn_save_teach_imageEnable1 = value;
                OnPropertyChanged(nameof(btn_save_teach_imageEnable));
            }
        }
        private bool teach_parameters_btnEnable1;

        public bool teach_parameters_btnEnable
        {
            get => teach_parameters_btnEnable1;

            set
            {
                teach_parameters_btnEnable1 = value;
                OnPropertyChanged(nameof(teach_parameters_btnEnable));
            }
        }
        private bool pviArea_parameters_btnEnable1;

        public bool pviArea_parameters_btnEnable
        {
            get => pviArea_parameters_btnEnable1;

            set
            {
                pviArea_parameters_btnEnable1 = value;
                OnPropertyChanged(nameof(pviArea_parameters_btnEnable));
            }
        }
        private bool btn_teachEnable1;

        public bool btn_teachEnable
        {
            get => btn_teachEnable1;

            set
            {
                btn_teachEnable1 = value;
                OnPropertyChanged(nameof(btn_teachEnable));
            }
        }
        private bool btn_next_teachEnable1 = false;

        public bool btn_next_teachEnable
        {
            get => btn_next_teachEnable1;

            set
            {
                btn_next_teachEnable1 = value;
                OnPropertyChanged(nameof(btn_next_teachEnable));
            }
        }
        private bool btn_abort_teachEnable1 = false;

        public bool btn_abort_teachEnable
        {
            get => btn_abort_teachEnable1;

            set
            {
                btn_abort_teachEnable1 = value;
                OnPropertyChanged(nameof(btn_abort_teachEnable));
            }
        }



        private bool btn_save_current_imageEnable1;

        public bool btn_save_current_imageEnable
        {
            get => btn_save_current_imageEnable1;

            set
            {
                btn_save_current_imageEnable1 = value;
                OnPropertyChanged(nameof(btn_save_current_imageEnable));
            }
        }
        private bool btn_enable_saveimageEnable1;

        public bool btn_enable_saveimageEnable
        {
            get => btn_enable_saveimageEnable1;

            set
            {
                btn_enable_saveimageEnable1 = value;
                OnPropertyChanged(nameof(btn_enable_saveimageEnable));
            }
        }

        private ActionCommand btn_inspect_ClickCmd1;

        public ICommand btn_inspect_ClickCmd
        {
            get
            {
                if (btn_inspect_ClickCmd1 == null)
                {
                    btn_inspect_ClickCmd1 = new ActionCommand(Performbtn_inspect_ClickCmd);
                }

                return btn_inspect_ClickCmd1;
            }
        }

        private void Performbtn_inspect_ClickCmd()
        {
            if (!inspect_btnEnable || MainWindowVM.m_bSequenceRunning)
                return;

            Master.InspectEvent[MainWindowVM.activeImageDock.trackID].Set();

        }

        private ActionCommand btn_load_image_File_ClickCmd1;

        public ICommand btn_load_image_File_ClickCmd
        {
            get
            {
                if (btn_load_image_File_ClickCmd1 == null)
                {
                    btn_load_image_File_ClickCmd1 = new ActionCommand(Performbtn_load_image_File_ClickCmd);
                }

                return btn_load_image_File_ClickCmd1;
            }
        }

        string m_strSelectionFolderFilePath = "";
        private void Performbtn_load_image_File_ClickCmd()
        {
            if (MainWindowVM.m_bSequenceRunning)
                return;

            if (m_strSelectionFolderFilePath == @"C:\" || m_strSelectionFolderFilePath == "")
                m_strSelectionFolderFilePath = Path.Combine(Application.pathImageSave, "UI Image");
            // Set the initial directory for the dialog box
            System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog();

            FileDialog.FileName = m_strSelectionFolderFilePath;

            // Display the dialog box and wait for the user's response
            System.Windows.Forms.DialogResult result = FileDialog.ShowDialog();

            // If the user clicked the OK button, open the selected folder
            if ((int)result == 1)
            {
                // Get the path of the selected folder
                m_strSelectionFolderFilePath = FileDialog.FileName;

                MainWindowVM.master.loadImageFromFileToUI(MainWindowVM.activeImageDock.trackID, FileDialog.FileName);
                // Open the folder using a DirectoryInfo or other appropriate method
                // ...
            }
            else
                return;

        }

        private ActionCommand btn_save_current_image_ClickCmd1;

        public ICommand btn_save_current_image_ClickCmd
        {
            get
            {
                if (btn_save_current_image_ClickCmd1 == null)
                {
                    btn_save_current_image_ClickCmd1 = new ActionCommand(Performbtn_save_current_image_ClickCmd);
                }

                return btn_save_current_image_ClickCmd1;
            }
        }

        private void Performbtn_save_current_image_ClickCmd()
        {
            if (MainWindowVM.m_bSequenceRunning)
                return;
            //var result = MessageBox.Show("Do you want to save as teach image ?", "Save as Teach Image", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if (result == MessageBoxResult.Yes)
            //{
            MainWindowVM.master.SaveUIImage(MainWindowVM.activeImageDock.trackID);
            //master.m_Tracks[0].m_imageViews[0].SaveTeachImage(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_1.bmp"));
            //}
        }

        private ActionCommand btn_load_teach_image_ClickCmd;

        public ICommand Btn_load_teach_image_ClickCmd
        {
            get
            {
                if (btn_load_teach_image_ClickCmd == null)
                {
                    btn_load_teach_image_ClickCmd = new ActionCommand(PerformBtn_load_teach_image_ClickCmd);
                }

                return btn_load_teach_image_ClickCmd;
            }
        }

        private void PerformBtn_load_teach_image_ClickCmd()
        {
            if (MainWindowVM.m_bSequenceRunning)
                return;

            MainWindowVM.master.loadTeachImageToUI(MainWindowVM.activeImageDock.trackID);
        }

        private ActionCommand btn_save_teach_image_ClickCmd1;

        public ICommand btn_save_teach_image_ClickCmd
        {
            get
            {
                if (btn_save_teach_image_ClickCmd1 == null)
                {
                    btn_save_teach_image_ClickCmd1 = new ActionCommand(Performbtn_save_teach_image_ClickCmd);
                }

                return btn_save_teach_image_ClickCmd1;
            }
        }

        private void Performbtn_save_teach_image_ClickCmd()
        {
            var result = MessageBox.Show("Do you want to save as teach image ?", "Save as Teach Image", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MainWindowVM.master.SaveUITeachImage(MainWindowVM.activeImageDock.trackID);
                //master.m_Tracks[0].m_imageViews[0].SaveTeachImage(System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_1.bmp"));
            }
        }

        private ActionCommand btn_teach_clickCmd1;

        public ICommand btn_teach_clickCmd
        {
            get
            {
                if (btn_teach_clickCmd1 == null)
                {
                    btn_teach_clickCmd1 = new ActionCommand(Performbtn_teach_clickCmd);
                }

                return btn_teach_clickCmd1;
            }
        }

        private void Performbtn_teach_clickCmd()
        {
            if (Master.m_bIsTeaching)
                return;

            SetEnableTeachButton();
            //nCurrentTeachingStep = 0;
            //master.m_Tracks[0].m_imageViews[0].Teach(nCurrentTeachingStep);
            MainWindowVM.master.TeachThread();
        }

        public void SetEnableTeachButton()
        {
            btn_next_teachEnable = true;
            btn_abort_teachEnable = true;
            inspect_btnEnable = false;
            load_teach_image_btnEnable= false;
            MainWindowVM.bEnableGrabCycle = false ;
            teach_parameters_btnEnable = false;
            btn_save_teach_imageEnable = false;


        }

        public delegate void SetDisableTeachButtonDelegate();
        public delegate void SetEnableTeachButtonDelegate();
        public static SetDisableTeachButtonDelegate setDisableTeachButtonDelegate;
        public static SetEnableTeachButtonDelegate setEnableTeachButtonDelegate;


        private MainWindowVM _mainWindowVM { set; get; }
        public InspectionTabVM(MainWindowVM mainVM)
        {
            _mainWindowVM = mainVM;
            setDisableTeachButtonDelegate = SetDisableTeachButton;
            setEnableTeachButtonDelegate = SetEnableTeachButton;
            loginEnableButtonDelegate = enableAftetLogin;
        }


        public void SetDisableTeachButton()
        {
            btn_next_teachEnable = false;
            btn_abort_teachEnable = false;
            inspect_btnEnable = true;
            load_teach_image_btnEnable = true;
            MainWindowVM.bEnableGrabCycle = true;
            teach_parameters_btnEnable = true;
            btn_save_teach_imageEnable = true;
        }


        private ActionCommand btn_abort_teach_ClickCmd1;

        public ICommand btn_abort_teach_ClickCmd
        {
            get
            {
                if (btn_abort_teach_ClickCmd1 == null)
                {
                    btn_abort_teach_ClickCmd1 = new ActionCommand(Performbtn_abort_teach_ClickCmd);
                }

                return btn_abort_teach_ClickCmd1;
            }
        }

        private void Performbtn_abort_teach_ClickCmd()
        {
            Master.m_bIsTeaching = false;
            SetDisableTeachButton();
            for (int nTrack = 0; nTrack < Application.m_nTrack; nTrack++)
            {
                MainWindowVM.master.loadTeachImageToUI(nTrack);
                MainWindowVM.master.m_Tracks[nTrack].m_imageViews[0].resultTeach.Children.Clear();
                MainWindowVM.master.m_Tracks[nTrack].m_imageViews[0].ClearOverlay();
                MainWindowVM.master.m_Tracks[nTrack].m_imageViews[0].controlWin.Visibility = Visibility.Collapsed;
            }
        }

        private ActionCommand btn_debug_UncheckedCmd1;

        public ICommand btn_debug_UncheckedCmd
        {
            get
            {
                if (btn_debug_UncheckedCmd1 == null)
                {
                    btn_debug_UncheckedCmd1 = new ActionCommand(Performbtn_debug_UncheckedCmd);
                }

                return btn_debug_UncheckedCmd1;
            }
        }

        private void Performbtn_debug_UncheckedCmd()
        {

            _mainWindowVM.mStepDebugVM.isVisible = Visibility.Collapsed;
            //MainWindow.mainWindow.grd_Defect_Settings.Visibility = Visibility.Collapsed;
        }

        private ActionCommand btn_debug_CheckedCmd1;

        public ICommand btn_debug_CheckedCmd
        {
            get
            {
                if (btn_debug_CheckedCmd1 == null)
                {
                    btn_debug_CheckedCmd1 = new ActionCommand(Performbtn_debug_CheckedCmd);
                }

                return btn_debug_CheckedCmd1;
            }
        }

        private void Performbtn_debug_CheckedCmd()
        {
            _mainWindowVM.mStepDebugVM.isVisible = Visibility.Visible;
            doStepDebugDelegate?.Invoke(MainWindowVM.activeImageDock.trackID);
        }


        //private string _color_portReceive;
        public void UpdateDebugInfor()
        {
            //if (!m_bEnableDebug)
            //{
            //    grd_Defect_Settings.Visibility = Visibility.Collapsed;
            //    return;
            //}
            ////defectInfor.lvDefect.View = gridView;
            //MainWindowVM.defectInfor.lvDefect.ItemsSource = null;
            //MainWindowVM.defectInfor.lvDefect.ItemsSource = MainWindowVM.master.m_Tracks[MainWindowVM.activeImageDock.trackID].m_StepDebugInfors;
            //DialogDefectHeight = MainWindowVM.defectInfor.Height;
            //DialogDefectWidth = MainWindowVM.defectInfor.Width;

            //grd_Defect.Children.Clear();
            //grd_Defect.Children.Add(defectInfor);
            //grd_Defect_Settings.Visibility = Visibility.Visible;

        }


        private ActionCommand btn_inspect_offline_UnCheckedCmd1;

        public ICommand btn_inspect_offline_UnCheckedCmd
        {
            get
            {
                if (btn_inspect_offline_UnCheckedCmd1 == null)
                {
                    btn_inspect_offline_UnCheckedCmd1 = new ActionCommand(Performbtn_inspect_offline_UnCheckedCmd);
                }

                return btn_inspect_offline_UnCheckedCmd1;
            }
        }

        private void Performbtn_inspect_offline_UnCheckedCmd()
        {
            bEnableOfflineInspection = false;
        }

        private ActionCommand btn_inspect_offline_CheckedCmd1;

        public ICommand btn_inspect_offline_CheckedCmd
        {
            get
            {
                if (btn_inspect_offline_CheckedCmd1 == null)
                {
                    btn_inspect_offline_CheckedCmd1 = new ActionCommand(Performbtn_inspect_offline_CheckedCmd);
                }

                return btn_inspect_offline_CheckedCmd1;
            }
        }

        private void Performbtn_inspect_offline_CheckedCmd()
        {
            MainWindowVM.master.RunOfflineSequenceThread(MainWindowVM.activeImageDock.trackID);
        }

        private bool? btn_enable_saveimageChecked1;

        public bool? btn_enable_saveimageChecked
        {
            get => btn_enable_saveimageChecked1;

            set
            {
                btn_enable_saveimageChecked1 = value;
                OnPropertyChanged(nameof(btn_enable_saveimageChecked));
            }
        }
        private ActionCommand btn_enable_saveimage_UncheckedCmd1;

        public ICommand btn_enable_saveimage_UncheckedCmd
        {
            get
            {
                if (btn_enable_saveimage_UncheckedCmd1 == null)
                {
                    btn_enable_saveimage_UncheckedCmd1 = new ActionCommand(Performbtn_enable_saveimage_UncheckedCmd);
                }

                return btn_enable_saveimage_UncheckedCmd1;
            }
        }

        private void Performbtn_enable_saveimage_UncheckedCmd()
        {
            Source.Application.Application.m_bEnableSavingOnlineImage = false;

        }

        private ActionCommand btn_enable_saveimage_CheckedCmd1;

        public ICommand btn_enable_saveimage_CheckedCmd
        {
            get
            {
                if (btn_enable_saveimage_CheckedCmd1 == null)
                {
                    btn_enable_saveimage_CheckedCmd1 = new ActionCommand(Performbtn_enable_saveimage_CheckedCmd);
                }

                return btn_enable_saveimage_CheckedCmd1;
            }
        }

        private void Performbtn_enable_saveimage_CheckedCmd()
        {
            Source.Application.Application.m_bEnableSavingOnlineImage = true;

        }

        private bool? isTeachParametersChecked1;

        public bool? isTeachParametersChecked
        {
            get => isTeachParametersChecked1;

            set
            {
                isTeachParametersChecked1 = value;
                OnPropertyChanged(nameof(isTeachParametersChecked));
            }
        }
        private ActionCommand btn_teach_parameters_UncheckedCmd1;

        public ICommand btn_teach_parameters_UncheckedCmd
        {
            get
            {
                if (btn_teach_parameters_UncheckedCmd1 == null)
                {
                    btn_teach_parameters_UncheckedCmd1 = new ActionCommand(Performbtn_teach_parameters_UncheckedCmd);
                }

                return btn_teach_parameters_UncheckedCmd1;
            }
        }

        private void Performbtn_teach_parameters_UncheckedCmd()
        {
            _mainWindowVM.mTeachParameterVM.isVisible = Visibility.Collapsed;
        }

        private ActionCommand btn_teach_parameters_CheckedCmd1;

        public ICommand btn_teach_parameters_CheckedCmd
        {
            get
            {
                if (btn_teach_parameters_CheckedCmd1 == null)
                {
                    btn_teach_parameters_CheckedCmd1 = new ActionCommand(Performbtn_teach_parameters_CheckedCmd);
                }

                return btn_teach_parameters_CheckedCmd1;
            }
        }

        private void Performbtn_teach_parameters_CheckedCmd()
        {
            _mainWindowVM.mTeachParameterVM.isVisible = Visibility.Visible;
        }

        private ActionCommand pviArea_parameters_btn_UncheckedCmd1;

        public ICommand pviArea_parameters_btn_UncheckedCmd
        {
            get
            {
                if (pviArea_parameters_btn_UncheckedCmd1 == null)
                {
                    pviArea_parameters_btn_UncheckedCmd1 = new ActionCommand(PerformpviArea_parameters_btn_UncheckedCmd);
                }

                return pviArea_parameters_btn_UncheckedCmd1;
            }
        }

        private void PerformpviArea_parameters_btn_UncheckedCmd()
        {
            _mainWindowVM.mVisionParameterVM.isVisible = Visibility.Collapsed;

        }

        private ActionCommand pviArea_parameters_btn_CheckedCmd1;

        public ICommand pviArea_parameters_btn_CheckedCmd
        {
            get
            {
                if (pviArea_parameters_btn_CheckedCmd1 == null)
                {
                    pviArea_parameters_btn_CheckedCmd1 = new ActionCommand(PerformpviArea_parameters_btn_CheckedCmd);
                }

                return pviArea_parameters_btn_CheckedCmd1;
            }
        }

        private void PerformpviArea_parameters_btn_CheckedCmd()
        {
            _mainWindowVM.mVisionParameterVM.isVisible = Visibility.Visible;
        }

        private ActionCommand btn_next_teach_clickCmd1;

        public ICommand btn_next_teach_clickCmd
        {
            get
            {
                if (btn_next_teach_clickCmd1 == null)
                {
                    btn_next_teach_clickCmd1 = new ActionCommand(Performbtn_next_teach_clickCmd);
                }

                return btn_next_teach_clickCmd1;
            }
        }

        private void Performbtn_next_teach_clickCmd()
        {
            if (Master.m_bIsTeaching)
                Master.m_NextStepTeachEvent.Set();
        }


        public delegate void LoginEnableButtonDelegate(bool enable);
        public static LoginEnableButtonDelegate loginEnableButtonDelegate;
        public void enableAftetLogin(bool benable)
        {
            inspect_btnEnable = benable;
            debug_btnEnable = benable;
            bEnableOfflineInspectionUI = benable;
            btn_load_image_FileEnable = benable;
            load_teach_image_btnEnable = benable;
            btn_save_teach_imageEnable = benable;
            btn_save_current_imageEnable = benable;
            btn_enable_saveimageEnable = benable;
            teach_parameters_btnEnable = benable;
            pviArea_parameters_btnEnable = benable;
            btn_teachEnable = benable;
        }


    }
}
