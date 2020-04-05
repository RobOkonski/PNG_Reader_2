using System;

namespace PNG_Reader_2
{
    public class gAMA : Chunk
    {
        public double gama;

        public gAMA(Chunk chunk)
        {
            byteLength = chunk.byteLength;
            byteSign = chunk.byteSign;
            byteData = chunk.byteData;
            byteCheckSum = chunk.byteCheckSum;
            length = chunk.length;
            sign = chunk.sign;

            gama = ((double)Int32.Parse(BitConverter.ToString(byteData).Replace("-", ""), System.Globalization.NumberStyles.HexNumber))/100000;
        }

        public override void Display()
        {
            Console.WriteLine("\n[{0}]\n", sign);
            Console.WriteLine(" - gama: {0}", gama);
        }

    }
}
