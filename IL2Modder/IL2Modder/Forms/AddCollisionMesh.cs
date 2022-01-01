using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MeshDecimator;
using MeshDecimator.Math;

using IL2Modder.IL2;
using System.Windows.Media.Media3D;

namespace IL2Modder.Forms
{
    public partial class AddCollisionMesh : Form
    {
        MeshNode target;
        MeshDecimator.Mesh dec;
        MeshDecimator.Mesh orig;

        Vector3d[] vertices;
        int[][] indices;
        Bitmap b;
        float scale = 10.0f;
        float quality = 1;
        float newquality = 1;
        int origtriangles;
        Vector3d centre;


        public AddCollisionMesh(MeshNode mn)
        {
            InitializeComponent();

            target = mn;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            CollisionMeshPart cmp = new CollisionMeshPart();
            cmp.Name = textBox4.Text;
            cmp.NFrames = 1;
            foreach (var vec in dec.Vertices)
            {
                cmp.AddVertex(vec.x, vec.y, vec.z);
            }
            
            for (int i = 0; i < target.mesh.FaceGroups.Count; i++)
            {
                if (IsValid(i))
                {
                    int[] inds = dec.GetIndices(i);

                    for (int j=0; j<inds.Length; j++)
                    {
                        cmp.Faces.Add((short)inds[j]);
                        

                    }
                    for (int j=0; j<cmp.Verts.Count; j++)
                    {
                        List<int> neighbours = new List<int>();
                        // loop over all the faces in the mesh

                        for (int k = 0; k < inds.Length; k += 3)
                        {
                            int v1 = inds[k];
                            int v2 = inds[k + 1];
                            int v3 = inds[k + 2];

                            // does this face include our vertex
                            if ((v1 == j) || (v2 == j) || (v3 == j))
                            {
                                if ((v1 != j) && (!neighbours.Contains(v1)))
                                    neighbours.Add(v1);
                                if ((v2 != j) && (!neighbours.Contains(v2)))
                                    neighbours.Add(v2);
                                if ((v3 != j) && (!neighbours.Contains(v3)))
                                    neighbours.Add(v3);
                            }
                        }

                        cmp.NeiCount.Add(neighbours.Count);
                        for (int k = 0; k < neighbours.Count; k++)
                        {
                            cmp.Neighbours.Add(neighbours[k]);
                        }
                    }


                }
            }
            if (target.mesh.colmesh.Blocks.Count >0)
            {
                target.mesh.colmesh.Blocks[0].Parts.Add(cmp);
                target.mesh.colmesh.Blocks[0].NParts++;
            }
            else
            {
                CollisionMeshBlock cmb = new CollisionMeshBlock();
                cmb.Parts.Add(cmp);
                cmb.NParts = 1;
                target.mesh.colmesh.Blocks.Add(cmb);
                target.mesh.colmesh.NBlocks++;
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void AddCollisionMesh_Load(object sender, EventArgs e)
        {
            b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = b;

            vertices = new Vector3d[target.mesh.Verts.Length];
            double cx, cy, cz;
            cx = cy = cz = 0;
            for (int i = 0; i < target.mesh.Verts.Length; i++)
            {
                vertices[i] = new Vector3d(target.mesh.Verts[i].Position.X, target.mesh.Verts[i].Position.Y, target.mesh.Verts[i].Position.Z);
                cx += target.mesh.Verts[i].Position.X;
                cy += target.mesh.Verts[i].Position.Y;
                cz += target.mesh.Verts[i].Position.Z;

            }
            cx /= target.mesh.Verts.Length;
            cy /= target.mesh.Verts.Length;
            cz /= target.mesh.Verts.Length;

            centre = new Vector3d(cx, cy, cz);

            List<int[]> submeshes = new List<int[]>();

            for (int i = 0; i < target.mesh.FaceGroups.Count; i++)
            {
                List<int> indices = new List<int>();
                FaceGroup f = target.mesh.FaceGroups[i];
                int si = f.StartFace * 3;
                for (int j = 0; j < f.FaceCount; j++)
                {
                    indices.Add(target.mesh.indices[si++]);
                    indices.Add(target.mesh.indices[si++]);
                    indices.Add(target.mesh.indices[si++]);
                }
                checkedListBox1.Items.Add(target.mesh.Materials[f.Material].Name,true);
                submeshes.Add(indices.ToArray());
            }
            indices = submeshes.ToArray();
            orig = new MeshDecimator.Mesh(vertices, indices);
            int count = 0;
            for (int i = 0; i < target.mesh.FaceGroups.Count; i++)
            {
                count += orig.GetTriangleCount(i);
            }
            

            dec = MeshDecimation.DecimateMesh(Algorithm.FastQuadricMesh, orig, count);
            Redraw();

            textBox1.Text = count.ToString();
            textBox2.Text = target.mesh.FaceGroups.Count.ToString();
            textBox3.Text = count.ToString();

            origtriangles = count;

            Timer timer = new Timer();
            timer.Interval = (100); 
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
            Application.DoEvents();
        }

        private void Redraw()
        {
            if (quality != newquality)
            {
                quality = newquality;
                int targetc = (int)(origtriangles * quality);
                dec = MeshDecimation.DecimateMesh(Algorithm.FastQuadricMesh, orig, targetc);

                int count = 0;
                for (int i = 0; i < target.mesh.FaceGroups.Count; i++)
                {
                    if (IsValid(i))
                    {
                        count += dec.GetTriangleCount(i);
                    }
                }
                textBox3.Text = count.ToString();
            }

            int w = pictureBox1.Width;
            int h = pictureBox1.Height;

            if ((w != b.Width) || (h != b.Width))
            {
                b.Dispose();
                b = new Bitmap(w, h);
                pictureBox1.Image = b;
            }

            Graphics g = Graphics.FromImage(b);

            g.Clear(Color.CornflowerBlue);

            int cx = w / 4;
            int cy = h / 4;
            Draw(g, cx, cy, 1);

            cx = (3 * w) / 4;
            cy = h / 4;
            Draw(g, cx, cy, 0);

            cy = (3 * h) / 4;
            Draw(g, cx, cy, 2);

            cx = w / 4;
            Draw(g, cx, cy, 3);


            g.Dispose();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Redraw();
        }

        private void Draw(Graphics g, int cx, int cy, int axis)
        {
            List<PointF> points = new List<PointF>();
            Vector3d[] verts = dec.Vertices;
            switch (axis)
            {
                case 0:
                    {
                        for (int i = 0; i < verts.Length; i++)
                        {
                            float x = (float)((verts[i].y - centre.y) * scale);
                            float y = (float)((verts[i].z - centre.z) * scale);
                            PointF p = new PointF(cx + x, cy - y);
                            points.Add(p);
                        }
                    }
                    break;
                case 1:
                    {
                        for (int i = 0; i < verts.Length; i++)
                        {
                            float x = (float)((verts[i].x - centre.x) * scale);
                            float y = (float)((verts[i].z - centre.z) * scale);
                            PointF p = new PointF(cx + x, cy - y);
                            points.Add(p);
                        }
                    }
                    break;
                case 2:
                    {
                        for (int i = 0; i < verts.Length; i++)
                        {
                            float x = (float)((verts[i].y - centre.y) * scale);
                            float y = (float)((verts[i].x - centre.x) * scale);
                            PointF p = new PointF(cx + x, cy - y);
                            points.Add(p);
                        }
                    }
                    break;

                case 3:
                    {
                        for (int i = 0; i < verts.Length; i++)
                        {
                            float x = (float)((verts[i].x - centre.x)* scale);
                            float y = (float)((verts[i].y - centre.y)* scale);
                            float z = (float)((verts[i].z - centre.z)* scale);

                            float x1 = (y / 2) + (x / 2);
                            float y1 = (y / 2) - (z / 2);

                            PointF p = new PointF(cx + x1, cy + y1);
                            points.Add(p);
                        }
                    }
                    break;
            }
            int[][] inds = dec.GetSubMeshIndices();

            for (int i = 0; i < dec.SubMeshCount; i++)
            {
                if (IsValid(i))
                {
                    Pen p = Pens.Black;
                    switch (i & 3)
                    {
                        case 0:
                            p = Pens.Red;
                            break;
                        case 1:
                            p = Pens.Blue;
                            break;
                        case 2:
                            p = Pens.Green;
                            break;
                        case 3:
                            p = Pens.Yellow;
                            break;


                    }
                    int j = 0;
                    while (j < inds[i].Length)
                    {
                        g.DrawLine(p, points[inds[i][j]], points[inds[i][j + 1]]);
                        g.DrawLine(p, points[inds[i][j + 2]], points[inds[i][j + 1]]);
                        g.DrawLine(p, points[inds[i][j]], points[inds[i][j + 2]]);
                        j += 3;
                    }
                }
            }
            points.Clear();

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            scale = (float) numericUpDown2.Value;

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            newquality = (float)numericUpDown1.Value;
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private bool IsValid(int i)
        {
            String name = target.mesh.Materials[target.mesh.FaceGroups[i].Material].Name;
            return checkedListBox1.CheckedItems.Contains(name);
        }
    }
}
