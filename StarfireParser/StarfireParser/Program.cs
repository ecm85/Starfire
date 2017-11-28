using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StarfireParser
{
    class Program
    {


        static void Main()
        {
            var lines = File.ReadAllLines(@"C:\Users\Ezramc\Desktop\Starfire\ezra and sarahs txt for parsing.txt")
                .Select((line, index) => new Line { Index = index, Text = line.Trim()})
                .ToDictionary(line => line.Index);

            MoveAllColorsToStartOfLine(lines);

            FillInMissingColors(lines);

            RemoveExtraStartingBlackColor(lines);

            var dates = GroupIntoDatesAndTextLines(lines);

            FixMissingLinesBySarahType(dates);

            //var pdfSharpGenerator = new PdfSharpGenerator();
            //pdfSharpGenerator.GeneratePdf(dates);
            var textSharpGenerator = new TestSharpGenerator();
            textSharpGenerator.GeneratePdf(dates);

            File.WriteAllLines(@"C:\Users\Ezramc\Desktop\Starfire\output.txt", dates.SelectMany(date => date.GetTextLines()).ToList());
        }

        private static void FixMissingLinesBySarahType(List<ChatDay> dates)
        {
            var linesBySarahWithEzraType = dates
                .SelectMany(date => date.Lines)
                .Where(chatLine =>
                    (chatLine.TextType == TextType.EzraChat || chatLine.TextType == TextType.EzraPersonal) &&
                    chatLine.Person.ToLower() != "ezra")
                .ToList();
            foreach (var chatLine in linesBySarahWithEzraType)
            {
                chatLine.TextType = TextType.SarahChat;
            }
        }

        private static List<ChatDay> GroupIntoDatesAndTextLines(Dictionary<int, Line> lines)
        {
            var dates = new List<ChatDay>();
            foreach (var line in lines.Values)
            {
                DateTime date;
                if (DateTime.TryParse(line.Text.Substring(4), out date))
                {
                    dates.Add(new ChatDay
                    {
                        TextType = TextType.Date,
                        Date = date.Date,
                        Lines = new List<ChatLine>()
                    });
                }
                else
                {
                    dates.Last().Lines.Add(new ChatLine(line.Text));
                }
            }
            return dates;
        }

        private static void RemoveExtraStartingBlackColor(Dictionary<int, Line> lines)
        {
            var linesWithMultipleColors = lines.Values
                .Where(line => line.Text.IndexOf(@"\cf") != line.Text.LastIndexOf(@"\cf")).ToList();
            foreach (var line in linesWithMultipleColors)
            {
                line.Text = line.Text.Substring(5);
            }
        }

        private static void FillInMissingColors(Dictionary<int, Line> lines)
        {
            string currentColor = null;
            foreach (var line in lines.Values)
            {
                if (!line.Text.StartsWith(@"\cf"))
                {
                    line.Text = $"{currentColor} {line.Text}";
                }
                else
                {
                    currentColor = line.Text.Split(' ')[0];
                }
            }
        }

        private static void MoveAllColorsToStartOfLine(Dictionary<int, Line> lines)
        {
            foreach (var line in lines.Values)
            {
                var lineTokens = line.Text.Split(' ');
                if (lineTokens.Last().StartsWith(@"\cf"))
                {
                    if (lines[line.Index + 1].Text.StartsWith(@"\cf"))
                    {
                        if (!line.Text.EndsWith(@"\cf0"))
                            throw new InvalidOperationException();
                    }
                    else
                    {
                        var colorCode = lineTokens.Last();
                        lines[line.Index + 1].Text = colorCode + " " + lines[line.Index + 1].Text;
                    }
                    lines[line.Index].Text = string.Join(" ", lineTokens.Take(lineTokens.Length - 1));
                }
            }
        }
    }

    public class Line
    {
        public string Text { get; set; }
        public int Index { get; set; } 
    }

    public class ChatDay
    {
        public DateTime Date { get; set; }
        public TextType TextType { get; set; }
        public IList<ChatLine> Lines { get; set; }

        public IEnumerable<string> GetTextLines()
        {
            return new[] {$"[{TextType}] [{Date}]"}.Concat(Lines.Select(line => line.GetTextLine()));
        }
    }

    public class ChatLine
    {
        public ChatLine(string lineText)
        {
            var lineTokens = lineText.Split(' ');
            var textTypeSegment = lineTokens[0];
            var timeSegment = lineTokens[1];
            var personSegment = lineTokens[2];
            var textSegment = string.Join(" ", lineTokens.Skip(3));
            if (string.IsNullOrWhiteSpace(textSegment))
                textSegment = " ";
            while (textSegment.Contains(@"\u-"))
                textSegment = ReplaceNextRtfUnicode(textSegment);
            while (textSegment.Contains(@"\u"))
                textSegment = ReplaceNextShortUnicode(textSegment);
            Person = personSegment;
            Text = textSegment;
            Time = DateTime.Parse(timeSegment).TimeOfDay;
            TextType = GetTextType(textTypeSegment);
        }

        private string ReplaceNextShortUnicode(string textSegment)
        {
            var startOfNextUnicodeCharacter = textSegment.IndexOf(@"\u");
            var endOfNextUnicodeCharacter = startOfNextUnicodeCharacter;
            var endOfUnicodeCharactersFound = 0;
            while (endOfUnicodeCharactersFound < 1)
            {
                endOfNextUnicodeCharacter++;
                if (textSegment[endOfNextUnicodeCharacter] == '?')
                    endOfUnicodeCharactersFound++;
            }
            var nextRtfUnicodeCharacter = textSegment.Substring(startOfNextUnicodeCharacter, endOfNextUnicodeCharacter - startOfNextUnicodeCharacter + 1);
            var code = nextRtfUnicodeCharacter.Substring(2);
            code = code.Replace("?", "");
            var unicode = DecodeEncodedNonAsciiCharacters(Convert.ToString(Convert.ToInt32(code), 16));
            return textSegment.Replace(nextRtfUnicodeCharacter, unicode);
        }

        private static string ReplaceNextRtfUnicode(string textSegment)
        {
            var startOfNextUnicodeCharacter = textSegment.IndexOf(@"\u-");
            var endOfNextUnicodeCharacter = startOfNextUnicodeCharacter;
            var endOfUnicodeCharactersFound = 0;
            while (endOfUnicodeCharactersFound < 2)
            {
                endOfNextUnicodeCharacter++;
                if (textSegment[endOfNextUnicodeCharacter] == '?')
                    endOfUnicodeCharactersFound++;
            }
            var nextRtfUnicodeCharacter = textSegment.Substring(startOfNextUnicodeCharacter, endOfNextUnicodeCharacter - startOfNextUnicodeCharacter + 1);
            var unicode = ConvertRtfUnicodeToHex(nextRtfUnicodeCharacter);
            var decodedUnicode = DecodeEncodedNonAsciiCharacters(unicode);
            return textSegment.Replace(nextRtfUnicodeCharacter, decodedUnicode);
        }

        private static string ConvertRtfUnicodeToHex(string input)
        {
            var tokens = input.Split(new[] { @"\u" }, StringSplitOptions.RemoveEmptyEntries);
            var firstToken = tokens[0].Replace("?", "");
            var secondToken = tokens[1].Replace("?", "");

            var firstHalf = (ushort)int.Parse(firstToken);
            var secondHalf = (ushort)int.Parse(secondToken);
            firstHalf -= ushort.Parse("D800", NumberStyles.HexNumber);
            secondHalf -= ushort.Parse("DC00", NumberStyles.HexNumber);
            var binary = Convert.ToInt32(Convert.ToString(firstHalf, 2) + PadWithLeadingZeros(Convert.ToString(secondHalf, 2), 10), 2);
            binary += int.Parse("10000", NumberStyles.HexNumber);
            return Convert.ToString(binary, 16);
        }

        private static string PadWithLeadingZeros(string value, int total)
        {
            return new string('0', total - value.Length) + value;
        }

        static string DecodeEncodedNonAsciiCharacters(string text)
        {
            var value = int.Parse(text, NumberStyles.HexNumber);
            return char.ConvertFromUtf32(value);
        }

        private TextType GetTextType(string textTypeSegment)
        {
            switch (textTypeSegment.Substring(3))
            {
                case "0":
                    return TextType.EzraChat;
                case "1":
                    return TextType.Date;
                case "2":
                    return TextType.SarahChat;
                case "3":
                    return TextType.MusicAndLinkChat;
                case "4":
                    throw new InvalidOperationException();
                case "5":
                    return TextType.RomanticChat;
                case "6":
                    return TextType.RomanticChat;
                case "7":
                    throw new InvalidOperationException();
                case "8":
                    return TextType.EzraPersonal;
                case "9":
                    return TextType.SarahPersonal;
                case "10":
                    return TextType.EzraPersonal;
                case "11":
                    return TextType.EzraPersonal;
                case "12":
                    return TextType.FunnyChat;
                case "13":
                    return TextType.RomanticChat;
                default:
                    throw new InvalidOperationException();
            }
        }

        public string Person { get; }
        public TimeSpan Time { get; }
        public string Text { get; }
        public TextType TextType { get; set; }

        public string GetTextLine()
        {
            return $"[{TextType}][{Time}][{Person}]: {Text}";
        }
    }

    public enum TextType
    {
        EzraChat,
        SarahChat,
        Date,
        FunnyChat,
        RomanticChat,
        MusicAndLinkChat,
        SarahPersonal,
        EzraPersonal
    }
}
