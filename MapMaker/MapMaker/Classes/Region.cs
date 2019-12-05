using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class Region
    {
        public double MinX;
        public double MinY;
        public double MaxX;
        public double MaxY;

        public Region()
        {
            MinX = MaxX = MinY = MaxY = 0;
        }

        public Region(BinaryReader b)
        {
            MinX = b.ReadDouble();
            MinY = b.ReadDouble();
            MaxX = b.ReadDouble();
            MaxY = b.ReadDouble();
        }

        public bool Intersects(Region other)
        {
            return (other.MinX < MaxX) &&
                   (MinX < other.MaxX) &&
                   (other.MinY < MaxY) &&
                   (MinY < other.MaxY);
        }
    }
}
