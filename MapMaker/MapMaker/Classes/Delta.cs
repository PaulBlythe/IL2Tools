using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    /// <summary>
    /// Convenience class
    /// Map deltas in mx and my
    /// Deltas of latitude and longitude (0-1)
    /// Also size of one pixel in meters converted to latitude and longitude
    /// </summary>
    public class Delta
    {
        public int mx;
        public int my;

        public double dx;
        public double dy;

        public double sx;
        public double sy;

        
    }
}
