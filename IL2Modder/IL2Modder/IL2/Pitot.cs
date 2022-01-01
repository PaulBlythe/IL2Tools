using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.IL2
{
    public class Pitot
    {
        public static float[] pitot = { 0.0F, 0.630378F, 0.00632175F, -3.07351E-005F, 4.47977E-008F };

        public static float poly(float[] paramArrayOfFloat, float paramFloat)
        {
            return (((paramArrayOfFloat[4] * paramFloat + paramArrayOfFloat[3]) * paramFloat + paramArrayOfFloat[2]) * paramFloat + paramArrayOfFloat[1]) * paramFloat + paramArrayOfFloat[0];
        }

        public static float Indicator(float paramFloat1, float paramFloat2)
        {
            paramFloat2 *= Atmosphere.density(paramFloat1) / Atmosphere.density(0.0F);
            return paramFloat2;
        }
    }

}
