using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.Lighting
{
    public class PointLight
    {
        public Vector3 Position;
        public Color Color;

        public PointLight(Vector3 pos, Color col)
        {
            Position = pos;
            Color = col;
        }
    }
}
