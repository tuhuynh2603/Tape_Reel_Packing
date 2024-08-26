using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Define;
using TapeReelPacking.Source.Model;
using TapeReelPacking.Source.Repository;
using TapeReelPacking.UI.UserControls.ViewModel;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class TeachParameterVM :BaseVM
    {

        bool _isDisableOnpropertyChanged = false;
        public void OnPropertyChanged(PropertyValueChangedEventArgs e)
        {
            // Access changed property and its new value
            if (e == null || _isDisableOnpropertyChanged)
                return;

            var propertyItem = e.OriginalSource as Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem;
            var propertyName = propertyItem?.DisplayName;

            if (!propertyName.ToString().Contains("Defect ROI Index"))
                return;

            //var propertyName = e.GetType();
            var oldValue = e.OldValue;
            var newValue = e.NewValue;
            if (oldValue == newValue)
                return;

            nDefectROIIndex = (int)newValue;
            ReloadTeachParameterUI(SelectedCameraIndex, nDefectROIIndex);
            // Handle the event
            // For example, you might log the change or update some other parts of your UI
            Console.WriteLine($"changed from '{oldValue}' to '{newValue}'");
        }


        public int nDefectROIIndex = 0;

        private int _SelectedCameraIndex;
        public int SelectedCameraIndex
        {
            get => _SelectedCameraIndex;
            set
            {
                if (_SelectedCameraIndex != value)
                {
                    _SelectedCameraIndex = value;
                    ReloadTeachParameterUI(SelectedCameraIndex, nDefectROIIndex);
                    OnPropertyChanged(nameof(SelectedCameraIndex));
                    // Add logic to handle camera selection change if necessary
                }
            }
        }

        private CategoryTeachParameter _categoriesTeachParam;

        public CategoryTeachParameter CategoriesTeachParam
        {
            get => _categoriesTeachParam;
            set => SetProperty(ref _categoriesTeachParam, value);
        }


        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PropertyChangedCommand { set; get; }

        CategoryTeachParameterService categoryTeachParameterService { set; get; }

        public TeachParameterVM()
        {

            //categoryTeachParameterService = service;
            

            SaveCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
                                         (p) =>
                                         {
                                             SaveParameterTeachDefault();
                                             //var data = await categoryTeachParameterService.GetCategoryTeachParameterById(SelectedCameraIndex);
                                             //if (data != null)
                                             //{
                                             //    await categoryTeachParameterService.UpdateCategoryTeachParameter(categoriesTeachParam);
                                             //}
                                             //else
                                             //    await categoryTeachParameterService.CreateCategoryTeachParameter(categoriesTeachParam);
                                         });

            CancelCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
                             (p) =>
                             {
                                 UpdateTeachParamFromDictToUI(Application.dictTeachParam);


                                 MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                                 mainWindow.teach_parameters_btn.IsChecked = false;
                             });

            PropertyChangedCommand = new DelegateCommand<PropertyValueChangedEventArgs>(OnPropertyChanged);

            CategoriesTeachParam = Application.categoriesTeachParam;


        }





        public bool SaveParameterTeachDefault()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                //LoadTeachParamFromUIToDict();
                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                mainWindow.master.m_Tracks[SelectedCameraIndex].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                //mainWindow.master.m_Tracks[SelectedCameraIndex].m_InspectionCore.UpdateAreaParameterFromUIToInspectionCore((int)Application.categoriesTeachParam.DR_DefectROIIndex);
                mainWindow.master.WriteTeachParam(SelectedCameraIndex);
                Application.LoadTeachParamFromFileToDict(SelectedCameraIndex);
                //Application.LoadAreaParamFromFileToDict(SelectedCameraIndex, (int)Application.categoriesTeachParam.DR_DefectROIIndex);


                Mouse.OverrideCursor = null;
                //mainWindow.teach_parameters_btn.IsChecked = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ReloadTeachParameterUI(int nTrack, int nArea = 0)
        {
            _isDisableOnpropertyChanged = true;

            Application.dictTeachParam.Clear();
            Application.LoadTeachParamFromFileToDict(nTrack);
            //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
            UpdateTeachParamFromDictToUI(Application.dictTeachParam);

            //ReloadAreaParameterUI(SelectedCameraIndex, nDefectROIIndex);
            CategoriesTeachParam = Application.categoriesTeachParam;
            _isDisableOnpropertyChanged = false;


        }

        public void ReloadAreaParameterUI(int nTrack, int nArea = 0)
        {
            Application.dictTeachParam.Clear();
            Application.LoadAreaParamFromFileToDict(nTrack, nDefectROIIndex);
            //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
            UpdateTeachParamFromDictToUI(Application.dictPVIAreaParam[nDefectROIIndex]);
            //pgr_PropertyGrid_Teach.Update();
        }


        public bool UpdateTeachParamFromDictToUI(Dictionary<string, string> dictTeachParam)
        {
            if (dictTeachParam == null)
                return false;
            //_dictTeachParam = dictTeachParam;
            object category = Application.categoriesTeachParam;
            //object category_local = categoriesTeachParam;

            bool bSuccess = Application.UpdateParamFromDictToUI(dictTeachParam, ref category);
            Application.categoriesTeachParam = (CategoryTeachParameter)category;

            CategoriesTeachParam = Application.categoriesTeachParam;


            return bSuccess;
        }


        public class AreaComboBox : IItemsSource
        {
            Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection m_ComboBox_Area = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();

            public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
            {
                m_ComboBox_Area.Add("Area 1");
                m_ComboBox_Area.Add("Area 2");
                m_ComboBox_Area.Add("Area 3");
                m_ComboBox_Area.Add("Area 4");
                m_ComboBox_Area.Add("Area 5");

                return m_ComboBox_Area;
            }
        }
        /// <summary>
        /// Model For PropertyGrid
        /// </summary>


    }
}
