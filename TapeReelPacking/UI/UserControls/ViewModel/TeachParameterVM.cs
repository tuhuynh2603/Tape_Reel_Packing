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

        //private CategoryTeachParameter _selectedObject;

        //public CategoryTeachParameter SelectedObject
        //{
        //    get => _selectedObject;
        //    set => SetProperty(ref _selectedObject, value);
        //}


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

        public CategoryTeachParameter categoriesTeachParam
        {
            get => _categoriesTeachParam;
            set => SetProperty(ref _categoriesTeachParam, value);
        }


        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PropertyChangedCommand { set; get; }

        public TeachParameterVM()
        {



            SaveCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
                                         (p) =>
                                         {
                                             SaveParameterTeachDefault();
                                         });

            CancelCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
                             (p) =>
                             {
                                 UpdateTeachParamFromDictToUI(Application.dictTeachParam);


                                 MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                                 mainWindow.teach_parameters_btn.IsChecked = false;
                             });

            PropertyChangedCommand = new DelegateCommand<PropertyValueChangedEventArgs>(OnPropertyChanged);

            categoriesTeachParam = Application.categoriesTeachParam;


        }





        public bool SaveParameterTeachDefault()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                //LoadTeachParamFromUIToDict();
                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                mainWindow.master.m_Tracks[SelectedCameraIndex].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                mainWindow.master.m_Tracks[SelectedCameraIndex].m_InspectionCore.UpdateAreaParameterFromUIToInspectionCore((int)Application.categoriesTeachParam.DR_DefectROIIndex);
                mainWindow.master.WriteTeachParam(SelectedCameraIndex);
                Application.LoadTeachParamFromFileToDict(SelectedCameraIndex);
                Application.LoadAreaParamFromFileToDict(SelectedCameraIndex, (int)Application.categoriesTeachParam.DR_DefectROIIndex);


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

            ReloadAreaParameterUI(SelectedCameraIndex, nDefectROIIndex);
            categoriesTeachParam = Application.categoriesTeachParam;
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

            categoriesTeachParam = Application.categoriesTeachParam;


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
        [CategoryOrder("LOCATION", 0)]
        [CategoryOrder("OPPOSITE CHIP", 1)]
        [CategoryOrder("DEFECT ROI", 2)]
        [CategoryOrder("LABEL DEFECT", 3)]
        [DisplayName("Teach Parameter")]
        public class CategoryTeachParameter : Prism.Mvvm.BindableBase
        {

            #region LOCATION
            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Device Location Roi")]
            [Range(0, 5)]
            [Description("")]
            [PropertyOrder(0)]
            public Rectangles L_DeviceLocationRoi { 
                get => _L_DeviceLocationRoi;

                set => SetProperty(ref _L_DeviceLocationRoi, value);
            }
            private Rectangles _L_DeviceLocationRoi;



            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Location Enable")]
            [PropertyOrder(1)]
            [DefaultValue(true)]
            public bool L_LocationEnable {
                get => _L_LocationEnable;

                set => SetProperty(ref _L_LocationEnable, value);

            }

            private bool _L_LocationEnable;


            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Threshold Method")]
            [DefaultValue(THRESHOLD_TYPE.BINARY_THRESHOLD)]
            [Description("Threshold method")]
            [PropertyOrder(2)]
            public THRESHOLD_TYPE L_ThresholdType {
            get => _L_ThresholdType;

                set => SetProperty(ref _L_ThresholdType, value);

            }

            private THRESHOLD_TYPE _L_ThresholdType;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Object Color")]
            [DefaultValue(OBJECT_COLOR.BLACK)]
            [Description("The color of Object want to catch")]
            [PropertyOrder(3)]
            public OBJECT_COLOR L_ObjectColor
            {
                get => _L_ObjectColor;

                set => SetProperty(ref _L_ObjectColor, value);

            }

            private OBJECT_COLOR _L_ObjectColor;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Lower Threshold")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(4)]

            public int L_lowerThreshold
            {
                get => _L_lowerThreshold;

                set => SetProperty(ref _L_lowerThreshold, value);

            }

            private int _L_lowerThreshold;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Upper Threshold")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(5)]
            public int L_upperThreshold
            {
                get => _L_upperThreshold;

                set => SetProperty(ref _L_upperThreshold, value);

            }
            private int _L_upperThreshold;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Lower Threshold Inner Chip")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(6)]
            public int L_lowerThresholdInnerChip
            {
                get => _L_lowerThresholdInnerChip;

                set => SetProperty(ref _L_lowerThresholdInnerChip, value);

            }
            private int _L_lowerThresholdInnerChip;


            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Upper Threshold Inner Chip")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(7)]
            public int L_upperThresholdInnerChip
            {
                get => _L_upperThresholdInnerChip;

                set => SetProperty(ref _L_upperThresholdInnerChip, value);

            }
            private int _L_upperThresholdInnerChip;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Opening Mask")]
            [Range(1, 500)]
            [DefaultValue(11)]
            [Description("")]
            [PropertyOrder(8)]
            public int L_OpeningMask
            {
                get => _L_OpeningMask;

                set => SetProperty(ref _L_OpeningMask, value);

            }
            private int _L_OpeningMask;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Dilation Mask")]
            [Range(1, 500)]
            [DefaultValue(30)]
            [Description("")]
            [PropertyOrder(9)]
            public int L_DilationMask
            {
                get => _L_DilationMask;

                set => SetProperty(ref _L_DilationMask, value);

            }
            private int _L_DilationMask;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Min Width Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(10)]
            public int L_MinWidthDevice
            {
                get => _L_MinWidthDevice;

                set => SetProperty(ref _L_MinWidthDevice, value);

            }
            private int _L_MinWidthDevice;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Min Height Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(11)]
            public int L_MinHeightDevice
            {
                get => _L_MinHeightDevice;

                set => SetProperty(ref _L_MinHeightDevice, value);

            }
            private int _L_MinHeightDevice;

            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Template Roi")]
            [Range(0, 5)]
            [Description("")]
            [PropertyOrder(12)]
            public Rectangles L_TemplateRoi
            {
                get => _L_TemplateRoi;

                set => SetProperty(ref _L_TemplateRoi, value);

            }
            private Rectangles _L_TemplateRoi;
            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Number Side")]
            [Range(1, 360)]
            [DefaultValue(4)]
            [Description("")]
            [PropertyOrder(13)]
            public int L_NumberSide
            {
                get => _L_NumberSide;

                set => SetProperty(ref _L_NumberSide, value);

            }
            private int _L_NumberSide;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Scale Image Ratio")]
            [Range(0.1, 1)]
            [DefaultValue(1)]
            [Description("Before Inspecting, The image will be scaled by this value to reduce inspection time.")]
            [PropertyOrder(14)]
            public double L_ScaleImageRatio
            {
                get => _L_ScaleImageRatio;

                set => SetProperty(ref _L_ScaleImageRatio, value);

            }
            private double _L_ScaleImageRatio;

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Min Score")]
            [Range(0.0, 99999.0)]
            [DefaultValue(50.0)]
            [Description("")]
            [PropertyOrder(15)]
            public double L_MinScore
            {
                get => _L_MinScore;

                set => SetProperty(ref _L_MinScore, value);

            }
            private double _L_MinScore;

            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Corner Index")]
            [Range(0, 3)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(16)]
            public int L_CornerIndex
            {
                get => _L_CornerIndex;

                set => SetProperty(ref _L_CornerIndex, value);

            }
            private int _L_CornerIndex;
            #endregion

            #region Area

            [Browsable(false)]
            [Category("DEFECT ROI")]
            [DisplayName("Defect ROI Locations")]
            [Range(0, 5)]
            [Description("")]
            [PropertyOrder(0)]
            public List<Rectangles> DR_DefectROILocations
            {
                get => _DR_DefectROILocations;

                set => SetProperty(ref _DR_DefectROILocations, value);

            }
            private List<Rectangles> _DR_DefectROILocations;

            [Browsable(true)]
            [Category("DEFECT ROI")]
            [DisplayName("Number ROI Location")]
            [Range(0, Application.TOTAL_AREA)]
            [DefaultValue(1)]
            [Description("")]
            [PropertyOrder(1)]
            public int DR_NumberROILocation
            {
                get => _DR_NumberROILocation;

                set => SetProperty(ref _DR_NumberROILocation, value);

            }
            private int _DR_NumberROILocation;
            [Browsable(true)]
            [Category("DEFECT ROI")]
            [DisplayName("Defect ROI Index")]
            [DefaultValue(AREA_INDEX.A1)]
            [PropertyOrder(2)]
            //[ItemsSource(typeof(AreaComboBox))]
            public AREA_INDEX DR_DefectROIIndex
            {
                get => _DR_DefectROIIndex;

                set => SetProperty(ref _DR_DefectROIIndex, value);

            }
            private AREA_INDEX _DR_DefectROIIndex;
            #endregion

            #region OPPOSITE Chip


            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Enable")]
            [PropertyOrder(0)]
            [DefaultValue(false)]
            public bool OC_EnableCheck
            {
                get => _OC_EnableCheck;

                set => SetProperty(ref _OC_EnableCheck, value);

            }
            private bool _OC_EnableCheck;
            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("lower Threshold")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [PropertyOrder(1)]
            //[ItemsSource(typeof(AreaComboBox))]
            public int OC_lowerThreshold
            {
                get => _OC_lowerThreshold;

                set => SetProperty(ref _OC_lowerThreshold, value);

            }
            private int _OC_lowerThreshold;
            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("upper Threshold")]
            [Range(0, 255)]
            [DefaultValue(100)]
            [PropertyOrder(2)]
            //[ItemsSource(typeof(AreaComboBox))]
            public int OC_upperThreshold
            {
                get => _OC_upperThreshold;

                set => SetProperty(ref _OC_upperThreshold, value);

            }
            private int _OC_upperThreshold;
            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Opening Mask")]
            [Range(0, 100)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(3)]
            public int OC_OpeningMask
            {
                get => _OC_OpeningMask;

                set => SetProperty(ref _OC_OpeningMask, value);

            }
            private int _OC_OpeningMask;
            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Dilation Mask")]
            [Range(0, 100)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(4)]
            public int OC_DilationMask
            {
                get => _OC_DilationMask;

                set => SetProperty(ref _OC_DilationMask, value);

            }
            private int _OC_DilationMask;

            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Min Width Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(5)]
            public int OC_MinWidthDevice
            {
                get => _OC_MinWidthDevice;

                set => SetProperty(ref _OC_MinWidthDevice, value);

            }
            private int _OC_MinWidthDevice;

            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Min Height Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(6)]
            public int OC_MinHeightDevice
            {
                get => _OC_MinHeightDevice;

                set => SetProperty(ref _OC_MinHeightDevice, value);

            }
            private int _OC_MinHeightDevice;
            #endregion

            #region LABEL

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Area Enable")]
            [PropertyOrder(0)]
            [DefaultValue(false)]
            public bool LD_AreaEnable
            {
                get => _LD_AreaEnable;

                set => SetProperty(ref _LD_AreaEnable, value);

            }
            private bool _LD_AreaEnable;

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Lower Threshold")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(2)]
            public int LD_lowerThreshold
            {
                get => _LD_lowerThreshold;

                set => SetProperty(ref _LD_lowerThreshold, value);

            }
            private int _LD_lowerThreshold;

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Upper Threshold")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(3)]
            public int LD_upperThreshold
            {
                get => _LD_upperThreshold;

                set => SetProperty(ref _LD_upperThreshold, value);

            }
            private int _LD_upperThreshold;

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Opening Mask")]
            [Range(0, 100)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(4)]
            public int LD_OpeningMask
            {
                get => _LD_OpeningMask;

                set => SetProperty(ref _LD_OpeningMask, value);

            }
            private int _LD_OpeningMask;

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Dilation Mask")]
            [Range(0, 100)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(5)]
            public int LD_DilationMask
            {
                get => _LD_DilationMask;

                set => SetProperty(ref _LD_DilationMask, value);

            }
            private int _LD_DilationMask;

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Object Cover PerCent")]
            [Range(0, 100)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(6)]
            public int LD_ObjectCoverPercent
            {
                get => _LD_ObjectCoverPercent;

                set => SetProperty(ref _LD_ObjectCoverPercent, value);

            }
            private int _LD_ObjectCoverPercent;
            #endregion

        }

    }
}
