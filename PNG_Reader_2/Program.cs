using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using Extreme.Mathematics.SignalProcessing;
//using ComponentAce.Compression.Libs.zlib;
//using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Diagnostics;

namespace PNG_Reader_2
{
    class Program
    {
        public static void Main(string[] args)
        {
            PNG_signs signs = new PNG_signs();
            Queue<Chunk> chunks = new Queue<Chunk>();
            Queue<Chunk> chunksToWrite = new Queue<Chunk>();

            string fileName = "data\\exif.png";
            string newFileName = "data\\test.png";

            Read(signs, chunks, chunksToWrite, fileName);
            Display(chunks);
            Write(signs, chunksToWrite, newFileName);

            DisplayImage(fileName);
        }

        public static void DisplayImage(string fileName)
        {
            string fileDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            string filePath = Path.Combine(fileDir, fileName);

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Path.Combine("ms-photos://",filePath);
            info.UseShellExecute = true;
            info.CreateNoWindow = true;
            info.Verb = string.Empty;

            Process.Start(info);

            /*Console.WriteLine("Wyświetlam obraz");
            Bitmap picture = new Bitmap(filePath);
            Graphics graphics = Graphics.FromImage(picture);
            graphics.DrawImage(picture,0,0);
            Console.WriteLine("Wyświetliłem obraz");*/

        }

        public static void Display(Queue<Chunk> chunks)
        {
            Console.WriteLine("[PNG]");
            while (chunks.Count != 0)
            {
                chunks.Dequeue().Display();
            }
        }

        public static void Write(PNG_signs signs, Queue<Chunk> chunks, string fileName)
        {
            string fileDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            string filePath = Path.Combine(fileDir, fileName);

            BinaryWriter NewPicture = new BinaryWriter(File.OpenWrite(filePath));

            NewPicture.Write(signs.bytePNG_sign);
            while (chunks.Count != 0)
            {
                chunks.Dequeue().Write(NewPicture);
            }
            NewPicture.Close();
        }

        public static void Read(PNG_signs signs, Queue<Chunk> chunks, Queue<Chunk> chunksToWrite, string fileName)
        {
            Inflater infl = new Inflater();

            string fileDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            string filePath = Path.Combine(fileDir, fileName);

            BinaryReader Picture = new BinaryReader(File.OpenRead(filePath));

            if (!(signs.IsPNG(Picture)))
            {
                Console.WriteLine("Wczytany plik nie jest obrazem PNG");
            }

            bool endOfFile = true;

            do
            {

                Chunk chunk = new Chunk();
                chunk.Read(Picture);

                if (chunk.sign == "IHDR")
                {
                    IHDR ihdr = new IHDR(chunk);
                    chunks.Enqueue(ihdr);
                    chunksToWrite.Enqueue(ihdr);
                }
                else if (chunk.sign == "PLTE")
                {
                    PLTE plte = new PLTE(chunk);
                    chunks.Enqueue(plte);
                    chunksToWrite.Enqueue(plte);
                }
                else if (chunk.sign == "IDAT")
                {
                    IDAT idat = new IDAT(chunk);
                    chunks.Enqueue(idat);
                    chunksToWrite.Enqueue(idat);
                }
                else if (chunk.sign == "IEND")
                {
                    IEND iend = new IEND(chunk);
                    chunks.Enqueue(iend);
                    chunksToWrite.Enqueue(iend);
                    endOfFile = false;
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
                else
                {
                    chunks.Enqueue(chunk);
                }

            } while (endOfFile);

            Picture.Close();
        }
    }
}
