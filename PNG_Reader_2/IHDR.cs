using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace PNG_Reader_2
{
    class IHDR : Chunk
    {
        public int width;
        public int height;
        public int bitDepth;
        public int colorType;
        public int compresionMethod;
        public int filterMethod;
        public int interlanceMethod;

        public IHDR(Chunk chunk)
        {
            byteLength = chunk.byteLength;
            byteSign = chunk.byteSign;
            byteData = chunk.byteData;
            byteCheckSum = chunk.byteCheckSum;
            length = chunk.length;
            sign = chunk.sign;

            string[] data = BitConverter.ToString(byteData).Split("-");
            width = Int32.Parse(data[0] + data[1] + data[2] + data[3], System.Globalization.NumberStyles.HexNumber);
            height = Int32.Parse(data[4] + data[5] + data[6] + data[7], System.Globalization.NumberStyles.HexNumber);
            bitDepth = Int32.Parse(data[8], System.Globalization.NumberStyles.HexNumber);
            colorType = Int32.Parse(data[9], System.Globalization.NumberStyles.HexNumber);
            compresionMethod = Int32.Parse(data[10], System.Globalization.NumberStyles.HexNumber);
            filterMethod = Int32.Parse(data[11], System.Globalization.NumberStyles.HexNumber);
            interlanceMethod = Int32.Parse(data[12], System.Globalization.NumberStyles.HexNumber);
        }

        public override void Display()
        {
            Console.WriteLine("\n[{0}] byteLength: {1}\n", sign, length);
            Console.WriteLine(" - width: {0}", width);
            Console.WriteLine(" - height: {0}", height);
            Console.WriteLine(" - bitDepth: {0}", bitDepth);
            Console.WriteLine(" - colorType: {0}", colorType);
            Console.WriteLine(" - compresionMethod: {0}", compresionMethod);
            Console.WriteLine(" - filterMethod: {0}", filterMethod);
            Console.WriteLine(" - interlanceMethod: {0}", interlanceMethod);
        }
    }
}
