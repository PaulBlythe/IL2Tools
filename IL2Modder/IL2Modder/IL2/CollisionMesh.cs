using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Skill.FbxSDK;

namespace IL2Modder.IL2
{
    public class CollisionMeshPart
    {
        public String Type;
        public String Name;
        public int NFrames;
        public List<Vector3> Verts = new List<Vector3>();
        public List<int> NeiCount = new List<int>();
        public List<int> Neighbours = new List<int>();
        public List<short> Faces = new List<short>();
        public int TypeIntExt;

        public short[] indices;
        public VertexPositionColor[] verts;

        public CollisionMeshPart()
        {
            Type = "";
            NFrames = 0;
            TypeIntExt = 0;
        }

        public FbxNode AddCollisionMeshPart(String basename, FbxSdkManager sdkManager, Matrix rot)
        {
            FbxMesh smesh;

            FbxNode node = FbxNode.Create(sdkManager, basename + "_" + Name);
            node.Shading_Mode = FbxNode.ShadingMode.WireFrame;

            if (Form1.Instance.fbx_meshes.ContainsKey(Name))
            {
                smesh = Form1.Instance.fbx_meshes[Name];
            }
            else
            {
                smesh = FbxMesh.Create(sdkManager, Name);
                smesh.InitControlPoints(Verts.Count);

                FbxVector4[] sVerts = new FbxVector4[Verts.Count];
                for (int i = 0; i < Verts.Count; i++)
                {
                    Vector3 vr = Vector3.Transform(Verts[i], rot);
                    sVerts[i] = new FbxVector4(vr.X, vr.Y, vr.Z);
                }

                smesh.ControlPoints = sVerts;

                for (int j = 0; j < Faces.Count; j += 3)
                {
                    smesh.BeginPolygon(-1, -1, 0, true);
                    smesh.AddPolygon(Faces[j], -1);
                    smesh.AddPolygon(Faces[j + 1], -1);
                    smesh.AddPolygon(Faces[j + 2], -1);
                    smesh.EndPolygon();
                }
                Form1.Instance.fbx_meshes.Add(Name, smesh);
            }
            node.NodeAttribute = smesh;
            return node;
        }

        public void SaveToFox1(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Verts.Count);
            foreach (Vector3 v in Verts)
            {
                bw.Write(v.X);
                bw.Write(v.Y);
                bw.Write(v.Z);
            }
            bw.Write(Faces.Count);
            foreach (short s in Faces)
            {
                bw.Write(s);
            }
        }

        public void AddVertex(double x, double y, double z)
        {
            Vector3 v = new Vector3((float)x, (float)y, (float)z);
            Verts.Add(v);

        }
    }
    public class CollisionMeshBlock
    {
        public int NParts;
        public List<CollisionMeshPart> Parts = new List<CollisionMeshPart>();

        public void SaveToFox1(BinaryWriter bw)
        {
            bw.Write(NParts);
            foreach (CollisionMeshPart cmp in Parts)
            {
                cmp.SaveToFox1(bw);
            }
        }
    }
    public class CollisionMesh
    {
        public int NBlocks;
        public List<CollisionMeshBlock> Blocks = new List<CollisionMeshBlock>();
        //public int NParts;
        //public List<CollisionMeshPart> Parts = new List<CollisionMeshPart>();

        public int CurrentBlock;
        public int CurrentPart;

        public CollisionMesh()
        {
            NBlocks = 0;
            CurrentBlock = 0;
            //NParts = 0;
            CurrentPart = 0;
            
        }
    }
}
