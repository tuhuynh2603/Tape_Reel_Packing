using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Model;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class VisionParameterVM:BaseVM
    {

        bool _isDisableOnpropertyChanged = false;
        public void OnPropertyChanged(PropertyValueChangedEventArgs e)
        {
        }

        private int selectedPVIAreaIndex = 0;
        public int SelectedPVIAreaIndex 
        {
            get => selectedPVIAreaIndex;
            set
            {
                SetProperty(ref selectedPVIAreaIndex, value);

                ReloadCameraParameterUI(_SelectedCameraIndex, selectedPVIAreaIndex);
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
                    ReloadCameraParameterUI(_SelectedCameraIndex, selectedPVIAreaIndex);
                    // Add logic to handle camera selection change if necessary
                }
            }
        }

        private CategoryVisionParameter _categoriesVisionParam;

        public CategoryVisionParameter categoriesVisionParam
        {
            get => _categoriesVisionParam;
            set => SetProperty(ref _categoriesVisionParam, value);
        }


        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PropertyChangedCommand { set; get; }

        public VisionParameterVM()
        {



            SaveCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
                                         (p) =>
                                         {
                                             SaveParameterDefault();
                                         });

            CancelCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
                             (p) =>
                             {
                                 UpdateAreaParamFromDictToUI(Application.dictPVIAreaParam[selectedPVIAreaIndex], selectedPVIAreaIndex);


                                 MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                                 mainWindow.teach_parameters_btn.IsChecked = false;
                             });

            PropertyChangedCommand = new DelegateCommand<PropertyValueChangedEventArgs>(OnPropertyChanged);

            categoriesVisionParam = Application.categoriesVisionParam;


        }





        public bool SaveParameterDefault()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                //LoadTeachParamFromUIToDict();
                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                //mainWindow.master.m_Tracks[SelectedCameraIndex].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                mainWindow.master.m_Tracks[SelectedCameraIndex].m_InspectionCore.UpdateAreaParameterFromUIToInspectionCore(categoriesVisionParam, selectedPVIAreaIndex);
                Application.WritePVIAreaParam(SelectedCameraIndex, selectedPVIAreaIndex);
                Application.LoadTeachParamFromFileToDict(SelectedCameraIndex);
                Application.LoadAreaParamFromFileToDict(SelectedCameraIndex, selectedPVIAreaIndex);


                Mouse.OverrideCursor = null;
                //mainWindow.teach_parameters_btn.IsChecked = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ReloadCameraParameterUI(int nTrack, int nArea = 0)
        {
            _isDisableOnpropertyChanged = true;

            Application.dictPVIAreaParam[nArea].Clear();
            Application.LoadAreaParamFromFileToDict(nTrack, nArea);
            //Application.LoadTeachParamFromFileToDict(nTrack);
            //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
            UpdateAreaParamFromDictToUI(Application.dictPVIAreaParam[nArea], nArea);
            ReloadAreaParameterUI(nTrack, nArea);
            categoriesVisionParam = Application.categoriesVisionParam;
            _isDisableOnpropertyChanged = false;


        }

        public void ReloadAreaParameterUI(int nTrack, int nArea = 0)
        {
            Application.dictTeachParam.Clear();
            Application.LoadAreaParamFromFileToDict(nTrack, nArea);
            //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
            UpdateAreaParamFromDictToUI(Application.dictPVIAreaParam[nArea], nArea);
            //pgr_PropertyGrid_Teach.Update();
        }


        public bool UpdateAreaParamFromDictToUI(Dictionary<string, string> dictParam, int nArea)
        {
            if (dictParam == null)
                return false;
            //_dictTeachParam = dictTeachParam;
            object category = Application.categoriesVisionParam;
            //object category_local = categoriesTeachParam;

            bool bSuccess = Application.UpdateParamFromDictToUI(dictParam, ref category);
            //Application.categoriesTeachParam = (CategoryTeachParameter)category;

            categoriesVisionParam = (CategoryVisionParameter)category;


            return bSuccess;
        }


        /// <summary>
        /// Model For PropertyGrid
        /// </summary>


    }
}
