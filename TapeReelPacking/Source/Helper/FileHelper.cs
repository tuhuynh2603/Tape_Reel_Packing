using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TapeReelPacking.Source.Application;
using TapeReelPacking.Source.Define;
using TapeReelPacking.Source.Model;

namespace TapeReelPacking.Source.Helper
{
    public class FileHelper
    {
        public static string GetCommInfo(string key, string defaults, string pathReg)
        {
            RegistryKey registerPreferences = Registry.CurrentUser.CreateSubKey(pathReg + "\\Hardware", true);
            if ((string)registerPreferences.GetValue(key) == null)
            {
                registerPreferences.SetValue(key, defaults);
                return defaults;
            }
            else
                return (string)registerPreferences.GetValue(key);
        }

        public static void ReadLine_Magnus(string strGroup, string strName, IniFile ini, Dictionary<string, string> dictionary)
        {
            string strParameterName = (strGroup + strName).Replace(" ", "").ToLower();
            string data = ini.ReadValue(strGroup, strName, "");
            if (data == "")
            {
                data = "";
                ini.WriteValue(strGroup, strName, data);
            }
            else
            {
                if (data.Split(',').Length > 1)
                    data = data.Split(',')[1];
            }
            if (dictionary.ContainsKey(strParameterName))
                dictionary[strParameterName] = data;
            else
                dictionary.Add(strParameterName, data);
        }

        public static void WriteLine(string section, string key, IniFile ini, string param)
        {
            ini.WriteValue(section, key, param);
        }


