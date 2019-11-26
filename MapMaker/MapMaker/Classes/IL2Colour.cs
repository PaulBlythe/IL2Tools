using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class IL2Colour
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public Int32 Colour
        {
            get
            {
                //return (Int32)(0xff + (Red << 8) + (Green << 16) + (Blue << 24));
                //return (Int32)(0xff000000 + ((Red << 16) & 0xff0000) + ((Green << 8) & 0xff00) + Blue);
                return (Int32)((Blue << 24) + (255 << 16) + (Red << 8) + Green);
            }

        }

        public IL2Colour(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
    }
}
