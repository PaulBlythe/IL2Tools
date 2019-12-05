using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class MercatorProjection : MapProjection
    {
        double StartLatitude;
        double StartLongitude;

        public MercatorProjection()
        {
            StartLatitude = StartLongitude = 0;
        }

        public MercatorProjection(double slat, double slon)
        {
            StartLatitude = slat;
            StartLongitude = slon;
        }

        public override Delta GetDelta(double Latitude, double Longitude)
        {
            Delta d = new Delta();

            d.mx = (int)(Longitude - StartLongitude);
            d.my = (int)(Latitude - StartLatitude);

            d.dx = (Longitude - StartLongitude) - d.mx;
            d.dy = (Latitude - StartLatitude) - d.my;
            d.sx = GetMetresPerDegreeLongitude(Latitude);
            d.sy = 111320.0;
            return d;
        }

        public override double GetMetresPerDegreeLongitude(double Latitude)
        {
            double rad = Math.PI / 180.0;
            return 40075000.0 * Math.Cos(rad * Latitude) / 360.0;
        }

        public override double GetNewLatitude(double start_latitude, double metres)
        {
            return start_latitude + (metres / 111320.0);
        }

        public override Delta Step(Delta d, double x)
        {
            d.dx += (x / d.sx);
           
            if (d.dx >= 1)
            {
                d.mx++;
                d.dx -= 1;
            }
           
            return d;
        }

        public override double GetNewLongitude(double longitude, double start_latitude, double metres)
        {
            return longitude + metres / GetMetresPerDegreeLongitude(start_latitude) ; 
        }

        public override Region GetRegion(int width, int height, int scale)
        {
            Region r = new Region();
            r.MinX = StartLongitude;
            r.MaxY = StartLatitude;

            double w1 = (width * scale) / (GetMetresPerDegreeLongitude(StartLatitude));
            double l2 = StartLatitude - (height * scale) / 111320.0;
            double w2 = (width * scale) / (GetMetresPerDegreeLongitude(l2));           
            r.MinY = l2;

            if (w2 > w1)
            {
                w1 = w2;
            }
            r.MaxX = r.MinX + w1;

            return r;
        }

        /// <summary>
        /// Takes longitude, latitude and projects it into pixel space
        /// </summary>
        /// <param name="x">longitude</param>
        /// <param name="y">latitude</param>
        /// <returns></returns>
        public override IntPoint Project(double x, double y, int scale)
        {
            IntPoint res = new IntPoint(0, 0);

            double dy = StartLatitude - y;
            dy *= 111320.0;
            res.y1 = (int)(dy / scale);

            double dx = x - StartLongitude;
            dx *= GetMetresPerDegreeLongitude(x);
            res.x1 = (int)(dx / scale);

            return res;
        }
    }
}
