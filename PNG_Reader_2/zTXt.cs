using System;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Text;

namespace PNG_Reader_2
{
    public class zTXt : Chunk
    {
        public string keyword;
        public int compressionMethod;
        public string text;

        public zTXt(Chunk chunk)
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
            while(byteData[i]!=0)
            {
                i++;
            }

            keyword = ascii.GetString(byteData,0,i);

            compressionMethod = byteData[i+1];

            byte[] byteText = new byte[length-i-2];
            for(int j=0; j<length-i-2; j++)
            {
                byteText[j] = byteData[i+2+j];
            }

            Inflater infl = new Inflater();
            infl.SetInput(byteText);
            byte[] decompressedByteText = new byte[100000];
            infl.Inflate(decompressedByteText);

            int k = 0;
            while (decompressedByteText[k] != 0)
            {
                k++;
            }
            text = iso.GetString(decompressedByteText, 0, k);
        }

        public override void Display()
        {
            Console.WriteLine("\n[{0}] byteLength: {1}\n", sign, length);
            Console.WriteLine(" - keyword: {0}",keyword);

            if (compressionMethod == 0) Console.WriteLine(" - compressionMethod: {0} - deflate/inflate", compressionMethod);
            else Console.WriteLine("error");

            Console.WriteLine(" - text: {0}",text);
        }
    }
}
