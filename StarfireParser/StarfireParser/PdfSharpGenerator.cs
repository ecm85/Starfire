using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace StarfireParser
{
    class PdfSharpGenerator
    {

        public static readonly double PageVerticalPadding = XUnit.FromInch(0.31).Point;
        public static readonly double PageHorizontalPadding = XUnit.FromInch(0.88).Point;
        public const int VerticalHeaderPadding = 50;
        public const int ColumnPadding = 5;
        public const int LinePadding = 3;

        public void GeneratePdf(List<ChatDay> dates)
        {
            var sampleDateText = dates
                .SelectMany(date => date.Lines)
                .Select(line => line.Time)
                .OrderByDescending(time => time.ToString().Length)
                .First()
                .ToString();
            var samplePersonText = dates
                .SelectMany(date => date.Lines)
                .Select(line => line.Person)
                .OrderByDescending(person => person.Length)
                .First();

            //TODO: Page numbers
            //TODO: Foreword/Dedication/both
            //TODO: Emoji options:
            /*
             * 1. Replace them with ""
             * 2. Replace them with " "
             * 3. Replace them with ?
             * 4. Replace them with any non-printing emoji
             * 5. 
             * */
            //TODO: Lines that don't wrap (no linebreaks)
            //TODO: Add book title to header on non-chapter pages?
            //TODO: Cover
            //TODO: Jacket

            var brushesByTextType = new Dictionary<TextType, XSolidBrush>
            {
                {TextType.EzraChat, new XSolidBrush(XColor.FromArgb(Color.FromArgb(40, 150, 40).ToArgb()))},
                {TextType.EzraPersonal, new XSolidBrush(XColor.FromArgb(Color.FromArgb(40, 150, 40).ToArgb()))},
                {TextType.FunnyChat, new XSolidBrush(XColor.FromArgb(Color.FromArgb(138, 43, 226).ToArgb()))},
                {TextType.MusicAndLinkChat, new XSolidBrush(XColor.FromArgb(Color.FromArgb(0, 0, 255).ToArgb()))},
                {TextType.RomanticChat, new XSolidBrush(XColor.FromArgb(Color.FromArgb(205, 51, 51).ToArgb()))},
                {TextType.SarahChat, new XSolidBrush(XColor.FromArgb(Color.FromArgb(0, 150, 150).ToArgb()))},
                {TextType.SarahPersonal, new XSolidBrush(XColor.FromArgb(Color.FromArgb(0, 150, 150).ToArgb()))},
            };
            var boldChatTypes = new[]
            {
                TextType.EzraPersonal,
                TextType.SarahPersonal,
                TextType.FunnyChat,
                TextType.RomanticChat
            };

            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            var pdfDocument = new PdfDocument();
            var headerFont = new XFont("Garamond", 30, XFontStyle.Bold);
            var blackBrush = new XSolidBrush(XColor.FromArgb(Color.Black.ToArgb()));
            var textFont = new XFont("Segoe UI Symbol", 12, XFontStyle.Regular, options);
            var borderDemoBrush = new XSolidBrush(XColor.FromArgb(Color.FromArgb(240, 240, 240).ToArgb()));
            var boldTextFont = new XFont("Segoe UI Symbol", 12, XFontStyle.Bold, options);
            foreach (var chatDay in dates)
            {
                var page = CreatePage(pdfDocument);
                var xGraphics = XGraphics.FromPdfPage(page);
                xGraphics.DrawRectangle(borderDemoBrush, new XRect(0, 0, page.Width, page.Height));
                var textFormatter = new XTextFormatterEx2(xGraphics);

                var chatDayDate = chatDay.Date.ToLongDateString();
                var headerSize = xGraphics.MeasureString(chatDayDate, headerFont);
                var headerX = (page.Width / 2) - (headerSize.Width / 2);
                var headerRectangle = new XRect(headerX, VerticalHeaderPadding, headerSize.Width, headerSize.Height);
                textFormatter.DrawString(chatDayDate, headerFont, blackBrush, headerRectangle);

                var mainSectionTop = headerRectangle.Bottom + VerticalHeaderPadding;
                var mainSectionHeight = page.Height - (headerRectangle.Height + VerticalHeaderPadding + VerticalHeaderPadding + PageVerticalPadding);
                var pageLayout = new PdfSharpPageLayout(xGraphics);
                pageLayout.InitializeBasePageLayout(mainSectionTop, mainSectionHeight, textFont, sampleDateText, samplePersonText, page.Width);

                var nextTop = pageLayout.TextColumn.Top;
                foreach (var nextChatLine in chatDay.Lines)
                {
                    var remainingHeight = pageLayout.TextColumn.Bottom - (nextTop + LinePadding);
                    var newPage = false;
                    double neededHeightForChatText = 0;
                    if (remainingHeight <= 0)
                    {
                        newPage = true;
                    }
                    else
                    {
                        neededHeightForChatText = GetNeededHeightForChatText(pageLayout, nextTop, textFormatter, nextChatLine, textFont);
                        if (neededHeightForChatText <= 0 || neededHeightForChatText > remainingHeight)
                        {
                            newPage = true;
                        }
                    }
                    if (newPage)
                    {
                        page = CreatePage(pdfDocument);
                        xGraphics = XGraphics.FromPdfPage(page);
                        xGraphics.DrawRectangle(borderDemoBrush, new XRect(0, 0, page.Width, page.Height));
                        textFormatter = new XTextFormatterEx2(xGraphics);
                        mainSectionTop = PageVerticalPadding;
                        mainSectionHeight = page.Height - 2 * PageVerticalPadding;

                        pageLayout = new PdfSharpPageLayout(xGraphics);
                        pageLayout.InitializeBasePageLayout(mainSectionTop, mainSectionHeight, textFont, sampleDateText, samplePersonText, page.Width);
                        nextTop = pageLayout.DateColumn.Top;
                        neededHeightForChatText = GetNeededHeightForChatText(pageLayout, nextTop, textFormatter, nextChatLine, textFont);
                    }
                    var textRect = new XRect(pageLayout.TextColumn.Left, nextTop, pageLayout.TextColumn.Width, neededHeightForChatText);
                    var dateRect = new XRect(pageLayout.DateColumn.Left, nextTop, pageLayout.DateColumn.Width, neededHeightForChatText);
                    var personRect = new XRect(pageLayout.PersonColumn.Left, nextTop, pageLayout.PersonColumn.Width, neededHeightForChatText);

                    var font = boldChatTypes.Contains(nextChatLine.TextType) ? boldTextFont : textFont;
                    textFormatter.DrawString(nextChatLine.Time.ToString(), textFont, brushesByTextType[nextChatLine.TextType], dateRect);
                    textFormatter.DrawString(nextChatLine.Person, textFont, brushesByTextType[nextChatLine.TextType], personRect);
                    textFormatter.DrawString(nextChatLine.Text, font, brushesByTextType[nextChatLine.TextType], textRect, XStringFormats.TopLeft);
                    nextTop += neededHeightForChatText + LinePadding;
                }
            }

            pdfDocument.Save($@"C:\Users\Ezramc\Desktop\Starfire\pdf\output{DateTime.Now.ToFileTime()}.pdf");
        }

        private static double GetNeededHeightForChatText(PdfSharpPageLayout pdfSharpPageLayout, double nextTop, XTextFormatterEx2 textFormatter,
            ChatLine nextChatLine, XFont textFont)
        {
            var textRect = new XRect(pdfSharpPageLayout.TextColumn.Left, nextTop, pdfSharpPageLayout.TextColumn.Width,
                pdfSharpPageLayout.TextColumn.Height);
            double neededHeightForChatText;
            textFormatter.PrepareDrawString(nextChatLine.Text, textFont, textRect, out int _, out neededHeightForChatText);
            return neededHeightForChatText;
        }

        private static PdfPage CreatePage(PdfDocument pdfDocument)
        {
            var page = pdfDocument.AddPage();
            page.Width = XUnit.FromInch(6);
            page.Height = XUnit.FromInch(9);
            return page;
        }
    }
}
