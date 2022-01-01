using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IL2Modder.IL2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.Fox1Animators
{
    public class FieldGunAnimator
        : Animator
    {
        public float MaxPitch = 20;
        public float MinPitch = 0;
        public float Sweep = 22;
        public float BarrelTravel = 0.5f;
        public int RateOfFire = 4;

        enum AnimationState
        {
            Raising,
            Lowering,
            Waiting,
            Firing,
            Recovering
        }

        AnimationState state = AnimationState.Waiting;
        float timer = 0;
        float pitch = 0;
        float yaw = 0;
        float yaw_dir = 3;
        float pitch_dir = 2;
        int count = 0;

        public override void Update(IL2.HIM him, float dt)
        {
            float delta = 0;
            Node n = him.FindNode("Gun");
            Node n2 = him.FindNode("Head");

            switch (state)
            {
                
                case AnimationState.Waiting:
                    {
                        timer += dt;
                        float target_time = 60.0f / RateOfFire;
                        if (timer >= target_time)
                        {
                            timer = 0;
                            state = AnimationState.Firing;

                        }
                        delta = 0;
                    }
                    break;

                case AnimationState.Firing:
                    {
                        state = AnimationState.Recovering;
                        delta = 0;
                        count++;
                        if (count == 5)
                        {
                            state = AnimationState.Waiting;
                        }
                        else
                        {
                            List<Vector3> positions = new List<Vector3>();
                            List<Vector3> directions = new List<Vector3>();
                            him.FindHook(" gun <base>", ref positions, ref directions);
                            int i = 0;
                            foreach (Vector3 v in positions)
                            {
                                ObjectViewer.Instance.AddGunShot(v, directions[i]);
                                ObjectViewer.Instance.AddGunShot(v, directions[i]);
                                ObjectViewer.Instance.AddGunShot(v, directions[i]);

                                ObjectViewer.Instance.AddSmoke(v, 0.1f * directions[i]);
                                ObjectViewer.Instance.AddSmoke(v, 0.1f * directions[i]);
                                ObjectViewer.Instance.AddSmoke(v, 0.1f * directions[i]);
                                i++;
                            }
                        }
                    }
                    break;

                case AnimationState.Recovering:
                    {
                        timer += dt;
                        delta = 1 - timer;
                        if (timer >= 1)
                        {
                            state = AnimationState.Waiting;
                            timer = 0;
                            delta = 0;
                        }
                        delta *= BarrelTravel;
                    }
                    break;
            }

            #region Sweep
            yaw += yaw_dir * dt;
            if (yaw > Sweep)
            {
                yaw = Sweep;
                yaw_dir = -yaw_dir;
            }
            if (yaw < -Sweep)
            {
                yaw = -Sweep;
                yaw_dir = -yaw_dir;
            }
            #endregion

            #region Pitch

            pitch += pitch_dir * dt;
            if (pitch > MaxPitch)
            {
                pitch = MaxPitch;
                pitch_dir *= -1;
            }
            if (pitch < MinPitch)
            {
                pitch = MinPitch;
                pitch_dir *= -1;
            }
            #endregion

            Matrix m = Matrix.CreateTranslation(-delta, 0, 0);
            m *= Matrix.CreateRotationZ(MathHelper.ToRadians(-pitch));
            n.world = m * n.base_matrix;

            n2.world = Matrix.CreateRotationZ(MathHelper.ToRadians(yaw)) * n2.base_matrix;

        }
    }
}
