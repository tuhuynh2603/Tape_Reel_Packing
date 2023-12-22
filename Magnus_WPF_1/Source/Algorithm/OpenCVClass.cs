using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Magnus_WPF_1.Source.Define;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static Magnus_WPF_1.Source.Define.Rectangles;
using CvContourArray = Emgu.CV.Util.VectorOfVectorOfPoint;
using CvImage = Emgu.CV.Mat;
using CvPointArray = Emgu.CV.Util.VectorOfPoint;
using CvPointFArray = Emgu.CV.Util.VectorOfPointF;
using Line = Emgu.CV.Structure.LineSegment2D;
using LineArray = System.Collections.Generic.List<Emgu.CV.Structure.LineSegment2D>;

namespace Magnus_WPF_1.Source.Algorithm
{
    class MagnusOpenCVLib
    {



        //*******************************************************************************************************************************************************//
        // Internal Inspection Function
        //*******************************************************************************************************************************************************//
        #region Internnal Inspection Function
        //Get Corner From Region
        public static bool Different(CvImage regionSource, CvImage regionMask, ref CvImage regionOutput)
        {
            CvImage XOR_Region = new CvImage();
            regionOutput = new CvImage();
            CvInvoke.BitwiseXor(regionSource, regionMask, XOR_Region);
            CvInvoke.BitwiseAnd(XOR_Region, regionSource, regionOutput);
            return true;
        }

        public static int GetCornerFromRegion(ref CvImage region, List<Rectangle> corner, Size imgSize)
        {
            if (region.IsEmpty)
            {
                corner.Add(new Rectangle(0, 0, 0, 0));
                return -1;
            }
            CvImage result = new CvImage();
            CvContourArray contourArray = new CvContourArray();
            CvContourArray contourArrayHull = new CvContourArray();
            CvPointArray PointArray = new CvPointArray();
            Rectangle rect = new Rectangle();

            ExtractExternalContours(ref region, ref result, ref contourArray);
            ConvexHull(contourArray, ref contourArrayHull, ref result, imgSize);
            PointArray = contourArrayHull[0];
            rect = CvInvoke.BoundingRectangle(PointArray);
            corner.Add(rect);
            return 0;
        }

        //Rect To Points
        public static bool Rect1ToPoints(ref Rectangle Rect, CvPointFArray result)
        {
            PointF[] point = new PointF[4];
            point[0] = new PointF(Rect.X, Rect.Y);
            point[1] = new PointF(Rect.X + Rect.Width, Rect.Y);
            point[2] = new PointF(Rect.X + Rect.Width, Rect.Y + Rect.Height);
            point[3] = new PointF(Rect.X, Rect.Y + Rect.Height);
            result.Push(point);
            return true;
        }

        //Get Region Points
        public static int GetArea(ref CvImage source)
        {
            Matrix<byte> matrix = new Matrix<byte>(source.Rows, source.Cols, source.NumberOfChannels);

            source.CopyTo(matrix);
            return (int)matrix.Sum / 255;
        }
        public static bool GetRegionPoints(ref CvImage source, List<int> Rows, List<int> Cols)
        {
            Matrix<byte> matrix = new Matrix<byte>(source.Rows, source.Cols, source.NumberOfChannels);

            source.CopyTo(matrix);
            int nsize = (int)(matrix.Sum / 255);
            for (int i = 0; i < source.Rows - 1; i++)
            {
                for (int j = 0; j < source.Cols - 1; j++)
                {
                    byte a = matrix.Data[i, j];
                    if (a == 255)
                    {
                        Rows.Add(i);
                        Cols.Add(j);
                    }
                }

            }
            return true;
        }

        // Smallest Rectangle1
        public static bool SmallestRectangle1(ref CvImage source, List<int> outputRect)
        {
            List<int> Rows = new List<int>();
            List<int> Cols = new List<int>();

            GetRegionPoints(ref source, Rows, Cols);

            int top, bottom, left, right, isPass;
            isPass = 1;
            if (Rows.Count > 0)
            {
                top = Rows.Min();
                bottom = Rows.Max();
                left = Cols.Min();
                right = Cols.Max();

                outputRect.Add(top);
                outputRect.Add(bottom);
                outputRect.Add(left);
                outputRect.Add(right);
                outputRect.Add(isPass);

            }
            else
            {
                left = 0;
                right = 0;
                top = 0;
                bottom = 0;
                isPass = 0;

                outputRect.Add(top);
                outputRect.Add(bottom);
                outputRect.Add(left);
                outputRect.Add(right);
                outputRect.Add(isPass);
            }
            return true;
        }

        public static void GetWidthHeightRegion(ref CvImage source, ref int nWidth, ref int nHeight)
        {
            List<int> Rows = new List<int>();
            List<int> Cols = new List<int>();

            GetRegionPoints(ref source, Rows, Cols);
            if (Rows.Count > 0)
            {

                nWidth = Cols.Max() - Cols.Min();
                nHeight = Rows.Max() - Rows.Min();

            }
            else
            {
                nWidth = nHeight = 0;
            }
            return;
        }
        // Connection
        public static bool Connection(ref CvImage source, ref CvImage connection, ref int numberOfConnection, ref CvImage stats, ref CvImage centroids)
        {
            connection = new CvImage(source.Size, DepthType.Cv32S, 1);
            numberOfConnection = CvInvoke.ConnectedComponentsWithStats(source, connection, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            //Don't count background as label
            numberOfConnection = numberOfConnection - 1;
            return true;
        }

        // Count Number of Objects
        public static int CountObj(CvImage region)
        {
            CvImage result = new CvImage();
            List<Rectangle> rectCheck = new List<Rectangle>();
            SelectRegionByArea(ref region, ref result, ref rectCheck, 1, 999999999, (int)OPERATOR.AND);
            return (int)rectCheck.Count();
        }

        //Select Region
        public static bool SelectRegionBySize(ref CvImage source, ref CvImage result, ref List<Rectangle> rectArray, int minWidth, int minHeight, List<double> List_WidthOfDefect, List<double> List_HeightOfDefet, int isOperator)
        {
            int offset = 5;
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            result = CvImage.Zeros(source.Height, source.Width, DepthType.Cv8U, 1);
            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);
            for (int i = 1; i < nLabels; i++)
            {
                if (isOperator == 1)
                {
                    //bOperator == True: OR operation
                    if ((statsData[i * stats.Cols + 3]) > minHeight || (statsData[i * stats.Cols + 2]) > minWidth)
                    {
                        List_WidthOfDefect.Add(statsData[i * stats.Cols + 2]);
                        List_HeightOfDefet.Add(statsData[i * stats.Cols + 3]);
                        var x = statsData[i * stats.Cols + 0];
                        var y = statsData[i * stats.Cols + 1];
                        var width = statsData[i * stats.Cols + 2];
                        var height = statsData[i * stats.Cols + 3];
                        CvImage compareMat = new CvImage();
                        CvImage interestedLabelIndexMat = labelImage.Clone();
                        interestedLabelIndexMat.SetTo(new MCvScalar(i));
                        CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                        result = result | compareMat;
                        rectArray.Add(new Rectangle(x - offset, y - offset, width + 2 * offset, height + 2 * offset));

                    }
                }
                else
                {
                    //bOperator == False: AND operation
                    if ((statsData[i * stats.Cols + 3]) > minHeight && (statsData[i * stats.Cols + 2]) > minWidth)
                    {
                        List_WidthOfDefect.Add(statsData[i * stats.Cols + 2]);
                        List_HeightOfDefet.Add(statsData[i * stats.Cols + 3]);
                        var x = statsData[i * stats.Cols + 0];
                        var y = statsData[i * stats.Cols + 1];
                        var width = statsData[i * stats.Cols + 2];
                        var height = statsData[i * stats.Cols + 3];
                        CvImage compareMat = new CvImage();
                        CvImage interestedLabelIndexMat = labelImage.Clone();
                        interestedLabelIndexMat.SetTo(new MCvScalar(i));
                        CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                        result = result | compareMat;
                        rectArray.Add(new Rectangle(x - offset, y - offset, width + 2 * offset, height + 2 * offset));
                    }
                }
            }


            return true;
        }
        public static bool SelectRegionByRect2(ref CvImage source, ref CvImage result, ref List<Rectangle> rectArray, int minWidth, int minHeight, List<double> List_WidthOfDefect, List<double> List_HeightOfDefet, int isOperator)
        {
            int offset = 5;
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            result = CvImage.Zeros(source.Height, source.Width, DepthType.Cv8U, 1);
            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);
            for (int i = 1; i < nLabels; i++)
            {
                if (isOperator == 1)
                {
                    //bOperator == True: OR operation
                    CvImage compareMat = new CvImage();
                    CvImage interestedLabelIndexMat = labelImage.Clone();
                    interestedLabelIndexMat.SetTo(new MCvScalar(i));
                    CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                    CvContourArray contours = new CvContourArray();
                    GenContourRegion(ref compareMat, ref contours, RetrType.External);
                    RotatedRect rect2 = CvInvoke.MinAreaRect(contours[0]);

                    if ((rect2.Size.Height > minHeight) || (rect2.Size.Width > minWidth))
                    {
                        List_WidthOfDefect.Add(statsData[i * stats.Cols + 2]);
                        List_HeightOfDefet.Add(statsData[i * stats.Cols + 3]);
                        var x = statsData[i * stats.Cols + 0];
                        var y = statsData[i * stats.Cols + 1];
                        var width = statsData[i * stats.Cols + 2];
                        var height = statsData[i * stats.Cols + 3];

                        result = result | compareMat;
                        rectArray.Add(new Rectangle(x - offset, y - offset, width + 2 * offset, height + 2 * offset));

                    }
                }
                else
                {
                    //bOperator == False: AND operation
                    CvImage compareMat = new CvImage();
                    CvImage interestedLabelIndexMat = labelImage.Clone();
                    interestedLabelIndexMat.SetTo(new MCvScalar(i));
                    CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                    CvContourArray contours = new CvContourArray();
                    GenContourRegion(ref compareMat, ref contours, RetrType.External);
                    RotatedRect rect2 = CvInvoke.MinAreaRect(contours[0]);
                    if ((rect2.Size.Height > minHeight) & (rect2.Size.Width > minWidth))
                    {
                        List_WidthOfDefect.Add(statsData[i * stats.Cols + 2]);
                        List_HeightOfDefet.Add(statsData[i * stats.Cols + 3]);
                        var x = statsData[i * stats.Cols + 0];
                        var y = statsData[i * stats.Cols + 1];
                        var width = statsData[i * stats.Cols + 2];
                        var height = statsData[i * stats.Cols + 3];
                        result = result | compareMat;
                        rectArray.Add(new Rectangle(x - offset, y - offset, width + 2 * offset, height + 2 * offset));
                    }
                }
            }
            return true;
        }

