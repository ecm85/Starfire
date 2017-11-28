using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace StarfireParser
{
    public class TextSharpPageLayout : BasePageLayout<Rectangle, Font, float>
    {
        protected override Rectangle CreateColumn(float left, float top, float width, float height)
        {
            return new Rectangle(left, top - height, left + width, top);
        }

        protected override float MeasureTextWidth(string text, Font font)
        {
            return font.BaseFont.GetWidthPoint(text, font.Size);
        }

        protected override float GetColumnWidth(Rectangle column)
        {
            return column.Width;
        }

        protected override float GetColumnRight(Rectangle column)
        {
            return column.Right;
        }

        protected override float GetColumnPadding()
        {
            return TestSharpGenerator.ColumnPadding;
        }

        protected override float GetPageHorizontalPadding()
        {
            return TestSharpGenerator.PageHorizontalPadding;
        }

        protected override float Add(params float[] args)
        {
            return args.Sum();
        }

        protected override float Subtract(float var1, float var2)
        {
            return var1 - var2;
        }
    }
}
