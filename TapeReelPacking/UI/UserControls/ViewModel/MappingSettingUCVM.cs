using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using TapeReelPacking.Source.Application;
using System.Windows;
using Application = TapeReelPacking.Source.Application.Application;

namespace TapeReelPacking.UI.UserControls.ViewModel
{ 
    public class MappingSetingUCVM : BaseVM
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

        private CatergoryMappingParameters _dataMapping;
        public CatergoryMappingParameters categoriesMappingParam
        {
            set
            {
                _dataMapping = value;
                OnPropertyChanged(nameof(categoriesMappingParam));
            }
            get => _dataMapping;
        }

        public MappingSetingUCVM()
        {
            categoriesMappingParam = Application.categoriesMappingParam;
        }

        [CategoryOrder("MAPPING", 0)]
        [DisplayName("Mapping Setting")]
        public class CatergoryMappingParameters
        {

            #region MAPPING
            [Browsable(true)]
            [Category("MAPPING")]
            [DisplayName("Number Device X")]
            [Range(10, 100)]
            [DefaultValue(10)]
            [Description("")]
            [PropertyOrder(0)]
            public int M_NumberDeviceX { get; set; }
            [Browsable(true)]
            [Category("MAPPING")]
            [DisplayName("Number Device Y")]
            [Range(1, 100)]
            [DefaultValue(10)]
            [Description("")]
            [PropertyOrder(1)]
            public int M_NumberDeviceY { get; set; }

            [Browsable(true)]
            [Category("MAPPING")]
            [DisplayName("Number Device Per Lot")]
            [Range(1, 10000)]
            [DefaultValue(1000)]
            [Description("")]
            [PropertyOrder(2)]
            public int M_NumberDevicePerLot { get; set; }
            #endregion

        }


    }

}
