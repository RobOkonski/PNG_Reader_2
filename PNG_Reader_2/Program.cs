using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using Extreme.Mathematics.SignalProcessing;
//using ComponentAce.Compression.Libs.zlib;
//using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Diagnostics;
using System.Drawing.Imaging;
using AForge.Imaging;
using ImageMagick;

namespace PNG_Reader_2
{
    class Program
    {
        public static void Main(string[] args)
        {
            PNG_signs signs = new PNG_signs();
            Queue<Chunk> chunks = new Queue<Chunk>();
            Queue<Chunk> chunksToWrite = new Queue<Chunk>();

            string fileName = Start();// "data\\camaro.png";
            string newFileName = "data\\test.png";

            Read(signs, chunks, chunksToWrite, fileName);
            Display(chunks);
            Write(signs, chunksToWrite, newFileName);

            DisplayImage(fileName);
            MakeFFT(fileName);
        }

        public static void Menu()
        {
            Console.WriteLine("\n---   MENU   ---\n");
            Console.WriteLine("1. camaro.png");
            Console.WriteLine("2. 4colors.png");
            Console.WriteLine("3. 16colors.png");
            Console.WriteLine("4. 256colors.png");
            Console.WriteLine("5. adaptive.png");
            Console.WriteLine("6. exif.png");
            Console.WriteLine("7. ogien.png");
            Console.WriteLine("8. ex1.png");
            Console.WriteLine("");
        }

        public static string Start()
        {
            string fileName;
            string schoice;
            int choice=0;
            do
            {
                Menu();
                Console.Write("Choose picture: ");
                schoice = (Console.ReadLine());
                Console.WriteLine("");
                Int32.TryParse(schoice,out choice);
            } while (choice < 1 || choice > 8);

            switch(choice)
            {
                case 1:
                    Console.WriteLine("camaro.png\n");
                    fileName = "data\\camaro.png";
                    break;
                case 2:
                    Console.WriteLine("4colors.png\n");
                    fileName = "data\\4colors.png";
                    break;
                case 3:
                    Console.WriteLine("16colors.png\n");
                    fileName = "data\\16colors.png";
                    break;
                case 4:
                    Console.WriteLine("256colors.png\n");
                    fileName = "data\\256colors.png";
                    break;
                case 5:
                    Console.WriteLine("adaptive.png\n");
                    fileName = "data\\adaptive.png";
                    break;
                case 6:
                    Console.WriteLine("exif.png\n");
                    fileName = "data\\exif.png";
                    break;
                case 7:
                    Console.WriteLine("ogien.png\n");
                    fileName = "data\\ogien.png";
                    break;
                case 8:
                    Console.WriteLine("ex1.png\n");
                    fileName = "data\\ex1.png";
                    break;
                default:
                    Console.WriteLine("Undefined picture\n");
                    fileName = "data\\camaro.png";
                    break;
            }

            return fileName;
        }

        public static int PowerOf2(int x)
        {
            int y=2;
            int temp=1;
            do
            {
                temp *= 2;
                if (temp < x) y = temp;
            } while (temp < x);
            return y;
        }

        public static void MakeFFT(string fileName)
        {
            string fileDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            string filePath = Path.Combine(fileDir, fileName);

            Bitmap org = new Bitmap(filePath);

            int width = PowerOf2(org.Width);
            int height = PowerOf2(org.Height);

            Bitmap bmp = new Bitmap(org,width,height);
            Color p;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = bmp.GetPixel(x, y);
                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;
                    int avg = (r + g + b) / 3;
                    bmp.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                }
            }
            Bitmap gray = ConvertPixelformat(bmp);

            ComplexImage complexImage = ComplexImage.FromBitmap(gray);
            complexImage.ForwardFourierTransform();
            Bitmap fourierImage = complexImage.ToBitmap();

            string fftFileName = "data\\fft.png";
            string fftFilePath = Path.Combine(fileDir, fftFileName);
            fourierImage.Save(fftFilePath);

            DisplayImage(fftFilePath);

        }

        private static Bitmap ConvertPixelformat(Bitmap Bmp)
        {
            Bitmap myBitmap = new Bitmap(Bmp);
            Rectangle cloneRect = new Rectangle(0, 0, Bmp.Width, Bmp.Height);
            PixelFormat format = PixelFormat.Format8bppIndexed;
            Bitmap cloneBitmap = myBitmap.Clone(cloneRect, format);
            var pal = cloneBitmap.Palette;

            for (int i = 0; i < cloneBitmap.Palette.Entries.Length; ++i)
            {
                var entry = cloneBitmap.Palette.Entries[i];
                var gray = (int)(0.30 * entry.R + 0.59 * entry.G + 0.11 * entry.B);
                pal.Entries[i] = Color.FromArgb(gray, gray, gray);
            }
            cloneBitmap.Palette = pal;
            cloneBitmap.SetResolution(500.0F, 500.0F);
            return cloneBitmap;
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
            string fileDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            string filePath = Path.Combine(fileDir, fileName);
            int idatQuantity = 0;

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
                    idatQuantity++;
                    IDAT idat = new IDAT(chunk,idatQuantity);
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
                else if (chunk.sign == "zTXt")
                {
                    zTXt ztxt = new zTXt(chunk);
                    chunks.Enqueue(ztxt);
                }
                else if (chunk.sign == "tEXt")
                {
                    tEXt text = new tEXt(chunk);
                    chunks.Enqueue(text);
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
