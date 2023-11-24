using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace ReadResultAnalyzer
{
    class JpegOperate
    {
        public const string EXTENDED_STRING = "_drawn.JPG";
        public const int JPG_NOT_EXIST = -1;
        public const int NOT_JPG_FILE = -2;
        public const int NOT_MAKE_BMP = -3;
        public const int NOT_SAVE_FILE = -4;

        /// <summary>
        /// Draw rectangle and make new file
        /// </summary>
        /// <param name="org_filename">Jpeg file name</param>
        /// <param name="cornerList">List of corner point</param>
        /// <returns>0:OK / -1:NG</returns>
        public int DrawAndSave(string org_filename, List<Point> cornerList)
        {
            // Check file name
            string ext = Path.GetExtension(org_filename);
            if (ext != ".JPG")
            {
                return NOT_JPG_FILE;
            }

            if (File.Exists(org_filename) != true)
            {
                return JPG_NOT_EXIST;
            }

            Bitmap org_jpeg;
            try
            {
                org_jpeg = new Bitmap(org_filename);
                if (org_jpeg == null)
                {
                    return NOT_MAKE_BMP;
                }

            }
            catch (Exception)
            {
                return NOT_MAKE_BMP;
            }

            Bitmap new_jpeg = DrawRectangle(org_jpeg, cornerList);

            // Make new jpeg file
            try
            {
                new_jpeg.Save(org_filename + EXTENDED_STRING, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch
            {
                return NOT_SAVE_FILE;
            }
            finally
            {
                org_jpeg.Dispose();
                new_jpeg.Dispose();
            }

            return 0;
        }

        /// <summary>
        /// Draw rectangle
        /// </summary>
        /// <param name="org_image">Jpeg data</param>
        /// <param name="cnrList">List of corner point</param>
        /// <returns>Jpeg data with rectangle</returns>
        private Bitmap DrawRectangle(Bitmap org_image, List<Point> cnrList)
        {
            //rectangle color and width
            const int LINE_WIDTH = 12;
            Color lineColor = Color.Lime;

            Bitmap new_image = new Bitmap(org_image.Width, org_image.Height);
            Graphics graph = Graphics.FromImage(new_image);

            graph.DrawImage(org_image, new Point(0, 0));

            Pen pen = new Pen(lineColor, LINE_WIDTH);

            for (int cnt = 0; cnt < cnrList.Count(); cnt += 4)
            {
                graph.DrawPolygon(pen, cnrList.GetRange(cnt, 4).ToArray());
            }

            pen.Dispose();
            graph.Dispose();

            return new_image;
        }
    }
}
