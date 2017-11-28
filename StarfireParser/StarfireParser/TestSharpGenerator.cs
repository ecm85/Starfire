using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;
using Rectangle = iTextSharp.text.Rectangle;

namespace StarfireParser
{
    public class TestSharpGenerator
    {
        public static readonly float PageVerticalPadding = Utilities.InchesToPoints(0.5f);
        public static readonly float PageHorizontalPadding = Utilities.InchesToPoints(0.88f);
        public const int VerticalHeaderPadding = 50;
        public const int ColumnPadding = 7;
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


            //TODO: Cover
            //TODO: Jacket

            int pageNumber = 0;

            const float textFontSize = 12;
            
            var pristina = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "PRISTINA.TTF");
            var pristinaBaseFont = BaseFont.CreateFont(pristina, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var segoeUi = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "seguisym.ttf");
            var baseFont = BaseFont.CreateFont(segoeUi, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var garamond = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "GARA.TTF");
            var garamondBaseFont = BaseFont.CreateFont(garamond, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var fontsByTextType = new Dictionary<TextType, Font>
            {
                {TextType.EzraChat, new Font(baseFont, textFontSize, Font.NORMAL, new CMYKColor(.733f, 0f, .7333f, .4188f))},
                {TextType.EzraPersonal, new Font(baseFont, textFontSize, Font.BOLD, new CMYKColor(.733f, 0f, .7333f, .4188f))},
                {TextType.FunnyChat, new Font(baseFont, textFontSize, Font.NORMAL, new CMYKColor(.3894f, .8097f, 0f, .1137f))},
                {TextType.MusicAndLinkChat, new Font(baseFont, textFontSize, Font.NORMAL, new CMYKColor(1f, 1f, 0f, 0f))},
                {TextType.RomanticChat, new Font(baseFont, textFontSize, Font.NORMAL, new CMYKColor(0f, .7512f, .7512f, .1961f))},
                {TextType.SarahChat, new Font(baseFont, textFontSize, Font.NORMAL, new CMYKColor(1f, 0f, 0f, .4118f))},
                {TextType.SarahPersonal, new Font(baseFont, textFontSize, Font.BOLD, new CMYKColor(1f, 0f, 0f, .4118f))}
            };
            var textFont = new Font(baseFont, textFontSize);
            var pageNumberFont = new Font(baseFont, textFontSize - 4f);
            var dedicationFont = new Font(baseFont, textFontSize, Font.ITALIC);
            var starfireBigFont = new Font(pristinaBaseFont, 80f, Font.BOLDITALIC);
            var starfireSmallFont = new Font(pristinaBaseFont, 45f, Font.BOLDITALIC);

            var documentRectangle = new Rectangle(0, 0, Utilities.InchesToPoints(6f), Utilities.InchesToPoints(9f));
            using (var document = new Document(documentRectangle))
            {
                using (var fileStream = new FileStream($@"C:\Users\Ezramc\Desktop\Starfire\pdf\output{DateTime.Now.ToFileTime()}.pdf", FileMode.Create))
                {
                    using (var pdfWriter = PdfWriter.GetInstance(document, fileStream))
                    {
                        document.Open();
                        var contentByte = pdfWriter.DirectContent;

                        //First 2 blank pages
                        //TODO Should these be a different stock?
                        document.NewPage();
                        document.Add(new Chunk());
                        document.NewPage();
                        document.Add(new Chunk());

                        //Title page
                        document.NewPage();
                        var starfireText = "Starfire";
                        var starfireBigRectangle = new Rectangle(document.PageSize.Width / 2 - 150, 400f, document.PageSize.Width / 2 + 150, 500f);
                        WriteTextInRectangle(contentByte, starfireText, starfireBigFont, starfireBigRectangle, Element.ALIGN_CENTER);

                        //Back of title page
                        document.NewPage();
                        document.Add(new Chunk());
                        
                        //Title page
                        document.NewPage();
                        var dedicationMessage = "For my beautiful wife.\nWhen compiling this, I was reminded once again how magnetic and truly electric our attraction was, and I marvel at how it continues unabated.\nI love you, now and forever.";
                        var dedicationRectangle = new Rectangle(document.PageSize.Width / 2 - 150, 400f, document.PageSize.Width / 2 + 150, 500f);
                        WriteTextInRectangle(contentByte, dedicationMessage, dedicationFont, dedicationRectangle, Element.ALIGN_CENTER);
                        document.NewPage();

                        //Back of dedication page
                        document.NewPage();
                        document.Add(new Chunk());

                        //Small title page
                        document.NewPage();
                        var starfireSmallRectangle = new Rectangle(document.PageSize.Width / 2 - 150, 400f, document.PageSize.Width / 2 + 150, 500f);
                        WriteTextInRectangle(contentByte, starfireText, starfireSmallFont, starfireSmallRectangle, Element.ALIGN_CENTER);

                        //Back of title page
                        document.NewPage();
                        document.Add(new Chunk());

                        foreach (var chatDay in dates)
                        {

                            pageNumber = AddPage(document, pageNumber, contentByte, pageNumberFont);

                            const float headerFontSize = 30f;
                            var headerFont = new Font(garamondBaseFont, headerFontSize, Font.BOLD);
                            var chatDayDate = chatDay.Date.ToLongDateString();
                            var headerWidth = headerFont.BaseFont.GetWidthPoint(chatDayDate, headerFontSize);

                            const int verticalHeaderPadding = 50;
                            var headerLowerLeftX = document.PageSize.Width / 2 - headerWidth / 2;
                            var headerLowerLeftY = document.PageSize.Height - (verticalHeaderPadding + headerFontSize);
                            var headerUpperRightX = headerLowerLeftX + headerWidth;
                            var headerUpperRightY = document.PageSize.Height - verticalHeaderPadding;
                            var headerRectangle = new Rectangle(headerLowerLeftX, headerLowerLeftY, headerUpperRightX, headerUpperRightY);

                            //DrawRectangle(content, headerRectangle);

                            WriteTextInRectangle(contentByte, chatDayDate, headerFont, headerRectangle, Element.ALIGN_CENTER);
                            var pageLayout = new TextSharpPageLayout();
                            var mainSectionTop = headerRectangle.Bottom - VerticalHeaderPadding;
                            var mainSectionHeight = document.PageSize.Height - (headerRectangle.Height + VerticalHeaderPadding + VerticalHeaderPadding + PageVerticalPadding);
                            pageLayout.InitializeBasePageLayout(mainSectionTop, mainSectionHeight, textFont, sampleDateText, samplePersonText, document.PageSize.Width);
                            var nextTop = pageLayout.TextColumn.Top;
                            foreach (var chatDayLine in chatDay.Lines)
                            {
                                float neededHeightForChatText = 0;
                                var newPage = false;
                                var remainingHeight = nextTop - (LinePadding + textFontSize) - (pageLayout.TextColumn.Bottom);

                                if (remainingHeight <= 0)
                                {
                                    newPage = true;
                                }
                                else
                                {
                                    neededHeightForChatText = HeightRequiredForChat(contentByte, chatDayLine.Text, pageLayout, nextTop, textFont, Element.ALIGN_LEFT);
                                    if (neededHeightForChatText > remainingHeight)
                                        newPage = true;
                                }
                                if (newPage)
                                {
                                    pageNumber = AddPage(document, pageNumber, contentByte, pageNumberFont);
                                    mainSectionTop = document.PageSize.Height - PageVerticalPadding;
                                    mainSectionHeight = document.PageSize.Height - 2 * PageVerticalPadding;
                                    pageLayout = new TextSharpPageLayout();
                                    pageLayout.InitializeBasePageLayout(mainSectionTop, mainSectionHeight, textFont, sampleDateText, samplePersonText, document.PageSize.Width);
                                    nextTop = pageLayout.TextColumn.Top;
                                    neededHeightForChatText = HeightRequiredForChat(contentByte, chatDayLine.Text, pageLayout, nextTop, textFont, Element.ALIGN_LEFT);
                                }
                                var textRect = new Rectangle(pageLayout.TextColumn.Left, nextTop - neededHeightForChatText, pageLayout.TextColumn.Right, nextTop);
                                var dateRect = new Rectangle(pageLayout.DateColumn.Left, nextTop - neededHeightForChatText, pageLayout.DateColumn.Right, nextTop);
                                var personRect = new Rectangle(pageLayout.PersonColumn.Left, nextTop - neededHeightForChatText, pageLayout.PersonColumn.Right, nextTop);
                                //DrawRectangle(content, textRect);
                                //DrawRectangle(content, dateRect);
                                //DrawRectangle(content, personRect);
                                WriteTextInRectangle(contentByte, chatDayLine.Time.ToString(), fontsByTextType[chatDayLine.TextType], dateRect, Element.ALIGN_LEFT);
                                WriteTextInRectangle(contentByte, chatDayLine.Person, fontsByTextType[chatDayLine.TextType], personRect, Element.ALIGN_LEFT);
                                WriteTextInRectangle(contentByte, chatDayLine.Text, fontsByTextType[chatDayLine.TextType], textRect, Element.ALIGN_LEFT);
                                nextTop -= neededHeightForChatText + LinePadding;
                            }
                        }
                        document.Close();
                    }
                }
            }
        }

