using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class HIKRobotVM:BaseVM
    {
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

        public HIKRobotVM()
        {

        }
    }
}
