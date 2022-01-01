using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.IL2
{
    public class Tuple3d
    {
        public double x;
        public double y;
        public double z;

        public Tuple3d(double paramDouble1, double paramDouble2, double paramDouble3)
        {
            this.x = paramDouble1;
            this.y = paramDouble2;
            this.z = paramDouble3;
        }

        public Tuple3d(double[] paramArrayOfDouble)
        {
            this.x = paramArrayOfDouble[0];
            this.y = paramArrayOfDouble[1];
            this.z = paramArrayOfDouble[2];
        }

        public Tuple3d(Tuple3d paramTuple3d)
        {
            this.x = paramTuple3d.x;
            this.y = paramTuple3d.y;
            this.z = paramTuple3d.z;
        }

        public Tuple3d(Tuple3f paramTuple3f)
        {
            this.x = paramTuple3f.x;
            this.y = paramTuple3f.y;
            this.z = paramTuple3f.z;
        }

        public Tuple3d()
        {
            this.x = 0.0D;
            this.y = 0.0D;
            this.z = 0.0D;
        }

        public void set(double paramDouble1, double paramDouble2, double paramDouble3)
        {
            this.x = paramDouble1;
            this.y = paramDouble2;
            this.z = paramDouble3;
        }

        public void set(double[] paramArrayOfDouble)
        {
            this.x = paramArrayOfDouble[0];
            this.y = paramArrayOfDouble[1];
            this.z = paramArrayOfDouble[2];
        }

        public void set(Tuple3d paramTuple3d)
        {
            this.x = paramTuple3d.x;
            this.y = paramTuple3d.y;
            this.z = paramTuple3d.z;
        }

        public void set(Tuple3f paramTuple3f)
        {
            this.x = paramTuple3f.x;
            this.y = paramTuple3f.y;
            this.z = paramTuple3f.z;
        }

        public void set(Tuple2d paramTuple2d)
        {
            this.x = paramTuple2d.x;
            this.y = paramTuple2d.y;
            this.z = 0.0D;
        }

        public void set2(Tuple2d paramTuple2d)
        {
            this.x = paramTuple2d.x;
            this.y = paramTuple2d.y;
        }

        public void get(double[] paramArrayOfDouble)
        {
            paramArrayOfDouble[0] = this.x;
            paramArrayOfDouble[1] = this.y;
            paramArrayOfDouble[2] = this.z;
        }

        public void get(Tuple3d paramTuple3d)
        {
            paramTuple3d.x = this.x;
            paramTuple3d.y = this.y;
            paramTuple3d.z = this.z;
        }

        public void add(Tuple3d paramTuple3d1, Tuple3d paramTuple3d2)
        {
            paramTuple3d1.x += paramTuple3d2.x;
            paramTuple3d1.y += paramTuple3d2.y;
            paramTuple3d1.z += paramTuple3d2.z;
        }

        public void add(Tuple3d paramTuple3d)
        {
            this.x += paramTuple3d.x;
            this.y += paramTuple3d.y;
            this.z += paramTuple3d.z;
        }

        public void add(double paramDouble1, double paramDouble2, double paramDouble3)
        {
            this.x += paramDouble1;
            this.y += paramDouble2;
            this.z += paramDouble3;
        }

        public void sub(Tuple3d paramTuple3d1, Tuple3d paramTuple3d2)
        {
            paramTuple3d1.x -= paramTuple3d2.x;
            paramTuple3d1.y -= paramTuple3d2.y;
            paramTuple3d1.z -= paramTuple3d2.z;
        }

        public void sub(Tuple3d paramTuple3d)
        {
            this.x -= paramTuple3d.x;
            this.y -= paramTuple3d.y;
            this.z -= paramTuple3d.z;
        }

        public void sub(double paramDouble1, double paramDouble2, double paramDouble3)
        {
            this.x -= paramDouble1;
            this.y -= paramDouble2;
            this.z -= paramDouble3;
        }

        public void negate(Tuple3d paramTuple3d)
        {
            this.x = (-paramTuple3d.x);
            this.y = (-paramTuple3d.y);
            this.z = (-paramTuple3d.z);
        }

        public void negate()
        {
            this.x = (-this.x);
            this.y = (-this.y);
            this.z = (-this.z);
        }

        public void scale(double paramDouble, Tuple3d paramTuple3d)
        {
            this.x = (paramDouble * paramTuple3d.x);
            this.y = (paramDouble * paramTuple3d.y);
            this.z = (paramDouble * paramTuple3d.z);
        }

        public void scale(Tuple3d paramTuple3d, double paramDouble)
        {
            this.x = (paramDouble * paramTuple3d.x);
            this.y = (paramDouble * paramTuple3d.y);
            this.z = (paramDouble * paramTuple3d.z);
        }

        public void scale(double paramDouble)
        {
            this.x *= paramDouble;
            this.y *= paramDouble;
            this.z *= paramDouble;
        }

        public void scaleAdd(double paramDouble, Tuple3d paramTuple3d1, Tuple3d paramTuple3d2)
        {
            this.x = (paramDouble * paramTuple3d1.x + paramTuple3d2.x);
            this.y = (paramDouble * paramTuple3d1.y + paramTuple3d2.y);
            this.z = (paramDouble * paramTuple3d1.z + paramTuple3d2.z);
        }

        public void scaleAdd(double paramDouble, Tuple3d paramTuple3d)
        {
            this.x = (paramDouble * this.x + paramTuple3d.x);
            this.y = (paramDouble * this.y + paramTuple3d.y);
            this.z = (paramDouble * this.z + paramTuple3d.z);
        }


        public bool equals(Tuple3d paramTuple3d)
        {
            return (paramTuple3d != null) && (this.x == paramTuple3d.x) && (this.y == paramTuple3d.y) && (this.z == paramTuple3d.z);
        }

        public bool epsilonEquals(Tuple3d paramTuple3d, double paramDouble)
        {
            return (Math.Abs(paramTuple3d.x - this.x) <= paramDouble) && (Math.Abs(paramTuple3d.y - this.y) <= paramDouble) && (Math.Abs(paramTuple3d.z - this.z) <= paramDouble);
        }


        public void absolute(Tuple3d paramTuple3d)
        {
            set(paramTuple3d);
            absolute();
        }

        public void absolute()
        {
            if (this.x < 0.0D)
            {
                this.x = (-this.x);
            }
            if (this.y < 0.0D)
            {
                this.y = (-this.y);
            }
            if (this.z < 0.0D)
            {
                this.z = (-this.z);
            }
        }

        /**
         * @deprecated
         */
        public void clamp(float paramFloat1, float paramFloat2)
        {
            clampMin(paramFloat1);
            clampMax(paramFloat2);
        }

        /**
         * @deprecated
         */
        public void clamp(float paramFloat1, float paramFloat2, Tuple3d paramTuple3d)
        {
            set(paramTuple3d);
            clamp(paramFloat1, paramFloat2);
        }

        /**
         * @deprecated
         */
        public void clampMin(float paramFloat)
        {
            if (this.x < paramFloat)
            {
                this.x = paramFloat;
            }
            if (this.y < paramFloat)
            {
                this.y = paramFloat;
            }
            if (this.z < paramFloat)
            {
                this.z = paramFloat;
            }
        }

        /**
         * @deprecated
         */
        public void clampMin(float paramFloat, Tuple3d paramTuple3d)
        {
            set(paramTuple3d);
            clampMin(paramFloat);
        }

        /**
         * @deprecated
         */
        public void clampMax(float paramFloat, Tuple3d paramTuple3d)
        {
            set(paramTuple3d);
            clampMax(paramFloat);
        }

        /**
         * @deprecated
         */
        public void clampMax(float paramFloat)
        {
            if (this.x > paramFloat)
            {
                this.x = paramFloat;
            }
            if (this.y > paramFloat)
            {
                this.y = paramFloat;
            }
            if (this.z > paramFloat)
            {
                this.z = paramFloat;
            }
        }

        public void clamp(double paramDouble1, double paramDouble2)
        {
            clampMin(paramDouble1);
            clampMax(paramDouble2);
        }

        public void clamp(double paramDouble1, double paramDouble2, Tuple3d paramTuple3d)
        {
            set(paramTuple3d);
            clamp(paramDouble1, paramDouble2);
        }

        public void clampMin(double paramDouble)
        {
            if (this.x < paramDouble)
            {
                this.x = paramDouble;
            }
            if (this.y < paramDouble)
            {
                this.y = paramDouble;
            }
            if (this.z < paramDouble)
            {
                this.z = paramDouble;
            }
        }

        public void clampMin(double paramDouble, Tuple3d paramTuple3d)
        {
            set(paramTuple3d);
            clampMin(paramDouble);
        }

        public void clampMax(double paramDouble, Tuple3d paramTuple3d)
        {
            set(paramTuple3d);
            clampMax(paramDouble);
        }

        public void clampMax(double paramDouble)
        {
            if (this.x > paramDouble)
            {
                this.x = paramDouble;
            }
            if (this.y > paramDouble)
            {
                this.y = paramDouble;
            }
            if (this.z > paramDouble)
            {
                this.z = paramDouble;
            }
        }

        public void interpolate(Tuple3d paramTuple3d1, Tuple3d paramTuple3d2, float paramFloat)
        {
            set(paramTuple3d1);
            interpolate(paramTuple3d2, paramFloat);
        }

        public void interpolate(Tuple3d paramTuple3d, float paramFloat)
        {
            this.x += paramFloat * (paramTuple3d.x - this.x);
            this.y += paramFloat * (paramTuple3d.y - this.y);
            this.z += paramFloat * (paramTuple3d.z - this.z);
        }

        public void interpolate(Tuple3d paramTuple3d1, Tuple3d paramTuple3d2, double paramDouble)
        {
            paramTuple3d1.x += (paramTuple3d2.x - paramTuple3d1.x) * paramDouble;
            paramTuple3d1.y += (paramTuple3d2.y - paramTuple3d1.y) * paramDouble;
            paramTuple3d1.z += (paramTuple3d2.z - paramTuple3d1.z) * paramDouble;
        }

        public void interpolate(Tuple3d paramTuple3d, double paramDouble)
        {
            this.x += paramDouble * (paramTuple3d.x - this.x);
            this.y += paramDouble * (paramTuple3d.y - this.y);
            this.z += paramDouble * (paramTuple3d.z - this.z);
        }
    }

}
