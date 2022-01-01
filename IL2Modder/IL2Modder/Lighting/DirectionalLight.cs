using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.Lighting
{
    public class DirectionalLight
    {
        public Vector3 Direction;
        public Color Colour;

        public DirectionalLight(Vector3 direction, Color col)
        {
            Direction = direction;
            Colour = col;
        }
    }
}
