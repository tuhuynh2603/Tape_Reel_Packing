using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TapeReelPacking.Source.Define;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TapeReelPacking.Source.Model
{
    [CategoryOrder(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, 0)]
    [DisplayName("Vision Parameter")]
    public class CategoryVisionParameter : Prism.Mvvm.BindableBase
    {

        public const string CAETEGORYORDER_LABEL_DEFECT = "LABEL DEFECT";
        [Browsable(false)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Defect ROI Location")]
        [Range(0, 5)]
        [Description("")]
        [PropertyOrder(0)]
        public RectanglesModel LD_DefectROILocation
        {
            get => _LD_DefectROILocation;

            set => SetProperty(ref _LD_DefectROILocation, value);

        }

        private RectanglesModel _LD_DefectROILocation = new RectanglesModel();


        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Area Enable")]
        [PropertyOrder(0)]
        [DefaultValue(false)]
        public bool LD_AreaEnable
        {
            get => _LD_AreaEnable;

            set => SetProperty(ref _LD_AreaEnable, value);

        }
        private bool _LD_AreaEnable;

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Lower Threshold")]
        [Range(0, 255)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(2)]
        public int LD_lowerThreshold
        {
            get => _LD_lowerThreshold;

            set => SetProperty(ref _LD_lowerThreshold, value);

        }
        private int _LD_lowerThreshold;

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Upper Threshold")]
        [Range(0, 255)]
        [DefaultValue(255)]
        [Description("")]
        [PropertyOrder(3)]
        public int LD_upperThreshold
        {
            get => _LD_upperThreshold;

            set => SetProperty(ref _LD_upperThreshold, value);

        }
        private int _LD_upperThreshold;

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Opening Mask")]
        [Range(0, 100)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(4)]
        public int LD_OpeningMask
        {
            get => _LD_OpeningMask;

            set => SetProperty(ref _LD_OpeningMask, value);

        }
        private int _LD_OpeningMask;

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Dilation Mask")]
        [Range(0, 100)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(5)]
        public int LD_DilationMask
        {
            get => _LD_DilationMask;

            set => SetProperty(ref _LD_DilationMask, value);

        }
        private int _LD_DilationMask;

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Object Cover PerCent")]
        [Range(0, 100)]
        [DefaultValue(50)]
        [Description("")]
        [PropertyOrder(6)]
        public int LD_ObjectCoverPercent
        {
            get => _LD_ObjectCoverPercent;

            set => SetProperty(ref _LD_ObjectCoverPercent, value);

        }
        private int _LD_ObjectCoverPercent;
    }
}
