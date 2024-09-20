using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class HIKControlCameraVM:BaseVM, ICustomUserControl
    {
        private DragDropUserControlVM _dragDropVM { set; get; }
        public void RegisterUserControl()
        {
            _dragDropVM.RegisterMoveGrid();
            _dragDropVM.RegisterResizeGrid();
        }

        public HIKControlCameraVM(DragDropUserControlVM dragDropVM)
        {
            _dragDropVM = (DragDropUserControlVM)dragDropVM;
            RegisterUserControl();
        }

    }
}
