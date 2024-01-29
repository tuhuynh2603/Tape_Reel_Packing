using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TapeReelPacking.Source.Application
{
    public class IniFile
    {
        private string filePath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// IniFile Constructor.
        public IniFile(string iniPath, bool flush = false)
        {
            filePath = iniPath;
            if (flush)
                File.Delete(filePath);
        }
        /// Write Data to the Ini File
        public void WriteValue<T>(string section, string key, T val)
        {
            WritePrivateProfileString(section, key, val.ToString(), filePath);
        }

        /// Read Data Value From the Ini File
        public T ReadValue<T>(string section, string key, T defaultVal)
        {
            StringBuilder str = new StringBuilder(255);
            int status = GetPrivateProfileString(section, key, "", str, 255, filePath);
            try
            {
                string str1 = str.ToString();
                if (!string.IsNullOrEmpty(str1))
                    return (T)System.Convert.ChangeType(str1, typeof(T));
            }
            catch
            {

            }

            return defaultVal;
        }

        public List<string> ReadListSection(string inipath, string posSection = "")
        {
            List<string> ListSection = new List<string>();
            if (!File.Exists(inipath))
                return null;

            TextReader iniFile = new StreamReader(inipath);
            string strLine = iniFile.ReadLine();
            string currentRoot = null;

            while (strLine != null)
            {
                if (strLine != "")
                {
                    if (strLine.StartsWith("[" + posSection) && strLine.EndsWith("]"))
                    {
                        currentRoot = strLine.Substring(1, strLine.Length - 2);
                        ListSection.Add(currentRoot);
                    }
                }
                strLine = iniFile.ReadLine();
            }
            iniFile.Close();
            return ListSection;
        }
    }
}
