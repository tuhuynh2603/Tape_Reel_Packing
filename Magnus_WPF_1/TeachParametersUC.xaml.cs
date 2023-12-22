using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Define;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Application = Magnus_WPF_1.Source.Application;

namespace Magnus_WPF_1
{
    /// <summary>
    /// Interaction logic for TeachParametersUC.xaml
    /// </summary>
    public partial class TeachParametersUC : UserControl
    {
        //private Dictionary<string, string> _dictTeachParam = new Dictionary<string, string>();
        int m_nTrackSelected = 0;
        //public CategoryTeachParameter categoriesTeachParam = new CategoryTeachParameter();
        public int nDefectROIIndex = 0;

        public TeachParametersUC()
        {
            InitializeComponent();
            this.DataContext = Application.Application.categoriesTeachParam;
        }


        public bool UpdateTeachParamFromDictToUI(Dictionary<string, string> dictTeachParam)
        {
            if (dictTeachParam == null)
                return false;
            //_dictTeachParam = dictTeachParam;
            object category = Application.Application.categoriesTeachParam;
            //object category_local = categoriesTeachParam;

            bool bSuccess = Application.Application.UpdateParamFromDictToUI(dictTeachParam, ref category);
            Application.Application.categoriesTeachParam = (CategoryTeachParameter)category;
            //categoriesTeachParam = (CategoryTeachParameter)category_local;
            //pgr_PropertyGrid_Teach.Update();

            return bSuccess;
        }

