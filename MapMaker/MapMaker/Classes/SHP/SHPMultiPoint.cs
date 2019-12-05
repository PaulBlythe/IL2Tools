using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Classes.SHP
{
    public class SHPMultiPoint:SHPElement
    {
        public Region region;
        public Int32 NumPoints;
        public SHPPoint[] Points;

        public SHPMultiPoint(BinaryReader b)
        {
            region = new Region(b);

            NumPoints = b.ReadInt32();
            Points = new SHPPoint[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new SHPPoint(b);
            }
        }

    }
}