        public static bool SelectRegionBySizeMinMax(ref CvImage source, ref CvImage result, ref List<Rectangle> rectArray, int minValue, int maxValue, ref List<double> List_SizeOut, int methodSize)
        {

            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            result = CvImage.Zeros(source.Height, source.Width, DepthType.Cv8U, 1);
            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);
            for (int i = 1; i < nLabels; i++)
            {
                if (methodSize == (int)SIZE.WIDTH)
                {
                    // + 3 Height
                    // + 2 Width
                    if ((statsData[i * stats.Cols + 2]) > minValue && (statsData[i * stats.Cols + 2]) < maxValue)
                    {
                        List_SizeOut.Add(statsData[i * stats.Cols + 2]);

                        var x = statsData[i * stats.Cols + 0];
                        var y = statsData[i * stats.Cols + 1];
                        var width = statsData[i * stats.Cols + 2];
                        var height = statsData[i * stats.Cols + 3];
                        CvImage compareMat = new CvImage();
                        CvImage interestedLabelIndexMat = labelImage.Clone();
                        interestedLabelIndexMat.SetTo(new MCvScalar(i));
                        CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                        result = result | compareMat;
                        rectArray.Add(new Rectangle(x, y, width, height));
                    }
                }
                else if (methodSize == (int)SIZE.HEIGHT)
                {
                    if ((statsData[i * stats.Cols + 3]) > minValue && (statsData[i * stats.Cols + 3]) < maxValue)
                    {
                        List_SizeOut.Add(statsData[i * stats.Cols + 3]);
                        var x = statsData[i * stats.Cols + 0];
                        var y = statsData[i * stats.Cols + 1];
                        var width = statsData[i * stats.Cols + 2];
                        var height = statsData[i * stats.Cols + 3];
                        CvImage compareMat = new CvImage();
                        CvImage interestedLabelIndexMat = labelImage.Clone();
                        interestedLabelIndexMat.SetTo(new MCvScalar(i));
                        CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                        result = result | compareMat;
                        rectArray.Add(new Rectangle(x, y, width, height));
                    }
                }
            }
            return true;
        }
        // Select Region By Area
        public static bool SelectRegionByArea(ref CvImage source, ref CvImage result, ref List<Rectangle> rectArray, int minArea, int maxArea, int isOperator)
        {
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();

            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            result = CvImage.Zeros(source.Height, source.Width, DepthType.Cv8U, 1);

            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);

            for (int i = 1; i < nLabels; i++)
            {
                if (isOperator == 1)
                {
                    //bOperator == True: OR operation
                    if ((statsData[i * stats.Cols + 4]) > minArea || (statsData[i * stats.Cols + 4]) < maxArea)
                    {
                        var x = statsData[i * stats.Cols + 0];
                        var y = statsData[i * stats.Cols + 1];
                        var width = statsData[i * stats.Cols + 2];
                        var height = statsData[i * stats.Cols + 3];
                        var area = statsData[i * stats.Cols + 4];
                        CvImage compareMat = new CvImage();
                        CvImage interestedLabelIndexMat = labelImage.Clone();
                        interestedLabelIndexMat.SetTo(new MCvScalar(i));

                        CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                        result = result | compareMat;

                        rectArray.Add(new Rectangle(x, y, width, height));
                    }
                }
                else
                {
                    //bOperator == False: AND operation
                    if ((statsData[i * stats.Cols + 4]) > minArea && (statsData[i * stats.Cols + 4]) < maxArea)
                    {
                        var x = statsData[i * stats.Cols + 0];
                        var y = statsData[i * stats.Cols + 1];
                        var width = statsData[i * stats.Cols + 2];
                        var height = statsData[i * stats.Cols + 3];
                        var area = statsData[i * stats.Cols + 4];

                        CvImage compareMat = new CvImage();
                        CvImage interestedLabelIndexMat = labelImage.Clone();
                        interestedLabelIndexMat.SetTo(new MCvScalar(i));

                        CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                        result = result | compareMat;

                        rectArray.Add(new Rectangle(x, y, width, height));
                    }
                }
            }
            return true;
        }

        // Select the Biggest Region
        public static bool SelectBiggestRegion(ref CvImage source, ref CvImage result)
        {
            int nLabels;
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            //Stopwatch timeIns = new Stopwatch();
            //timeIns.Start();
            nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            //LogMessage.LogMessage.WriteToDebugViewer(5, $"Connection time : {timeIns.ElapsedMilliseconds} ms");
            //timeIns.Restart();
            int maxAreaIndexLabel = -1;
            int maxArea = 0;

            CCStatsOp[] statsOp = new CCStatsOp[stats.Rows];

            stats.CopyTo(statsOp);
            result = CvImage.Zeros(source.Rows, source.Cols, DepthType.Cv8U, 1);

            for (int i = 1; i < nLabels; i++)
                if (statsOp[i].Area > maxArea)
                {
                    maxAreaIndexLabel = i;
                    maxArea = statsOp[i].Area;
                }
            if (maxAreaIndexLabel == -1)
                return false;

            //LogMessage.LogMessage.WriteToDebugViewer(5, $"Get Max Area time : {timeIns.ElapsedMilliseconds} ms");
            //timeIns.Restart();

            //Get size result equal size of source
            CvImage imgtest = new CvImage(source.Size, DepthType.Cv32S, 1);
            imgtest.SetTo(new MCvScalar(maxAreaIndexLabel));
            CvInvoke.Compare(labelImage, imgtest, result, CmpType.Equal);
            //LogMessage.LogMessage.WriteToDebugViewer(5, $"End selectbiggest time : {timeIns.ElapsedMilliseconds} ms");
            //timeIns.Restart();
            return true;
        }
        // select region  by width height 
        public static bool SelectRegion(ref CvImage source, ref CvImage result, ref List<Rectangle> rectArray, int minWidth, int minHeight, int maxWidth = 9999999, int maxHeight = 999999)
        {
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            result = CvImage.Zeros(source.Height, source.Width, DepthType.Cv8U, 1);
            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);
            for (int i = 1; i < nLabels; i++)
            {
                if ((statsData[i * stats.Cols + 3]) > minHeight && (statsData[i * stats.Cols + 3]) < maxHeight && (statsData[i * stats.Cols + 2]) > minWidth && (statsData[i * stats.Cols + 2]) < maxWidth)
                {

                    var x = statsData[i * stats.Cols + 0];
                    var y = statsData[i * stats.Cols + 1];
                    var width = statsData[i * stats.Cols + 2];
                    var height = statsData[i * stats.Cols + 3];
                    CvImage compareMat = new CvImage();
                    CvImage interestedLabelIndexMat = labelImage.Clone();
                    interestedLabelIndexMat.SetTo(new MCvScalar(i));
                    CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                    result = result | compareMat;
                    rectArray.Add(new Rectangle(x, y, width, height));
                }


            }
            return true;
        }

        // Select obj (Select region by index)
        public static bool SelectObj(ref CvImage connection, ref CvImage selectedRegion, int index)
        {
            if (selectedRegion.IsEmpty)
                selectedRegion = CvImage.Zeros(connection.Rows, connection.Cols, DepthType.Cv8U, 1);

            CvImage compareMat = new CvImage();
            CvImage connectionIndexMat = connection.Clone();
            connectionIndexMat.SetTo(new MCvScalar(index));

            CvInvoke.Compare(connection, connectionIndexMat, compareMat, CmpType.Equal);
            selectedRegion = selectedRegion | compareMat;
            return true;
        }

        // Rotate Image 
        public static bool RotateImage(CvImage source, ref CvImage result, float angleRotate)
        {
            CvImage rotatedImage = new CvImage();
            CvImage mapMatrix = new CvImage();
            CvInvoke.GetRotationMatrix2D(new PointF(source.Size.Width / 2.0f, source.Size.Height / 2.0f),
                        angleRotate, 1.0, mapMatrix);

            System.Drawing.Size szTemplateSize = new System.Drawing.Size(source.Cols, source.Rows);

            CvInvoke.WarpAffine(source, rotatedImage, mapMatrix, szTemplateSize);
            result = rotatedImage;
            return true;
        }
        public static CvImage RotateShiftImage(ref CvImage source,PointF centerPoint, float angleRotate, float fShiftX, float fShiftY)
        {
            CvImage rotatedImage = new CvImage();
            CvImage mapMatrix = new CvImage();
            CvInvoke.GetRotationMatrix2D(centerPoint,
                        angleRotate, 1.0, mapMatrix);
            float fX = (float)mapMatrix.GetValue(0, 2);
            fX += (float)fShiftX;

            float fY = (float) mapMatrix.GetValue(1, 2);
            fY  += (float)fShiftY;

            mapMatrix.SetValue(0, 2, fX);
            mapMatrix.SetValue(1, 2, fY);
            System.Drawing.Size szTemplateSize = new System.Drawing.Size(source.Cols, source.Rows);
            CvInvoke.WarpAffine(source, rotatedImage, mapMatrix, szTemplateSize);
            return rotatedImage;
        }
        // Shift Image
        public static bool ShiftImage(CvImage source, ref CvImage result, ref PointF[] srcPnt, ref PointF[] dstPnt)
        {
            CvImage shiftedImage = new CvImage();
            CvImage mapMatrix = new CvImage();

            //PointF[] pt1 = new PointF[1];
            //PointF[] pt2 = new PointF[1];

            //pt1[0] = new PointF(srcPnt.X, srcPnt.Y);
            //pt2[0] = new PointF(dstPnt.X, dstPnt.Y);

            mapMatrix = CvInvoke.GetAffineTransform(srcPnt, dstPnt);
            //mapMatrix.SetValue(0, 1, 1);
            //mapMatrix.SetValue(0, 2, 100);
            //mapMatrix.SetValue(1, 1, 1);
            //mapMatrix.SetValue(1, 2, 100);
            System.Drawing.Size szTemplateSize = new System.Drawing.Size(source.Cols, source.Rows);

            CvInvoke.WarpAffine(source, shiftedImage, mapMatrix, szTemplateSize);
            result = shiftedImage;
            return true;
        }
        // Select Region By Index
        public static bool SelectRegionByIndex(ref CvImage connection, ref CvImage selectedRegion, int index)
        {
            if (selectedRegion.IsEmpty)
                selectedRegion = CvImage.Zeros(connection.Rows, connection.Cols, DepthType.Cv8U, 1);

            CvImage compareMat = new CvImage();
            CvImage connectionIndexMat = connection.Clone();
            connectionIndexMat.SetTo(new MCvScalar(index));

            CvInvoke.Compare(connection, connectionIndexMat, compareMat, CmpType.Equal);
            selectedRegion = selectedRegion | compareMat;
            return true;
        }

        public static int SelectPointBased_Top_Left_Bottom_Right(ref List<PointF> source, ref PointF result, int position)
        {
            result = new PointF(0, 0);
            if (source.Count < 1)
                return -1;

            int nIndex = 0;
            for (int i = 1; i < source.Count; i++)
            {
                switch (position)
                {
                    case (int)POSITION._LEFT:
                        if (source[i].X <= source[nIndex].X)
                        {
                            nIndex = i;
                        }
                        break;
                    case (int)POSITION._RIGHT:
                        if (source[i].X >= source[nIndex].X)
                        {
                            nIndex = i;
                        }
                        break;
                    case (int)POSITION._TOP:
                        if (source[i].Y <= source[nIndex].Y)
                        {
                            nIndex = i;
                        }
                        break;
                    case (int)POSITION._BOTTOM:
                        if (source[i].Y >= source[nIndex].Y)
                        {
                            nIndex = i;
                        }
                        break;
                }
            }
            result = source[nIndex];
            return nIndex;
        }

        // Select Region Based Position
        public static bool SelectRegionBasedPosition(ref CvImage source, ref CvImage result, int position)
        {
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);

            PointF centrePoint = new PointF(0, 0);
            for (int i = 1; i < nLabels; i++)
            {
                centrePoint.X += MatExtension.GetValue(centroids, i, 0);
                centrePoint.Y += MatExtension.GetValue(centroids, i, 1);
            }

            centrePoint.X /= (nLabels - 1);
            centrePoint.Y /= (nLabels - 1);

            int nInterestedLabelIndex = -1;

            for (int i = 1; i < nLabels; i++)
            {
                PointF labelCentroid = new PointF((float)MatExtension.GetValue(centroids, i, 0), (float)MatExtension.GetValue(centroids, i, 1));
                switch (position)
                {
                    case (int)POSITION.TOP_LEFT:
                        if ((labelCentroid.X <= centrePoint.X) && (labelCentroid.Y <= centrePoint.Y))
                        {
                            nInterestedLabelIndex = i;
                        }
                        break;
                    case (int)POSITION.TOP_RIGHT:
                        if ((labelCentroid.X >= centrePoint.X) && (labelCentroid.Y <= centrePoint.Y))
                        {
                            nInterestedLabelIndex = i;
                        }
                        break;
                    case (int)POSITION.BOTTOM_RIGHT:
                        if ((labelCentroid.X >= centrePoint.X) && (labelCentroid.Y >= centrePoint.Y))
                        {
                            nInterestedLabelIndex = i;
                        }
                        break;
                    case (int)POSITION.BOTTOM_LEFT:
                        if ((labelCentroid.X <= centrePoint.X) && (labelCentroid.Y >= centrePoint.Y))
                        {
                            nInterestedLabelIndex = i;
                        }
                        break;
                }
            }

            if (nInterestedLabelIndex == -1)
                return false;
            CvImage compareMat = new CvImage();
            CvImage interestedLabelIndexMat = labelImage.Clone();
            interestedLabelIndexMat.SetTo(new MCvScalar(nInterestedLabelIndex));

            CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
            result = CvImage.Zeros(source.Rows, source.Cols, DepthType.Cv8U, 1);
            result = result | compareMat;
            return true;
        }
        // SelectRegionBasedPosition2 (Nhan)
        public static bool SelectRegionBasedPosition2(ref CvImage source, ref CvImage result, int position)
        {
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);

            MCvPoint2D64f[] centroidPoints = new MCvPoint2D64f[nLabels];
            centroids.CopyTo(centroidPoints);

            int nInterestedLabelIndex = -1;

            PointF centrePoint = new PointF(0, 0);

            double maxRow = 0;

            for (int i = 1; i < nLabels; i++)
            {
                if (maxRow < centroidPoints[i].Y)
                {
                    nInterestedLabelIndex = i;
                    maxRow = centroidPoints[i].Y;
                }
            }

            if (nInterestedLabelIndex == -1)
                return false;
            CvImage compareMat = new CvImage();
            CvImage interestedLabelIndexMat = labelImage.Clone();
            interestedLabelIndexMat.SetTo(new MCvScalar(nInterestedLabelIndex));

            CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
            result = CvImage.Zeros(source.Rows, source.Cols, DepthType.Cv8U, 1);
            result = result | compareMat;
            return true;
        }

        // Select most bottom object with size limitation
        public static bool SelectBottomObjectWithSize(ref CvImage source, ref CvImage result, ref CvImage TopSelectedRegion, int minValue, int maxValue, int methodSize)
        {
            result = CvImage.Zeros(source.Rows, source.Cols, DepthType.Cv8U, 1);

            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);

            MCvPoint2D64f[] centroidPoints = new MCvPoint2D64f[nLabels];
            centroids.CopyTo(centroidPoints);

            int nInterestedLabelIndex = -1;
            double maxRow = 0;

            for (int i = 1; i < nLabels; i++)
            {
                if (methodSize == (int)SIZE.WIDTH)
                {
                    // + 3 Height
                    // + 2 Width
                    if ((statsData[i * stats.Cols + 2]) > minValue && (statsData[i * stats.Cols + 2]) < maxValue)
                    {
                        if (maxRow < centroidPoints[i].Y)
                        {
                            nInterestedLabelIndex = i;
                            maxRow = centroidPoints[i].Y;
                        }
                    }
                }
                else if (methodSize == (int)SIZE.HEIGHT)
                {
                    if ((statsData[i * stats.Cols + 3]) > minValue && (statsData[i * stats.Cols + 3]) < maxValue)
                    {
                        if (maxRow < centroidPoints[i].Y)
                        {
                            nInterestedLabelIndex = i;
                            maxRow = centroidPoints[i].Y;
                        }
                    }
                }
            }

            if (nInterestedLabelIndex == -1)
                return false;
            CvImage compareMat = new CvImage();
            CvImage interestedLabelIndexMat = labelImage.Clone();
            interestedLabelIndexMat.SetTo(new MCvScalar(nInterestedLabelIndex));

            CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);

            // find top white region //
            MagnusOpenCVLib.SubRegion(ref source, ref compareMat, ref TopSelectedRegion);

            result = result | compareMat;
            return true;
        }

        // Binary Thresholding
        public static bool BinaryThresholding(ref CvImage source, ref CvImage result, int color, ref double usedThreshold, int offset, ref CvImage mask)
        {
            usedThreshold = CvInvoke.Threshold(source, result, 0, 255, ((color == (int)OBJECT_COLOR.BLACK) ? ThresholdType.BinaryInv : ThresholdType.Binary) | ThresholdType.Otsu);
            if (offset > 0)
                CvInvoke.Threshold(source, result, usedThreshold + (color == (int)OBJECT_COLOR.BLACK ? 1 : -1) * offset,
                          255, (color == (int)OBJECT_COLOR.BLACK) ? ThresholdType.BinaryInv : ThresholdType.Binary);
            CvInvoke.BitwiseAnd(result, mask, result);
            return true;
        }

        // Var Thresholding
        public static bool VarThresholding(ref CvImage source, ref CvImage result, int color, int blockSize, ref CvImage mask, double offset = 0)
        {
            CvInvoke.AdaptiveThreshold(source, result, 255, AdaptiveThresholdType.MeanC, (color == (int)OBJECT_COLOR.BLACK) ? ThresholdType.BinaryInv : ThresholdType.BinaryInv, blockSize, offset * ((color == (int)OBJECT_COLOR.BLACK) ? 1 : -1));
            CvInvoke.BitwiseAnd(result, mask, result);
            return true;
        }

        // Threshold2 for Gray image
        public static bool Threshold2(ref CvImage imgGray, ref CvImage imgThreshold, int minThreshold, int maxThreshold)
        {
            CvImage lowerThreshMat = new CvImage();
            CvImage upperThreshMat = new CvImage();
            Mat result = new Mat();
            //CvInvoke.Add(imgGray, new UMat(imgGray.Size, DepthType.Cv8U, 1), result, null);
            int nMin = minThreshold;
            if (minThreshold == 0)
                nMin = - 1;

            CvInvoke.Threshold(imgGray, lowerThreshMat, nMin, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
            CvInvoke.Threshold(imgGray, upperThreshMat, (double)maxThreshold, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            CvInvoke.BitwiseAnd(lowerThreshMat, upperThreshMat, imgThreshold);
            return true;
        }

        // Threshold2 for HSV image
        public static bool Threshold2(ref CvImage imgHsv, ref CvImage imgThreshold, List<int> minThreshold, List<int> maxThreshold)
        {
            ScalarArray _minThreshold = new ScalarArray(new MCvScalar(minThreshold[0], minThreshold[1], minThreshold[2]));
            ScalarArray _maxThreshold = new ScalarArray(new MCvScalar(maxThreshold[0], maxThreshold[1], maxThreshold[2]));
            CvInvoke.InRange(imgHsv, _minThreshold, _maxThreshold, imgThreshold);
            return true;
        }

        // Find the ExternalContours
        public static bool GenContourRegion(ref CvImage source, ref CvContourArray contours, RetrType mode)
        {
            Mat hier = new Mat();
            CvInvoke.FindContours(source, contours, hier, mode, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            return true;
        }

        // Convex Hull
        public static bool ConvexHull(CvContourArray contours, ref CvContourArray hulls, ref CvImage result, Size imgSize)
        {
            if (contours.Size == 0)
                return false;

            hulls.Equals(contours);

            List<PointF> ls = new List<PointF>();
            List<PointF[]> lss = new List<PointF[]>();
            foreach (var array in contours.ToArrayOfArray())
            {
                ls.Clear();
                foreach (var point in array)
                {
                    PointF center = new PointF((float)point.X, (float)point.Y);
                    ls.Add(center);
                }
                PointF[] hull = CvInvoke.ConvexHull(ls.ToArray(), true);
                lss.Add(hull);
            }
            List<Point[]> pts = new List<Point[]>();
            foreach (var arrayF in lss)
            {
                pts.Add(Array.ConvertAll<PointF, Point>(arrayF, Point.Round));
            }
            CvContourArray HullArray = new CvContourArray(pts.ToArray());
            hulls = HullArray;
            MCvScalar sColor = new MCvScalar(255);

            DrawContours(ref result, ref hulls, imgSize, sColor, -1, 0);
            return true;
        }

        // Fill up region
        public static bool FillUp(ref CvImage source, ref CvImage result)
        {
            CvContourArray contours = new CvContourArray();
            result = CvImage.Zeros(source.Rows, source.Cols, DepthType.Cv8U, 1);
            MCvScalar sColor = new MCvScalar(255);
            GenContourRegion(ref source, ref contours, RetrType.External);
            DrawContours(ref result, ref contours, source.Size, sColor, -1, -1);
            return true;
        }

        public static bool FillUpSmallShapes(ref CvImage source, ref CvImage fillUpRegions, int nMinSize = 0, int nMaxSize = -1)
        {
            CvContourArray contours = new CvContourArray();
            CvImage fillUpRegionsTemp = CvImage.Zeros(source.Rows, source.Cols, DepthType.Cv8U, 1);
            MCvScalar sColor = new MCvScalar(255);
            GenContourRegion(ref source, ref contours, RetrType.External);
            DrawContours(ref fillUpRegionsTemp, ref contours, source.Size, sColor, -1, -1);
            CvImage holeRegions = new CvImage();
            CvInvoke.BitwiseXor(fillUpRegionsTemp, source, holeRegions);
            CvImage selectRegions = new CvImage();
            CvImage closingRegions = new CvImage();
            CvImage regionFilterMaxWidth = new CvImage();
            CvImage regionFilterMinWidth = new CvImage();

            if (nMaxSize > 0)
            {
                ClosingCircle(ref source, ref closingRegions, nMaxSize);
                CvInvoke.BitwiseAnd(holeRegions, closingRegions, regionFilterMaxWidth);
            }
            else
                regionFilterMaxWidth = holeRegions.Clone();

            if (nMinSize > 0)
            {
                OpeningCircle(ref holeRegions, ref regionFilterMinWidth, nMinSize);
            }
            else
                regionFilterMinWidth = holeRegions.Clone();

            CvInvoke.BitwiseAnd(regionFilterMinWidth, regionFilterMaxWidth, selectRegions);


            //CvPointArray point_regions = new CvPointArray();
            //List<Rectangle> rectArray = new List<Rectangle>();
            //SelectRegion(ref holeRegions, ref selectRegions, ref rectArray, nMinWidth, nMinHeight, nMaxWidth, nMaxHeight);
            CvInvoke.BitwiseOr(source, selectRegions, fillUpRegions);

            return true;
        }


        // Extract External Contours
        public static bool ExtractExternalContours(ref CvImage source, ref CvImage result, ref CvContourArray contours, int thickness = 1)
        {
            if (source == null)
                return false;
            MCvScalar sColor = new MCvScalar(255);
            result = CvImage.Zeros(source.Rows, source.Cols, DepthType.Cv8U, 1);
            GenContourRegion(ref source, ref contours, RetrType.External);
            DrawContours(ref result, ref contours, source.Size, sColor, thickness, -1);
            return true;
        }
        // Sobel Edge Detection
        public static int SobelEdgeDetection(ref CvImage source, ref CvImage edgeMat, ref CvImage edge, int contrast, int edgePosition, int direction,
                       ref CvImage region, bool isEliminateEdge = false, int size = 7,
                       bool isFilterHighEdge = false, int highContrast = 255)
        {
            //CvImage edgeMat = new CvImage();
            CvImage eliminateEdge = new CvImage();

            CvImage filteredImage = new CvImage();
            filteredImage = source;
            //Filters.GaussFilter(ref source, ref filteredImage, 3);
            //filteredImage = source;
            if (direction == (int)(DIRECTION.X))
            {
                CvInvoke.Sobel(filteredImage, edgeMat, DepthType.Cv16S, 1, 0, 3, 1, 0, BorderType.Default);
                edgeMat = edgeMat / 4;

            }
            else if (direction == (int)(DIRECTION.Y))
            {
                CvInvoke.Sobel(filteredImage, edgeMat, DepthType.Cv16S, 0, 1, 3, 1, 0, BorderType.Default);
                edgeMat = edgeMat / 4;

            }
            else
            {

                CvImage xMat = new CvImage();
                CvImage yMat = new CvImage();
                CvImage scaledXMat = new CvImage();
                CvImage scaledYMat = new CvImage();

                CvInvoke.Sobel(filteredImage, xMat, DepthType.Cv32F, 1, 0, 3, 1, 0, BorderType.Default);
                CvInvoke.ConvertScaleAbs(xMat, scaledXMat, 1, 0);
                CvInvoke.Sobel(filteredImage, yMat, DepthType.Cv32F, 0, 1, 3, 1, 0, BorderType.Default);
                CvInvoke.ConvertScaleAbs(yMat, scaledYMat, 1, 0);
                CvInvoke.AddWeighted(scaledXMat, 0.25, scaledYMat, 0.25, 0, edgeMat);
                edgeMat.ConvertTo(edgeMat, DepthType.Cv8U);
            }

            edge = new CvImage();
            if (edgePosition == (int)EDGE_POSITION.POSITIVE)
            {
                CvInvoke.Threshold(edgeMat, edge, contrast, 255, ThresholdType.Binary);
            }
            else if (edgePosition == (int)EDGE_POSITION.NEGATIVE)
                CvInvoke.Threshold(edgeMat, edge, -contrast, 255, ThresholdType.BinaryInv);
            edge.ConvertTo(edge, DepthType.Cv8U);

            if (isFilterHighEdge)
            {
                //Eliminate edges on edge of image

                if (edgePosition == (int)EDGE_POSITION.POSITIVE)
                {
                    //edge = new CvImage();
                    CvInvoke.Threshold(edgeMat, eliminateEdge, highContrast, 255, ThresholdType.BinaryInv);
                }
                else if (edgePosition == (int)EDGE_POSITION.NEGATIVE)
                    CvInvoke.Threshold(edgeMat, eliminateEdge, -highContrast, 255, ThresholdType.Binary);

                eliminateEdge.ConvertTo(eliminateEdge, DepthType.Cv8U);
                CvInvoke.BitwiseAnd(edge, eliminateEdge, edge);
            }

            if (isEliminateEdge)
            {
                CvImage erodeRegion = new CvImage();
                ErosionCircle(ref region, ref erodeRegion, size);
                CvInvoke.BitwiseAnd(edge, erodeRegion, edge);
            }
            return 0;
        }

        // Hough Lines
        public static bool HoughLines(ref CvImage source, ref CvImage result, int houghCoeff, ref LineArray edges)
        {
            //MagnusOpenCVLib.ClosingRectangle(ref source, ref source, 5, 3);
            CvPointFArray lines = new CvPointFArray();
            CvInvoke.HoughLines(source, lines, 1, Math.PI / 1800,
                       houghCoeff, 0, 0);

            if (lines.Size == 0)
                return false;

            edges = new LineArray(lines.Size);
            float rho = 0, theta = 0;
            for (int i = 0; i < lines.Size; i++)
            {
                rho = lines[i].X;
                theta = lines[i].Y;

                DrawHoughLines(ref result, rho, theta, source.Size);

                Point pt1 = new Point();
                Point pt2 = new Point();
                ExtendHoughLine(source, rho, theta, ref pt1, ref pt2);
                edges.Add(new Line(pt1, pt2));
            }
            return true;
        }

        // Extend Hough Line
        public static bool ExtendHoughLine(CvImage source, float rho, float theta, ref Point pt1, ref Point pt2)
        {
            double a = Math.Cos(theta), b = Math.Sin(theta);
            double x0 = a * rho, y0 = b * rho;
            Point p1 = new Point((int)(x0 + 1000 * (-b)), (int)(y0 + 1000 * (a)));
            Point p2 = new Point((int)(x0 - 1000 * (-b)), (int)(y0 - 1000 * (a)));

            Point[] imageCorners = new Point[4];

            imageCorners[0] = new Point(0, 0);
            imageCorners[1] = new Point(source.Width, 0);
            imageCorners[2] = new Point(source.Width, source.Height);
            imageCorners[3] = new Point(0, source.Height);

            bool bFoundFirstPoint = false;
            for (int i = 0; i < 4; i++)
            {
                Point intersect = new Point();
                LineIntersection(p1, p2, imageCorners[i], imageCorners[(i + 1) % 4], ref intersect);

                PointF iP = intersect;
                if (iP.X >= 0 && iP.Y >= 0 &&
                        iP.X <= source.Width && iP.Y <= source.Height &&
                        iP.X != iP.Y)
                {

                    if (!bFoundFirstPoint)
                    {
                        pt1 = Point.Round(iP);
                        bFoundFirstPoint = true;
                    }
                    else
                    {
                        pt2 = Point.Round(iP);
                    }
                }
            }
            return true;
        }

        // Line Intersection
        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        public static void LineIntersection(PointF pt1, PointF pt2, PointF pt3, PointF pt4, ref Point intersection)
        {
            //bool lines_intersect;
            bool segments_intersect;
            PointF close_p1;
            PointF close_p2;

            // Get the segments' parameters.
            float dx12 = pt2.X - pt1.X;
            float dy12 = pt2.Y - pt1.Y;
            float dx34 = pt4.X - pt3.X;
            float dy34 = pt4.Y - pt3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((pt1.X - pt3.X) * dy34 + (pt3.Y - pt1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                //lines_intersect = false;
                segments_intersect = false;
                intersection = new Point(int.MaxValue, int.MaxValue);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            //lines_intersect = true;

            float t2 =
                ((pt3.X - pt1.X) * dy12 + (pt1.Y - pt3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new Point((int)(pt1.X + dx12 * t1), (int)(pt1.Y + dy12 * t1));

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(pt1.X + dx12 * t1, pt1.Y + dy12 * t1);
            close_p2 = new PointF(pt3.X + dx34 * t2, pt3.Y + dy34 * t2);
        }

        public static void LineIntersectionRegion(PointF pt1, PointF pt2, CvImage region, CvImage sobelEdge, bool isUpper, ref Point intersection, ref CvImage regionExpand,
            ref bool isPass)
        {
            CvImage line = CvImage.Zeros(region.Rows, region.Cols, DepthType.Cv8U, 1);
            Point pt1New = new Point((int)pt1.X, (int)pt1.Y);
            Point pt2New = new Point((int)pt2.X, (int)pt2.Y);
            CvInvoke.Line(line, pt1New, pt2New, new MCvScalar(255), 1, LineType.EightConnected);
            CvImage regionUnion = new CvImage();
            CvImage regionClosing = new CvImage();
            CvImage regionOpening = new CvImage();
            CvImage regionIntersection = new CvImage();
            CvImage posiblePoint = new CvImage();
            CvPointArray arrayPoint = new CvPointArray();

            CvInvoke.BitwiseOr(line, region, regionUnion);
            int area = 0;
            //double[] minValues, maxValues;
            //Point[] minLoc, maxLoc;
            isPass = false;

            //  20211021
            int low = 1;
            int high = 32;
            int value = (int)((high + low) / 2);
            while (true)
            {
                value = (int)((high + low) / 2);
                MagnusOpenCVLib.ClosingRectangle(ref regionUnion, ref regionClosing, 5 * value, 1);
                MagnusOpenCVLib.OpeningRectangle(ref regionClosing, ref regionOpening, 2, 1);
                CvInvoke.BitwiseAnd(regionOpening, line, regionIntersection);
                CvInvoke.FindNonZero(regionIntersection, arrayPoint);
                area = CvInvoke.CountNonZero(regionIntersection);
                if (area > 0)
                {
                    high = value;
                }
                else
                {
                    low = value;
                }
                if (high - low <= 1)
                {
                    break;
                }
            }
            //
            //for (int i = 1; i <= 5; i++)
            //{
            //    EmageCVLib.ClosingRectangle(ref regionUnion, ref regionClosing, 5 * i, 1);
            //    EmageCVLib.OpeningRectangle(ref regionClosing, ref regionOpening, 2, 1);
            //    CvInvoke.BitwiseAnd(regionOpening, line, regionIntersection);
            //    CvInvoke.FindNonZero(regionIntersection, arrayPoint);
            //    area = CvInvoke.CountNonZero(regionIntersection);
            //    if (area > 0)
            //    {
            //        break;
            //    }
            //}
            if (area > 0)
            {
                if (!isUpper)
                {
                    intersection = arrayPoint[0];
                }
                else
                {
                    intersection = arrayPoint[arrayPoint.Size - 1];
                }
                //}
                isPass = true;
            }

            regionExpand = regionOpening;
        }

        // Intersection Line Contour
        public static void IntersectionLineContour(Point linePt1, Point linePt2, CvImage contour, ref Point firstPoint, ref Point secondPoint, int sortType)
        {
            CvImage lineMat = new CvImage();
            CvImage intersect = new CvImage();

            DrawLine(ref lineMat, linePt1, linePt2, contour.Size);
            CvInvoke.BitwiseAnd(lineMat, contour, intersect);
            CvPointArray linePoint = new CvPointArray();
            Point pt1, pt2;
            CvInvoke.FindNonZero(intersect, linePoint);
            pt1 = linePoint[0];
            pt2 = linePoint[0];

            if (sortType == (int)DIRECTION.X)
                for (int nPointIndex = 0; nPointIndex < linePoint.Size; nPointIndex++)
                {
                    if (linePoint[nPointIndex].X < pt1.X)
                        pt1 = linePoint[nPointIndex];

                    if (linePoint[nPointIndex].X > pt2.X)
                        pt2 = linePoint[nPointIndex];
                }
            else if (sortType == (int)DIRECTION.Y)
            {
                for (int nPointIndex = 0; nPointIndex < linePoint.Size; nPointIndex++)
                {
                    if (linePoint[nPointIndex].Y < pt1.Y)
                        pt1 = linePoint[nPointIndex];

                    if (linePoint[nPointIndex].Y > pt2.Y)
                        pt2 = linePoint[nPointIndex];
                }
            }

            firstPoint = pt1;
            secondPoint = pt2;
        }

        // Sub Region
        public static bool SubRegion(ref CvImage region, ref CvImage subRegion, ref CvImage result)
        {
            CvImage inverseSubRegion = new CvImage();
            CvInvoke.BitwiseNot(subRegion, inverseSubRegion);
            CvInvoke.BitwiseAnd(region, inverseSubRegion, result);
            return true;
        }

        // Sub Image (using size of image not large)
        public static bool subImage(CvImage imageMinued, CvImage imageSub, Image<Gray, byte> grayResult)
        {
            int widthStandard = 0;
            int heightStandard = 0;
            if (imageMinued.Width > imageSub.Width)
            {
                widthStandard = imageSub.Width;
            }
            else
            {
                widthStandard = imageMinued.Width;

            }
            if (imageMinued.Height > imageSub.Height)
            {
                heightStandard = imageSub.Height;
            }
            else
            {
                heightStandard = imageMinued.Height;

            }

            for (int a = 0; a < widthStandard; a++)
            {
                for (int b = 0; b < heightStandard; b++)
                {
                    if (b == heightStandard - 1)
                    {
                        break;
                    }

                    int grayValue = imageSub.GetValue(b, a);
                    int grayValue1 = imageMinued.GetValue(b, a);

                    double sub = (grayValue1 - grayValue) + 128;
                    if (sub <= 10)
                    {
                        grayResult.Data[b, a, 0] = 0;
                    }
                    else if (sub >= 250)
                    {
                        grayResult.Data[b, a, 0] = 255;
                    }
                    else
                    {
                        grayResult.Data[b, a, 0] = 128;
                    }
                }
                if (a == widthStandard - 1)
                {
                    break;
                }
            }

            return true;

        }
        public static bool CropImage(ref Image<Gray, byte> imageCrop, ref Image<Gray, byte> regionCrop, CvImage regionRoi, ref Rectangle rectangleRoi)
        {
            CvPointArray arrayPointRegion = new CvPointArray();
            CvInvoke.FindNonZero(regionRoi, arrayPointRegion);

            rectangleRoi = CvInvoke.BoundingRectangle(arrayPointRegion);
            //rectangleRoi.X = rectangleRoi.X - 2;
            //rectangleRoi.Y = rectangleRoi.Y   - 2;
            //rectangleRoi.Width = rectangleRoi.Width + 4;
            //rectangleRoi.Height = rectangleRoi.Height+ 4;
            imageCrop.ROI = rectangleRoi;
            regionCrop.ROI = rectangleRoi;
            return true;
        }
        public static bool getTemplateIns(ref CvImage templateIns, ref Image<Gray, byte> RegionRoi, ref CvImage templateTeach, ref Rectangle rectangleTemplateIns,ref Size imageSize)

        {
            CvImage region = new CvImage();
            CvImage res = new CvImage();
            CvInvoke.MatchTemplate(templateIns, templateTeach, res, TemplateMatchingType.Ccoeff);
            double minVal = 0.0;
            double maxVal = 0.0;
            System.Drawing.Point minLoc = new System.Drawing.Point(0, 0);
            System.Drawing.Point maxLoc = new System.Drawing.Point(0, 0);
            CvInvoke.MinMaxLoc(res, ref minVal, ref maxVal, ref minLoc, ref maxLoc, new CvImage());
            CvImage Rectangle = new CvImage();
            CvImage Rectangle1 = new CvImage();
            CvImage RectangleResult = new CvImage();
            MagnusOpenCVLib.DrawRect(ref Rectangle, rectangleTemplateIns, imageSize, new MCvScalar(255));
            rectangleTemplateIns.X = rectangleTemplateIns.X - maxLoc.X;
            rectangleTemplateIns.Y = rectangleTemplateIns.Y - maxLoc.Y;
            rectangleTemplateIns.Width = templateTeach.Width;
            rectangleTemplateIns.Height = templateTeach.Height;
            MagnusOpenCVLib.DrawRect(ref Rectangle1, rectangleTemplateIns, imageSize, new MCvScalar(255));
            CvInvoke.BitwiseAnd(Rectangle, Rectangle1, RectangleResult);

            Mat hier1 = new Mat();
            CvContourArray contourscheck = new CvContourArray();
            CvInvoke.FindContours(RectangleResult, contourscheck, hier1, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            double totalAreaCheck = 0;
            for (int cnt = 0; cnt < contourscheck.Size; cnt++)
            {
                totalAreaCheck += CvInvoke.ContourArea(contourscheck[cnt]);

            }
            return true;
        }
        // Calculate Transformation From Reference
        public static bool CalculateTransformationFromReference(ref List<PointF> referenceTeach, ref List<PointF> referenceInspection, ref CvImage maxtrixReference)
        {
            bool error = true;

            CvPointFArray _referenceTeach = new CvPointFArray();
            _referenceTeach.Push(referenceTeach.ToArray());

            CvPointFArray _referenceInspection = new CvPointFArray();
            _referenceInspection.Push(referenceInspection.ToArray());

            CvImage inliers = new CvImage();
            CvImage R = CvInvoke.EstimateAffinePartial2D(_referenceTeach, _referenceInspection, inliers, RobustEstimationAlgorithm.Ransac, 3.0, 2000, 0.99, 10);

            double scaling_x = Math.Sqrt((double)R.GetValue(0, 0) * (double)R.GetValue(0, 0) + (double)R.GetValue(0, 1) * (double)R.GetValue(0, 1));
            double scaling_y = Math.Sqrt((double)R.GetValue(1, 0) * (double)R.GetValue(1, 0) + (double)R.GetValue(1, 1) * (double)R.GetValue(1, 1));

            if ((Math.Abs(1 - scaling_x) > 0.05) || (Math.Abs(1 - scaling_y) > 0.05))
                error = false;

            maxtrixReference = new CvImage(3, 3, DepthType.Cv64F, 1);

            maxtrixReference.SetValue(0, 0, (double)R.GetValue(0, 0));
            maxtrixReference.SetValue(0, 1, (double)R.GetValue(0, 1));
            maxtrixReference.SetValue(0, 2, (double)R.GetValue(0, 2));

            maxtrixReference.SetValue(1, 0, (double)R.GetValue(1, 0));
            maxtrixReference.SetValue(1, 1, (double)R.GetValue(1, 1));
            maxtrixReference.SetValue(1, 2, (double)R.GetValue(1, 2));

            maxtrixReference.SetValue(2, 0, 0);
            maxtrixReference.SetValue(2, 1, 0);
            maxtrixReference.SetValue(2, 2, 1.0);

            return error;
        }

        // Distance Poit to Point
        public static double DistancePP(Point pt1, Point pt2)
        {
            Point diff = new Point(pt1.X - pt2.X, pt1.Y - pt2.Y);
            double distance = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
            return distance;
        }

        // Minimum Distance From Point To Contour
        public static bool MinimumDistanceFromPointToContour(Point pt, CvPointArray contour, ref double minDistance, ref int minPointIndex)
        {
            List<double> distance = new List<double>();
            List<double> _distance = new List<double>();

            for (int pointId = 0; pointId < contour.Size; pointId++)
            {
                Point diff = new Point(contour[pointId].X - pt.X, contour[pointId].Y - pt.Y);
                distance.Add(Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y));
                _distance.Add(Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y));

            }
            _distance.Sort();
            minDistance = _distance[0];
            for (int i = 0; i < distance.Count; i++)
            {
                if (distance[i] == minDistance)
                {
                    minPointIndex = i;
                }
            }

            return true;
        }
        public static bool MinimumDistanceFromPointToContourFloat(PointF pt, CvPointArray contour, ref double minDistance, ref int minPointIndex)
        {
            List<double> distance = new List<double>();
            List<double> _distance = new List<double>();

            for (int pointId = 0; pointId < contour.Size; pointId++)
            {
                PointF diff = new PointF(contour[pointId].X - pt.X, contour[pointId].Y - pt.Y);
                distance.Add(Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y));
                _distance.Add(Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y));

            }
            _distance.Sort();
            minDistance = _distance[0];
            for (int i = 0; i < distance.Count; i++)
            {
                if (distance[i] == minDistance)
                {
                    minPointIndex = i;
                }
            }

            return true;
        }
        public static bool MinnimumDistanceFromPointToLine(PointF pointStandard, PointF line1, PointF line2, ref double distance, bool vertical, bool horizontal)
        {
            double a = 0.0;
            double b = 0.0;
            if (vertical)
            {
                if (line1.X == line2.X)
                {
                    distance = Math.Abs(pointStandard.X - line1.X);
                }
                else
                {
                    a = (line1.Y - line2.Y) / (line1.X - line2.X);
                    b = line1.Y - a * line1.X;
                    distance = (Math.Abs(a * pointStandard.X - pointStandard.Y + b)) / (Math.Sqrt(a * a + b * b));
                }

            }
            if (horizontal)
            {
                if (line1.Y == line2.Y)
                {
                    distance = Math.Abs(pointStandard.Y - line1.Y);
                }
                else
                {
                    a = (line1.Y - line2.Y) / (line1.X - line2.X);
                    b = line1.Y - a * line1.X;
                    distance = (Math.Abs(a * pointStandard.X - pointStandard.Y + b)) / (Math.Sqrt(a * a + b * b));
                }
            }
            return true;
        }
        #endregion

        //*******************************************************************************************************************************************************//
        // Morphology 
        //*******************************************************************************************************************************************************//
        #region Morphology
        // Erosion circle
        public static bool ErosionCircle(ref CvImage source, ref CvImage result, int radius, int iter = 1)
        {
            CvImage kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(2 * radius + 1, 2 * radius + 1), new Point(radius, radius));
            CvInvoke.MorphologyEx(source, result, MorphOp.Erode, kernel, new Point(-1, -1), iter, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            return true;
        }

        // Dilation Circle
        public static bool DilationCircle(ref CvImage source, ref CvImage result, int radius, int iter = 1)
        {
            CvImage kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(2 * radius + 1, 2 * radius + 1), new Point(radius, radius));
            CvInvoke.MorphologyEx(source, result, MorphOp.Dilate, kernel, new Point(-1, -1), iter, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            return true;
        }

        // Opening Circle
        public static bool OpeningCircle(ref CvImage source, ref CvImage result, int radius, int iter = 1)
        {
            CvImage kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(2 * radius + 1, 2 * radius + 1), new Point(radius, radius));
            CvInvoke.MorphologyEx(source, result, MorphOp.Open, kernel, new Point(-1, -1), iter, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            return true;
        }

        // Closing Circle
        public static bool ClosingCircle(ref CvImage source, ref CvImage result, int radius, int iter = 1)
        {
            CvImage kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(2 * radius + 1, 2 * radius + 1), new Point(radius, radius));
            CvInvoke.MorphologyEx(source, result, MorphOp.Close, kernel, new Point(-1, -1), iter, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            return true;
        }

        // Erosion Rectangle
        public static bool ErodeRectangle(ref CvImage source, ref CvImage result, int width, int height, int iter = 1)
        {
            CvImage kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2 * width + 1, 2 * height + 1), new Point(width, height));
            CvInvoke.MorphologyEx(source, result, MorphOp.Erode, kernel, new Point(-1, -1), iter, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            return true;
        }

        // Dilation Rectangle
        public static bool DilationRectangle(ref CvImage source, ref CvImage result, int width, int height, int iter = 1)
        {
            CvImage kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2 * width + 1, 2 * height + 1), new Point(width, height));
            CvInvoke.MorphologyEx(source, result, MorphOp.Dilate, kernel, new Point(-1, -1), iter, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            return true;
        }

        // Opening rectangle
        public static bool OpeningRectangle(ref CvImage source, ref CvImage result, int width, int height, int nIter = 1)
        {
            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2 * width + 1, 2 * height + 1), new Point(-1, -1));
            CvInvoke.MorphologyEx(source, result, MorphOp.Open, kernel, new Point(-1, -1), nIter, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            return true;
        }

        // Closing Rectagle
        public static bool ClosingRectangle(ref CvImage source, ref CvImage result, int nWidth, int nHeight, int nIter = 1)
        {
            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2 * nWidth + 1, 2 * nHeight + 1), new Point(-1, -1));
            CvInvoke.MorphologyEx(source, result, MorphOp.Close, kernel, new Point(-1, -1), nIter, BorderType.Constant, new MCvScalar(0));
            return true;
        }
        #endregion

        //*******************************************************************************************************************************************************//
        // Drawing 
        //*******************************************************************************************************************************************************//
        #region Drawing
        //Draw Line
        public static bool DrawLine(ref CvImage result, Point pt1, Point pt2, Size imgSize)
        {
            if (result.IsEmpty)
                result = CvImage.Zeros(imgSize.Height, imgSize.Width, DepthType.Cv8U, 1);
            CvInvoke.Line(result, pt1, pt2, new MCvScalar(255));
            return true;
        }

        //Draw Rectangle
        public static bool DrawRect(ref CvImage result, Rectangle rect, Size imgSize, MCvScalar color, int thickness = -1)
        {
            if (result.IsEmpty)
                result = CvImage.Zeros(imgSize.Height, imgSize.Width, DepthType.Cv8U, 1);
            CvInvoke.Rectangle(result, rect, new MCvScalar(255), -1);
            return true;
        }

        // Gen Rectangle 2
        public static Mat GenRectangle2(Mat input, RotatedRect rect, MCvScalar color = default(MCvScalar),
        int thickness = 1, LineType lineType = LineType.EightConnected, int shift = 0)
        {
            var v = rect.GetVertices();

            var prevPoint = v[0];
            var firstPoint = prevPoint;
            var nextPoint = prevPoint;
            var lastPoint = nextPoint;


            for (var i = 1; i < v.Length; i++)
            {
                nextPoint = v[i];
                CvInvoke.Line(input, Point.Round(prevPoint), Point.Round(nextPoint), color, thickness, lineType, shift);
                prevPoint = nextPoint;
                lastPoint = prevPoint;
            }
            CvInvoke.Line(input, Point.Round(lastPoint), Point.Round(firstPoint), color, thickness, lineType, shift);
            return input;
        }

        //Draw Contour
        public static bool DrawContour(ref CvImage result, CvPointArray contour, Size imgSize, MCvScalar color, int thickness = -1)
        {
            if (result.IsEmpty)
                result = CvImage.Zeros(imgSize.Height, imgSize.Width, DepthType.Cv8U, 1);
            CvContourArray conts = new CvContourArray(contour);
            CvInvoke.DrawContours(result, conts, -1, color, thickness);
            return true;
        }

        //Draw Contours 
        public static bool DrawContours(ref CvImage result, ref CvContourArray contours, Size imgSize, MCvScalar color, int thickness = -1, int contourIndex = -1)
        {
            if (result.IsEmpty)
                result = CvImage.Zeros(imgSize.Height, imgSize.Width, DepthType.Cv8U, 1);
            CvInvoke.DrawContours(result, contours, contourIndex, color, thickness);
            return true;
        }

        // Draw Hough Lines
        public static bool DrawHoughLines(ref CvImage result, float rho, float theta, Size imgSize)
        {
            if (result.IsEmpty)
                result = CvImage.Zeros(imgSize.Height, imgSize.Width, DepthType.Cv8U, 1);

            double a = Math.Cos(theta), b = Math.Sin(theta);
            double x0 = a * rho, y0 = b * rho;
            Point pt1 = new Point((int)(x0 + 1000 * (-b)), (int)(y0 + 1000 * (a)));
            Point pt2 = new Point((int)(x0 - 1000 * (-b)), (int)(y0 - 1000 * (a)));
            CvInvoke.Line(result, pt1, pt2, new MCvScalar(255), 5);
            return true;
        }

        // Draw Rotated Rectangle
        public static bool DrawRotatedRect(ref CvImage result, RotatedRect rect, Size imgSize, int thickness = -1)
        {
            if (result.IsEmpty)
                result = CvImage.Zeros(imgSize.Height, imgSize.Width, DepthType.Cv8U, 1);

            //List<PointF[]> vertices = new List<PointF[]>();
            //convert RotatedRect --> cvPoint
            CvPointArray vertices = new CvPointArray();
            //vertices.Push(rect.Angle);
            Rectangle rectangle = new Rectangle();

            rectangle.X = (int)(rect.Center.X - rect.Size.Width / 2);
            rectangle.Y = (int)(rect.Center.Y - rect.Size.Height / 2);
            rectangle.Width = (int)rect.Size.Width;
            rectangle.Height = (int)rect.Size.Height;


            Point topLeft = new Point(rectangle.X, rectangle.Y);
            Point topRight = new Point(rectangle.X + rectangle.Width, rectangle.Y);
            Point bottomRight = new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
            Point bottomLeft = new Point(rectangle.X, rectangle.Y + rectangle.Height);

            rect.Equals(vertices);
            Point[] points1 = new Point[4];
            points1[0] = topLeft;
            points1[1] = topRight;
            points1[2] = bottomRight;
            points1[3] = bottomLeft;

            CvPointArray points = new CvPointArray();
            points.Push(points1);
            //for (int i = 0; i < 4; i++)

            DrawContour(ref result, points, imgSize, new MCvScalar(255), -1);
            return true;
        }
        public static bool DrawLineV2(ref CvImage result, PointF point1, PointF point2, Size imgSize, int thickness = -1)
        {
            if (result.IsEmpty)
                result = CvImage.Zeros(imgSize.Height, imgSize.Width, DepthType.Cv8U, 1);
            Point _pt1 = new Point((int)point1.X, (int)point1.Y);
            Point _pt2 = new Point((int)point2.X, (int)point2.Y);
            CvInvoke.Line(result, _pt1, _pt2, new MCvScalar(255), -1);

            return true;
        }
        public static bool DrawRotatedRectNew(ref CvImage result, RotatedRect rect, Size imgSize, int thickness = -1)
        {
            if (result.IsEmpty)
                result = CvImage.Zeros(imgSize.Height, imgSize.Width, DepthType.Cv8U, 1);

            //List<PointF[]> vertices = new List<PointF[]>();
            //convert RotatedRect --> cvPoint
            CvPointArray vertices = new CvPointArray();
            //vertices.Push(rect.Angle);
            Rectangle rectangle = new Rectangle();
            float tempW = rect.Size.Width;
            float tempH = rect.Size.Height;



            if (rect.Size.Width < rect.Size.Height)
            {
                rect.Size.Width = tempH;
                rect.Size.Height = tempW;
            }
            rectangle.X = (int)(rect.Center.X - rect.Size.Width / 2);
            rectangle.Y = (int)(rect.Center.Y - rect.Size.Height / 2);
            rectangle.Width = (int)rect.Size.Width;
            rectangle.Height = (int)rect.Size.Height;


            Point topLeft = new Point(rectangle.X, rectangle.Y);
            Point topRight = new Point(rectangle.X + rectangle.Width, rectangle.Y);
            Point bottomRight = new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
            Point bottomLeft = new Point(rectangle.X, rectangle.Y + rectangle.Height);

            rect.Equals(vertices);
            Point[] points1 = new Point[4];
            points1[0] = topLeft;
            points1[1] = topRight;
            points1[2] = bottomRight;
            points1[3] = bottomLeft;

            CvPointArray points = new CvPointArray();
            points.Push(points1);
            //for (int i = 0; i < 4; i++)

            DrawContour(ref result, points, imgSize, new MCvScalar(255), -1);
            return true;
        }
        public static bool SelectGray(ref CvImage image, ref CvImage source, ref CvImage result, ref List<Rectangle> rectArray, ref int grayOfDefect, int minValue, int maxValue, double minDev)
        {
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            result = CvImage.Zeros(source.Height, source.Width, DepthType.Cv8U, 1);
            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);
            int maxMeanGrayValue = 0;
            for (int i = 1; i < nLabels; i++)
            {
                CvImage compareMat = new CvImage();
                CvImage interestedLabelIndexMat = labelImage.Clone();
                interestedLabelIndexMat.SetTo(new MCvScalar(i));
                CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);

                MCvScalar meanGray = new MCvScalar();
                MCvScalar dev = new MCvScalar();
                Point maxLoc = new Point();
                Point minLoc = new Point();
                double maxGray = new double();
                double minGray = new double();
                CvInvoke.MinMaxLoc(image, ref minGray, ref maxGray, ref minLoc, ref maxLoc, compareMat);
                CvInvoke.MeanStdDev(image, ref meanGray, ref dev, compareMat);
                if ((int)meanGray.V0 >= minValue && (int)meanGray.V0 <= maxValue && dev.V0 > minDev/* && minGray <= 105*/)
                {
                    var x = statsData[i * stats.Cols + 0];
                    var y = statsData[i * stats.Cols + 1];
                    var width = statsData[i * stats.Cols + 2];
                    var height = statsData[i * stats.Cols + 3];

                    result = result | compareMat;
                    rectArray.Add(new Rectangle(x, y, width, height));
                    if (maxMeanGrayValue < (int)meanGray.V0)
                    {
                        maxMeanGrayValue = (int)meanGray.V0;
                    }
                }
            }
            return true;
        }
        public static bool ExtractValuePosibleDefectPackageSide(CvImage imageChanelG, CvImage source, ref List<double> lmWidth, ref List<double> lmHeight, ref List<double> lMaxMeanGrayValue)
        {
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);

            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);
            for (int i = 1; i < nLabels; i++)
            {
                CvImage compareMat = new CvImage();
                CvImage interestedLabelIndexMat = labelImage.Clone();
                interestedLabelIndexMat.SetTo(new MCvScalar(i));
                CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);

                MCvScalar meanGray = new MCvScalar();
                MCvScalar dev = new MCvScalar();
                Point maxLoc = new Point();
                Point minLoc = new Point();
                double maxGray = new double();
                double minGray = new double();
                CvInvoke.MinMaxLoc(imageChanelG, ref minGray, ref maxGray, ref minLoc, ref maxLoc, compareMat);
                CvInvoke.MeanStdDev(imageChanelG, ref meanGray, ref dev, compareMat);

                var width = statsData[i * stats.Cols + 2];
                lmWidth.Add(width);
                var height = statsData[i * stats.Cols + 3];
                lmHeight.Add(height);
                lMaxMeanGrayValue.Add(meanGray.V0);
            }
            return true;
        }
        public static bool SelectRegionBySizeAndIntersetion(ref CvImage source, ref CvImage reference, ref CvImage result, ref List<Rectangle> rectArray, int minWidth, int minHeight, List<double> List_WidthOfDefect, List<double> List_HeightOfDefet, int isOperator)
        {
            int delta = 5;
            CvImage labelImage = new CvImage(source.Size, DepthType.Cv32S, 1);
            CvImage stats = new CvImage();
            CvImage centroids = new CvImage();
            int nLabels = CvInvoke.ConnectedComponentsWithStats(source, labelImage, stats, centroids, LineType.EightConnected, DepthType.Cv32S);
            result = CvImage.Zeros(source.Height, source.Width, DepthType.Cv8U, 1);
            int[] statsData = new int[stats.Rows * stats.Cols];
            stats.CopyTo(statsData);
            for (int i = 1; i < nLabels; i++)
            {
                if (isOperator == 1)
                {
                    //bOperator == True: OR operation

                    if ((statsData[i * stats.Cols + 3]) > minHeight || (statsData[i * stats.Cols + 2]) > minWidth)
                    {

                        CvImage compareMat = new CvImage();
                        CvImage intersection = new CvImage();
                        CvImage interestedLabelIndexMat = labelImage.Clone();
                        interestedLabelIndexMat.SetTo(new MCvScalar(i));
                        CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                        CvInvoke.BitwiseAnd(compareMat, reference, intersection);
                        int area = CvInvoke.CountNonZero(intersection);
                        if (area > 10)
                        {
                            List_WidthOfDefect.Add(statsData[i * stats.Cols + 2]);
                            List_HeightOfDefet.Add(statsData[i * stats.Cols + 3]);
                            var x = statsData[i * stats.Cols + 0];
                            var y = statsData[i * stats.Cols + 1];
                            var width = statsData[i * stats.Cols + 2];
                            var height = statsData[i * stats.Cols + 3];
                            result = result | compareMat;
                            rectArray.Add(new Rectangle(x - delta, y - delta, width + 2 * delta, height + 2 * delta));
                        }
                    }
                }
                else
                {
                    //bOperator == False: AND operation
                    if ((statsData[i * stats.Cols + 3]) > minHeight && (statsData[i * stats.Cols + 2]) > minWidth)
                    {
                        CvImage compareMat = new CvImage();
                        CvImage intersection = new CvImage();
                        CvImage interestedLabelIndexMat = labelImage.Clone();
                        interestedLabelIndexMat.SetTo(new MCvScalar(i));
                        CvInvoke.Compare(labelImage, interestedLabelIndexMat, compareMat, CmpType.Equal);
                        CvInvoke.BitwiseAnd(compareMat, reference, intersection);
                        int area = CvInvoke.CountNonZero(intersection);
                        if (area > 0)
                        {
                            List_WidthOfDefect.Add(statsData[i * stats.Cols + 2]);
                            List_HeightOfDefet.Add(statsData[i * stats.Cols + 3]);
                            var x = statsData[i * stats.Cols + 0];
                            var y = statsData[i * stats.Cols + 1];
                            var width = statsData[i * stats.Cols + 2];
                            var height = statsData[i * stats.Cols + 3];
                            result = result | compareMat;
                            rectArray.Add(new Rectangle(x - delta, y - delta, width + 2 * delta, height + 2 * delta));
                        }

                    }
                }
            }

            return true;
        }

        #endregion
    }
}
