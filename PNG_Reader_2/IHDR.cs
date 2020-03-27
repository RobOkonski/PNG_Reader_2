using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace PNG_Reader_2
{
    class IHDR : Chunk
    {
        public int width;
        public int height;
        public int bitDepth;
        public int colorType;
        public int compresionMethod;
        public int filterMethod;
        public int interlanceMethod;


    }
}
