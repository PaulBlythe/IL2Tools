using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.IL2
{
    public class FlightVariables
    {
        public float Altitude = 500;
        public float FlapsControl = 0;
        public float GearControl = 0;
        public float Fuel = 0;
        public float Yaw = 0;
        public float Roll = 0;
        public float Pitch = 0;
        public float RPM = 0;
        public float KMH = 0;
        public float ElevatorControl = 0;
        public float AileronControl = 0;
        public float PowerControl = 0;
        public float BayControls = 0;
        public float CockpitState = 0;
        public float VSpeed = 0;
        public float[] astateEngineStates = new float[] { 0, 0, 0, 0 };

        float AltitudeDelta = 47; 
        float FuelDelta = 6.5f;
        float RPMDelta = 10;
        float KMHDelta = 10;
        float PowerControlDelta = 0.05f;
        float Time = 0;

        public void Animate()
        {
            DateTime now = DateTime.Now;
            Time = (((now.TimeOfDay.Hours * 60) + now.TimeOfDay.Minutes) * 60) + now.TimeOfDay.Seconds;
            Fuel += FuelDelta;
            Yaw += Form1.Yaw * 0.5f;
            Roll += Form1.Roll * 2.0f;
            Pitch += Form1.Pitch * 2.0f;

            Matrix world = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll);
            VSpeed = 1000 * Vector3.Transform(Vector3.UnitZ, world).Y * KMH / (60.0f * 60.0f);
            Altitude += (VSpeed * 0.02f);
            RPM += Form1.Throttle;
            RPM = Math.Min(RPM, 12000);
            KMH += Form1.Throttle * 0.2f;
            KMH = Math.Min(KMH, 2500);

            ElevatorControl = Form1.Pitch;
            AileronControl = Form1.Roll;
            PowerControl = Form1.Throttle;

            CockpitState = ObjectViewer.DoorAngle;

            if ((Altitude > 40000) || (Altitude < 100))
                AltitudeDelta = -AltitudeDelta;
            if ((Fuel > 700) || (Fuel < 0))
                FuelDelta = -FuelDelta;
            if ((RPM > 6000) || (RPM < 10))
                RPMDelta = -RPMDelta;
            if ((KMH > 1000) || (KMH < 10))
                KMHDelta = -KMHDelta;
            if ((PowerControl > 0.99) || (PowerControl < 0.01))
                PowerControlDelta = -PowerControlDelta;
            if (Yaw > 360)
                Yaw -= 360;
            if (Yaw < 0)
                Yaw += 360;
            if (Roll > 360)
                Roll -= 360;
            if (Roll < 0)
                Roll += 360;
            if (Pitch > 360)
                Pitch -= 360;
            if (Pitch < 0)
                Pitch += 320;

            FlapsControl = ObjectViewer.FlapAngle / 60.0f;
            GearControl = ObjectViewer.GearAngle / MathHelper.PiOver2;
            BayControls = ObjectViewer.BayAngle / MathHelper.PiOver2;
            if (ObjectViewer.EngineFireActive)
            {
                astateEngineStates[0] = astateEngineStates[1] = astateEngineStates[2] = astateEngineStates[3] = 0;
            }
            else
            {
                astateEngineStates[0] = astateEngineStates[1] = astateEngineStates[2] = astateEngineStates[3] = 4;
            }
        }

        public float getVertSpeed()
        {
            return VSpeed;
        }
        public float getGear()
        {
            return 1.0f - GearControl;
        }
        public float getRudder()
        {
            return Form1.Yaw;
        }
        public float getTimeofDay()
        {
            return Time;
        }
        public float getKren()
        {
            return Roll;
        }
        public float getTangage()
        {
            return -Pitch;
        }
        Vector3 getBallAccel()
        {
            Vector3 TmpV = new Vector3(0,Form1.Throttle, 0);
            TmpV = Vector3.Transform(TmpV, Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(Yaw), MathHelper.ToRadians(Pitch), MathHelper.ToRadians(Roll)));
            TmpV.Z -= 9.81f;
            
            return TmpV;
        }
        public float getBall(double paramDouble)
        {
            double pictBall;
            double d1;
            
            if (-getBallAccel().Z > 0.001D)
            {
                d1 = MathHelper.ToDegrees((float)Math.Atan2(getBallAccel().Y, -getBallAccel().Z));
                if (d1 > 20.0D) 
                    d1 = 20.0D;
                else if (d1 < -20.0D) 
                    d1 = -20.0D;
                pictBall =  d1;
            }
            else
            {
                pictBall = 20;
            }
            if (pictBall > paramDouble) 
                pictBall = paramDouble;
            else if (pictBall < -paramDouble) 
                pictBall = (-paramDouble);
            return (float)pictBall;
        }
    }
}
