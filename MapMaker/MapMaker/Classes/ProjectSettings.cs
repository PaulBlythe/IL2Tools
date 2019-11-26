using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class ProjectSettings
    {
        public double StartLatitude;
        public double StartLongitude;
        public double PixelWidthInMetres;
        public bool UseMercatorProjection;
        public int ImageWidth;
        public int ImageHeight;

        public ProjectSettings()
        {
            ImageHeight = ImageWidth = 0;
            StartLongitude = StartLatitude = PixelWidthInMetres = 0;
            UseMercatorProjection = true;
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
