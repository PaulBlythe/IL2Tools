using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.IL2
{
    public class Atmosphere
    {
        public float g = 9.8F;
        public float P0 = 101300.0F;
        public float T0 = 288.16F;
        public float ro0 = 1.225F;
        public float Mu0 = 1.825E-006F;

        private static float[] Density = { 1.0F, -9.59387E-005F, 3.53118E-009F, -5.83556E-014F, 2.28719E-019F };
        private static float[] Pressure = { 1.0F, -0.00011844F, 5.6763E-009F, -1.3738E-013F, 1.60373E-018F };
        private static float[] Temperature = { 1.0F, -2.27712E-005F, 2.18069E-010F, -5.71104E-014F, 3.97306E-018F };

        private static float poly(float[] paramArrayOfFloat, float paramFloat)
        {
            return (((paramArrayOfFloat[4] * paramFloat + paramArrayOfFloat[3]) * paramFloat + paramArrayOfFloat[2]) * paramFloat + paramArrayOfFloat[1]) * paramFloat + paramArrayOfFloat[0];
        }

        public static void set(float paramFloat1, float paramFloat2)
        {
            paramFloat1 *= 133.28947F;
            paramFloat2 += 273.16F;

            World.cur().Atm.P0 = paramFloat1;
            World.cur().Atm.T0 = paramFloat2;
            World.cur().Atm.ro0 = (1.225F * (paramFloat1 / 101300.0F) * (288.16F / paramFloat2));
        }

        public static float pressure(float paramFloat)
        {
            if (paramFloat > 18300.0F)
            {
                return 18300.0F / paramFloat * World.cur().Atm.P0 * poly(Pressure, 18300.0F);
            }
            return World.cur().Atm.P0 * poly(Pressure, paramFloat);
        }

        public static float temperature(float paramFloat)
        {
            if (paramFloat > 18300.0F)
            {
                paramFloat = 18300.0F;
            }
            return World.cur().Atm.T0 * poly(Temperature, paramFloat);
        }

        public static float sonicSpeed(float paramFloat)
        {
            return 20.1F * (float)Math.Sqrt(temperature(paramFloat));
        }

        public static float density(float paramFloat)
        {
            if (paramFloat > 18300.0F)
            {
                return 18300.0F / paramFloat * World.cur().Atm.ro0 * poly(Density, 18300.0F);
            }
            return World.cur().Atm.ro0 * poly(Density, paramFloat);
        }

        public static float viscosity(float paramFloat)
        {
            return World.cur().Atm.Mu0 * (float)Math.Pow(temperature(paramFloat) / World.cur().Atm.T0, 0.76D);
        }

        public static float kineticViscosity(float paramFloat)
        {
            return viscosity(paramFloat) / density(paramFloat);
        }
    }

}
