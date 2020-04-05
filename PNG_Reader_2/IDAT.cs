using System;
using System.Collections.Generic;
using System.Text;

namespace PNG_Reader_2
{
    public class IDAT : Chunk
    {
        public int number;
        public Compression compression;
        public IDAT(Chunk chunk, int idatQuantity)
        {
            byteLength = chunk.byteLength;
            byteSign = chunk.byteSign;
            byteData = chunk.byteData;
            byteCheckSum = chunk.byteCheckSum;
            length = chunk.length;
            sign = chunk.sign;
            number = idatQuantity;

            if(idatQuantity==1)
            {
                string[] data = BitConverter.ToString(byteData).Split("-");
                compression = new Compression(data[0], byteData[1]);
            }
        }

        public override void Display()
        {
            if(number==1)
            {
                Console.WriteLine("\n[{0}]\n", sign);
                compression.Display();
            }
            Console.WriteLine(" - {0}. byteLength: {1}", number, length);
        }
    }
}
