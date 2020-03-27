using System;
using System.IO;
using System.Collections.Generic;

namespace PNG_Reader_2
{
    class Program
    {
        public static void Main(string[] args)
        {
            PNG_signs signs = new PNG_signs();
            Queue<Chunk> chunks = new Queue<Chunk>();

            string fileName = "data\\adaptive.png";
            string fileDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            string filePath = Path.Combine(fileDir, fileName);

            BinaryReader Picture = new BinaryReader(File.OpenRead(filePath));

            if(!(signs.IsPNG(Picture)))
            {
                Console.WriteLine("Wczytany plik nie jest obrazem PNG");
            }
            Console.WriteLine("[PNG]");

            Read(Picture,chunks);

            while(chunks.Count != 0)
            {
                chunks.Dequeue().Display();
            }
        }

        public static void Read(BinaryReader Picture, Queue<Chunk> chunks)
        {
            bool endOfFile = true;

            do
            {

                Chunk chunk = new Chunk();
                chunk.Read(Picture);

                if (chunk.sign == "IHDR")
                {
                    IHDR ihdr = new IHDR(chunk);
                    chunks.Enqueue(ihdr);
                }
                else if (chunk.sign == "PLTE")
                {
                    PLTE plte = new PLTE(chunk);
                    chunks.Enqueue(plte);
                }
                else if (chunk.sign == "gAMA")
                {
                    gAMA gama = new gAMA(chunk);
                    chunks.Enqueue(gama);
                }
                else if (chunk.sign == "cHRM")
                {
                    cHRM chrm = new cHRM(chunk);
                    chunks.Enqueue(chrm);
                }
                else if (chunk.sign == "IDAT")
                {
                    IDAT idat = new IDAT(chunk);
                    chunks.Enqueue(idat);
                }
                else if (chunk.sign == "IEND")
                {
                    IEND iend = new IEND(chunk);
                    chunks.Enqueue(iend);
                    endOfFile = false;
                }
                else chunks.Enqueue(chunk);

            } while (endOfFile);
        }
    }
}
