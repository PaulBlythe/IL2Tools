using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Skill.FbxSDK;

namespace IL2Modder.IL2
{
    public class Lod
    {
        public List<Material> Materials = new List<Material>();
        public List<FaceGroup> FaceGroups = new List<FaceGroup>();
        public List<VertexPositionColor> ShadowVerts = new List<VertexPositionColor>();
        public List<short> ShadowIndices = new List<short>();
        public List<List<VertexPositionNormalTexture>> animation_frames = new List<List<VertexPositionNormalTexture>>();
        public List<List<VertexPositionColor>> animation_shadow_frames = new List<List<VertexPositionColor>>();

        public short NumFrames;
        public int VertexCount;
        public int FaceCount;

        public VertexPositionNormalTexture[] Verts;
        public short[] Indices;
        public VertexPositionColor[] ShadowArray;
        public short[] ShadowIndicesArray;

        public bool Done;
        public bool Animated;
        public String continueance;

        #region Constructors
        public Lod()
        {
            Animated = false;
            NumFrames = 0;
        }
        public Lod(bool animated, short frames)
        {
            Animated = animated;
            NumFrames = frames;
            for (short i = 0; i < frames; i++)
            {
                animation_frames.Add(new List<VertexPositionNormalTexture>());
                animation_shadow_frames.Add(new List<VertexPositionColor>());
            }
        }

        public Lod(TextReader reader, String dir)
        {
            Done = false;
            continueance = "";
            ReadMaterials(reader, dir);
            ReadFaceGroups(reader);
            ReadVertices(reader);
            ReadUVs(reader);
            ReadFaces(reader);
            if (ReadShadowVerts(reader))
            {
                ReadShadowFaces(reader);
                ShadowArray = ShadowVerts.ToArray();
                ShadowIndicesArray = ShadowIndices.ToArray();
            }

        }
        #endregion

        #region Loaders
        private void ReadShadowFaces(TextReader reader)
        {
            string line;
            string[] parts;

            line = reader.ReadLine();
            while (!line.Contains("ShFaces"))
                line = reader.ReadLine();

            while (true)
            {
                line = reader.ReadLine();
                if (String.IsNullOrWhiteSpace(line))
                    return;
                parts = line.Split(' ');
                if ((line.Length == 0) || (line.StartsWith("//")))
                {
                    return;
                }
                ShadowIndices.Add(short.Parse(parts[0]));
                ShadowIndices.Add(short.Parse(parts[1]));
                ShadowIndices.Add(short.Parse(parts[2]));
            }
        }

