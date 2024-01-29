using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CvImage = Emgu.CV.Mat;
using CvPointFArray = Emgu.CV.Util.VectorOfPointF;

namespace TapeReelPacking.Source.Algorithm
{
    public class TemplateMatchingModel
    {
        private Rectangle m_rectTemplateRoi;
        private CvImage _source;
        private double _angleStart, _angleExtent, _angleStep;
        private int numberOfSteps = 0;
        System.Drawing.Size templateOffset;

        List<CvImage> m_templateImages;
        List<CvImage> m_maskTemplate;

        public Rectangle m_searchRect;
        public TemplateMatchingModel() { }
        ~TemplateMatchingModel() { }

        public TemplateMatchingModel(ref CvImage source, Rectangle roi, double angleStart, double angleExtent, double angleStep)
        {
            _source = source.Clone();
            m_rectTemplateRoi = roi;

            if (angleExtent < angleStep) return;

            _angleStart = angleStart;
            _angleExtent = angleExtent;
            _angleStep = angleStep;

            numberOfSteps = (int)(angleExtent / angleStep);
            m_templateImages = new List<Mat>(numberOfSteps);
            m_maskTemplate = new List<Mat>(numberOfSteps);
            templateOffset = new System.Drawing.Size(0, 0);

            for (int index = 0; index < numberOfSteps; ++index)
            {
                CvImage templateMat = new CvImage();
                CvImage maskMat = new CvImage();

                RotateCropImageAndMask(ref _source, m_rectTemplateRoi, (float)(angleStart + angleStep * index), ref templateMat, ref maskMat);
                m_templateImages.Add(templateMat);
                m_maskTemplate.Add(maskMat);

                if ((templateMat.Size.Width - m_rectTemplateRoi.Width) > templateOffset.Width)
                {
                    templateOffset.Width = templateMat.Size.Width - m_rectTemplateRoi.Width;
                }
                if ((templateMat.Size.Height - m_rectTemplateRoi.Height) > templateOffset.Height)
                {
                    templateOffset.Height = templateMat.Size.Height - m_rectTemplateRoi.Height;
                }
            }
        }

        // Rotate Image
        private CvImage RotateImage(CvImage source, float rotateAngle)
        {
            CvImage rotatedImage = new CvImage();
            CvImage mapMatrix = new CvImage();
            CvInvoke.GetRotationMatrix2D(new PointF(source.Size.Width / 2.0f, source.Size.Height / 2.0f),
                        rotateAngle, 1.0, mapMatrix);

            System.Drawing.Size szTemplateSize = new System.Drawing.Size(source.Cols, source.Rows);

            CvInvoke.WarpAffine(source, rotatedImage, mapMatrix, szTemplateSize);
            return rotatedImage;
        }

        // Rotate Crop Image
        private CvImage RotateCropImage(CvImage source, Rectangle roi, float rotateAngle)
        {
            CvImage rotatedImage = new CvImage();
            CvImage mapMatrix = new CvImage();
            PointF _centerRoi = new PointF((float)(roi.Right + roi.X) / 2.0f,
                                     (float)(roi.Bottom + roi.Y) / 2.0f);
            CvInvoke.GetRotationMatrix2D(_centerRoi, rotateAngle, 1.0, mapMatrix);

            System.Drawing.Size szSource = source.Size;

            CvInvoke.WarpAffine(source, rotatedImage, mapMatrix, szSource);
            rotatedImage = new CvImage(rotatedImage, roi);

            return rotatedImage;
        }

        // Rotate Crop Image And Mask
        private int RotateCropImageAndMask(ref CvImage source, Rectangle roi, float rotateAngle, ref CvImage rotatedCropImage, ref CvImage rotatedMask)
        {
            CvImage rotatedImage = new CvImage();
            CvImage maskRotated = new CvImage();
            CvImage mapMatrix = new CvImage();

            PointF _centerRoi = new PointF((float)(roi.Right + roi.X) / 2.0f,
                                                 (float)(roi.Bottom + roi.Y) / 2.0f);

            CvInvoke.GetRotationMatrix2D(_centerRoi, rotateAngle, 1.0, mapMatrix);

            System.Drawing.Size szSource = source.Size;
            CvInvoke.WarpAffine(source, rotatedImage, mapMatrix, szSource);

            CvImage mask = new CvImage(szSource, DepthType.Cv8U, 1);

            CvInvoke.Rectangle(mask, roi, new MCvScalar(255), -1);
            CvInvoke.WarpAffine(mask, maskRotated, mapMatrix, szSource);

            CvImage Points = new CvImage();
            CvInvoke.FindNonZero(maskRotated, Points);
            Rectangle newRoi = CvInvoke.BoundingRectangle(Points);

            rotatedCropImage = (new CvImage(rotatedImage, newRoi)).Clone();
            rotatedMask = (new CvImage(maskRotated, newRoi)).Clone();

            return 0;
        }

