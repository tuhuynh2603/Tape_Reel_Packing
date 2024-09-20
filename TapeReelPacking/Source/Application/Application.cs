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
using TapeReelPacking.UI.UserControls.ViewModel;
using TapeReelPacking.Source.Model;
using Rectangles = TapeReelPacking.Source.Define.Rectangles;
using TapeReelPacking.UI.UserControls.View;
using TapeReelPacking.Source.Helper;
using static TapeReelPacking.UI.UserControls.ViewModel.MappingSetingUCVM;

namespace TapeReelPacking.Source.Application
{
    public class Application
    {
        public const int TOTAL_AREA = 5;
        public static int m_nTrack = (int)TRACK_TYPE.TRACK_ALL;
        public static int m_nDoc = 1;
        public static bool m_bEnableSavingOnlineImage = false;

        //public static TeachParametersUC.CategoryAreaParameter categoryAreaParam = new TeachParametersUC.CategoryAreaParameter();

        //public static MappingSetingUCVM.CatergoryMappingParameters categoriesMappingParam = new MappingSetingUCVM.CatergoryMappingParameters();

        //public static Dictionary<string, string> dictTeachParam = new Dictionary<string, string>();

        //public static Dictionary<string, string> dictMappingParam = new Dictionary<string, string>();

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

        MainWindowVM _mainWindowVM { set; get; }    
        public Application(MainWindowVM mainWindowVM)
        {
            _mainWindowVM = mainWindowVM;

            Application.CheckRegistry();
            Application.LoadRegistry();

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
            //_mainWindowVM.m_strCurrentLotID = m_strCurrentLot;
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
                m_strCameraSerial.Add(FileHelper.GetCommInfo($"Camera{nTrack + 1} IP Serial: ", "", Application.pathRegistry));

            }

        }

        public static void setRecipeToRegister(string strRecipe)
        {
            currentRecipe = strRecipe;

            RegistryKey registerPreferences = Registry.CurrentUser.CreateSubKey(pathRegistry + "\\Preferences", true);
            registerPreferences.SetValue("Recipe Name", currentRecipe);
        }




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
