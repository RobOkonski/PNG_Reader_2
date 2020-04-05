using System;
using System.IO;

namespace PNG_Reader_2
{
    public class PNG_signs
    {
        public string PNG_sign = "89-50-4E-47-0D-0A-1A-0A";
        public string IHDR_sign = "49-48-44-52";
        public string PLTE_sign = "50-4C-54-45";
        public string IDAT_sign = "49-44-41-54";
        public string IEND_sign = "49-45-4E-44";
        public string bKGD_sign = "62-4B-47-44";
        public string cHRM_sign = "63-48-52-4D";
        public string dSIG_sign = "64-53-49-47";
        public string eXIf_sign = "65-58-49-66";
        public string gAMA_sign = "67-41-4D-41";
        public string hIST_sign = "68-49-53-54";
        public string iCCP_sign = "69-43-43-50";
        public string iTXt_sign = "69-54-58-74";
        public string pHYs_sign = "70-48-59-73";
        public string sBIT_sign = "73-42-49-54";
        public string sPLT_sign = "73-50-4C-54";
        public string sRGB_sign = "73-52-47-42";
        public string sTER_sign = "73-54-45-52";
        public string tEXt_sign = "74-45-58-74";
        public string tIME_sign = "74-49-4D-45";
        public string tRNS_sign = "74-52-4E-53";
        public string zTXt_sign = "7A-54-58-74";

        public byte[] bytePNG_sign = new byte[8];

        public void ReadSign(BinaryReader Picture)
        {
            bytePNG_sign = Picture.ReadBytes(8);
        }

        public bool IsPNG(BinaryReader Picture)
        {
            ReadSign(Picture);

            if (BitConverter.ToString(bytePNG_sign) == PNG_sign) return true;
            return false;
        }

    }
}
