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
           
            if (d.dx>=1)
            {
                d.mx++;
                d.dx -= 1;
            }
           
            return d;
        }

    }
}
