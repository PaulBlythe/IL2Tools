using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IL2Modder.IL2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.Fox1Animators
{
    public class ShipsGunAnimation
        : Animator
    {
        public float MinPitch = 0;
        public float MaxPitch = 45;
        public float Speed = 10;
        public float RateOfFire = 12;
        public float BarrelTravel = 1;
        public String parent_node = "";
        public int GunType = 0;

        enum SGA_Animation_state
        {
            Waiting,
            Raising,
            Lowering,
            Firing
        };

        SGA_Animation_state state = SGA_Animation_state.Raising;
        float angle = 0;
        int count = 0;
        float timer = 0;
        float displacement = 0;

        public ShipsGunAnimation(String name)
        {
            parent_node = name;

        }

        public override void Update(IL2.HIM him, float dt)
        {
            Node n = him.FindNode(parent_node);

            switch (state)
            {
                case SGA_Animation_state.Raising:
                    {
                        angle += Speed * dt;
                        if (angle >= MaxPitch)
                        {
                            angle = MaxPitch;
                            state = SGA_Animation_state.Waiting;
                        }
                    }
                    break;

                case SGA_Animation_state.Waiting:
                    {
                        timer += dt;
                        float delay = (60 / RateOfFire );
                        if (timer >= delay)
                        {
                            timer = 0;
                            state = SGA_Animation_state.Firing;
                        }
                    }
                    break;

                case SGA_Animation_state.Firing:
                    {
                        count++;
                        if (count == 5)
                        {
                            state = SGA_Animation_state.Lowering;
                            count = 0;
                        }
                        else
                        {
                            List<Vector3> positions = new List<Vector3>();
                            List<Vector3> directions = new List<Vector3>();
                            him.FindHook("ShellStart", ref positions, ref directions, n);
                            int i = 0;
                            foreach (Vector3 v in positions)
                            {
                                if (GunType > 0)
                                {
                                    ObjectViewer.Instance.AddGunShot(v, directions[i]);
                                    ObjectViewer.Instance.AddGunShot(v, directions[i]);
                                    ObjectViewer.Instance.AddGunShot(v, directions[i]);
                                }
                                else
                                {
                                    Vector3 right = directions[i];

                                    for (int ii = 0; ii < 20; ii++)
                                    {
                                        float d = ii * 0.25f;
                                        ObjectViewer.Instance.AddLargeGunShot(v + d * directions[i], Vector3.Zero);
                                        ObjectViewer.Instance.AddLargeGunShot(v + d * directions[i], Vector3.Zero);
                                        ObjectViewer.Instance.AddSmoke(v + d * directions[i], Vector3.Zero);
                                        ObjectViewer.Instance.AddSmoke(v + d * directions[i], Vector3.Zero);
                                    }
                                }

                                ObjectViewer.Instance.AddSmoke(v, directions[i]);
                                ObjectViewer.Instance.AddSmoke(v, directions[i]);
                                ObjectViewer.Instance.AddSmoke(v, directions[i]);
                                i++;
                            }
                            state = SGA_Animation_state.Waiting;
                            displacement = BarrelTravel;
                        }
                    }
                    break;

                case SGA_Animation_state.Lowering:
                    {
                        angle -= Speed * dt;
                        if (angle <= MinPitch)
                        {
                            angle = MinPitch;
                            state = SGA_Animation_state.Raising;
                        }
                    }
                    break;
            }

            if (displacement > 0)
            {
                displacement -= 0.02f;
                if (displacement < 0)
                {
                    displacement = 0;
                }
            }
          
            Matrix m = Matrix.CreateTranslation(-displacement, 0, 0);
            m *= Matrix.CreateRotationZ(MathHelper.ToRadians(-angle));
            n.world = m * n.base_matrix;

            
        }
    }
}
