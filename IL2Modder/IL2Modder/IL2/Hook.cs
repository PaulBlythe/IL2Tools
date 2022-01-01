using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.IL2
{
    public class Hook
    {
        public String Name;
        public Matrix matrix;

        public BoundingSphere? bs;
        bool selected;

        public Hook(String name)
        {
            Name = name;
            matrix = Matrix.Identity;
            bs = null;
            selected = false;
        }
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            if (bs == null)
            {
                bs = new BoundingSphere(Vector3.Transform(Vector3.Zero, matrix * world), 0.1f);
            }
            if (ObjectViewer.ray != null)
            {
                selected = Hit((Ray)ObjectViewer.ray);
            }
            char [] sepe = new char[]{' ','\a','\t'};
            Matrix cur = Matrix.Identity;
            RasterizerState stat = new RasterizerState();
            stat.CullMode = CullMode.None;
            
            Form1.graphics.RasterizerState = stat;

            Model mod = ObjectViewer.cube2;
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[mod.Bones.Count];
            mod.CopyAbsoluteBoneTransformsTo(transforms);

            Vector3 scales = Vector3.Transform(Vector3.UnitZ, matrix * world);
            
            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in mod.Meshes)
            {
                cur = transforms[mesh.ParentBone.Index]
                        * Matrix.CreateScale(scales * 0.1f)
                        * matrix * world;
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (selected)
                        effect.DiffuseColor = Color.BlueViolet.ToVector3();
                    else
                        effect.DiffuseColor = Color.Yellow.ToVector3();
                    effect.World = cur;
                    effect.View = view;
                    effect.Projection = projection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            Vector3 wp = Vector3.Transform(Vector3.UnitZ,cur);
            Vector3 screenSpace = Form1.graphics.Viewport.Project(Vector3.Zero,projection,view,cur);           
            Vector2 textPosition = new Vector2(screenSpace.X, screenSpace.Y);

            String[] parts = Name.Split(sepe, StringSplitOptions.RemoveEmptyEntries);
            
            Vector2 size = ObjectViewer.font.MeasureString(parts[0]) * 0.5f;
            textPosition.X = (int)(textPosition.X - size.X);
            textPosition.Y = (int)(textPosition.Y - size.Y);

            
            ObjectViewer.batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.AnisotropicClamp,DepthStencilState.DepthRead,RasterizerState.CullNone);
            ObjectViewer.batch.DrawString(ObjectViewer.font, parts[0], textPosition, Color.Black);
            ObjectViewer.batch.End();
            Form1.graphics.DepthStencilState = DepthStencilState.Default;
        }

        public bool Hit(Ray ray)
        {
            float? test = ray.Intersects((BoundingSphere)bs);
            if (test != null)
            {
                return true;
            }

            return false;
        }
    }
}