        private void track_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_nTrackSelected != track_ComboBox.SelectedIndex)
                m_nTrackSelected = track_ComboBox.SelectedIndex;
            {
                ReloadTeachParameterUI(m_nTrackSelected, nDefectROIIndex);
            }

        }

        public void ReloadTeachParameterUI(int nTrack, int nArea = 0)
        {
            Application.Application.dictTeachParam.Clear();
            Application.Application.LoadTeachParamFromFileToDict(ref nTrack);
            //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
            UpdateTeachParamFromDictToUI(Application.Application.dictTeachParam);

            ReloadAreaParameterUI(m_nTrackSelected, nDefectROIIndex);
            pgr_PropertyGrid_Teach.Update();

        }



        public void ReloadAreaParameterUI(int nTrack, int nArea = 0)
        {
            Application.Application.dictTeachParam.Clear();
            Application.Application.LoadAreaParamFromFileToDict(ref nTrack, nDefectROIIndex);
            //m_Tracks[nTrack].m_InspectionCore.LoadTeachImageToInspectionCore(nTrack);
            UpdateTeachParamFromDictToUI(Application.Application.dictPVIAreaParam[nDefectROIIndex]);
            pgr_PropertyGrid_Teach.Update();

        }

        private void pgr_PropertyGrid_Teach_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            if (nDefectROIIndex != (int)Application.Application.categoriesTeachParam.DR_DefectROIIndex)
            {
                nDefectROIIndex = (int)Application.Application.categoriesTeachParam.DR_DefectROIIndex;
                ReloadAreaParameterUI(m_nTrackSelected, nDefectROIIndex);
            }

        }



        //public void LoadTeachParamFromUIToDict()
        //{
        //    System.Reflection.PropertyInfo[] infos = Application.Application.categoriesTeachParam.GetType().GetProperties();
        //    //System.Reflection.PropertyInfo[] infosApp = Application.Application.categoriesTeachParam.GetType().GetProperties();


        //    for (int i = 0; i < infos.Length; i++)
        //    {
        //        var attributes = infos[i].GetCustomAttributes(typeof(BrowsableAttribute), true);
        //        bool isBrowsable = true;
        //        if (attributes.Length > 0)
        //        {
        //            BrowsableAttribute browseAttr = (BrowsableAttribute)attributes[0];
        //            isBrowsable = browseAttr.Browsable;
        //            //Console.WriteLine($"Is MyProperty browsable? {isBrowsable}");
        //        }

        //        if (isBrowsable == true)
        //        {
        //            //infosApp[i].SetValue(Application.Application.categoriesTeachParam, infos[i].GetValue(categoriesTeachParam));
        //            Type type = infos[i].PropertyType;
        //            _dictTeachParam[infos[i].Name] = infos[i].GetValue(Application.Application.categoriesTeachParam).ToString();
        //        }
        //    }
        //}

        private void btn_Save_Param_Teach_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveParameterTeachDefault();
        }
        

        public bool SaveParameterTeachDefault()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                //LoadTeachParamFromUIToDict();
                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                mainWindow.master.m_Tracks[m_nTrackSelected].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                mainWindow.master.m_Tracks[m_nTrackSelected].m_InspectionCore.UpdateAreaParameterFromUIToInspectionCore((int)Application.Application.categoriesTeachParam.DR_DefectROIIndex);       
                mainWindow.master.WriteTeachParam(m_nTrackSelected);
                Application.Application.LoadTeachParamFromFileToDict(ref m_nTrackSelected);
                Application.Application.LoadAreaParamFromFileToDict(ref m_nTrackSelected, (int)Application.Application.categoriesTeachParam.DR_DefectROIIndex);
                pgr_PropertyGrid_Teach.Update();

                Mouse.OverrideCursor = null;
                //mainWindow.teach_parameters_btn.IsChecked = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void btn_Cancel_Save_Param_Teach_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateTeachParamFromDictToUI(Application.Application.dictTeachParam);
            pgr_PropertyGrid_Teach.Update();

            MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            mainWindow.teach_parameters_btn.IsChecked = false;
        }
        //public class AreaComboBox : IItemsSource
        //{
        //    Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection m_ComboBox_Area = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();

        //    public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
        //    {
        //        m_ComboBox_Area.Add("Area 1");
        //        m_ComboBox_Area.Add("Area 2");
        //        m_ComboBox_Area.Add("Area 3");
        //        m_ComboBox_Area.Add("Area 4");
        //        m_ComboBox_Area.Add("Area 5");

        //        return m_ComboBox_Area;
        //    }
        //}
        /// <summary>
        /// Model For PropertyGrid
        /// </summary>
        [CategoryOrder("LOCATION", 0)]
        [DisplayName("Teach Parameter")]
        public class CategoryTeachParameter
        {

            const int index = 1;
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
            [DisplayName("Threshold Method")]
            [DefaultValue(THRESHOLD_TYPE.BINARY_THRESHOLD)]
            [Description("Threshold method")]
            [PropertyOrder(1)]
            public THRESHOLD_TYPE L_ThresholdType { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Object Color")]
            [DefaultValue(OBJECT_COLOR.BLACK)]
            [Description("The color of Object want to catch")]
            [PropertyOrder(2)]
            public OBJECT_COLOR L_ObjectColor { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Lower Threshold")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(3)]

            public int L_lowerThreshold { get; set; }
            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Upper Threshold")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(4)]
            public int L_upperThreshold { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Lower Threshold Inner Chip")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(5)]
            public int L_lowerThresholdInnerChip { get; set; }


            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Upper Threshold Inner Chip")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(6)]
            public int L_upperThresholdInnerChip { get; set; }


            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Opening Mask")]
            [Range(1, 500)]
            [DefaultValue(11)]
            [Description("")]
            [PropertyOrder(7)]
            public int L_OpeningMask { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Dilation Mask")]
            [Range(1, 500)]
            [DefaultValue(30)]
            [Description("")]
            [PropertyOrder(8)]
            public int L_DilationMask { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Min Width Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(9)]
            public int L_MinWidthDevice { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Min Height Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(10)]
            public int L_MinHeightDevice { get; set; }

            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Template Roi")]
            [Range(0, 5)]
            [Description("")]
            [PropertyOrder(11)]
            public Rectangles L_TemplateRoi { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Number Side")]
            [Range(1, 360)]
            [DefaultValue(4)]
            [Description("")]
            [PropertyOrder(12)]
            public int L_NumberSide { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Scale Image Ratio")]
            [Range(0.1, 1)]
            [DefaultValue(1)]
            [Description("Before Inspecting, The image will be scaled by this value to reduce inspection time.")]
            [PropertyOrder(13)]
            public double L_ScaleImageRatio { get; set; }

            [Browsable(true)]
            [Category("LOCATION")]
            [DisplayName("Min Score")]
            [Range(0.0, 99999.0)]
            [DefaultValue(50.0)]
            [Description("")]
            [PropertyOrder(14)]
            public double L_MinScore { get; set; }


            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Corner Index")]
            [Range(0, 3)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(15)]
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
            [Range(0, Application.Application.TOTAL_AREA)]
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



        private void pgr_PropertyGrid_Area_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {

        }
    }
}
