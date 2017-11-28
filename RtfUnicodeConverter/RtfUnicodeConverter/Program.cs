using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RtfUnicodeConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            //\u-10179?\u-8629? -> \u+1F408
            ConvertHexStringToUtf16("1F5FD");
            Console.WriteLine("\n\n-----------\n\n");
            Console.WriteLine(ConvertRtfUnicodeToHex(@"\u-10179?\u-8707?"));
            //const string input = @"\u-10179?\u-8629?";
            //should get to d8 3d, dc 08
            //var tokens = input.Split(new[] { @"\u-" }, StringSplitOptions.RemoveEmptyEntries);

            //firstHalf -= Int32.Parse("D800", NumberStyles.HexNumber);
            //secondHalf -= Int32.Parse("DC00", NumberStyles.HexNumber);
            //var firstHalfBinary = Convert.ToString(firstHalf, 2);
            //var secondHalfBinary = Convert.ToString(secondHalf, 2);
            //Console.WriteLine(firstHalfBinary);
            //Console.WriteLine(secondHalfBinary);
            //Console.WriteLine(firstHalfBinary.Substring(0, 8) + secondHalfBinary.Substring(8));
            //var extraPart = Int32.Parse("10000", NumberStyles.HexNumber);
            //var sum = (firstHalf | secondHalf) + extraPart;
            //var output = Convert.ToString(sum, 16);
            //Console.WriteLine(output);
            //128008
            //const string expectedOutput = @"\u+10437";
            //var expectedOutputValue = int.Parse(expectedOutput.Substring(3), NumberStyles.HexNumber);
            //var binary = Convert.ToString(expectedOutputValue, 2);
            //Console.WriteLine(binary);
        }

        private static void ConvertHexStringToUtf16(string input)
        {
            var x = int.Parse(input, NumberStyles.HexNumber);
            x -= int.Parse("10000", NumberStyles.HexNumber);
            var xBinary = Convert.ToString(x, 2);
            Console.WriteLine(xBinary);
            var firstBits = xBinary.Substring(0, xBinary.Length - 10);
            var secondBits = xBinary.Substring(firstBits.Length);
            var firstBitsValue = Convert.ToInt32(firstBits, 2);
            var secondBitsValue = Convert.ToInt32(secondBits, 2);
            Console.WriteLine(firstBitsValue);
            Console.WriteLine(secondBitsValue);
            firstBitsValue += Int32.Parse("D800", NumberStyles.HexNumber);
            secondBitsValue += Int32.Parse("DC00", NumberStyles.HexNumber);
            Console.WriteLine(Convert.ToString(firstBitsValue, 16));
            Console.WriteLine(Convert.ToString(secondBitsValue, 16));
            Console.WriteLine(firstBitsValue);
            Console.WriteLine(secondBitsValue);
            Console.WriteLine((short)firstBitsValue);
            Console.WriteLine((short)secondBitsValue);
            Console.WriteLine((ushort)(short)firstBitsValue);
            Console.WriteLine((ushort)(short)secondBitsValue);
            firstBitsValue = ushort.MaxValue + 1 - firstBitsValue;
            secondBitsValue = ushort.MaxValue + 1 - secondBitsValue;
            Console.WriteLine(firstBitsValue);
            Console.WriteLine(secondBitsValue);
        }

        private static string ConvertRtfUnicodeToHex(string input)
        {
            var tokens = input.Split(new[] { @"\u" }, StringSplitOptions.RemoveEmptyEntries);
            var firstToken = tokens[0].Replace("?", "");
            var secondToken = tokens[1].Replace("?", "");

            var firstHalf = (ushort)int.Parse(firstToken);
            var secondHalf = (ushort)int.Parse(secondToken);
            Console.WriteLine(firstHalf);
            Console.WriteLine(secondHalf);
            firstHalf -= ushort.Parse("D800", NumberStyles.HexNumber);
            secondHalf -= ushort.Parse("DC00", NumberStyles.HexNumber);
            Console.WriteLine(firstHalf);
            Console.WriteLine(secondHalf);
            var binaryString = Convert.ToString(firstHalf, 2) + PadWithLeadingZeros(Convert.ToString(secondHalf, 2), 10);
            Console.WriteLine(binaryString);
            var binary = Convert.ToInt32(binaryString, 2);
            binary += int.Parse("10000", NumberStyles.HexNumber);
            return Convert.ToString(binary, 16);
        }

        private static string PadWithLeadingZeros(string value, int total)
        {
            return new string('0', total - value.Length) + value;
        }

        //Parse each (including negative sign!) 
        //Cast each to UShort
        //subtract hex of "D800" from first
        //subtract hex of "DC00" from second
        //convert both to binary and concatenate
        //add binary of "10000"
        //convert back to hex

    }
}
