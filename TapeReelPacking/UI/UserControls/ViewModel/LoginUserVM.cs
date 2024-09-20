using Microsoft.Expression.Interactivity.Core;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using TapeReelPacking.Source.Define;
using TapeReelPacking.Source.LogMessage;
using TapeReelPacking.UI.UserControls.View;
using System.Drawing;
using TapeReelPacking.UI.UserControls.ViewModel;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class LoginUserVM:BaseVM, ICustomUserControl
    {

        public MainWindowVM _mainWindowVM { get; set; }
        private DragDropUserControlVM _dragDropVM { set; get; }
        public void RegisterUserControl()
        {
            _dragDropVM.RegisterMoveGrid();
            _dragDropVM.RegisterResizeGrid();
        }

        public LoginUserVM(DragDropUserControlVM dragDropVM, MainWindowVM mainVM)
        {
            _mainWindowVM = mainVM;
            _dragDropVM = (DragDropUserControlVM)dragDropVM;
            RegisterUserControl();
        }


        private bool _isFocused;
        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                _isFocused = value;
                OnPropertyChanged(nameof(IsFocused));
            }
        }



    }
}
