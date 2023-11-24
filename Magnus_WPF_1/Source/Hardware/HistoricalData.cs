using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace ReadResultAnalyzer
{
    class HistoricalData
    {
        private List<string> jpegList = new List<string>();
        private List<Point> posList = new List<Point>();

        public List<string> imagefileinfoList = new List<string>();

        public bool isDataOK { get; private set; }
        public string _date { get; private set; }
        public string _time { get; private set; }
        public string _out_data { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="historicalDataFileName">file name</param>
        public HistoricalData(string historicalDataFileName)
        {
            isDataOK = false;

            // Check file name
            string ext = Path.GetExtension(historicalDataFileName);
            if (ext != ".json")
            {
                return;
            }

            if (File.Exists(historicalDataFileName) != true)
            {
                return;
            }

            StreamReader st;
            try
            {
                st = new StreamReader(historicalDataFileName);
            }
            catch
            {
                return;
            }

            while (st.Peek() != -1)
            {
                string tmp_line = st.ReadLine();
                if (tmp_line.Contains("\"date\":"))
                {
                    _date = tmp_line.Replace("\"date\":\"", "");
                    _date = _date.Replace("\",", "");

                }
                else if (tmp_line.Contains("\"time\":"))
                {
                    _time = tmp_line.Replace("\"time\":\"", "");
                    _time = _time.Replace("\",", "");
                }
                else if (tmp_line.Contains("\"ftpimagefilename\":["))
                {
                    while (!tmp_line.Contains("]"))
                    {
                        // Read next line
                        tmp_line = st.ReadLine();

                        if (tmp_line.Contains(".JPG\""))
                        {
                            string work = tmp_line.Replace(",", "");
                            work = work.Replace("\"", "");
                            work = work.Replace("]", "");
                            jpegList.Add(work);
                        }
                    }
                }
                else if (tmp_line.Contains("\"outdata\":"))
                {
                    string work = tmp_line.Replace("\"outdata\":\"", "");
                    _out_data = work.Replace("\",", "");
                }
                else if (tmp_line.Contains("\"ftpimagefileinfo\":"))
                {
                    char[] char_sep = new char[] { ',' };
                    string work = tmp_line.Replace("\"ftpimagefileinfo\":[", "");
                    work = work.Replace("],", "");
                    string[] work_ret = work.Split(char_sep, StringSplitOptions.RemoveEmptyEntries);

                    for (int cnt = 0; cnt < work_ret.Count(); cnt++)
                    {
                        imagefileinfoList.Add(work_ret[cnt].Replace("\"", ""));
                    }
                }
                else if (tmp_line.Contains("\"codecorner\":"))
                {
                    // Read next line
                    tmp_line = st.ReadLine();

                    Point[] tmp_pos = GetCornerFromLine(tmp_line);
                    if (tmp_pos.Length != 0)
                    {
                        foreach (Point pos in tmp_pos)
                        {
                            posList.Add(pos);
                        }
                    }
                }
                else
                {
                    //do nothing
                }

            }

            st.Close();
            st.Dispose();

            isDataOK = true;
        }

        /// <summary>
        /// Get corner point from historical data file
        /// </summary>
        /// <returns>List of corner point</returns>
        public List<Point> GetCornerPointList()
        {
            return posList;
        }

        /// <summary>
        /// Get corner point from line
        /// </summary>
        /// <param name="tmp_str">1 line string</param>
        /// <returns>Corner Point</returns>
        private Point[] GetCornerFromLine(string tmp_str)
        {
            Point[] ret_pos = new Point[4];

            int[] pos_data = Regex.Matches(tmp_str, "[0-9]+").Cast<Match>().Select(m => int.Parse(m.Value)).ToArray();

            // Ignored if there are not 8 Point
            if (pos_data.Length == 8)
            {
                for (int cnt = 0; cnt < 4; cnt++)
                {
                    ret_pos[cnt] = new Point(pos_data[cnt * 2], pos_data[cnt * 2 + 1]);
                }
            }

            return ret_pos;
        }

        /// <summary>
        /// Get Jpeg file name from historical data file
        /// </summary>
        /// <returns>Jpeg file name</returns>
        public string GetJpegFileName()
        {
            string ret_str = string.Empty;

            // Find "Read OK"
            foreach (string str in jpegList)
            {
                if (str.Contains("_S_"))
                {
                    ret_str = str;
                    break;
                }
            }

            // If there is no "Read OK"
            if (ret_str == string.Empty)
            {
                if (jpegList.Count() != 0)
                {
                    ret_str = jpegList[0];
                }
                else
                {
                    ret_str = string.Empty;
                }
            }

            return ret_str;
        }

        /// <summary>
        /// Get out data maiking Byte String
        /// </summary>
        /// <param name="out data">Byte String</param>
        /// <returns>Readdata</returns>
        public string GetReaddataString(string bytestring)
        {
            int len = bytestring.Length / 2;
            int i, j;
            byte[] wk_num16 = new byte[len];

            j = 0;
            for (i = 0; i < len; i++)
            {
                wk_num16[i] = Convert.ToByte(bytestring.Substring(j, 2), 16);
                j += 2;
            }

            return Encoding.GetEncoding("shift_jis").GetString(wk_num16);
        }

    }
}
