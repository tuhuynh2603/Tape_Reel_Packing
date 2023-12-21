using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.WPF;
using Magnus_WPF_1.Source.Algorithm;
using Magnus_WPF_1.Source.Application;
using Magnus_WPF_1.Source.Define;
using Magnus_WPF_1.Source.DrawingOverlay;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CvImage = Emgu.CV.Mat;
using Line = System.Windows.Shapes.Line;
using LineArray = System.Collections.Generic.List<Emgu.CV.Structure.LineSegment2D>;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Magnus_WPF_1.UI.UserControls.View
{
    /// <summary>
    /// Interaction logic for ImageView.xaml
    /// </summary>
    ///  public  Master master;

    public partial class ImageView : UserControl, INotifyPropertyChanged
    {
        public int _imageWidth;
        public int _imageHeight;

        public int _stride;
        public int _dpi;
        private List<Results> results = new List<Results>();

        public int stepDebug = 0;
        public bool isStepDebugging = false;
        public byte[] bufferImage;
        public BitmapSource btmSource;
        public TransformImage transform;

        public string nameImage;
        // public DefectInfor defectInfor = new DefectInfor();
        public bool IsCamStream = false;

        public int trackID;
        public int docID;

        public int enableGray = 0;
        private int valueSliderGray = 127;

        public int dockPaneID = 0;

        public bool isShowOverLay = true;
        public bool isBinary = false;
        public bool isSetupGrid = false;
        public bool isLoadingImage = true;

        //public TransformImage transform;


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private Visibility _visibleRGB = Visibility.Collapsed;
        public Visibility visibleRGB
        {
            get { return _visibleRGB; }
            set
            {
                _visibleRGB = value;
                OnPropertyChanged("visibleRGB");
            }
        }


        private double _OldWidth = 0;
        public double OldWidth
        {
            get { return _OldWidth; }
            set
            {
                if (_OldWidth != value)
                {
                    _OldWidth = value;
                    OnPropertyChanged("OldWidth");
                }
            }
        }
        private double _OldHeight = 0;
        public double OldHeight
        {
            get { return _OldHeight; }
            set
            {
                if (_OldHeight != value)
                {
                    _OldHeight = value;
                    OnPropertyChanged("OldHeight");
                }
            }
        }

        public ImageView(int width, int height, int dpi, int track, int docID)
        {
            this.trackID = track;
            this.DataContext = this;
            InitializeComponent();
            dockPaneID += docID;
            SetupImageDockMouseFeature();


        }
        public void SetBackgroundDoc(int trackID)
        {
            //byte[] bufer;
            switch (trackID)
            {
                case 0:
                    image.Source = null;
                    btmSource = null;
                    break;
                case 1:
                case 2:
                    image.Source = null;
                    btmSource = null;
                    //bufer = Enumerable.Repeat((byte)0x2D, _imageWidth * _imageHeight).ToArray();
                    //btmSource = BitmapSource.Create(_imageWidth, _imageHeight, _dpi, _dpi,
                    //	PixelFormats.Gray8, BitmapPalettes.Gray256, bufer, (PixelFormats.Gray8.BitsPerPixel + 7) / 8 * _imageWidth);
                    break;
                default:
                    break;
            }
        }
        #region Load Image To View
        public bool UpdateNewImage(BitmapImage btm)
        {
            try
            {
                IsCamStream = false;
                GridOverlay.Children.Clear();
                GridResult.Children.Clear();
                _imageWidth = btm.PixelWidth;
                _imageHeight = btm.PixelHeight;
                _stride = _imageWidth * 4;
                bufferImage = new byte[_stride * _imageHeight];
                btm.CopyPixels(bufferImage, _stride, 0);
                UpdateSourceImageMono();

                // UpdateSourceImageColor(true);
                // MainWindow.mainWindow.isInspectOffline = false;
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Wrong Image File", "Load Image", MessageBoxButton.OK, MessageBoxImage.Information);
                SetBackgroundDoc(trackID);
                return false;
            }
        }
        public void UpdateNewImageColor(byte[] bufferImage, int imageWidth, int imageHeight, int dpi)
        {
            this.Dispatcher.Invoke(() =>
            {
                IsCamStream = true;
                GridOverlay.Children.Clear();
                GridResult.Children.Clear();
                _imageWidth = imageWidth;
                _imageHeight = imageHeight;
                _stride = _imageWidth * 3;
                _dpi = dpi;
                this.bufferImage = bufferImage;

                UpdateSourceImageColor();
            });
        }

        public byte[] ReadBufferFromFile(string pathImage, ref int iwI, ref int ihI, ref int stride)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Stream stream = new FileStream(pathImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(stream);

            // Lock the bitmap's bits.  
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                bmp.PixelFormat);
            int channels = bmpData.Stride / bmpData.Width;
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;
            iwI = bmp.Width;
            stride = iwI * channels;
            ihI = bmp.Height;
            byte[] bufferCamera = new byte[stride * ihI];
            if (bmp.Width != _imageWidth || bmp.Height != _imageHeight)
                return bufferCamera;
            int x = bmpData.Stride - bmpData.Width * channels;
            if (x != 0)
            {
                int bmpStride = bmpData.Stride;
                int bmpWidth = bmpData.Width;
                int bmpHeight = bmpData.Height;
                byte[] temp = new byte[bmpData.Stride * bmpData.Height];
                Marshal.Copy(ptr, temp, 0, temp.Length);
                Parallel.For(0, ihI, i =>
                {
                    Array.Copy(temp, i * bmpStride, bufferCamera, i * bmpWidth * channels, bmpWidth * channels);
                });
            }
            else
            {
                Marshal.Copy(ptr, bufferCamera, 0, bufferCamera.Length);
            }
            bmp.UnlockBits(bmpData);
            bmp.Dispose();
            stream.Dispose();
            return bufferCamera;
        }
        public bool UpdateNewImageMono(string pathImage)
        {
            try
            {
                int iw = 0;
                int ih = 0;
                int stride = 0;
                bufferImage = ReadBufferFromFile(pathImage, ref iw, ref ih, ref stride);

                //CvImage matTemp = CvInvoke.Imread(pathImage, Emgu.CV.CvEnum.ImreadModes.Grayscale);

                //bufferImage = ConvertMonoMatToByteArray(matTemp);
                //UpdateUIImageMono(bufferImage);
                UpdateSourceImageMono();
                return true;
            }
            catch (Exception)
            {
                //MessageBox.Show("Wrong Image File", "Load Image", MessageBoxButton.OK, MessageBoxImage.Information);
                SetBackgroundDoc(trackID);
                return false;
            }
        }

        static byte[] ConvertMonoMatToByteArray(Mat matMono)
        {
            // Convert mono Mat to Image<Gray, byte>
            Image<Gray, byte> imageMono = matMono.ToImage<Gray, byte>();

            // Convert Image<Gray, byte> to byte array
            byte[] byteArrayMono = ImageToByteArray(imageMono);

            return byteArrayMono;
        }

        static byte[] ImageToByteArray(Image<Gray, byte> imageMono)
        {
            // Convert Image<Gray, byte> to Bitmap
            System.Drawing.Bitmap bitmapMono = imageMono.ToBitmap();

            // Lock the bits of the bitmap
            System.Drawing.Imaging.BitmapData bmpData = bitmapMono.LockBits(new System.Drawing.Rectangle(0, 0, bitmapMono.Width, bitmapMono.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmapMono.PixelFormat);

            // Calculate the number of bytes required to store the bitmap
            int byteCount = Math.Abs(bmpData.Stride) * bitmapMono.Height;

            // Create a byte array to hold the bitmap data
            byte[] byteArrayMono = new byte[byteCount];

            // Copy the locked bytes from memory to the byte array
            Marshal.Copy(bmpData.Scan0, byteArrayMono, 0, byteCount);

            // Unlock the bits of the bitmap
            bitmapMono.UnlockBits(bmpData);

            return byteArrayMono;
        }

        public void UpdateSourceImageMono(bool isOnline = false)
        {
            // //DebugMessage.WriteToDebugViewer(1, $"_imageWidth of track: {_imageWidth}");
            ////DebugMessage.WriteToDebugViewer(1, $"_imageHeight of track: {_imageHeight}");

            lock (bufferImage)
            {
                //DebugMessage.WriteToDebugViewer(1, $"Start - Buffer camera:");
                if (isOnline)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    if (enableGray < 1)
                    {
                        IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(bufferImage, 0);
                        System.Drawing.Bitmap bitmap1 = new System.Drawing.Bitmap(_imageWidth, _imageHeight, _imageWidth,
                            System.Drawing.Imaging.PixelFormat.Format8bppIndexed, ptr);
                        System.Drawing.Imaging.ColorPalette cp = bitmap1.Palette;
                        for (int i = 0; i < 256; i++)
                        {
                            cp.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                        }
                        // set palette back
                        bitmap1.Palette = cp;
                        using (Stream stream = new MemoryStream())
                        {
                            bitmap1.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                            BitmapImage bitmapimage = new BitmapImage();
                            bitmapimage.BeginInit();
                            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapimage.StreamSource = stream;
                            bitmapimage.DecodePixelWidth = _imageWidth;
                            bitmapimage.EndInit();
                            bitmapimage.Freeze();
                            image.Dispatcher.Invoke((Action)delegate
                            {
                                btmSource = bitmapimage;
                                image.Source = bitmapimage;
                            });
                        }
                    }
                    else
                    {
                        byte[] bufferConvert = ConvertMonoToBinary();
                        //DebugMessage.WriteToDebugViewer(1, $"ConvertMonoToBinary: ");
                        if (bufferConvert.Length == 0)
                            return;
                        image.Dispatcher.Invoke((Action)delegate
                        {
                            image.Source = BitmapSource.Create(_imageWidth, _imageHeight, _dpi, _dpi,
                                PixelFormats.Gray8, BitmapPalettes.Gray256, bufferConvert, (PixelFormats.Gray8.BitsPerPixel + 7) / 8 * _imageWidth);
                        });
                    }
                    stopwatch.Stop();
                }
                else
                {
                    if (enableGray < 1)
                    {
                        image.Dispatcher.Invoke(() =>
                        {
                            btmSource = BitmapSource.Create(_imageWidth, _imageHeight, _dpi, _dpi,
                                PixelFormats.Gray8, BitmapPalettes.Gray256, bufferImage, (PixelFormats.Gray8.BitsPerPixel + 7) / 8 * _imageWidth);
                            image.Source = btmSource;
                        });
                        //DebugMessage.WriteToDebugViewer(1, $"enableGray: {enableGray}");
                    }
                    else
                    {
                        byte[] bufferConvert = ConvertMonoToBinary();
                        //DebugMessage.WriteToDebugViewer(1, $"ConvertMonoToBinary: ");
                        if (bufferConvert.Length == 0)
                            return;
                        image.Dispatcher.Invoke(() =>
                        {
                            btmSource = BitmapSource.Create(_imageWidth, _imageHeight, _dpi, _dpi,
                                PixelFormats.Gray8, BitmapPalettes.Gray256, bufferConvert, (PixelFormats.Gray8.BitsPerPixel + 7) / 8 * _imageWidth);
                            image.Source = btmSource;
                        });
                        //DebugMessage.WriteToDebugViewer(1, $"ConvertMonoToBinary: ");
                    }
                }
                //DebugMessage.WriteToDebugViewer(1, $"End - Buffer camera:");
            }
        }

        public void UpdateUIImageMono(byte[] buff)
        {
            // //DebugMessage.WriteToDebugViewer(1, $"_imageWidth of track: {_imageWidth}");
            ////DebugMessage.WriteToDebugViewer(1, $"_imageHeight of track: {_imageHeight}");

            lock (buff)
            {
            //DebugMessage.WriteToDebugViewer(1, $"Start - Buffer camera:");
                image.Dispatcher.Invoke(() =>
                {
                    btmSource = BitmapSource.Create(_imageWidth, _imageHeight, _dpi, _dpi,
                        PixelFormats.Gray8, BitmapPalettes.Gray256, buff, (PixelFormats.Gray8.BitsPerPixel + 7) / 8 * _imageWidth);
                    image.Source = btmSource;
                });
                //DebugMessage.WriteToDebugViewer(1, $"enableGray: {enableGray}");
            }
        }





        public byte[] ConvertMonoToBinary()
        {
            long count = 0;
            lock (bufferImage)
            {
                if (bufferImage != null)
                {
                    byte[] binarizeBufferImage = new byte[bufferImage.Length];
                    foreach (byte a in bufferImage)
                    {
                        binarizeBufferImage[count++] = a < valueSliderGray ? (byte)0 : (byte)255;
                    }
                    return binarizeBufferImage;
                }
                return new byte[0];
            }
        }
        public void UpdateSourceImageColor(bool isload = false)
        {
            //DebugMessage.WriteToDebugViewer(1, $"Width of track: {_imageWidth}");
            //DebugMessage.WriteToDebugViewer(1, $"Height of track: {_imageHeight}");
            lock (bufferImage)
            {
                if (isload)
                {
                    try
                    {
                        if (bufferImage != null)
                        {
                            if (enableGray > 1)
                            {
                                image.Source = ConvertColorToBinary();
                            }
                            else
                            {
                                btmSource = BitmapSource.Create(_imageWidth, _imageHeight, _dpi, _dpi, PixelFormats.Bgr32, null, bufferImage, _stride);
                                image.Source = btmSource;
                            }
                        }
                    }
                    catch
                    {
                        //DebugMessage.WriteToDebugViewer(0, string.Format("Update Color Image: {0}", ex.ToString()));
                    }
                }
                else
                {
                    try
                    {
                        if (bufferImage != null)
                        {
                            if (enableGray > 1)
                            {
                                image.Source = ConvertColorToBinary();
                            }
                            else
                            {
                                btmSource = BitmapSource.Create(_imageWidth, _imageHeight, _dpi, _dpi, PixelFormats.Bgr24, null, bufferImage, _stride);
                                image.Source = btmSource;
                            }

                        }
                    }
                    catch
                    {
                        //DebugMessage.WriteToDebugViewer(0, string.Format("Update Color Image: {0}", ex.ToString()));
                    }
                }
            }
        }
        #endregion

        #region Setup Grid
        public void CreateGrid()
        {
            isSetupGrid = true;
            Line l1 = new Line();
            l1.Stroke = System.Windows.Media.Brushes.Red;
            l1.StrokeThickness = 2;

            l1.X1 = 0; l1.Y1 = GridCoordinate.Height / 2;
            l1.X2 = GridCoordinate.Width; l1.Y2 = l1.Y1;

            Line l11 = new Line();
            l11.Stroke = System.Windows.Media.Brushes.Red;
            l11.StrokeThickness = 2;
            l11.X1 = GridCoordinate.Width / 2; l11.Y1 = 0;
            l11.X2 = l11.X1; l11.Y2 = GridCoordinate.Height;
            double acw = GridCoordinate.Width, ach = GridCoordinate.Height;
            double iw = 0; double ih = 0;
            while (iw <= acw)
            {
                Line v = new Line();
                v.Stroke = System.Windows.Media.Brushes.Green;
                v.StrokeThickness = 1;

                v.Y1 = 0; v.Y2 = ach;
                v.X1 = iw; v.X2 = v.X1;
                iw += (acw / 10);
                GridCoordinate.Children.Add(v);
            }
            while (ih <= ach)
            {
                Line h = new Line();
                h.Stroke = System.Windows.Media.Brushes.Green;
                h.StrokeThickness = 1;

                h.X1 = 0; h.X2 = acw;
                h.Y1 = ih; h.Y2 = ih;

                ih += (ach / 10);
                GridCoordinate.Children.Add(h);
            }
            GridCoordinate.Children.Add(l1);
            GridCoordinate.Children.Add(l11);
        }
        public void HiddenGrid()
        {
            isSetupGrid = false;
            GridCoordinate.Children.Clear();
        }

        #endregion

        #region Drawing 

        public bool DrawString(string text, int X, int Y, SolidColorBrush brushColor, int fontsize = 21)
        {

            Results result = new Results(text, brushColor, fontsize);
            //if (results.Count == 0)
            //    result.y = 45;
            //else
            //    result.y = results.Last().y + 28;
            //results.Add(result);
            TextBlock textBlock = new TextBlock();
            textBlock.Text = result.text;
            textBlock.FontSize = result.fontSize;
            textBlock.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Bold");
            textBlock.Foreground = result.color;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.FontWeight = FontWeights.ExtraBold;

            Canvas.SetLeft(textBlock, X);
            Canvas.SetTop(textBlock, Y);
            this.Dispatcher.Invoke(delegate
            {
                GridResult.Children.Add(textBlock);
            });
            return true;
        }
        public bool DrawStringInfor(string text, System.Windows.Media.Brush color, bool B = false, int fontsize = 17)

        {
            Results result = new Results(text, color, fontsize);
            if (results.Count == 0)
                result.y = 100;
            else
                result.y = results.Last().y + 30;
            results.Add(result);
            TextBlock textBlock = new TextBlock();
            textBlock.Text = result.text;
            textBlock.FontSize = result.fontSize;
            textBlock.FontFamily = new System.Windows.Media.FontFamily("Helvetica");
            textBlock.Foreground = result.color;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            Canvas.SetLeft(textBlock, result.x);
            Canvas.SetTop(textBlock, result.y);
            if (B)
            {
                textBlock.FontWeight = FontWeights.ExtraBold;
            }
            this.Dispatcher.Invoke(delegate
            {
                GridResult.Children.Add(textBlock);
            });
            return true;
        }
        // Draw Pass Fail

        public void DrawLineOverLay(ref LineArray array_edges)
        {
            //try
            //{
            //    if (trackID == 0)
            SolidColorBrush color = new SolidColorBrush(Colors.Yellow);
            for (int n = 0; n < array_edges.Count(); n++)
            {
                EDDrawingOverlay.DrawLine(trackID, GridOverlay, array_edges[n].P1, array_edges[n].P2, color);
            }
            //    else
            //    {
            //        DrawingOverlay.DrawAllInspectionResultFS(GridOverlay, trackID);
            //    }
            //}
            //catch (Exception)
            //{
            //    //DebugMessage.WriteToDebugViewer(7, "Draw All Overlay Error");
            //}
        }

        public void DrawCrossPointOverlay(ref System.Drawing.Point pCenter)
        {
            SolidColorBrush color = new SolidColorBrush(Colors.Yellow);
            List<System.Drawing.Point> p1 = new List<System.Drawing.Point>();
            p1.Add(new System.Drawing.Point(pCenter.X - 5, pCenter.Y));
            p1.Add(new System.Drawing.Point(pCenter.X + 5, pCenter.Y));
            DrawPolygonOverlay(ref p1, color, 1);
            List<System.Drawing.Point> p2 = new List<System.Drawing.Point>();
            p2.Add(new System.Drawing.Point(pCenter.X, pCenter.Y - 5));
            p2.Add(new System.Drawing.Point(pCenter.X, pCenter.Y + 5));
            DrawPolygonOverlay(ref p2, color, 1);

        }
        public void DrawPolygonOverlay(ref List<System.Drawing.Point> polygon_Input, SolidColorBrush color, int nLineWidth = 1)
        {
            try
            {
                EDDrawingOverlay.DrawPolygon(trackID,GridOverlay, polygon_Input, color, nLineWidth);
            }
            catch { }

        }

        public void DrawStringOverlay(string text, int X, int Y, SolidColorBrush brushColor, int fontsize = 21)
        {
            try
            {
                EDDrawingOverlay.DrawString(trackID,GridOverlay, text, X, Y, brushColor, fontsize);
            }
            catch { }
        }

        public void DrawRegionOverlay(Mat mat_Region, SolidColorBrush color)
        {
            //SolidColorBrush color = new SolidColorBrush(Colors.Cyan);
            try
            {
                EDDrawingOverlay.DrawRegion(GridOverlay, trackID, mat_Region, color, 1);
            }
            catch { }
        }

        public void DrawRectangle(Rectangles rec, System.Windows.Media.Brush color)
        {
            try
            {
                Rectangle recUI = new Rectangle();
                double scaleX = image.ActualWidth / ((BitmapSource)image.Source).PixelWidth;
                double scaleY = image.ActualHeight / ((BitmapSource)image.Source).PixelHeight;
                recUI.Width = rec.Width * scaleX;
                recUI.Height = rec.Height * scaleY;
                recUI.Stroke = System.Windows.Media.Brushes.Green;
                recUI.StrokeThickness = 3;
                //a.Fill = System.Windows.Media.Brushes.White;
                Canvas.SetTop(recUI, rec.TopLeft.Y * scaleY);
                Canvas.SetLeft(recUI, rec.TopLeft.X * scaleX);
                GridOverlay.Children.Add(recUI);
            }
            catch { }

        }
        public bool ClearOverlay()
        {
            resultTeach.Children.Clear();
            image.Dispatcher.Invoke((Action)delegate
            {
                GridOverlay.Children.Clear();
                GridCoordinate.Children.Clear();
            });
            return true;
        }
        public bool ClearText()
        {
            image.Dispatcher.Invoke((Action)delegate
            {
                results.Clear();
                GridResult.Children.Clear();
            });
            return true;
        }
        #endregion

        #region Binarize
        private void CheckTextChange(object sender, TextChangedEventArgs e)
        {
            string str = (sender as TextBox).Text;
            if (str != "" && (sender as TextBox).IsFocused)
                try
                {
                    int x = int.Parse(str.Split('.')[0]);
                    if (x > 255)
                        (sender as TextBox).Text = "255";
                    else if (x < 0)
                        (sender as TextBox).Text = "0";
                }
                catch
                {
                    (sender as TextBox).Text = (sender as TextBox).Text.Remove((sender as TextBox).Text.Count() - 1, 1);
                }
        }
        private void HigherValueChangedRange(object sender, RoutedEventArgs e)
        {
            ShowBinaryImage();
        }

        private void LowerValueChangedRange(object sender, RoutedEventArgs e)
        {
            ShowBinaryImage();
        }

        private BitmapSource ConvertColorToBinary()
        {
            List<int> _lowThresh = new List<int>();
            List<int> _upperThesh = new List<int>();
            //_lowThresh.Add((int)Stamp_rangeSlider_H.LowerValue);
            //_lowThresh.Add((int)Stamp_rangeSlider_S.LowerValue);
            //_lowThresh.Add((int)Stamp_rangeSlider_V.LowerValue);

            //_upperThesh.Add((int)Stamp_rangeSlider_H.HigherValue);
            //_upperThesh.Add((int)Stamp_rangeSlider_S.HigherValue);
            //_upperThesh.Add((int)Stamp_rangeSlider_V.HigherValue);

            CvImage imgBrg = new CvImage();
            CvImage imgHsv = new CvImage();
            CvImage imgThreshold = new CvImage();
            imgBrg = BitmapSourceConvert.ToMat(btmSource);

            //CvInvoke.CvtColor(imgBrg, imgHsv, ColorConversion.Bgr2Hsv);
            //EmageCVLib.Threshold2(ref imgHsv, ref imgThreshold, _lowThresh, _upperThesh);

            return BitmapSourceConvert.ToBitmapSource(imgBrg);
        }

        public void ShowBinaryImage()
        {
            if (bufferImage != null)
                image.Source = ConvertColorToBinary();
        }
        #endregion

        private System.Windows.Point _startPositionDlg;
        private System.Windows.Vector _startOffsetPositionDlg;
        private void grd_Defect_Settings_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _startPositionDlg = e.GetPosition(this);
            if (_startPositionDlg.X != 0 && _startPositionDlg.Y != 0)
            {
                _startOffsetPositionDlg = new System.Windows.Vector(tt_DefectSettings.X, tt_DefectSettings.Y);
                grd_Defect_Settings.CaptureMouse();
            }


        }

        private void grd_Defect_Settings_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (grd_Defect_Settings.IsMouseCaptured)
            {
                System.Windows.Vector offset = System.Windows.Point.Subtract(e.GetPosition(this), _startPositionDlg);
                tt_DefectSettings.X = _startOffsetPositionDlg.X + offset.X;
                tt_DefectSettings.Y = _startOffsetPositionDlg.Y + offset.Y;
            }
        }
        private void grd_Defect_Settings_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            grd_Defect_Settings.ReleaseMouseCapture();
        }

        #region Update RGB Value and Gray Value
        //private bool positionInImage = false;
        private void GetCoordinateInImage(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Point a = e.GetPosition(image);
            if (image.Source == null)
                return;
            if (bufferImage == null)
                return;
            //if (trackID == 0)
            //{
            //BitmapSource currentSource = (BitmapSource)image.Source;
            //CvImage imgBgr = BitmapSourceConvert.ToMat(currentSource);
            //positionInImage = true;
            //int iw = ((BitmapSource)image.Source).PixelWidth;
            //int ih = ((BitmapSource)image.Source).PixelHeight;
            //int stride = iw * 4;
            //byte[] pixels = new byte[stride * ih];
            //((BitmapSource)image.Source).CopyPixels(pixels, stride, 0);

            //double scaleX = image.ActualWidth / ((BitmapSource)image.Source).PixelWidth;
            //double scaleY = image.ActualHeight / ((BitmapSource)image.Source).PixelHeight;

            //if ((int)(a.X / scaleX) >= ((BitmapSource)image.Source).PixelWidth - 15 || (int)(a.Y / scaleY) >= ((BitmapSource)image.Source).PixelHeight - 15)
            //    positionInImage = false;

            //int diX = (int)(a.X / scaleX) >= ((BitmapSource)image.Source).PixelWidth ? ((BitmapSource)image.Source).PixelWidth - 1 : (int)(a.X / scaleX);
            //int diY = (int)(a.Y / scaleY) >= ((BitmapSource)image.Source).PixelHeight ? ((BitmapSource)image.Source).PixelHeight - 1 : (int)(a.Y / scaleY);
            //int index = diY * (stride) + diX;

            //byte red = pixels[index < 0 ? 0 : index > stride - 1 ? 0 : index];   // index overside
            //System.Drawing.Color color = new System.Drawing.Color();
            //color = imgBgr.Bitmap.GetPixel(diX, diY);
            //System.Drawing.Color colorGray = new System.Drawing.Color();
            //CvImage imgGray = new CvImage();
            //CvInvoke.CvtColor(imgBgr, imgGray, Emgu.CV.CvEnum.ColorConversion.Rgb2Gray);
            //colorGray = imgGray.Bitmap.GetPixel(diX, diY);
            //((MainWindow)(System.Windows.Application.Current.MainWindow)).UpdateRGBValue("[" + diX.ToString() + ", " + diY.ToString() + "]",
            //                                                                "[" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "]",
            //                                                                "[" + colorGray.R.ToString() + "]");
            ////}
            //else if (trackID == 1 || trackID == 2)
            //{
            int stride = _imageWidth;
            int size = _imageHeight * stride;

            double scaleX = image.ActualWidth / _imageWidth;
            double scaleY = image.ActualHeight / _imageHeight;

            int diX = (int)(a.X / scaleX) >= _imageWidth ? _imageWidth - 1 : (int)(a.X / scaleX);
            int diY = (int)(a.Y / scaleY) >= _imageHeight ? _imageHeight - 1 : (int)(a.Y / scaleY);
            int index = diY * (stride) + diX;

            System.Drawing.PointF robotPoint = new System.Drawing.PointF(0, 0);
            if (MainWindow.mainWindow.master != null)
                robotPoint = Track.MagnusMatrix.ApplyTransformation(MainWindow.mainWindow.master.m_hiWinRobotInterface.m_hiWinRobotUserControl.m_MatCameraRobotTransform,  new System.Drawing.PointF(diX , diY));

            byte red = bufferImage[index < 0 ? 0 : index > size - 1 ? 0 : index];   // index overside
            ((MainWindow)(System.Windows.Application.Current.MainWindow)).UpdateGrayValue(trackID,
                                                                            "[" + diX.ToString() + ", " + diY.ToString() + "]" + " Robot[" + robotPoint.X.ToString() + ", " + robotPoint.Y.ToString() + "]" ,
                                                                            "[" + red.ToString() + "]");
            //}
        }
        #endregion
        private void GridOverlaySizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReDraw();
        }
        public void ReDraw()
        {
            if (_imageWidth == 0 || _imageHeight == 0)
                return;
            double newScaleX = GridOverlay.Width / _imageWidth;
            double newScaleY = GridOverlay.Height / _imageHeight;

            double oldScaleX = OldWidth / _imageWidth;
            double oldScaleY = OldHeight / _imageHeight;

            OldWidth = GridOverlay.Width;
            OldHeight = GridOverlay.Height;

            UIElement[] uIElement = new UIElement[GridOverlay.Children.Count];

            GridOverlay.Children.CopyTo(uIElement, 0);
            GridOverlay.Children.Clear();
            for (int i = 0; i < uIElement.Length; i++)
            {
                GridOverlay.Dispatcher.Invoke(() =>
                {
                    string namee = uIElement[i].GetType().Name;
                    if (uIElement[i].GetType().Name == "Polygon")
                    {
                        Polygon polygon = new Polygon();
                        polygon.StrokeThickness = ((Polygon)uIElement[i]).StrokeThickness;
                        polygon.Stroke = ((Polygon)uIElement[i]).Stroke;
                        foreach (System.Windows.Point point in ((Polygon)uIElement[i]).Points)
                        {
                            System.Windows.Point scaledPoint = new System.Windows.Point(point.X * (newScaleX / oldScaleX), point.Y * (newScaleY / oldScaleY));
                            polygon.Points.Add(scaledPoint);
                        }
                        GridOverlay.Children.Add(polygon);
                    }
                    else if (uIElement[i].GetType().Name == "Rectangle")
                    {
                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();

                        Canvas.SetTop(rect, Canvas.GetTop((System.Windows.Shapes.Rectangle)uIElement[i]) * (newScaleX / oldScaleX));
                        Canvas.SetLeft(rect, Canvas.GetLeft((System.Windows.Shapes.Rectangle)uIElement[i]) * (newScaleY / oldScaleY));
                        rect.Stroke = ((System.Windows.Shapes.Rectangle)uIElement[i]).Stroke;
                        rect.StrokeThickness = ((System.Windows.Shapes.Rectangle)uIElement[i]).StrokeThickness;
                        rect.Width = ((System.Windows.Shapes.Rectangle)uIElement[i]).Width * (newScaleX / oldScaleX);
                        rect.Height = ((System.Windows.Shapes.Rectangle)uIElement[i]).Height * (newScaleX / oldScaleX);

                        GridOverlay.Children.Add(rect);
                    }
                    else if (uIElement[i].GetType().Name == "Line")
                    {
                        System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
                        line.Stroke = ((Line)uIElement[i]).Stroke;
                        line.StrokeThickness = ((Line)uIElement[i]).StrokeThickness;
                        line.X1 = ((Line)uIElement[i]).X1 * (newScaleX / oldScaleX);
                        line.Y1 = ((Line)uIElement[i]).Y1 * (newScaleX / oldScaleX);

                        line.X2 = ((Line)uIElement[i]).X2 * (newScaleX / oldScaleX);
                        line.Y2 = ((Line)uIElement[i]).Y2 * (newScaleX / oldScaleX);

                        GridOverlay.Children.Add(line);
                    }
                    else if (uIElement[i].GetType().Name == "TextBlock")
                    {
                        double X = Canvas.GetLeft(uIElement[i]);
                        double Y = Canvas.GetTop(uIElement[i]);
                        Canvas.SetLeft(uIElement[i], X * (newScaleX / oldScaleX));
                        Canvas.SetTop(uIElement[i],  Y * (newScaleX / oldScaleX));
                        GridOverlay.Dispatcher.Invoke(delegate
                        {
                            GridOverlay.Children.Add(uIElement[i]);
                        });
                    }
                    

                });
            }
            if (isSetupGrid)
            {
                GridCoordinate.Children.Clear();
                CreateGrid();
            }
        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //GridOverlay.Children.Clear();
            //TextBlock b = new TextBlock();
            //b.Text = "Magnus";
            //b.FontSize = 20;
            //b.FontWeight = FontWeights.Bold;
            //Canvas.SetTop(b, e.GetPosition(image).Y);
            //Canvas.SetLeft(b, e.GetPosition(image).X);
            //GridOverlay.Children.Add(b);

            //Rectangle a = new Rectangle();
            //a.Width = 100;
            //a.Height = 100;
            //a.Stroke = System.Windows.Media.Brushes.Green;
            //a.StrokeThickness = 3;
            ////a.Fill = System.Windows.Media.Brushes.White;
            //Canvas.SetTop(a, e.GetPosition(image).Y);
            //Canvas.SetLeft(a, e.GetPosition(image).X);
            //GridOverlay.Children.Add(a);

        }
        public void UpdateTextOverlay(string status1, string status2, System.Windows.Media.Brush color1, System.Windows.Media.Brush color2, bool isFirstStep = false)
        {
            resultTeach.Children.Clear();

            tbl_Status2.Foreground = color2;
            if (isFirstStep)
            {
                SolidColorBrush solidColorBrush = new SolidColorBrush();
                solidColorBrush.Color = Colors.Crimson;
                solidColorBrush.Opacity = 0.3;
                tbl_Status1.Foreground = color1;
                tbl_Status1.Background = solidColorBrush;
                tbl_Status1.Opacity = 1;
                tbl_Status1.Margin = new Thickness(-10, 20, 0, 0);
                tbl_Status1.Text = status1;
                tbl_Status1.HorizontalAlignment = HorizontalAlignment.Left;
                tbl_Status1.VerticalAlignment = VerticalAlignment.Center;
                tbl_Status2.Text = status2;
            }
            else
            {
                tbl_Status1.Margin = new Thickness(0, 0, 0, 0);
                SolidColorBrush solidColorBrush = new SolidColorBrush();
                solidColorBrush.Color = Colors.Transparent;
                tbl_Status1.Background = solidColorBrush;
                tbl_Status1.Foreground = color1;
                tbl_Status1.Text = status1;
                tbl_Status2.Text = status2;
            }
            resultTeach.Children.Add(tbl_Status1);
        }

        public void loadTeachImageToUI(int nTrack)
        {

            string strImagePath = System.IO.Path.Combine(Source.Application.Application.pathRecipe, Source.Application.Application.currentRecipe, "teachImage_Track" + (nTrack+1).ToString() +".bmp");
            if (!File.Exists(strImagePath))
                return;
            else
            {
                string[] nameImageArray = strImagePath.Split('\\');
                int leght = nameImageArray.Count();
                string _nameImage = nameImageArray[leght - 1];
                nameImage = _nameImage;
                //Color Image
                //BitmapImage bitmap = new BitmapImage();
                //bitmap = MainWindow.mainWindow.master.LoadBitmap(strImagePath);
                //UpdateNewImage(bitmap);
                UpdateNewImageMono(strImagePath);
                GridOverlay.Children.Clear();
                UpdateTextOverlay("", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                MainWindow.mainWindow.UpdateTitleDoc(0, "teachImage.bmp", true);
            }
        }


        //List<Rectangles> TP_roiNo_Temp = new List<Rectangles>();
        //Rectangles L_DeviceLocationRoi_Temp = new Rectangles();
        //Rectangles L_TemplateRoi_Temp = new Rectangles();
        //int L_CornerIndex_Temp = 0;
        //public int Teach(int nCurrentTeachingStep = 0)
        //{
        //    System.Windows.Shapes.Rectangle rec = new System.Windows.Shapes.Rectangle();
        //    double scaleX = image.ActualWidth / ((BitmapSource)image.Source).PixelWidth;
        //    double scaleY = image.ActualHeight / ((BitmapSource)image.Source).PixelHeight;
        //    // now only teach device location so only TP_roiNo
        //    switch (nCurrentTeachingStep)
        //    {
        //        //case (int)TEACHSTEP.TEACH_LOADIMAGE:
        //        //    GridOverlay.Children.Clear();
        //        //    UpdateTextOverlay("[" + (nCurrentTeachingStep + 1).ToString() + "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString() + "] Teach Imaged Loaded, Press 'Next' to continue! ", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
        //        //    break;

        //        case (int)TEACHSTEP.TEACH_DEVICELOCATION:
        //            GridOverlay.Children.Clear();
        //            UpdateTextOverlay("[" + (nCurrentTeachingStep + 1).ToString() + "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString() + "] Please Locate Searching Area", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
        //            controlWin.Visibility = Visibility.Visible;
        //            SetControlWin(Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi);
        //            break;

        //        case (int)TEACHSTEP.TEACH_DEVICELOCATION_TEACHED:
        //            L_DeviceLocationRoi_Temp = GetRectangle();
        //            controlWin.Visibility = Visibility.Collapsed;
        //            UpdateTextOverlay("[" + (nCurrentTeachingStep + 1).ToString() + "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString() + "] Searching Area Taught", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);

        //            rec.Width = GetRectangle().Width * scaleX;
        //            rec.Height = GetRectangle().Height * scaleY;
        //            rec.Stroke = System.Windows.Media.Brushes.Green;
        //            rec.StrokeThickness = 3;
        //            //a.Fill = System.Windows.Media.Brushes.White;
        //            Canvas.SetTop(rec, GetRectangle().TopLeft.Y * scaleY);
        //            Canvas.SetLeft(rec, GetRectangle().TopLeft.X * scaleX);
        //            GridOverlay.Children.Add(rec);
        //            break;

        //        case (int)TEACHSTEP.TEACH_TEMPLATEROI:
        //            UpdateTextOverlay("[" + (nCurrentTeachingStep + 1).ToString() + "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString() + "] Please Locate Device BoundingBox", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
        //            controlWin.Visibility = Visibility.Visible;
        //            SetControlWin(Source.Application.Application.categoriesTeachParam.L_TemplateRoi);
        //            break;

        //        case (int)TEACHSTEP.TEACH_TEMPLATEROI_TEACHED:
        //            L_TemplateRoi_Temp = GetRectangle();
        //            List<System.Drawing.Point> p_Regionpolygon = new List<System.Drawing.Point>();
        //            Mat mat_DeviceLocationRegion = new Mat();
        //            double nAngleOutput = 0.0;
        //            double dScoreOutput = 0.0;
        //            System.Drawing.Rectangle rectMatchingPosition = new System.Drawing.Rectangle();
        //            InspectionCore.AutoTeachDatumLocation(ref p_Regionpolygon, L_DeviceLocationRoi_Temp, L_TemplateRoi_Temp, ref mat_DeviceLocationRegion, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput, ref L_CornerIndex_Temp);

        //            controlWin.Visibility = Visibility.Collapsed;
        //            //UpdateTextOverlay("[" + (nCurrentTeachingStep + 1).ToString() + "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString() + "Device Found, Corner = " + L_CornerIndex_Temp.ToString() + "]", "" , DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
        //            UpdateTextOverlay( (nCurrentTeachingStep + 1).ToString() + "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString() + "  Device Found", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);

        //            rec.Width = GetRectangle().Width * scaleX;
        //            rec.Height = GetRectangle().Height * scaleY;
        //            rec.Stroke = System.Windows.Media.Brushes.Green;
        //            rec.StrokeThickness = 3;
        //            //a.Fill = System.Windows.Media.Brushes.White;
        //            Canvas.SetTop(rec, GetRectangle().TopLeft.Y * scaleY);
        //            Canvas.SetLeft(rec, GetRectangle().TopLeft.X * scaleX);
        //            GridOverlay.Children.Add(rec);
        //            SolidColorBrush color = new SolidColorBrush(Colors.Yellow);

        //            DrawPolygonOverlay(ref p_Regionpolygon, color, 1);
        //            DrawRegionOverlay(ref mat_DeviceLocationRegion);
        //            break;
        //    }           
        //    return 0;
        //}
        private int waitNextTeachStep()
        {
            while (!Master.m_NextStepTeachEvent.WaitOne(100))
            {
                if (MainWindow.mainWindow == null || !Master.m_bIsTeaching)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        controlWin.Visibility = Visibility.Collapsed;
                        MainWindow.mainWindow.SetDisableTeachButton();
                        MainWindow.mainWindow.master.loadTeachImageToUI(trackID);
                        Master.m_bIsTeaching = false;

                    });
                    return -1;
                }
            }
            return 0;
        }

        List<Rectangles> L_PVIArea = new List<Rectangles>();
        public void func_TeachSequence(int nTeachTrackID)
        {
            Master.m_bIsTeaching = true;
            Master.m_NextStepTeachEvent.Reset();
            Source.Application.Application.LoadTeachParamFromFileToDict(ref nTeachTrackID);
            //MainWindow.mainWindow.master.m_Tracks[nTeachTrackID].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {

                resultTeach.Children.Clear();
                ClearOverlay();
                loadTeachImageToUI(nTeachTrackID);
                MainWindow.mainWindow.master.teachParameter.UpdateTeachParamFromDictToUI(Source.Application.Application.dictTeachParam);

            });

            // now only teach device location so only TP_roiNo
            int nCurrentStep = 0;
            Rectangles L_DeviceLocationRoi_Temp = Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi;
            //if (L_DeviceLocationRoi_Temp.Width > _imageWidth)
            //{
            //    L_DeviceLocationRoi_Temp.Width = _imageWidth;
            //    L_DeviceLocationRoi_Temp.Height = _imageHeight;
            //}

            if (Source.Application.Application.categoriesTeachParam.DR_DefectROILocations.Count < (int)AREA_INDEX.TOTAL_AREA)
            {
                Source.Application.Application.categoriesTeachParam.DR_DefectROILocations.Clear();
                for (int n = 0; n < (int)AREA_INDEX.TOTAL_AREA; n++)
                {
                    Source.Application.Application.categoriesTeachParam.DR_DefectROILocations.Add(new Rectangles(new Point(100, 100), 100, 100));
                }
            }

            if (true)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    UpdateTextOverlay("[" + (nCurrentStep + 1).ToString() /*+ "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString()*/ + "] Please Locate Searching Area", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                    controlWin.Visibility = Visibility.Visible;
                    SetControlWin(Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi);
                });

                if (waitNextTeachStep() < 0)
                    return;

                nCurrentStep++;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    L_DeviceLocationRoi_Temp = GetRectangle();
                    Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi = L_DeviceLocationRoi_Temp;
                    controlWin.Visibility = Visibility.Collapsed;
                    UpdateTextOverlay("[" + (nCurrentStep + 1).ToString() /*+ "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString()*/ + "] Searching Area Taught", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                    UpdateRegionOverlay();

                });

                if (waitNextTeachStep() < 0)
                    return;

                nCurrentStep++;
            }
            if (true)
            {
                if (L_PVIArea.Count() < Source.Application.Application.categoriesTeachParam.DR_DefectROILocations.Count)
                {
                    L_PVIArea.Clear();

                    for (int nPVIArea = 0; nPVIArea < Source.Application.Application.categoriesTeachParam.DR_DefectROILocations.Count; nPVIArea++)
                    {
                        L_PVIArea.Add(Source.Application.Application.categoriesTeachParam.DR_DefectROILocations[nPVIArea]);
                    }
                }
                for (int nPVIArea = 0; nPVIArea < Source.Application.Application.categoriesTeachParam.DR_NumberROILocation; nPVIArea++)
                {
                    Rectangles lPVIAreaTemp = L_PVIArea[nPVIArea];
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        UpdateTextOverlay("[" + (nCurrentStep + 1).ToString() /*+ "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString()*/ + "] Please Locate Pvi Area " + nPVIArea.ToString(), "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                        controlWin.Visibility = Visibility.Visible;
                        SetControlWin(lPVIAreaTemp);
                    });

                    if (waitNextTeachStep() < 0)
                        return;

                    nCurrentStep++;

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        L_PVIArea[nPVIArea] = GetRectangle();
                        //Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi = L_DeviceLocationRoi_Temp;
                        controlWin.Visibility = Visibility.Collapsed;
                        UpdateTextOverlay("[" + (nCurrentStep + 1).ToString() /*+ "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString()*/ + "] Pvi Area " + nPVIArea.ToString() + " is Taught", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                        UpdateRegionOverlay();

                    });

                    if (waitNextTeachStep() < 0)
                        return;

                    nCurrentStep++;
                }
            }

            if (true)
            {
                // Teach Region 2
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    UpdateTextOverlay("[" + (nCurrentStep + 1).ToString() /*+ "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString()*/ + "] Please Locate Device BoundingBox", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                    controlWin.Visibility = Visibility.Visible;
                    SetControlWin(Source.Application.Application.categoriesTeachParam.L_TemplateRoi);
                });

                if (waitNextTeachStep() < 0)
                    return;

                nCurrentStep++;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Rectangles L_TemplateRoi_Temp = GetRectangle();
                    controlWin.Visibility = Visibility.Collapsed;

                    //List<System.Drawing.Point> p_Regionpolygon = new List<System.Drawing.Point>();
                    //Mat mat_DeviceLocationRegion = new Mat();
                    //double nAngleOutput = 0.0;
                    //double dScoreOutput = 0.0;
                    //int L_CornerIndex_Temp = 0;
                    //System.Drawing.Rectangle rectMatchingPosition = new System.Drawing.Rectangle();
                    //MainWindow.mainWindow.master.m_Tracks[nTeachTrackID].m_InspectionCore.AutoTeachDatumLocation(ref p_Regionpolygon, L_DeviceLocationRoi_Temp, L_TemplateRoi_Temp, ref mat_DeviceLocationRegion, ref rectMatchingPosition, ref nAngleOutput, ref dScoreOutput, ref L_CornerIndex_Temp);
                    Source.Application.Application.categoriesTeachParam.L_TemplateRoi = L_TemplateRoi_Temp;
                    //Source.Application.Application.categoriesTeachParam.L_CornerIndex = L_CornerIndex_Temp;
                    MainWindow.mainWindow.master.m_Tracks[nTeachTrackID].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                    MainWindow.mainWindow.master.m_Tracks[nTeachTrackID].m_InspectionCore.LoadTeachImageToInspectionCore(nTeachTrackID);
                    MainWindow.mainWindow.master.m_Tracks[nTeachTrackID].AutoTeach(ref MainWindow.mainWindow.master.m_Tracks[nTeachTrackID], true);
                    //SolidColorBrush color = new SolidColorBrush(Colors.Yellow);
                    //DrawPolygonOverlay(ref p_Regionpolygon, color, 1);
                    //DrawRegionOverlay(mat_DeviceLocationRegion, color);

                    UpdateTextOverlay((nCurrentStep + 1).ToString()/* + "/" + ((int)(TEACHSTEP.TEACH_TOTALSTEP)).ToString()*/ + "  Device Found", "", DefautTeachingSequence.ColorContentTeached, DefautTeachingSequence.ColorExplaintionTeahing);
                    UpdateRegionOverlay();
                });

                if (waitNextTeachStep() < 0)
                    return;

                nCurrentStep++;
            }
            // Save Parameter
            var result = MessageBox.Show("Do you want to save teach parameter ?", "Save Teach Parameter", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                for (int n = 0; n < (int)AREA_INDEX.TOTAL_AREA; n++)
                {
                    Source.Application.Application.categoriesTeachParam.DR_DefectROILocations[n] = L_PVIArea[n];
                }
                //SetTeachParameterToCategories();
                MainWindow.mainWindow.master.m_Tracks[nTeachTrackID].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                MainWindow.mainWindow.master.m_Tracks[nTeachTrackID].m_InspectionCore.SetTemplateImage();
                MainWindow.mainWindow.master.SaveTemplateImage(nTeachTrackID);
                MainWindow.mainWindow.master.WriteTeachParam(nTeachTrackID);
                //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                //{
                //    //resultTeach.Children.Clear();
                //    ClearOverlay();
                //});
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    MainWindow.mainWindow.master.teachParameter.UpdateTeachParamFromDictToUI(Source.Application.Application.dictTeachParam);
                });

                MainWindow.mainWindow.master.m_Tracks[nTeachTrackID].m_InspectionCore.UpdateTeachParamFromUIToInspectionCore();
                MainWindow.mainWindow.master.m_Tracks[nTeachTrackID].m_InspectionCore.LoadTeachImageToInspectionCore(nTeachTrackID);
                //MainWindow.mainWindow.master.LoadRecipe();
            }

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ClearOverlay();
                MainWindow.mainWindow.SetDisableTeachButton();
                MainWindow.mainWindow.master.loadTeachImageToUI(nTeachTrackID);
                Master.m_bIsTeaching = false;

            });
        }


        //public void SetTeachParameterToCategories()
        //{
        //    Source.Application.Application.categoriesTeachParam.L_DeviceLocationRoi = L_DeviceLocationRoi_Temp;
        //    Source.Application.Application.categoriesTeachParam.L_TemplateRoi = L_TemplateRoi_Temp;
        //    Source.Application.Application.categoriesTeachParam.L_CornerIndex = L_CornerIndex_Temp;
        //}

        Rectangles GetRectangle()
        {
            Rectangles rectangles;

            double scaleX = image.ActualWidth / btmSource.Width;
            double scaleY = image.ActualHeight / btmSource.Height;
            rectangles = new Rectangles(new Point(Canvas.GetLeft(controlWin) / scaleX, Canvas.GetTop(controlWin) / scaleY), controlWin.Width / scaleX, controlWin.Height / scaleY);
            return rectangles;
        }

        private void SetControlWin(Rectangles rectangles)
        {
            if (rectangles.Width == 0 && rectangles.Height == 0)
            {
                Canvas.SetLeft(controlWin, gridteach.ActualWidth / 2);
                Canvas.SetTop(controlWin, gridteach.ActualHeight / 2);
                controlWin.Height = 30;
                controlWin.Width = 30;
                return;
            }

            double scaleX = image.ActualWidth / ((BitmapSource)image.Source).PixelWidth;
            double scaleY = image.ActualHeight / ((BitmapSource)image.Source).PixelHeight;

            Canvas.SetLeft(controlWin, rectangles.TopLeft.X * scaleX);
            Canvas.SetTop(controlWin, rectangles.TopLeft.Y * scaleY);
            controlWin.Height = rectangles.Height * scaleX;
            controlWin.Width = rectangles.Width * scaleY;
        }

        private void UpdateRegionOverlay()
        {
            Rectangle rec = new Rectangle();
            double scaleX = image.ActualWidth / ((BitmapSource)image.Source).PixelWidth;
            double scaleY = image.ActualHeight / ((BitmapSource)image.Source).PixelHeight;
            rec.Width = GetRectangle().Width * scaleX;
            rec.Height = GetRectangle().Height * scaleY;
            rec.Stroke = System.Windows.Media.Brushes.Green;
            rec.StrokeThickness = 3;
            Canvas.SetTop(rec, GetRectangle().TopLeft.Y * scaleY);
            Canvas.SetLeft(rec, GetRectangle().TopLeft.X * scaleX);
            GridOverlay.Children.Add(rec);
        }
        //private void SldValueBinaChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    valueSliderGray = (int)sldBinary.Value;
        //    UpdateSourceSliderChangeValue();
        //}
        //public void UpdateSourceSliderChangeValue()
        //{
        //    byte[] bufferConvert = ConvertMonoToBinary();
        //    if (bufferConvert.Length == 0)
        //        return;
        //    image.Source = BitmapSource.Create(_imageWidth, _imageHeight, _dpi, _dpi,
        //        PixelFormats.Gray8, BitmapPalettes.Gray256, bufferConvert, (PixelFormats.Gray8.BitsPerPixel + 7) / 8 * _imageWidth);
        //}

        private void SetupImageDockMouseFeature()
        {
            MouseLeftButtonDown += getChosenDoc;
            PreviewMouseDoubleClick += ImageDoc_DoubleClick;
        }

        private void getChosenDoc(object sender, MouseButtonEventArgs e)
        {
            MainWindow.activeImageDock = sender as ImageView;
            //MainWindow.activeImageDock = this;
            if (MainWindow.activeImageDock.enableGray == 0)
                MainWindow.mainWindow.btn_Binarize.IsChecked = false;
            else
                MainWindow.mainWindow.btn_Binarize.IsChecked = true;
        }

        private void ImageDoc_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.ZoomDocPanel(trackID);
            //getChosenDoc()
        }

        private void SldValueBinaChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            valueSliderGray = (int)sldBinary.Value;
            UpdateSourceSliderChangeValue();
        }
        public void UpdateSourceSliderChangeValue()
        {
            byte[] bufferConvert = ConvertMonoToBinary();
            if (bufferConvert.Length == 0)
                return;
            image.Source = BitmapSource.Create(_imageWidth, _imageHeight, _dpi, _dpi,
                PixelFormats.Gray8, BitmapPalettes.Gray256, bufferConvert, (PixelFormats.Gray8.BitsPerPixel + 7) / 8 * _imageWidth);
        }
    }

    public enum RegionType
    {
        CIRCLE, RECTANGLE, POLYGON
    }

    public class Region
    {
        public string name;
        public RegionType type;
        public System.Windows.Media.Brush color;
        public double thickness;
        public List<System.Windows.Point> pointList;
        public System.Windows.Point leftTop;
        public System.Windows.Point rightBottom;

        public Region(List<System.Windows.Point> pointList, System.Windows.Media.Brush color, RegionType type, double thickness = 1)
        {
            this.pointList = pointList;
            this.color = color;
            this.type = type;
            this.thickness = thickness;
        }
        public Region(System.Windows.Point leftTop, System.Windows.Point rightBottom, System.Windows.Media.Brush color, RegionType type, double thickness = 1)
        {
            this.leftTop = leftTop;
            this.rightBottom = rightBottom;
            this.color = color;
            this.type = type;
            this.thickness = thickness;
        }
    }
    public class Results
    {
        public string text;
        public System.Windows.Media.Brush color;
        public int fontSize;
        public double x, y;
        public Results(string text, System.Windows.Media.Brush color, int fontsize = 21, double x = 10, double y = 15)
        {
            this.text = text;
            this.fontSize = fontsize;
            this.color = color;
            this.x = x;
            this.y = y;
        }
    }



}