        public static bool UpdateParamFromDictToUI<T>(Dictionary<string, string> dictParam, T application_Category/*, ref object local_Category*/)
        {
            try
            {
                PropertyInfo[] arrInfo = application_Category.GetType().GetProperties();
                for (int i = 0; i < arrInfo.Count(); i++)
                {
                    PropertyInfo info = arrInfo[i];
                    Type type = info.PropertyType;

                    var attributes = info.GetCustomAttributes(typeof(CategoryAttribute), true);
                    string strGroup = "";
                    string strName = "";
                    if (attributes.Length > 0)
                    {
                        CategoryAttribute attr = (CategoryAttribute)attributes[0];
                        strGroup = attr.Category;
                        //Console.WriteLine($"Is MyProperty browsable? {isBrowsable}");
                    }

                    attributes = info.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    if (attributes.Length > 0)
                    {
                        DisplayNameAttribute attr = (DisplayNameAttribute)attributes[0];
                        strName = attr.DisplayName;
                        //Console.WriteLine($"Is MyProperty browsable? {isBrowsable}");
                    }

                    if (strName == "" || strGroup == "")
                        continue;

                    //var a = info.GetCustomAttributes(typeof(CategoryAttribute), true);
                    //string strGroup = info.GetCustomAttributes(typeof(CategoryAttribute), true).ToString();// +  ElementAt(nAttribute).ConstructorArguments[0].Value.ToString();
                    // string strName = info.GetCustomAttributes(typeof(DisplayNameAttribute), true).ToString();// info.CustomAttributes.ElementAt(nAttribute + 1).ConstructorArguments[0].Value.ToString();
                    string strParameterName = (strGroup + strName).Replace(" ", "").ToLower();


                    bool bKeyFound = false;
                    foreach (KeyValuePair<string, string> kvp in dictParam)
                    {
                        if (kvp.Key.Contains(strParameterName))
                        {
                            bKeyFound = true;
                            break;
                        }
                    }

                    if (!bKeyFound)
                        continue;

                    if (type.Name == "Int32")
                    {
                        int value = 0;
                        bool success = true;

                        string str_value = "";
                        dictParam.TryGetValue(strParameterName, out str_value);
                        if (str_value == null)
                            value = (int)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        else
                            success = Int32.TryParse(dictParam[strParameterName].ToString(), out value);

                        if (success == false)
                            value = (int)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(application_Category, value);
                        //info.SetValue(local_Category, value);
                    }
                    else if (type.Name == "Color")
                    {
                        object value = dictParam[strParameterName] == "white" ? Colors.White : Colors.Black;
                        info.SetValue(application_Category, value);
                    }

                    else if (type.Name == "THRESHOLD_TYPE")
                    {
                        THRESHOLD_TYPE value;
                        bool success = Enum.TryParse(dictParam.Values.ElementAt(i), out value);
                        if (success == false)
                            value = (THRESHOLD_TYPE)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(application_Category, value);
                        //info.SetValue(local_Category, value);
                    }

                    else if (type.Name == "OBJECT_COLOR")
                    {
                        OBJECT_COLOR value;
                        bool success = Enum.TryParse(dictParam.Values.ElementAt(i), out value);
                        if (success == false)
                            value = (OBJECT_COLOR)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(application_Category, value);
                        //info.SetValue(local_Category, value);
                    }

                    else if (type.Name == "AREA_INDEX")
                    {
                        AREA_INDEX value;
                        bool success = Enum.TryParse(dictParam.Values.ElementAt(i), out value);
                        if (success == false)
                            value = (AREA_INDEX)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(application_Category, value);
                        //info.SetValue(local_Category, value);
                    }

                    else if (type.Name == "Double")
                    {
                        double value = 0.0;
                        bool success = true;
                        string str_value = "";

                        //CultureInfo cultureInfo = new CultureInfo("en-US"); // US culture, uses "." as decimal separator

                        dictParam.TryGetValue(strParameterName, out str_value);
                        if (str_value == null)
                        {
                            value = (double)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        }
                        else
                            success = double.TryParse(str_value, /*NumberStyles.Float, cultureInfo,*/ out value);

                        if (success == false)
                            value = (double)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(application_Category, value);
                        //info.SetValue(local_Category, value);

                    }
                    else if (type.Name == "List`1")
                    {
                        if (type.FullName.Contains("Int32"))
                        {
                            int[] value = new int[3];
                            List<int> listValue;
                            string str_value = "";
                            dictParam.TryGetValue(strParameterName, out str_value);
                            if (str_value == null)
                            {
                                value = (int[])info.GetCustomAttribute<DefaultValueAttribute>().Value;
                                listValue = new List<int>(value);
                            }
                            else
                                listValue = TypeConverterHelper.ConverStringToList(dictParam[strParameterName]);
                            info.SetValue(application_Category, listValue);
                            //info.SetValue(local_Category, listValue);

                        }
                        else if (type.FullName.Contains("Rectangles"))
                        {
                            List<Rectangles> listValue = new List<Rectangles> { };
                            foreach (KeyValuePair<string, string> kvp in dictParam)
                            {
                                if (kvp.Key.Contains(strParameterName))
                                    listValue.Add(TypeConverterHelper.GetRectangles(dictParam[kvp.Key.ToString()]));
                            }
                            info.SetValue(application_Category, listValue);
                            //info.SetValue(local_Category, listValue);
                        }
                    }

                    else if (type.Name == "Rectangles" || type.Name == "RectanglesModel")
                    {

                        Rectangles rect = TypeConverterHelper.GetRectangles(dictParam[strParameterName]);
                        RectanglesModel rectanglesModel = new RectanglesModel();
                        rectanglesModel.SetRectangle(rect);
                        info.SetValue(application_Category, rectanglesModel);
                        //info.SetValue(local_Category, rect);

                    }
                    else if (type.Name == "String")
                    {

                        string str = dictParam[strParameterName].ToString();
                        if (str == "")
                            str = (string)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(application_Category, str);

                        //info.SetValue(local_Category, str);

                    }
                    else if (type.Name == "Boolean")
                    {

                        bool value = false;
                        bool success = bool.TryParse(dictParam[strParameterName], out value);
                        if (success == false)
                            value = (bool)info.GetCustomAttribute<DefaultValueAttribute>().Value;
                        info.SetValue(application_Category, value);
                        //info.SetValue(local_Category, value);

                    }

                }
            }
            catch
            {
                return false;
            }
            return true;
        }


    }
}
