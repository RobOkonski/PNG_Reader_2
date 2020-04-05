using System;
using System.Collections.Generic;
using System.Text;

namespace PNG_Reader_2
{
    public class tEXt : Chunk
    {
        public string keyword;
        public string text;

        public tEXt(Chunk chunk)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");

            byteLength = chunk.byteLength;
            byteSign = chunk.byteSign;
            byteData = chunk.byteData;
            byteCheckSum = chunk.byteCheckSum;
            length = chunk.length;
            sign = chunk.sign;

            int i = 0;
            while (byteData[i] != 0)
            {
                i++;
            }
            Console.WriteLine(i);

            keyword = ascii.GetString(byteData, 0, i);
            text = iso.GetString(byteData, i+1, length-i-1);
        }

        public override void Display()
        {
            Console.WriteLine("\n[{0}] byteLength: {1}\n", sign, length);
            Console.WriteLine(" - keyword: {0}", keyword);
            Console.WriteLine(" - text: {0}", text);
        }
    }
}

