using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TapeReelPacking.Source.Define;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TapeReelPacking.Source.Model
{

    //[Table("TeachParam")]
    [CategoryOrder(CategoryTeachParameter.CATEGORY_LOCATION, 0)]
    [CategoryOrder(CategoryTeachParameter.CATEGORY_OPPOSITE_CHIP, 1)]
    [DisplayName("Teach Parameter")]
    public class CategoryTeachParameter
    {

        public const string CATEGORY_LOCATION = "LOCATION";
        public const string CATEGORY_OPPOSITE_CHIP = "OPPOSITE CHIP";

        #region LOCATION
        [Browsable(false)]
        [Category(CATEGORY_LOCATION)]
        [DisplayName("Device Location Roi")]
        [Range(0, 5)]
        [Description("")]
        [PropertyOrder(0)]
        public RectanglesModel L_DeviceLocationRoi { get; set; } = new RectanglesModel();

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Location Enable")]
        [PropertyOrder(1)]
        [DefaultValue(true)]
        public bool L_LocationEnable { get; set; }


        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Threshold Method")]
        [DefaultValue(THRESHOLD_TYPE.BINARY_THRESHOLD)]
        [Description("Threshold method")]
        [PropertyOrder(2)]
        public THRESHOLD_TYPE L_ThresholdType
        {
            get;
            set;
        }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Object Color")]
        [DefaultValue(OBJECT_COLOR.BLACK)]
        [Description("The color of Object want to catch")]
        [PropertyOrder(3)]
        public OBJECT_COLOR L_ObjectColor { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Lower Threshold")]
        [Range(0, 255)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(4)]
        public int L_lowerThreshold { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Upper Threshold")]
        [Range(0, 255)]
        [DefaultValue(255)]
        [Description("")]
        [PropertyOrder(5)]
        public int L_upperThreshold { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Lower Threshold Inner Chip")]
        [Range(0, 255)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(6)]
        public int L_lowerThresholdInnerChip { get; set; }


        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Upper Threshold Inner Chip")]
        [Range(0, 255)]
        [DefaultValue(255)]
        [Description("")]
        [PropertyOrder(7)]
        public int L_upperThresholdInnerChip { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Opening Mask")]
        [Range(1, 500)]
        [DefaultValue(11)]
        [Description("")]
        [PropertyOrder(8)]
        public int L_OpeningMask { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Dilation Mask")]
        [Range(1, 500)]
        [DefaultValue(30)]
        [Description("")]
        [PropertyOrder(9)]
        public int L_DilationMask { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Min Width Device")]
        [Range(0, 99999)]
        [DefaultValue(50)]
        [Description("")]
        [PropertyOrder(10)]
        public int L_MinWidthDevice { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Min Height Device")]
        [Range(0, 99999)]
        [DefaultValue(50)]
        [Description("")]
        [PropertyOrder(11)]
        public int L_MinHeightDevice { get; set; }

        [Browsable(false)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Template Roi")]
        [Range(0, 5)]
        [Description("")]
        [PropertyOrder(12)]
        public RectanglesModel L_TemplateRoi { get; set; } = new RectanglesModel();

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Number Side")]
        [Range(1, 360)]
        [DefaultValue(4)]
        [Description("")]
        [PropertyOrder(13)]
        public int L_NumberSide { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Scale Image Ratio")]
        [Range(0.1, 1)]
        [DefaultValue(0.5)]
        [Description("Before Inspecting, The image will be scaled by this value to reduce inspection time.")]
        [PropertyOrder(14)]
        public double L_ScaleImageRatio { get; set; } = 0.5;

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Min Score")]
        [Range(0.0, 99999.0)]
        [DefaultValue(50.0)]
        [Description("")]
        [PropertyOrder(15)]
        public double L_MinScore { get; set; }

        [Browsable(false)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Corner Index")]
        [Range(0, 3)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(16)]
        public int L_CornerIndex { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_LOCATION)]
        [DisplayName("Number ROI Location")]
        [Range(0, 5)]
        [DefaultValue(1)]
        [Description("")]
        [PropertyOrder(17)]
        public int L_NumberROILocation { get; set; }

        #endregion



        #region OPPOSITE Chip


        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_OPPOSITE_CHIP)]
        [DisplayName("Enable")]
        [PropertyOrder(0)]
        [DefaultValue(false)]
        public bool OC_EnableCheck { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_OPPOSITE_CHIP)]
        [DisplayName("lower Threshold")]
        [Range(0, 255)]
        [DefaultValue(0)]
        [PropertyOrder(1)]
        //[ItemsSource(typeof(AreaComboBox))]
        public int OC_lowerThreshold { get; set; }


        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_OPPOSITE_CHIP)]
        [DisplayName("upper Threshold")]
        [Range(0, 255)]
        [DefaultValue(100)]
        [PropertyOrder(2)]
        //[ItemsSource(typeof(AreaComboBox))]
        public int OC_upperThreshold { get; set; }


        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_OPPOSITE_CHIP)]
        [DisplayName("Opening Mask")]
        [Range(0, 100)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(3)]
        public int OC_OpeningMask { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_OPPOSITE_CHIP)]
        [DisplayName("Dilation Mask")]
        [Range(0, 100)]
        [DefaultValue(0)]
        [Description("")]
        [PropertyOrder(4)]
        public int OC_DilationMask { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_OPPOSITE_CHIP)]
        [DisplayName("Min Width Device")]
        [Range(0, 99999)]
        [DefaultValue(50)]
        [Description("")]
        [PropertyOrder(5)]
        public int OC_MinWidthDevice { get; set; }

        [Browsable(true)]
        [Category(CategoryTeachParameter.CATEGORY_OPPOSITE_CHIP)]
        [DisplayName("Min Height Device")]
        [Range(0, 99999)]
        [DefaultValue(50)]
        [Description("")]
        [PropertyOrder(6)]
        public int OC_MinHeightDevice { get; set; }




        #endregion


        [Key]
        [Browsable(false)]
        public int cameraID { get; set; } // Primary key property

        [Browsable(false)]
        public DateTime dateChanged { set; get; }

        [Browsable(false)]
        public ICollection<CategoryVisionParameter> categoryVisionParameter { get; set; } = null;

        [Browsable(false)]
        [Column(TypeName = "longblob")]
        [DisplayName("Teach Image")]
        public byte[] teachImage { get; set; }

        [Browsable(false)]
        [DisplayName("Template Image")]
        [Column(TypeName = "longblob")]

        public byte[] templateImage { get; set; }


    }

}
