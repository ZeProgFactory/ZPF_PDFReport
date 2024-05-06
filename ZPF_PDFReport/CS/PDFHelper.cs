using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

namespace ZPF.PDF
{
    public partial class PDFHelper
    {
        // https://xfiniumsoft.com/xfinium-pdf-crossplatform/xfinium-pdf-samples.html
        // https://xfiniumsoft.com/samples/xfinium-pdf-samples-explorer-aspnet-mvc/
        // https://xfiniumsoft.com/help/xfinium.pdf/Xfinium.Pdf.Pcl.html

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public PageConfig pageConfig = new PageConfig();

        PdfFixedDocument document = null;
        PdfPage page = null;
        PdfPageGraphics graphics = null;
        PdfBrush brush = new PdfBrush(PdfRgbColor.Black);

        PdfFont font = new PdfStandardFont(PdfStandardFontFace.Helvetica, 10);
        //PdfStandardFont helveticaBold = new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 16);

        //static PdfColor color = new PdfRgbColor(0, 0, 0);
        Color cBlack = Colors.Black;
        Color cLightGray = Colors.LightGray;

        public double poil { get; } = 0.1;

        public enum CallBacks
        {
            Test, GroupBy, TableFooter
        }

        // - - -  - - - 

        public static double To(double cm)
        {
            return 28.34 * cm;
        }

        public static double From(double pica)
        {
            return pica / 28.34;
        }

        // - - -  - - - 

        public void InitDoc(string fileName, string Title = "", Orientations orientation = Orientations.Portrait)
        {
            pageConfig.Size = PageSizes.A4;
            pageConfig.Orientation = orientation;

            // Create a new PDF document.
            document = new PdfFixedDocument();
            document.DocumentInformation = new PdfDocumentInformation();

            document.DocumentInformation.CreationDate = DateTime.Now;
            document.DocumentInformation.ModifyDate = DateTime.Now;
            document.DocumentInformation.Title = (string.IsNullOrEmpty(Title) ? System.IO.Path.GetFileNameWithoutExtension(fileName) : Title);

            AddPage();
        }

        public void AddPage()
        {
            // Add a page
            page = document.Pages.Add();

            // A4 = 8.27x11.69" x72points/inch = 595x842 points
            //page.Height = 865;

            if (pageConfig.Orientation == Orientations.Portrait)
            {
                page.Height = 842;
                page.Width = 595;
            }
            else
            {
                page.Height = 595;
                page.Width = 842;
            };

            // Creates Pdf graphics for the page
            graphics = page.Graphics;
        }