        // Correlation
        private double Correlation(CvImage source1, CvImage source2)
        {
            double correl = 0;
            Mat corr = new Mat();
            if (source1.Width <= source2.Width && source1.Height <= source2.Height)
            {
                CvImage im_float_1 = new CvImage();
                source1.ConvertTo(im_float_1, DepthType.Cv32F);

                CvImage im_float_2 = new CvImage();
                source2.ConvertTo(im_float_2, DepthType.Cv32F);
                int n_pixels = im_float_1.Rows * im_float_1.Cols;

                // Compute mean and standard deviation of both images
                MCvScalar im1_Mean = new MCvScalar();
                MCvScalar im1_Std = new MCvScalar();

                MCvScalar im2_Mean = new MCvScalar();
                MCvScalar im2_Std = new MCvScalar();

                CvInvoke.MeanStdDev(im_float_1, ref im1_Mean, ref im1_Std);
                CvInvoke.MeanStdDev(im_float_2, ref im2_Mean, ref im2_Std);

                // Compute covariance and correlation coefficient	
                double covar = (im_float_1 - im1_Mean).Dot(im_float_2 - im2_Mean) * 100 / n_pixels;

                double deviation = (im1_Std.ToArray()[0] * im2_Std.ToArray()[0]);
                correl = covar / deviation;
            }
            else
                correl = 0;
            return correl;
        }

        // Template Matching
        private bool CvTemplateMatching(CvImage searchImage, CvImage templateImage, CvImage maskImage, ref Rectangle rectFoundPosition, ref double matchingScore)
        {
            System.Drawing.Size szMatchingResult = searchImage.Size - templateImage.Size + new System.Drawing.Size(1, 1);

            CvImage result = new CvImage(szMatchingResult, DepthType.Cv32F, 1);
            CvImage foundTemplate;
            CvInvoke.MatchTemplate(searchImage, templateImage, result, TemplateMatchingType.Ccoeff);
            double minVal = 0.0;
            double maxVal = 0.0;
            System.Drawing.Point minLoc = new System.Drawing.Point(0, 0);
            System.Drawing.Point maxLoc = new System.Drawing.Point(0, 0);
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc, new CvImage());
            rectFoundPosition = new Rectangle(maxLoc.X, maxLoc.Y, (int)((templateImage.Cols)),
                                                   (int)((templateImage.Rows)));
            //Measure the score
            if (rectFoundPosition.X > 0 && rectFoundPosition.Y > 0 && rectFoundPosition.Width > 0 && rectFoundPosition.Height > 0 && (rectFoundPosition.X + rectFoundPosition.Width) < searchImage.Cols && (rectFoundPosition.Y + rectFoundPosition.Height) < searchImage.Rows)
            {
                foundTemplate = new CvImage(searchImage, rectFoundPosition);
                //if (foundTemplate.Width == templateImage.Width && foundTemplate.Height == templateImage.Height)
                //{
                double dScore = Correlation(foundTemplate, templateImage);
                matchingScore = dScore;
                //}

                //else
                //	dMatchingScore = 0;
            }
            else
                matchingScore = 0;

            return true;
        }

