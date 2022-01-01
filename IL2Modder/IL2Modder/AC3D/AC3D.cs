using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using IL2Modder.IL2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using IL2Modder.Tools;

namespace IL2Modder.AC3D
{
    public class AC3DTriangle
    {
        public Vector3[] Position;
        public Vector2[] UV;
        public int Material;

        public AC3DTriangle()
        {
            Position = new Vector3[3];
            UV = new Vector2[3];
        }
    }
    
}
