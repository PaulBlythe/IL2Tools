using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IL2Modder.IL2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.Fox1Animators
{
    public class AAGunAnimation
        : Animator
    {
        public float MaxPitch = 90;
        public float MinPitch = 0;
        public float BarrelTravel = 1;
        public int RateOfFire = 1;

        enum AnimationState
        {
            Raising,
            Lowering,
            Waiting,
            Firing,
            Recovering
        }

        AnimationState state = AnimationState.Raising;
        float timer = 0;
        float pitch = 0;
        int count = 0;

        public override void Update(IL2.HIM him, float dt)
        {
            float delta = 0;
            Node n = him.FindNode("Gun");

            switch (state)
            {
                case AnimationState.Lowering:
                    {
                        pitch -= 10 * dt;
                        if (pitch <= MinPitch)
                        {
                            pitch = MinPitch;
                            state = AnimationState.Raising;
                        }
                    }
                    break;
                case AnimationState.Raising:
                    {
                        pitch += 10 * dt;
                        if (pitch >= MaxPitch)
                        {
                            pitch = MaxPitch;
                            state = AnimationState.Waiting;
                            count = 0;
                        }

                    }
                    break;

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
                            state = AnimationState.Lowering;
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

            
            Matrix m = Matrix.CreateTranslation(-delta, 0, 0);
            m *= Matrix.CreateRotationZ(MathHelper.ToRadians(-pitch));
            n.world = m * n.base_matrix;

        }
    }
}
