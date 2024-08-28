using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class SummaryTemplateVM:BaseVM
    {


        private System.Windows.Media.Brush brushRowList;

        public System.Windows.Media.Brush BrushRowList
        {
            get => brushRowList;
            set
            {
                brushRowList = value;
                OnPropertyChanged(nameof(BrushRowList));
            }
        }

        private string nameSummary1;

        public string nameSummary 
        { 
            get => nameSummary1;
            set
            {
                nameSummary1 = value;
                OnPropertyChanged(nameof(nameSummary));
            }
        }

        private System.Windows.Media.Brush color1;

        public System.Windows.Media.Brush color 
        {
            get => color1;
            set
            {
                color1 = value;
                OnPropertyChanged(nameof(color));
            }
        }

        private double valueSummary_Camera21;

        public double valueSummary_Camera2 
        { 
            
            get => valueSummary_Camera21;
            set
            {
                valueSummary_Camera21 = value;
                OnPropertyChanged(nameof(valueSummary_Camera2));
            }

        }

        private double valueSummary_Camera11;

        public double valueSummary_Camera1
        {

            get => valueSummary_Camera11;
            set
            {
                valueSummary_Camera11 = value;
                OnPropertyChanged(nameof(valueSummary_Camera1));
            }

        }

    }
}