        private bool ReadShadowVerts(TextReader reader)
        {
            string line;
            string[] parts;


            line = reader.ReadLine();
            if (line == null)
            {
                Done = true;
                return false;
            }


            if ((line.StartsWith("//")) || (line.Length < 1))
            {
                line = reader.ReadLine();
            }
            while (String.IsNullOrWhiteSpace(line))
            {
                line = reader.ReadLine();
                if (line == null)
                {
                    Done = true;
                    return false;
                }
            }

            if (line.StartsWith(";"))
            {
                Done = true;
                return false;
            }
            if (line.StartsWith("[CoCommon"))
            {
                //Done = true;
                continueance = line;
                return false;
            }
            if (!line.Contains("ShVertices"))       // no shadow verts for this lod
            {
                if (line.StartsWith("[LOD"))
                {
                    continueance = line;
                    return false;
                }
                return false;
            }
            while (true)
            {
                line = reader.ReadLine();
                line = line.Replace('\t', ' ');
                parts = line.Split(' ');
                if ((line.Length == 0) || (line.StartsWith("//")))
                {
                    return true;
                }
                Vector3 v = new Vector3(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                                        float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                                        float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                ShadowVerts.Add(new VertexPositionColor(v, Color.Black));
            }
        }

        private void ReadMaterials(TextReader reader, String dir)
        {
            while (true)
            {
                if (reader.Peek() == '[')
                    return;

                String line = reader.ReadLine();
                if (String.IsNullOrWhiteSpace(line))
                    return;

                String[] parts = line.Split(' ');
                Material m = new Material(parts[0], dir);
                Materials.Add(m);
            }
        }

        private void ReadFaceGroups(TextReader reader)
        {
            String line = "";
            char[] seperators = new char[] { ' ', '\t' };
            string[] parts;

            while (!line.StartsWith("[LOD"))
                line = reader.ReadLine();

            line = reader.ReadLine();

            parts = line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            VertexCount = int.Parse(parts[0]);
            FaceCount = int.Parse(parts[1]);
            Verts = new VertexPositionNormalTexture[VertexCount];
            Indices = new short[FaceCount * 3];

            while (true)
            {
                line = reader.ReadLine();
                if (line.Length < 2)
                    return;

                parts = line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

                FaceGroup f = new FaceGroup();
                f.Material = int.Parse(parts[0]);
                f.StartVertex = int.Parse(parts[1]);
                f.VertexCount = int.Parse(parts[2]);
                f.StartFace = int.Parse(parts[3]);
                f.FaceCount = int.Parse(parts[4]);
                FaceGroups.Add(f);

            }
        }

        private void ReadVertices(TextReader reader)
        {
            string line = null;
            string[] parts;
            char[] seps = new char[] { ' ', '\a', '\t' };

            while (String.IsNullOrWhiteSpace(line))
                line = reader.ReadLine();

            if (line.StartsWith("//"))
                line = reader.ReadLine();



            for (int i = 0; i < VertexCount; i++)
            {
                line = reader.ReadLine();
                if (line.Length < 1)
                    line = reader.ReadLine();
                if (line.StartsWith("//"))
                    line = reader.ReadLine();

                parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                Vector3 v = new Vector3(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                                        float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                                        float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                Vector3 n = new Vector3(float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                                        float.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                                        float.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture));
                VertexPositionNormalTexture vn = new VertexPositionNormalTexture(v, n, Vector2.Zero);
                Verts[i] = vn;
            }
            reader.ReadLine();
        }

        private void ReadUVs(TextReader reader)
        {
            string line;
            string[] parts;

            line = reader.ReadLine();
            while (!line.Contains("MaterialMapping]"))
                line = reader.ReadLine();


            for (int i = 0; i < VertexCount; i++)
            {
                line = reader.ReadLine();
                while (String.IsNullOrWhiteSpace(line))
                    line = reader.ReadLine();
                if (line.StartsWith("//"))
                    line = reader.ReadLine();

                parts = line.Split(' ');
                Vector2 v = new Vector2(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                                        float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
                Verts[i].TextureCoordinate = v;
            }
            reader.ReadLine();
        }

        private void ReadFaces(TextReader reader)
        {
            int i = 0;
            string line;
            string[] parts;

            line = reader.ReadLine();
            while (!line.Contains("Faces]"))
                line = reader.ReadLine();
            int j = 0;
            while (j < FaceCount)
            {
                line = reader.ReadLine();
                while (String.IsNullOrWhiteSpace(line))
                    line = reader.ReadLine();


                if (!line.StartsWith("//"))
                {
                    parts = line.Split(' ');
                    Indices[i++] = short.Parse(parts[0]);
                    Indices[i++] = short.Parse(parts[1]);
                    Indices[i++] = short.Parse(parts[2]);
                    j++;
                }
            }
            reader.ReadLine();
        }
        #endregion

        #region Draw methods
        public void Draw(Effect be, Matrix world, bool sort)
        {
            int i = 0;
            foreach (FaceGroup f in FaceGroups)
            {
                be.Parameters["World"].SetValue(world);
                be.Parameters["WorldInverseTranspose"].SetValue(Matrix.Invert(Matrix.Transpose(world)));
                Materials[i++].Apply(be, false);
                if (Materials[i - 1].Sort == sort)
                {
                    foreach (EffectPass pass in be.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        be.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                            PrimitiveType.TriangleList,
                            Verts,
                            f.StartVertex,                  // vertex buffer offset to add to each element of the index buffer
                            f.VertexCount,      // number of vertices to draw
                            Indices,
                            f.StartFace * 3,      // first index element to read
                            f.FaceCount         // number of primitives to draw
                            );
                    }
                }
            }
        }

        public void DrawShadow(Effect be, float distance, Matrix world)
        {
            if (ShadowVerts.Count == 0)
                return;

            be.Parameters["world"].SetValue(world);
            be.Parameters["WorldInverseTranspose"].SetValue(Matrix.Invert(Matrix.Transpose(world)));
            foreach (EffectPass pass in be.CurrentTechnique.Passes)
            {
                pass.Apply();
                be.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    ShadowArray,
                    0,                      // vertex buffer offset to add to each element of the index buffer
                    ShadowVerts.Count,      // number of vertices to draw
                    ShadowIndicesArray,
                    0,                      // first index element to read
                    ShadowIndices.Count / 3   // number of primitives to draw
                    );
            }


        }
        #endregion

