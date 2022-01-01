using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.Lighting
{
    public class LightingManager
    {
        public List<PointLight> pointLights = new List<PointLight>();
        public List<DirectionalLight> directionalLights = new List<DirectionalLight>();
        public List<SpotLight> spotLights = new List<SpotLight>();

        Effect lights;

        public LightingManager(Effect effect)
        {
            lights = effect;
        }

        public void AddPointLight(Vector3 position, Color colour)
        {
            pointLights.Add(new PointLight(position, colour));
        }

        public void AddDirectionalLight(Vector3 dir, Color col)
        {
            directionalLights.Add(new DirectionalLight(dir, col));
        }

        public void AddSpotLight(Vector3 pos, Vector3 dir, Color col)
        {
            spotLights.Add(new SpotLight(pos, dir, col));
        }

        public void Clear()
        {
            pointLights.Clear();
            directionalLights.Clear();
            spotLights.Clear();
        }
    }
}
