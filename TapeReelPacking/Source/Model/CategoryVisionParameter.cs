using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TapeReelPacking.Source.Define;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TapeReelPacking.Source.Model
{
    [CategoryOrder(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT, 0)]
    [DisplayName("Vision Parameter")]
    public class CategoryVisionParameter
    {
        public const string CAETEGORYORDER_LABEL_DEFECT = "LABEL DEFECT";

        //[Key]
        [Browsable(false)]
        public int areaID { get; set; }


        // Foreign key
        [Browsable(false)]
        public int cameraID { get; set; }
        public CategoryTeachParameter categoryTeachParameter { get; set; }
        //


        [Browsable(false)]
        public DateTime dateChanged { set; get; }

        [Browsable(false)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Defect ROI Location")]
        [Range(0, 5)]
        [Description("")]
        [PropertyOrder(0)]
        public RectanglesModel LD_DefectROILocation { set; get; }



        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Area Enable")]
        [PropertyOrder(0)]
        [DefaultValue(false)]
        public bool LD_AreaEnable { set; get; }

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Lower Threshold")]
        [Range(0, 255)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(2)]
        public int LD_lowerThreshold { set; get; }

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Upper Threshold")]
        [Range(0, 255)]
        [DefaultValue(255)]
        [Description("")]
        [PropertyOrder(3)]
        public int LD_upperThreshold { set; get; }

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Opening Mask")]
        [Range(0, 100)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(4)]
        public int LD_OpeningMask { set; get; }

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Dilation Mask")]
        [Range(0, 100)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(5)]
        public int LD_DilationMask { set; get; }

        [Browsable(true)]
        [Category(CategoryVisionParameter.CAETEGORYORDER_LABEL_DEFECT)]
        [DisplayName("Object Cover PerCent")]
        [Range(0, 100)]
        [DefaultValue(50)]
        [Description("")]
        [PropertyOrder(6)]
        public int LD_ObjectCoverPercent { set; get; }
    }
}
