using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Classes.SHP
{
    public class SHPPoint:SHPElement
    {
        public double X;
        public double Y;

        public SHPPoint(BinaryReader b)
        {
            X = b.ReadDouble();
            Y = b.ReadDouble();

        }
    }
}
