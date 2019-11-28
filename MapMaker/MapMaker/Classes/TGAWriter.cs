using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class TGAWriter
    {
        public static void Save(byte[] Pixels, int width, int height, String name)
        {
            using (BinaryWriter b = new BinaryWriter(File.Open(name, FileMode.Create)))
            {
                b.Write((byte)0);       // image id length
                b.Write((byte)0);       // no colour map
                b.Write((byte)3);       // uncompressed gray scale
                b.Write((byte)0);       // colour map data 
                b.Write((byte)0);
                b.Write((byte)0);
                b.Write((byte)0);
                b.Write((byte)0);
                b.Write((Int16)0);      // x origin
                b.Write((Int16)0);      // y origin
                b.Write((Int16)width);  // width
                b.Write((Int16)height); // height
                b.Write((Int16)8);     // bits per pixel
                b.Write((byte)0);
                b.Write(Pixels);
            }
        }

        public static void Save(IL2Colour[] Pixels, int width, int height, String name)
        {
            using (BinaryWriter b = new BinaryWriter(File.Open(name, FileMode.Create)))
            {
                b.Write((byte)0);       // image id length
                b.Write((byte)0);       // no colour map
                b.Write((byte)2);       // uncompressed true colour
                b.Write((byte)0);       // colour map data 
                b.Write((byte)0);
                b.Write((byte)0);
                b.Write((byte)0);
                b.Write((byte)0);
                b.Write((Int16)0);      // x origin
                b.Write((Int16)0);      // y origin
                b.Write((Int16)width);  // width
                b.Write((Int16)height); // height
                b.Write((Int16)32);     // bits per pixel
                b.Write((byte)0);
                for (int i = 0; i < width * height; i++)
                {
                    b.Write(Pixels[i].Colour);
                }
            }
        }

        public static void Save(short[] Pixels, int width, int height, String name)
        {
            using (BinaryWriter b = new BinaryWriter(File.Open(name, FileMode.Create)))
            {
                b.Write((byte)0);       // image id length
                b.Write((byte)0);       // no colour map
                b.Write((byte)3);       // uncompressed gray scale
                b.Write((byte)0);       // colour map data 
                b.Write((byte)0);
                b.Write((byte)0);
                b.Write((byte)0);
                b.Write((byte)0);
                b.Write((Int16)0);      // x origin
                b.Write((Int16)height); // y origin
                b.Write((Int16)width);  // width
                b.Write((Int16)height); // height
                b.Write((Int16)16);     // bits per pixel
                b.Write((byte)0b00100000);  
                for (int i=0; i<width * height; i++)
                {
                    b.Write(Pixels[i]);
                }
            }
        }
    }
}
