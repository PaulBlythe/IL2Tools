using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.IL2
{
    public class AnglesFork
    {
        private short src;
        private short dst;
        //private static double halfCircle = 32768.0D;
        //private static float fromDeg = 182.04445F;
        //private static float fromRad = 10430.378F;
        //private static float toDeg = 0.005493164F;
        //private static float toRad = 9.58738E-005F;

        public static float signedAngleDeg(float paramFloat)
        {
            return 0.005493164F * (short)(int)(paramFloat * 182.04445F);
        }

        public static float signedAngleRad(float paramFloat)
        {
            return 9.58738E-005F * (short)(int)(paramFloat * 10430.378F);
        }

        public AnglesFork()
        {
            this.src = (this.dst = 0);
        }

        public AnglesFork(float paramFloat)
        {
            setDeg(paramFloat);
        }

        public AnglesFork(float paramFloat1, float paramFloat2)
        {
            setDeg(paramFloat1, paramFloat2);
        }

        public void set(AnglesFork paramAnglesFork)
        {
            this.src = paramAnglesFork.src;
            this.dst = paramAnglesFork.dst;
        }

        public void setDeg(float paramFloat)
        {
            this.src = (this.dst = (short)(int)(paramFloat * 182.04445F));
        }

        public void setRad(float paramFloat)
        {
            this.src = (this.dst = (short)(int)(paramFloat * 10430.378F));
        }

        public void setSrcDeg(float paramFloat)
        {
            this.src = ((short)(int)(paramFloat * 182.04445F));
        }

        public void setSrcRad(float paramFloat)
        {
            this.src = ((short)(int)(paramFloat * 10430.378F));
        }

        public void setDstDeg(float paramFloat)
        {
            this.dst = ((short)(int)(paramFloat * 182.04445F));
        }

        public void setDstRad(float paramFloat)
        {
            this.dst = ((short)(int)(paramFloat * 10430.378F));
        }

        public void setDeg(float paramFloat1, float paramFloat2)
        {
            this.src = ((short)(int)(paramFloat1 * 182.04445F));
            this.dst = ((short)(int)(paramFloat2 * 182.04445F));
        }

        public void setRad(float paramFloat1, float paramFloat2)
        {
            this.src = ((short)(int)(paramFloat1 * 10430.378F));
            this.dst = ((short)(int)(paramFloat2 * 10430.378F));
        }

        public void reverseSrc()
        {
            this.src = ((short)(this.src + 32768));
        }

        public void reverseDst()
        {
            this.dst = ((short)(this.dst + 32768));
        }

        public void rotateDeg(float paramFloat)
        {
            int i = (short)(int)(paramFloat * 182.04445F);
            this.src = ((short)(this.src + i));
            this.dst = ((short)(this.dst + i));
        }

        public void makeSrcSameAsDst()
        {
            this.src = this.dst;
        }

        public float getSrcDeg()
        {
            return this.src * 0.005493164F;
        }

        public float getDstDeg()
        {
            return this.dst * 0.005493164F;
        }

        public float getSrcRad()
        {
            return this.src * 9.58738E-005F;
        }

        public float getDstRad()
        {
            return this.dst * 9.58738E-005F;
        }

        public float getDeg(float paramFloat)
        {
            if (paramFloat <= 0.0F)
            {
                return this.src * 0.005493164F;
            }
            if (paramFloat >= 1.0F)
            {
                return this.dst * 0.005493164F;
            }
            return (short)(this.src + (short)(int)((short)(this.dst - this.src) * paramFloat)) * 0.005493164F;
        }

        public float getRad(float paramFloat)
        {
            if (paramFloat <= 0.0F)
            {
                return this.src * 9.58738E-005F;
            }
            if (paramFloat >= 1.0F)
            {
                return this.dst * 9.58738E-005F;
            }
            return (short)(this.src + (short)(int)((short)(this.dst - this.src) * paramFloat)) * 9.58738E-005F;
        }

        public float getAbsDiffDeg()
        {
            return Math.Abs((short)(this.dst - this.src) * 0.005493164F);
        }

        public float getAbsDiffRad()
        {
            return Math.Abs((short)(this.dst - this.src) * 9.58738E-005F);
        }

        public float getDiffDeg()
        {
            return (short)(this.dst - this.src) * 0.005493164F;
        }

        public float getDiffRad()
        {
            return (short)(this.dst - this.src) * 9.58738E-005F;
        }

        public bool isInsideDeg(float paramFloat)
        {
            int i = (short)(int)(paramFloat * 182.04445F);
            if ((short)(this.dst - this.src) >= 0)
            {
                return ((short)(i - this.src) >= 0) && ((short)(this.dst - i) >= 0);
            }
            return ((short)(i - this.dst) >= 0) && ((short)(this.src - i) >= 0);
        }

        public bool isInsideRad(float paramFloat)
        {
            int i = (short)(int)(paramFloat * 10430.378F);
            if ((short)(this.dst - this.src) >= 0)
            {
                return ((short)(i - this.src) >= 0) && ((short)(this.dst - i) >= 0);
            }
            return ((short)(i - this.dst) >= 0) && ((short)(this.src - i) >= 0);
        }
    }
}
