using Magnus_WPF_1.Source.Define;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Application = Magnus_WPF_1.Source.Application;

namespace Magnus_WPF_1
{
    /// <summary>
    /// Interaction logic for MappingSetingUC.xaml
    /// </summary>
    public partial class MappingSetingUC : UserControl
    {
        private Dictionary<string, string> _dictMappingParam = new Dictionary<string, string>();

        //public CatergoryMappingParameters categoriesMappingParam = new CatergoryMappingParameters();
        public MappingSetingUC()
        {
            InitializeComponent();
            this.DataContext = Application.Application.categoriesMappingParam;

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
        public bool UpdateMappingParamFromDictToUI(Dictionary<string, string> dictParam)
        {
            //_dictMappingParam = dictParam;
            object category = Application.Application.categoriesMappingParam;
            //object category_local = categoriesMappingParam;
            bool bSuccess = Application.Application.UpdateParamFromDictToUI(dictParam, ref category/*, ref category_local*/);
            Application.Application.categoriesMappingParam = (CatergoryMappingParameters)category;
            //categoriesMappingParam = (CatergoryMappingParameters)category_local;
            pgr_PropertyGrid_Mapping.Update();
            return bSuccess;
        }

        //public void UpdateDictionaryParam()
        //{
        //    System.Reflection.PropertyInfo[] infos = Application.Application.categoriesMappingParam.GetType().GetProperties();
        //    System.Reflection.PropertyInfo[] infosApp = Application.Application.categoriesMappingParam.GetType().GetProperties();

        //    for (int i = 0; i < infos.Length; i++)
        //    {
        //        //infosApp[i].SetValue(Application.Application.categoriesMappingParam, infos[i].GetValue(Application.Application.categoriesMappingParam));
        //        Type type = infos[i].PropertyType;
        //        _dictMappingParam[infos[i].Name] = infos[i].GetValue(Application.Application.categoriesMappingParam).ToString();
        //    }
        //}

        public bool SaveParameterMappingDefault()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                //UpdateDictionaryParam();
                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                mainWindow.master.WriteMappingParam();
                Application.Application.LoadMappingParamFromFile();
                Mouse.OverrideCursor = null;
                mainWindow.btn_mapping_parameters.IsChecked = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            SaveParameterMappingDefault();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            UpdateMappingParamFromDictToUI(Application.Application.dictMappingParam);
            MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            mainWindow.btn_mapping_parameters.IsChecked = false;
        }

        [CategoryOrder("MAPPING", 0)]
        [DisplayName("Mapping Setting")]
        public class CatergoryMappingParameters
        {

            #region MAPPING
            [Browsable(true)]
            [Category("MAPPING")]
            [DisplayName("Number Device X")]
            [Range(5, 100)]
            [DefaultValue(10)]
            [Description("")]
            [PropertyOrder(0)]
            public int M_NumberDeviceX { get; set; }
            [Browsable(true)]
            [Category("MAPPING")]
            [DisplayName("Number Device Y")]
            [Range(5, 100)]
            [DefaultValue(10)]
            [Description("")]
            [PropertyOrder(1)]
            public int M_NumberDeviceY { get; set; }

            [Browsable(true)]
            [Category("MAPPING")]
            [DisplayName("Number Device Per Lot")]
            [Range(1, 10000)]
            [DefaultValue(1000)]
            [Description("")]
            [PropertyOrder(2)]
            public int M_NumberDevicePerLot { get; set; }
            #endregion

        }
    }
}
