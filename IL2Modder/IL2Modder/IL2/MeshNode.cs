using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.IL2
{
    public class MeshNode:Node
    {
        public String filename;
        public String OriginalName;
        public Mesh mesh;
        public float Angle = 0;
        
        #region Constructors
        public MeshNode(String name)
        {
            Name = "Mesh_" + name;
            OriginalName = name;
            mesh = new Mesh();
            mesh.parent = this;
            originalHidden = false;
        }

        public MeshNode(MeshNode mn)
        {
            filename = mn.filename;
            OriginalName = mn.OriginalName;
            mesh = mn.mesh;
            Angle = mn.Angle;
            mesh.parent = this;
            Name = "pasted_" + mn.Name;
            Hidden = mn.Hidden;
            Seperable = mn.Seperable;
            world = Matrix.Identity * mn.world;
            Start = mn.Start;

            Type = mn.Type;
            foreach (Node n in mn.children)
            {
                children.Add(CopyNode(n));
            }
            foreach (CollisionNode b in mn.Colliders)
            {
                Colliders.Add(b);
            }
        }

        public MeshNode(String name, String dir)
        {
            Name = "Mesh_" + name;
            OriginalName = name;
            filename = dir + "/" + name + ".msh";

            mesh = new Mesh(filename);
            mesh.parent = this;
            if (name.Contains("CAP"))
                Hidden = true;
            if (name.EndsWith("_dmg"))
                Hidden = true;
            if (name.Contains("Damage"))
                Hidden = true;
            if (name.Length > 3)
            {
                if (name.ElementAt(name.Length - 2) == 'D')
                {
                    if (!Name.EndsWith("0"))
                        Hidden = true;
                }
            }
            originalHidden = Hidden;
        }
        #endregion

        #region Draw methods
        public void Draw(BasicEffect effect, float distance, Matrix World)
        {
            Matrix mv = world * World;
            effect.World = mv;

            if (!Hidden)
            {
                if (Name.Contains("PropRot"))
                {
                    mv = Matrix.CreateRotationZ(Angle) * mv;
                }
                mesh.Draw(effect, distance, mv);
            }

            foreach (Node n in children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.Draw(effect, distance, mv);
                }
            }
        }

        public void Draw(Effect effect, float distance, Matrix World, bool sort)
        {
            

            String test = Name.ToLower(); 
            Matrix mv = world * World;
           
            if (Form1.Animate)
            {
                Angle += 0.42f;
            }
            
            if (test.Contains("proprot"))
            {
                mv = Matrix.CreateRotationY(Angle) * mv;
                if (!Form1.Animate)
                    Hidden = true;
                else
                    Hidden = false;
            }
            else if (test.Contains("prop"))
            {
                if (Form1.Animate)
                    return;
            }
            
            if (test.Contains("rudder"))
            {
                mv = Matrix.CreateRotationY(Form1.Yaw) * mv;
            }
            
            if (!Hidden)
                mesh.Draw(effect, distance, mv, sort);
            
            foreach (Node n in children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.Draw(effect, distance, mv, sort);
                }
            }

            if (ObjectViewer.HookDisplay)
            {
                foreach (Hook h in mesh.Hooks)
                {
                    h.Draw(mv, ObjectViewer.mView, ObjectViewer.mProjection);
                }
            }
            previous_matrix = mv;
        }

        public void DrawGlass(Effect effect, float distance, Matrix World, Matrix vp)
        {
            Matrix mv = world * World;

            if (!Hidden)
                mesh.DrawGlass(effect, distance, mv, vp);

            foreach (Node n in children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.Draw(effect, distance, mv, false);
                }
            }
        }

        public void DrawShadow(Effect effect, float distance, Matrix World)
        {
            Matrix mv = world * World;

            if (!Hidden)
                mesh.DrawShadow(effect, distance, mv);

            foreach (Node n in children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawShadow(effect, distance, mv);
                }
            }
        }

        public void DrawCollisionMesh(BasicEffect be, Matrix World)
        {
            Matrix mv = world * World;
            if (!Hidden)
            {
                mesh.DrawCollisionMesh(be, mv);
            }
            foreach (Node n in children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawCollisionMesh(be, mv);
                }
            }
        }

        public void DrawSkin(Graphics g, Pen p, Brush b, String texture, float size)
        {
            mesh.DrawSkin(g, p, b, texture, size);
            foreach (Node n in children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawSkin(g, p, b, texture, size);
                }
            }
        }

        public void DrawSkinPart(Graphics g, Pen p, Brush b, String texture, float size)
        {
            mesh.DrawSkinPart(g, p, b, texture, size);
            
        }

        public void DrawAo(BasicEffect effect, int size, ref List<float> ao_values)
        {
            if (!Hidden)
            {
                mesh.DrawAo(effect, size, ref ao_values);
            }
        }

        public void DrawAo(BasicEffect effect, int size, String texture)
        {
            if (!Hidden)
            {
                mesh.DrawAo(effect,size,texture);
            }
        }

        public void DrawNormals(BasicEffect be, Matrix world)
        {
            if (!Hidden)
            {
                Matrix mv = this.world * world;
                be.World = mv;
                mesh.DrawNormals(be);
            }
        }

        public void DrawBumped(Effect effect, float distance, Matrix World, bool sort)
        {
            String test = Name.ToLower();

            Matrix mv = world * World;

            if (Form1.Animate)
            {
                Angle += 0.42f;
            }

            if (test.Contains("proprot"))
            {
                mv = Matrix.CreateRotationY(Angle) * mv;
                if (!Form1.Animate)
                    return;
            }
            else if (test.Contains("prop"))
            {
                if (Form1.Animate)
                    return;
            }

            if (!Hidden)
                mesh.DrawBumped(effect, distance, mv, sort);

            foreach (Node n in children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawBumped(effect, distance, mv, sort);
                }
            }
        }
        #endregion

        #region Search methods
        public void FindHook(String name, ref List<Vector3> hooks, Matrix pos)
        {         
            mesh.FindHook(name, ref hooks, world * pos);
        }
        public void FindHook(String name, ref List<Vector3> hooks,ref List<Vector3>directions, Matrix pos)
        {
            mesh.FindHook(name, ref hooks, ref directions, world * pos);
        }
        public Hook FindHook(String name)
        {
            return mesh.FindHook(name);
        }
        public void FindHook(String name, String ignore, ref List<Vector3> hooks, Matrix pos)
        {
            mesh.FindHook(name, ignore, ref hooks, world * pos);
        }
        public MeshNode Inside(float x, float y, String texture, float size)
        {
            if (mesh.Inside(x, y, texture, size))
            {
                return this;
            }
            foreach (Node n in children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (mn.Inside(x, y, texture, size) != null)
                    {
                        return mn;
                    }
                }
            }
            return null;
        }

        public void BuildAoList(Matrix world, ref List<Vector3> verts, ref List<Vector3> normals)
        {
            mesh.BuildAoList(world, ref verts, ref normals);
        }
        public void BuildAoListVertexCamera(String texture, Matrix world, ObjectViewer viewer)
        {
            mesh.BuildAoListVertexCamera(texture, this.world * world, viewer);
        }
        public void BuildAoListMultiVertexCamera(String texture, Matrix world, ObjectViewer viewer)
        {
            mesh.BakeAmbientMultiVertex(texture, this.world * world, viewer);
        }
        public void BuildAoListVertexRay(String texture, Matrix world, ObjectViewer viewer, int count)
        {
            mesh.BakeAmbientVertexRay(texture, this.world * world, viewer,count);
        }
        public bool Blocked(Ray r, Matrix world)
        {
            return false;
        }

        public Hook RaytraceHook(Ray r)
        {
            foreach (Hook h in mesh.Hooks)
            {
                if (h.Hit(r))
                {
                    return h;
                }
            }
            return null;
        }
        #endregion

        #region Serializers
        public void Serialize(TextWriter tw)
        {
            mesh.SaveHeader(tw);           
            mesh.SaveLodTable(tw);
            mesh.SaveHooks(tw);
            mesh.SaveMaterials(tw);
            mesh.SaveFaceGroups(tw);
            mesh.SaveFrame0(tw);
            mesh.SaveUVs(tw);
            mesh.SaveFaces(tw);
            mesh.SaveShadows(tw);
            mesh.SaveLods(tw);
            mesh.SaveCollisionMesh(tw);
        }

        public void SaveMaterials(String dir)
        {
            mesh.SaveMaterials(dir);
        }

        public void ExportCollision(String dir, Node n)
        {
            String path = Path.Combine(dir, n.Name);
            path += ".collision";
            mesh.ExportCollision(path);
        }

        public void ExportHooks(TextWriter t)
        {
            mesh.SaveHooks(t);
        }
        
        #endregion

        #region Tests
        public void Shoot(int x, int y, Matrix projection, Matrix view, Matrix World)
        {
            Matrix mv = world * World;
            if (mesh != null)
            {
                if (mesh.Shoot(x, y, projection, view, mv))
                {
                    Damage++;
                    if (Damage > 10)
                    {
                        String target = Name.TrimEnd('0');
                        target += "1";
                        MeshNode mn = ObjectViewer.Instance.FindMeshNode(target);
                        if (mn != null)
                        {
                            mn.Hidden = false;
                        } 
                    }
                    if (Damage > 30)
                    {
                        String target = Name.TrimEnd('0');
                        target += "2";
                        MeshNode mn = ObjectViewer.Instance.FindMeshNode(target);
                        if (mn != null)
                        {
                            mn.Hidden = false;
                        }
                    }
                    if (Damage > 40)
                    {
                        String target = Name.TrimEnd('0');
                        target += "3";
                        MeshNode mn = ObjectViewer.Instance.FindMeshNode(target);
                        if (mn != null)
                        {
                            mn.Hidden = false;
                        }
                    }
                    return;
                }
                foreach (Node n in children)
                {
                    if (n is MeshNode)
                    {
                        MeshNode mn = (MeshNode)n;
                        mn.Shoot(x, y, projection, view, mv);
                    }
                }
            }
        }
        public bool QuickCheck(Ray r, Matrix world)
        {
            return mesh.QuickCheck(world, r);
        }
        public bool QuickCheck(Ray r, Matrix world, float d)
        {
            return mesh.QuickCheck(world, r, d);
        }
        #endregion

        #region Modifiers
        public void MoveUV(float dx, float dy)
        {
            mesh.MoveUV(dx, dy);
        }
        public void FlipNormals()
        {
            FlipNormals fn = new FlipNormals(mesh);
            fn.Show();
        }
        public void ResetHidden()
        {
            Hidden = originalHidden;
        }
        public void RegenerateNormals()
        {
            mesh.RegenerateNormals();
        }
        public void SwapTriangles()
        {
            mesh.SwapTriangles();
        }
        public void AdjustLighting()
        {
            mesh.AdjustLighting();
        }

        
        #endregion

        #region Binormals and tangents
        public void GenerateBinormalsAndTangents()
        {
            mesh.GenerateBinormalsAndTangents();
        }

        public void EnableBumpMapping(int i, int t)
        {
            mesh.EnableBumpMapping(i, t);
        }
        #endregion

    }
}

