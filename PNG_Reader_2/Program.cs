using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using AForge.Imaging;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace PNG_Reader_2
{
    class Program
    {
        public static void Main(string[] args)
        {
            bool end = false;
            PNG_signs signs = new PNG_signs();
            Queue<Chunk> chunks = new Queue<Chunk>();
            Queue<Chunk> chunksToWrite = new Queue<Chunk>();
            Queue<Chunk> chunksToEncrypt = new Queue<Chunk>();

            string fileName = ChoosePicture();
            string newFileName = "data\\test.png";

            Read(signs, chunks, chunksToWrite, chunksToEncrypt, fileName);

            while(!end)
            {
                end = Execute(signs, chunks, chunksToWrite, chunksToEncrypt, fileName, newFileName);
            }
        }

        public static void MakeRSA(Queue<Chunk> chunksToEncrypt, PNG_signs signs)
        {
            string fileDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            string filePath = Path.Combine(fileDir, "data\\encrypted.png");
            string filePath2 = Path.Combine(fileDir, "data\\decrypted.png");

            byte[] encryptedData;
            //byte[] decryptedData;

            BinaryWriter EncryptedPicture = new BinaryWriter(File.OpenWrite(filePath));
            //BinaryWriter DecryptedPicture = new BinaryWriter(File.OpenWrite(filePath2));

            EncryptedPicture.Write(signs.bytePNG_sign);
            //DecryptedPicture.Write(signs.bytePNG_sign);

            while (chunksToEncrypt.Count != 0)
            {
                var c = chunksToEncrypt.Dequeue();
                if(c.sign == "IDAT" )
                {
                    using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                    {
                        Console.WriteLine(RSA.KeySize);
                        encryptedData = Encrypt(c.ReturnData(), RSA.ExportParameters(false));
                        //decryptedData = Decrypt(encryptedData, RSA.ExportParameters(true));
                    }
                    Deflater defl = new Deflater();
                    defl.SetInput(encryptedData);
                    defl.SetLevel(((IDAT)c).compression.FLEVEL);
                    byte[] compressedData = new byte[100000];
                    defl.Deflate(compressedData);

                    EncryptedPicture.Write(BitConverter.GetBytes(compressedData.Length));
                    EncryptedPicture.Write(c.byteSign);
                    EncryptedPicture.Write(compressedData);
                    EncryptedPicture.Write(c.byteCheckSum);

                    //DecryptedPicture.Write(BitConverter.GetBytes(decryptedData.Length));
                    //DecryptedPicture.Write(c.byteSign);
                    //DecryptedPicture.Write(decryptedData);
                    //DecryptedPicture.Write(c.byteCheckSum);
                }
                else
                {
                    c.Write(EncryptedPicture);
                    //c.Write(DecryptedPicture);
                }
            }
            EncryptedPicture.Close();
            //DecryptedPicture.Close();
        }

        public static byte[] Encrypt(byte[] encryptThis, RSAParameters publicKeyInfo)
        {
            //// Our bytearray to hold all of our data after the encryption
            byte[] encryptedBytes = new byte[0];
            using (var RSA = new RSACryptoServiceProvider())
            {
                try
                { 
                    RSA.ImportParameters(publicKeyInfo);

                    int blockSize = (RSA.KeySize / 8) - 32;

                    //// buffer to write byte sequence of the given block_size
                    byte[] buffer = new byte[blockSize];

                    byte[] encryptedBuffer = new byte[blockSize];

                    //// Initializing our encryptedBytes array to a suitable size, depending on the size of data to be encrypted
                    encryptedBytes = new byte[encryptThis.Length + blockSize - (encryptThis.Length % blockSize) + 32];

                    for (int i = 0; i < encryptThis.Length; i += blockSize)
                    {
                        //// If there is extra info to be parsed, but not enough to fill out a complete bytearray, fit array for last bit of data
                        if (2 * i > encryptThis.Length && ((encryptThis.Length - i) % blockSize != 0))
                        {
                            buffer = new byte[encryptThis.Length - i];
                            blockSize = encryptThis.Length - i;
                        }

                        //// If the amount of bytes we need to decrypt isn't enough to fill out a block, only decrypt part of it
                        if (encryptThis.Length < blockSize)
                        {
                            buffer = new byte[encryptThis.Length];
                            blockSize = encryptThis.Length;
                        }

                        //// encrypt the specified size of data, then add to final array.
                        Buffer.BlockCopy(encryptThis, i, buffer, 0, blockSize);
                        encryptedBuffer = RSA.Encrypt(buffer, false);
                        encryptedBuffer.CopyTo(encryptedBytes, i);
                    }
                }
                catch (CryptographicException e)
                {
                    Console.Write(e);
                }
                finally
                {
                    //// Clear the RSA key container, deleting generated keys.
                    RSA.PersistKeyInCsp = false;
                }
            }
            //// Convert the byteArray using Base64 and returns as an encrypted string
            return encryptedBytes;
        }

        /// <summary>
        /// Decrypt this message using this key
        /// </summary>
        /// <param name="dataToDecrypt">
        /// The data To decrypt.
        /// </param>
        /// <param name="privateKeyInfo">
        /// The private Key Info.
        /// </param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        /*public static byte[] Decrypt(byte[] bytesToDecrypt, RSAParameters privateKeyInfo)
        {
            //// The bytearray to hold all of our data after decryption
            byte[] decryptedBytes;

            //Create a new instance of RSACryptoServiceProvider.
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                try
                {
                    //// Import the private key info
                    RSA.ImportParameters(privateKeyInfo);

                    //// No need to subtract padding size when decrypting (OR do I?)
                    int blockSize = RSA.KeySize / 8;

                    //// buffer to write byte sequence of the given block_size
                    byte[] buffer = new byte[blockSize];

                    //// buffer containing decrypted information
                    byte[] decryptedBuffer = new byte[blockSize];

                    //// Initializes our array to make sure it can hold at least the amount needed to decrypt.
                    decryptedBytes = new byte[bytesToDecrypt.Length];

                    for (int i = 0; i < bytesToDecrypt.Length; i += blockSize)
                    {
                        if (2 * i > bytesToDecrypt.Length && ((bytesToDecrypt.Length - i) % blockSize != 0))
                        {
                            buffer = new byte[bytesToDecrypt.Length - i];
                            blockSize = bytesToDecrypt.Length - i;
                        }

                        //// If the amount of bytes we need to decrypt isn't enough to fill out a block, only decrypt part of it
                        if (bytesToDecrypt.Length < blockSize)
                        {
                            buffer = new byte[bytesToDecrypt.Length];
                            blockSize = bytesToDecrypt.Length;
                        }

                        Buffer.BlockCopy(bytesToDecrypt, i, buffer, 0, blockSize);
                        decryptedBuffer = RSA.Decrypt(buffer, false);
                        decryptedBuffer.CopyTo(decryptedBytes, i);
                    }
                }
                finally
                {
                    //// Clear the RSA key container, deleting generated keys.
                    RSA.PersistKeyInCsp = false;
                }
            }

            //// We encode each byte with UTF8 and then write to a string while trimming off the extra empty data created by the overhead.
            return decryptedBytes;

        }*/

        public static void ProgramMenu()
        {
            Console.WriteLine("\n---   MENU   ---\n");
            Console.WriteLine("1. Display chunks");
            Console.WriteLine("2. Write anonimized picture");
            Console.WriteLine("3. Display picture");
            Console.WriteLine("4. Make fft");
            Console.WriteLine("5. Make RSA");
            Console.WriteLine("6. End");
            Console.WriteLine("");
        }

        public static bool Execute(PNG_signs signs, Queue<Chunk> chunks, Queue<Chunk> chunksToWrite, Queue<Chunk> chunksToEncrypt, string fileName, string newFileName)
        {
            string schoice;
            int choice = 0;
            do
            {
                ProgramMenu();
                Console.Write("Choose what to do: ");
                schoice = (Console.ReadLine());
                Console.WriteLine("");
                Int32.TryParse(schoice, out choice);
            } while (choice < 1 || choice > 5);

            switch (choice)
            {
                case 1:
                    Display(chunks);
                    break;
                case 2:
                    Write(signs, chunksToWrite, newFileName);
                    break;
                case 3:
                    DisplayImage(fileName);
                    break;
                case 4:
                    MakeFFT(fileName);
                    break;
                case 5:
                    MakeRSA(chunksToEncrypt, signs);
                    break;
                case 6:
                    return true;
                default:
                    Console.WriteLine("Undefined operation\n");
                    break;
            }
            return false;
        }

        public static void PictureMenu()
        {
            Console.WriteLine("\n---   Choose picture   ---\n");
            Console.WriteLine("1. camaro.png");
            Console.WriteLine("2. 4colors.png");
            Console.WriteLine("3. rim.png");
            Console.WriteLine("4. 256colors.png");
            Console.WriteLine("5. adaptive.png");
            Console.WriteLine("6. exif.png");
            Console.WriteLine("7. ogien.png");
            Console.WriteLine("8. ex1.png");
            Console.WriteLine("");
        }

        public static string ChoosePicture()
        {
            string fileName;
            string schoice;
            int choice=0;
            do
            {
                PictureMenu();
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
                    Console.WriteLine("rim.png\n");
                    fileName = "data\\rim.png";
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

        public static void Read(PNG_signs signs, Queue<Chunk> chunks, Queue<Chunk> chunksToWrite, Queue<Chunk> chunksToEncrypt, string fileName)
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
                    chunksToEncrypt.Enqueue(ihdr);
                }
                else if (chunk.sign == "PLTE")
                {
                    PLTE plte = new PLTE(chunk);
                    chunks.Enqueue(plte);
                    chunksToWrite.Enqueue(plte);
                    chunksToEncrypt.Enqueue(plte);
                }
                else if (chunk.sign == "IDAT")
                {
                    idatQuantity++;
                    IDAT idat = new IDAT(chunk,idatQuantity);
                    chunks.Enqueue(idat);
                    chunksToWrite.Enqueue(idat);
                    chunksToEncrypt.Enqueue(idat);
                }
                else if (chunk.sign == "IEND")
                {
                    IEND iend = new IEND(chunk);
                    chunks.Enqueue(iend);
                    chunksToWrite.Enqueue(iend);
                    chunksToEncrypt.Enqueue(iend);
                    endOfFile = false;
                }
                else if (chunk.sign == "gAMA")
                {
                    gAMA gama = new gAMA(chunk);
                    chunks.Enqueue(gama);
                    chunksToEncrypt.Enqueue(gama);
                }
                else if (chunk.sign == "cHRM")
                {
                    cHRM chrm = new cHRM(chunk);
                    chunks.Enqueue(chrm);
                    chunksToEncrypt.Enqueue(chrm);
                }
                else if (chunk.sign == "zTXt")
                {
                    zTXt ztxt = new zTXt(chunk);
                    chunks.Enqueue(ztxt);
                    chunksToEncrypt.Enqueue(ztxt);
                }
                else if (chunk.sign == "tEXt")
                {
                    tEXt text = new tEXt(chunk);
                    chunks.Enqueue(text);
                    chunksToEncrypt.Enqueue(text);
                }
                else
                {
                    chunks.Enqueue(chunk);
                    chunksToEncrypt.Enqueue(chunk);
                }

            } while (endOfFile);

            Picture.Close();
        }
    }
}
