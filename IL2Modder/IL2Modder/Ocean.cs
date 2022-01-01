using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder
{
    /// <summary>
    /// A class to draw an ocean
    /// </summary>
    class Ocean
    {
        // the ocean's required content
        public Effect oceanEffect;
        public Texture2D[] OceanNormalMaps = new Texture2D[4];

        // The two-triangle generated model for the ocean
        private VertexPositionNormalTexture[] OceanVerts;
        
        /// <summary>
        /// Creates an Ocean object
        /// </summary>
        public Ocean()
        {
            // generate the geometry
            OceanVerts = new VertexPositionNormalTexture[6];
            OceanVerts[0] = new VertexPositionNormalTexture(new Vector3(-1000, 0, -1000), Vector3.Up, new Vector2(0, 0));
            OceanVerts[1] = new VertexPositionNormalTexture(new Vector3( 1000, 0, -1000), Vector3.Up, new Vector2(1, 0));
            OceanVerts[2] = new VertexPositionNormalTexture(new Vector3(-1000, 0,  1000), Vector3.Up, new Vector2(0, 1));
            OceanVerts[3] = OceanVerts[2];
            OceanVerts[4] = OceanVerts[1];
            OceanVerts[5] = new VertexPositionNormalTexture(new Vector3( 1000, 0,  1000), Vector3.Up, new Vector2(1, 1));
           
        }

        public void Draw(float gameTime, Vector3 cam, Matrix view , TextureCube skyTexture, Matrix proj, bool ship)
        {
            oceanEffect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // set the transforms
            if (ship)
                oceanEffect.Parameters["World"].SetValue(Matrix.CreateRotationX(MathHelper.ToRadians(90)));
            else
                oceanEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(Vector3.UnitY * -20.0f) * Matrix.CreateRotationX(MathHelper.ToRadians(90)));
            oceanEffect.Parameters["View"].SetValue(view);
            oceanEffect.Parameters["Projection"].SetValue(proj);
            oceanEffect.Parameters["EyePos"].SetValue(cam);

            // choose and set the ocean textures
            int oceanTexIndex = ((int)(gameTime) % 4);
            oceanEffect.Parameters["normalTex"].SetValue(OceanNormalMaps[(oceanTexIndex + 1) % 4]);
            oceanEffect.Parameters["normalTex2"].SetValue(OceanNormalMaps[(oceanTexIndex) % 4]);
            oceanEffect.Parameters["textureLerp"].SetValue((((((float)gameTime) - (int)(gameTime)) * 2 - 1) * 0.5f) + 0.5f);

            // set the time used for moving waves
            oceanEffect.Parameters["time"].SetValue((float)gameTime * 0.02f);

            // set the sky texture
            oceanEffect.Parameters["cubeTex"].SetValue(skyTexture);



            foreach (EffectPass pass in oceanEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Form1.graphics.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, OceanVerts, 0, 2);

            }
        }
    }
}
