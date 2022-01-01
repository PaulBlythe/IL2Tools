using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.IL2
{
    public class Tuple3f
    {

        public float x;
        public float y;
        public float z;

        public Tuple3f(float paramFloat1, float paramFloat2, float paramFloat3)
        {
            this.x = paramFloat1;
            this.y = paramFloat2;
            this.z = paramFloat3;
        }

        public Tuple3f(float[] paramArrayOfFloat)
        {
            this.x = paramArrayOfFloat[0];
            this.y = paramArrayOfFloat[1];
            this.z = paramArrayOfFloat[2];
        }

        public Tuple3f(Tuple3f paramTuple3f)
        {
            this.x = paramTuple3f.x;
            this.y = paramTuple3f.y;
            this.z = paramTuple3f.z;
        }

        public Tuple3f(Tuple3d paramTuple3d)
        {
            this.x = ((float)paramTuple3d.x);
            this.y = ((float)paramTuple3d.y);
            this.z = ((float)paramTuple3d.z);
        }

        public Tuple3f()
        {
            this.x = 0.0F;
            this.y = 0.0F;
            this.z = 0.0F;
        }

        public void set(float paramFloat1, float paramFloat2, float paramFloat3)
        {
            this.x = paramFloat1;
            this.y = paramFloat2;
            this.z = paramFloat3;
        }

        public void set(float[] paramArrayOfFloat)
        {
            this.x = paramArrayOfFloat[0];
            this.y = paramArrayOfFloat[1];
            this.z = paramArrayOfFloat[2];
        }

        public void set(Tuple3f paramTuple3f)
        {
            this.x = paramTuple3f.x;
            this.y = paramTuple3f.y;
            this.z = paramTuple3f.z;
        }

        public void set(Tuple3d paramTuple3d)
        {
            this.x = ((float)paramTuple3d.x);
            this.y = ((float)paramTuple3d.y);
            this.z = ((float)paramTuple3d.z);
        }

        public void set(Tuple2f paramTuple2f)
        {
            this.x = paramTuple2f.x;
            this.y = paramTuple2f.y;
            this.z = 0.0F;
        }

        public void set2(Tuple2f paramTuple2f)
        {
            this.x = paramTuple2f.x;
            this.y = paramTuple2f.y;
        }

        public void get(float[] paramArrayOfFloat)
        {
            paramArrayOfFloat[0] = this.x;
            paramArrayOfFloat[1] = this.y;
            paramArrayOfFloat[2] = this.z;
        }

        public void get(Tuple3f paramTuple3f)
        {
            paramTuple3f.x = this.x;
            paramTuple3f.y = this.y;
            paramTuple3f.z = this.z;
        }

        public void add(Tuple3f paramTuple3f1, Tuple3f paramTuple3f2)
        {
            paramTuple3f1.x += paramTuple3f2.x;
            paramTuple3f1.y += paramTuple3f2.y;
            paramTuple3f1.z += paramTuple3f2.z;
        }

        public void add(Tuple3f paramTuple3f)
        {
            this.x += paramTuple3f.x;
            this.y += paramTuple3f.y;
            this.z += paramTuple3f.z;
        }

        public void add(float paramFloat1, float paramFloat2, float paramFloat3)
        {
            this.x += paramFloat1;
            this.y += paramFloat2;
            this.z += paramFloat3;
        }

        public void sub(Tuple3f paramTuple3f1, Tuple3f paramTuple3f2)
        {
            paramTuple3f1.x -= paramTuple3f2.x;
            paramTuple3f1.y -= paramTuple3f2.y;
            paramTuple3f1.z -= paramTuple3f2.z;
        }

        public void sub(Tuple3f paramTuple3f)
        {
            this.x -= paramTuple3f.x;
            this.y -= paramTuple3f.y;
            this.z -= paramTuple3f.z;
        }

        public void sub(float paramFloat1, float paramFloat2, float paramFloat3)
        {
            this.x -= paramFloat1;
            this.y -= paramFloat2;
            this.z -= paramFloat3;
        }

        public void negate(Tuple3f paramTuple3f)
        {
            this.x = (-paramTuple3f.x);
            this.y = (-paramTuple3f.y);
            this.z = (-paramTuple3f.z);
        }

        public void negate()
        {
            this.x = (-this.x);
            this.y = (-this.y);
            this.z = (-this.z);
        }

        public void scale(float paramFloat, Tuple3f paramTuple3f)
        {
            this.x = (paramFloat * paramTuple3f.x);
            this.y = (paramFloat * paramTuple3f.y);
            this.z = (paramFloat * paramTuple3f.z);
        }

        public void scale(Tuple3f paramTuple3f, float paramFloat)
        {
            this.x = (paramFloat * paramTuple3f.x);
            this.y = (paramFloat * paramTuple3f.y);
            this.z = (paramFloat * paramTuple3f.z);
        }

        public void scale(float paramFloat)
        {
            this.x *= paramFloat;
            this.y *= paramFloat;
            this.z *= paramFloat;
        }

        public void scaleAdd(float paramFloat, Tuple3f paramTuple3f1, Tuple3f paramTuple3f2)
        {
            this.x = (paramFloat * paramTuple3f1.x + paramTuple3f2.x);
            this.y = (paramFloat * paramTuple3f1.y + paramTuple3f2.y);
            this.z = (paramFloat * paramTuple3f1.z + paramTuple3f2.z);
        }

        public void scaleAdd(float paramFloat, Tuple3f paramTuple3f)
        {
            this.x = (paramFloat * this.x + paramTuple3f.x);
            this.y = (paramFloat * this.y + paramTuple3f.y);
            this.z = (paramFloat * this.z + paramTuple3f.z);
        }

        

        public bool equals(Tuple3f paramTuple3f)
        {
            return (paramTuple3f != null) && (this.x == paramTuple3f.x) && (this.y == paramTuple3f.y) && (this.z == paramTuple3f.z);
        }

        public bool epsilonEquals(Tuple3f paramTuple3f, float paramFloat)
        {
            return (Math.Abs(paramTuple3f.x - this.x) <= paramFloat) && (Math.Abs(paramTuple3f.y - this.y) <= paramFloat) && (Math.Abs(paramTuple3f.z - this.z) <= paramFloat);
        }

       

        public void clamp(float paramFloat1, float paramFloat2, Tuple3f paramTuple3f)
        {
            set(paramTuple3f);
            clamp(paramFloat1, paramFloat2);
        }

        public void clampMin(float paramFloat, Tuple3f paramTuple3f)
        {
            set(paramTuple3f);
            clampMin(paramFloat);
        }

        public void clampMax(float paramFloat, Tuple3f paramTuple3f)
        {
            set(paramTuple3f);
            clampMax(paramFloat);
        }

        public void absolute(Tuple3f paramTuple3f)
        {
            set(paramTuple3f);
            absolute();
        }

        public void clamp(float paramFloat1, float paramFloat2)
        {
            clampMin(paramFloat1);
            clampMax(paramFloat2);
        }

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

        public void interpolate(Tuple3f paramTuple3f1, Tuple3f paramTuple3f2, float paramFloat)
        {
            paramTuple3f1.x += (paramTuple3f2.x - paramTuple3f1.x) * paramFloat;
            paramTuple3f1.y += (paramTuple3f2.y - paramTuple3f1.y) * paramFloat;
            paramTuple3f1.z += (paramTuple3f2.z - paramTuple3f1.z) * paramFloat;
        }

        public void interpolate(Tuple3f paramTuple3f, float paramFloat)
        {
            this.x += (paramTuple3f.x - this.x) * paramFloat;
            this.y += (paramTuple3f.y - this.y) * paramFloat;
            this.z += (paramTuple3f.z - this.z) * paramFloat;
        }
    }
}
