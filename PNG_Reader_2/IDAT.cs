using System;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Linq;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO.Compression;
//using ComponentAce.Compression.Libs.zlib;

namespace PNG_Reader_2
{
    public class IDAT : Chunk
    {
        public int number;
        public Compression compression;
        public byte[] decopressedData;
        public IDAT(Chunk chunk, int idatQuantity)
        {
            byteLength = chunk.byteLength;
            byteSign = chunk.byteSign;
            byteData = chunk.byteData;
            byteCheckSum = chunk.byteCheckSum;
            length = chunk.length;
            sign = chunk.sign;
            number = idatQuantity;

            /*Inflater infl = new Inflater();
            infl.SetInput(byteData);
            byte[] decompressedByteData = new byte[100000];
            infl.Inflate(decompressedByteData);

            decopressedData = decompressedByteData;*/

            decopressedData = Decompress(byteData);

            if (idatQuantity==1)
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

            Console.WriteLine("{0} {1}", byteData.Length, decopressedData.Length);
        }

        public override void Write(BinaryWriter NewPicture)
        {
            Deflater defl = new Deflater();
            defl.SetInput(decopressedData);
            defl.SetLevel(compression.FLEVEL);
            byte[] compressedData = new byte[100000];
            defl.Deflate(compressedData);
            //byte[] data = compressedData.Where(x => x != 0).ToArray();

            //byte[] compressedData = Compress(decopressedData);

            Console.WriteLine("{0} {1}",compressedData.Length, byteData.Length);
            /*foreach (byte b in compressedData) Console.Write(b);
            Console.WriteLine("\n\n");
            foreach (byte b in byteData) Console.Write(b);
            Console.WriteLine("");*/

            NewPicture.Write(BitConverter.GetBytes(compressedData.Length));
            NewPicture.Write(byteSign);
            NewPicture.Write(compressedData);
            NewPicture.Write(byteCheckSum);
            
        }

        public override byte[] ReturnData()
        {
            return decopressedData;
        }

        public byte[] Decompress(byte[] data)
        {
            var outputStream = new MemoryStream();
            byte[] decompressedData;
            using (var compressedStream = new MemoryStream(data))
            using (var inputStream = new InflaterInputStream(compressedStream))
            {
                inputStream.CopyTo(outputStream);
                outputStream.Position = 0;
                decompressedData = outputStream.ToArray();
                return decompressedData;
            }
        }

        /*public byte[] Compress(byte[] data)
        {
            Deflater defl = new Deflater(0,false);
            var decompressedStream = new MemoryStream(data);
            byte[] compressedData;
            using (var outputStream = new MemoryStream())
            using (var inputStream = new DeflaterOutputStream(outputStream,defl))
            {
                decompressedStream.CopyTo(inputStream);
                outputStream.Position = 0;
                compressedData = outputStream.ToArray();
                return compressedData;
            }
        }*/
    }
}
