using System;

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

            if (colorType==0) Console.WriteLine(" - colorType: {0} - grayscale, allowed bit depths: 1, 2, 4, 8, 16", colorType);
            else if (colorType == 2) Console.WriteLine(" - colorType: {0} - truecolour, allowed bit depths: 8, 16", colorType);
            else if (colorType == 3) Console.WriteLine(" - colorType: {0} - indexed-colour, allowed bit depths: 1, 2, 4, 8", colorType);
            else if (colorType == 4) Console.WriteLine(" - colorType: {0} - greyscale with alpha, allowed bit depths: 8, 16", colorType);
            else if (colorType == 6) Console.WriteLine(" - colorType: {0} - truecolour with aplha, allowed bit depths: 8, 16", colorType);
            else Console.WriteLine("error");

            if (compresionMethod==0) Console.WriteLine(" - compresionMethod: {0} - deflate/inflate", compresionMethod);
            else Console.WriteLine("error");

            if (filterMethod==0) Console.WriteLine(" - filterMethod: {0} - adaptive filtering with five basic filter types", filterMethod);
            else Console.WriteLine("error");

            if (interlanceMethod==0) Console.WriteLine(" - interlanceMethod: {0} - no interlace", interlanceMethod);
            else if (interlanceMethod==1) Console.WriteLine(" - interlanceMethod: {0} - Adam7 interlace", interlanceMethod);
            else Console.WriteLine("error");
        }
    }
}
