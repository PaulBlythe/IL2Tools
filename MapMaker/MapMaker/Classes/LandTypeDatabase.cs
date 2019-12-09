using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class LandTypeDatabase
    {
        byte[] data;
        int width = 3600;
        int height = 1800;

        public LandTypeDatabase()
        {
            using (BinaryReader b = new BinaryReader(File.Open("data.bin", FileMode.Open)))
            {
                data = b.ReadBytes(width * height);
                System.Diagnostics.Debug.Assert(data.Length == width * height);
            }
        }

        public byte GetType(double Latitude, double Longitude)
        {
            int y;
            int x;

            y = (int)(((90 - Latitude) * height) / 180.0);
            x = (int)(((180 + Longitude) * width) / 360.0);

            x = Math.Min(x, width-1);
            y = Math.Min(y, height-1);

            return data[x + (y * width)];
        }

    }
}
