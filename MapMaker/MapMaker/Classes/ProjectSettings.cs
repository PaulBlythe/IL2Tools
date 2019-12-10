using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class ProjectSettings
    {
        public double StartLatitude = 0;
        public double StartLongitude = 0;
        public double PixelWidthInMetres = 200;
        public bool UseMercatorProjection = true;
        public int ImageWidth = 1024;
        public int ImageHeight = 1024;

        public ProjectSettings()
        {
        }

        public void Save(BinaryWriter b)
        {
            b.Write(StartLatitude);
            b.Write(StartLongitude);
            b.Write(PixelWidthInMetres);
            b.Write(UseMercatorProjection);
            b.Write(ImageWidth);
            b.Write(ImageHeight);
        }

        public void Load(BinaryReader b)
        {
            StartLatitude = b.ReadDouble();
            StartLongitude = b.ReadDouble();
            PixelWidthInMetres = b.ReadDouble();
            UseMercatorProjection = b.ReadBoolean();
            ImageWidth = b.ReadInt32();
            ImageHeight = b.ReadInt32();
        }
    }
}
