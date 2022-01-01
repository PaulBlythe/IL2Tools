using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IL2Modder.AC3D
{
    public class SGI_RGB
    {
        public ushort magic;
        public uint max;
        public uint min;
        public uint colormap;
        public byte type;
        public byte bpp;
        public uint[] start;
        public int[] leng;
        public ushort dim;
        public ushort xsize;
        public ushort ysize;
        public ushort zsize;
        public int tablen;
        public bool isSwapped;

        private BinaryReader reader;
        private byte[] rle_temp;

        private ushort swab_short(ushort x)
        {
            int y = x;
            if (isSwapped)
            {
                y = ((x >> 8) & 0x00FF) | ((x << 8) & 0xFF00);
            }
            return (ushort)y;
        }

        private uint swab_int(uint x)
        {
            uint y = 0;
            if (isSwapped)
            {
                y = ((x >> 24) & 0x000000FF) |
                     ((x >> 8) & 0x0000FF00) |
                     ((x << 8) & 0x00FF0000) |
                     ((x << 24) & 0xFF000000);
            }
            return (uint)y;
        }

        private void swab_int_array(int[] x, int leng)
        {
            if (!isSwapped)
                return;

            for (int i = 0; i < leng; i++)
            {
                int y = (int)((x[i] >> 24) & 0x0000FF); 
                y += (int)((x[i] >> 8) & 0x00FF00);
                y += (int)((x[i] << 8) & 0xFF0000); 
                y += (int)((x[i] << 24) & (UInt32)0xFF000000);

                x[i] = y;
            }
        }

        private void swab_uint_array(uint[] x, int leng)
        {
            if (!isSwapped)
                return;

            for (int i = 0; i < leng; i++)
            {
                uint y = (uint)((x[i] >> 24) & 0x0000FF);
                y += (uint)((x[i] >> 8) & 0x00FF00);
                y += (uint)((x[i] << 8) & 0xFF0000);
                y += (uint)((x[i] << 24) & (UInt32)0xFF000000);

                x[i] = y;
            }
        }

        private byte readByte()
        {
            return reader.ReadByte();
        }

        private ushort readShort()
        {
            ushort x = reader.ReadUInt16();
            return swab_short(x);
        }

        private uint readInt()
        {
            uint x = reader.ReadUInt32();
            return swab_int(x);
        }

        void getRow ( ref byte[] buf, int pos2, int y, int z )
        {
            if ( y >= ysize ) y = ysize - 1 ;
            if ( z >= zsize ) z = zsize - 1 ;

            reader.BaseStream.Position = start [ z * ysize + y ];

            if (type == 1)
            {
                int pos = 0;
                int length = leng[z * ysize + y];

                rle_temp = reader.ReadBytes(length);
                byte pixel, count;
                while (pos < length)
                {
                    pixel = rle_temp[pos++];
                    count = (byte)(pixel & 0x7f);

                    if (count == 0)
                        break;

                    if ((pixel & 0x80) != 0)
                    {
                        while (count-- > 0)
                            buf[pos2++] = rle_temp[pos++];
                    }
                    else
                    {
                        pixel = rle_temp[pos++];

                        while (count-- > 0)
                            buf[pos2++] = pixel;
                    }
                }
            }
            else
            {
                rle_temp = reader.ReadBytes(xsize);
                for (int i = 0; i < xsize; i++)
                    buf[i + pos2] = rle_temp[i];
            }
        }

        void getPlane (ref byte [] buf, int z )
        {
             if ( z >= zsize ) z = zsize - 1 ;

            for ( int y = 0 ; y < ysize ; y++ )
                getRow (ref buf , y * xsize , y, z ) ;
        }

        void getImage ( ref byte [] buf )
        {
 
          for ( int y = 0 ; y < ysize ; y++ )
            for ( int z = 0 ; z < zsize ; z++ )
              getRow ( ref buf , ( z * ysize + y ) * xsize , y, z ) ;
        }

        void readHeader()
        {
            magic = readShort();

            if ((magic != 0x01DA) && (magic != 0xDA01))
            {
                throw new Exception("Magic failure... oh err missus  magic = " + magic);
            }

            if (magic == 0xDA01)
            {
                isSwapped = true;
                magic = swab_short(magic);
            }

            type = readByte();
            bpp = readByte();
            dim = readShort();

            if (dim > 255)
            {
                
                isSwapped = !isSwapped;
                dim = swab_short(dim);
                magic = 0x01DA;
            }

            xsize = readShort();
            ysize = readShort();
            zsize = readShort();
            min = readInt();
            max = readInt();
            readInt();  /* Dummy field */

             int i ;

            for ( i = 0 ; i < 80 ; i++ )
                readByte () ;         /* Name field */

            colormap = readInt () ;

            for ( i = 0 ; i < 404 ; i++ )
                readByte () ;         /* Dummy field */

            makeConsistant () ;

            tablen = ysize * zsize ;
            start = new uint [ tablen ] ;
            leng  = new int [ tablen ] ;
        }

        void makeConsistant ()
        {
             /*
                Sanity checks - and a workaround for buggy RGB files generated by
                the MultiGen Paint program because it will sometimes get confused
                about the way to represent maps with more than one component.

                eg   Y > 1, Number of dimensions == 1
                     Z > 1, Number of dimensions == 2
              */

            if (ysize > 1 && dim < 2) dim = 2;
            if (zsize > 1 && dim < 3) dim = 3;
            if (dim < 1) ysize = 1;
            if (dim < 2) zsize = 1;
            if (dim > 3) dim = 3;
            if (zsize < 1 && ysize == 1) dim = 1;
            if (zsize < 1 && ysize != 1) dim = 2;
            if (zsize >= 1) dim = 3;

            /*
              A very few SGI image files have 2 bytes per component - this
              library cannot deal with those kinds of files. 
            */

            if (bpp == 2)
            {
                throw new Exception("ssgLoadTexture: Can't work with SGI images with bpp"+ bpp);
            }

            bpp = 1;
            min = 0;
            max = 255;
            magic = 0x01DA;
            colormap = 0;
        }

        public TextureImage LoadFile(String name)
        {
            TextureImage ti = new TextureImage();

            reader = new BinaryReader(File.Open(name, FileMode.Open));
            readHeader();

            if ( type == 1 )
            {
                for (int i=0; i<tablen; i++)
                {
                    start[i]=readInt();
                }
                for (int i=0; i<tablen; i++)
                {
                    leng[i]=(int)readInt();
                } 
               

                int maxlen = 0 ;

                for ( int i = 0 ; i < tablen ; i++ )
                if ( leng [ i ] > maxlen )
                    maxlen = leng [ i ] ;

                rle_temp = new byte [ maxlen ] ;
            }
            else
            {
                rle_temp = null ;

                for ( int i = 0 ; i < zsize ; i++ )
                    for ( int j = 0 ; j < ysize ; j++ )
                    {
                        start [ i * ysize + j ] = (uint)(xsize * ( i * ysize + j ) + 512);
                        leng  [ i * ysize + j ] = xsize ;
                    }
            }

            byte [] image = new byte [ xsize * ysize * 4 ] ;
            byte [] rbuf = new byte [xsize ] ;
            byte [] gbuf = (zsize>1) ? new byte [ xsize ] : null ;
            byte [] bbuf = (zsize>2) ? new byte [ xsize ] : null ;
            byte [] abuf = (zsize>3) ? new byte [ xsize ] : null ;
            int wpos = 0;
            for ( int y = 0 ; y < ysize ; y++ )
            {
                int x ;

                switch (zsize )
                {
                    case 1 :
                        getRow ( ref rbuf,0, y, 0 ) ;

                        for (x = 0; x < xsize; x++)
                        {
                            image[wpos++] = rbuf[x];
                            image[wpos++] = 0;
                            image[wpos++] = 0;
                            image[wpos++] = 255;

                        }
                        break ;

                    case 2 :
                        getRow (ref rbuf, 0, y, 0 ) ;
                        getRow (ref gbuf, 0, y, 1 ) ;

                        for ( x = 0 ; x < xsize ; x++ )
                        {
                            image[wpos++] = rbuf [ x ] ;
                            image[wpos++] = gbuf [ x ] ;
                            image[wpos++] = 0;
                            image[wpos++] = 255;
                        }
                        break ;

                    case 3 :
                        getRow (ref rbuf,0, y, 0 ) ;
                        getRow (ref gbuf,0, y, 1 ) ;
                        getRow (ref bbuf,0, y, 2 ) ;

                        for ( x = 0 ; x < xsize ; x++ )
                        {
                            image[wpos++] = rbuf[x];
                            image[wpos++] = gbuf[x];
                            image[wpos++] = bbuf[x];
                            image[wpos++] = 255;
                        }
                        break ;

                    case 4 :
                        getRow (ref rbuf,0, y, 0 ) ;
                        getRow (ref gbuf,0, y, 1 ) ;
                        getRow (ref bbuf,0, y, 2 ) ;
                        getRow (ref abuf,0, y, 3 ) ;

                        for ( x = 0 ; x < xsize ; x++ )
                        {
                            image[wpos++] = rbuf[x];
                            image[wpos++] = gbuf[x];
                            image[wpos++] = bbuf[x];
                            image[wpos++] = abuf[x];
                        }
                        break ;

                }
            }

            reader.Close();
            ti.height = ysize;
            ti.width = xsize;
            ti.bpp = 32;
            ti.imageData = image;

            return ti;
        }
    }
}
