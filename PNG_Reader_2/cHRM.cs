using System;

namespace PNG_Reader_2
{
    public class cHRM : Chunk
    {
        public double whitePointX;
        public double whitePointY;
        public double redX;
        public double redY;
        public double greenX;
        public double greenY;
        public double blueX;
        public double blueY;

        public cHRM(Chunk chunk)
        {
            byteLength = chunk.byteLength;
            byteSign = chunk.byteSign;
            byteData = chunk.byteData;
            byteCheckSum = chunk.byteCheckSum;
            length = chunk.length;
            sign = chunk.sign;
            
            string[] data = BitConverter.ToString(byteData).Split("-");

            whitePointX = ((double)Int32.Parse(data[0] + data[1] + data[2] + data[3], System.Globalization.NumberStyles.HexNumber)) / 100000;
            whitePointY = ((double)Int32.Parse(data[4] + data[5] + data[6] + data[7], System.Globalization.NumberStyles.HexNumber)) / 100000;
            redX = ((double)Int32.Parse(data[8] + data[9] + data[10] + data[11], System.Globalization.NumberStyles.HexNumber)) / 100000;
            redY = ((double)Int32.Parse(data[12] + data[13] + data[14] + data[15], System.Globalization.NumberStyles.HexNumber)) / 100000;
            greenX = ((double)Int32.Parse(data[16] + data[17] + data[18] + data[19], System.Globalization.NumberStyles.HexNumber)) / 100000;
            greenY = ((double)Int32.Parse(data[20] + data[21] + data[22] + data[23], System.Globalization.NumberStyles.HexNumber)) / 100000;
            blueX = ((double)Int32.Parse(data[24] + data[25] + data[26] + data[27], System.Globalization.NumberStyles.HexNumber)) / 100000;
            blueY = ((double)Int32.Parse(data[28] + data[29] + data[30] + data[31], System.Globalization.NumberStyles.HexNumber)) / 100000;
        }

        public override void Display()
        {
            Console.WriteLine("\n[{0}]\n", sign);
            Console.WriteLine(" - whitePointX: {0}", whitePointX);
            Console.WriteLine(" - whitePointY: {0}", whitePointY);
            Console.WriteLine(" - redX: {0}", redX);
            Console.WriteLine(" - redY: {0}", redY);
            Console.WriteLine(" - greenX: {0}", greenX);
            Console.WriteLine(" - greenY: {0}", greenY);
            Console.WriteLine(" - blueX: {0}", blueX);
            Console.WriteLine(" - blueY: {0}", blueY);
        }
    }
}
