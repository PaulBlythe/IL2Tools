using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.Lighting
{
    public class SpotLight
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Color Colour;

        public SpotLight(Vector3 pos, Vector3 dir, Color col)
        {
            Position = pos;
            Direction = dir;
            Colour = col;
        }

    }
}
