using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IL2Modder.IL2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.Fox1Animators
{
    public class ShipTurretHeadAnimation
        :Animator
    {
        public float MinYaw = -90;
        public float MaxYaw = 90;
        public float Speed = 10;
        float direction = 1;
        float angle = 0;
        String parent;

        public ShipTurretHeadAnimation(String target)
        {
            parent = target;
        }

        public override void Update(HIM him, float dt)
        {
            angle += direction * Speed * dt;
            if (angle  >= MaxYaw)
            {
                angle = MaxYaw;
                direction *= -1;
            }
            if (angle <= MinYaw)
            {
                angle = MinYaw;
                direction *= -1;
            }
            Node n = him.FindNode(parent);

            Matrix m = Matrix.CreateRotationZ(MathHelper.ToRadians(angle));
            n.world = m * n.base_matrix;
        }
    }
}
