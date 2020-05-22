using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using AForge.Imaging;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Linq;
using System.Numerics;

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
            List<Chunk> chunksToEncrypt = new List<Chunk>();

            string fileName = ChoosePicture();
            string newFileName = "data\\test.png";

            Read(signs, chunks, chunksToWrite, chunksToEncrypt, fileName);

            while(!end)
            {
                end = Execute(signs, chunks, chunksToWrite, chunksToEncrypt, fileName, newFileName);
            }
        }
        ////////////////////////////////////////////////////////////////////////////////
        /// Cryptography                                                             ///
        ////////////////////////////////////////////////////////////////////////////////
        
        public static void EncryptDecrypt(List<Chunk> chunksToEncrypt, PNG_signs signs)
        {
            int blockSize = 64;
            BigInteger n, e, d;

            string fileDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            string filePath = Path.Combine(fileDir, "data\\encrypted.png");
            string filePath2 = Path.Combine(fileDir, "data\\decrypted.png");
            string ECBFilePath = Path.Combine(fileDir, "data\\encryptedECB.png");
            string ECBFilePath2 = Path.Combine(fileDir, "data\\decryptedECB.png");
            string CBCFilePath = Path.Combine(fileDir, "data\\encryptedCBC.png");
            string CBCFilePath2 = Path.Combine(fileDir, "data\\decryptedCBC.png");

            byte[] encryptedData;
            byte[] decryptedData;
            byte[] encryptedECBData;
            byte[] decryptedECBData;
            byte[] encryptedCBCData;
            byte[] decryptedCBCData;

            BinaryWriter EncryptedPicture = new BinaryWriter(File.OpenWrite(filePath));
            BinaryWriter DecryptedPicture = new BinaryWriter(File.OpenWrite(filePath2));
            BinaryWriter ECBEncryptedPicture = new BinaryWriter(File.OpenWrite(ECBFilePath));
            BinaryWriter ECBDecryptedPicture = new BinaryWriter(File.OpenWrite(ECBFilePath2));
            BinaryWriter CBCEncryptedPicture = new BinaryWriter(File.OpenWrite(CBCFilePath));
            BinaryWriter CBCDecryptedPicture = new BinaryWriter(File.OpenWrite(CBCFilePath2));

            EncryptedPicture.Write(signs.bytePNG_sign);
            DecryptedPicture.Write(signs.bytePNG_sign);
            ECBEncryptedPicture.Write(signs.bytePNG_sign);
            ECBDecryptedPicture.Write(signs.bytePNG_sign);
            CBCEncryptedPicture.Write(signs.bytePNG_sign);
            CBCDecryptedPicture.Write(signs.bytePNG_sign);

            foreach (Chunk c in chunksToEncrypt)
            {
                if (c.sign == "IDAT")
                {
                    List<Byte[]> divided = new List<Byte[]>();
                    List<Byte[]> encrypted = new List<Byte[]>();
                    List<Byte[]> decrypted = new List<Byte[]>();
                    List<Byte[]> encryptedECB = new List<Byte[]>();
                    List<Byte[]> decryptedECB = new List<Byte[]>();
                    List<Byte[]> encryptedCBC = new List<Byte[]>();
                    List<Byte[]> decryptedCBC = new List<Byte[]>();
                    byte[] data = c.ReturnData();
                    
                    for(int i=0; i<data.Length; i+=blockSize)
                    {
                        List<byte> bytes = new List<byte>();
                        for(int j=i; j<i+blockSize&& j<data.Length; j++)
                        {
                            bytes.Add(data[j]);
                        }
                        divided.Add(bytes.ToArray());
                    }

                    using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                    {
                        n = new BigInteger(RSA.ExportParameters(false).Modulus.Reverse().ToArray(), true);
                        e = new BigInteger(RSA.ExportParameters(false).Exponent.Reverse().ToArray(), true);
                        d = new BigInteger(RSA.ExportParameters(true).D.Reverse().ToArray(), true);
                        foreach (byte[] b in divided)
                        {
                            byte[] encryptedBlock = RSAEncrypt(b, RSA.ExportParameters(false), false);
                            encrypted.Add(encryptedBlock);
                            decrypted.Add(RSADecrypt(encryptedBlock ,RSA.ExportParameters(true), false));
                        }
                    }
                    encryptedData = encrypted.SelectMany(a => a).ToArray();
                    decryptedData = decrypted.SelectMany(a => a).ToArray();

                    byte[] compressedData = Compress(encryptedData, ((IDAT)c).compression.FLEVEL);

                    EncryptedPicture.Write(BitConverter.GetBytes(compressedData.Length));
                    EncryptedPicture.Write(c.byteSign);
                    EncryptedPicture.Write(compressedData);
                    EncryptedPicture.Write(c.byteCheckSum);

                    byte[] compressedDataDe = Compress(decryptedData, ((IDAT)c).compression.FLEVEL);

                    DecryptedPicture.Write(BitConverter.GetBytes(compressedDataDe.Length));
                    DecryptedPicture.Write(c.byteSign);
                    DecryptedPicture.Write(compressedDataDe);
                    DecryptedPicture.Write(c.byteCheckSum);

                    //ECB

                    foreach(byte[] b  in divided)
                    {
                        
                        bool test = false;
                        BigInteger intData = new BigInteger(b);
                        if(intData.Sign==-1)
                        {
                            test = true;
                        }
                        intData = new BigInteger(b, true);
                        BigInteger intEncrypted = BigInteger.ModPow(intData,e,n);
                        BigInteger intDecrypted = BigInteger.ModPow(intEncrypted, d, n);
                        encryptedECB.Add(intEncrypted.ToByteArray());
                        byte[] temp = new byte[blockSize];
                        byte[] temp2;
                        if (test)
                        {
                            byte[] temp3 = intDecrypted.ToByteArray();
                            temp2 = new byte[temp3.Length - 1];
                            for(int i=0; i<temp3.Length-1; i++)
                            {
                                temp2[i] = temp3[i];
                            }
                        }
                        else
                        {
                            temp2 = intDecrypted.ToByteArray();
                        }

                        int l = b.Length - temp2.Length;

                        if (l != 0)
                        {
                            foreach (byte by in temp2)
                            {
                                temp[l-1] = by;
                                l++;
                            }
                            decryptedECB.Add(temp);
                        }
                        else
                        {
                            decryptedECB.Add(temp2);
                        }
                    }
                    encryptedECBData = encryptedECB.SelectMany(a => a).ToArray();
                    decryptedECBData = decryptedECB.SelectMany(a => a).ToArray();

                    byte[] compressedDataECB = Compress(encryptedECBData, ((IDAT)c).compression.FLEVEL);

                    ECBEncryptedPicture.Write(BitConverter.GetBytes(compressedDataECB.Length));
                    ECBEncryptedPicture.Write(c.byteSign);
                    ECBEncryptedPicture.Write(compressedDataECB);
                    ECBEncryptedPicture.Write(c.byteCheckSum);

                    byte[] compressedDataECBde = Compress(decryptedECBData, ((IDAT)c).compression.FLEVEL);

                    ECBDecryptedPicture.Write(BitConverter.GetBytes(compressedDataECBde.Length));
                    ECBDecryptedPicture.Write(c.byteSign);
                    ECBDecryptedPicture.Write(compressedDataECBde);
                    ECBDecryptedPicture.Write(c.byteCheckSum);

                    //CBC

                    Random rnd = new Random();
                    byte[] initializationVector = new byte[blockSize];
                    rnd.NextBytes(initializationVector);
                    byte[] vector = new byte[blockSize];

                    for (int j=0; j<divided.Count(); j++)
                    {
                        byte[] b = divided[j];
                        byte[] xored = new byte[b.Length];
                        if(j==0)
                        {
                            vector = initializationVector;  
                        }

                        for (int k = 0; k < b.Length; k++)
                        {
                            xored[k] = (byte)(b[k] ^ vector[k]);
                        }

                        bool test = false;
                        BigInteger intData = new BigInteger(xored);
                        if (intData.Sign == -1)
                        {
                            test = true;
                        }
                        intData = new BigInteger(xored, true);
                        BigInteger intEncrypted = BigInteger.ModPow(intData, e, n);
                        BigInteger intDecrypted = BigInteger.ModPow(intEncrypted, d, n);
                        
                        byte[] temp = new byte[blockSize];
                        byte[] temp2;
                        if (test)
                        {
                            byte[] temp3 = intDecrypted.ToByteArray();
                            temp2 = new byte[temp3.Length - 1];
                            for (int i = 0; i < temp3.Length - 1; i++)
                            {
                                temp2[i] = temp3[i];
                            }
                        }
                        else
                        {
                            temp2 = intDecrypted.ToByteArray();
                        }

                        int l = b.Length - temp2.Length;

                        if (l != 0)
                        {
                            foreach (byte by in temp2)
                            {
                                temp[l - 1] = by;
                                l++;
                            }
                            for (int k = 0; k < b.Length; k++)
                            {
                                temp[k] = (byte)(temp[k] ^ vector[k]);
                            }
                            decryptedCBC.Add(temp);
                        }
                        else
                        {
                            for (int k = 0; k < b.Length; k++)
                            {
                                temp2[k] = (byte)(temp2[k] ^ vector[k]);
                            }
                            decryptedCBC.Add(temp2);
                        }
                        vector = intEncrypted.ToByteArray();
                        encryptedCBC.Add(vector);
                    }
                    encryptedCBCData = encryptedCBC.SelectMany(a => a).ToArray();
                    decryptedCBCData = decryptedCBC.SelectMany(a => a).ToArray();

                    byte[] compressedDataCBC = Compress(encryptedCBCData, ((IDAT)c).compression.FLEVEL);

                    CBCEncryptedPicture.Write(BitConverter.GetBytes(compressedDataCBC.Length));
                    CBCEncryptedPicture.Write(c.byteSign);
                    CBCEncryptedPicture.Write(compressedDataCBC);
                    CBCEncryptedPicture.Write(c.byteCheckSum);

                    byte[] compressedDataCBCde = Compress(decryptedCBCData, ((IDAT)c).compression.FLEVEL);

                    CBCDecryptedPicture.Write(BitConverter.GetBytes(compressedDataCBCde.Length));
                    CBCDecryptedPicture.Write(c.byteSign);
                    CBCDecryptedPicture.Write(compressedDataCBCde);
                    CBCDecryptedPicture.Write(c.byteCheckSum);
                }
                else
                {
                    c.Write(EncryptedPicture);
                    c.Write(DecryptedPicture);
                    c.Write(ECBEncryptedPicture);
                    c.Write(ECBDecryptedPicture);
                    c.Write(CBCEncryptedPicture);
                    c.Write(CBCDecryptedPicture);
                }
            }
            EncryptedPicture.Close();
            DecryptedPicture.Close();
            ECBEncryptedPicture.Close();
            ECBDecryptedPicture.Close();
            CBCEncryptedPicture.Close();
            CBCDecryptedPicture.Close();
        }

        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }
        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /// Compression                                                                       ///
        /////////////////////////////////////////////////////////////////////////////////////////
        
        public static byte[] Compress(byte[] data, int compressionLevel)
        {
            Deflater defl = new Deflater();
            defl.SetInput(data);
            defl.SetLevel(compressionLevel);
            byte[] compressedData = new byte[100000];
            defl.Deflate(compressedData);

            return compressedData;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /// Menu and choice                                                                   ///
        /////////////////////////////////////////////////////////////////////////////////////////

        public static void ProgramMenu()
        {
            Console.WriteLine("\n---   MENU   ---\n");
            Console.WriteLine("1. Display chunks");
            Console.WriteLine("2. Write anonimized picture");
            Console.WriteLine("3. Display picture");
            Console.WriteLine("4. Make fft");
            Console.WriteLine("5. Encrypt/Decrypt RSA/ECB/CBC");
            Console.WriteLine("6. End");
            Console.WriteLine("");
        }

        public static bool Execute(PNG_signs signs, Queue<Chunk> chunks, Queue<Chunk> chunksToWrite, List<Chunk> chunksToEncrypt, string fileName, string newFileName)
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
            } while (choice < 1 || choice > 6);

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
                    EncryptDecrypt(chunksToEncrypt, signs);
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// FFT                                                                                          ///
        ////////////////////////////////////////////////////////////////////////////////////////////////////

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

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// Picture services                                                                        ///
        ///////////////////////////////////////////////////////////////////////////////////////////////

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

        public static void Read(PNG_signs signs, Queue<Chunk> chunks, Queue<Chunk> chunksToWrite, List<Chunk> chunksToEncrypt, string fileName)
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
                    chunksToEncrypt.Add(ihdr);
                }
                else if (chunk.sign == "PLTE")
                {
                    PLTE plte = new PLTE(chunk);
                    chunks.Enqueue(plte);
                    chunksToWrite.Enqueue(plte);
                    chunksToEncrypt.Add(plte);
                }
                else if (chunk.sign == "IDAT")
                {
                    idatQuantity++;
                    IDAT idat = new IDAT(chunk,idatQuantity);
                    chunks.Enqueue(idat);
                    chunksToWrite.Enqueue(idat);
                    chunksToEncrypt.Add(idat);
                }
                else if (chunk.sign == "IEND")
                {
                    IEND iend = new IEND(chunk);
                    chunks.Enqueue(iend);
                    chunksToWrite.Enqueue(iend);
                    chunksToEncrypt.Add(iend);
                    endOfFile = false;
                }
                else if (chunk.sign == "gAMA")
                {
                    gAMA gama = new gAMA(chunk);
                    chunks.Enqueue(gama);
                    chunksToEncrypt.Add(gama);
                }
                else if (chunk.sign == "cHRM")
                {
                    cHRM chrm = new cHRM(chunk);
                    chunks.Enqueue(chrm);
                    chunksToEncrypt.Add(chrm);
                }
                else if (chunk.sign == "zTXt")
                {
                    zTXt ztxt = new zTXt(chunk);
                    chunks.Enqueue(ztxt);
                    chunksToEncrypt.Add(ztxt);
                }
                else if (chunk.sign == "tEXt")
                {
                    tEXt text = new tEXt(chunk);
                    chunks.Enqueue(text);
                    chunksToEncrypt.Add(text);
                }
                else
                {
                    chunks.Enqueue(chunk);
                    chunksToEncrypt.Add(chunk);
                }

            } while (endOfFile);

            Picture.Close();
        }
    }
}
