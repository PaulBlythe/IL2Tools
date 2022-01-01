using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;

namespace IL2Modder.IL2
{
    public enum CollisionNodeType
    {
        Sphere,
        Mesh,
        CollisionNodeTypes
    };
    public class CollisionNode:Node
    {
        public CollisionNodeType nType;
        public BoundingSphere Sphere;
        
        public CollisionNode(String data)
        {
            char [] seperators = new char[]{' ','\t'};
            string[] parts = data.Split(seperators,StringSplitOptions.RemoveEmptyEntries);
            if (parts[1].Equals("sphere"))
            {
                Sphere = new BoundingSphere();
                Sphere.Radius = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                if (parts.Length > 3)
                {
                    Sphere.Center = new Vector3(float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                                                float.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                                                float.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture));

                }
                else
                {
                    Sphere.Center = new Vector3(0, 0, 0);
                }

                nType = CollisionNodeType.Sphere;
            }
            else
            {
                nType = CollisionNodeType.Mesh;
                Name = parts[1];
            }
        }
        public void Serialise(TextWriter tw)
        {
            switch (nType)
            {
                case CollisionNodeType.Mesh:
                    {
                        tw.WriteLine(String.Format("CollisionObject {0}", Name));
                    }
                    break;
                case CollisionNodeType.Sphere:
                    {
                        tw.WriteLine(String.Format("CollisionObject sphere {0} {1} {2} {3}",
                                     Sphere.Radius,
                                     Sphere.Center.X,
                                     Sphere.Center.Y,
                                     Sphere.Center.Z));
                    }
                    break;
            }
        }
    }
}
