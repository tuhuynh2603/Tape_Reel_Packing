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
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using static TapeReelPacking.TeachParametersUC;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class TeachParameterVM :BaseVM
    {
        public Action UpdatePropertyGridAction { get; set; }

        private int _m_nTrackSelected;
        public int m_nTrackSelected
        {
            set
            {
                if (_m_nTrackSelected != value)
                    _m_nTrackSelected = value;
                {
                    ReloadTeachParameterUI(m_nTrackSelected, nDefectROIIndex);
                }
                OnPropertyChanged(nameof(m_nTrackSelected));
            }
            get => _m_nTrackSelected;       
        }




        public int nDefectROIIndex = 0;

        private object _selectedObject;
        public object SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject != value)
                {
                    _selectedObject = value;

                    if (nDefectROIIndex != (int)Application.categoriesTeachParam.DR_DefectROIIndex)
                    {
                        nDefectROIIndex = (int)Application.categoriesTeachParam.DR_DefectROIIndex;
                        ReloadAreaParameterUI(m_nTrackSelected, nDefectROIIndex);
                    }
                    OnPropertyChanged(nameof(SelectedObject));
                }
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
                    _SelectedCameraIndex = value;
                    OnPropertyChanged(nameof(SelectedCameraIndex));
                    // Add logic to handle camera selection change if necessary
                }
            }
        }

        private CategoryTeachParameter _categoriesTeachParam;
        public CategoryTeachParameter categoriesTeachParam
        
        {
            set
            {
                _categoriesTeachParam = value;
                OnPropertyChanged(nameof(categoriesTeachParam));
            }
               
            get => _categoriesTeachParam;
        }



        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public TeachParameterVM()
        {
            categoriesTeachParam = Application.categoriesTeachParam;
            SaveCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
                                         (p) =>
                                         {
                                             SaveParameterTeachDefault();
                                         });

            CancelCommand = new RelayCommand<TeachParameterVM>((p) => { return true; },
                             (p) =>
                             {
                                 UpdateTeachParamFromDictToUI(Application.dictTeachParam);
                                 UpdatePropertyGridAction?.Invoke();


                                 MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                                 mainWindow.teach_parameters_btn.IsChecked = false;
                             });

        }


        public bool SaveParameterTeachDefault()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                //LoadTeachParamFromUIToDict();
                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                mainWindow.master.m_Tracks[m_nTrackSelected].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                mainWindow.master.m_Tracks[m_nTrackSelected].m_InspectionCore.UpdateAreaParameterFromUIToInspectionCore((int)Application.categoriesTeachParam.DR_DefectROIIndex);
                mainWindow.master.WriteTeachParam(m_nTrackSelected);
                Application.LoadTeachParamFromFileToDict(m_nTrackSelected);
                Application.LoadAreaParamFromFileToDict(m_nTrackSelected, (int)Application.categoriesTeachParam.DR_DefectROIIndex);
                UpdatePropertyGridAction?.Invoke();


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
            Application.dictTeachParam.Clear();
            Application.LoadTeachParamFromFileToDict(nTrack);
            //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
            UpdateTeachParamFromDictToUI(Application.dictTeachParam);

            ReloadAreaParameterUI(m_nTrackSelected, nDefectROIIndex);
            UpdatePropertyGridAction?.Invoke();


        }

        public void ReloadAreaParameterUI(int nTrack, int nArea = 0)
        {
            Application.dictTeachParam.Clear();
            Application.LoadAreaParamFromFileToDict(nTrack, nDefectROIIndex);
            //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
            UpdateTeachParamFromDictToUI(Application.dictPVIAreaParam[nDefectROIIndex]);
            //pgr_PropertyGrid_Teach.Update();
            UpdatePropertyGridAction?.Invoke();
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

            //categoriesTeachParam = (CategoryTeachParameter)category_local;
            UpdatePropertyGridAction?.Invoke();


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
        public class CategoryTeachParameter
        {

            #region LOCATION
            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Device Location Roi")]
            [Range(0, 5)]
            [Description("")]
            [PropertyOrder(0)]
            public Rectangles L_DeviceLocationRoi { get; set; }


            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Location Enable")]
            [PropertyOrder(1)]
            [DefaultValue(true)]
            public bool L_LocationEnable { get; set; }


            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Threshold Method")]
            [DefaultValue(THRESHOLD_TYPE.BINARY_THRESHOLD)]
            [Description("Threshold method")]
            [PropertyOrder(2)]
            public THRESHOLD_TYPE L_ThresholdType { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Object Color")]
            [DefaultValue(OBJECT_COLOR.BLACK)]
            [Description("The color of Object want to catch")]
            [PropertyOrder(3)]
            public OBJECT_COLOR L_ObjectColor { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Lower Threshold")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(4)]

            public int L_lowerThreshold { get; set; }
            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Upper Threshold")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(5)]
            public int L_upperThreshold { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Lower Threshold Inner Chip")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(6)]
            public int L_lowerThresholdInnerChip { get; set; }


            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Upper Threshold Inner Chip")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(7)]
            public int L_upperThresholdInnerChip { get; set; }


            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Opening Mask")]
            [Range(1, 500)]
            [DefaultValue(11)]
            [Description("")]
            [PropertyOrder(8)]
            public int L_OpeningMask { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Dilation Mask")]
            [Range(1, 500)]
            [DefaultValue(30)]
            [Description("")]
            [PropertyOrder(9)]
            public int L_DilationMask { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Min Width Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(10)]
            public int L_MinWidthDevice { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Min Height Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(11)]
            public int L_MinHeightDevice { get; set; }

            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Template Roi")]
            [Range(0, 5)]
            [Description("")]
            [PropertyOrder(12)]
            public Rectangles L_TemplateRoi { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Number Side")]
            [Range(1, 360)]
            [DefaultValue(4)]
            [Description("")]
            [PropertyOrder(13)]
            public int L_NumberSide { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Scale Image Ratio")]
            [Range(0.1, 1)]
            [DefaultValue(1)]
            [Description("Before Inspecting, The image will be scaled by this value to reduce inspection time.")]
            [PropertyOrder(14)]
            public double L_ScaleImageRatio { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Min Score")]
            [Range(0.0, 99999.0)]
            [DefaultValue(50.0)]
            [Description("")]
            [PropertyOrder(15)]
            public double L_MinScore { get; set; }


            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Corner Index")]
            [Range(0, 3)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(16)]
            public int L_CornerIndex { get; set; }

            #endregion

            #region Area

            [Browsable(false)]
            [Category("DEFECT ROI")]
            [DisplayName("Defect ROI Locations")]
            [Range(0, 5)]
            [Description("")]
            [PropertyOrder(0)]
            public List<Rectangles> DR_DefectROILocations { get; set; }

            [Browsable(true)]
            [Category("DEFECT ROI")]
            [DisplayName("Number ROI Location")]
            [Range(0, Application.TOTAL_AREA)]
            [DefaultValue(1)]
            [Description("")]
            [PropertyOrder(1)]
            public int DR_NumberROILocation { get; set; }

            [Browsable(true)]
            [Category("DEFECT ROI")]
            [DisplayName("Defect ROI Index")]
            [DefaultValue(AREA_INDEX.A1)]
            [PropertyOrder(2)]
            //[ItemsSource(typeof(AreaComboBox))]
            public AREA_INDEX DR_DefectROIIndex { get; set; }

            #endregion

            #region OPPOSITE Chip


            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Enable")]
            [PropertyOrder(0)]
            [DefaultValue(false)]
            public bool OC_EnableCheck { get; set; }

            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("lower Threshold")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [PropertyOrder(1)]
            //[ItemsSource(typeof(AreaComboBox))]
            public int OC_lowerThreshold { get; set; }

            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("upper Threshold")]
            [Range(0, 255)]
            [DefaultValue(100)]
            [PropertyOrder(2)]
            //[ItemsSource(typeof(AreaComboBox))]
            public int OC_upperThreshold { get; set; }

            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Opening Mask")]
            [Range(0, 100)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(3)]
            public int OC_OpeningMask { get; set; }

            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Dilation Mask")]
            [Range(0, 100)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(4)]
            public int OC_DilationMask { get; set; }


            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Min Width Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(5)]
            public int OC_MinWidthDevice { get; set; }

            [Browsable(true)]
            [Category("OPPOSITE CHIP")]
            [DisplayName("Min Height Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(6)]
            public int OC_MinHeightDevice { get; set; }

            #endregion

            #region LABEL

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Area Enable")]
            [PropertyOrder(0)]
            [DefaultValue(false)]
            public bool LD_AreaEnable { get; set; }

            //[Browsable(false)]
            //[Category("LABEL DEFECT")]
            //[DisplayName("Number ROI Location")]
            //[Range(0, 5)]
            //[DefaultValue(1)]
            //[Description("")]
            //[PropertyOrder(1)]
            //public int LD_NumberROILocation { get; set; }

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Lower Threshold")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(2)]
            public int LD_lowerThreshold { get; set; }


            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Upper Threshold")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(3)]
            public int LD_upperThreshold { get; set; }

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Opening Mask")]
            [Range(0, 100)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(4)]
            public int LD_OpeningMask { get; set; }

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Dilation Mask")]
            [Range(0, 100)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(5)]
            public int LD_DilationMask { get; set; }

            [Browsable(true)]
            [Category("LABEL DEFECT")]
            [DisplayName("Object Cover PerCent")]
            [Range(0, 100)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(6)]
            public int LD_ObjectCoverPercent { get; set; }

            #endregion

            //[DisplayName("Defect Parameter")]
            //[CategoryOrder("AREA", 1)]
            //[CategoryOrder("LABEL", 1)]
            //public class CategoryAreaParameter
            //{
            //    #region Area
            //    [Category("AREA")]
            //    [Browsable(true)]
            //    [DisplayName("Area Index")]
            //    [DefaultValue(AREA_INDEX.A1)]
            //    //[ItemsSource(typeof(AreaComboBox))]
            //    [PropertyOrder(0)]
            //    public AREA_INDEX A_AreaIndex { get; set; }

            //    #endregion

            //    #region LABEL

            //    [Browsable(false)]
            //    [Category("LABEL DEFECT")]
            //    [DisplayName("Label Locations")]
            //    [Range(0, 5)]
            //    [Description("")]
            //    [PropertyOrder(0)]
            //    public Rectangles LD_LabelLocations { get; set; }


            //    [Category("LABEL DEFECT")]
            //    [DisplayName("Area Enable")]
            //    [PropertyOrder(0)]
            //    [DefaultValue(false)]
            //    public bool LD_AreaEnable { get; set; }

            //    //[Browsable(false)]
            //    //[Category("LABEL DEFECT")]
            //    //[DisplayName("Number ROI Location")]
            //    //[Range(0, 5)]
            //    //[DefaultValue(1)]
            //    //[Description("")]
            //    //[PropertyOrder(1)]
            //    //public int LD_NumberROILocation { get; set; }

            //    //[Browsable(false)]
            //    [Category("LABEL DEFECT")]
            //    [DisplayName("Lower Threshold")]
            //    [Range(0, 255)]
            //    [DefaultValue(0)]
            //    [Description("")]
            //    [PropertyOrder(2)]
            //    public int LD_lowerThreshold { get; set; }
            //    //[Browsable(false)]
            //    [Category("LABEL DEFECT")]
            //    [DisplayName("Upper Threshold")]
            //    [Range(0, 255)]
            //    [DefaultValue(255)]
            //    [Description("")]
            //    [PropertyOrder(3)]
            //    public int LD_upperThreshold { get; set; }

            //    //[Browsable(false)]
            //    [Category("LABEL DEFECT")]
            //    [DisplayName("Opening Mask")]
            //    [Range(1, 100)]
            //    [DefaultValue(1)]
            //    [Description("")]
            //    [PropertyOrder(4)]
            //    public int LD_OpeningMask { get; set; }

            //    //[Browsable(false)]
            //    [Category("LABEL DEFECT")]
            //    [DisplayName("Dilation Mask")]
            //    [Range(1, 100)]
            //    [DefaultValue(1)]
            //    [Description("")]
            //    [PropertyOrder(5)]
            //    public int LD_DilationMask { get; set; }

            //    #endregion
            //}

        }

    }
}
