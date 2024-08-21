using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapeReelPacking.Source.Define;

namespace TapeReelPacking.Source.Model
{
    [Table("Rectangles ROI")]
    public class RectanglesModel
    {
        [Key]
        public int Id { get; set; } // Primary key

        public double left { get; set; }
        public double top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Angle { get; set; }

        public void SetRectangle(Rectangles rec)
        {
            left = rec.TopLeft.X;
            top = rec.TopLeft.Y;
            Width = rec.Width;
            Height = rec.Height;
            Angle = rec.Angle;
        } 
        public Rectangles GetRectangle()
        {
            return new Rectangles(left, top, Width, Height, Angle);
        }


    }
}
