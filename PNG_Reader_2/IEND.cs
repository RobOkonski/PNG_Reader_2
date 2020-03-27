using System;
using System.Collections.Generic;
using System.Text;

namespace PNG_Reader_2
{
    public class IEND : Chunk
    {
        public IEND(Chunk chunk)
        {
            byteLength = chunk.byteLength;
            byteSign = chunk.byteSign;
            byteData = chunk.byteData;
            byteCheckSum = chunk.byteCheckSum;
            length = chunk.length;
            sign = chunk.sign;
        }

        public override void Display()
        {
            Console.WriteLine("\n[{0}]\n", sign);
        }
    }
}
