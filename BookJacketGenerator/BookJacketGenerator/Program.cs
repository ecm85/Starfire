using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace BookJacketGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateDustJacket();
            GenerateCover();
        }


        private static void GenerateCover()
        {
            var documentRectangle = new Rectangle(0, 0, Utilities.InchesToPoints(14.59f), Utilities.InchesToPoints(10.5f));
            using (var document = new Document(documentRectangle))
            {
                using (var fileStream =
                    new FileStream($@"C:\Users\Ezramc\Desktop\Starfire\pdf\bookcover{DateTime.Now.ToFileTime()}.pdf",
                        FileMode.Create))
                {
                    using (var pdfWriter = PdfWriter.GetInstance(document, fileStream))
                    {
                        var pristina = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "PRISTINA.TTF");
                        var pristinaBaseFont = BaseFont.CreateFont(pristina, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        var starfireBigFont = new Font(pristinaBaseFont, 80f, Font.BOLDITALIC, BaseColor.RED);
                        var starfireSpineFont = new Font(pristinaBaseFont, 30f, Font.BOLDITALIC, BaseColor.RED);
                        document.Open();
                        document.NewPage();
                        var contentByte = pdfWriter.DirectContent;
                        DrawRectangle(contentByte, documentRectangle);
                        var starfireText = "Starfire";
                        var starfireBigRectangle = new Rectangle(Utilities.InchesToPoints(7.92f), 400,
                            Utilities.InchesToPoints(13.77f), 500);
                        WriteTextInRectangle(contentByte, starfireText, starfireBigFont, starfireBigRectangle,
                            Element.ALIGN_CENTER);

                        var spineRectangle = new Rectangle(Utilities.InchesToPoints(7.2f), 400, 500, 500);
                        WriteTextInRectangle(contentByte, starfireText, starfireSpineFont, spineRectangle, Element.ALIGN_CENTER, 270);
                        document.Close();
                        document.Close();
                    }
                }
            }
        }

        private static void GenerateDustJacket()
        {
            var documentRectangle = new Rectangle(0, 0, Utilities.InchesToPoints(21.09f), Utilities.InchesToPoints(9.5f));
            using (var document = new Document(documentRectangle))
            {
                using (var fileStream =
                    new FileStream($@"C:\Users\Ezramc\Desktop\Starfire\pdf\bookjacket{DateTime.Now.ToFileTime()}.pdf",
                        FileMode.Create))
                {
                    using (var pdfWriter = PdfWriter.GetInstance(document, fileStream))
                    {
                        var pristina = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "PRISTINA.TTF");
                        var pristinaBaseFont = BaseFont.CreateFont(pristina, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        var starfireBigFont = new Font(pristinaBaseFont, 80f, Font.BOLDITALIC, BaseColor.RED);
                        var starfireSpineFont = new Font(pristinaBaseFont, 50f, Font.BOLDITALIC, BaseColor.RED);
                        document.Open();
                        document.NewPage();
                        var contentByte = pdfWriter.DirectContent;
                        DrawRectangle(contentByte, documentRectangle);
                        var starfireText = "Starfire";
                        var starfireBigRectangle = new Rectangle(Utilities.InchesToPoints(11.22f), 400,
                            Utilities.InchesToPoints(17.085f), 500);
                        WriteTextInRectangle(contentByte, starfireText, starfireBigFont, starfireBigRectangle,
                            Element.ALIGN_CENTER);

                        var spineRectangle = new Rectangle(Utilities.InchesToPoints(10.37f), 400, 500, 500);
                        WriteTextInRectangle(contentByte, starfireText, starfireSpineFont, spineRectangle, Element.ALIGN_CENTER, 270);
                        document.Close();
                    }
                }
            }
        }

        private static void DrawRectangle(PdfContentByte content, Rectangle rectangle)
        {
            content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
            content.FillStroke();
        }

        private static bool WriteTextInRectangle(PdfContentByte contentByte, string text, Font font, Rectangle rectangle, int alignment, bool simulation = false)
        {
            var phrase = new Phrase(text, font);

            var columnText = new ColumnText(contentByte)
            {
                Alignment = alignment
            };
            columnText.SetSimpleColumn(phrase, rectangle.Left, rectangle.Bottom, rectangle.Right + 2, rectangle.Top, font.Size, alignment);
            var result = columnText.Go(simulation);
            return result != ColumnText.NO_MORE_COLUMN;
        }

        private static void WriteTextInRectangle(PdfContentByte contentByte, string text, Font font, Rectangle rectangle, int alignment, float rotation)
        {
            var phrase = new Phrase(text, font);
            ColumnText.ShowTextAligned(contentByte, alignment, phrase, rectangle.Left, rectangle.Bottom, rotation);
        }
    }
}
