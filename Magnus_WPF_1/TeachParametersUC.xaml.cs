using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Define;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        private Dictionary<string, string> _dictTeachParam = new Dictionary<string, string>();
        int m_nTrackSelected = 0;
        public CategoryTeachParameter categoriesTeachParam = new CategoryTeachParameter();

        public TeachParametersUC()
        {
            InitializeComponent();
            this.DataContext = categoriesTeachParam;
        }

        private Rectangles GetRectangles(string str)
        {
            if (str == "")
                return new Rectangles(0, 0, 0, 0);
            string[] value = str.Split(':');
            if (value.Length == 4)
                return new Rectangles(double.Parse(value[0]), double.Parse(value[1]), double.Parse(value[2]), double.Parse(value[3]));
            else
                return new Rectangles(double.Parse(value[0]), double.Parse(value[1]), double.Parse(value[2]), double.Parse(value[3]), double.Parse(value[4]));

        }
        private List<int> ConverStringToList(string str)
        {
            List<int> list = new List<int>();
            string[] value = str.Split(':');
            foreach (string s in value)
            {
                list.Add(int.Parse(s));
            }
            return list;
        }
        public bool UpdateTeachParamFromDictToUI(Dictionary<string, string> dictTeachParam)
        {
            this._dictTeachParam = dictTeachParam;
            try
            {
                PropertyInfo[] arrInfo = Application.Application.categoriesTeachParam.GetType().GetProperties();
                for (int i = 0; i < arrInfo.Count(); i++)
                {
                    PropertyInfo info = arrInfo[i];
                    Type type = info.PropertyType;
                    if (type.Name == "Int32")
                    {
                        int value = 0;
                        bool success = Int32.TryParse(dictTeachParam[info.Name].ToString(), out value);
                        if (success == false)
                            value = (int)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(Application.Application.categoriesTeachParam, value);
                        info.SetValue(categoriesTeachParam, value);
                    }
                    else if (type.Name == "Color")
                    {
                        info.SetValue(Application.Application.categoriesTeachParam, dictTeachParam[info.Name] == "white" ? Colors.White : Colors.Black);
                    }
                    else if (type.Name == "Double")
                    {
                        double value = 0.0;
                        bool success = double.TryParse(dictTeachParam[info.Name].ToString(), out value);
                        if (success == false)
                            value = (double)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(Application.Application.categoriesTeachParam, value);
                        info.SetValue(categoriesTeachParam, value);

                    }
                    else if (type.Name == "List`1")
                    {
                        if (type.FullName.Contains("Int32"))
                        {
                            int[] value = new int[3];
                            List<int> listValue;
                            if (dictTeachParam[info.Name] == "")
                            {
                                value = (int[])info.GetCustomAttribute<DefaultValueAttribute>().Value;
                                listValue = new List<int>(value);
                            }
                            else
                                listValue = ConverStringToList(dictTeachParam[info.Name]);
                            info.SetValue(Application.Application.categoriesTeachParam, listValue);
                            info.SetValue(categoriesTeachParam, listValue);

                        }
                        else if (type.FullName.Contains("Rectangles"))
                        {
                            List<Rectangles> listValue = new List<Rectangles> { };
                            foreach (KeyValuePair<string, string> kvp in dictTeachParam)
                            {
                                if (kvp.Key.Contains(info.Name))
                                    listValue.Add(GetRectangles(dictTeachParam[kvp.Key.ToString()]));
                            }
                            info.SetValue(Application.Application.categoriesTeachParam, listValue);
                            info.SetValue(categoriesTeachParam, listValue);
                        }
                    }

                    else if (type.Name == "Rectangles")
                    {
                        Rectangles rect = GetRectangles(dictTeachParam[info.Name]);
                        info.SetValue(Application.Application.categoriesTeachParam, rect);
                        info.SetValue(categoriesTeachParam, rect);

                    }
                    else if (type.Name == "String")
                    {
                        string str = dictTeachParam[info.Name].ToString();
                        if (str == "")
                            str = (string)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(Application.Application.categoriesTeachParam, str);
                        info.SetValue(categoriesTeachParam, str);

                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            pgr_PropertyGrid_Teach.Update();
            return true;
        }

        private void track_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_nTrackSelected != track_ComboBox.SelectedIndex)
                m_nTrackSelected = track_ComboBox.SelectedIndex;
            {
                ReloadParameterUI(m_nTrackSelected);
            }

        }

        private void ReloadParameterUI(int nTrack)
        {

        }

        public void LoadTeachParamFromUIToDict()
        {
            System.Reflection.PropertyInfo[] infos = categoriesTeachParam.GetType().GetProperties();
            System.Reflection.PropertyInfo[] infosApp = Application.Application.categoriesTeachParam.GetType().GetProperties();


            for (int i = 0; i < infos.Length; i++)
            {

                var attributes = infos[i].GetCustomAttributes(typeof(BrowsableAttribute), true);

                bool isBrowsable = true;
                if (attributes.Length > 0)
                {
                    BrowsableAttribute browseAttr = (BrowsableAttribute)attributes[0];
                    isBrowsable = browseAttr.Browsable;
                    //Console.WriteLine($"Is MyProperty browsable? {isBrowsable}");
                }

                if (isBrowsable == true)
                {
                    infosApp[i].SetValue(Application.Application.categoriesTeachParam, infos[i].GetValue(categoriesTeachParam));
                    Type type = infos[i].PropertyType;
                    _dictTeachParam[infos[i].Name] = infos[i].GetValue(categoriesTeachParam).ToString();
                }
            }
        }

        private void btn_Save_Param_Teach_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveParameterTeachDefault();
        }
        

        public bool SaveParameterTeachDefault()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                LoadTeachParamFromUIToDict();
                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                mainWindow.master.WriteTeachParam();
                InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                Mouse.OverrideCursor = null;
                mainWindow.teach_parameters_btn.IsChecked = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void btn_Cancel_Save_Param_Teach_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateTeachParamFromDictToUI(_dictTeachParam);
            MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            mainWindow.teach_parameters_btn.IsChecked = false;
        }

        /// <summary>
        /// Model For PropertyGrid
        /// </summary>
        // [CategoryOrder("TOP SURFACE", 0)]
        [CategoryOrder("LOCATION", 0)]
        [CategoryOrder("INSPECTION", 1)]
        //[CategoryOrder("TOP PATTERN", 1)]
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

            //[Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Lower Threshold")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(1)]
            public int L_lowerThreshold { get; set; }
            //[Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Upper Threshold")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(2)]
            public int L_upperThreshold { get; set; }

            //[Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Opening Mask")]
            [Range(1, 500)]
            [DefaultValue(11)]
            [Description("")]
            [PropertyOrder(3)]
            public int L_OpeningMask { get; set; }

            //[Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Dilation Mask")]
            [Range(1, 500)]
            [DefaultValue(30)]
            [Description("")]
            [PropertyOrder(4)]
            public int L_DilationMask { get; set; }

            //[Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Min Width Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(5)]
            public int L_MinWidthDevice { get; set; }

            //[Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Min Height Device")]
            [Range(0, 99999)]
            [DefaultValue(50)]
            [Description("")]
            [PropertyOrder(6)]
            public int L_MinHeightDevice { get; set; }

            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Template Roi")]
            [Range(0, 5)]
            [Description("")]
            [PropertyOrder(7)]
            public Rectangles L_TemplateRoi { get; set; }

            //[Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Number Side")]
            [Range(1, 360)]
            [DefaultValue(4)]
            [Description("")]
            [PropertyOrder(8)]
            public int L_NumberSide { get; set; }

            //[Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Angle Resolution")]
            [Range(0.1, 360.0)]
            [DefaultValue(90.0)]
            [Description("")]
            [PropertyOrder(9)]
            public double L_AngleResolution { get; set; }

            //[Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Min Score")]
            [Range(0.0, 99999.0)]
            [DefaultValue(50.0)]
            [Description("")]
            [PropertyOrder(10)]
            public double L_MinScore { get; set; }


            [Browsable(false)]
            [Category("LOCATION")]
            [DisplayName("Corner Index")]
            [Range(0, 3)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(11)]
            public int L_CornerIndex { get; set; }

            #endregion


            #region LABEL

            [Browsable(false)]
            [Category("LABEL")]
            [DisplayName("Device Location Roi")]
            [Range(0, 5)]
            [Description("")]
            [PropertyOrder(0)]
            public Rectangles LB_DeviceLocationRoi { get; set; }

            //[Browsable(false)]
            [Category("LABEL")]
            [DisplayName("Lower Threshold")]
            [Range(0, 255)]
            [DefaultValue(0)]
            [Description("")]
            [PropertyOrder(1)]
            public int LB_lowerThreshold { get; set; }
            //[Browsable(false)]
            [Category("LABEL")]
            [DisplayName("Upper Threshold")]
            [Range(0, 255)]
            [DefaultValue(255)]
            [Description("")]
            [PropertyOrder(2)]
            public int LB_upperThreshold { get; set; }

            //[Browsable(false)]
            [Category("LABEL")]
            [DisplayName("Opening Mask")]
            [Range(1, 500)]
            [DefaultValue(11)]
            [Description("")]
            [PropertyOrder(3)]
            public int LB_OpeningMask { get; set; }

            //[Browsable(false)]
            [Category("LABEL")]
            [DisplayName("Dilation Mask")]
            [Range(1, 500)]
            [DefaultValue(30)]
            [Description("")]
            [PropertyOrder(4)]
            public int LB_DilationMask { get; set; }

            #endregion
        }
    }
}
