using System;
using System.Collections.Generic;
using System.Text;

namespace PNG_Reader_2
{
    public class PLTE : Chunk
    {
        public int colorQuantity;

        public PLTE(Chunk chunk)
        {
            byteLength = chunk.byteLength;
            byteSign = chunk.byteSign;
            byteData = chunk.byteData;
            byteCheckSum = chunk.byteCheckSum;
            length = chunk.length;
            sign = chunk.sign;

            colorQuantity = length / 3;
        }

        public override void Display()
        {
            Console.WriteLine("\n[{0}]\n", sign);
            Console.WriteLine(" - colorQuantity: {0}", colorQuantity);
        }
    }
}
