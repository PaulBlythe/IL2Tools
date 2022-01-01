using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2Modder;

namespace IL2Modder.IL2
{
    public class Aircraft
    {
        public static float[] xyz = new float[3];
        public static float[] ypr = new float[3];

        public static float cvt(float paramFloat1, float paramFloat2, float paramFloat3, float paramFloat4, float paramFloat5)
        {
            float y = Math.Min(Math.Max(paramFloat1, paramFloat2), paramFloat3);
            return paramFloat4 + (paramFloat5 - paramFloat4) * (y - paramFloat2) / (paramFloat3 - paramFloat2);
        }
        public static float interp(float paramFloat1, float paramFloat2, float paramFloat3)
        {
            return paramFloat2 + (paramFloat1 - paramFloat2) * paramFloat3;
        }
        public static float floatindex(float paramFloat, float[] paramArrayOfFloat)
        {
            int i = (int)paramFloat;
            if (i >= paramArrayOfFloat.GetLength(0)-1)
            {
                return paramArrayOfFloat[(paramArrayOfFloat.GetLength(0) - 1)];
            }
            if (i < 0)
            {
                return paramArrayOfFloat[0];
            }
            if ((i == 0) && (paramFloat > 0.0F))
            {
                return paramArrayOfFloat[0] + paramFloat * (paramArrayOfFloat[1] - paramArrayOfFloat[0]);
            }
            return paramArrayOfFloat[i] + paramFloat % i * (paramArrayOfFloat[(i + 1)] - paramArrayOfFloat[i]);
        }
        public static void weaponHooksRegister(String[] hooks)
        {
            Form1.Instance.RegisterHooks(hooks);
        }
        public static void weaponsRegister(String name, String[] weapons)
        {
            Form1.Instance.weaponsRegister(name, weapons);
        }
    }
}