        #region Serialisers
        public void SaveMaterials(String dir)
        {
            foreach (Material m in Materials)
            {
                m.Serialise(dir);
            }
        }
        #endregion

        #region FBX
        public FbxNode CreateFBXNode(FbxSdkManager sdkManager, String mesh_name, int level, String dir, Matrix rot)
        {
            String lod_name = String.Format("{0}_LOD{1}", mesh_name, level + 1);
            FbxNode node = FbxNode.Create(sdkManager, lod_name);


            int sc = FaceGroups[0].StartFace * 3;
            sc += FaceGroups[0].FaceCount * 3;
            if (sc > Indices.Length)
            {
                MessageBox.Show(String.Format("Error detected in LOD {0} of mesh {1} : Bad facegroup\n\nPlease report this to Stainless.", level, mesh_name), " IL2Modder FBX exporter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return node;
            }

            #region Create mesh and add verts
            FbxMesh mesh = FbxMesh.Create(sdkManager, lod_name);
            mesh.InitControlPoints(Verts.Length);

            FbxVector4[] fVerts = new FbxVector4[Verts.Length];
            for (int i = 0; i < Verts.Length; i++)
            {
                Vector3 vr = Vector3.Transform(Verts[i].Position, rot);
                fVerts[i] = new FbxVector4(vr.X, vr.Y, vr.Z);
            }
            mesh.ControlPoints = fVerts;
            #endregion

            #region Add normals
            // Set the normals on Layer 0.
            FbxLayer layer = mesh.GetLayer(0);
            if (layer == null)
            {
                mesh.CreateLayer();
                layer = mesh.GetLayer(0);
            }
            // We want to have one normal for each vertex (or control point),
            // so we set the mapping mode to eBY_CONTROL_POINT.
            FbxLayerElementNormal layerElementNormal = FbxLayerElementNormal.Create(mesh, "");

            layerElementNormal.Mapping_Mode = FbxLayerElement.MappingMode.ByControlPoint;
            layerElementNormal.Reference_Mode = FbxLayerElement.ReferenceMode.Direct;

            for (int i = 0; i < Verts.Length; i++)
            {
                FbxVector4 fn = new FbxVector4(Verts[i].Normal.X, Verts[i].Normal.Y, Verts[i].Normal.Z);
                layerElementNormal.DirectArray.Add(fn);
            }
            layer.Normals = layerElementNormal;

            #endregion

            #region Add UV coords
            // Create UV for Diffuse channel
            FbxLayerElementUV UVDiffuseLayer = FbxLayerElementUV.Create(mesh, "DiffuseUV");
            UVDiffuseLayer.Mapping_Mode = FbxLayerElement.MappingMode.ByPolygonVertex;
            UVDiffuseLayer.Reference_Mode = FbxLayerElement.ReferenceMode.IndexToDirect;
            for (int i = 0; i < Verts.Length; i++)
            {
                FbxVector2 fu = new FbxVector2(Verts[i].TextureCoordinate.X, Verts[i].TextureCoordinate.Y);
                UVDiffuseLayer.DirectArray.Add(fu);
            }
            layer.SetUVs(UVDiffuseLayer, FbxLayerElement.LayerElementType.DiffuseTextures);
            #endregion

            #region Materials
            // Set material mapping.
            FbxLayerElementMaterial materialLayer = FbxLayerElementMaterial.Create(mesh, "");
            materialLayer.Mapping_Mode = FbxLayerElement.MappingMode.ByPolygon;
            materialLayer.Reference_Mode = FbxLayerElement.ReferenceMode.IndexToDirect;
            layer.Materials = materialLayer;

            FbxLayerElementTexture textureDiffuseLayer = FbxLayerElementTexture.Create(mesh, "Diffuse Texture");
            textureDiffuseLayer.Mapping_Mode = FbxLayerElement.MappingMode.ByPolygon;
            textureDiffuseLayer.Reference_Mode = FbxLayerElement.ReferenceMode.IndexToDirect;
            layer.DiffuseTextures = textureDiffuseLayer;


            foreach (Material m in Materials)
            {
                FbxTexture texture = FbxTexture.Create(sdkManager, "DiffuseTexture" + m.Name);
                String np = Path.Combine(dir, Path.GetFileNameWithoutExtension(m.tname));
                np += ".png";
                // Set texture properties.
                texture.SetFileName(np); // Resource file is in current directory.
                texture.TextureUseType = FbxTexture.TextureUse.Standard;
                texture.Mapping = FbxTexture.MappingType.Uv;
                texture.MaterialUseType = FbxTexture.MaterialUse.Model;
                texture.SwapUV = false;
                texture.SetTranslation(0.0, 0.0);
                texture.SetScale(1.0, 1.0);
                texture.SetRotation(0.0, 0.0);
                texture.AlphaSrc = FbxTexture.AlphaSource.RgbIntensity;
                texture.DefaultAlpha = 0;
                texture.SetWrapMode(FbxTexture.WrapMode.Repeat, FbxTexture.WrapMode.Repeat);
                layer.DiffuseTextures.DirectArray.Add(texture);

                FbxSurfacePhong material = FbxSurfacePhong.Create(sdkManager, m.Name);

                // Generate primary and secondary colors.
                material.EmissiveColor = new FbxDouble3(0.0, 0.0, 0.0);
                material.AmbientColor = new FbxDouble3(m.Ambient, m.Ambient, m.Ambient);
                material.DiffuseColor = new FbxDouble3(m.Diffuse, m.Diffuse, m.Diffuse);
                material.SpecularColor = new FbxDouble3(m.Specular, m.Specular, m.Specular);
                material.TransparencyFactor = m.AlphaTestVal;
                material.Shininess = m.SpecularPow;
                material.ShadingModel = "phong";


                layer.Materials.DirectArray.Add(material);
            }
            #endregion

            int count = 0;
            int group = 0;
            foreach (FaceGroup f in FaceGroups)
            {
                int s = f.StartFace * 3;
                for (int i = 0; i < f.FaceCount; i++)
                {
                    mesh.BeginPolygon(f.Material, f.Material, group, true);
                    mesh.AddPolygon(Indices[s] + f.StartVertex, Indices[s] + f.StartVertex);
                    layer.DiffuseTextures.IndexArray.SetAt(s, f.Material);
                    layer.Materials.IndexArray.SetAt(s, f.Material);
                    s++;

                    mesh.AddPolygon(Indices[s] + f.StartVertex, Indices[s] + f.StartVertex);
                    layer.DiffuseTextures.IndexArray.SetAt(s, f.Material);
                    layer.Materials.IndexArray.SetAt(s, f.Material);

                    s++;
                    mesh.AddPolygon(Indices[s] + f.StartVertex, Indices[s] + f.StartVertex);
                    layer.DiffuseTextures.IndexArray.SetAt(s, f.Material);
                    layer.Materials.IndexArray.SetAt(s, f.Material);
                    s++;
                    mesh.EndPolygon();

                    count += 3;
                }
                group++;
            }

            UVDiffuseLayer.IndexArray.Count = count;
            node.NodeAttribute = mesh;
            node.Shading_Mode = FbxNode.ShadingMode.TextureShading;

            return node;
        }

        public FbxNode AddShadow(FbxSdkManager sdkManager, String mesh_name, int level, String dir, Matrix rot)
        {
            if (ShadowArray == null)
                return null;

            String lod_name = String.Format("{0}_LOD{1}_SHADOW", mesh_name, level + 1);
            FbxNode node = FbxNode.Create(sdkManager, lod_name);

            #region Create mesh and add verts
            FbxMesh smesh = FbxMesh.Create(sdkManager, lod_name);
            smesh.InitControlPoints(ShadowArray.Length);

            FbxVector4[] sVerts = new FbxVector4[ShadowArray.Length];
            for (int i = 0; i < ShadowArray.Length; i++)
            {
                Vector3 vr = Vector3.Transform(ShadowArray[i].Position, rot);
                sVerts[i] = new FbxVector4(vr.X, vr.Y, vr.Z);
            }

            smesh.ControlPoints = sVerts;
            #endregion

            #region Add triangles
            for (int j = 0; j < ShadowIndicesArray.Length; j += 3)
            {
                smesh.BeginPolygon(-1, -1, 0, true);
                smesh.AddPolygon(ShadowIndicesArray[j], -1);
                smesh.AddPolygon(ShadowIndicesArray[j + 1], -1);
                smesh.AddPolygon(ShadowIndicesArray[j + 2], -1);
                smesh.EndPolygon();
            }
            #endregion

            node.NodeAttribute = smesh;
            node.Shading_Mode = FbxNode.ShadingMode.WireFrame;
            return node;
        }

        #endregion

    }
}
