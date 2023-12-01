using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Define;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Magnus_WPF_1.Source.Application
{
    public class Application
    {
        public static int m_nTrack = (int)TRACK_TYPE.TRACK_ALL;
        public static int m_nDoc = 1;
        public static bool m_bEnableSavingOnlineImage = true;

        public static TeachParametersUC.CategoryTeachParameter categoriesTeachParam = new TeachParametersUC.CategoryTeachParameter();
        public static MappingSetingUC.CatergoryMappingParameters categoriesMappingParam = new MappingSetingUC.CatergoryMappingParameters();

        public static Dictionary<string, string> dictTeachParam = new Dictionary<string, string>();
        public static Dictionary<string, string> dictMappingParam = new Dictionary<string, string>();

        public static string pathRecipe;// = "C:\\Wisely\\C#\\Magnus_WPF_1\\Config";
        public static string currentRecipe;// = "Recipe1";
        public static string pathRegistry;
        public static string pathImageSave;
        public static List<string> m_strCameraSerial;
        public static int[] m_Width = { 3840, 680 };
        public static int[] m_Height = { 2748, 512 };

        public static void CheckRegistry()
        {
            pathRegistry = "Software\\HD Vision\\SemiConductor_1";
            RegistryKey register = Registry.CurrentUser.CreateSubKey(pathRegistry, true);
        }
        public void SetRegistry()
        {

        }
        public static void LoadRegistry()
        {
            RegistryKey registerPreferences = Registry.CurrentUser.CreateSubKey(pathRegistry + "\\Preferences", true);

            if ((string)registerPreferences.GetValue("Folder: Recipe") == "" || (string)registerPreferences.GetValue("Folder: Recipe") == null)
            {
                pathRecipe = "C:\\Magnus_SemiConductor_Config";
                registerPreferences.SetValue("Folder: Recipe", pathRecipe);
                if (!Directory.Exists(pathRecipe))
                    Directory.CreateDirectory(pathRecipe);
            }
            else
                pathRecipe = (string)registerPreferences.GetValue("Folder: Recipe");

            if ((string)registerPreferences.GetValue("Recipe Name") == "" || (string)registerPreferences.GetValue("Recipe Name") == null)
            {
                currentRecipe = "Default";
                if (!Directory.Exists(pathRecipe + "\\" + currentRecipe))
                    Directory.CreateDirectory(pathRecipe + "\\" + currentRecipe);
                registerPreferences.SetValue("Recipe Name", "Default");
            }
            else
                currentRecipe = (string)registerPreferences.GetValue("Recipe Name");

            #region Load Folder Save Image

            if ((string)registerPreferences.GetValue("Folder: Image Save") == "" || (string)registerPreferences.GetValue("Folder: Image Save") == null)
            {
                pathImageSave = "C:\\Magnus SemiConductor Images"; /*+ "\\"+ (string)registerPreferences.GetValue("Recipe")*/;
                if (!Directory.Exists(pathImageSave))
                    Directory.CreateDirectory(pathImageSave);
                registerPreferences.SetValue("Folder: Image Save", pathImageSave);
            }
            else
                pathImageSave = (string)registerPreferences.GetValue("Folder: Image Save");
            #endregion


            m_strCameraSerial = new List<string>();
            for (int nTrack = 0; nTrack < m_nTrack; nTrack++)
            {
                if ((string)registerPreferences.GetValue("Camera"+ (nTrack+1).ToString() + " IP Serial: ") == "" || (string)registerPreferences.GetValue("Camera" + (nTrack + 1).ToString() + " IP Serial: ") == null)
                {
                    m_strCameraSerial.Add(""); /*+ "\\"+ (string)registerPreferences.GetValue("Recipe")*/;
                    registerPreferences.SetValue("Camera" + (nTrack + 1).ToString() + " IP Serial: ", m_strCameraSerial[nTrack]);
                }
                else
                    m_strCameraSerial.Add((string)registerPreferences.GetValue("Camera" + (nTrack + 1).ToString() + " IP Serial: "));
            }

        }
        static void ReadLine(string section, string key, IniFile ini, ref Dictionary<string, string> dictionary)
        {
            string[] arr = section.Split(' ');
            string keys = "";
            foreach (string str in arr)
            {
                keys += str.ElementAt(0);
            }
            keys += "_";
            arr = key.Split(' ');
            keys += arr[0];
            if (arr.Length > 1)
            {
                for (int i = 1; i < arr.Length; i++)
                {
                    if (!char.IsDigit(arr[i][0]))
                        keys += arr[i].Insert(0, arr[i][0].ToString().ToUpper()).Remove(1, 1);
                    else
                        keys += arr[i];
                }
            }
            string data = ini.ReadValue(section, key, "");
            if (data == "")
            {
                data = "";
                ini.WriteValue(section, key, data);
            }
            else
            {
                if (data.Split(',').Length > 1)
                    data = data.Split(',')[1];
            }
            if (dictionary.ContainsKey(keys))
                dictionary[keys] = data;
            else
                dictionary.Add(keys, data);
        }

        void WriteLine(string section, string key, IniFile ini, string param)
        {
            ini.WriteValue(section, key, param);
        }

        public static CameraSettingParam cameraSettingParam = new CameraSettingParam();
        public static void LoadCamSetting(int nTrack)
        {

            #region USB Camera
            string strRecipePath = Path.Combine(pathRecipe, currentRecipe);
            string pathCam = Path.Combine(strRecipePath, "camera_Track" + (nTrack + 1).ToString() + ".cam");
            IniFile ini = new IniFile(pathCam);
            cameraSettingParam.gain = (float)ini.ReadValue("Camera Setting", "gain", 5.0);
            cameraSettingParam.exposureTime = (int)ini.ReadValue("Camera Setting", "exposure time", 10000);
            cameraSettingParam.softwareTrigger = ini.ReadValue("Camera Setting", "software trigger", false);
            cameraSettingParam.frameRate = (float)ini.ReadValue("Camera Setting", "FrameRate", 17);
            #endregion

            if (!Directory.Exists(strRecipePath))
            {
                Directory.CreateDirectory(strRecipePath);
                WriteCamSetting(nTrack);
            }
        }
        public static void WriteCamSetting(int nTrack)
        {
                #region USB Camera
                string pathCam = Path.Combine(pathRecipe, currentRecipe);
                string fullpathCam = Path.Combine(pathCam, "camera_Track" + (nTrack + 1).ToString() +".cam");
                IniFile ini = new IniFile(fullpathCam);
                ini.WriteValue("Camera Setting", "gain", cameraSettingParam.gain);
                ini.WriteValue("Camera Setting", "exposure time", cameraSettingParam.exposureTime);
                ini.WriteValue("Camera Setting", "software trigger", cameraSettingParam.softwareTrigger);
                ini.WriteValue("Camera Setting", "FrameRate", cameraSettingParam.frameRate);
            #endregion
            if (!Directory.Exists(pathCam))
            {
                Directory.CreateDirectory(pathCam);
                WriteCamSetting(nTrack);
            }


        }

        public static void LoadTeachParamFromFileToDict(ref int nTrack)
        {
            if (currentRecipe == null || pathRecipe == null)
                return;
            string strFileName = "TeachParameters_Track" + (nTrack+1).ToString() + ".cfg";
            string pathFile = Path.Combine(pathRecipe, currentRecipe, strFileName);
            IniFile ini = new IniFile(pathFile);

            ReadLine("LOCATION", "Device Location Roi", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Threshold Type", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Object Color", ini, ref dictTeachParam);
            ReadLine("LOCATION", "lower threshold", ini, ref dictTeachParam);
            ReadLine("LOCATION", "upper threshold", ini, ref dictTeachParam);
            ReadLine("LOCATION", "lower threshold Inner Chip", ini, ref dictTeachParam);
            ReadLine("LOCATION", "upper threshold Inner Chip", ini, ref dictTeachParam);

            ReadLine("LOCATION", "Opening Mask", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Dilation Mask", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Min Width Device", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Min Height Device", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Template Roi", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Number Side", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Scale Image Ratio", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Min Score", ini, ref dictTeachParam);
            ReadLine("LOCATION", "Corner Index", ini, ref dictTeachParam);
            //ReadLine("TOP PATTERN", "no of pattern", ini, ref dictTeachParam);
            ////ReadLine("TOP PATTERN", "no of pattern", ini, ref categoriesTeachParam.TP_noOfPattern);
            ////categoriesTeachParam.TP_noOfPattern = dictTeachParam[]
            ////double a = int.Parse(dictTeachParam["TP_noOfPattern"]);
            //if (dictTeachParam["TP_noOfPattern"] != "")
            //{
            //    for (int n = 0; n < int.Parse(dictTeachParam["TP_noOfPattern"]); n++)
            //        ReadLine("TOP PATTERN", "roi no " + (n + 1).ToString(), ini, ref dictTeachParam);
            //}
        }
        public void WriteTeachParam(int nTrack)
        {

            string strFileName = "TeachParameters_Track" + (nTrack + 1).ToString() + ".cfg";
            string pathFile = Path.Combine(pathRecipe, currentRecipe, strFileName);

            string strDateTime = string.Format("({0}.{1}.{2}_{3}.{4}.{5})", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
            string backup_path = Path.Combine(pathRecipe, currentRecipe, "Backup_Teach Parameter");
            if (!Directory.Exists(backup_path))
                Directory.CreateDirectory(backup_path);

            string backup_fullpath = Path.Combine(backup_path, $"TeachParameters_Track{nTrack + 1} {strDateTime}" + ".cfg");
            FileInfo file = new FileInfo(pathFile);

            if (!file.Exists)
                file.Create();

            file.CopyTo(backup_fullpath);

            IniFile ini = new IniFile(pathFile);
            WriteLine("LOCATION", "Device Location Roi", ini, ConvertRectanglesToString(categoriesTeachParam.L_DeviceLocationRoi));
            WriteLine("LOCATION", "Threshold Type", ini, categoriesTeachParam.L_ThresholdType.ToString());
            WriteLine("LOCATION", "Object Color", ini, categoriesTeachParam.L_ObjectColor.ToString());
          
            WriteLine("LOCATION", "lower threshold", ini, categoriesTeachParam.L_lowerThreshold.ToString());
            WriteLine("LOCATION", "upper threshold", ini, categoriesTeachParam.L_upperThreshold.ToString());
            WriteLine("LOCATION", "lower threshold Inner Chip", ini, categoriesTeachParam.L_lowerThresholdInnerChip.ToString());
            WriteLine("LOCATION", "upper threshold Inner Chip", ini, categoriesTeachParam.L_upperThresholdInnerChip.ToString());
            WriteLine("LOCATION", "Opening Mask", ini, categoriesTeachParam.L_OpeningMask.ToString());
            WriteLine("LOCATION", "Dilation Mask", ini, categoriesTeachParam.L_DilationMask.ToString());
            WriteLine("LOCATION", "Min Width Device", ini, categoriesTeachParam.L_MinWidthDevice.ToString());
            WriteLine("LOCATION", "Min Height Device", ini, categoriesTeachParam.L_MinHeightDevice.ToString());

            WriteLine("LOCATION", "Template Roi", ini, ConvertRectanglesToString(categoriesTeachParam.L_TemplateRoi));
            WriteLine("LOCATION", "Number Side", ini, categoriesTeachParam.L_NumberSide.ToString());
            WriteLine("LOCATION", "Scale Image Ratio", ini, categoriesTeachParam.L_ScaleImageRatio.ToString());
            WriteLine("LOCATION", "Min Score", ini, categoriesTeachParam.L_MinScore.ToString());
            WriteLine("LOCATION", "Corner Index", ini, categoriesTeachParam.L_CornerIndex.ToString());

            //WriteLine("TOP PATTERN", "no of pattern", ini, categoriesTeachParam.TP_noOfPattern.ToString());

            //if (categoriesTeachParam.TP_noOfPattern != categoriesTeachParam.TP_roiNo.Count())
            //{
            //    categoriesTeachParam.TP_roiNo.Clear();
            //    for (int n = 0; n <= categoriesTeachParam.TP_noOfPattern; n++)
            //    {
            //        Rectangles rec = new Rectangles();
            //        categoriesTeachParam.TP_roiNo.Add(rec);
            //    }
            //} 
            //for (int n = 0; n < categoriesTeachParam.TP_roiNo.Count(); n++)
            //    WriteLine("TOP PATTERN", "roi no " + (n+1).ToString(), ini, ConvertRectanglesToString(categoriesTeachParam.TP_roiNo[n]));

        }

        public static void LoadMappingParamFromFile()
        {
            string pathFile = Path.Combine(pathRecipe, currentRecipe, "MappingParameters.cfg");
            IniFile ini = new IniFile(pathFile);

            ReadLine("MAPPING", "Number Device X", ini, ref dictMappingParam);
            ReadLine("MAPPING", "Number Device Y", ini, ref dictMappingParam);
            ReadLine("MAPPING", "Number Device Per Lot", ini, ref dictMappingParam);

        }

        public void WriteMappingParam()
        {
            string pathFile = Path.Combine(pathRecipe, currentRecipe, "MappingParameters.cfg");
            IniFile ini = new IniFile(pathFile);
            WriteLine("MAPPING", "Number Device X", ini, categoriesMappingParam.M_NumberDeviceX.ToString());
            WriteLine("MAPPING", "Number Device Y", ini, categoriesMappingParam.M_NumberDeviceY.ToString());
            WriteLine("MAPPING", "Number Device Per Lot", ini, categoriesMappingParam.M_NumberDevicePerLot.ToString());        
        }

        string ConvertRectanglesToString(Rectangles rectangles)
        {
            return string.Format("{0}:{1}:{2}:{3}", rectangles.TopLeft.X, rectangles.TopLeft.Y, rectangles.Width, rectangles.Height);
        }

        string ConvertListToString(List<int> list)
        {
            string format = "";
            for (int i = 0; i < list.Count; i++)
            {
                string temp = "";
                if (i == list.Count - 1)
                    temp = string.Format("{0}", list[i]);
                else
                    temp = string.Format("{0}:", list[i]);
                format = string.Concat(format, temp);
            }

            return format;
        }


    }
}
