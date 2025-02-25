using Xfinium.Pdf.Graphics;

namespace ZPF.PDF
{
   public static class PDF_Extensions
   {
      public static PdfColor ToPDFColor(this ZPF.Graphics.Color color)
      {
         return new PdfRgbColor((byte)(255 * color.Red), (byte)(255 * color.Green), (byte)(255 * color.Blue));
      }
   }
}
