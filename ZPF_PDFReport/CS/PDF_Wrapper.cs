using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using SkiaSharp;
using Xfinium.Pdf.Actions;
using Xfinium.Pdf.Annotations;
using Xfinium.Pdf.Graphics;
using Xfinium.Pdf.Graphics.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ZPF.PDF
{
    public partial class PDFHelper
    {
        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public double GetTextHeight(double FontSize = 10, string text = "abcABCjq°", double width = 999)
        {
            // Sets the font.
            PdfFont font = new PdfStandardFont(PdfStandardFontFace.Helvetica, FontSize);

            return From(PdfTextEngine.GetStringHeight(text, font, To(width)));
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public void DrawText(string text, double PosX, double PosY, double width, double height, HAlignments hAlignment, VAlignments vAlignment, TFont Font, bool IsFooter = false)
        {
            //width = CheckBorderX(PosX, width);
            //height = CheckBorderY(PosY, height, IsFooter);
            //if (width < 0 || height < 0) return;

            if (string.IsNullOrEmpty(text)) return;

            // Sets the font.
            PdfFont font = new PdfStandardFont(PdfStandardFontFace.Helvetica, Font.FontSize);
            font.Bold = Font.Bold;
            font.Italic = Font.Italic;
            font.Strikethrough = Font.Strikethrough;
            font.Underline = Font.Underline;

            PdfStringLayoutOptions layout = new PdfStringLayoutOptions();
            layout.Rotation = 0;

            layout.X = To(PosX);
            layout.Y = To(PosY);
            layout.Width = To(width);
            layout.Height = To(height);

            switch (vAlignment)
            {
                case VAlignments.Top:
                    layout.VerticalAlign = PdfStringVerticalAlign.Top;
                    break;
                case VAlignments.Center:
                    layout.VerticalAlign = PdfStringVerticalAlign.Middle;
                    break;
                case VAlignments.Bottom:
                    layout.VerticalAlign = PdfStringVerticalAlign.Bottom;
                    break;
            };

            switch (hAlignment)
            {
                case HAlignments.Right:
                    layout.HorizontalAlign = PdfStringHorizontalAlign.Right;
                    break;
                case HAlignments.Center:
                    layout.HorizontalAlign = PdfStringHorizontalAlign.Center;
                    break;
                case HAlignments.Default:
                case HAlignments.Left:
                    layout.HorizontalAlign = PdfStringHorizontalAlign.Left;
                    break;
                case HAlignments.Justify:
                    layout.HorizontalAlign = PdfStringHorizontalAlign.Justified;
                    break;
            };

            PdfStringAppearanceOptions appearance = new PdfStringAppearanceOptions();
            appearance.Font = font;
            appearance.Brush = new PdfBrush(Font.ForeGround.ToPDFColor());

            // Draws the text.
            graphics.DrawString(text, appearance, layout);
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public double AddImage(MemoryStream memoryStream, double PosX = 0, double PosY = 0, double width = 0, double height = 0, bool centered = true)
        {
            if (memoryStream == null || memoryStream.Length == 0)
            {
                return PosY;
            };

            PosX = (PosX == 0 ? pageConfig.Margins.LeftMargin : PosX);
            PosY = (PosY == 0 ? pageConfig.Margins.TopMargin : PosY);

            try
            {
                memoryStream.Position = 0;

                width = (width == 0 ? pageConfig.Width - pageConfig.Margins.LeftMargin - pageConfig.Margins.RightMargin : width);
                height = (height == 0 ? pageConfig.Height - PosY - pageConfig.Margins.BottomMargin : height);

                if (MimeType.GetMimeType(memoryStream) == "image/jpeg")
                {
                    var image = new PdfJpegImage(memoryStream);

                    // - - - 

                    var ratioX = To(width) / image.Width;
                    var ratioY = To(height) / image.Height;
                    var ratio = Math.Min(ratioX, ratioY);

                    var newWidth = (int)(image.Width * ratio);
                    var newHeight = (int)(image.Height * ratio);

                    var posX = (centered ? To(PosX) + (To(width) - newWidth) / 2 : To(PosX));
                    var posY = (centered ? To(PosY) + (To(height) - newHeight) / 2 : To(PosY));

                    // - - - 

                    graphics.DrawImage(image, posX, posY, newWidth, newHeight);
                };

                if (MimeType.GetMimeType(memoryStream) == "image/png")
                {
                    var image = new PdfPngImage(memoryStream);

                    // - - - 

                    var ratioX = To(width) / image.Width;
                    var ratioY = To(height) / image.Height;
                    var ratio = Math.Min(ratioX, ratioY);

                    var newWidth = (centered ? (int)(image.Width * ratio) : To(width));
                    var newHeight = (centered ? (int)(image.Height * ratio) : To(height));

                    var posX = (centered ? To(PosX) + (To(width) - newWidth) / 2 : To(PosX));
                    var posY = (centered ? To(PosY) + (To(height) - newHeight) / 2 : To(PosY));

                    // - - - 

                    graphics.DrawImage(image, posX, posY, newWidth, newHeight);
                    //graphics.DrawImage(image, To(PosX), To(PosY), newWidth, newHeight);
                    //graphics.DrawImage(image, To(PosX), To(PosY), To(5), To(5));
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            };

            return PosY + height;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        void DrawLine(Color color, PdfPoint begin, PdfPoint end, double width = 0.02, bool isFooter = false)
        {
            PdfPen pen = new PdfPen();

            pen.Color = color.ToPDFColor();
            pen.LineCap = PdfLineCap.Round;
            pen.Width = To(width);

            if (color.Alpha > 0)
            {
                graphics.DrawLine(pen, new PdfPoint(begin.X, begin.Y), new PdfPoint(end.X, end.Y));
            };
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        void DrawBox(Color color, double X, double Y, double width, double height, bool isFooter = false)
        {
            width = CheckBorderX(X, width);
            height = CheckBorderY(Y, height, isFooter);
            if (width < 0 || height < 0) return;

            PdfBrush brush = new PdfBrush(color.ToPDFColor());

            if (color.Alpha > 0)
            {
                graphics.DrawRectangle(brush, To(X), To(Y), To(width), To(height));
            };
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public void DrawRectangle(Color color, double X, double Y, double width, double height, bool isFooter = false)
        {
            width = CheckBorderX(X, width);
            height = CheckBorderY(Y, height, isFooter);
            if (width < 0 || height < 0) return;

            PdfBrush brush = new PdfBrush(color.ToPDFColor());

            //brush = new PdfBrush(PdfRgbColor.Lavender);

            double corner = 0.0;
            double rotation = 0.0;

            if (color.Alpha > 0)
            {
                graphics.DrawRoundRectangle(brush, To(X), To(Y), To(width), To(height), To(corner), To(corner), rotation);
            };
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public void AddLink(double PosX, double PosY, double width, double height, string URL)
        {
            // Create a link annotation on top of the widget.
            PdfLinkAnnotation link = new PdfLinkAnnotation();
            document.Pages[document.Pages.Count - 1].Annotations.Add(link);
            link.VisualRectangle = new PdfVisualRectangle(To(PosX), To(PosY), To(width), To(height));

            // Create an uri action and attach it to the link.
            PdfUriAction uriAction = new PdfUriAction();
            uriAction.URI = URL;
            link.Action = uriAction;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public MemoryStream ReadStream(string FullFileName)
        {
            // Create the stream
            MemoryStream Result = new MemoryStream();

            try
            {
                var fileStream = File.OpenRead(FullFileName);
                fileStream.CopyTo(Result);
                fileStream.Close();

                Result.Position = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            };

            return Result;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -
    }
}
