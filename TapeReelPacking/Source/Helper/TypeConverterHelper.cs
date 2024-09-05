using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapeReelPacking.Source.Define;

namespace TapeReelPacking.Source.Helper
{
    public static class TypeConverterHelper
    {
        public static string ConvertRectanglesToString(Rectangles rectangles)
        {
            return string.Format("{0}:{1}:{2}:{3}", rectangles.TopLeft.X, rectangles.TopLeft.Y, rectangles.Width, rectangles.Height);
        }

        public static string ConvertListToString(List<int> list)
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
        public static List<int> ConverStringToList(string str)
        {
            List<int> list = new List<int>();
            string[] value = str.Split(':');
            foreach (string s in value)
            {
                list.Add(int.Parse(s));
            }
            return list;
        }

        public static Rectangles GetRectangles(string str)
        {
            if (str == "")
                return new Rectangles(0, 0, 0, 0);
            string[] value = str.Split(':');
            if (value.Length == 4)
                return new Rectangles(double.Parse(value[0]), double.Parse(value[1]), double.Parse(value[2]), double.Parse(value[3]));
            else
                return new Rectangles(double.Parse(value[0]), double.Parse(value[1]), double.Parse(value[2]), double.Parse(value[3]), double.Parse(value[4]));

        }

    }
}