        public MemoryStream SaveDoc(string fileName, int ind=1)
        {
            //page.Graphics.CompressAndClose();

            // - - -  - - -

            for (int i = 0; i < document.Pages.Count; i++)
            {
                page = document.Pages[i];
                graphics = page.Graphics;

                AddPageDecorations(ind + i );

                graphics.CompressAndClose();
            };

            // - - -  - - -

            // Saves the document.
            //if (OnProgress != null)
            //   OnProgress("save PDF");

            MemoryStream stream = new MemoryStream();
            document.Save(stream);
            // document.Close(true);

            WriteStream(stream, fileName);
            stream.Position = 0;
            return stream;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public enum PageDecorationLocations { header, footer }

        public delegate void PageDecorationsEventHandler(object sender, PageDecorationLocations pageDecorationLocation);
        public event PageDecorationsEventHandler OnPageDecoration;

        private void AddPageDecorations(int pageNo)
        {
            PageNo = pageNo;

            TFont font = pageConfig.PageDecorationsFont;

            if (pageConfig.DrawHeader && (pageNo != 1 || pageConfig.DrawHeaderOnFirst))
            {
                var y = pageConfig.Margins.TopMargin - poil;

                //DrawRectangle(pageConfig.PageDecorationsBackgroundColor, y - 0.6, 0.6);
                //AddLine(y);

                if (OnPageDecoration != null)
                {
                    PosY = y;
                    OnPageDecoration(this, PageDecorationLocations.header);
                }
                else
                {
                    y = y - GetTextHeight();

                    //DrawText(DateTime.Now.ToShortDateString(), To(pageConfig.Margins.LeftMargin), y, To(3), GetTextHeight(), HAlignments.Left, VAlignments.Top, 10, cBlack);
                    //DrawText($"page {PageNo}/{document.Pages.Count}", To(pageConfig.Width - pageConfig.Margins.RightMargin - 3), y, To(3), GetTextHeight(), HAlignments.Right, VAlignments.Top, 10, cBlack);

                    DrawText(document.DocumentInformation.Title, (pageConfig.Width / 2) - 3, y, 3 * 2, GetTextHeight(), HAlignments.Center, VAlignments.Top, font);
                };
            };

            if (pageConfig.DrawFooter)
            {
                bool IsFooter = true;
                var y = pageConfig.Height - pageConfig.Margins.BottomMargin + poil;

                DrawRectangle(pageConfig.PageDecorationsBackgroundColor, y, 0.6, IsFooter);
                y = AddLine(y, IsFooter);

                if (OnPageDecoration != null)
                {
                    PosY = y;
                    OnPageDecoration(this, PageDecorationLocations.footer);
                }
                else
                {
                    y = y + poil;

                    DrawText(DateTime.Now.ToString("dd/MM/yyyy"),
                       pageConfig.Margins.LeftMargin, y, 3, GetTextHeight(), HAlignments.Left, VAlignments.Top, font, IsFooter);

                    DrawText($"page {pageNo}/{document.Pages.Count}",
                       pageConfig.Width - pageConfig.Margins.RightMargin - 3 - poil, y, 3, GetTextHeight(), HAlignments.Right, VAlignments.Top, font, IsFooter);

                    DrawText(Copyright,
                       (pageConfig.Width / 2) - 3, y, 3 * 2, GetTextHeight(), HAlignments.Center, VAlignments.Top, font, IsFooter);
                };
            };
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public double AddLine(double y = 0, bool isFooter = false)
        {
            if (y == 0)
            {
                y = PosY;
            };

            y = To(y);

            if (pageConfig.PageDecorationsLineColor.Alpha > 0)
            {
                DrawLine(pageConfig.PageDecorationsLineColor,
                   new PdfPoint(To(pageConfig.Margins.LeftMargin), y), new PdfPoint(To(pageConfig.Width - pageConfig.Margins.RightMargin), y),
                   pageConfig.PageDecorationsLineWidth, isFooter);
            };

            return From(y) + pageConfig.PageDecorationsLineWidth;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public double AddTitle(string title, string subTitle = null, double PosY = 0)
        {
            document.DocumentInformation.Title = title;

            double PosX = pageConfig.Margins.LeftMargin;
            PosY = (PosY <= 0 ? pageConfig.Margins.TopMargin : PosY);

            double FontSize = 24;

            FontSize = 14;
            PosY = PosY + GetTextHeight(FontSize);

            FontSize = pageConfig.TitleFont.FontSize;
            var height = GetTextHeight(FontSize, title);

            DrawText(title,
               pageConfig.Margins.LeftMargin, PosY,
               pageConfig.Width - pageConfig.Margins.LeftMargin - pageConfig.Margins.RightMargin, height,
               HAlignments.Center, VAlignments.Top, pageConfig.TitleFont);

            PosY = PosY + height;
            FontSize = 14;

            if (subTitle == null)
            {
                subTitle = DateTime.Now.ToString();
            };

            if (subTitle.Trim() == "")
            {
                return From(PosY);
            };

            FontSize = pageConfig.SubTitleFont.FontSize;
            height = GetTextHeight(FontSize, title);

            DrawText(subTitle,
               pageConfig.Margins.LeftMargin, PosY,
               pageConfig.Width - pageConfig.Margins.LeftMargin - pageConfig.Margins.RightMargin, height,
               HAlignments.Center, VAlignments.Top, pageConfig.SubTitleFont);

            PosY = PosY + height;

            FontSize = 14;
            PosY = PosY + GetTextHeight(FontSize);

            return PosY;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -
        //ToDo: Attributes

        public double AddDataTable(DataTable dt, int MaxLines = -1, List<ColDef> Fields = null, double PosX_Init = 2, double PosY_Init = 2, bool GenArray = true, string GroupBy = "", bool Header = true)
        {
            string GroupBy_val = null;
            int Groupby_idx = dt.Columns.IndexOf(GroupBy);

            double PosX = PosX_Init;
            double PosY = PosY_Init + poil;

            int Cols = -1;

            if (dt == null) return PosY;

            // - - - fields - - - 

            if (Fields == null)
            {
                Fields = new List<ColDef>();

                int Ind = 0;
                foreach (DataColumn c in dt.Columns)
                {
                    if (c.ColumnName != GroupBy)
                    {
                        Fields.Add(new ColDef
                        {
                            Name = c.ColumnName,
                            Header = c.ColumnName,
                            NbChars = (int)(c.ColumnName.Length * From(.6)),
                            Ind = Ind,
                        });
                    };

                    Ind++;
                };

                // - - - widths - - - 

                foreach (DataRow r in dt.Rows)
                {
                    Cols = Fields.Count;

                    for (int x = 0; x < Fields.Count; x++)
                    {
                        Ind = 0;
                        double width = To(3);

                        foreach (var c in r.ItemArray)
                        {
                            if (Fields[x].Ind == Ind)
                            {
                                Fields[x].NbChars = Math.Max(Fields[x].NbChars, c.ToString().TrimEnd().Length);
                            };

                            Ind++;
                        };
                    };
                };

                double FontSize = 10;

                foreach (var f in Fields)
                {
                    f.Width = f.NbChars * FontSize * From(0.7);
                };
            }
            else
            {
                int Ind = 0;

                foreach (var f in Fields)
                {
                    f.Ind = -1;
                };

                foreach (DataColumn c in dt.Columns)
                {
                    var f = Fields.Where(x => x.Name == c.ColumnName).FirstOrDefault();

                    if (f != null)
                    {
                        f.Ind = Ind;
                    };

                    Ind++;
                };

                foreach (var f in Fields)
                {
                    f.NbChars = 32;
                    f.Width = f.Width;
                };
            };

            // - - - header - - - 

            void PrintHeader()
            {
                double FontSize = 10;

                double height = GetTextHeight(FontSize);

                foreach (var f in Fields)
                {
                    if (f.Ind != Groupby_idx)
                    {
                        VAlignments vAlignment = VAlignments.Top;

                        DrawBox(cLightGray, PosX, PosY - poil, f.Width, height + poil);

                        if (f.NbChars > 0)
                        {
                            DrawText(f.Header.Left(f.NbChars), PosX, PosY, f.Width, height, f.HAlignment, vAlignment, pageConfig.HeaderFont);
                        }
                        else
                        {
                            DrawText(f.Header, PosX, PosY, f.Width, height, f.HAlignment, vAlignment, pageConfig.HeaderFont);
                        };

                        PosX = PosX + f.Width;
                    };
                };

                // fill EOL
                DrawBox(cLightGray, PosX, PosY - poil, pageConfig.Width, height + poil);


                PosX = PosX_Init;
                PosY = PosY + height + poil;
            };

            if (Header)
            {
                PrintHeader();
            };

            // - - - rows - - - 

            long row = 0;

            foreach (DataRow r in dt.Rows)
            {
                Cols = Fields.Count;
                double FontSize = pageConfig.DataRowFont.FontSize;

                double height = GetTextHeight(FontSize);

                if (!string.IsNullOrEmpty(GroupBy))
                {
                    if (GroupBy_val != r.ItemArray[Groupby_idx].ToString())
                    {
                        if (OnCallBack != null && !string.IsNullOrEmpty(GroupBy_val))
                        {
                            OnCallBack(pageConfig, CallBacks.GroupBy, PosX, PosY, r);
                            PosY = _PosY;
                        };

                        PosX = PosX_Init;
                        PosY = PosY + height + poil;

                        GroupBy_val = r.ItemArray[Groupby_idx].ToString();

                        FontSize = 12;
                        height = GetTextHeight(FontSize);

                        DrawText(GroupBy_val, PosX, PosY, pageConfig.Width, height, HAlignments.Left, VAlignments.Top, pageConfig.DataRowFont);

                        PosX = PosX_Init;
                        PosY = PosY + height;
                        var y = PosY;
                        DrawLine(cBlack, new PdfPoint(pageConfig.Margins.LeftMargin, y), new PdfPoint(pageConfig.Width - pageConfig.Margins.RightMargin, y));
                        PosY = PosY + poil + poil;

                        FontSize = 10;
                    };
                };

                foreach (var f in Fields)
                {
                    if (row % 2 != 0 && pageConfig.DataRow2ndColor.Alpha > 0)
                    {
                        DrawBox(pageConfig.DataRow2ndColor, PosX, PosY - poil, f.Width, height + poil);
                    };

                    if (f.Ind != Groupby_idx)
                    {
                        VAlignments vAlignment = VAlignments.Top;

                        if (f.Ind != -1)
                        {
                            var c = r.ItemArray[f.Ind];

                            DrawText(c.ToString().Left((int)(16 * From(1024))), PosX, PosY, f.Width, height, f.HAlignment, vAlignment, pageConfig.DataRowFont);
                        };
                    };

                    PosX = PosX + f.Width;
                };

                // fill EOL
                if (row % 2 != 0 && pageConfig.DataRow2ndColor.Alpha > 0)
                {
                    DrawBox(pageConfig.DataRow2ndColor, PosX, PosY - poil, pageConfig.Width, height + poil);
                };

                row++;
                PosX = PosX_Init;
                PosY = PosY + height + poil;

                if (CheckBorderY(PosY, height) < 0)
                {
                    //page.Graphics.CompressAndClose();

                    AddPage();

                    PosX = pageConfig.Margins.LeftMargin;
                    PosY = pageConfig.Margins.TopMargin + poil;

                    PrintHeader();
                };

                if (MaxLines > 0 && PosY > MaxLines + 2)
                {
                    return PosY;
                };
            };

            if (OnCallBack != null && !string.IsNullOrEmpty(GroupBy_val))
            {
                OnCallBack(pageConfig, CallBacks.GroupBy, PosX, PosY, null);

                PosY = _PosY;
            };

            if (OnCallBack != null)
            {
                OnCallBack(pageConfig, CallBacks.TableFooter, PosX, PosY, null);
                PosY = _PosY;
            };

            // - - - table - - - 

            if (Cols > -1 && GenArray)
            {
                //TablePart table = sheet.AddTable("My Table" + sheet.SheetID, "My Table" + sheet.SheetID, sheet.Rows[0].Cells[0], sheet.Rows[PosY - 1].Cells[Cols - 1]);

                //PosX = 0;
                //foreach (var s in strings)
                //{
                //   if (widths[PosX] != -1)
                //   {
                //      if (widths[PosX] > 20) widths[PosX] = 20;
                //      sheet.AddColumnSizeDefinition(PosX, PosX, widths[PosX] * 1.3);
                //   };

                //   table.TableColumns[PosX].Name = s.String;
                //   PosX++;
                //};
            };

            // - - -  - - - 

            return PosY;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public double AddDummy(double PosX_Init = 2, double PosY_Init = 2)
        {
            double PosX = To(PosX_Init);
            double PosY = To(PosY_Init + poil);

            // - - -  - - - 

            if (OnCallBack != null)
            {
                OnCallBack(pageConfig, CallBacks.TableFooter, From(PosX), From(PosY), null);
                PosY = To(_PosY);
            };

            // - - -  - - - 

            return From(PosY);
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        private double CheckBorderY(double y, double height, bool IsFooter = false)
        {
            if (IsFooter)
            {
                if (y + height > pageConfig.Height)
                {
                    return -1;
                };
            }
            else
            {
                if (y + height > pageConfig.Height - pageConfig.Margins.BottomMargin)
                {
                    return -1;
                };
            };

            return height;
        }

        private double CheckBorderX(double x, double width)
        {
            if (x > (pageConfig.Width - pageConfig.Margins.RightMargin))
            {
                return -1;
            }
            else
            {
                if (x + width > (pageConfig.Width - pageConfig.Margins.RightMargin))
                {
                    width = pageConfig.Width - pageConfig.Margins.RightMargin - x;
                };
            };

            return width;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public double PosX { get => _PosX; set => _PosX = value; }
        double _PosX = 2;

        public double PosY { get => _PosY; set => _PosY = value; }
        double _PosY = 2;

        public int PagesCount { get => document.Pages.Count; }
        public object PageNo { get; internal set; }
        public string Copyright { get; set; } = "Copyright © 2022 - ZPF.fr";

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public delegate void CallBackHandler(object s, PDFHelper.CallBacks callBack, double PosX, double PosY, object data);
        public event CallBackHandler OnCallBack;

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public double AddImage(string FileName, double PosX = 0, double PosY = 0, double width = 0, double height = 0)
        {
            if (!System.IO.File.Exists(FileName))
            {
                return PosY;
            };

            MemoryStream ms = new MemoryStream();

            using (FileStream file = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                file.CopyTo(ms);
            };

            return AddImage(ms, PosX, PosY, width, height);
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public double DrawRectangle(Color color, double y, double height, bool isFooter = false)
        {
            //width = CheckBorderX(X, width);
            //height = CheckBorderY(Y, height);
            //if (width < 0 || height < 0) return;

            double x = pageConfig.Margins.LeftMargin;
            double width = pageConfig.Width - pageConfig.Margins.LeftMargin - pageConfig.Margins.RightMargin;

            DrawRectangle(color, x, y, width, height, isFooter);

            return y + height;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

        public bool WriteStream(Stream stream, string FullFileName)
        {
            try
            {
                var fileStream = File.Create(FullFileName);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            };

            return true;
        }

        // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -
    }
}
