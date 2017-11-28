using System.Linq;
using PdfSharp.Drawing;

namespace StarfireParser
{
    public class PdfSharpPageLayout : BasePageLayout<XRect, XFont, double>
    {
        private XGraphics XGraphics { get; }

        public PdfSharpPageLayout(XGraphics xGraphics)
        {
            XGraphics = xGraphics;
        }

        protected override XRect CreateColumn(double left, double top, double width, double height)
        {
            return new XRect(left, top, width, height);
        }

        protected override double MeasureTextWidth(string text, XFont font)
        {
            return XGraphics.MeasureString(text, font).Width;
        }

        protected override double GetColumnWidth(XRect column)
        {
            return column.Width;
        }

        protected override double GetColumnRight(XRect column)
        {
            return column.Right;
        }

        protected override double GetColumnPadding()
        {
            return PdfSharpGenerator.ColumnPadding;
        }

        protected override double GetPageHorizontalPadding()
        {
            return PdfSharpGenerator.PageHorizontalPadding;
        }

        protected override double Add(params double[] args)
        {
            return args.Sum();
        }

        protected override double Subtract(double var1, double var2)
        {
            return var1 - var2;
        }
    }
}
