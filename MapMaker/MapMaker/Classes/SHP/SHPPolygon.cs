using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Classes.SHP
{
    public class SHPPolygon:SHPElement
    {
        public Region region;
        public Int32 NumParts;
        public Int32 NumPoints;
        public Int32[] Parts;
        public SHPPoint[] Points;

        public SHPPolygon(BinaryReader b)
        {
            region = new Region(b);

            NumParts = b.ReadInt32();
            NumPoints = b.ReadInt32();

            Parts = new Int32[NumParts];
            Points = new SHPPoint[NumPoints];

            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = b.ReadInt32();
            }

            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new SHPPoint(b);
            }
        }
    }
}
