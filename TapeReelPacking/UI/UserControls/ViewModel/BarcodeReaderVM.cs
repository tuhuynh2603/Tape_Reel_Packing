using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class BarcodeReaderVM : BaseVM, ICustomUserControl
    {
        private DragDropUserControlVM _dragDropVM { set; get; }
        public void RegisterUserControl()
        {
            _dragDropVM.RegisterMoveGrid();
            _dragDropVM.RegisterResizeGrid();
        }

        public BarcodeReaderVM(DragDropUserControlVM dragDropVM) 
        {
            _dragDropVM =  (DragDropUserControlVM)dragDropVM;
            RegisterUserControl();
        }

    }

}