        public bool MAgnus_TemplateMatching(CvImage img_ImageSource, CvImage img_Template, double minMatchingScore, int nStep, double nResolution, ref Rectangle rectMatchingPosition, ref double matchingAngle, ref double matchingScore)
        {

            float angleStart = -180;
            List<CvImage> list_templateImages = new List<CvImage>();
            List<CvImage> list_maskTemplateImages = new List<CvImage>();

            for (int index = 0; index < nStep; ++index)
            {
                CvImage templateMat = new CvImage();
                CvImage maskMat = new CvImage();
                Rectangle searchRoi = new Rectangle(0, 0, img_Template.Width, img_Template.Height);
                RotateCropImageAndMask(ref img_Template, searchRoi, (float)(angleStart + nResolution * index), ref templateMat, ref maskMat);

                list_templateImages.Add(templateMat);
                list_maskTemplateImages.Add(maskMat);
            }

            Rectangle[] rectTemplateMatchedPosition = new Rectangle[nStep];
            double[] dTemplateMatchedScore = new double[nStep];

            Parallel.For(0, nStep,
             index =>
             {
                 CvTemplateMatching(img_ImageSource, list_templateImages[index], list_maskTemplateImages[index], ref rectTemplateMatchedPosition[index], ref dTemplateMatchedScore[index]);
             });
            //for (int index = 0; index < nStep; index++)
            //    CvTemplateMatching(img_ImageSource, list_templateImages[index], list_maskTemplateImages[index], ref rectTemplateMatchedPosition[index], ref dTemplateMatchedScore[index]);
            //for (int index = 0; index < nStep; index++)
            //    CvTemplateMatching(img_ImageSource, list_templateImages[index], list_maskTemplateImages[index], ref rectTemplateMatchedPosition[index], ref dTemplateMatchedScore[index]);

            var nMaxScoreIndex = dTemplateMatchedScore.ToList().LastIndexOf(dTemplateMatchedScore.Max());
            bool bFound = true;
            if (dTemplateMatchedScore[nMaxScoreIndex] < minMatchingScore)
            {
                bFound = false;
            }
            //bFound = true;
            matchingScore = dTemplateMatchedScore[nMaxScoreIndex];
            matchingAngle = angleStart + nResolution * (nMaxScoreIndex);

            //if (matchingAngle >= 180)
            //    matchingAngle = 180 - matchingAngle;
            //if (matchingAngle <= -180)
            //    matchingAngle = 180 + matchingAngle;

            rectMatchingPosition = new Rectangle(new System.Drawing.Point(rectTemplateMatchedPosition[nMaxScoreIndex].X,
                                               rectTemplateMatchedPosition[nMaxScoreIndex].Y)
                                               , rectTemplateMatchedPosition[nMaxScoreIndex].Size);
            return bFound;
        }


        public bool MAgnus_KdTreeTemplateMatching(CvImage img_ImageSource, CvImage img_Template, double minMatchingScore, double dResolution, ref Rectangle rectMatchingPosition, ref double matchingAngle, ref double matchingScore)
        {

            int nStep = 5;
            double dResolutionTemp = 15;

            double dAngleStart = matchingAngle - dResolutionTemp;

            while (dResolutionTemp >= dResolution)
            {
                dResolutionTemp = dResolutionTemp / 2;

                List<CvImage> list_templateImages = new List<CvImage>();
                List<CvImage> list_maskTemplateImages = new List<CvImage>();

                for (int index = 0; index < nStep; ++index)
                {
                    CvImage templateMat = new CvImage();
                    CvImage maskMat = new CvImage();
                    Rectangle searchRoi = new Rectangle(0, 0, img_Template.Width, img_Template.Height);
                    RotateCropImageAndMask(ref img_Template, searchRoi, (float)(dAngleStart + dResolutionTemp * index), ref templateMat, ref maskMat);

                    list_templateImages.Add(templateMat);
                    list_maskTemplateImages.Add(maskMat);
                }

                Rectangle[] rectTemplateMatchedPosition = new Rectangle[nStep];
                double[] dTemplateMatchedScore = new double[nStep];

                Parallel.For(0, nStep,
                 index =>
                 {
                     CvTemplateMatching(img_ImageSource, list_templateImages[index], list_maskTemplateImages[index], ref rectTemplateMatchedPosition[index], ref dTemplateMatchedScore[index]);
                 });

                var nMaxScoreIndex = dTemplateMatchedScore.ToList().LastIndexOf(dTemplateMatchedScore.Max());
                //bFound = true;
                matchingScore = dTemplateMatchedScore[nMaxScoreIndex];
                matchingAngle = dAngleStart + dResolutionTemp * nMaxScoreIndex;

                if (matchingAngle >= 180)
                    matchingAngle = 180 - matchingAngle;
                if (matchingAngle <= -180)
                    matchingAngle = 180 + matchingAngle;

                rectMatchingPosition = new Rectangle(new System.Drawing.Point(rectTemplateMatchedPosition[nMaxScoreIndex].X,
                                                   rectTemplateMatchedPosition[nMaxScoreIndex].Y)
                                                   , rectTemplateMatchedPosition[nMaxScoreIndex].Size);


                dAngleStart = matchingAngle - dResolutionTemp;

                if (dAngleStart >= 180)
                    dAngleStart = 180 - dAngleStart;
                if (dAngleStart <= -180)
                    dAngleStart = 180 + dAngleStart;


            }

            if (matchingScore < minMatchingScore)
                return false;
            else
                return true;
        }


