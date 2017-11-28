namespace StarfireParser
{
    public abstract class BasePageLayout<ColumnType, FontType, NumericType>
    {
        public ColumnType DateColumn { get; set; }
        public ColumnType PersonColumn { get; set; }
        public ColumnType TextColumn { get; set; }

        protected abstract ColumnType CreateColumn(NumericType left, NumericType top, NumericType width, NumericType height);
        protected abstract NumericType MeasureTextWidth(string text, FontType font);
        protected abstract NumericType GetColumnWidth(ColumnType column);
        protected abstract NumericType GetColumnRight(ColumnType column);
        protected abstract NumericType GetColumnPadding();
        protected abstract NumericType GetPageHorizontalPadding();
        protected abstract NumericType Add(params NumericType[] args);
        protected abstract NumericType Subtract(NumericType var1, NumericType var2);

        public void InitializeBasePageLayout(NumericType mainSectionTop, NumericType mainSectionHeight, FontType textFont, string sampleDateText, string samplePersonText, NumericType pageWidth)
        {
            DateColumn = CreateDateColumn(sampleDateText, textFont, mainSectionTop, mainSectionHeight);
            PersonColumn = CreatePersonColumn(samplePersonText, textFont, GetColumnRight(DateColumn), mainSectionTop, mainSectionHeight);
            TextColumn = CreateTextColumn(GetColumnRight(PersonColumn), pageWidth, GetColumnWidth(PersonColumn), GetColumnWidth(DateColumn), mainSectionTop, mainSectionHeight);
        }

        private ColumnType CreateTextColumn(NumericType personColumnRight, NumericType pageWidth, NumericType personColumnWidth,
            NumericType dateColumnWidth, NumericType mainSectionTop, NumericType mainSectionHeight)
        {
            var textColumnLeft = Add(personColumnRight, GetColumnPadding());
            var textColumnWidth = Subtract(pageWidth, Add(GetPageHorizontalPadding(), GetPageHorizontalPadding(), GetColumnPadding(), GetColumnPadding(), personColumnWidth, dateColumnWidth));
            var textColumn = CreateColumn(textColumnLeft, mainSectionTop, textColumnWidth, mainSectionHeight);
            return textColumn;
        }

        private ColumnType CreatePersonColumn(string samplePersonText, FontType textFont,
            NumericType dateColumnRight, NumericType mainSectionTop, NumericType mainSectionHeight)
        {
            var personColumnLeft = Add(dateColumnRight, GetColumnPadding());
            var personColumnWidth = MeasureTextWidth(samplePersonText, textFont);
            var personColumn = CreateColumn(personColumnLeft, mainSectionTop, personColumnWidth, mainSectionHeight);
            return personColumn;
        }

        private ColumnType CreateDateColumn(string sampleDateText, FontType textFont, NumericType mainSectionTop,
            NumericType mainSectionHeight)
        {
            var dateColumnLeft = GetPageHorizontalPadding();
            var dateColumnWidth = MeasureTextWidth(sampleDateText, textFont);
            var dateColumn = CreateColumn(dateColumnLeft, mainSectionTop, dateColumnWidth, mainSectionHeight);
            return dateColumn;
        }
    }
}
