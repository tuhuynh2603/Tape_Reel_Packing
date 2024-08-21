using TapeReelPacking.Source.Algorithm;
using TapeReelPacking.Source.Define;
using TapeReelPacking.UI.UserControls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace TapeReelPacking.Source.Application
{
    public class Application
    {
        public const int TOTAL_AREA = 5;
        public static int m_nTrack = (int)TRACK_TYPE.TRACK_ALL;
        public static int m_nDoc = 1;
        public static bool m_bEnableSavingOnlineImage = false;

        public static TeachParametersUC.CategoryTeachParameter categoriesTeachParam = new TeachParametersUC.CategoryTeachParameter();
        //public static TeachParametersUC.CategoryAreaParameter categoryAreaParam = new TeachParametersUC.CategoryAreaParameter();

        public static MappingSetingUC.CatergoryMappingParameters categoriesMappingParam = new MappingSetingUC.CatergoryMappingParameters();

        public static Dictionary<string, string> dictTeachParam = new Dictionary<string, string>();
        public static Dictionary<string, string>[] dictPVIAreaParam = new Dictionary<string, string>[TOTAL_AREA];
        public static Dictionary<string, string> dictMappingParam = new Dictionary<string, string>();

        public static string pathRecipe;// = "C:\\Wisely\\C#\\TapeReelPacking\\Config";
        public static string currentRecipe;// = "Recipe1";
        public static string m_strCurrentLot = "";
        public static string m_strStartLotDay = "";
        public const string  m_strCurrentLot_Registry = "Lot ID";
        public static string[] m_strCurrentDeviceID_Registry = { "Current Device ID 1", "Current Device ID 2" };


        public static string pathRegistry;
        public static string pathImageSave;
        public static string pathStatistics;

        public static List<string> m_strCameraSerial;
        public static int[] m_Width = { 3840, 680 };
        public static int[] m_Height = { 2748, 512 };
        static RegistryKey registerPreferences;

        public Application()
        {
            if (!CheckMuTexProcess())
            {
                MessageBox.Show("The other Application is running!");
                KillCurrentProcess();
            }
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.SetValue("HD Tape And Reel Packing Vision", System.Windows.Forms.Application.ExecutablePath.ToString());
        }

        #region KILL PROCCESS
        public void KillCurrentProcess()
        {
            Environment.Exit(0);
        }
        #endregion

        #region CHECK MUTEX (Run Only One SW)
        public bool CheckMuTexProcess()
        {
            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                return false;
            }
            else
            { return true; }
        }
        #endregion

        public static void CheckRegistry()
        {
            pathRegistry = "Software\\HD Vision\\Tape And Reel Vision";
            RegistryKey register = Registry.CurrentUser.CreateSubKey(pathRegistry, true);
            registerPreferences = Registry.CurrentUser.CreateSubKey(pathRegistry + "\\Preferences", true);

        }
        public void SetRegistry()
        {

        }


        public static string GetStringRegistry(string strKey, string strDefault)
        {
            string strTemp = "";
            if (registerPreferences == null)
                CheckRegistry();


            if ((string)registerPreferences.GetValue(strKey) == "" || (string)registerPreferences.GetValue(strKey) == null)
            {
                strTemp = strDefault;
                registerPreferences.SetValue(strKey, strTemp);
            }
            else
                strTemp = (string)registerPreferences.GetValue(strKey);

            return strTemp;
        }

        public static void SetStringRegistry(string strKey, string strValue)
        {
            //strInOutput = strValue;
            registerPreferences.SetValue(strKey, strValue);
        }



        public static int GetIntRegistry(string strKey, int nDefault)
        {
            int nValue = 0;
            if (registerPreferences.GetValue(strKey) == null)
            {
                nValue = nDefault;
                registerPreferences.SetValue(strKey, nValue);
            }
            else
                nValue = (int)registerPreferences.GetValue(strKey);

            return nValue;
        }

        public static void SetIntRegistry( string strKey, int nValue)
        {
            //nInOutput = nValue;
            registerPreferences.SetValue(strKey, nValue);
        }



        public static void LoadRegistry()
        {
            pathRecipe = GetStringRegistry("Folder: Recipe", "C:\\Magnus_SemiConductor_Config");
            if (!Directory.Exists(pathRecipe))
                Directory.CreateDirectory(pathRecipe);

            currentRecipe = GetStringRegistry("Recipe Name", "Default");
            if (!Directory.Exists(pathRecipe + "\\" + currentRecipe))
                Directory.CreateDirectory(pathRecipe + "\\" + currentRecipe);

            //// Load lot ID


            string strLotTemp = string.Format("{0}{1}{2}_{3}{4}{5}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
            m_strCurrentLot = GetStringRegistry(m_strCurrentLot_Registry, strLotTemp);
            MainWindow.mainWindow.m_strCurrentLotID = m_strCurrentLot;
            m_strStartLotDay = m_strCurrentLot.Split('_')[0];

            #region Load Folder Save Image
            pathImageSave = GetStringRegistry("Folder: Image Save", "C:\\SemiConductor Images");


            if (!Directory.Exists(pathImageSave))
                Directory.CreateDirectory(pathImageSave);

            #endregion

            #region Load Folder Lot Result Image
            pathStatistics = GetStringRegistry("Folder: Statistics", "C:\\SemiConductor Statistics");


            if (!Directory.Exists(pathStatistics))
                Directory.CreateDirectory(pathStatistics);

            #endregion



            m_strCameraSerial = new List<string>();
            for (int nTrack = 0; nTrack < m_nTrack; nTrack++)
            {
                m_strCameraSerial.Add(GetCommInfo($"Camera{nTrack + 1} IP Serial: ", ""));
            }

        }

        public static void setRecipeToRegister(string strRecipe)
        {
            currentRecipe = strRecipe;

            RegistryKey registerPreferences = Registry.CurrentUser.CreateSubKey(pathRegistry + "\\Preferences", true);
            registerPreferences.SetValue("Recipe Name", currentRecipe);
        }

        public static string UserDefault;
        public static string LevelDefault;
        public static string PwsDefault;

        private string nameUserDefault;
        private string levelUserDefault;
        private string pwsUserDefault;

        public void acountDefault()
        {
            UserDefault = "Engineer";
            LevelDefault = "Engineer";
            PwsDefault = "6_XZ_VVc25>?";

            nameUserDefault = "Name=" + UserDefault;
            levelUserDefault = "Level=" + LevelDefault;
            pwsUserDefault = "Pswd=" + PwsDefault;
        }

        public static string GetCommInfo(string key, string defaults)
        {
            RegistryKey registerPreferences = Registry.CurrentUser.CreateSubKey(pathRegistry + "\\Hardware", true);
            if ((string)registerPreferences.GetValue(key) == null)
            {
                registerPreferences.SetValue(key, defaults);
                return defaults;
            }
            else
                return (string)registerPreferences.GetValue(key);
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
        static void ReadLine_Magnus(string strGroup, string strName, IniFile ini, ref Dictionary<string, string> dictionary)
        {
            string strParameterName = (strGroup + strName).Replace(" ", "").ToLower();
            //string[] arr = section.Split(' ');
            //string keys = "";
            //foreach (string str in arr)
            //{
            //    keys += str.ElementAt(0);
            //}
            //keys += "_";
            //arr = key.Split(' ');
            //keys += arr[0];
            //if (arr.Length > 1)
            //{
            //    for (int i = 1; i < arr.Length; i++)
            //    {
            //        if (!char.IsDigit(arr[i][0]))
            //            keys += arr[i].Insert(0, arr[i][0].ToString().ToUpper()).Remove(1, 1);
            //        else
            //            keys += arr[i];
            //    }
            //}

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

        void WriteLine(string section, string key, IniFile ini, string param)
        {
            ini.WriteValue(section, key, param);
        }

        public static CameraSettingParam cameraSettingParam = new CameraSettingParam();

        public static void LoadCamSetting(int nTrack)
        {
            if (nTrack != 0)
                return;
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


        public static void LoadAreaParamFromFileToDict(ref int nTrack, int nAreaIndex = TOTAL_AREA)
        {
            if (currentRecipe == null || pathRecipe == null)
                return;

            //if (nAreaIndex == TOTAL_AREA)
            //{
            //    for (int n = 0; n < TOTAL_AREA; n++)
            //    {

            //        string strFileName = $"PVIAreaParameters_Track{nTrack + 1}_Area{n + 1}" + ".cfg";
            //        string pathFile = Path.Combine(pathRecipe, currentRecipe, strFileName);
            //        IniFile ini = new IniFile(pathFile);

            //        //ReadLine_Magnus("DEFECT ROI", $"Defect ROI Locations {n + 1}", ini, ref dictTeachParam);
            //        ReadLine_Magnus("LABEL DEFECT", $"Area Enable {n + 1}", ini, ref dictPVIAreaParam[n]);
            //        ReadLine_Magnus("LABEL DEFECT", $"Lower Threshold {n + 1}", ini, ref dictPVIAreaParam[n]);
            //        ReadLine_Magnus("LABEL DEFECT", $"Upper Threshold {n + 1}", ini, ref dictPVIAreaParam[n]);
            //        ReadLine_Magnus("LABEL DEFECT", $"Opening Mask {n + 1}", ini, ref dictPVIAreaParam[n]);
            //        ReadLine_Magnus("LABEL DEFECT", $"Dilation Mask {n + 1}", ini, ref dictPVIAreaParam[n]);
            //    }
            //}
            //else
            //{

            string strFileName = $"PVIAreaParameters_Track{nTrack + 1}_Area{nAreaIndex + 1}" + ".cfg";
            string pathFile = Path.Combine(pathRecipe, currentRecipe, strFileName);
            IniFile ini = new IniFile(pathFile);

            ReadLine_Magnus("LABEL DEFECT", $"Area Enable", ini, ref dictPVIAreaParam[nAreaIndex]);
            ReadLine_Magnus("LABEL DEFECT", $"Lower Threshold", ini, ref dictPVIAreaParam[nAreaIndex]);
            ReadLine_Magnus("LABEL DEFECT", $"Upper Threshold", ini, ref dictPVIAreaParam[nAreaIndex]);
            ReadLine_Magnus("LABEL DEFECT", $"Opening Mask", ini, ref dictPVIAreaParam[nAreaIndex]);
            ReadLine_Magnus("LABEL DEFECT", $"Dilation Mask", ini, ref dictPVIAreaParam[nAreaIndex]);
            ReadLine_Magnus("LABEL DEFECT", $"Object Cover PerCent", ini, ref dictPVIAreaParam[nAreaIndex]);
            //}
        }

        public void WritePVIAreaParam(int nTrack, int nAreaIndex)
        {


            string strFileName = $"PVIAreaParameters_Track{nTrack + 1}_Area{nAreaIndex + 1}" + ".cfg";
            string pathFile = Path.Combine(pathRecipe, currentRecipe, strFileName);

            string strDateTime = string.Format("({0}.{1}.{2}_{3}.{4}.{5})", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"));
            string backup_path = Path.Combine(pathRecipe, currentRecipe, "Backup_Teach Parameter");
            if (!Directory.Exists(backup_path))
                Directory.CreateDirectory(backup_path);

            string backup_fullpath = Path.Combine(backup_path, $"PVIAreaParameters_Track{nTrack + 1}_Area{nAreaIndex + 1} {strDateTime}" + ".cfg");
            FileInfo file = new FileInfo(pathFile);

            if (!file.Exists)
                file.Create();

            file.MoveTo(backup_fullpath);
            file.Create();

            IniFile ini = new IniFile(pathFile);
            InspectionCore inspectionCore = MainWindow.mainWindow.master.m_Tracks[nTrack].m_InspectionCore;

            WriteLine("LABEL DEFECT", $"Area Enable", ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_DR_AreaEnable.ToString());
            WriteLine("LABEL DEFECT", $"Lower Threshold", ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_lowerThreshold.ToString());
            WriteLine("LABEL DEFECT", $"Upper Threshold", ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_upperThreshold.ToString());
            WriteLine("LABEL DEFECT", $"Opening Mask", ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_OpeningMask.ToString());
            WriteLine("LABEL DEFECT", $"Dilation Mask", ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_DilationMask.ToString());
            WriteLine("LABEL DEFECT", $"Object Cover PerCent", ini, inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_LD_ObjectCoverPercent.ToString());

            

        }


        public static void LoadTeachParamFromFileToDict(ref int nTrack)
        {
            if (currentRecipe == null || pathRecipe == null)
                return;
            string strFileName = "TeachParameters_Track" + (nTrack + 1).ToString() + ".cfg";
            string pathFile = Path.Combine(pathRecipe, currentRecipe, strFileName);
            IniFile ini = new IniFile(pathFile);

            ReadLine_Magnus("LOCATION", "Device Location Roi", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Location Enable", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Threshold Method", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Object Color", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "lower threshold", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "upper threshold", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "lower threshold Inner Chip", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "upper threshold Inner Chip", ini, ref dictTeachParam);

            ReadLine_Magnus("LOCATION", "Opening Mask", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Dilation Mask", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Min Width Device", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Min Height Device", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Template Roi", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Number Side", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Scale Image Ratio", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Min Score", ini, ref dictTeachParam);
            ReadLine_Magnus("LOCATION", "Corner Index", ini, ref dictTeachParam);


            ReadLine_Magnus("OPPOSITE CHIP", "Enable", ini, ref dictTeachParam);
            ReadLine_Magnus("OPPOSITE CHIP", "lower threshold", ini, ref dictTeachParam);
            ReadLine_Magnus("OPPOSITE CHIP", "upper threshold", ini, ref dictTeachParam);
            ReadLine_Magnus("OPPOSITE CHIP", "Opening Mask", ini, ref dictTeachParam);
            ReadLine_Magnus("OPPOSITE CHIP", "Dilation Mask", ini, ref dictTeachParam);
            ReadLine_Magnus("OPPOSITE CHIP", "Min Width Device", ini, ref dictTeachParam);
            ReadLine_Magnus("OPPOSITE CHIP", "Min Height Device", ini, ref dictTeachParam);

            ReadLine_Magnus("DEFECT ROI", $"Number ROI Location", ini, ref dictTeachParam);
            ReadLine_Magnus("DEFECT ROI", "Area Index", ini, ref dictTeachParam);

            for (int n = 0; n < TOTAL_AREA; n++)
            {
                ReadLine_Magnus("DEFECT ROI", $"Defect ROI Locations {n + 1}", ini, ref dictTeachParam);
                //ReadLine_Magnus("LABEL DEFECT", $"Area Enable {n + 1}", ini, ref dictTeachParam);

                //ReadLine_Magnus("LABEL DEFECT", $"Lower Threshold {n + 1}", ini, ref dictTeachParam);
                //ReadLine_Magnus("LABEL DEFECT", $"Upper Threshold {n + 1}", ini, ref dictTeachParam);
                //ReadLine_Magnus("LABEL DEFECT", $"Opening Mask {n + 1}", ini, ref dictTeachParam);
                //ReadLine_Magnus("LABEL DEFECT", $"Dilation Mask {n + 1}", ini, ref dictTeachParam);
            }


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

            file.MoveTo(backup_fullpath);
            file.Create();

            IniFile ini = new IniFile(pathFile);
            InspectionCore inspectionCore = MainWindow.mainWindow.master.m_Tracks[nTrack].m_InspectionCore;

            WriteLine("LOCATION", "Device Location Roi", ini, ConvertRectanglesToString(inspectionCore.m_DeviceLocationParameter.m_L_DeviceLocationRoi));
            WriteLine("LOCATION", "Location Enable", ini, inspectionCore.m_DeviceLocationParameter.m_L_LocationEnable.ToString());

            WriteLine("LOCATION", "Threshold Method", ini, inspectionCore.m_DeviceLocationParameter.m_L_ThresholdType.ToString());
            WriteLine("LOCATION", "Object Color", ini, inspectionCore.m_DeviceLocationParameter.m_L_ObjectColor.ToString());
          
            WriteLine("LOCATION", "lower threshold", ini, inspectionCore.m_DeviceLocationParameter.m_L_lowerThreshold.ToString());
            WriteLine("LOCATION", "upper threshold", ini, inspectionCore.m_DeviceLocationParameter.m_L_upperThreshold.ToString());
            WriteLine("LOCATION", "lower threshold Inner Chip", ini, inspectionCore.m_DeviceLocationParameter.m_L_lowerThresholdInnerChip.ToString());
            WriteLine("LOCATION", "upper threshold Inner Chip", ini, inspectionCore.m_DeviceLocationParameter.m_L_upperThresholdInnerChip.ToString());
            WriteLine("LOCATION", "Opening Mask", ini, inspectionCore.m_DeviceLocationParameter.m_L_OpeningMask.ToString());
            WriteLine("LOCATION", "Dilation Mask", ini, inspectionCore.m_DeviceLocationParameter.m_L_DilationMask.ToString());
            WriteLine("LOCATION", "Min Width Device", ini, inspectionCore.m_DeviceLocationParameter.m_L_MinWidthDevice.ToString());
            WriteLine("LOCATION", "Min Height Device", ini, inspectionCore.m_DeviceLocationParameter.m_L_MinHeightDevice.ToString());

            WriteLine("LOCATION", "Template Roi", ini, ConvertRectanglesToString(inspectionCore.m_DeviceLocationParameter.m_L_TemplateRoi));
            //WriteLine("LOCATION", "Number Side", ini, inspectionCore.m_DeviceLocationParameter.m_L_NumberSide.ToString());
            WriteLine("LOCATION", "Scale Image Ratio", ini, inspectionCore.m_DeviceLocationParameter.m_L_ScaleImageRatio.ToString());
            WriteLine("LOCATION", "Min Score", ini, inspectionCore.m_DeviceLocationParameter.m_L_MinScore.ToString());
            WriteLine("LOCATION", "Corner Index", ini, inspectionCore.m_DeviceLocationParameter.m_L_CornerIndex.ToString());


            WriteLine("OPPOSITE CHIP", "Enable", ini, inspectionCore.m_blackChipParameter.m_OC_EnableCheck.ToString());
            WriteLine("OPPOSITE CHIP", "lower threshold", ini, inspectionCore.m_blackChipParameter.m_OC_lowerThreshold.ToString());
            WriteLine("OPPOSITE CHIP", "upper threshold", ini, inspectionCore.m_blackChipParameter.m_OC_upperThreshold.ToString());
            WriteLine("OPPOSITE CHIP", "Opening Mask", ini, inspectionCore. m_blackChipParameter.m_OC_OpeningMask.ToString());
            WriteLine("OPPOSITE CHIP", "Dilation Mask", ini, inspectionCore.m_blackChipParameter.m_OC_DilationMask.ToString());
            WriteLine("OPPOSITE CHIP", "Min Width Device", ini, inspectionCore. m_blackChipParameter.m_OC_MinWidthDevice.ToString());
            WriteLine("OPPOSITE CHIP", "Min Height Device", ini, inspectionCore.m_blackChipParameter.m_OC_MinHeightDevice.ToString());



            WriteLine("DEFECT ROI", $"Number ROI Location", ini, inspectionCore.m_DeviceLocationParameter.m_DR_NumberROILocation.ToString());
            WriteLine("DEFECT ROI", $"Defect ROI Index", ini, inspectionCore.m_DeviceLocationParameter.m_DR_DefectROIIndex.ToString());

            for (int nAreaIndex = 0; nAreaIndex < TOTAL_AREA; nAreaIndex++)
            {
                
                WriteLine("DEFECT ROI", $"Defect ROI Locations {nAreaIndex+1}", ini, ConvertRectanglesToString(inspectionCore.m_SurfaceDefectParameter[nAreaIndex].m_DR_DefectROILocations));
                WritePVIAreaParam(nTrack, nAreaIndex);
            }

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

            ReadLine_Magnus("MAPPING", "Number Device X", ini, ref dictMappingParam);
            ReadLine_Magnus("MAPPING", "Number Device Y", ini, ref dictMappingParam);
            ReadLine_Magnus("MAPPING", "Number Device Per Lot", ini, ref dictMappingParam);

        }

        public void WriteMappingParam()
        {
            string pathFile = Path.Combine(pathRecipe, currentRecipe, "MappingParameters.cfg");
            IniFile ini = new IniFile(pathFile);
            WriteLine("MAPPING", "Number Device X", ini, categoriesMappingParam.M_NumberDeviceX.ToString());
            WriteLine("MAPPING", "Number Device Y", ini, categoriesMappingParam.M_NumberDeviceY.ToString());
            WriteLine("MAPPING", "Number Device Per Lot", ini, categoriesMappingParam.M_NumberDevicePerLot.ToString());        
        }


        public static bool UpdateParamFromDictToUI(Dictionary<string, string> dictParam, ref object application_Category/*, ref object local_Category*/)
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
                                listValue = ConverStringToList(dictParam[strParameterName]);
                            info.SetValue(application_Category, listValue);
                            //info.SetValue(local_Category, listValue);

                        }
                        else if (type.FullName.Contains("Rectangles"))
                        {
                            List<Rectangles> listValue = new List<Rectangles> { };
                            foreach (KeyValuePair<string, string> kvp in dictParam)
                            {
                                if (kvp.Key.Contains(strParameterName))
                                    listValue.Add(GetRectangles(dictParam[kvp.Key.ToString()]));
                            }
                            info.SetValue(application_Category, listValue);
                            //info.SetValue(local_Category, listValue);
                        }
                    }

                    else if (type.Name == "Rectangles")
                    {

                        Rectangles rect = GetRectangles(dictParam[strParameterName]);
                        info.SetValue(application_Category, rect);
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
        private static List<int> ConverStringToList(string str)
        {
            List<int> list = new List<int>();
            string[] value = str.Split(':');
            foreach (string s in value)
            {
                list.Add(int.Parse(s));
            }
            return list;
        }

        private static Rectangles GetRectangles(string str)
        {
            if (str == "")
                return new Rectangles(0, 0, 0, 0);
            string[] value = str.Split(':');
            if (value.Length == 4)
                return new Rectangles(double.Parse(value[0]), double.Parse(value[1]), double.Parse(value[2]), double.Parse(value[3]));
            else
                return new Rectangles(double.Parse(value[0]), double.Parse(value[1]), double.Parse(value[2]), double.Parse(value[3]), double.Parse(value[4]));

        }


        #region Acount
        public DataTable tableAccount = new DataTable();
        public static LoginUser loginUser = new LoginUser();
        public void ReadLogAccount()
        {
            DataColumn column = new DataColumn();

            tableAccount = new DataTable();
            column.ColumnName = "username";
            column.DataType = typeof(string);
            column.Unique = true;
            tableAccount.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "access";
            column.DataType = typeof(AccessLevel);
            column.Unique = false;
            tableAccount.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "password";
            column.DataType = typeof(string);
            column.Unique = false;
            tableAccount.Columns.Add(column);

            Directory.CreateDirectory(pathRecipe);
            string pathFile = Path.Combine(pathRecipe, "LogAccount.lgn");
            if (!File.Exists(pathFile))
            {
                // pw: Engineer
                string[] fileAccount =
                {
                    "[NUM USER]",
                    "NoOfUsers=1",
                    "",
                    "[User0]",
                   nameUserDefault,
                   levelUserDefault,
                   pwsUserDefault,
                };

                using (StreamWriter Files = new StreamWriter(pathFile))
                {
                    foreach (string line in fileAccount)
                        Files.WriteLine(line);
                }
                // To do: convert datatable
                GetTableAccount(tableAccount, fileAccount);
                loginUser.tableAccount = tableAccount;
            }
            else
            {
                string[] fileAccount = File.ReadAllLines(pathFile);
                GetTableAccount(tableAccount, fileAccount);
                loginUser.tableAccount = tableAccount;

            }
        }
        public void GetTableAccount(DataTable tableacount, string[] accounts)
        {
            DataRow row;
            foreach (string line in accounts)
            {
                if (line.Contains("[User"))
                {
                    int pos = Array.IndexOf(accounts, line);
                    row = tableacount.NewRow();
                    row["username"] = accounts[++pos].Split('=')[1];
                    row["access"] = StringToAccessLevel(accounts[++pos].Split('=')[1]);
                    row["password"] = accounts[++pos].Split('=')[1];
                    tableacount.Rows.Add(row);
                }
            }
        }
        private AccessLevel StringToAccessLevel(string level)
        {
            if (string.Compare(level, "Engineer", true) == 0) return AccessLevel.Engineer;
            else if (string.Compare(level, "Operator", true) == 0) return AccessLevel.Operator;
            else if (string.Compare(level, "User", true) == 0) return AccessLevel.User;
            else return AccessLevel.None;
        }

        #endregion

        public static int LineNumber([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            return lineNumber;
        }
        public static string PrintCallerName()
        {
            MethodBase caller = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            string callerMethodName = caller.Name;
            string calledMethodName = MethodBase.GetCurrentMethod().Name;
            return $"{callerMethodName}  : {calledMethodName}";
        }

    }
}
