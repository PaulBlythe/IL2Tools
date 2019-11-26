using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class SRTM
    {
        public int Size;
        public short[,] Heights;
        public int Latitude;
        public bool IsWater = false;



        public SRTM(int Latitude, int Longitude, String directory)
        {
            String filename = Path.Combine(directory, GetFileName(Latitude, Longitude));
            if (File.Exists(filename))
            {
                Load(filename);
            }
            else
            {
                IsWater = true;
            }
        }

        public SRTM(String file)
        {
            Load(file);
        }


        private void Load(String file)
        {
            FileStream fin = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader br = new BinaryReader(fin);

            FileInfo f = new FileInfo(file);
            long s1 = f.Length;

            if (s1 == (1201 * 1201 * 2))
            {
                Size = 1201;
            }
            else if (s1 == (3601 * 3601 * 2))
            {
                Size = 3601;
            }
            else
            {
                throw new Exception("The height file is an illegal size ");
            }
            Heights = new short[Size, Size];


            String DirectoryName = Path.GetFileNameWithoutExtension(file);
            String NS = DirectoryName.Substring(0, 1);

            int ei = DirectoryName.IndexOf('E');
            int wi = DirectoryName.IndexOf('W');
            if (ei <= 0)
                ei = wi;
            String EW = DirectoryName.Substring(ei, 1);
            String lat = DirectoryName.Substring(1, ei - 1);
            Latitude = int.Parse(lat);

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    int hb = br.ReadByte();
                    int lb = br.ReadByte();

                    Heights[j, i] = (short)((hb * 256) + lb);
                }
            }

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (Heights[j,i] == -32768)
                    {
                        Heights[j, i] = Smooth(j, i);
                    }
                }
            }

            br.Close();
            fin.Close();
        }

        /// <summary>
        /// Width of one HGT file in metres
        /// </summary>
        /// <param name="Latitude"></param>
        /// <returns></returns>
        public static double GetMetresPerDegreeLongitude(double Latitude)
        {
            double rad = Math.PI / 180;
            return 40075000 * Math.Cos(rad * Latitude) / 360;
        }

        public static String GetFileName(double Latitude, double Longitude)
        {
            String NS, EW;
            NS = "S";

            if (Latitude > 0)
                NS = "N";
            EW = "E";
            if (Longitude < 0)
                EW = "W";

            return String.Format("{0}{1:00}{2}{3:000}.hgt", NS, Math.Abs(Latitude), EW, Math.Abs(Longitude));
        }

        /// <summary>
        /// Gets the height in metres at a point averaging the data
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public short GetHeight(Delta delta, double m)
        {
            if (IsWater)
                return 0;

            int x_range_in_records = (int)((m * Size) / delta.sx);
            int y_range_in_records = (int)((m * Size) / delta.sy);
            if (x_range_in_records < 1)
                x_range_in_records = 1;
            if (y_range_in_records < 1)
                y_range_in_records = 1;

            int start_x = (int)(delta.dx * Size);
            int start_y = (int)(delta.dy * Size);

            if (start_x > Size)
                start_x = Size;

            float res = 0;
            for (int i = 0; i < y_range_in_records; i++)
            {
                for (int j = 0; j < x_range_in_records; j++)
                {
                    res += Heights[Safe(start_x + j), Safe(start_y + i)];
                }
            }

            res /= (y_range_in_records * x_range_in_records);
            return (short)res;

        }

        /// <summary>
        /// Helper makes sure we don't go outside the array
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private int Safe(int x)
        {
            if (x < 0)
                x = 0;
            if (x >= Size)
                x = Size - 1;
            return x;
        }

        private short Smooth(int x, int y)
        {
            int sx = Math.Max(0, x - 3);
            int sy = Math.Max(0, y - 3);
            int ex = Math.Min(Size - 1, x + 3);
            int ey = Math.Min(Size - 1, y + 3);

            int h = 0;
            int count = 0;
            for (int yy = sy; yy<=ey; yy++)
            {
                for (int xx = sx; xx<=ex; xx++)
                {
                    if (Heights[xx, yy] != -32768)
                    {
                        h += (int)Heights[xx, yy];
                        count++;
                    }
                }
            }
            h /= count;

            return (short)h;

        }
    }
}
