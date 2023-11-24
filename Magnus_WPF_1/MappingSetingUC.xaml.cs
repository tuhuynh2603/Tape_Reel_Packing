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

        public CatergoryMappingParameters categoriesMappingParam = new CatergoryMappingParameters();
        public MappingSetingUC()
        {
            InitializeComponent();
            this.DataContext = categoriesMappingParam;

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
        public bool LoadMappingParamFromDictToUI(Dictionary<string, string> dictTeachParam)
        {
            this._dictMappingParam = dictTeachParam;
            try
            {
                PropertyInfo[] arrInfo = Application.Application.categoriesMappingParam.GetType().GetProperties();
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
                        info.SetValue(Application.Application.categoriesMappingParam, value);
                        info.SetValue(categoriesMappingParam, value);
                    }
                    else if (type.Name == "Color")
                    {
                        info.SetValue(Application.Application.categoriesMappingParam, dictTeachParam[info.Name] == "white" ? Colors.White : Colors.Black);
                    }
                    else if (type.Name == "THRESHOLD_TYPE")
                    {
                        THRESHOLD_TYPE value;
                        bool success = Enum.TryParse(_dictMappingParam.Values.ElementAt(i), out value);
                        if (success == false)
                            value = (THRESHOLD_TYPE)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(Application.Application.categoriesMappingParam, value);
                        info.SetValue(categoriesMappingParam, value);
                    }

                    else if (type.Name == "OBJECT_COLOR")
                    {
                        OBJECT_COLOR value;
                        bool success = Enum.TryParse(_dictMappingParam.Values.ElementAt(i), out value);
                        if (success == false)
                            value = (OBJECT_COLOR)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(Application.Application.categoriesMappingParam, value);
                        info.SetValue(categoriesMappingParam, value);
                    }

                    else if (type.Name == "Double")
                    {
                        double value = 0.0;
                        bool success = double.TryParse(dictTeachParam[info.Name].ToString(), out value);
                        if (success == false)
                            value = (double)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(Application.Application.categoriesMappingParam, value);
                        info.SetValue(categoriesMappingParam, value);

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
                            info.SetValue(Application.Application.categoriesMappingParam, listValue);
                            info.SetValue(categoriesMappingParam, listValue);

                        }
                        else if (type.FullName.Contains("Rectangles"))
                        {
                            List<Rectangles> listValue = new List<Rectangles> { };
                            foreach (KeyValuePair<string, string> kvp in dictTeachParam)
                            {
                                if (kvp.Key.Contains(info.Name))
                                    listValue.Add(GetRectangles(dictTeachParam[kvp.Key.ToString()]));
                            }
                            info.SetValue(Application.Application.categoriesMappingParam, listValue);
                            info.SetValue(categoriesMappingParam, listValue);
                        }
                    }

                    else if (type.Name == "Rectangles")
                    {
                        Rectangles rect = GetRectangles(dictTeachParam[info.Name]);
                        info.SetValue(Application.Application.categoriesMappingParam, rect);
                        info.SetValue(categoriesMappingParam, rect);

                    }
                    else if (type.Name == "String")
                    {
                        string str = dictTeachParam[info.Name].ToString();
                        if (str == "")
                            str = (string)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(Application.Application.categoriesMappingParam, str);
                        info.SetValue(categoriesMappingParam, str);

                    }
                }
            }
            catch
            {
                return false;
            }
            pgr_PropertyGrid_Mapping.Update();
            return true;
        }
        public void UpdateDictionaryParam()
        {
            System.Reflection.PropertyInfo[] infos = categoriesMappingParam.GetType().GetProperties();
            System.Reflection.PropertyInfo[] infosApp = Application.Application.categoriesMappingParam.GetType().GetProperties();

            for (int i = 0; i < infos.Length; i++)
            {
                infosApp[i].SetValue(Application.Application.categoriesMappingParam, infos[i].GetValue(categoriesMappingParam));
                Type type = infos[i].PropertyType;
                _dictMappingParam[infos[i].Name] = infos[i].GetValue(categoriesMappingParam).ToString();
            }
        }

        public bool SaveParameterMappingDefault()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                UpdateDictionaryParam();
                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                mainWindow.master.WriteMappingParam();
                Mouse.OverrideCursor = null;
                mainWindow.mapping_parameters_btn.IsChecked = false;
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
            LoadMappingParamFromDictToUI(_dictMappingParam);
            MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            mainWindow.mapping_parameters_btn.IsChecked = false;
        }

        [CategoryOrder("MAPPING", 0)]
        [DisplayName("Mapping Setting")]
        public class CatergoryMappingParameters
        {

            #region MAPPING
            //[Browsable(false)]
            [Category("MAPPING")]
            [DisplayName("Number Device X")]
            [Range(1, 20)]
            [DefaultValue(5)]
            [Description("")]
            [PropertyOrder(0)]
            public int M_NumberDeviceX { get; set; }
            //[Browsable(false)]
            [Category("MAPPING")]
            [DisplayName("Number Device Y")]
            [Range(1, 20)]
            [DefaultValue(5)]
            [Description("")]
            [PropertyOrder(1)]
            public int M_NumberDeviceY { get; set; }
            #endregion

        }
    }
}
