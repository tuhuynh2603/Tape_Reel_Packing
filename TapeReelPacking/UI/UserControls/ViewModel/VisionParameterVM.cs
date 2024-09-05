using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using TapeReelPacking.Source.Algorithm;
using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Helper;
using TapeReelPacking.Source.Interface;
using TapeReelPacking.Source.Model;
using TapeReelPacking.UI.UserControls.View;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class VisionParameterVM : BaseVM, IParameter
    {



        private int selectedPVIAreaIndex = 0;
        public int SelectedPVIAreaIndex
        {
            get => selectedPVIAreaIndex;
            set
            {
                SetProperty(ref selectedPVIAreaIndex, value);

                categoriesVisionParam = ReloadAreaParameterUI(_SelectedCameraIndex, selectedPVIAreaIndex);
            }
        }


        private int _SelectedCameraIndex;
        public int SelectedCameraIndex
        {
            get => _SelectedCameraIndex;
            set
            {
                if (_SelectedCameraIndex != value)
                {
                    SetProperty(ref _SelectedCameraIndex, value);
                    categoriesVisionParam = ReloadAreaParameterUI(_SelectedCameraIndex, selectedPVIAreaIndex);
                    // Add logic to handle camera selection change if necessary
                }
            }
        }

        private CategoryVisionParameter _categoriesVisionParam;
        public CategoryVisionParameter categoriesVisionParam
        {
            get => _categoriesVisionParam;
            set
            {
                _categoriesVisionParam = value;
                OnPropertyChanged(nameof(categoriesVisionParam));
            }
        }


        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PropertyChangedCommand { set; get; }

        public VisionParameterVM()
        {



            SaveCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
                                         (p) =>
                                         {
                                             SaveParameterDefault(categoriesVisionParam, SelectedCameraIndex, selectedPVIAreaIndex);
                                         });

            CancelCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
            (p) =>
            {
                //for (int area = 0; area < Application.TOTAL_AREA; area++)
                //{
                categoriesVisionParam = ReloadAreaParameterUI(SelectedCameraIndex, selectedPVIAreaIndex);
                //}
            });

            initCategoryDelegate = InitCategory;

        }

        public delegate void InitCategoryDelegate(int nTrack, int nArea);
        public static InitCategoryDelegate initCategoryDelegate;
        public void InitCategory(int nTrack, int nArea)
        {
            SelectedCameraIndex = nTrack;
            SelectedPVIAreaIndex = nArea;
            //categoriesVisionParam = ReloadAreaParameterUI(nTrack, nArea);

        }



        public static bool SaveParameterDefault(CategoryVisionParameter categoryVisionParameter, int nTrack, int nArea)
        {
            try
            {
                //Mouse.OverrideCursor = Cursors.Wait;
                MainWindow.mainWindow.master.m_Tracks[nTrack].m_InspectionCore.UpdateAreaParameterFromUIToInspectionCore(categoryVisionParameter, nArea);
                WritePVIAreaParam(nTrack, nArea);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static CategoryVisionParameter ReloadAreaParameterUI(int nTrack, int nArea = 0)
        {
            Dictionary<string, string> dictPVIAreaParam = LoadAreaParamFromFileToDict(nTrack, nArea);
            CategoryVisionParameter categoryVisionParameter = new CategoryVisionParameter();
            bool bSuccess = FileHelper.UpdateParamFromDictToUI<CategoryVisionParameter>(dictPVIAreaParam, categoryVisionParameter);

            return categoryVisionParameter;
        }

        /// <summary>
        /// Model For PropertyGrid
        /// </summary>


        public static Dictionary<string, string> LoadAreaParamFromFileToDict(int nTrack, int nAreaIndex)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (Application.currentRecipe == null || Application.pathRecipe == null)
                return dict;

            string strFileName = $"PVIAreaParameters_Track{nTrack + 1}_Area{nAreaIndex + 1}" + ".cfg";
            string pathFile = Path.Combine(Application.pathRecipe, Application.currentRecipe, strFileName);
            IniFile ini = new IniFile(pathFile);

            FileHelper.ReadLine_Magnus(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_AreaEnable)), ini, ref dict);
            FileHelper.ReadLine_Magnus(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_lowerThreshold)), ini, ref dict);
            FileHelper.ReadLine_Magnus(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_upperThreshold)), ini, ref dict);
            FileHelper.ReadLine_Magnus(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_OpeningMask)), ini, ref dict);
            FileHelper.ReadLine_Magnus(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_DilationMask)), ini, ref dict);
            FileHelper.ReadLine_Magnus(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_ObjectCoverPercent)), ini, ref dict);
            FileHelper.ReadLine_Magnus(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_DefectROILocation)), ini, ref dict);

            return dict;
            //}
        }

        public static void WritePVIAreaParam(int nTrack, int nAreaIndex)
        {


            string strFileName = $"PVIAreaParameters_Track{nTrack + 1}_Area{nAreaIndex + 1}" + ".cfg";
            string pathFile = Path.Combine(Application.pathRecipe, Application.currentRecipe, strFileName);

            string strDateTime = string.Format("({0}.{1}.{2}_{3}.{4}.{5})", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
            string backup_path = Path.Combine(Application.pathRecipe, Application.currentRecipe, "Backup_Teach Parameter");
            if (!Directory.Exists(backup_path))
                Directory.CreateDirectory(backup_path);

            string backup_fullpath = Path.Combine(backup_path, $"PVIAreaParameters_Track{nTrack + 1}_Area{nAreaIndex + 1} {strDateTime}" + ".cfg");
            FileInfo file = new FileInfo(pathFile);

            if (!file.Exists)
                file.Create();

            file.MoveTo(backup_fullpath);
            file.Create();

            IniFile ini = new IniFile(pathFile);
            InspectionCore inspectionCore = MainWindow.mainWindow.master.m_Tracks[nTrack].m_InspectionCore;
            FileHelper.WriteLine(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_AreaEnable)), ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_DR_AreaEnable.ToString());
            FileHelper.WriteLine(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_lowerThreshold)), ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_lowerThreshold.ToString());
            FileHelper.WriteLine(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_upperThreshold)), ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_upperThreshold.ToString());
            FileHelper.WriteLine(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_OpeningMask)), ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_OpeningMask.ToString());
            FileHelper.WriteLine(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_DilationMask)), ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_DilationMask.ToString());
            FileHelper.WriteLine(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_ObjectCoverPercent)), ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_ObjectCoverPercent.ToString());
            FileHelper.WriteLine(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, ExceedToolkit.GetDisplayName<CategoryVisionParameter>(nameof(CategoryVisionParameter.LD_DefectROILocation)), ini, TypeConverterHelper.ConvertRectanglesToString(inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_DR_DefectROILocations));

        }


    }
}
