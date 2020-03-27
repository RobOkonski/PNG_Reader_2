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

            string fileName = "data\\camaro.png";
            string fileDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            string filePath = Path.Combine(fileDir, fileName);

            BinaryReader Picture = new BinaryReader(File.OpenRead(filePath));

            if(!(signs.IsPNG(Picture)))
            {
                Console.WriteLine("Wczytany plik nie jest obrazem PNG");
            }
            Console.WriteLine("Wczytano obraz PNG");

            bool endOfFile = true;

            do {
                Chunk chunk = new Chunk();
                chunk.Read(Picture);
                chunk.Display();
                if (chunk.sign == "IEND") endOfFile = false;
            } while (endOfFile);
        }
    }
}