        private static int AddPage(Document document, int pageNumber, PdfContentByte contentByte, Font pageNumberFont)
        {
            document.NewPage();
            pageNumber++;
            var pageNumberText = $"{pageNumber}";
            //var left = pageNumber % 2 == 0 ? 25 : document.PageSize.Width - 25;
            //var right = pageNumber % 2 == 0 ? 35 : document.PageSize.Width - 15;
            var pageNumberRectangle = new Rectangle(document.PageSize.Width / 2 - 10, 35, document.PageSize.Width / 2 + 10, 45);
            WriteTextInRectangle(contentByte, pageNumberText, pageNumberFont, pageNumberRectangle, Element.ALIGN_CENTER);
            return pageNumber;
        }

        private static void DrawRectangle(PdfContentByte content, Rectangle rectangle)
        {
            content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
            content.Stroke();
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

        private static float HeightRequiredForChat(PdfContentByte contentByte, string text, TextSharpPageLayout textSharpPageLayout, float nextTop, Font font, int alignment)
        {
            var lines = 1;
            while (true)
            {
                var nextTryRectangle = new Rectangle(textSharpPageLayout.TextColumn.Left, nextTop - (lines * font.Size), textSharpPageLayout.TextColumn.Right, nextTop);
                if (WriteTextInRectangle(contentByte, text, font, nextTryRectangle, alignment, true))
                {
                    return font.Size * lines;
                }
                lines++;
            }
        }
    }
}
