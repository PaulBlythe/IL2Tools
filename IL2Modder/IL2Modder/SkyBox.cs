﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace IL2Modder
{
    /// <summary>
    /// Handles all of the aspects of working with a skybox.
    /// </summary>
    public class Skybox
    {
        /// <summary>
        /// The skybox model, which will just be a cube
        /// </summary>
        public Model skyBox;

        /// <summary>
        /// The actual skybox texture
        /// </summary>
        public TextureCube skyBoxTexture;

        /// <summary>
        /// The effect file that the skybox will use to render
        /// </summary>
        public Effect skyBoxEffect;

        /// <summary>
        /// The size of the cube, used so that we can resize the box
        /// for different sized environments.
        /// </summary>
        private float size = 1000f;

        public float rotation_x = 0;
        public float rotation_y = 0;
        public float rotation_z = 0;

        /// <summary>
        /// Creates a new skybox
        /// </summary>
        /// <param name="skyboxTexture">the name of the skybox texture to use</param>
        public Skybox()
        {

        }

        /// <summary>
        /// Does the actual drawing of the skybox with our skybox effect.
        /// There is no world matrix, because we're assuming the skybox won't
        /// be moved around.  The size of the skybox can be changed with the size
        /// variable.
        /// </summary>
        /// <param name="view">The view matrix for the effect</param>
        /// <param name="projection">The projection matrix for the effect</param>
        /// <param name="cameraPosition">The position of the camera</param>
        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            RasterizerState stat = new RasterizerState();
            stat.CullMode = CullMode.None;
            Form1.graphics.RasterizerState = stat;

            //Form1.graphics.BlendState = BlendState.NonPremultiplied;

            // Go through each pass in the effect, but we know there is only one...
            foreach (EffectPass pass in skyBoxEffect.CurrentTechnique.Passes)
            {
                // Draw all of the components of the mesh, but we know the cube really
                // only has one mesh
                foreach (ModelMesh mesh in skyBox.Meshes)
                {
                    // Assign the appropriate values to each of the parameters
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = skyBoxEffect;
                        part.Effect.Parameters["World"].SetValue(
                            Matrix.CreateScale(size) * 
                            Matrix.CreateRotationZ(MathHelper.ToRadians(rotation_z)) *
                            Matrix.CreateRotationY(MathHelper.ToRadians(rotation_y)) *
                            Matrix.CreateRotationX(MathHelper.ToRadians(rotation_x)) * 
                            Matrix.CreateTranslation(cameraPosition));
                        part.Effect.Parameters["View"].SetValue(view);
                        part.Effect.Parameters["Projection"].SetValue(projection);
                        part.Effect.Parameters["SkyBoxTexture"].SetValue(skyBoxTexture);
                        part.Effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                    }

                    // Draw the mesh with the skybox effect
                    mesh.Draw();
                }
            }
        }
    }
}