        public static bool CalculateTransformationFromPattern(ref List<TemplateMatchingModel> models,
                                      List<bool> bPatternFound,
                                      ref List<RotatedRect> rrPatternLocation,
                                       ref CvImage transformMatrix)
        {
            bool error = true;
            List<PointF> originalPoints = new List<PointF>();
            List<PointF> destinationPoints = new List<PointF>();

            int nFoundPatternCount = 0;
            for (int i = 0; i < bPatternFound.Count; i++)
            {
                if (bPatternFound[i])
                {
                    nFoundPatternCount = nFoundPatternCount + 1;
                }
            }

            if (nFoundPatternCount > 1)
            {
                for (int nModelIndex = 0; nModelIndex < models.Count; nModelIndex++)
                {
                    if (bPatternFound[nModelIndex])
                    {
                        originalPoints.Add((models[nModelIndex].GetPatternCenter()));
                        destinationPoints.Add(rrPatternLocation[nModelIndex].Center);
                    }
                }
            }
            else
            {
                originalPoints = new List<PointF>(2);
                destinationPoints = new List<PointF>(2);

                for (int nModelIndex = 0; nModelIndex < models.Count; nModelIndex++)
                {
                    if (bPatternFound[nModelIndex])
                    {
                        originalPoints.Add(models[nModelIndex].GetPatternTopLeft());
                        originalPoints.Add(models[nModelIndex].GetPatternBottomRight());

                        //cv::Point2f vertices[4];
                        List<PointF> vertices = new List<PointF>(4);

                        destinationPoints.Add(new PointF(rrPatternLocation[nModelIndex].GetVertices()[1].X, rrPatternLocation[nModelIndex].GetVertices()[1].Y));
                        destinationPoints.Add(new PointF(rrPatternLocation[nModelIndex].GetVertices()[3].X, rrPatternLocation[nModelIndex].GetVertices()[3].Y));
                    }
                }
            }

            CvPointFArray _originalPoints = new CvPointFArray();
            _originalPoints.Push(originalPoints.ToArray());

            CvPointFArray _destinationPoints = new CvPointFArray();
            _destinationPoints.Push(destinationPoints.ToArray());

            CvImage inliers = new CvImage();
            CvImage R = CvInvoke.EstimateAffinePartial2D(_originalPoints, _destinationPoints, inliers, RobustEstimationAlgorithm.Ransac, 3.0, 2000, 0.99, 10);

            double scaling_x = Math.Sqrt((double)R.GetValue(0, 0) * (double)R.GetValue(0, 0) + (double)R.GetValue(0, 1) * (double)R.GetValue(0, 1));
            double scaling_y = Math.Sqrt((double)R.GetValue(1, 0) * (double)R.GetValue(1, 0) + (double)R.GetValue(1, 1) * (double)R.GetValue(1, 1));

            if ((Math.Abs(1 - scaling_x) > 0.05) || (Math.Abs(1 - scaling_y) > 0.05))
                error = false;

            transformMatrix = new CvImage(3, 3, DepthType.Cv64F, 1);

            transformMatrix.SetValue(0, 0, (double)R.GetValue(0, 0));
            transformMatrix.SetValue(0, 1, (double)R.GetValue(0, 1));
            transformMatrix.SetValue(0, 2, (double)R.GetValue(0, 2));

            transformMatrix.SetValue(1, 0, (double)R.GetValue(1, 0));
            transformMatrix.SetValue(1, 1, (double)R.GetValue(1, 1));
            transformMatrix.SetValue(1, 2, (double)R.GetValue(1, 2));

            transformMatrix.SetValue(2, 0, 0);
            transformMatrix.SetValue(2, 1, 0);
            transformMatrix.SetValue(2, 2, 1.0);

            return error;
        }
        public PointF GetPatternCenter()
        {
            PointF _center = new PointF();
            _center.X = (float)((GetPatternTopLeft().X + GetPatternBottomRight().X) / 2.0);
            _center.Y = (float)((GetPatternTopLeft().Y + GetPatternBottomRight().Y) / 2.0);
            return _center;
        }

        public PointF GetPatternTopLeft()
        {
            return new PointF((float)m_rectTemplateRoi.X, (float)m_rectTemplateRoi.Y);
        }

        public PointF GetPatternBottomRight()
        {
            return new PointF((float)m_rectTemplateRoi.Right, (float)m_rectTemplateRoi.Bottom);
        }
    }
}
