using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IL2Modder
{
    
    public class Tga
    {
        byte IDLength;
        byte ColourMapType;
        public byte ImageType;
        // 0 = no image
        // 1 = uncompressed colour mapped
        // 2 = uncompressed true colour
        // 3 = uncompressed black and white
        // 9 = run length encoded colour mapped image
        //10 = run length encoded true colour image
        //11 = run length encoded black and white image
        short ColourMapFirstEntry;
        short ColourMapLength;
        byte ColourMapEntrySize;  // 15,16,24,32 bits

        public short XOrigin;
        public short YOrigin;
        public short ImageWidth;
        public short ImageHeight;
        public byte PixelDepth;        // 8,16,24,32
        byte ImageDescriptor;   
        // bits 0-3 alpha bits
        // bit  4 left to right
        // bit  5 top to bottom

        // optional Image ID section
        byte[] ImageID;
        // optional colour map
        int[] ColourMap;
        // image data
        public byte[] ImageData;



        public Tga(String file)
        {
            using (BinaryReader b = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                #region Read the header
                IDLength = b.ReadByte();
                ColourMapType = b.ReadByte();
                ImageType = b.ReadByte();

                ColourMapFirstEntry = b.ReadInt16();
                ColourMapLength = b.ReadInt16();
                ColourMapEntrySize = b.ReadByte();

                XOrigin = b.ReadInt16();
                YOrigin = b.ReadInt16();
                ImageWidth = b.ReadInt16();
                ImageHeight = b.ReadInt16();
                PixelDepth = b.ReadByte();
                ImageDescriptor = b.ReadByte();
                #endregion

                #region Image ID
                if (IDLength != 0)
                {
                    ImageID = b.ReadBytes(IDLength);
                }
                #endregion

                #region Colour map
                if (ColourMapType != 0)
                {
                    int red = 0;
                    int green = 0;
                    int blue = 0;
                    int alpha = 255;

                    ColourMap = new int[1024];
                    int wp = ColourMapFirstEntry;
                    for (int i = 0; i < ColourMapLength; i++)
                    {
                        switch (ColourMapEntrySize)
                        {
                            case 15:
                                {
                                    int c = b.ReadInt16();
                                    blue = (c & 0x1f);
                                    green = ((c >> 5) & 0x1f);
                                    red = ((c >> 10) & 0x1f);
                                    red = red << 3;
                                    green = green << 3;
                                    blue = blue << 3;
                                    
                                }
                                break;
                            case 16:
                                {
                                    int c = b.ReadInt16();
                                    blue = (c & 0x1f);
                                    green = ((c >> 5) & 0x1f);
                                    red = ((c >> 10) & 0x1f);
                                    red = red << 3;
                                    green = green << 3;
                                    blue = blue << 3;
                                    alpha = 0;
                                    if ((c & 0x8000) != 0)
                                    {
                                        alpha = 255;
                                    }
                                }
                                break;
                            case 24:
                                {
                                    blue = b.ReadByte();
                                    green = b.ReadByte();
                                    red = b.ReadByte();
                                }
                                break;
                            case 32:
                                {

                                    blue = b.ReadByte();
                                    green = b.ReadByte();
                                    red = b.ReadByte();
                                    alpha = b.ReadByte();
                                    
                                }
                                break;
                            default:
                                {
                                    throw new Exception("Tga::Colour map of unsupoorted type " + ColourMapEntrySize);
                                }
                               
                        }
                        ColourMap[wp++] = (alpha << 24) + (red << 16) + (green << 8) + blue;
                    }
                }
                #endregion

                #region Image data
                int image_size = ImageHeight * ImageWidth * 4;
                ImageData = new byte[image_size];
                switch (ImageType)
                {
                    // uncompressed colour mapped
                    case 1:
                        {
                            UncColourMapped(b);
                        }
                        break;
                    // uncompressed true colour
                    case 2:
                        {
                            switch (PixelDepth)
                            {
                                case 16:
                                    UncSixteen(b);
                                    break;
                                case 24:
                                    UncTriple(b);
                                    break;
                                case 32:
                                    UncTrue(b);
                                    break;
                                default:
                                    {
                                        throw new Exception("Tga::Unsupported PixelDepth for uncompressed true colour " + PixelDepth);
                                    }
                            }
                        }
                        break;
                    // uncompressed black and white
                    case 3:
                        {
                            switch (PixelDepth)
                            {
                                case 8:
                                    UncBw8(b);
                                    break;
                                case 16:
                                    UncBw16(b);
                                    break;
                                case 24:
                                    UncBw24(b);
                                    break;
                                case 32:
                                    UncBw32(b);
                                    break;
                                default:
                                    {
                                        throw new Exception("Tga::Unsupported PixelDepth for uncompressed black and white " + PixelDepth);
                                    }
                            }
                        }
                        break;
                    // run length encoded colour mapped 
                    case 9:
                        {
                            RlcColourMapped(b);
                        }
                        break;
                    // run length encoded true colour
                    case 10:
                        {
                            switch (PixelDepth)
                            {
                                case 24:
                                    {
                                        RlcTrueColour24(b);
                                    }
                                    break;
                                case 32:
                                    {
                                        RlcTrueColour32(b);
                                    }
                                    break;
                                default:
                                    throw new Exception("Tga::Unsupported pixel depth for run length encoded true colour " + PixelDepth);
                            }
                        }
                        break;
                    default:
                        throw new Exception("Tga::Unsupported image type " + ImageType);

                }      
                #endregion

                b.Close();

                if ( ImageDescriptor == 8)
                {
                    int linelength = ImageWidth * 4;
                
                    for (int y = 0; y < ImageHeight / 2; y++)
                    {
                        int rp = (ImageHeight - 1) - y;
                        rp *= linelength;
                
                        int wp = (y * linelength);
                
                        for (int x = 0; x < linelength; x++)
                        {
                            byte tp = ImageData[rp + x];
                            ImageData[rp + x] = ImageData[wp + x];
                            ImageData[wp + x] = tp;
                
                        }
                    }
                }

            }
            
        }
        // run length encoded 32 bit
        private void RlcTrueColour32(BinaryReader b)
        {
            int size = ImageWidth * ImageHeight;
            int pos = 0;
            int wp = 0;
            while (pos < size)
            {
                byte h = b.ReadByte();
                if (h < 128)
                {
                    h++;
                    while (h > 0)
                    {
                        ImageData[wp++] = b.ReadByte();
                        pos++;
                        h--;
                    }
                }
                else
                {
                    h -= 127;
                    byte p = b.ReadByte();

                    for (int i = 0; i < h; i++)
                    {
                        ImageData[wp++] = p;
                        pos++;
                    }

                }
            }
        }

        // run length encoded true colour 24 bits
        private void RlcTrueColour24(BinaryReader b)
        {
            int size = ImageWidth * ImageHeight;
            int pos = 0;
            int wp = 0;
            int count = 0;
            int alpha = 255;
            while (pos < size)
            {
                byte h = b.ReadByte();
                if (h < 128)
                {
                    h++;
                    while (h > 0)
                    {
                        ImageData[wp++] = b.ReadByte();
                        count++;
                        if (count == 3)
                        {
                            ImageData[wp++] = (byte)(alpha);
                            count = 0;
                            alpha = 0;
                        }
                        
                        pos++;
                        h--;
                    }
                }
                else
                {
                    h -= 127;
                    byte p = b.ReadByte();
                    
                    for (int i = 0; i < h; i++)
                    {
                        ImageData[wp++] = p;
                        count++;
                        if (count == 3)
                        {
                            ImageData[wp++] = 255;
                            count = 0;
                        }
                        pos++;
                    }

                }
            }
        }

        // run length encoded colour mapped 
        private void RlcColourMapped(BinaryReader b)
        {
            int size = ImageWidth * ImageHeight;
            int pos = 0;
            int wp = 0;
            while (pos < size)
            {
                byte h = b.ReadByte();
                if (h < 128)
                {
                    h++;
                    while (h > 0)
                    {
                        int p = b.ReadByte();
                        int c = ColourMap[p];

                        ImageData[wp++] = (byte)(c & 255);
                        ImageData[wp++] = (byte)((c >> 8) & 255);
                        ImageData[wp++] = (byte)((c >> 16) & 255);
                        ImageData[wp++] = (byte)((c >> 24) & 255);
                        pos++;
                        h--;
                    }
                }
                else
                {
                    h -= 127;
                    int p = b.ReadByte();
                    int c = ColourMap[p];
                    for (int i = 0; i < h; i++)
                    {
                        ImageData[wp++] = (byte)(c & 255);
                        ImageData[wp++] = (byte)((c >> 8) & 255);
                        ImageData[wp++] = (byte)((c >> 16) & 255);
                        ImageData[wp++] = (byte)((c >> 24) & 255);
                        pos++;
                    }

                }
            }
        }

        // Uncompressed black and white 32 bit
        private void UncBw32(BinaryReader b)
        {
            int writepos = 0;
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; y++)
                {
                    int a = (b.ReadInt32()) >> 24;
                    ImageData[writepos++] = (byte)(a & 255);
                    ImageData[writepos++] = (byte)(a & 255);
                    ImageData[writepos++] = (byte)(a & 255);
                    ImageData[writepos++] = 255;
                }
            }
        }

        // Uncompressed black and white 24 bit
        private void UncBw24(BinaryReader b)
        {
            int writepos = 0;
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; y++)
                {
                    int a = b.ReadInt16();
                    a = b.ReadByte();
                    ImageData[writepos++] = (byte)(a & 255);
                    ImageData[writepos++] = (byte)(a & 255);
                    ImageData[writepos++] = (byte)(a & 255);
                    ImageData[writepos++] = (byte)(a & 255);
                }
            }
        }

        // Uncompressed black and white 16 bit
        private void UncBw16(BinaryReader b)
        {
            int writepos = 0;
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; y++)
                {
                    int a = (b.ReadInt16()) >> 8; 
                    ImageData[writepos++] = (byte)(a & 255);
                    ImageData[writepos++] = (byte)(a & 255);
                    ImageData[writepos++] = (byte)(a & 255);
                    ImageData[writepos++] = 255;
                }
            }
        }

        // Uncompressed black and white 8 bit
        private void UncBw8(BinaryReader b)
        {
             int writepos = 0;
             for (int y = 0; y < ImageHeight; y++)
             {
                 for (int x = 0; x < ImageWidth; x++)
                 {
                     int a = b.ReadByte();
                     ImageData[writepos++] = (byte)(a & 255);
                     ImageData[writepos++] = (byte)(a & 255);
                     ImageData[writepos++] = (byte)(a & 255);
                     ImageData[writepos++] = 255;
                 }
             }
        }

        // Uncompressed 32 bit
        private void UncTrue(BinaryReader b)
        {
            int writepos = 0;
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                    int r = b.ReadByte();
                    int g = b.ReadByte();
                    int bl = b.ReadByte();
                    int a = b.ReadByte();
                    
                    ImageData[writepos++] = (byte)(bl & 255);
                    ImageData[writepos++] = (byte)(g & 255);
                    ImageData[writepos++] = (byte)(r & 255);
                    ImageData[writepos++] = (byte)(a & 255); 

                }
            }
        }

        // Uncompressed 24 bit
        private void UncTriple(BinaryReader b)
        {
            int writepos = 0;
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                    int r = b.ReadByte();
                    int g = b.ReadByte();
                    int bl = b.ReadByte();

                    ImageData[writepos++] = (byte)(bl & 255);
                    ImageData[writepos++] = (byte)(g & 255);
                    ImageData[writepos++] = (byte)(r & 255);
                    ImageData[writepos++] = (byte)(255);

                }
            }
        }

        private void UncSixteen(BinaryReader b)
        {
            int writepos = 0;
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                    ushort p = b.ReadUInt16();
                    int bl, g, r, a;
                    bl = (p & 0x1f) << 3;
                    g = ((p >> 5) & 0x1f) << 3;
                    r = ((p >> 10) & 0x1f) << 3;
                    a = 0;
                    if ((p & 0x8000) != 0)
                        a = 255;

                    ImageData[writepos++] = (byte)(bl & 255);
                    ImageData[writepos++] = (byte)(g & 255);
                    ImageData[writepos++] = (byte)(r & 255);
                    ImageData[writepos++] = (byte)a;

                }
            }
        }

        // Uncompressed colour mapped
        private void UncColourMapped(BinaryReader b)
        {
            int writepos = 0;
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                    int i = b.ReadByte();
                    int c = ColourMap[i];

                    ImageData[writepos++] = (byte)((c >> 16) & 255);
                    ImageData[writepos++] = (byte)((c >> 8) & 255);
                    ImageData[writepos++] = (byte)(c & 255);
                    ImageData[writepos++] = (byte)((c >> 24) & 255);
                }
            }
        }
    }
}
