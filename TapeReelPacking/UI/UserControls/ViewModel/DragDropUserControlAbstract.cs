using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public abstract class DragDropUserControlAbstract:BaseVM
    {

        private BaseVM _CurrentViewModel;

        public BaseVM CurrentViewModel
        {
            get => _CurrentViewModel;
            set
            {
                _CurrentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }


        private Visibility _isVisible = Visibility.Collapsed;
        public Visibility isVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(isVisible));
            }
        }

        #region Init Drag Resize Grid

        private double translateTransformX1;

        public double translateTransformX
        {
            get => translateTransformX1;
            set
            {
                translateTransformX1 = value;
                OnPropertyChanged(nameof(translateTransformX));
            }
        }
        private double translateTransformY1;

        public double translateTransformY
        {
            get => translateTransformY1;
            set
            {
                translateTransformY1 = value;
                OnPropertyChanged(nameof(translateTransformY));
            }
        }

        private double gridWidth = 800;

        public double GridWidth
        {
            get => gridWidth;
            set
            {
                gridWidth = value;
                OnPropertyChanged(nameof(GridWidth));
            }
        }

        private double gridHeight = 400;

        public double GridHeight
        {
            get => gridHeight;
            set
            {
                gridHeight = value;
                OnPropertyChanged(nameof(GridHeight));
            }
        }

        private double leftGrid1;

        public double leftGrid
        {
            get => leftGrid1;
            set
            {
                leftGrid1 = value;
                OnPropertyChanged(nameof(leftGrid));
            }
        }


        private double bottomGrid1;

        public double bottomGrid
        {
            get => bottomGrid1;
            set
            {
                bottomGrid1 = value;
                OnPropertyChanged(nameof(bottomGrid));
            }
        }


        private double topGrid1;

        public double topGrid
        {
            get => topGrid1;
            set
            {
                topGrid1 = value;
                OnPropertyChanged(nameof(topGrid));
            }
        }


        private double rightGrid1;

        public double rightGrid
        {
            get => rightGrid1;
            set
            {
                rightGrid1 = value;
                OnPropertyChanged(nameof(rightGrid));
            }
        }
        public ICommand MouseDownCmd { set; get; }
        public ICommand MouseMoveCmd { set; get; }
        public ICommand MouseUpCmd { set; get; }

        public ICommand ThumbDragDeltaCommandLeft { get; set; }

        public ICommand ThumbDragDeltaCommandRight { get; set; }

        private Point _startWarningPositionDlg;
        private System.Windows.Vector _startOffsetWarningPositionDlg;
        bool bIsMouseCapture = false;
        private void MouseDown(object e)
        {
            _startWarningPositionDlg = Mouse.GetPosition(null);
            if (_startWarningPositionDlg.X != 0 && _startWarningPositionDlg.Y != 0)
            {
                _startOffsetWarningPositionDlg = new Vector(translateTransformX, translateTransformY);
                bIsMouseCapture = true;
                (e as FrameworkElement).Cursor = Cursors.Hand;

                //grd_Warning_Setting.CaptureMouse();
            }
        }
        private void MouseMove(object e)
        {
            if (bIsMouseCapture)
            {
                Vector offset = Point.Subtract(Mouse.GetPosition(null), _startWarningPositionDlg);
                translateTransformX = _startOffsetWarningPositionDlg.X + offset.X;
                translateTransformY = _startOffsetWarningPositionDlg.Y + offset.Y;
            }
        }

        private void MouseUp(object e)
        {
            bIsMouseCapture = false;
            (e as FrameworkElement).Cursor = Cursors.Arrow;
        }

        private void OnThumbDragDeltaLeft(DragDeltaEventArgs e)
        {
            // Handle resizing logic
            leftGrid += e.HorizontalChange;
            bottomGrid += e.VerticalChange;
            GridWidth -= e.HorizontalChange;
            GridHeight += e.VerticalChange;
        }

        private void OnThumbDragDeltaRight(DragDeltaEventArgs e)
        {
            if (bIsMouseCapture)
                return;

            GridWidth += e.HorizontalChange;
            GridHeight += e.VerticalChange;

        }


        public void RegisterMoveGrid()
        {
            MouseDownCmd = new RelayCommand<Grid>(
                (e) => { return true; },

                (e) => { MouseDown(e); }
                );

            MouseMoveCmd = new RelayCommand<Grid>(
                (e) => { return true; },

                (e) => { MouseMove(e); }
                );

            MouseUpCmd = new RelayCommand<Grid>(
                (e) => { return true; },

                (e) => { MouseUp(e); }
                );

        }

        public void RegisterResizeGrid()
        {

            ThumbDragDeltaCommandRight = new RelayCommand<DragDeltaEventArgs>(
                (e) => { return true; },
                (e) => { OnThumbDragDeltaRight(e); }
                );

            ThumbDragDeltaCommandLeft = new RelayCommand<DragDeltaEventArgs>(
                (e) => { return true; },
                (e) => { OnThumbDragDeltaLeft(e); }
                );
        }
        #endregion

        public DragDropUserControlAbstract() {

        }

    }
}
