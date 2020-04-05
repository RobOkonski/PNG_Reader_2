using System;
using System.Collections;
using System.Text;

namespace PNG_Reader_2
{
    public class Compression
    {
        public int CINF;
        public int CM;
        public int FLEVEL;
        public bool FDICT;

        public Compression(string CMF, byte FLG)
        {
            CINF = Int32.Parse(CMF.Substring(0, 1));
            CM = Int32.Parse(CMF.Substring(1, 1));

            BitArray bFLG = new BitArray(FLG);

            if (bFLG[0] == false && bFLG[1] == false) FLEVEL = 0;
            else if (bFLG[0] == false && bFLG[1] == true) FLEVEL = 1;
            else if (bFLG[0] == true && bFLG[1] == false) FLEVEL = 2;
            else if (bFLG[0] == true && bFLG[1] == true) FLEVEL = 3;

            FDICT = bFLG[2];

        }

        public void Display()
        {
            Console.WriteLine(" - CompressionData\n");

            if(CINF==7) Console.WriteLine("   CINF: {0} - standard 32Kb compression window",CINF);
            else Console.WriteLine("   CINF: {0}", CINF);

            if (CM == 8) Console.WriteLine("   CM: {0} - deflate compression",CM);
            else Console.WriteLine("   CM: {0}",CM);

            if (FLEVEL == 0) Console.WriteLine("   FLEVEL: {0} - compressor used fastest algorithm", FLEVEL);
            else if (FLEVEL == 1) Console.WriteLine("   FLEVEL: {0} - compressor used fast algorithm", FLEVEL);
            else if (FLEVEL == 2) Console.WriteLine("   FLEVEL: {0} - compressor used default algorithm", FLEVEL);
            else if (FLEVEL == 3) Console.WriteLine("   FLEVEL: {0} - compressor used maximum compression, slowest algorithm", FLEVEL);
            else Console.WriteLine("   FLEVEL: error");

            if (FDICT) Console.WriteLine("   FDICT: 1 - preset dictionary");
            else Console.WriteLine("   FDICT: 0 - no preset dictionary");

            Console.WriteLine("");
        }
    }
}
