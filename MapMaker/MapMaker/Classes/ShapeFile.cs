using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using MapMaker.Classes.SHP;

namespace MapMaker
{
    public class ShapeFile
    {
        Int32 FileCode;                             // big endian
        UInt32[] Unused = new UInt32[5];            // big endian
        int FileLength;                             // in 16 bit words including header . big endian

        Int32 Version;
        Int32 ShapeType;

        Region TotalArea;

        double MinZ;
        double MaxZ;
        double MinM;
        double MaxM;

        List<SHPElement> Elements = new List<SHPElement>();
        List<String> Types = new List<string>();

        public ShapeFile(String filename)
        {
            using (BinaryReader b = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                FileCode = ReadBigEndian(b);

                Unused[0] = (UInt32)ReadBigEndian(b);
                Unused[1] = (UInt32)ReadBigEndian(b);
                Unused[2] = (UInt32)ReadBigEndian(b);
                Unused[3] = (UInt32)ReadBigEndian(b);
                Unused[4] = (UInt32)ReadBigEndian(b);

                FileLength = 2 * (int)ReadBigEndian(b);

                Version = b.ReadInt32();
                ShapeType = b.ReadInt32();
                TotalArea = new Region(b);

                MinZ = b.ReadDouble();
                MaxZ = b.ReadDouble();
                MinM = b.ReadDouble();
                MaxM = b.ReadDouble();

                while (b.BaseStream.Position < FileLength)
                {
                    int rn = ReadBigEndian(b);
                    int rs = ReadBigEndian(b);
                    int ShapeType = b.ReadInt32();
                    switch (ShapeType)
                    {
                        case 1:     //Point     
                            Elements.Add(new SHPPoint(b));
                            break;
                        case 3:     //PolyLine
                            Elements.Add(new SHPPolyLine(b));
                            break;
                        case 5:     //Polygon
                            Elements.Add(new SHPPolygon(b));
                            break;
                        case 8:     //MultiPoint
                            Elements.Add(new SHPMultiPoint(b));
                            break;
                        case 11:     //PointZ
                            Elements.Add(new SHPPointZ(b));
                            break;
                        default:
                            throw new Exception("Unknown shape file element " + ShapeType.ToString());
                    }
                }
            }

            String dbname = filename.Replace(".shp", ".csv");
            if (File.Exists(dbname))
            {
                using (TextReader tr = File.OpenText(dbname))
                {
                    String line;        // skip header
                    tr.ReadLine();
                    while (true)
                    {
                        line = tr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        string[] parts = line.Split(',');
                        Types.Add(parts[1]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Elements.Count; i++)
                    Types.Add("Road");
            }
        }

        public void Draw(byte[] target, byte type, Region r, MapProjection mp, int scale, int width, int height)
        {
            int el = 0;
            foreach (SHPElement e in Elements)
            {
                if ((e is SHPPolyLine) && (Types[el] == "Road"))
                {
                    SHPPolyLine sp = (SHPPolyLine)e;
                    if (sp.region.Intersects(r))
                    {
                        for (int i = 0; i < sp.NumParts; i++)
                        {
                            int s1 = sp.Parts[i];
                            int s2;
                            if ((i + 1) == sp.NumParts)
                            {
                                s2 = sp.NumPoints - 1;
                            }
                            else
                            {
                                s2 = sp.Parts[i + 1];
                            }

                            while (s1 < s2)
                            {
                                SHPPoint p1 = sp.Points[s1];
                                SHPPoint p2 = sp.Points[s1 + 1];

                                IntPoint ip1 = mp.Project(p1.X, p1.Y, scale);
                                IntPoint ip2 = mp.Project(p2.X, p2.Y, scale);

                                DrawLine(target, ip1.x1, ip1.y1, ip2.x1, ip2.y1, width, height, 32);

                                s1++;
                            }
                        }
                    }
                }
                el++;
            }
        }

        private Int32 ReadBigEndian(BinaryReader b)
        {
            Int32 res = 0;
            res += b.ReadByte();
            res = res << 8;
            res += b.ReadByte();
            res = res << 8;
            res += b.ReadByte();
            res = res << 8;
            res += b.ReadByte();

            return res;
        }

        private void putpixel(byte[] map, int x1, int y1, byte col, int width, int height)
        {
            if ((x1 >= 0) && (y1 >= 0) && (x1 < width) && (y1 < height))
            {
                int pos = (y1 * width) + x1;
                map[pos] |= col;
            }
        }

        private void DrawLine(byte[] map, int x1, int y1, int x2, int y2, int width, int height, byte col)
        {
            if (y1 > y2)
            {
                int temp = x1;
                x1 = x2;
                x2 = temp;

                temp = y2;
                y2 = y1;
                y1 = temp;

            }
            int delta_x = (x2 - x1);
            int ix = 1;
            if (delta_x < 0)
                ix = -1;

            delta_x = Math.Abs(delta_x) << 1;

            int delta_y = (y2 - y1);
            int iy = 1;
            delta_y = Math.Abs(delta_y) << 1;

            putpixel(map, x1, y1, col, width, height);

            if (delta_x >= delta_y)
            {
                int error = (delta_y - (delta_x >> 1));

                while (x1 != x2)
                {
                    // reduce error, while taking into account the corner case of error == 0
                    if ((error > 0) || ((error == 0) && (ix > 0)))
                    {
                        error -= delta_x;
                        y1 += iy;
                    }
                    // else do nothing

                    error += delta_y;
                    x1 += ix;

                    putpixel(map, x1, y1, col, width, height);
                }
            }
            else
            {
                // error may go below zero
                int error = (delta_x - (delta_y >> 1));

                while (y1 != y2)
                {
                    // reduce error, while taking into account the corner case of error == 0
                    if ((error > 0) || ((error == 0) && (iy > 0)))
                    {
                        error -= delta_y;
                        x1 += ix;
                    }
                    // else do nothing

                    error += delta_x;
                    y1 += iy;

                    putpixel(map, x1, y1, col, width, height);
                }
            }
        }
    }
}
