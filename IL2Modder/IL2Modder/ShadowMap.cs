using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace IL2Modder
{
    public class ShadowMap
    {
        #region Constants
        private const int DEFAULT_SHADOWMAP_SIZE = 1024;
        private const float DEFAULT_DEPTH_BIAS = 0.1f;
        #endregion

        #region Fields
        private int size;
        public float depthBias;
        private Vector3 lightDir;
        public Matrix lightViewMatrix;
        public Matrix lightProjectionMatrix;
        public Matrix lightViewProjectionMatrix;
        public Matrix textureMatrix;
        private RenderTarget2D renderTarget;
        public Texture2D shadowMapTexture;

        public Effect effect;
        #endregion

        public ShadowMap(GraphicsDevice device)
        {
            size = DEFAULT_SHADOWMAP_SIZE;
            depthBias = DEFAULT_DEPTH_BIAS;
            lightDir = Vector3.Forward;
            lightViewMatrix = Matrix.Identity;
            lightProjectionMatrix = Matrix.Identity;
            lightViewProjectionMatrix = Matrix.Identity;

            renderTarget = new RenderTarget2D(device, size, size, false,SurfaceFormat.Color,DepthFormat.None);

            CreateTextureScaleBiasMatrix();
        }
        public float TexelSize
        {
            get { return 1.0f / size; }
        }
        public void Begin()
        {
            
            Form1.graphics.SetRenderTarget(renderTarget);
            Form1.graphics.Clear(Color.Transparent);
        }
        public void Update(Vector3 lightDir, Matrix viewProjection, Effect e)
        {
            lightViewMatrix = Matrix.CreateLookAt(lightDir*-50.0f, Vector3.Zero, Vector3.Up);
            lightProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(1, 1.0f, 0.1f, 1500.0f);

            //// 1. Calculate the world space location of the 8 corners of the
            //// view frustum.

            //BoundingFrustum frustum = new BoundingFrustum(viewProjection);
            //Vector3[] frustumCornersWorldSpace = frustum.GetCorners();

            //// 2. Calculate the centroid of the frustum.

            //Vector3 centroid = Vector3.Zero;

            //foreach (Vector3 frustumCorner in frustumCornersWorldSpace)
            //    centroid += frustumCorner;

            //centroid /= (float)frustumCornersWorldSpace.Length;

            //// 3. Calculate the position of the direction light.
            //// Start at the centroid and then move back in the opposite
            //// direction of the light by an amount equal to the camera's far
            //// clip plane. This is the position of the light.

            //float distance = Math.Abs(frustum.Near.D) + Math.Abs(frustum.Far.D);

            ////Vector3 Right = Vector3.Normalize(Vector3.Cross(lightDir, Vector3.Up));
            ////Vector3 Up = Vector3.Cross(Right, lightDir);
            //lightViewMatrix = Matrix.CreateLookAt(centroid - (lightDir * distance), centroid, Vector3.Up);

            //// 4. Calculate the light space locations of the 8 corners of the
            //// (world space) view frustum. The lightViewMatrix is used to
            //// transform each world space frustum corner into light space.

            //Vector3[] frustumCornersLightSpace = new Vector3[frustumCornersWorldSpace.Length];
            //Vector3.Transform(frustumCornersWorldSpace, ref lightViewMatrix, frustumCornersLightSpace);

            //// 5. Calculate the bounding box for the light space frustum
            //// corners. The bounding box is used to construct the proper
            //// orthographic projection matrix for the directional light.

            //BoundingBox box = BoundingBox.CreateFromPoints(frustumCornersLightSpace);
            //lightProjectionMatrix = Matrix.CreateOrthographicOffCenter(
            //    box.Min.X, box.Max.X, box.Min.Y, box.Max.Y, -box.Max.Z, -box.Min.Z);

            lightViewProjectionMatrix = lightViewMatrix * lightProjectionMatrix;

            e.CurrentTechnique = e.Techniques["CreateShadowMap"];
            e.Parameters["lightViewProjection"].SetValue(lightViewProjectionMatrix);
        }
        public void End()
        {
            Form1.graphics.SetRenderTarget(null);
            shadowMapTexture = (Texture2D)renderTarget;
        }
        private void CreateTextureScaleBiasMatrix()
        {
            float offset = 0.5f + (0.5f / (float)size);

            textureMatrix = new Matrix(0.5f, 0.0f, 0.0f, 0.0f,
                                       0.0f, -0.5f, 0.0f, 0.0f,
                                       0.0f, 0.0f, 0.0f, 0.0f,
                                       offset, offset, 0.0f, 1.0f);
        }
    }
}
