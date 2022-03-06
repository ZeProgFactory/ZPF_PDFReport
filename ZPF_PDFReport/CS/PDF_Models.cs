using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZPF.PDF
{
   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

   public class Margins
   {
      /// <summary>
      /// TopMargin including header
      /// </summary>
      public double TopMargin { get; set; } = 2;
      public double HeaderHeight { get; set; } = 0.5;

      public double Gutter { get; set; } = 0.5;
      public double LeftMargin { get; set; } = 2;
      public double RightMargin { get; set; } = 2;

      /// <summary>
      /// BottomMargin including footer
      /// </summary>
      public double BottomMargin { get; set; } = 2;
      public double FooterHeight { get; set; } = 0.5;

      // - - -  - - - 

      public Margins()
      {
      }

      public Margins(double size)
      {
         TopMargin = size;
         LeftMargin = size;
         RightMargin = size;
         BottomMargin = size;
      }

      // - - -  - - - 
   }

   public class PageConfig
   {
      public PageSizes Size { get; set; } = PageSizes.A4;

      public Orientations Orientation
      {
         get => _Orientation;
         set
         {
            if (_Orientation != value)
            {
               _Orientation = value;

               if (_Orientation == Orientations.Portrait)
               {
                  Height = 29.7;
                  Width = 21.0;
               }
               else
               {
                  Height = 21.0;
                  Width = 29.7;
               };
            };
         }
      }
      Orientations _Orientation = Orientations.Portrait;

      public Margins Margins { get; set; } = new Margins();

      /// <summary>
      /// Gets or sets the page height
      /// </summary>
      public double Height { get; set; } = 29.7;

      /// <summary>
      /// Gets or sets the page width
      /// </summary>
      public double Width { get; set; } = 21.0;

      public bool DrawHeader { get; set; } = true;
      public bool DrawHeaderOnFirst { get; set; } = true;
      public bool DrawFooter { get; set; } = true;

      public TFont TitleFont { get; set; } = new TFont { FontSize = 24 };
      public TFont SubTitleFont { get; set; } = new TFont { FontSize = 18 };

      public TFont PageDecorationsFont { get; set; } = new TFont { FontSize = 10 };
      public Color PageDecorationsLineColor { get; set; } = Colors.Black;
      public double PageDecorationsLineWidth { get; set; } = 0.02;
      public Color PageDecorationsBackgroundColor { get; set; } = Color.Parse("0FFF");

      public TFont HeaderFont { get; set; } = new TFont { FontSize = 10, Bold = true };

      public TFont DataRowFont { get; set; } = new TFont { FontSize = 10 };
      public Color DataRow2ndColor { get; set; } = Colors.Lavender;
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

   public class TFont
   {
      public double FontSize { get; set; } = 10;
      public Color ForeGround { get; set; } = Colors.Black;

      public bool Bold { get; set; } = false;
      public bool Italic { get; set; } = false;
      public bool Strikethrough { get; set; } = false;
      public bool Underline { get; set; } = false;

      // - - -  - - - 

      public TFont Copy()
      {
         var o = new TFont();
         o.CopyPropertyValues(this, true);
         return o;
      }
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -
}
