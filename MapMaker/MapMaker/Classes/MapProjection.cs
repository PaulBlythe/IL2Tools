using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public abstract class MapProjection
    {
        public abstract double GetMetresPerDegreeLongitude(double Latitude);
        public abstract Delta GetDelta(double Latitude, double Longitude);
        public abstract double GetNewLatitude(double start_latitude, double metres);
        public abstract Delta Step(Delta d, double x);
    }
}
