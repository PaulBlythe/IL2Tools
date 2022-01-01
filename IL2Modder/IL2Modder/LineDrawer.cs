using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder
{
    public class LineDrawer
    {
        BasicEffect basicEffect;
        VertexPositionColor[] vertices;

        public LineDrawer(GraphicsDevice g)
        {
            basicEffect = new BasicEffect(g);
            basicEffect.VertexColorEnabled = true;
            
            vertices = new VertexPositionColor[2];
            vertices[0].Color = Color.Black;
            vertices[1].Color = Color.Black;
        }

        public void Draw(Matrix Projection, Matrix View, Vector3 start, Vector3 end, Color color)
        {
            basicEffect.Projection = Projection;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = View;
            vertices[0].Position = start;
            vertices[1].Position = end;
            vertices[0].Color = color;
            vertices[1].Color = color;
            basicEffect.CurrentTechnique.Passes[0].Apply();
            Form1.graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 1);
 
        }
    }
}
