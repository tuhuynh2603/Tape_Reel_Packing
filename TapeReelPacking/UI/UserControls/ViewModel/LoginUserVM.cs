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

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class LoginUserVM:BaseVM
    {

        private Visibility _isVisible = Visibility.Visible;
        public Visibility isVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(isVisible));
            }
        }

        public MainWindowVM _mainWindowVM { get; set; }
        public LoginUserVM(MainWindowVM mainVM)
        {
            _mainWindowVM = mainVM;
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
