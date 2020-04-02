using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace PNG_Reader_2
{
    public class Chunk
    {
        public byte[] byteLength;
        public byte[] byteSign;
        public byte[] byteData;
        public byte[] byteCheckSum;

        public string sign;
        public int length;

        ASCIIEncoding ascii = new ASCIIEncoding();


        public void Read(BinaryReader Picture)
        {
            byteLength = Picture.ReadBytes(4);
            length = Int32.Parse(BitConverter.ToString(byteLength).Replace("-", ""), System.Globalization.NumberStyles.HexNumber);
            byteSign = Picture.ReadBytes(4);
            sign = ascii.GetString(byteSign);
            if(length>0) byteData = Picture.ReadBytes(length);
            byteCheckSum = Picture.ReadBytes(4); 
        }

        public void Write(BinaryWriter NewPicture)
        {
            NewPicture.Write(byteLength);
            NewPicture.Write(byteSign);
            if(length>0) NewPicture.Write(byteData);
            NewPicture.Write(byteCheckSum);
        }

        virtual public void Display()
        {
            Console.WriteLine("\n[{0}] byteLength: {1}\n", sign, length);
            //Console.WriteLine(BitConverter.ToString(byteData));
        }


    }
}
