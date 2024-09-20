using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class PixelRulerVM:BaseVM, ICustomUserControl
    {
        public MainWindowVM _mainWindowVM { get; set; }
        private DragDropUserControlVM _dragDropVM { set; get; }
        public void RegisterUserControl()
        {
            _dragDropVM.RegisterMoveGrid();
            _dragDropVM.RegisterResizeGrid();
        }


        public PixelRulerVM(DragDropUserControlVM dragDropVM, MainWindowVM mainVM)
        {
            _mainWindowVM = mainVM;
            _dragDropVM = (DragDropUserControlVM)dragDropVM;
            RegisterUserControl();
        }

    }
}
