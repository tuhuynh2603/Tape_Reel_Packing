using TapeReelPacking.Source.Define;
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
using Application = TapeReelPacking.Source.Application;
using System.Runtime.CompilerServices;
using TapeReelPacking.UI.UserControls.ViewModel;

namespace TapeReelPacking
{
    /// <summary>
    /// Interaction logic for MappingSetingUC.xaml
    /// </summary>
    /// 
    public class MappingSetingUCVM : BaseVM
    {

        private CatergoryMappingParameters _dataMapping;
        public CatergoryMappingParameters categoriesMappingParam
        {
            set
            {
                _dataMapping = value;
                OnPropertyChanged(nameof(categoriesMappingParam));
            }
            get => _dataMapping;
        }

        public MappingSetingUCVM()
        {
            categoriesMappingParam = Application.Application.categoriesMappingParam;
        }

        [CategoryOrder("MAPPING", 0)]
        [DisplayName("Mapping Setting")]
        public class CatergoryMappingParameters
        {

            #region MAPPING
            [Browsable(true)]
            [Category("MAPPING")]
            [DisplayName("Number Device X")]
            [Range(10, 100)]
            [DefaultValue(10)]
            [Description("")]
            [PropertyOrder(0)]
            public int M_NumberDeviceX { get; set; }
            [Browsable(true)]
            [Category("MAPPING")]
            [DisplayName("Number Device Y")]
            [Range(1, 100)]
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

    public partial class MappingSetingUC : UserControl
    {

        private Dictionary<string, string> _dictMappingParam = new Dictionary<string, string>();
        public MappingSetingUC()
        {
            InitializeComponent();

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
            object category = Application.Application.categoriesMappingParam;
            bool bSuccess = Application.Application.UpdateParamFromDictToUI(dictParam, ref category/*, ref category_local*/);
            Application.Application.categoriesMappingParam = (MappingSetingUCVM.CatergoryMappingParameters)category;
            pgr_PropertyGrid_Mapping.Update();
            return bSuccess;
        }

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


    }
}
