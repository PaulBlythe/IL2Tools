using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;
using System.Diagnostics;

using Skill.FbxSDK;

namespace IL2Modder.IL2
{
    enum MeshLoaderMode
    {
        Start,
        Common,
        Lod,
        Hooks,
        Hookloc,
        Materials,
        FaceGroups,
        Vertices,
        UVS,
        Faces,
        ShadowVerts,
        ShadowFaces,
        LodMesh,
        CoCommon,
        CoCommonPart,
        CoCommonType,
        CoVer0,
        CoNeiCnt,
        CoNei,
        CoFaces,
        Done,
        MeshLoaderModes
    };

    public struct BinaryHeader
    {
        public Int32 magic;
        public Int32 filesize;
        public Int32 sections;
        public Int32 unknown;
        public Int32 table1;
        public Int32 table2;
        public Int32 table3;
    };

    public struct BumpVertex : IVertexType
    {
        Vector3 vertexPosition;
        Vector2 vertexTextureCoordinate;
        Vector3 vertexNormal;
        Vector3 vertexTangent;
        Vector3 vertexBiNormal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(44, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0)

        );

        public BumpVertex(Vector3 pos, Vector2 textureCoordinate, Vector3 normal, Vector3 tangent, Vector3 binormal)
        {
            vertexPosition = pos;
            vertexTextureCoordinate = textureCoordinate;
            vertexNormal = normal;
            vertexTangent = tangent;
            vertexBiNormal = binormal;
        }

        //Public methods for accessing the components of the custom vertex.
        public Vector3 Position
        {
            get { return vertexPosition; }
            set { vertexPosition = value; }
        }

        public Vector2 TextureCoordinate
        {
            get { return vertexTextureCoordinate; }
            set { vertexTextureCoordinate = value; }
        }

        public Vector3 Normal
        {
            get { return vertexNormal; }
            set { vertexNormal = value; }
        }

        public Vector3 Tangent
        {
            get { return vertexTangent; }
            set { vertexTangent = value; }
        }

        public Vector3 BiNormal
        {
            get { return vertexBiNormal; }
            set { vertexBiNormal = value; }
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    };

    public struct T1Entry
    {
        public Int32 Offset;
        public Int32 StartRecord;
        public Int16 NumberOfRecords;
        public Int16 Type;
    };

    public class BinaryFile
    {
        public BinaryHeader header = new BinaryHeader();
        public List<T1Entry> table1 = new List<T1Entry>();
        public List<String> sections = new List<string>();
        public List<Int32> subsection_size = new List<Int32>();
        public Int32 data_start;
        public List<FaceGroup> facegroups = new List<FaceGroup>();

    };

    public class Mesh
    {
        public Node parent;
        public bool Binary;
        public bool Animated = false;
        public int VertexCount;
        public int FaceCount;
        public int CurrentAnimationFrame = -1;
        public float AnimationTime = 0;
 

        #region Animation frame storage
        public List<List<VertexPositionNormalTexture>> animation_frames = new List<List<VertexPositionNormalTexture>>();
        public List<List<VertexPositionColor>> animated_shadow_frames = new List<List<VertexPositionColor>>();
        int AnimationFrame = 0;
        public short FrameCount = 0;
        #endregion

        BinaryFile bfile = new BinaryFile();

        public List<float> LodDistances = new List<float>();
        public List<Hook> Hooks = new List<Hook>();
        public List<Material> Materials = new List<Material>();
        public List<Lod> Lods = new List<Lod>();
        public CollisionMesh colmesh = new CollisionMesh();
        public List<FaceGroup> FaceGroups = new List<FaceGroup>();

        List<VertexPositionColor> ShadowVerts = new List<VertexPositionColor>();
        List<short> ShadowIndices = new List<short>();
        List<float> AOValues = new List<float>();
        Dictionary<int, int> AOLookUp = new Dictionary<int, int>();

        public BumpVertex[] BumpVerts;
        public VertexPositionNormalTexture[] Verts;
        public short[] indices;
        public FaceGroup SelectedFaceGroup = null;

        VertexPositionColor[] ShadowVertsArray;

        short[] ShadowIndicesArray;
        public String mesh_name;
        FaceGroup selected_facegroup = null;

        #region Constructors

        public Mesh()
        {
        }

        public Mesh(String filename)
        {
            String dir = Path.GetDirectoryName(filename);
            mesh_name = Path.GetFileNameWithoutExtension(filename);
            Binary = false;
            using (BinaryReader b = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                short test = b.ReadInt16();
                if (test == 16897)
                    Binary = true;
                b.Close();

                if (Binary)
                {
                    ReadBinary(filename, Path.GetDirectoryName(filename));
                    ShadowIndicesArray = ShadowIndices.ToArray();
                    ShadowVertsArray = ShadowVerts.ToArray();
                }
            }

            #region Text mode
            if (!Binary)
            {
                string line = "";
                int line_number = 0;
                try
                {
                    String cpart = "[CoCommon_b0]";
                    String cpart2 = "[CoCommon_b0p0]";
                    char[] seps = new char[] { ' ', '\a', '\t', '/' };
                    MeshLoaderMode mode = MeshLoaderMode.Start;
                    using (TextReader reader = File.OpenText(filename))
                    {
                        string[] parts;
                        int count = 0;
                        while ((line = reader.ReadLine()) != null)
                        {
                            line_number++;
                            if (String.IsNullOrWhiteSpace(line))
                                line = "";
                            if (line.StartsWith("\\"))
                                line = "";
                            if (line.StartsWith("//"))
                                line = "";
                            if ((line.StartsWith("[NFrames")) || (line.StartsWith("[NBlocks")))
                            {
                                line = line.TrimStart('[');
                                line = line.TrimEnd(']');
                            }

                            if ((line.Length > 0) && (!line.StartsWith("//")) && (!line.StartsWith(";")) && (!line.StartsWith("#")))
                            {
                                bool used = false;
                                string t = line;
                                int g = 0;
                                while (g < t.Length - 1)
                                {
                                    if ((t.ElementAt(g) == ' ') && (t.ElementAt(g + 1) == ' '))
                                    {
                                        t = t.Remove(g, 1);
                                    }
                                    else
                                    {
                                        if (t.ElementAt(g) == '\t')
                                        {
                                            t = t.Replace('\t', ' ');
                                            g--;
                                        }
                                        else
                                        {
                                            g++;
                                        }
                                    }
                                }
                                t = t.TrimStart(' ');
                                parts = t.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                                while (!used)
                                {

                                    if (line.StartsWith(";"))
                                    {
                                        mode = MeshLoaderMode.Done;
                                        used = true;
                                    }
                                    if (line.StartsWith("["))
                                    {
                                        if (mode == MeshLoaderMode.CoFaces)
                                        {
                                            colmesh.CurrentPart++;
                                            if (colmesh.CurrentPart == colmesh.Blocks[colmesh.CurrentBlock].NParts)
                                            {
                                                colmesh.CurrentPart = 0;
                                                colmesh.CurrentBlock++;
                                            }
                                            cpart2 = string.Format("[CoCommon_b{0}p{1}]", colmesh.CurrentBlock, colmesh.CurrentPart);
                                            cpart = string.Format("[CoCommon_b{0}]", colmesh.CurrentBlock);

                                        }
                                        mode = MeshLoaderMode.Start;
                                    }
                                    switch (mode)
                                    {
                                        case MeshLoaderMode.Start:
                                            if (line.StartsWith("[CoNei_"))
                                            {
                                                mode = MeshLoaderMode.CoNei;
                                                used = true;
                                            }
                                            if (line.StartsWith("[CoFac_"))
                                            {
                                                mode = MeshLoaderMode.CoFaces;
                                                used = true;
                                            }
                                            if (line.StartsWith("[CoNeiCnt_"))
                                            {
                                                mode = MeshLoaderMode.CoNeiCnt;
                                                used = true;
                                            }
                                            if (line.StartsWith("[CoVer0_"))
                                            {
                                                mode = MeshLoaderMode.CoVer0;
                                                used = true;
                                            }
                                            if (line.StartsWith(cpart2))
                                            {
                                                mode = MeshLoaderMode.CoCommonType;
                                                used = true;
                                            }
                                            if (line.StartsWith(cpart))
                                            {
                                                mode = MeshLoaderMode.CoCommonPart;
                                                used = true;
                                            }
                                            if (line.StartsWith("[CoCommon]"))
                                            {
                                                mode = MeshLoaderMode.CoCommon;
                                                used = true;
                                            }
                                            if (line.StartsWith("[Common]"))
                                            {
                                                mode = MeshLoaderMode.Common;
                                                used = true;
                                            }
                                            if (line.StartsWith("[LOD]"))
                                            {
                                                mode = MeshLoaderMode.Lod;
                                                used = true;
                                            }
                                            if (line.StartsWith("[Hooks]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.Hooks;
                                                used = true;
                                            }
                                            if (line.StartsWith("[HookLoc]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.Hookloc;
                                                used = true;
                                                count = 0;
                                            }
                                            if (line.StartsWith("[Materials]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.Materials;
                                                used = true;
                                            }
                                            if (line.StartsWith("[FaceGroups]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.FaceGroups;
                                                used = true;
                                            }
                                            if (line.StartsWith("[Vertices_Frame0]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.Vertices;
                                                used = true;
                                                count = 0;
                                            }
                                            if (line.StartsWith("[MaterialMapping]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.UVS;
                                                used = true;
                                                count = 0;
                                            }
                                            if (line.StartsWith("[Faces]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.Faces;
                                                used = true;
                                                count = 0;
                                            }
                                            if (line.StartsWith("[ShVertices_Frame0]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.ShadowVerts;
                                                used = true;
                                            }
                                            if ((line.StartsWith("[ShFaces]", StringComparison.OrdinalIgnoreCase)) ||
                                               (line.StartsWith("[Sh_Faces]", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                mode = MeshLoaderMode.ShadowFaces;
                                                used = true;
                                            }
                                            if (line.StartsWith("[sfVertices_Frame0]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.ShadowVerts;
                                                used = true;
                                            }
                                            if (line.StartsWith("[sfFaces]", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mode = MeshLoaderMode.ShadowFaces;
                                                used = true;
                                            }
                                          
                                            if ((line.StartsWith("[LOD", StringComparison.OrdinalIgnoreCase)) && !used)
                                            {
                                                Lod l = new Lod(reader, dir);
                                                Lods.Add(l);
                                                used = true;
                                                if (l.Done)
                                                {
                                                    reader.Close();
                                                    if (ShadowVerts.Count > 0)
                                                    {
                                                        ShadowVertsArray = ShadowVerts.ToArray();
                                                        ShadowIndicesArray = ShadowIndices.ToArray();
                                                    }
                                                    return;
                                                }
                                                if (l.continueance != "")
                                                {
                                                    line = l.continueance;
                                                    used = false;
                                                }

                                            }
                                            // error catching
                                            if (!used)
                                            {
                                                if (line.StartsWith("[CoCommon_b"))
                                                {

                                                    int pp = line.IndexOf('p');
                                                    if (pp > 0)
                                                    {
                                                        if (!line.Equals(cpart2))
                                                        {
                                                            MessageBox.Show("Collision mesh error in file " + filename + "\r\nMissing collision mesh blocks before " + line,
                                                                "IL2Modder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                            mode = MeshLoaderMode.Done;
                                                            line = "";
                                                            colmesh.Blocks.Clear();
                                                            colmesh.NBlocks = 0;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        case MeshLoaderMode.Done:
                                            reader.Close();
                                            if (ShadowVerts.Count > 0)
                                            {
                                                ShadowVertsArray = ShadowVerts.ToArray();
                                                ShadowIndicesArray = ShadowIndices.ToArray();
                                            }
                                            for (int j = 0; j < colmesh.NBlocks; j++)
                                            {
                                                for (int i = 0; i < colmesh.Blocks[j].NParts; i++)
                                                {
                                                    colmesh.Blocks[j].Parts[i].indices = colmesh.Blocks[j].Parts[i].Faces.ToArray();
                                                    colmesh.Blocks[j].Parts[i].verts = new VertexPositionColor[colmesh.Blocks[j].Parts[i].Verts.Count];
                                                    for (int k = 0; k < colmesh.Blocks[j].Parts[i].Verts.Count; k++)
                                                    {
                                                        colmesh.Blocks[j].Parts[i].verts[k].Position = colmesh.Blocks[j].Parts[i].Verts[j];
                                                        colmesh.Blocks[j].Parts[i].verts[k].Color = Microsoft.Xna.Framework.Color.Red;
                                                    }
                                                }
                                            }
                                            return;
                                        case MeshLoaderMode.Common:
                                            used = true;
                                            break;
                                        case MeshLoaderMode.Lod:
                                            {
                                                string[] nparts = line.Split(' ');
                                                LodDistances.Add(float.Parse(nparts[0], System.Globalization.CultureInfo.InvariantCulture));
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.Hooks:
                                            {
                                                Hook h = new Hook(parts[0]);
                                                Hooks.Add(h);
                                                used = true;
                                            }
                                            break;

                                        case MeshLoaderMode.Hookloc:
                                            if (count < Hooks.Count)
                                            {
                                                Hooks[count].matrix.M11 = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M12 = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M13 = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M21 = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M22 = float.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M23 = float.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M31 = float.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M32 = float.Parse(parts[7], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M33 = float.Parse(parts[8], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M41 = float.Parse(parts[9], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M42 = float.Parse(parts[10], System.Globalization.CultureInfo.InvariantCulture);
                                                Hooks[count].matrix.M43 = float.Parse(parts[11], System.Globalization.CultureInfo.InvariantCulture);
                                            }
                                            count++;
                                            used = true;
                                            break;

                                        case MeshLoaderMode.Materials:
                                            {
                                                Material m = new Material(parts[0], dir);
                                                Materials.Add(m);
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.FaceGroups:
                                            {
                                                if (parts.GetLongLength(0) > 2)
                                                {
                                                    FaceGroup fg = new FaceGroup();
                                                    fg.Material = int.Parse(parts[0]);
                                                    fg.StartVertex = int.Parse(parts[1]);
                                                    fg.VertexCount = int.Parse(parts[2]);
                                                    fg.StartFace = int.Parse(parts[3]);
                                                    fg.FaceCount = int.Parse(parts[4]);
                                                    FaceGroups.Add(fg);
                                                }
                                                else
                                                {
                                                    VertexCount = int.Parse(parts[0]);
                                                    FaceCount = int.Parse(parts[1]);
                                                    Verts = new VertexPositionNormalTexture[VertexCount];
                                                    indices = new short[FaceCount * 3];
                                                }
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.Vertices:
                                            {

                                                VertexPositionNormalTexture vp = new VertexPositionNormalTexture();
                                                vp.Position = new Vector3(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                                                                          float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                                                                          float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                                                vp.Normal = new Vector3(float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                                                                        float.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                                                                        float.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture));
                                                Verts[count] = vp;
                                                count++;
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.UVS:
                                            {
                                                Vector2 v = new Vector2(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                                                                        float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
                                                Verts[count].TextureCoordinate = v;
                                                count++;
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.Faces:
                                            {
                                                indices[count++] = short.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                                                indices[count++] = short.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                                indices[count++] = short.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.ShadowVerts:
                                            {
                                                Vector3 v = new Vector3(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                                                                        float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                                                                        float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                                                VertexPositionColor vp = new VertexPositionColor(v, Microsoft.Xna.Framework.Color.Black);
                                                ShadowVerts.Add(vp);
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.ShadowFaces:
                                            {
                                                ShadowIndices.Add(short.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture));
                                                ShadowIndices.Add(short.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
                                                ShadowIndices.Add(short.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.CoCommon:
                                            {
                                                colmesh.CurrentBlock = 0;
                                                colmesh.CurrentPart = 0;
                                                cpart = string.Format("[CoCommon_b{0}]", colmesh.CurrentBlock);
                                                cpart2 = string.Format("[CoCommon_b{0}p{1}]", colmesh.CurrentBlock, colmesh.CurrentPart);
                                                colmesh.NBlocks = int.Parse(parts[1]);
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.CoCommonPart:
                                            {
                                                CollisionMeshBlock cmb = new CollisionMeshBlock();
                                                cmb.NParts = int.Parse(parts[1]);
                                                colmesh.Blocks.Add(cmb);
                                                cpart2 = string.Format("[CoCommon_b{0}p{1}]", colmesh.CurrentBlock, colmesh.CurrentPart);
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.CoCommonType:
                                            {
                                                if (parts[0].Equals("Type"))
                                                {
                                                    CollisionMeshPart p = new CollisionMeshPart();
                                                    colmesh.Blocks[colmesh.CurrentBlock].Parts.Add(p);

                                                    p.Type = parts[1];
                                                    used = true;
                                                }
                                                if (parts[0].Equals("NFrames"))
                                                {
                                                    colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].NFrames = int.Parse(parts[1]);
                                                    used = true;
                                                }
                                                if (parts[0].Equals("Name"))
                                                {
                                                    if (parts.Length > 1)
                                                        colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].Name = parts[1];
                                                    else
                                                        colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].Name = "BUG unnamed part";
                                                    used = true;
                                                }
                                                if (parts[0].Equals("TypeIntExt"))
                                                {
                                                    if (parts[1].Equals("EXTERNAL"))
                                                        colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].TypeIntExt = 1;
                                                    else if (parts[1].Equals("INTERNAL"))
                                                        colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].TypeIntExt = 0;
                                                    else
                                                        colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].TypeIntExt = int.Parse(parts[1]);
                                                    used = true;
                                                }
                                            }
                                            break;
                                        case MeshLoaderMode.CoVer0:
                                            {
                                                Vector3 v = new Vector3();
                                                v.X = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                                                v.Y = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                                v.Z = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                                                colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].Verts.Add(v);
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.CoNeiCnt:
                                            {
                                                colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].NeiCount.Add(int.Parse(parts[0]));
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.CoNei:
                                            {
                                                colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].Neighbours.Add(int.Parse(parts[0]));
                                                used = true;
                                            }
                                            break;
                                        case MeshLoaderMode.CoFaces:
                                            {
                                                colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].Faces.Add(short.Parse(parts[0]));
                                                colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].Faces.Add(short.Parse(parts[1]));
                                                colmesh.Blocks[colmesh.CurrentBlock].Parts[colmesh.CurrentPart].Faces.Add(short.Parse(parts[2]));
                                                used = true;
                                            }
                                            break;


                                    }
                                }
                            }
                        }
                        reader.Close();
                        if (ShadowVerts.Count > 0)
                        {
                            ShadowVertsArray = ShadowVerts.ToArray();
                            ShadowIndicesArray = ShadowIndices.ToArray();
                        }
                    }
                }
                catch (Exception e)
                {
                    MSHLoaderErrorHandler msh = new MSHLoaderErrorHandler();
                    msh.SetMode(0);
                    msh.SetErrorText(e.ToString());
                    msh.SetData(String.Format("Line number {0}", line_number));
                    msh.SetErrorDescription(line);
                    msh.SetFilename(filename);
                    msh.ShowDialog();

                }
            }

            #endregion

            if (LodDistances.Count == 0)
            {
                LodDistances.Add(12999999);
            }

        }

        #endregion

        #region Search functions
        public void FindHook(String name, ref List<Vector3> hooks, Matrix pos)
        {
            string test = name.ToLower();
            foreach (Hook h in Hooks)
            {
                string test2 = h.Name.ToLower();

                if (test2.Contains(test))
                {
                    Matrix mt = h.matrix * pos;
                    Vector3 newpos = Vector3.Transform(Vector3.Zero, mt);
                    hooks.Add(newpos);
                }
            }
        }
        public void FindHook(String name, ref List<Vector3> hooks, ref List<Vector3> directions, Matrix pos)
        {
            string test = name.ToLower();
            foreach (Hook h in Hooks)
            {
                string test2 = h.Name.ToLower();

                if (test2.Contains(test))
                {
                    Matrix mt = h.matrix * pos;
                    Vector3 newpos = Vector3.Transform(Vector3.Zero, mt);
                    hooks.Add(newpos);
                    directions.Add(-mt.Left);//Vector3.Transform(Vector3.UnitY, mt));

                }
            }
        }
        public void FindHook(String name, String ignore, ref List<Vector3> hooks, Matrix pos)
        {
            foreach (Hook h in Hooks)
            {
                if ((h.Name.Contains(name)) && (!h.Name.Contains(ignore)))
                {
                    Matrix mt = h.matrix * pos;
                    Vector3 newpos = Vector3.Transform(Vector3.Zero, mt);
                    hooks.Add(newpos);
                }
            }
        }
        public Hook FindHook(String name)
        {
            string test = name.ToLower();
            test = test.Replace(' ', '@');
            foreach (Hook h in Hooks)
            {
                string test2 = h.Name.ToLower();
                test2 = test2.Replace(' ', '@');

                if (test.Equals(test2))
                    return h;
            }
            return null;
        }

        #endregion

        #region Draw methods
        public void Draw(BasicEffect be, float distance, Matrix world)
        {
            int i = 0;
            if (distance < LodDistances[0])
            {
                foreach (FaceGroup f in FaceGroups)
                {
                    be.World = world;
                    Materials[i++].Apply(be);
                    foreach (EffectPass pass in be.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        be.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                            PrimitiveType.TriangleList,
                            Verts,
                            f.StartVertex,                  // vertex buffer offset to add to each element of the index buffer
                            f.VertexCount,      // number of vertices to draw
                            indices,
                            f.StartFace * 3,      // first index element to read
                            f.FaceCount         // number of primitives to draw
                            );
                    }
                }
            }

        }
        Matrix lastWorld;

        public void Draw(Effect be, float distance, Matrix world, bool sort)
        {
            lastWorld = world;
            bool isDamaged = false;
            if (!mesh_name.EndsWith("D0"))
            {
                isDamaged = false;
            }
            int i = 0;
            if (distance <= LodDistances[0])
            {
                #region animated mesh code
                if (Animated)
                {
                    AnimationTime -= 0.02f;
                    if (AnimationTime < 0)
                    {
                        AnimationTime += 1;

                        AnimationFrame++;
                        if (AnimationFrame == FrameCount)
                            AnimationFrame = 0;
                    }
                    Verts = animation_frames[AnimationFrame].ToArray();
                }
                #endregion

                {
                    foreach (FaceGroup f in FaceGroups)
                    {
                        if (
                             ((ObjectViewer.BumpMapping) && (!Materials[i].BumpMapped)) || (!ObjectViewer.BumpMapping))
                        {
                            Materials[i].Apply(be, isDamaged);
                            if (SelectedFaceGroup == f)
                            {
                                be.Parameters["DiffuseColor"].SetValue(new Vector4(1, 0, 0, 0.5f));
                            }
                            bool isNight = Materials[i].tname.Contains("night");
                            bool drawNight = ObjectViewer.NightMode;
                            bool do_draw = !isNight;
                            if ((isNight) && (!drawNight))
                            {
                                do_draw = false;
                            }
                            if (do_draw)
                            {
                                be.Parameters["World"].SetValue(world);
                                be.Parameters["WorldInverseTranspose"].SetValue(Matrix.Invert(Matrix.Transpose(world)));

                                if (Materials[i].Sort == sort)
                                {
                                    if (f.FaceCount > 0)
                                    {
                                        foreach (EffectPass pass in be.CurrentTechnique.Passes)
                                        {
                                            pass.Apply();
                                            be.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                                PrimitiveType.TriangleList,
                                                Verts,
                                                f.StartVertex,                  // vertex buffer offset to add to each element of the index buffer
                                                f.VertexCount,      // number of vertices to draw
                                                indices,
                                                f.StartFace * 3,      // first index element to read
                                                f.FaceCount         // number of primitives to draw
                                                );
                                        }
                                    }
                                }
                            }
                        }
                        i++;
                    }
                }
            }
            else
            {
                int lod = 0;
                while ((lod < LodDistances.Count) && (distance > LodDistances[lod]))
                {
                    lod++;
                }
                if (lod == LodDistances.Count)
                {
                    return;
                }
                Lods[lod - 1].Draw(be, world, sort);
            }

        }

        public void DrawBumped(Effect be, float distance, Matrix world, bool sort)
        {
            int i = 0;
            if (distance <= LodDistances[0])
            {
                foreach (FaceGroup f in FaceGroups)
                {
                    if (Materials[i].Sort == sort)
                    {
                        if (Materials[i].BumpMapped)
                        {
                            Matrix wiv = Matrix.Invert(world);
                            wiv = Matrix.Transpose(world);
                            be.Parameters["WorldInverseTranspose"].SetValue(wiv);
                            be.Parameters["World"].SetValue(world);

                            Materials[i].ApplyBumped(be);
                            if (f.FaceCount > 0)
                            {
                                foreach (EffectPass pass in be.CurrentTechnique.Passes)
                                {
                                    pass.Apply();
                                    be.GraphicsDevice.DrawUserIndexedPrimitives<BumpVertex>(
                                        PrimitiveType.TriangleList,
                                        BumpVerts,
                                        f.StartVertex,                  // vertex buffer offset to add to each element of the index buffer
                                        f.VertexCount,      // number of vertices to draw
                                        indices,
                                        f.StartFace * 3,      // first index element to read
                                        f.FaceCount         // number of primitives to draw
                                        );
                                }
                            }
                        }
                    }
                    i++;
                }
            }


        }

        public void DrawShadow(Effect be, float distance, Matrix world)
        {
            if (ShadowVerts.Count == 0)
                return;

            if (distance <= LodDistances[0])
            {
                be.Parameters["world"].SetValue(world);
                foreach (EffectPass pass in be.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    be.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                        PrimitiveType.TriangleList,
                        ShadowVertsArray,
                        0,                      // vertex buffer offset to add to each element of the index buffer
                        ShadowVerts.Count,      // number of vertices to draw
                        ShadowIndicesArray,
                        0,                      // first index element to read
                        ShadowIndices.Count / 3   // number of primitives to draw
                        );
                }

            }
            else
            {
                int lod = 0;
                while ((lod < LodDistances.Count) && (distance > LodDistances[lod]))
                {
                    lod++;
                }
                if (lod == LodDistances.Count)
                {
                    return;
                }
                Lods[lod - 1].DrawShadow(be, distance, world);
            }
        }

        public void DrawGlass(Effect be, float distance, Matrix world, Matrix vp)
        {
            int i = 0;
            if (distance < LodDistances[0])
            {
                foreach (FaceGroup f in FaceGroups)
                {
                    be.Parameters["wvp"].SetValue(world * vp);
                    be.Parameters["world"].SetValue(world);
                    be.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
                    if (Materials[i++].Glass)
                    {
                        if (f.FaceCount > 0)
                        {
                            foreach (EffectPass pass in be.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                be.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                    PrimitiveType.TriangleList,
                                    Verts,
                                    f.StartVertex,                  // vertex buffer offset to add to each element of the index buffer
                                    f.VertexCount,      // number of vertices to draw
                                    indices,
                                    f.StartFace * 3,      // first index element to read
                                    f.FaceCount         // number of primitives to draw
                                    );
                            }
                        }
                    }
                }
            }

        }

        public void DrawCollisionMesh(BasicEffect be, Matrix world)
        {
            be.World = world;
            for (int j = 0; j < colmesh.NBlocks; j++)
            {
                foreach (CollisionMeshPart part in colmesh.Blocks[j].Parts)
                {
                    if (part.indices == null)
                    {
                        part.indices = part.Faces.ToArray();
                        part.verts = new VertexPositionColor[part.Verts.Count];
                        for (int i = 0; i < part.Verts.Count; i++)
                        {
                            part.verts[i].Color = Microsoft.Xna.Framework.Color.Red;
                            part.verts[i].Position = part.Verts[i];
                        }
                    }
                    if (part.Faces.Count > 0)
                    {
                        foreach (EffectPass pass in be.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            be.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                                PrimitiveType.TriangleList,
                                part.verts,
                                0,                  // vertex buffer offset to add to each element of the index buffer
                                part.Verts.Count,   // number of vertices to draw
                                part.indices,
                                0,                  // first index element to read
                                part.Faces.Count / 3  // number of primitives to draw
                                );
                        }
                    }
                }
            }
        }

        private int trim(float x, float size)
        {
            float dx = x - (float)((int)x);
            dx *= size;
            return (int)dx;
        }

        public void DrawSkin(Graphics g, Pen p, Brush b, String texture, float size)
        {
            int i = 0;
            System.Drawing.Point[] points = new System.Drawing.Point[3];
            foreach (FaceGroup f in FaceGroups)
            {
                if (Form1.Manager.Names[Materials[i].TextureID].Equals(texture, StringComparison.OrdinalIgnoreCase))
                {
                    int tri = f.StartFace * 3;
                    for (int j = 0; j < f.FaceCount; j++)
                    {
                        int v1 = indices[tri++] + f.StartVertex;
                        int v2 = indices[tri++] + f.StartVertex;
                        int v3 = indices[tri++] + f.StartVertex;

                        points[0].X = trim(Verts[v1].TextureCoordinate.X, size);
                        points[0].Y = trim(Verts[v1].TextureCoordinate.Y, size);
                        points[1].X = trim(Verts[v2].TextureCoordinate.X, size);
                        points[1].Y = trim(Verts[v2].TextureCoordinate.Y, size);
                        points[2].X = trim(Verts[v3].TextureCoordinate.X, size);
                        points[2].Y = trim(Verts[v3].TextureCoordinate.Y, size);
                        g.FillPolygon(b, points);
                        g.DrawPolygon(p, points);
                    }
                }
                i++;
            }

        }

        public void DrawSkinPart(Graphics g, Pen p, Brush b, String texture, float size)
        {
            System.Drawing.Point[] points = new System.Drawing.Point[3];
            FaceGroup f = selected_facegroup;

            int tri = f.StartFace * 3;
            for (int j = 0; j < f.FaceCount; j++)
            {
                int v1 = indices[tri++] + f.StartVertex;
                int v2 = indices[tri++] + f.StartVertex;
                int v3 = indices[tri++] + f.StartVertex;

                points[0].X = trim(Verts[v1].TextureCoordinate.X, size);
                points[0].Y = trim(Verts[v1].TextureCoordinate.Y, size);
                points[1].X = trim(Verts[v2].TextureCoordinate.X, size);
                points[1].Y = trim(Verts[v2].TextureCoordinate.Y, size);
                points[2].X = trim(Verts[v3].TextureCoordinate.X, size);
                points[2].Y = trim(Verts[v3].TextureCoordinate.Y, size);
                g.FillPolygon(b, points);
                g.DrawPolygon(p, points);
            }
        }

        public void DrawAo(BasicEffect effect, int size, ref List<float> ao_values)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();

            foreach (FaceGroup f in FaceGroups)
            {
                if (Form1.Manager.Names[Materials[f.Material].TextureID].Equals("skin1o.tga", StringComparison.OrdinalIgnoreCase))
                {
                    int tri = f.StartFace * 3;
                    for (int j = 0; j < f.FaceCount; j++)
                    {
                        int v1 = indices[tri++] + f.StartVertex;
                        int v2 = indices[tri++] + f.StartVertex;
                        int v3 = indices[tri++] + f.StartVertex;

                        VertexPositionColor pc = new VertexPositionColor();
                        pc.Position.X = trim(Verts[v1].TextureCoordinate.X, size);
                        pc.Position.Y = trim(Verts[v1].TextureCoordinate.Y, size);
                        pc.Position.Z = -0.5f;
                        float ac = ao_values[0];
                        ao_values.RemoveAt(0);
                        Microsoft.Xna.Framework.Color rc = Microsoft.Xna.Framework.Color.FromNonPremultiplied(new Vector4(ac, ac, ac, 1));
                        pc.Color = rc;
                        verts.Add(pc);

                        pc = new VertexPositionColor();
                        pc.Position.X = trim(Verts[v2].TextureCoordinate.X, size);
                        pc.Position.Y = trim(Verts[v2].TextureCoordinate.Y, size);
                        pc.Position.Z = -0.5f;
                        ac = ao_values[0];
                        ao_values.RemoveAt(0);
                        rc = Microsoft.Xna.Framework.Color.FromNonPremultiplied(new Vector4(ac, ac, ac, 1));
                        pc.Color = rc;
                        verts.Add(pc);

                        pc = new VertexPositionColor();
                        pc.Position.X = trim(Verts[v3].TextureCoordinate.X, size);
                        pc.Position.Y = trim(Verts[v3].TextureCoordinate.Y, size);
                        pc.Position.Z = -0.5f;
                        ac = ao_values[0];
                        ao_values.RemoveAt(0);
                        rc = Microsoft.Xna.Framework.Color.FromNonPremultiplied(new Vector4(ac, ac, ac, 1));
                        pc.Color = rc;
                        verts.Add(pc);

                    }
                }
            }
            if (verts.Count > 0)
            {
                VertexPositionColor[] vs = verts.ToArray();

                VertexBuffer vertexBuffer = new VertexBuffer(Form1.graphics, typeof(VertexPositionColor), verts.Count, BufferUsage.WriteOnly);
                vertexBuffer.SetData<VertexPositionColor>(vs);
                Form1.graphics.SetVertexBuffer(vertexBuffer);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    effect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, verts.Count / 3);
                }

                vertexBuffer.Dispose();

            }
        }

        public void DrawAo(BasicEffect effect, int size, String texture)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            foreach (FaceGroup f in FaceGroups)
            {
                if (Form1.Manager.Names[Materials[f.Material].TextureID].Equals(texture, StringComparison.OrdinalIgnoreCase))
                {
                    int tri = f.StartFace * 3;
                    for (int j = 0; j < f.FaceCount; j++)
                    {
                        int v1 = indices[tri++] + f.StartVertex;
                        int v2 = indices[tri++] + f.StartVertex;
                        int v3 = indices[tri++] + f.StartVertex;

                        VertexPositionColor pc = new VertexPositionColor();
                        pc.Position.X = trim(Verts[v1].TextureCoordinate.X, size);
                        pc.Position.Y = trim(Verts[v1].TextureCoordinate.Y, size);
                        pc.Position.Z = -0.5f;

                        int ic = AOLookUp[v1];
                        float ac = 1.0f - AOValues[ic];
                        Microsoft.Xna.Framework.Color rc = Microsoft.Xna.Framework.Color.FromNonPremultiplied(new Vector4(ac, ac, ac, 1));
                        pc.Color = rc;
                        verts.Add(pc);

                        pc = new VertexPositionColor();
                        pc.Position.X = trim(Verts[v2].TextureCoordinate.X, size);
                        pc.Position.Y = trim(Verts[v2].TextureCoordinate.Y, size);
                        pc.Position.Z = -0.5f;

                        ic = AOLookUp[v2];
                        ac = 1.0f - AOValues[ic];
                        rc = Microsoft.Xna.Framework.Color.FromNonPremultiplied(new Vector4(ac, ac, ac, 1));
                        pc.Color = rc;
                        verts.Add(pc);

                        pc = new VertexPositionColor();
                        pc.Position.X = trim(Verts[v3].TextureCoordinate.X, size);
                        pc.Position.Y = trim(Verts[v3].TextureCoordinate.Y, size);
                        pc.Position.Z = -0.5f;

                        ic = AOLookUp[v3];
                        ac = 1.0f - AOValues[ic];
                        rc = Microsoft.Xna.Framework.Color.FromNonPremultiplied(new Vector4(ac, ac, ac, 1));
                        pc.Color = rc;
                        verts.Add(pc);

                    }
                }
            }
            if (verts.Count > 0)
            {
                VertexPositionColor[] vs = verts.ToArray();

                VertexBuffer vertexBuffer = new VertexBuffer(Form1.graphics, typeof(VertexPositionColor), verts.Count, BufferUsage.WriteOnly);
                vertexBuffer.SetData<VertexPositionColor>(vs);
                Form1.graphics.SetVertexBuffer(vertexBuffer);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    effect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, verts.Count / 3);
                }

                vertexBuffer.Dispose();

            }
        }

        public void DrawNormals(BasicEffect effect)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();

            foreach (VertexPositionNormalTexture f in Verts)
            {
                VertexPositionColor vpc = new VertexPositionColor();
                vpc.Position = f.Position;
                vpc.Color = Microsoft.Xna.Framework.Color.DeepPink;
                verts.Add(vpc);
                vpc = new VertexPositionColor();
                vpc.Position = f.Position - (f.Normal * 0.25f);
                vpc.Color = Microsoft.Xna.Framework.Color.DeepPink;
                verts.Add(vpc);
            }
            VertexPositionColor[] v = verts.ToArray();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Form1.graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, v, 0, verts.Count / 2);
            }


        }

        #endregion

        #region BinaryReaders
        public void ReadBinary(String name, String dir)
        {
            byte re = 0;
            short se = 0;
            Animated = false;
            using (BinaryReader b = new BinaryReader(File.Open(name, FileMode.Open)))
            {
                String bname = "";
                String mode = "";
                try
                {
                    Lod lod = null;

                    #region Read header
                    mode = "Reading header";
                    bfile.header.magic = b.ReadInt32();
                    bfile.header.filesize = b.ReadInt32();
                    bfile.header.sections = b.ReadInt32();
                    bfile.header.unknown = b.ReadInt32();
                    bfile.header.table1 = b.ReadInt32();
                    bfile.header.table2 = b.ReadInt32();
                    bfile.header.table3 = b.ReadInt32();
                    #endregion

                    #region Read section names
                    mode = "Reading section names";
                    for (int i = 0; i < (int)bfile.header.sections; i++)
                    {
                        bfile.sections.Add(ReadString(b));
                    }
                    #endregion

                    #region Read table 1
                    mode = "Reading table 1";
                    b.BaseStream.Position = bfile.header.table1;
                    for (int i = 0; i < (int)bfile.header.sections; i++)
                    {
                        T1Entry t = new T1Entry();
                        t.Offset = b.ReadInt32();
                        t.StartRecord = b.ReadInt32();
                        t.NumberOfRecords = b.ReadInt16();
                        t.Type = b.ReadInt16();
                        bfile.table1.Add(t);
                    }
                    #endregion

                    #region Read table 3
                    mode = "Reading table 3";
                    b.BaseStream.Position = bfile.header.table3;
                    bfile.data_start = b.ReadInt32();
                    while (b.BaseStream.Position < bfile.data_start)
                    {
                        bfile.subsection_size.Add(b.ReadInt32());
                    }
                    #endregion

                    String block_start = "[CoCommon_b0]";
                    String part_start = "[CoCommon_b0p0]";

                    #region Read data
                    b.BaseStream.Position = bfile.data_start;
                    int sr = 0;
                    for (int i = 0; i < bfile.header.sections; i++)
                    {
                        bname = bfile.sections[i];

                        mode = "Reading spacer";
                        if (bname.Contains("Space"))
                        {
                            sr = bfile.table1[i].StartRecord;
                            for (int j = 0; j < bfile.table1[i].NumberOfRecords; j++)
                            {
                                int ss = bfile.subsection_size[sr++];
                                b.ReadBytes(ss);
                            }
                        }
                        #region Common
                        if (bname.StartsWith("[Common]"))
                        {
                            mode = "Reading Common";

                            for (int j = 0; j < bfile.table1[i].NumberOfRecords; j++)
                            {
                                long pos = b.BaseStream.Position;
                                String t = ReadString3(b);

                                switch (t)
                                {
                                    case "FramesType":
                                        {
                                            String h = ReadString3(b);
                                            if (h.Equals("Animation"))
                                                Animated = true;
                                        }
                                        break;
                                    case "NumFrames":
                                        {
                                            int b1 = b.ReadByte();
                                            int b2 = b.ReadByte();
                                            if (Animated)
                                            {

                                                FrameCount = (short)(b2 + (b1 * 256));
                                                for (short ki = 0; ki < FrameCount; ki++)
                                                {
                                                    animated_shadow_frames.Add(new List<VertexPositionColor>());
                                                    animation_frames.Add(new List<VertexPositionNormalTexture>());
                                                }
                                                //b.ReadByte();
                                                //b.ReadByte();
                                            }
                                            else
                                            {
                                                FrameCount = 1;
                                                b.ReadByte();
                                            }
                                        }
                                        break;
                                    default:
                                        {
                                            b.BaseStream.Position = pos + bfile.subsection_size[sr++];
                                        }
                                        break;
                                }

                            }
                        }
                        #endregion

                        #region Lod table
                        if (bname.StartsWith("[LOD]"))
                        {
                            mode = "Reading LOD";

                            Int16 lod_distance = 0;
                            switch (bfile.table1[i].Type)
                            {
                                case 1:
                                    {
                                        for (int j = 0; j < bfile.table1[i].NumberOfRecords; j++)
                                        {
                                            LodDistances.Add(b.ReadByte());
                                        }
                                    }
                                    break;
                                case 0x0101:
                                    {
                                        for (int j = 0; j < bfile.table1[i].NumberOfRecords; j++)
                                        {
                                            lod_distance += b.ReadByte();
                                            LodDistances.Add(lod_distance);
                                        }
                                    }
                                    break;
                                case 0:
                                    b.ReadByte();
                                    LodDistances.Add(lod_distance);
                                    for (int j = 0; j < bfile.table1[i].NumberOfRecords - 1; j++)
                                    {
                                        lod_distance += (Int16)b.ReadSingle();
                                        LodDistances.Add(lod_distance);
                                    }
                                    break;
                                case 258:
                                case 0x02:
                                    {
                                        for (int j = 0; j < bfile.table1[i].NumberOfRecords; j++)
                                        {
                                            lod_distance += b.ReadInt16();
                                            LodDistances.Add(lod_distance);
                                        }
                                    }
                                    break;

                            }
                        }
                        #endregion

                        #region Materials
                        if (bname.StartsWith("[Materials]"))
                        {
                            mode = "Reading materials";

                            sr = bfile.table1[i].StartRecord;
                            //if (Animated)
                            //  b.ReadByte();
                            for (int j = 0; j < bfile.table1[i].NumberOfRecords; j++)
                            {
                                int ss = bfile.subsection_size[sr++];
                                String mat = ReadString2(b, ss);
                                Material m = new Material(mat, dir);
                                Materials.Add(m);
                            }
                        }
                        #endregion

                        #region Hooks
                        if (bname.StartsWith("[Hooks]"))
                        {
                            mode = "Reading hooks";

                            sr = bfile.table1[i].StartRecord;
                            if (Animated)
                                b.ReadByte();
                            for (int j = 0; j < bfile.table1[i].NumberOfRecords; j++)
                            {

                                int ss = bfile.subsection_size[sr++];

                                String mat = ReadSplitString(b, ss);
                                Hook h = new Hook(mat);
                                if (Animated)
                                {
                                    for (int ih = 0; ih < FrameCount; ih++)
                                        Hooks.Add(h);
                                }
                                else
                                    Hooks.Add(h);
                            }
                        }
                        #endregion

                        #region Hookloc
                        if (bname.StartsWith("[HookLoc]"))
                        {
                            mode = "Reading hook locations";

                            sr = bfile.table1[i].StartRecord;
                            //b.ReadByte();
                            int k = 0;
                            for (int j = 0; j < bfile.table1[i].NumberOfRecords; j++)
                            {
                                int size = bfile.subsection_size[sr++];

                                switch (size)
                                {
                                    case 24:
                                        Hooks[k].matrix.M11 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M12 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M13 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M21 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M22 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M23 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M31 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M32 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M33 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M41 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M42 = b.ReadInt16() / 32768.0f;
                                        Hooks[k].matrix.M43 = b.ReadInt16() / 32768.0f;
                                        break;

                                    case 48:
                                        Hooks[k].matrix.M11 = b.ReadSingle();
                                        Hooks[k].matrix.M12 = b.ReadSingle();
                                        Hooks[k].matrix.M13 = b.ReadSingle();
                                        Hooks[k].matrix.M21 = b.ReadSingle();
                                        Hooks[k].matrix.M22 = b.ReadSingle();
                                        Hooks[k].matrix.M23 = b.ReadSingle();
                                        Hooks[k].matrix.M31 = b.ReadSingle();
                                        Hooks[k].matrix.M32 = b.ReadSingle();
                                        Hooks[k].matrix.M33 = b.ReadSingle();
                                        Hooks[k].matrix.M41 = b.ReadSingle();
                                        Hooks[k].matrix.M42 = b.ReadSingle();
                                        Hooks[k].matrix.M43 = b.ReadSingle();
                                        break;

                                    case 40:
                                        Hooks[k].matrix.M11 = b.ReadSingle();
                                        Hooks[k].matrix.M12 = b.ReadSingle();
                                        Hooks[k].matrix.M13 = b.ReadSingle();
                                        Hooks[k].matrix.M21 = b.ReadSingle();
                                        Hooks[k].matrix.M22 = b.ReadSingle();
                                        Hooks[k].matrix.M23 = b.ReadSingle();
                                        Hooks[k].matrix.M31 = b.ReadSingle();
                                        Hooks[k].matrix.M32 = b.ReadSingle();
                                        Hooks[k].matrix.M33 = b.ReadSingle();
                                        Hooks[k].matrix.M41 = b.ReadSingle();
                                        Hooks[k].matrix.M42 = 0;
                                        Hooks[k].matrix.M43 = 0;
                                        break;

                                    default:
                                        {
                                            // one byte out
                                            long pos = b.BaseStream.Position;
                                            Hooks[k].matrix.M11 = ReadMotorola(b);
                                            Hooks[k].matrix.M12 = ReadMotorola(b);
                                            Hooks[k].matrix.M13 = ReadMotorola(b);
                                            Hooks[k].matrix.M21 = ReadMotorola(b);
                                            Hooks[k].matrix.M22 = ReadMotorola(b);
                                            Hooks[k].matrix.M23 = ReadMotorola(b);
                                            Hooks[k].matrix.M31 = ReadMotorola(b);
                                            Hooks[k].matrix.M32 = ReadMotorola(b);
                                            Hooks[k].matrix.M33 = ReadMotorola(b);
                                            Hooks[k].matrix.M41 = ReadMotorola(b);
                                            Hooks[k].matrix.M42 = ReadMotorola(b);
                                            Hooks[k].matrix.M43 = ReadMotorola(b);
                                            pos = b.BaseStream.Position - pos;
                                            if (pos != size)
                                            {
                                                throw new Exception("Hook location format problem");
                                            }
                                        }
                                        break;

                                }
                                k++;
                            }
                        }
                        #endregion

                        #region Face groups
                        if (bname.StartsWith("[FaceGroups]"))
                        {
                            mode = "Reading face groups";

                            FaceGroup f;

                            int j = bfile.table1[i].NumberOfRecords;
                            int firstrecord = bfile.table1[i].StartRecord;
                            int size = bfile.subsection_size[firstrecord++];
                            if (size == 2)
                            {
                                switch (bfile.table1[i].Type)
                                {
                                    case 1:
                                        VertexCount = b.ReadByte();
                                        FaceCount = b.ReadByte();
                                        break;
                                    default:
                                        re = b.ReadByte();
                                        VertexCount = re;
                                        re += b.ReadByte();
                                        FaceCount = re;
                                        break;
                                }
                            }
                            else if (size == 4)
                            {
                                VertexCount = b.ReadInt16();
                                FaceCount = VertexCount + b.ReadInt16();
                            }
                            else
                            {
                                throw new NotSupportedException("FaceGroups start record is larger than 2 bytes");
                            }
                            indices = new short[3 * FaceCount];
                            Verts = new VertexPositionNormalTexture[VertexCount];
                            j--;
                            se = (short)FaceCount;
                            while (j > 0)
                            {
                                f = new FaceGroup();
                                size = bfile.subsection_size[firstrecord++];

                                if (size == 6)
                                {
                                    switch (bfile.table1[i].Type)
                                    {
                                        case 1:
                                            f.Material = b.ReadByte();
                                            f.StartVertex = b.ReadByte();
                                            f.VertexCount = b.ReadByte();
                                            f.StartFace = b.ReadByte();
                                            f.FaceCount = b.ReadByte();
                                            b.ReadByte();
                                            break;
                                        default:
                                            re += b.ReadByte();
                                            f.Material = re;
                                            re += b.ReadByte();
                                            f.StartVertex = re;
                                            re += b.ReadByte();
                                            f.VertexCount = re;
                                            re += b.ReadByte();
                                            f.StartFace = re;
                                            re += b.ReadByte();
                                            f.FaceCount = re;
                                            re += b.ReadByte();     // not sure what this byte is for
                                            break;
                                    }
                                }
                                else if (size == 12)
                                {

                                    se += b.ReadInt16();
                                    f.Material = se;
                                    se += b.ReadInt16();
                                    f.StartVertex = se;
                                    se += b.ReadInt16();
                                    f.VertexCount = se;
                                    se += b.ReadInt16();
                                    f.StartFace = se;
                                    se += b.ReadInt16();
                                    f.FaceCount = se;
                                    se += b.ReadInt16();

                                }
                                else
                                {
                                    throw new NotSupportedException("FaceGroups unknown size");
                                }
                                FaceGroups.Add(f);
                                j--;
                            }
                        }
                        #endregion

                        #region Vertices_frame

                        if (bname.StartsWith("[Vertices_Frame"))
                        {
                            mode = "Reading vertices";

                            Boolean had_motorola = false;
                            int j = bfile.table1[i].NumberOfRecords;
                            int firstrecord = bfile.table1[i].StartRecord;


                            CurrentAnimationFrame++;

                            for (int ii = 0; ii < j; ii++)
                            {
                                int size = bfile.subsection_size[firstrecord++];
#if !TEST
                                switch (size)
                                {
                                    case 18:
                                        {
                                            VertexPositionNormalTexture vp = new VertexPositionNormalTexture();
                                            if (had_motorola)
                                            {
                                                vp.Position.X = ReadMotorola(b);
                                                vp.Position.Y = ReadMotorola(b);
                                                vp.Position.Z = ReadMotorola(b);
                                                vp.Normal.X = ReadMotorola(b);
                                                vp.Normal.Y = ReadMotorola(b);
                                                vp.Normal.Z = ReadMotorola(b);

                                            }
                                            else
                                            {
                                                vp.Position.X = ReadTriple(b);
                                                vp.Position.Y = ReadTriple(b);
                                                vp.Position.Z = ReadTriple(b);
                                                vp.Normal.X = ReadTriple(b);
                                                vp.Normal.Y = ReadTriple(b);
                                                vp.Normal.Z = ReadTriple(b);
                                            }
                                            if (Animated)
                                            {
                                                animation_frames[CurrentAnimationFrame].Add(vp);
                                            }
                                            else
                                            {
                                                Verts[ii] = vp;
                                            }
                                        }
                                        break;
                                    case 20:
                                        {
                                            VertexPositionNormalTexture vp = new VertexPositionNormalTexture();

                                            vp.Position.X = ReadMotorola(b);
                                            vp.Position.Y = ReadMotorola(b);
                                            vp.Position.Z = ReadMotorola(b);
                                            vp.Normal.X = ReadMotorola(b);
                                            vp.Normal.Y = ReadMotorola(b);
                                            vp.Normal.Z = ReadMotorola(b);
                                            had_motorola = true;

                                            if (Animated)
                                            {
                                                animation_frames[CurrentAnimationFrame].Add(vp);
                                            }
                                            else
                                            {
                                                Verts[ii] = vp;
                                            }
                                        }
                                        break;
                                    case 22:
                                        {

                                            VertexPositionNormalTexture vp = new VertexPositionNormalTexture();
                                            vp.Position.X = ReadMotorola(b);
                                            vp.Position.Y = ReadMotorola(b);
                                            vp.Position.Z = ReadMotorola(b);
                                            vp.Normal.X = ReadMotorola(b);
                                            vp.Normal.Y = ReadMotorola(b);
                                            vp.Normal.Z = ReadMotorola(b);
                                            had_motorola = true;
                                            if (Animated)
                                            {
                                                animation_frames[CurrentAnimationFrame].Add(vp);
                                            }
                                            else
                                            {
                                                Verts[ii] = vp;
                                            }

                                        }
                                        break;
                                    case 24:
                                        {
                                            VertexPositionNormalTexture vp = new VertexPositionNormalTexture();
                                            if (had_motorola)
                                            {
                                                vp.Position.X = ReadMotorola(b);
                                                vp.Position.Y = ReadMotorola(b);
                                                vp.Position.Z = ReadMotorola(b);
                                                vp.Normal.X = ReadMotorola(b);
                                                vp.Normal.Y = ReadMotorola(b);
                                                vp.Normal.Z = ReadMotorola(b);
                                            }
                                            else
                                            {
                                                vp.Position.X = b.ReadSingle();
                                                vp.Position.Y = b.ReadSingle();
                                                vp.Position.Z = b.ReadSingle();
                                                vp.Normal.X = b.ReadSingle();
                                                vp.Normal.Y = b.ReadSingle();
                                                vp.Normal.Z = b.ReadSingle();
                                            }
                                            if (Animated)
                                            {
                                                animation_frames[CurrentAnimationFrame].Add(vp);
                                            }
                                            else
                                            {
                                                Verts[ii] = vp;
                                            }

                                        }
                                        break;
                                    case 30:
                                    case 26:
                                    case 28:
                                        {
                                            VertexPositionNormalTexture vp = new VertexPositionNormalTexture();
                                            had_motorola = true;
                                            int rsize = 0;
                                            int bt = b.ReadByte();
                                            if (bt == 0)
                                            {
                                                vp.Position.X = ReadDouble(b);
                                                rsize += 3;
                                            }
                                            else
                                            {
                                                vp.Position.X = b.ReadSingle();
                                                rsize += 5;
                                            }
                                            bt = b.ReadByte();
                                            if (bt == 0)
                                            {
                                                vp.Position.Y = ReadDouble(b);
                                                rsize += 3;
                                            }
                                            else
                                            {
                                                vp.Position.Y = b.ReadSingle();
                                                rsize += 5;
                                            }
                                            bt = b.ReadByte();
                                            if (bt == 0)
                                            {
                                                vp.Position.Z = ReadDouble(b);
                                                rsize += 3;
                                            }
                                            else
                                            {
                                                vp.Position.Z = b.ReadSingle();
                                                rsize += 5;
                                            }
                                            bt = b.ReadByte();
                                            if (bt == 0)
                                            {
                                                vp.Normal.X = ReadDouble(b);
                                                rsize += 3;
                                            }
                                            else
                                            {
                                                vp.Normal.X = b.ReadSingle();
                                                rsize += 5;
                                            }
                                            bt = b.ReadByte();
                                            if (bt == 0)
                                            {
                                                vp.Normal.Y = ReadDouble(b);
                                                rsize += 3;
                                            }
                                            else
                                            {
                                                vp.Normal.Y = b.ReadSingle();
                                                rsize += 5;
                                            }
                                            bt = b.ReadByte();
                                            if (bt == 0)
                                            {
                                                vp.Normal.Z = ReadDouble(b);
                                                rsize += 3;
                                            }
                                            else
                                            {
                                                vp.Normal.Z = b.ReadSingle();
                                                rsize += 5;
                                            }
                                            if (rsize != size)
                                            {
                                                throw new Exception("Bugger");
                                            }
                                            if (Animated)
                                            {
                                                animation_frames[CurrentAnimationFrame].Add(vp);
                                            }
                                            else
                                            {
                                                Verts[ii] = vp;
                                            }


                                        }
                                        break;
                                    default:
                                        throw new Exception("Unhandled vertex size " + size);
                                }
#else                       
                            VertexPositionNormalTexture vp = new VertexPositionNormalTexture();
                            int rsize=0;
                            int bt = b.ReadByte();
                            if (bt == 0)
                            {
                                vp.Position.X = ReadDouble(b);
                                rsize += 3;
                            }
                            else
                            {
                                vp.Position.X = b.ReadSingle();
                                rsize += 5;
                            }
                            bt = b.ReadByte();
                            if (bt == 0)
                            {
                                vp.Position.Y = ReadDouble(b);
                                rsize += 3;
                            }
                            else
                            {
                                vp.Position.Y = b.ReadSingle();
                                rsize += 5;
                            }
                            bt = b.ReadByte();
                            if (bt == 0)
                            {
                                vp.Position.Z = ReadDouble(b);
                                rsize += 3;
                            }
                            else
                            {
                                vp.Position.Z = b.ReadSingle();
                                rsize += 5;
                            }
                            bt = b.ReadByte();
                            if (bt == 0)
                            {
                                vp.Normal.X = ReadDouble(b);
                                rsize += 3;
                            }
                            else
                            {
                                vp.Normal.X = b.ReadSingle();
                                rsize += 5;
                            }
                            bt = b.ReadByte();
                            if (bt == 0)
                            {
                                vp.Normal.Y = ReadDouble(b);
                                rsize += 3;
                            }
                            else
                            {
                                vp.Normal.Y = b.ReadSingle();
                                rsize += 5;
                            }
                            bt = b.ReadByte();
                            if (bt == 0)
                            {
                                vp.Normal.Z = ReadDouble(b);
                                rsize += 3;
                            }
                            else
                            {
                                vp.Normal.Z = b.ReadSingle();
                                rsize += 5;
                            }
                            if (rsize != size)
                            {
                                throw new Exception("Bugger");
                            }
#endif
                            }
                        }
                        #endregion

                        #region Material mapping
                        if (bname.StartsWith("[MaterialMapping]"))
                        {
                            mode = "Reading material mapping";

                            CurrentAnimationFrame = -1;
                            re = 0;
                            int j = bfile.table1[i].NumberOfRecords;
                            for (int ii = 0; ii < j; ii++)
                            {
                                switch (bfile.table1[i].Type)
                                {
                                    case 257:
                                        {
                                            re += b.ReadByte();
                                            Verts[ii].TextureCoordinate.X = (float)re;
                                            re += b.ReadByte();
                                            Verts[ii].TextureCoordinate.Y = (float)re;

                                        }
                                        break;
                                    case 3:
                                        {
                                            int a1 = b.ReadByte();
                                            int a2 = b.ReadByte();
                                            int a3 = b.ReadByte();
                                            int a4 = 0;
                                            if ((a3 & 128) != 0)
                                                a4 = 255;
                                            a3 = (a4 << 24) + (a3 << 16) + (a2 << 8) + a1;
                                            Verts[ii].TextureCoordinate.X = (float)(a3 / 65536.0f);

                                            a1 = b.ReadByte();
                                            a2 = b.ReadByte();
                                            a3 = b.ReadByte();
                                            a4 = 0;
                                            if ((a3 & 128) != 0)
                                                a4 = 255;
                                            a3 = (a4 << 24) + (a3 << 16) + (a2 << 8) + a1;
                                            Verts[ii].TextureCoordinate.Y = (float)(a3 / 65536.0f);
                                        }
                                        break;
                                    case 4:
                                        {
                                            Verts[ii].TextureCoordinate.X = b.ReadSingle();
                                            Verts[ii].TextureCoordinate.Y = b.ReadSingle();

                                        }
                                        break;
                                    default:
                                        throw new Exception("MaterialMapping size not supported " + bfile.table1[i].Type);

                                }


                            }
                            if (Animated)
                            {
                                for (int frame = 0; frame < FrameCount; frame++)
                                {
                                    List<VertexPositionNormalTexture> this_frame = animation_frames[frame];
                                    VertexPositionNormalTexture[] vpt = this_frame.ToArray();
                                    for (int vert = 0; vert < this_frame.Count; vert++)
                                    {
                                        vpt[vert].TextureCoordinate.X = Verts[vert].TextureCoordinate.X;
                                        vpt[vert].TextureCoordinate.Y = Verts[vert].TextureCoordinate.Y;
                                    }
                                    this_frame.Clear();
                                    this_frame.AddRange(vpt);
                                    animation_frames[frame] = this_frame;
                                }
                            }
                        }
                        #endregion

                        #region Faces
                        if (bname.StartsWith("[Faces]"))
                        {
                            mode = "Reading faces";

                            int j = bfile.table1[i].NumberOfRecords;
                            switch (bfile.table1[i].Type)
                            {
                                case 1:
                                case 0x0101:
                                    {
                                        re = 0;
                                        int k = 0;
                                        for (int ii = 0; ii < j; ii++)
                                        {
                                            re += b.ReadByte();
                                            indices[k++] = (short)re;
                                            re += b.ReadByte();
                                            indices[k++] = (short)re;
                                            re += b.ReadByte();
                                            indices[k++] = (short)re;
                                        }
                                    }
                                    break;
                                case 258:
                                case 0x02:
                                    {
                                        se = 0;
                                        int k = 0;
                                        for (int jj = 0; jj < bfile.table1[i].NumberOfRecords; jj++)
                                        {
                                            se += b.ReadInt16();
                                            indices[k++] = se;
                                            se += b.ReadInt16();
                                            indices[k++] = se;
                                            se += b.ReadInt16();
                                            indices[k++] = se;
                                        }
                                    }
                                    break;


                                default:
                                    throw new Exception("Unhandled face size " + bfile.table1[i].Type);
                            }
                        }
                        #endregion

                        #region Shadow verts
                        if (bname.StartsWith("[ShVertices_Frame"))
                        {
                            mode = "Reading shadow verts";

                            CurrentAnimationFrame++;
                            int j = bfile.table1[i].NumberOfRecords;
                            VertexPositionColor v = new VertexPositionColor();
                            for (int ii = 0; ii < j; ii++)
                            {
                                switch (bfile.table1[i].Type)
                                {
                                    case 3:
                                        {
                                            v.Position.X = ReadTriple(b);
                                            v.Position.Y = ReadTriple(b);
                                            v.Position.Z = ReadTriple(b);
                                        }
                                        break;
                                    default:
                                        throw new Exception("Unhandled shadow vert type");
                                }
                                v.Color = Microsoft.Xna.Framework.Color.White;
                                if (Animated)
                                {
                                    animated_shadow_frames[CurrentAnimationFrame].Add(v);
                                }
                                else
                                    ShadowVerts.Add(v);
                            }
                        }
                        #endregion

                        #region Shadow faces
                        if (bname.StartsWith("[ShFaces]"))
                        {
                            mode = "Reading shadow faces";

                            CurrentAnimationFrame = -1;
                            re = 0;
                            int j = bfile.table1[i].NumberOfRecords;
                            switch (bfile.table1[i].Type)
                            {
                                case 1:
                                    {

                                        for (int ii = 0; ii < j; ii++)
                                        {
                                            re = b.ReadByte();
                                            ShadowIndices.Add((short)re);
                                            re = b.ReadByte();
                                            ShadowIndices.Add((short)re);
                                            re = b.ReadByte();
                                            ShadowIndices.Add((short)re);
                                        }
                                    }
                                    break;
                                case 0x0101:
                                    {

                                        for (int ii = 0; ii < j; ii++)
                                        {
                                            re += b.ReadByte();
                                            ShadowIndices.Add((short)re);
                                            re += b.ReadByte();
                                            ShadowIndices.Add((short)re);
                                            re += b.ReadByte();
                                            ShadowIndices.Add((short)re);
                                        }
                                    }
                                    break;

                                case 0x102:
                                    {
                                        for (int jj = 0; jj < bfile.table1[i].NumberOfRecords; jj++)
                                        {
                                            se += b.ReadInt16();
                                            ShadowIndices.Add((short)se);
                                            se += b.ReadInt16();
                                            ShadowIndices.Add((short)se);
                                            se += b.ReadInt16();
                                            ShadowIndices.Add((short)se);
                                        }
                                    }
                                    break;

                                case 0x02:
                                    {
                                        for (int jj = 0; jj < bfile.table1[i].NumberOfRecords; jj++)
                                        {
                                            se = b.ReadInt16();
                                            ShadowIndices.Add((short)se);
                                            se = b.ReadInt16();
                                            ShadowIndices.Add((short)se);
                                            se = b.ReadInt16();
                                            ShadowIndices.Add((short)se);
                                        }
                                    }
                                    break;
                                default:
                                    throw new Exception("Shadow faces unhandled size " + bfile.table1[i].Type);
                            }
                        }
                        #endregion

                        #region CoCommon
                        if (bname.StartsWith("[CoCommon]"))
                        {
                            mode = "Reading CoCommon";

                            String check = ReadString(b);
                            if (!check.Equals("NBlocks"))
                                throw new Exception("Binary reader CoCommon Error " + check);

                            colmesh.NBlocks = b.ReadInt16();
                            colmesh.CurrentBlock = 0;
                            colmesh.CurrentPart = 0;
                            block_start = "[CoCommon_b0]";
                            part_start = "[CoCommon_b0p0]";
                        }
                        #endregion

                        #region CoCommonBlock
                        if (bname.Equals(block_start))
                        {
                            mode = "Reading " + block_start;

                            CollisionMeshBlock nb = new CollisionMeshBlock();
                            colmesh.Blocks.Add(nb);
                            String check = ReadString(b);
                            if (!check.Equals("NParts"))
                                throw new Exception("Binary reader CoCommonBlock parts error " + check);
                            if (colmesh.NBlocks != colmesh.Blocks.Count)
                            {
                                // account for missing [CoCommon] block
                                colmesh.NBlocks = colmesh.Blocks.Count;
                                colmesh.CurrentBlock = 0;
                                colmesh.CurrentPart = 0;
                                MessageBox.Show("Malformed binary :- Missing CoCommon section " + name, "IL2 Modder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            nb.NParts = b.ReadInt16();

                        }
                        #endregion

                        #region CoCommonBlockPart
                        if (bname.Equals(part_start))
                        {
                            mode = "Reading " + part_start;

                            CollisionMeshPart cmp = new CollisionMeshPart();
                            colmesh.Blocks[colmesh.Blocks.Count - 1].Parts.Add(cmp);
                            int j = bfile.table1[i].NumberOfRecords;

                            String check = ReadString3(b);
                            if (!check.Equals("Type"))
                                throw new Exception("Binary reader CoCommonBlock part error " + check);
                            cmp.Type = ReadString3(b);

                            check = ReadString(b);
                            if (!check.Equals("NFrames"))
                                throw new Exception("Binary reader CoCommonBlock part frame error " + check);
                            cmp.NFrames = b.ReadInt16();

                            check = ReadString3(b);
                            if (!check.Equals("Name"))
                                throw new Exception("Binary reader CoCommonBlock part name error " + check);
                            cmp.Name = ReadString3(b);

                            if (j > 3)
                            {
                                check = ReadString3(b);
                                if (!check.Equals("TypeIntExt"))
                                    throw new Exception("Binary reader CoCommonBlock TypeIntExt error " + check);
                                check = ReadString3(b);
                                cmp.TypeIntExt = 1;
                                if (check.Equals("INTERNAL"))
                                    cmp.TypeIntExt = 0;
                            }

                        }
                        #endregion

                        #region [CoVer0_b0p0]
                        if (bname.StartsWith("[CoVer"))
                        {
                            mode = "Reading collision verts";

                            CollisionMeshPart cmp = colmesh.Blocks[colmesh.Blocks.Count - 1].Parts[colmesh.CurrentPart];
                            int j = bfile.table1[i].NumberOfRecords;
                            for (int ii = 0; ii < j; ii++)
                            {
                                switch (bfile.table1[i].Type)
                                {
                                    case 4:
                                        {
                                            float x = b.ReadSingle();
                                            float y = b.ReadSingle();
                                            float z = b.ReadSingle();
                                            cmp.Verts.Add(new Vector3(x, y, z));
                                        }
                                        break;
                                    default:
                                        throw new Exception("Binary reader collision vert type error " + bfile.table1[i].Type);
                                }
                            }
                        }
                        #endregion

                        #region [CoNeiCnt_
                        if (bname.StartsWith("[CoNeiCnt_"))
                        {
                            mode = "Reading collision neighbours";

                            CollisionMeshPart cmp = colmesh.Blocks[colmesh.Blocks.Count - 1].Parts[colmesh.CurrentPart];
                            int j = bfile.table1[i].NumberOfRecords;
                            re = 0;
                            for (int ii = 0; ii < j; ii++)
                            {
                                switch (bfile.table1[i].Type)
                                {
                                    case 1:
                                        {
                                            re = b.ReadByte();
                                            cmp.NeiCount.Add((int)re);
                                        }
                                        break;

                                    case 257:
                                        {
                                            re += b.ReadByte();
                                            cmp.NeiCount.Add((int)re);
                                        }
                                        break;
                                    default:
                                        throw new Exception("Binary reader collision NeiCnt type error " + bfile.table1[i].Type);
                                }
                            }
                        }
                        #endregion

                        #region CoNei_b0p0
                        if (bname.StartsWith("[CoNei_b"))
                        {
                            mode = "Reading collision neighbour parts";

                            CollisionMeshPart cmp = colmesh.Blocks[colmesh.Blocks.Count - 1].Parts[colmesh.CurrentPart];
                            int j = bfile.table1[i].NumberOfRecords;
                            re = 0;
                            for (int ii = 0; ii < j; ii++)
                            {
                                switch (bfile.table1[i].Type)
                                {
                                    case 1:
                                        {
                                            re = b.ReadByte();
                                            cmp.Neighbours.Add((int)re);
                                        }
                                        break;
                                    case 257:
                                        {
                                            re += b.ReadByte();
                                            cmp.Neighbours.Add((int)re);
                                        }
                                        break;
                                    default:
                                        throw new Exception("Binary reader collision Nei type error " + bfile.table1[i].Type);
                                }
                            }
                        }
                        #endregion

                        #region [CoFac_b0p0]
                        if (bname.StartsWith("[CoFac_b"))
                        {
                            mode = "Reading collision faces";

                            CollisionMeshPart cmp = colmesh.Blocks[colmesh.Blocks.Count - 1].Parts[colmesh.CurrentPart];
                            int j = bfile.table1[i].NumberOfRecords;
                            re = 0;
                            for (int ii = 0; ii < j; ii++)
                            {
                                switch (bfile.table1[i].Type)
                                {
                                    case 1:
                                        {
                                            re = b.ReadByte();
                                            cmp.Faces.Add((short)re);
                                            re = b.ReadByte();
                                            cmp.Faces.Add((short)re);
                                            re = b.ReadByte();
                                            cmp.Faces.Add((short)re);
                                        }
                                        break;
                                    case 257:
                                        {
                                            re += b.ReadByte();
                                            cmp.Faces.Add((short)re);
                                            re += b.ReadByte();
                                            cmp.Faces.Add((short)re);
                                            re += b.ReadByte();
                                            cmp.Faces.Add((short)re);
                                        }
                                        break;
                                    default:
                                        throw new Exception("Binary reader collision Nei type error " + bfile.table1[i].Type);
                                }
                            }
                            colmesh.CurrentPart++;
                            if (colmesh.CurrentPart == colmesh.Blocks[colmesh.CurrentBlock].NParts)
                            {
                                colmesh.CurrentPart = 0;
                                colmesh.CurrentBlock++;

                            }
                            block_start = String.Format("[CoCommon_b{0}]", colmesh.CurrentBlock);
                            part_start = String.Format("[CoCommon_b{0}p{1}]", colmesh.CurrentBlock, colmesh.CurrentPart);
                        }
                        #endregion

                        #region Lod materials
                        if ((bname.StartsWith("[LOD")) && (bname.Contains("Materials")))
                        {
                            mode = "Reading lod materials";

                            CurrentAnimationFrame = -1;
                            if (Animated)
                            {
                                lod = new Lod(Animated, FrameCount);
                            }
                            else
                                lod = new Lod();

                            int type = bfile.table1[i].Type;
                            sr = bfile.table1[i].StartRecord;
                            for (int j = 0; j < bfile.table1[i].NumberOfRecords; j++)
                            {
                                int ss = bfile.subsection_size[sr++];
                                String mat = ReadString2(b, ss);
                                if (mat == null)
                                {
                                    Material m = Materials[Lods.Count];
                                    lod.Materials.Add(m);
                                }
                                else
                                {
                                    Material m = new Material(mat, dir);
                                    lod.Materials.Add(m);
                                }
                            }
                            Lods.Add(lod);
                        }
                        #endregion

                        #region Lod face groups
                        if ((bname.StartsWith("[LOD")) && (bname.Contains("FaceGroups")))
                        {
                            mode = "Reading LOD face groups";

                            FaceGroup f;
                            CurrentAnimationFrame = -1;
                            int j = bfile.table1[i].NumberOfRecords;
                            int type = bfile.table1[i].Type;
                            int firstrecord = bfile.table1[i].StartRecord;
                            int size = bfile.subsection_size[firstrecord++];
                            if (size == 2)
                            {
                                re = b.ReadByte();
                                lod.VertexCount = re;
                                re += b.ReadByte();
                                lod.FaceCount = re;
                            }
                            else if (size == 4)
                            {
                                se += b.ReadInt16();
                                lod.VertexCount = se;// b.ReadInt16();
                                se += b.ReadInt16();
                                lod.FaceCount = se;// lod.VertexCount + b.ReadInt16();
                            }
                            else
                            {
                                throw new NotSupportedException("FaceGroups start record is larger than 2 bytes");
                            }
                            lod.Indices = new short[3 * lod.FaceCount];
                            lod.Verts = new VertexPositionNormalTexture[lod.VertexCount];
                            j--;
                            se = (short)lod.FaceCount;
                            while (j > 0)
                            {
                                f = new FaceGroup();

                                size = bfile.subsection_size[firstrecord++];
                                switch (size)
                                {
                                    case 5:
                                        {
                                            re += b.ReadByte();
                                            f.Material = re;
                                            re += b.ReadByte();
                                            f.StartVertex = re;
                                            re += b.ReadByte();
                                            f.VertexCount = re;
                                            re += b.ReadByte();
                                            f.StartFace = re;
                                            re += b.ReadByte();
                                            f.FaceCount = re;
                                        }
                                        break;
                                    case 6:
                                        {
                                            if (bfile.table1[i].Type == 1)
                                            {
                                                f.Material = b.ReadByte();
                                                f.StartVertex = b.ReadByte();
                                                f.VertexCount = b.ReadByte();
                                                f.StartFace = b.ReadByte();
                                                f.FaceCount = b.ReadByte();
                                                re = b.ReadByte();
                                            }
                                            else
                                            {
                                                re += b.ReadByte();
                                                f.Material = re;
                                                re += b.ReadByte();
                                                f.StartVertex = re;
                                                re += b.ReadByte();
                                                f.VertexCount = re;
                                                re += b.ReadByte();
                                                f.StartFace = re;
                                                re += b.ReadByte();
                                                f.FaceCount = re;
                                                re += b.ReadByte();     // not sure what this byte is for
                                            }
                                        }
                                        break;

                                    case 10:
                                        {
                                            se += b.ReadInt16();
                                            f.Material = se;
                                            se += b.ReadInt16();
                                            f.StartVertex = se;
                                            se += b.ReadInt16();
                                            f.VertexCount = se;
                                            se += b.ReadInt16();
                                            f.StartFace = se;
                                            se += b.ReadInt16();
                                            f.FaceCount = se;
                                        }
                                        break;
                                    case 12:
                                        {

                                            se += b.ReadInt16();
                                            f.Material = se;
                                            se += b.ReadInt16();
                                            f.StartVertex = se;
                                            se += b.ReadInt16();
                                            f.VertexCount = se;
                                            se += b.ReadInt16();
                                            f.StartFace = se;
                                            se += b.ReadInt16();
                                            f.FaceCount = se;
                                            se += b.ReadInt16();

                                        }
                                        break;
                                    default:
                                        {
                                            Debug.WriteLine(bfile.table1[i].Type);
                                            throw new NotSupportedException("FaceGroups unknown size");
                                        }
                                }

                                lod.FaceGroups.Add(f);
                                j--;
                            }
                        }
                        #endregion

                        #region Lod vertices
                        if ((bname.StartsWith("[LOD")) && (bname.Contains("_Vertices_Frame")))
                        {
                            mode = "Reading lod vertices";

                            CurrentAnimationFrame++;
                            int j = bfile.table1[i].NumberOfRecords;
                            int firstrecord = bfile.table1[i].StartRecord;
                            int size = bfile.subsection_size[firstrecord++];
                            switch (size)
                            {
                                case 24:
                                    {
                                        for (int ii = 0; ii < j; ii++)
                                        {
                                            VertexPositionNormalTexture vp = new VertexPositionNormalTexture();
                                            vp.Position.X = b.ReadSingle();
                                            vp.Position.Y = b.ReadSingle();
                                            vp.Position.Z = b.ReadSingle();
                                            vp.Normal.X = b.ReadSingle();
                                            vp.Normal.Y = b.ReadSingle();
                                            vp.Normal.Z = b.ReadSingle();
                                            if (Animated)
                                            {
                                                lod.animation_frames[CurrentAnimationFrame].Add(vp);
                                            }
                                            else
                                            {
                                                lod.Verts[ii] = vp;
                                            }
                                        }
                                    }
                                    break;
                                case 18:
                                    {
                                        for (int ii = 0; ii < j; ii++)
                                        {
                                            VertexPositionNormalTexture vp = new VertexPositionNormalTexture();
                                            vp.Position.X = ReadTriple(b);
                                            vp.Position.Y = ReadTriple(b);
                                            vp.Position.Z = ReadTriple(b);
                                            vp.Normal.X = ReadTriple(b);
                                            vp.Normal.Y = ReadTriple(b);
                                            vp.Normal.Z = ReadTriple(b);
                                            if (Animated)
                                            {
                                                lod.animation_frames[CurrentAnimationFrame].Add(vp);
                                            }
                                            else
                                            {
                                                lod.Verts[ii] = vp;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    throw new Exception("Unhandled vertex size " + size);
                            }
                        }
                        #endregion

                        #region Lod UVS
                        if ((bname.StartsWith("[LOD")) && (bname.Contains("MaterialMapping")))
                        {
                            mode = "Reading lod material mapping";

                            CurrentAnimationFrame = -1;
                            int j = bfile.table1[i].NumberOfRecords;
                            byte vre = 0;
                            for (int ii = 0; ii < j; ii++)
                            {
                                switch (bfile.table1[i].Type)
                                {
                                    case 257:
                                        {
                                            vre += b.ReadByte();
                                            lod.Verts[ii].TextureCoordinate.X = (float)vre;
                                            vre += b.ReadByte();
                                            lod.Verts[ii].TextureCoordinate.Y = (float)vre;
                                        }
                                        break;
                                    case 3:
                                        {
                                            lod.Verts[ii].TextureCoordinate.X = ReadTriple(b);
                                            lod.Verts[ii].TextureCoordinate.Y = ReadTriple(b);
                                        }
                                        break;
                                    case 4:
                                        {
                                            lod.Verts[ii].TextureCoordinate.X = b.ReadSingle();
                                            lod.Verts[ii].TextureCoordinate.Y = b.ReadSingle();
                                        }
                                        break;
                                    default:
                                        throw new Exception("MaterialMapping size not supported " + bfile.table1[i].Type);

                                }
                            }
                        }
                        #endregion

                        #region Lod faces
                        if ((bname.StartsWith("[LOD")) && (bname.Contains("_Faces")))
                        {
                            mode = "Reading lod faces";

                            int j = bfile.table1[i].NumberOfRecords;
                            switch (bfile.table1[i].Type)
                            {
                                case 1:
                                case 0x0101:
                                    {
                                        re = 0;
                                        int k = 0;
                                        for (int ii = 0; ii < j; ii++)
                                        {
                                            re += b.ReadByte();
                                            lod.Indices[k++] = (short)re;
                                            re += b.ReadByte();
                                            lod.Indices[k++] = (short)re;
                                            re += b.ReadByte();
                                            lod.Indices[k++] = (short)re;
                                        }
                                    }
                                    break;
                                case 258:
                                case 0x02:
                                    {
                                        se = 0;
                                        int k = 0;
                                        for (int jj = 0; jj < bfile.table1[i].NumberOfRecords; jj++)
                                        {
                                            se += b.ReadInt16();
                                            lod.Indices[k++] = se;
                                            se += b.ReadInt16();
                                            lod.Indices[k++] = se;
                                            se += b.ReadInt16();
                                            lod.Indices[k++] = se;
                                        }
                                    }
                                    break;


                                default:
                                    throw new Exception("Unhandled face size " + bfile.table1[i].Type);
                            }
                        }
                        #endregion

                        #region Lod shadow verts
                        if ((bname.StartsWith("[LOD")) && (bname.Contains("ShVertices_Frame")))
                        {
                            mode = "Reading lod shadow verts";

                            CurrentAnimationFrame++;
                            int j = bfile.table1[i].NumberOfRecords;
                            VertexPositionColor v = new VertexPositionColor();
                            for (int ii = 0; ii < j; ii++)
                            {
                                switch (bfile.table1[i].Type)
                                {
                                    case 3:
                                        {
                                            int a1 = b.ReadByte();
                                            int a2 = b.ReadByte();
                                            int a3 = b.ReadByte();
                                            int a4 = 0;
                                            if ((a3 & 128) != 0)
                                                a4 = 255;
                                            a3 = (a4 << 24) + (a3 << 16) + (a2 << 8) + a1;
                                            v.Position.X = (float)(a3 / 65536.0f);

                                            a1 = b.ReadByte();
                                            a2 = b.ReadByte();
                                            a3 = b.ReadByte();
                                            a4 = 0;
                                            if ((a3 & 128) != 0)
                                                a4 = 255;
                                            a3 = (a4 << 24) + (a3 << 16) + (a2 << 8) + a1;
                                            v.Position.Y = (float)(a3 / 65536.0f);

                                            a1 = b.ReadByte();
                                            a2 = b.ReadByte();
                                            a3 = b.ReadByte();
                                            a4 = 0;
                                            if ((a3 & 128) != 0)
                                                a4 = 255;
                                            a3 = (a4 << 24) + (a3 << 16) + (a2 << 8) + a1;
                                            v.Position.Z = (float)(a3 / 65536.0f);
                                        }
                                        break;
                                    default:
                                        throw new Exception("Unhandled shadow vert type");
                                }
                                v.Color = Microsoft.Xna.Framework.Color.White;
                                if (Animated)
                                {
                                    lod.animation_shadow_frames[CurrentAnimationFrame].Add(v);
                                }
                                else
                                    lod.ShadowVerts.Add(v);
                            }
                            lod.ShadowArray = lod.ShadowVerts.ToArray();
                        }
                        #endregion

                        #region Lod shadow faces
                        if ((bname.StartsWith("[LOD")) && (bname.Contains("ShFaces")))
                        {
                            mode = "Reading lod shadow faces";

                            int j = bfile.table1[i].NumberOfRecords;
                            switch (bfile.table1[i].Type)
                            {
                                case 1:
                                case 0x0101:
                                    {
                                        re = 0;
                                        for (int ii = 0; ii < j; ii++)
                                        {
                                            re += b.ReadByte();
                                            lod.ShadowIndices.Add((short)re);
                                            re += b.ReadByte();
                                            lod.ShadowIndices.Add((short)re);
                                            re += b.ReadByte();
                                            lod.ShadowIndices.Add((short)re);
                                        }
                                    }
                                    break;
                                case 258:
                                case 0x02:
                                    {
                                        for (int jj = 0; jj < bfile.table1[i].NumberOfRecords; jj++)
                                        {
                                            se += b.ReadInt16();
                                            lod.ShadowIndices.Add((short)se);
                                            se += b.ReadInt16();
                                            lod.ShadowIndices.Add((short)se);
                                            se += b.ReadInt16();
                                            lod.ShadowIndices.Add((short)se);
                                        }
                                    }
                                    break;
                                default:
                                    throw new Exception("Shadow faces unhandled size " + bfile.table1[i].Type);
                            }
                            lod.ShadowIndicesArray = lod.ShadowIndices.ToArray();
                        }
                        #endregion
                    }
                    b.Close();
                    if (LodDistances.Count == 0)
                    {
                        LodDistances.Add(12000);
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    MSHLoaderErrorHandler msh = new MSHLoaderErrorHandler();
                    msh.SetMode(1);
                    msh.SetErrorText(e.ToString());
                    msh.SetData(mode);
                    msh.SetErrorDescription(bname);
                    msh.SetFilename(name);
                    msh.ShowDialog();
                }
            }
        }

        private float ReadMotorola(BinaryReader b)
        {
            float res = 0;
            byte t = b.ReadByte();
            if (t == 0)
            {
                byte l = b.ReadByte();
                byte h = b.ReadByte();
                short v = (short)((short)((short)h * 256) + ((short)l));
                res = (float)v;
            }
            else
            {
                res = b.ReadSingle();
            }
            return res;
        }

        private String ReadString(BinaryReader reader)
        {
            String result;
            byte nchars = reader.ReadByte();
            nchars--;
            byte[] chars = new byte[nchars];
            for (int i = 0; i < nchars; i++)
            {
                chars[i] = reader.ReadByte();
            }
            result = System.Text.Encoding.Default.GetString(chars);
            reader.ReadByte();
            return result;
        }

        private String ReadSplitString(BinaryReader reader, int size)
        {
            String result = "";
            while (size > 0)
            {
                String next;
                byte nchars = reader.ReadByte();
                size -= nchars;
                nchars--;

                byte[] chars = new byte[nchars];
                for (int i = 0; i < nchars; i++)
                {
                    chars[i] = reader.ReadByte();
                }
                next = System.Text.Encoding.Default.GetString(chars);

                result += " " + next;

            }
            return result;
        }

        private String ReadString2(BinaryReader reader, int nchars)
        {
            String result;
            byte t = reader.ReadByte();
            //Debug.WriteLine("String length {0}  spare byte {1}", nchars, t);
            nchars--;
            if (t == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    reader.ReadByte();
                }
                return null;
            }
            byte[] chars = new byte[nchars];
            for (int i = 0; i < nchars; i++)
            {
                chars[i] = reader.ReadByte();
            }
            result = System.Text.Encoding.Default.GetString(chars);
            return result;
        }

        private String ReadString3(BinaryReader reader)
        {
            String result;
            byte nchars = reader.ReadByte();
            nchars--;
            byte[] chars = new byte[nchars];
            for (int i = 0; i < nchars; i++)
            {
                chars[i] = reader.ReadByte();
            }
            result = System.Text.Encoding.Default.GetString(chars);
            return result;
        }

        private float ReadTriple(BinaryReader b)
        {
            float res = 0;
            int a1 = b.ReadByte();
            int a2 = b.ReadByte();
            int a3 = b.ReadByte();
            int a4 = 0;
            if ((a3 & 128) != 0)
                a4 = 255;
            a3 = (a4 << 24) + (a3 << 16) + (a2 << 8) + a1;
            res = (float)a3 / 65536.0f;
            return res;
        }

        private float ReadDouble(BinaryReader b)
        {
            float res = 0;
            int a1 = 0;
            int a2 = b.ReadByte();
            int a3 = b.ReadByte();
            int a4 = 0;
            if ((a3 & 128) != 0)
                a4 = 255;
            a3 = (a4 << 24) + (a3 << 16) + (a2 << 8) + a1;
            res = (float)a3 / 65536.0f;
            return res;
        }
        #endregion

        #region Serializers
        public void SaveShadows(TextWriter tw)
        {
            if (ShadowVerts.Count > 0)
            {
                tw.WriteLine("[ShVertices_Frame0]");
                foreach (VertexPositionColor vp in ShadowVerts)
                {
                    tw.WriteLine(String.Format("{0} {1} {2}", vp.Position.X, vp.Position.Y, vp.Position.Z));
                }
                tw.WriteLine("");
                tw.WriteLine("[ShFaces]");
                for (int i = 0; i < ShadowIndices.Count; i += 3)
                {
                    tw.WriteLine(String.Format("{0} {1} {2}", ShadowIndices[i], ShadowIndices[i + 1], ShadowIndices[i + 2]));
                }
                tw.WriteLine("");
            }
            if (Animated)
            {
                for (int i = 0; i < FrameCount; i++)
                {
                    tw.WriteLine(String.Format("[ShVertices_Frame{0}]", i));
                    foreach (VertexPositionColor v in animated_shadow_frames[i])
                    {
                        tw.WriteLine(String.Format("{0} {1} {2}", v.Position.X, v.Position.Y, v.Position.Z));
                    }
                }
                tw.WriteLine("");
                tw.WriteLine("[ShFaces]");
                for (int i = 0; i < ShadowIndices.Count; i += 3)
                {
                    tw.WriteLine(String.Format("{0} {1} {2}", ShadowIndices[i], ShadowIndices[i + 1], ShadowIndices[i + 2]));
                }
                tw.WriteLine("");
            }
        }

        public void SaveLodTable(TextWriter tw)
        {
            tw.WriteLine("[LOD]");
            foreach (int i in LodDistances)
            {
                tw.WriteLine(String.Format("{0}", i));
            }
            tw.WriteLine("");
        }

        public void SaveMaterials(TextWriter tw)
        {
            tw.WriteLine("[Materials]");
            foreach (Material m in Materials)
            {
                String f = m.Name.Replace('"', '_');
                tw.WriteLine(f);
            }
            tw.WriteLine("");
        }

        public void SaveHeader(TextWriter tw)
        {
            tw.WriteLine("//Generated by Stainless");
            tw.WriteLine("");
            tw.WriteLine("[Common]");
            tw.WriteLine("NumBones 0");
            if (Animated)
            {
                tw.WriteLine(" FramesType Animation");
                tw.WriteLine(String.Format(" NumFrames {0}", FrameCount));
            }
            else
            {
                tw.WriteLine(" FramesType Single");
                tw.WriteLine(" NumFrames 1");
            }
            tw.WriteLine("");

        }

        public void SaveHooks(TextWriter tw)
        {
            tw.WriteLine("//  " + mesh_name);

            if (Hooks.Count > 0)
            {
                tw.WriteLine("[Hooks]");
                if (Animated)
                {
                    tw.WriteLine(Hooks[0].Name.Replace((char)7, ' '));
                }
                else
                {
                    foreach (Hook h in Hooks)
                    {
                        tw.WriteLine(h.Name.Replace((char)7, ' '));
                    }
                }
                tw.WriteLine("");
                tw.WriteLine("[HookLoc]");

               
                foreach (Hook h in Hooks)
                {
                    Vector3 res = Vector3.Transform(Vector3.Zero, h.matrix);

                    tw.Write(String.Format("{0} ", h.matrix.M11));
                    tw.Write(String.Format("{0} ", h.matrix.M12));
                    tw.Write(String.Format("{0} ", h.matrix.M13));
                    tw.Write(String.Format("{0} ", h.matrix.M21));
                    tw.Write(String.Format("{0} ", h.matrix.M22));
                    tw.Write(String.Format("{0} ", h.matrix.M23));
                    tw.Write(String.Format("{0} ", h.matrix.M31));
                    tw.Write(String.Format("{0} ", h.matrix.M32));
                    tw.Write(String.Format("{0} ", h.matrix.M33));
                    tw.Write(String.Format("{0} ", h.matrix.M41));
                    tw.Write(String.Format("{0} ", h.matrix.M42));              
                    tw.Write(String.Format("{0}", h.matrix.M43));

                    tw.WriteLine(String.Format("      {0} ", res));
                }
                tw.WriteLine("");
            }
        }

        public void SaveFaceGroups(TextWriter tw)
        {
            tw.WriteLine("[FaceGroups]");
            tw.WriteLine(String.Format("{0} {1}", Verts.Length, FaceCount));
            foreach (FaceGroup f in FaceGroups)
            {
                tw.WriteLine(String.Format("{0} {1} {2} {3} {4} 0", f.Material, f.StartVertex, f.VertexCount, f.StartFace, f.FaceCount));
            }
            tw.WriteLine("");
        }

        public void SaveFrame0(TextWriter tw)
        {
            if (Animated)
            {
                for (int i = 0; i < FrameCount; i++)
                {
                    tw.WriteLine(String.Format("[Vertices_Frame{0}]", i));
                    foreach (VertexPositionNormalTexture v in animation_frames[i])
                    {
                        tw.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", v.Position.X, v.Position.Y, v.Position.Z, v.Normal.X, v.Normal.Y, v.Normal.Z));
                    }
                    tw.WriteLine("");
                }
            }
            else
            {
                tw.WriteLine("[Vertices_Frame0]");
                foreach (VertexPositionNormalTexture v in Verts)
                {
                    tw.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", v.Position.X, v.Position.Y, v.Position.Z, v.Normal.X, v.Normal.Y, v.Normal.Z));
                }
                tw.WriteLine("");
            }
        }

        public void SaveUVs(TextWriter tw)
        {
            tw.WriteLine("[MaterialMapping]");
            foreach (VertexPositionNormalTexture v in Verts)
            {
                tw.WriteLine(String.Format("{0} {1}", v.TextureCoordinate.X, v.TextureCoordinate.Y));
            }
            tw.WriteLine("");
        }

        public void SaveFaces(TextWriter tw)
        {
            tw.WriteLine("[Faces]");
            for (int i = 0; i < indices.GetLength(0); i += 3)
            {
                tw.WriteLine("{0} {1} {2}", indices[i], indices[i + 1], indices[i + 2]);
            }
            tw.WriteLine("");
        }

        public void SaveLods(TextWriter tw)
        {

            for (int i = 0; i < Lods.Count; i++)
            {
                tw.WriteLine(String.Format("[LOD{0}_Materials]", i + 1));
                foreach (Material m in Lods[i].Materials)
                {
                    tw.WriteLine(m.Name);
                }
                tw.WriteLine("");

                tw.WriteLine(String.Format("[LOD{0}_FaceGroups]", i + 1));
                tw.WriteLine(String.Format("{0} {1}", Lods[i].VertexCount, Lods[i].FaceCount));
                foreach (FaceGroup f in Lods[i].FaceGroups)
                {
                    tw.WriteLine(String.Format("{0} {1} {2} {3} {4} 0", f.Material, f.StartVertex, f.VertexCount, f.StartFace, f.FaceCount));
                }
                tw.WriteLine("");

                if (Animated)
                {
                    for (int j = 0; j < FrameCount; j++)
                    {
                        tw.WriteLine(String.Format("[LOD{0}_Vertices_Frame{1}]", i + 1, j));
                        foreach (VertexPositionNormalTexture v in Lods[i].animation_frames[j])
                        {
                            tw.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", v.Position.X, v.Position.Y, v.Position.Z, v.Normal.X, v.Normal.Y, v.Normal.Z));
                        }
                        tw.WriteLine("");
                    }
                }
                else
                {
                    tw.WriteLine(String.Format("[LOD{0}_Vertices_Frame0]", i + 1));
                    foreach (VertexPositionNormalTexture v in Lods[i].Verts)
                    {
                        tw.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", v.Position.X, v.Position.Y, v.Position.Z, v.Normal.X, v.Normal.Y, v.Normal.Z));
                    }
                    tw.WriteLine("");
                }

                tw.WriteLine(String.Format("[LOD{0}_MaterialMapping]", i + 1));
                foreach (VertexPositionNormalTexture v in Lods[i].Verts)
                {
                    tw.WriteLine(String.Format("{0} {1}", v.TextureCoordinate.X, v.TextureCoordinate.Y));
                }
                tw.WriteLine("");

                tw.WriteLine(String.Format("[LOD{0}_Faces]", i + 1));
                for (int j = 0; j < Lods[i].Indices.GetLength(0); j += 3)
                {
                    tw.WriteLine(String.Format("{0} {1} {2}", Lods[i].Indices[j], Lods[i].Indices[j + 1], Lods[i].Indices[j + 2]));
                }
                tw.WriteLine("");

                if (Lods[i].ShadowIndicesArray != null)
                {
                    if (Animated)
                    {
                        for (int j = 0; j < FrameCount; j++)
                        {
                            tw.WriteLine(String.Format("[LOD{0}_ShVertices_Frame{1}]", i + 1, j));
                            foreach (VertexPositionColor v in Lods[i].animation_shadow_frames[j])
                            {
                                tw.WriteLine(String.Format("{0} {1} {2}", v.Position.X, v.Position.Y, v.Position.Z));
                            }
                            tw.WriteLine("");
                        }
                    }
                    else
                    {
                        tw.WriteLine(String.Format("[LOD{0}_ShVertices_Frame0]", i + 1));
                        foreach (VertexPositionColor v in Lods[i].ShadowVerts)
                        {
                            tw.WriteLine(String.Format("{0} {1} {2}", v.Position.X, v.Position.Y, v.Position.Z));
                        }
                        tw.WriteLine("");
                    }


                    tw.WriteLine(String.Format("[LOD{0}_ShFaces]", i + 1));
                    for (int j = 0; j < Lods[i].ShadowIndicesArray.GetLength(0); j += 3)
                    {
                        tw.WriteLine(String.Format("{0} {1} {2}", Lods[i].ShadowIndicesArray[j], Lods[i].ShadowIndicesArray[j + 1], Lods[i].ShadowIndicesArray[j + 2]));
                    }
                }

                tw.WriteLine("");
            }
        }

        public void SaveMaterials(String dir)
        {
            foreach (Material m in Materials)
            {
                m.Serialise(dir);
            }
            foreach (Lod l in Lods)
            {
                l.SaveMaterials(dir);
            }
        }

        public void SaveCollisionMesh(TextWriter tw)
        {
            if (colmesh.NBlocks > 0)
            {
                tw.WriteLine("[CoCommon]");
                tw.WriteLine(String.Format("NBlocks {0}", colmesh.NBlocks));
                tw.WriteLine("");
                for (int i = 0; i < colmesh.NBlocks; i++)
                {
                    CollisionMeshBlock cmb = colmesh.Blocks[i];
                    tw.WriteLine(String.Format("[CoCommon_b{0}]", i));
                    tw.WriteLine(String.Format("NParts {0}", cmb.NParts));
                    tw.WriteLine("");
                    for (int j = 0; j < cmb.NParts; j++)
                    {
                        tw.WriteLine(String.Format("[CoCommon_b{0}p{1}]", i, j));

                        CollisionMeshPart cmp = cmb.Parts[j];
                        tw.WriteLine(String.Format("Type {0}", cmp.Type));
                        tw.WriteLine(String.Format("NFrames {0}", cmp.NFrames));
                        tw.WriteLine(String.Format("Name {0}", cmp.Name));
                        tw.WriteLine("");

                        int VertsPerFrame = cmp.Verts.Count / cmp.NFrames;
                        int Nv = 0;
                        for (int frame = 0; frame < cmp.NFrames; frame++)
                        {
                            tw.WriteLine(String.Format("[CoVer{0}_b{1}p{2}]", frame, i, j));
                            for (int tv = 0; tv < VertsPerFrame; tv++)
                            {
                                Vector3 v = cmp.Verts[tv + Nv];
                                tw.WriteLine(String.Format("{0} {1} {2}", v.X, v.Y, v.Z));
                            }
                            Nv += VertsPerFrame;
                        }
                        tw.WriteLine("");
                        tw.WriteLine(String.Format("[CoNeiCnt_b{0}p{1}]", i, j));
                        foreach (int s in cmp.NeiCount)
                        {
                            tw.WriteLine(String.Format("{0}", s));
                        }
                        tw.WriteLine("");
                        tw.WriteLine(String.Format("[CoNei_b{0}p{1}]", i, j));
                        foreach (int s in cmp.Neighbours)
                        {
                            tw.WriteLine(String.Format("{0}", s));
                        }
                        tw.WriteLine("");
                        tw.WriteLine(String.Format("[CoFac_b{0}p{1}]", i, j));
                        for (int s = 0; s < cmp.Faces.Count; s += 3)
                        {
                            tw.WriteLine(String.Format("{0} {1} {2}", cmp.Faces[s], cmp.Faces[s + 1], cmp.Faces[s + 2]));
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Export to fox1
        /// </summary>
        /// <param name="path"></param>
        public void ExportCollision(String path)
        {
            FileStream writeStream = new FileStream(path, FileMode.Create);
            BinaryWriter wb = new BinaryWriter(writeStream);
            wb.Write(colmesh.NBlocks);
            if (colmesh.NBlocks > 0)
            {
                for (int i = 0; i < colmesh.NBlocks; i++)
                {
                    CollisionMeshBlock cmb = colmesh.Blocks[i];
                    cmb.SaveToFox1(wb);
                }
            }
            wb.Close();
            writeStream.Close();

        }

        public void SaveDAEEffects(TextWriter tw, bool effect)
        {
            foreach (Material m in Materials)
            {
                m.SaveDAEEffects(tw, effect);
            }
        }

        public void SaveDAEMesh(TextWriter tw)
        {
            tw.WriteLine(String.Format("\t\t<geometry id=\"{0}-mesh\">", mesh_name));
            tw.WriteLine("\t\t\t<mesh>");
            tw.WriteLine(String.Format("\t\t\t\t<source id=\"{0}-mesh-positions\">", mesh_name));
            tw.WriteLine(String.Format("\t\t\t\t\t<float_array id=\"{0}-mesh-positions-array\" count=\"{1}\">", mesh_name, Verts.GetLength(0) * 3));
            for (int i = 0; i < Verts.GetLength(0); i++)
            {
                tw.Write(String.Format("{0} {1} {2} ", Verts[i].Position.X, Verts[i].Position.Y, Verts[i].Position.Z));
            }
            tw.WriteLine("");
            tw.WriteLine("\t\t\t\t\t<technique_common>");
            tw.WriteLine(String.Format("\t\t\t\t\t\t<accessor source=\"#{0}-mesh-positions-array\" count=\"{1}\" stride=\"3\">", mesh_name, Verts.GetLength(0)));
            tw.WriteLine("\t\t\t\t\t\t\t<param name=\"X\" type=\"float\"/>");
            tw.WriteLine("\t\t\t\t\t\t\t<param name=\"Y\" type=\"float\"/>");
            tw.WriteLine("\t\t\t\t\t\t\t<param name=\"Z\" type=\"float\"/>");
            tw.WriteLine("\t\t\t\t\t\t</accessor>");
            tw.WriteLine("\t\t\t\t\t</technique_common>");
            tw.WriteLine("\t\t\t\t</source>");

            tw.WriteLine(String.Format("\t\t\t\t<source id=\"{0}-mesh-normals\">", mesh_name));
            tw.WriteLine(String.Format("\t\t\t\t\t<float_array id=\"{0}-mesh-normals-array\" count=\"{1}\">", mesh_name, Verts.GetLength(0) * 3));
            for (int i = 0; i < Verts.GetLength(0); i++)
            {
                tw.Write(String.Format("{0} {1} {2} ", Verts[i].Normal.X, Verts[i].Normal.Y, Verts[i].Normal.Z));
            }
            tw.WriteLine("");
            tw.WriteLine("\t\t\t\t\t<technique_common>");
            tw.WriteLine(String.Format("\t\t\t\t\t\t<accessor source=\"#{0}-mesh-normals-array\" count=\"{1}\" stride=\"3\">", mesh_name, Verts.GetLength(0)));
            tw.WriteLine("\t\t\t\t\t\t\t<param name=\"X\" type=\"float\"/>");
            tw.WriteLine("\t\t\t\t\t\t\t<param name=\"Y\" type=\"float\"/>");
            tw.WriteLine("\t\t\t\t\t\t\t<param name=\"Z\" type=\"float\"/>");
            tw.WriteLine("\t\t\t\t\t\t</accessor>");
            tw.WriteLine("\t\t\t\t\t</technique_common>");
            tw.WriteLine("\t\t\t\t</source>");

            tw.WriteLine(String.Format("\t\t\t\t<source id=\"{0}-mesh-map\">", mesh_name));
            tw.WriteLine(String.Format("\t\t\t\t\t<float_array id=\"{0}-mesh-map-array\" count=\"{1}\">", mesh_name, Verts.GetLength(0) * 3));
            for (int i = 0; i < Verts.GetLength(0); i++)
            {
                tw.Write(String.Format("{0} {1} ", Verts[i].TextureCoordinate.X, Verts[i].TextureCoordinate.Y));
            }
            tw.WriteLine("");
            tw.WriteLine("\t\t\t\t\t<technique_common>");
            tw.WriteLine(String.Format("\t\t\t\t\t\t<accessor source=\"#{0}-mesh-map-array\" count=\"{1}\" stride=\"2\">", mesh_name, Verts.GetLength(0)));
            tw.WriteLine("\t\t\t\t\t\t\t<param name=\"S\" type=\"float\"/>");
            tw.WriteLine("\t\t\t\t\t\t\t<param name=\"T\" type=\"float\"/>");
            tw.WriteLine("\t\t\t\t\t\t</accessor>");
            tw.WriteLine("\t\t\t\t\t</technique_common>");
            tw.WriteLine("\t\t\t\t</source>");

            tw.WriteLine(String.Format("\t\t\t\t<vertices id=\"{0}-mesh-vertices\">", mesh_name));
            tw.WriteLine(String.Format("\t\t\t\t\t<input semantic=\"POSITION\" source=\"#{0}-mesh-positions\"/>", mesh_name));
            tw.WriteLine("\t\t\t\t</vertices>");

            foreach (FaceGroup f in FaceGroups)
            {
                tw.WriteLine(String.Format("\t\t\t\t<polylist material=\"{0}\" count=\"{1}\">", Materials[f.Material].Name, f.FaceCount));
                tw.WriteLine(String.Format("\t\t\t\t<input semantic=\"VERTEX\" source=\"#{0}-mesh-vertices\" offset=\"0\"/>", mesh_name));
                tw.WriteLine(String.Format("\t\t\t\t<input semantic=\"NORMAL\" source=\"#{0}-mesh-normals\" offset=\"1\"/>", mesh_name));
                tw.WriteLine(String.Format("\t\t\t\t<input semantic=\"TEXCOORD\" source=\"#{0}-mesh-map-0\" offset=\"2\" set=\"0\"/>", mesh_name));
                tw.WriteLine("\t\t\t\t<vcount>");
                tw.Write("\t\t\t\t");
                for (int i = 0; i < f.FaceCount; i++)
                {
                    tw.Write("3 ");
                }
                tw.WriteLine("\t\t\t\t</vcount>");
                tw.WriteLine("\t\t\t\t<p>");
                tw.Write("\t\t\t\t\t");

                for (int i = 0; i < f.FaceCount; i++)
                {
                    int pos = (f.StartFace + i) * 3;
                    int vert = pos;
                    tw.Write(String.Format("{0} {1} {2} ", indices[vert] + f.StartVertex, indices[vert + 1] + f.StartVertex, indices[vert + 2] + f.StartVertex));

                }
                tw.WriteLine("\t\t\t\t\t</p>");
                tw.WriteLine("\t\t\t\t</polylist>");
            }
            tw.WriteLine("\t\t\t</mesh>");
            tw.WriteLine("\t\t</geometry>");
        }

        public void SaveOGRE(String dir)
        {
            String name = Path.Combine(dir, mesh_name);
            name += ".xml";
            using (TextWriter writer = File.CreateText(name))
            {
                writer.WriteLine("<mesh>");
                writer.WriteLine("<submeshes>");
                foreach (FaceGroup f in FaceGroups)
                {
                    String tname = Materials[f.Material].tname;
                    writer.WriteLine(String.Format("<submesh material=\"{0}\" usesharedvertices=\"false\" use32bitindexes=\"false\" operationtype=\"triangle_list\">", tname));
                    writer.WriteLine(String.Format("<faces count=\"{0}\">", f.FaceCount));
                    for (int j = 0; j < f.FaceCount; j++)
                    {
                        int pos = (f.StartFace + j) * 3;
                        writer.WriteLine(String.Format("<face v1=\"{0}\" v2=\"{1}\" v3=\"{2}\" />",
                            indices[pos],
                            indices[pos + 1],
                            indices[pos + 2]));
                    }
                    writer.WriteLine("</faces>");
                    writer.WriteLine(String.Format("<geometry vertexcount=\"{0}\">", f.VertexCount));
                    writer.WriteLine("<vertexbuffer positions=\"true\" normals=\"true\" texture_coord_dimensions_0=\"float2\" texture_coords=\"1\">");
                    for (int i = f.StartVertex; i < f.StartVertex + f.VertexCount; i++)
                    {
                        writer.WriteLine("<vertex>");
                        writer.WriteLine(String.Format("<position x=\"{0}\" y=\"{1}\" z=\"{2}\" />", Verts[i].Position.X, Verts[i].Position.Y, Verts[i].Position.Z));
                        writer.WriteLine(String.Format("<normal x=\"{0}\" y=\"{1}\" z=\"{2}\" />", Verts[i].Normal.X, Verts[i].Normal.Y, Verts[i].Normal.Z));
                        writer.WriteLine(String.Format("<texcoord u=\"{0}\" v=\"~{1}\" />", Verts[i].TextureCoordinate.X, Verts[i].TextureCoordinate.Y));
                        writer.WriteLine("</vertex>");

                    }
                    writer.WriteLine("</vertexbuffer>");
                    writer.WriteLine("</geometry>");
                    writer.WriteLine("</submesh>");

                }
                writer.WriteLine("</submeshes>");
                writer.WriteLine("</mesh>");
                writer.Close();
            }
        }

        public void SaveAsOBJ(String dir, String matname)
        {
            String name = Path.Combine(dir, mesh_name);
            name += ".obj";
            using (TextWriter writer = File.CreateText(name))
            {
                writer.WriteLine("mtllib " + matname + ".mtl");
                writer.WriteLine("");
                writer.WriteLine("");
                for (int i = 0; i < Verts.Length; i++)
                {
                    writer.WriteLine(String.Format("v {0} {1} {2}", Verts[i].Position.X, Verts[i].Position.Y, Verts[i].Position.Z));
                    writer.WriteLine(String.Format("vn {0} {1} {2}", Verts[i].Normal.X, Verts[i].Normal.Y, Verts[i].Normal.Z));
                    writer.WriteLine(String.Format("vt {0} {1}", Verts[i].TextureCoordinate.X, Verts[i].TextureCoordinate.Y));
                }
                foreach (FaceGroup f in FaceGroups)
                {
                    writer.WriteLine(String.Format("g group_{0}", Materials[f.Material].Name));
                    writer.WriteLine(String.Format("usemtl {0}", Materials[f.Material].Name));
                    writer.WriteLine("");
                    int si = f.StartFace * 3;
                    for (int i = 0; i < f.FaceCount; i++)
                    {
                        int v1, v2, v3;
                        v1 = 1 + f.StartVertex + indices[si++];
                        v2 = 1 + f.StartVertex + indices[si++];
                        v3 = 1 + f.StartVertex + indices[si++];

                        writer.WriteLine(String.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", v1, v2, v3));

                    }
                    writer.WriteLine("");

                    Material m = Materials[f.Material];
                    if (!ObjectViewer.saved_materials.Contains(m.Name))
                    {
                        ObjectViewer.saved_materials.Add(m.Name);
                        m.Saveobj(ObjectViewer.material_writer);
                    }
                }
                writer.Close();
            }
        }

        public void SaveToFox1(String dir, Node n, TextWriter g)
        {
            String filename = Path.Combine(dir, n.Name);
            filename += ".meshpart";
            FileStream writeStream = new FileStream(filename, FileMode.Create);
            BinaryWriter writeBinary = new BinaryWriter(writeStream);

            writeBinary.Write((int)FaceGroups.Count);
            foreach (FaceGroup f in FaceGroups)
            {
                f.Write(writeBinary);
            }

            writeBinary.Write((int)indices.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                writeBinary.Write(indices[i]);
            }

            writeBinary.Write((int)Verts.Length);
            for (int i = 0; i < Verts.Length; i++)
            {
                VertexPositionNormalTexture v = Verts[i];
                writeBinary.Write(v.Position.X);
                writeBinary.Write(v.Position.Y);
                writeBinary.Write(v.Position.Z);

                writeBinary.Write(v.Normal.X);
                writeBinary.Write(v.Normal.Y);
                writeBinary.Write(v.Normal.Z);

                writeBinary.Write(v.TextureCoordinate.X);
                writeBinary.Write(v.TextureCoordinate.Y);

            }

            writeBinary.Write(n.base_matrix.M11);
            writeBinary.Write(n.base_matrix.M12);
            writeBinary.Write(n.base_matrix.M13);
            writeBinary.Write(n.base_matrix.M14);

            writeBinary.Write(n.base_matrix.M21);
            writeBinary.Write(n.base_matrix.M22);
            writeBinary.Write(n.base_matrix.M23);
            writeBinary.Write(n.base_matrix.M24);

            writeBinary.Write(n.base_matrix.M31);
            writeBinary.Write(n.base_matrix.M32);
            writeBinary.Write(n.base_matrix.M33);
            writeBinary.Write(n.base_matrix.M34);

            writeBinary.Write(n.base_matrix.M41);
            writeBinary.Write(n.base_matrix.M42);
            writeBinary.Write(n.base_matrix.M43);
            writeBinary.Write(n.base_matrix.M44);

            writeBinary.Close();



            filename = Path.Combine(dir, n.Name);
            filename += ".materials";
            writeStream = new FileStream(filename, FileMode.Create);
            writeBinary = new BinaryWriter(writeStream);
            writeBinary.Write(Materials.Count);
            for (int i = 0; i < Materials.Count; i++)
            {
                Materials[i].SaveToFox1(writeBinary);
            }
            writeBinary.Close();


            


        }

        public void AddCollisionMeshComponent(String dir, Node n, TextWriter g)
        {
            // Export collision mesh components
            if (colmesh.NBlocks > 0)
            {
                g.WriteLine(n.Name + "collision");
                g.WriteLine("CollisionMeshComponent");
                g.WriteLine("1");
                g.WriteLine("Root#Root#" + n.Name + ":GameComponents");
                g.WriteLine("0");
                g.WriteLine("1");
                g.WriteLine("Filename String " + Path.Combine(dir, n.Name + ".collision"));
            }
        }
        #endregion

        #region Tests

        public bool Shoot(int x, int y, Matrix projection, Matrix view, Matrix world)
        {
            bool result = false;
            float? distance;
            Ray r = CreateRay(x, y, projection, view, world);
            int j = 0;
            for (int k = 0; k < colmesh.NBlocks; k++)
            {
                foreach (CollisionMeshPart p in colmesh.Blocks[k].Parts)
                {
                    for (int i = 0; i < p.Faces.Count; i += 3)
                    {
                        if (p.indices != null)
                        {
                            distance = Intersects(ref r, k, j, p.indices[i], p.indices[i + 1], p.indices[i + 2], world);
                            if (distance != null)
                            {
                                Vector3? pos = r.Position + (r.Direction * (distance - 1.0f));

                                ObjectViewer.shoot_system.AddParticle((Vector3)pos, Vector3.Zero);
                                ObjectViewer.hitmesh = p.Name;
                                return true;
                            }
                        }

                    }
                    j++;
                }
            }
            return result;
        }

        public bool QuickCheck(Matrix world, Ray r, float d)
        {

            foreach (FaceGroup f in FaceGroups)
            {
                int tri = f.StartFace * 3;
                for (int j = 0; j < f.FaceCount; j++)
                {
                    int v1 = indices[tri++] + f.StartVertex;
                    int v2 = indices[tri++] + f.StartVertex;
                    int v3 = indices[tri++] + f.StartVertex;

                    Vector3 vv1 = Verts[v1].Position;
                    Vector3 vv2 = Verts[v2].Position;
                    Vector3 vv3 = Verts[v3].Position;

                    float? distance = Intersects(ref r, vv1, vv2, vv3, world);
                    if (distance != null)
                    {
                        return (distance.Value < d);
                    }
                }
            }


            return false;
        }

        public bool QuickCheck(Matrix world, Ray r)
        {

            foreach (FaceGroup f in FaceGroups)
            {
                int tri = f.StartFace * 3;
                for (int j = 0; j < f.FaceCount; j++)
                {
                    int v1 = indices[tri++] + f.StartVertex;
                    int v2 = indices[tri++] + f.StartVertex;
                    int v3 = indices[tri++] + f.StartVertex;

                    Vector3 vv1 = Verts[v1].Position;
                    Vector3 vv2 = Verts[v2].Position;
                    Vector3 vv3 = Verts[v3].Position;

                    float? distance = Intersects(ref r, vv1, vv2, vv3, world);
                    if (distance != null)
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        public bool Inside(float x, float y, String texture, float size)
        {
            Vector2 s = new Vector2(x, y);
            int i = 0;
            Vector2[] points = new Vector2[3];
            foreach (FaceGroup f in FaceGroups)
            {
                if (Form1.Manager.Names[Materials[i].TextureID].Equals(texture, StringComparison.OrdinalIgnoreCase))
                {
                    int tri = f.StartFace * 3;
                    for (int j = 0; j < f.FaceCount; j++)
                    {
                        int v1 = indices[tri++] + f.StartVertex;
                        int v2 = indices[tri++] + f.StartVertex;
                        int v3 = indices[tri++] + f.StartVertex;

                        points[0].X = trim(Verts[v1].TextureCoordinate.X, size);
                        points[0].Y = trim(Verts[v1].TextureCoordinate.Y, size);
                        points[1].X = trim(Verts[v2].TextureCoordinate.X, size);
                        points[1].Y = trim(Verts[v2].TextureCoordinate.Y, size);
                        points[2].X = trim(Verts[v3].TextureCoordinate.X, size);
                        points[2].Y = trim(Verts[v3].TextureCoordinate.Y, size);
                        if (intpoint_inside_trigon(s, points[0], points[1], points[2]))
                        {
                            selected_facegroup = f;
                            return true;
                        }

                    }
                }
                i++;
            }
            selected_facegroup = null;
            return false;
        }

        bool intpoint_inside_trigon(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            float s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y;
            float t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y;
            if (A < 0.0)
            {
                s = -s;
                t = -t;
                A = -A;
            }
            return s > 0 && t > 0 && (s + t) < A;
        }

        // Returns the distance from the origin of the ray to the intersection with 
        // the triangle, null if no intersect and negative if behind.
        private float? Intersects(ref Ray ray, Vector3 v1, Vector3 v2, Vector3 v3, Matrix world)
        {
            Vector3[] Vertex = new Vector3[3];
            Vertex[0] = Vector3.Transform(v1, world);
            Vertex[1] = Vector3.Transform(v2, world);
            Vertex[2] = Vector3.Transform(v3, world);

            // Set the Distance to indicate no intersect
            float? distance = null;
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref Vertex[2], ref Vertex[1], out edge1);
            Vector3.Subtract(ref Vertex[0], ref Vertex[1], out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                return distance;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref Vertex[1], out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                return distance;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                return distance;
            }

            // == By here the ray must be inside the triangle

            // Compute the distance along the ray to the triangle.
            float length = 0;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out length);
            distance = length * inverseDeterminant;

            return distance;
        }

        // Returns the distance from the origin of the ray to the intersection with 
        // the triangle, null if no intersect and negative if behind.
        private float? Intersects(ref Ray ray, int block, int part, short v1, short v2, short v3, Matrix world)
        {
            Vector3[] Vertex = new Vector3[3];
            Vertex[0] = Vector3.Transform(colmesh.Blocks[block].Parts[part].Verts[v1], world);
            Vertex[1] = Vector3.Transform(colmesh.Blocks[block].Parts[part].Verts[v2], world);
            Vertex[2] = Vector3.Transform(colmesh.Blocks[block].Parts[part].Verts[v3], world);

            // Set the Distance to indicate no intersect
            float? distance = null;
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref Vertex[2], ref Vertex[1], out edge1);
            Vector3.Subtract(ref Vertex[0], ref Vertex[1], out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                return distance;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref Vertex[1], out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                return distance;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                return distance;
            }

            // == By here the ray must be inside the triangle

            // Compute the distance along the ray to the triangle.
            float length = 0;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out length);
            distance = length * inverseDeterminant;

            return distance;
        }

        private Ray CreateRay(int x, int y, Matrix projection, Matrix view, Matrix world)
        {
            //  Unproject the screen space mouse coordinate into model space 
            //  coordinates. Because the world space matrix is identity, this 
            //  gives the coordinates in world space.
            Viewport vp = Form1.graphics.Viewport;

            Vector3 pos1 = vp.Unproject(new Vector3(x, y, 0), projection, view, Matrix.Identity);
            Vector3 pos2 = vp.Unproject(new Vector3(x, y, 1), projection, view, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);

            return new Ray(pos1, dir);
        }

        public void BuildAoList(Matrix world, ref List<Vector3> verts, ref List<Vector3> normals)
        {
            Matrix inv = Matrix.Transpose(Matrix.Invert(world));

            foreach (FaceGroup f in FaceGroups)
            {
                if (Form1.Manager.Names[Materials[f.Material].TextureID].Equals("skin1o.tga", StringComparison.OrdinalIgnoreCase))
                {
                    int tri = f.StartFace * 3;
                    for (int j = 0; j < f.FaceCount; j++)
                    {
                        int v1 = indices[tri++] + f.StartVertex;
                        int v2 = indices[tri++] + f.StartVertex;
                        int v3 = indices[tri++] + f.StartVertex;

                        verts.Add(Vector3.Transform(Verts[v1].Position, world));
                        verts.Add(Vector3.Transform(Verts[v2].Position, world));
                        verts.Add(Vector3.Transform(Verts[v3].Position, world));

                        normals.Add(Vector3.Transform(Verts[v1].Normal, inv));
                        normals.Add(Vector3.Transform(Verts[v2].Normal, inv));
                        normals.Add(Vector3.Transform(Verts[v3].Normal, inv));

                    }
                }
            }
        }

        public void BuildAoListVertexCamera(String texture, Matrix world, ObjectViewer viewer)
        {
            AOBuilder.Instance.SetNode(mesh_name);
            AOBuilder.Instance.SetProgress(0);
            AOValues.Clear();
            AOLookUp.Clear();
            float prog;
            int i = 0;
            int total = indices.GetLength(0);
            int done = 0;
            Matrix inv = Matrix.Transpose(Matrix.Invert(world));

            foreach (FaceGroup f in FaceGroups)
            {
                if (Form1.Manager.Names[Materials[f.Material].TextureID].Equals(texture, StringComparison.OrdinalIgnoreCase))
                {
                    int tri = f.StartFace * 3;
                    for (int j = 0; j < f.FaceCount; j++)
                    {
                        int v1 = indices[tri++] + f.StartVertex;
                        int v2 = indices[tri++] + f.StartVertex;
                        int v3 = indices[tri++] + f.StartVertex;
                        if (!AOLookUp.ContainsKey(v1))
                        {
                            AOLookUp.Add(v1, i);
                            i++;
                            AOValues.Add(viewer.GetAo(Vector3.Transform(Verts[v1].Position, world), Vector3.Transform(Verts[v1].Normal, inv), Vector3.UnitY));
                        }
                        if (!AOLookUp.ContainsKey(v2))
                        {
                            AOLookUp.Add(v2, i);
                            i++;
                            AOValues.Add(viewer.GetAo(Vector3.Transform(Verts[v2].Position, world), Vector3.Transform(Verts[v2].Normal, inv), Vector3.UnitY));
                        }
                        if (!AOLookUp.ContainsKey(v3))
                        {
                            AOLookUp.Add(v3, i);
                            i++;
                            AOValues.Add(viewer.GetAo(Vector3.Transform(Verts[v3].Position, world), Vector3.Transform(Verts[v3].Normal, inv), Vector3.UnitY));
                        }
                        done += 3;
                        prog = (done * 100) / total;
                        AOBuilder.Instance.SetProgress((int)prog);
                    }

                }
                else
                {
                    done += f.FaceCount * 3;
                    prog = (done * 100) / total;
                    AOBuilder.Instance.SetProgress((int)prog);
                }
            }

        }

        public void BakeAmbientMultiVertex(String texture, Matrix world, ObjectViewer viewer)
        {
            AOBuilder.Instance.SetNode(mesh_name);
            AOBuilder.Instance.SetProgress(0);
            AOValues.Clear();
            AOLookUp.Clear();
            float prog;
            float val;
            int i = 0;
            int total = indices.GetLength(0);
            int done = 0;
            foreach (FaceGroup f in FaceGroups)
            {
                if (Form1.Manager.Names[Materials[f.Material].TextureID].Equals(texture, StringComparison.OrdinalIgnoreCase))
                {
                    int tri = f.StartFace * 3;
                    for (int j = 0; j < f.FaceCount; j++)
                    {
                        int v1 = indices[tri++] + f.StartVertex;
                        int v2 = indices[tri++] + f.StartVertex;
                        int v3 = indices[tri++] + f.StartVertex;

                        if (!AOLookUp.ContainsKey(v1))
                        {
                            AOLookUp.Add(v1, i);
                            i++;

                            val = viewer.GetAo(Vector3.Transform(Verts[v1].Position, world), Vector3.UnitX, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v1].Position, world), -Vector3.UnitX, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v1].Position, world), Vector3.UnitZ, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v1].Position, world), -Vector3.UnitZ, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v1].Position, world), Vector3.UnitY, Vector3.UnitZ);
                            val += viewer.GetAo(Vector3.Transform(Verts[v1].Position, world), -Vector3.UnitY, Vector3.UnitZ);

                            val -= 3;
                            val /= 3;
                            val = Math.Max(0, val);
                            AOValues.Add(val);
                        }
                        if (!AOLookUp.ContainsKey(v2))
                        {
                            AOLookUp.Add(v2, i);
                            i++;

                            val = viewer.GetAo(Vector3.Transform(Verts[v2].Position, world), Vector3.UnitX, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v2].Position, world), -Vector3.UnitX, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v2].Position, world), Vector3.UnitZ, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v2].Position, world), -Vector3.UnitZ, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v2].Position, world), Vector3.UnitY, Vector3.UnitZ);
                            val += viewer.GetAo(Vector3.Transform(Verts[v2].Position, world), -Vector3.UnitY, Vector3.UnitZ);

                            val -= 3;
                            val /= 3;
                            val = Math.Max(0, val);
                            AOValues.Add(val);
                        }
                        if (!AOLookUp.ContainsKey(v3))
                        {
                            AOLookUp.Add(v3, i);
                            i++;
                            val = viewer.GetAo(Vector3.Transform(Verts[v3].Position, world), Vector3.UnitX, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v3].Position, world), -Vector3.UnitX, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v3].Position, world), Vector3.UnitZ, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v3].Position, world), -Vector3.UnitZ, Vector3.UnitY);
                            val += viewer.GetAo(Vector3.Transform(Verts[v3].Position, world), Vector3.UnitY, Vector3.UnitZ);
                            val += viewer.GetAo(Vector3.Transform(Verts[v3].Position, world), -Vector3.UnitY, Vector3.UnitZ);

                            val -= 3;
                            val /= 3;
                            val = Math.Max(0, val);
                            AOValues.Add(val);
                        }
                        done += 3;
                        prog = (done * 100) / total;
                        AOBuilder.Instance.SetProgress((int)prog);
                    }

                }
                else
                {
                    done += f.FaceCount * 3;
                    prog = (done * 100) / total;
                    AOBuilder.Instance.SetProgress((int)prog);
                }
            }

        }

        public void BakeAmbientVertexRay(String texture, Matrix world, ObjectViewer viewer, int count)
        {
            AOBuilder.Instance.SetNode(mesh_name);
            AOBuilder.Instance.SetProgress(0);
            AOValues.Clear();
            AOLookUp.Clear();
            float prog;
            int i = 0;
            int total = indices.GetLength(0);
            int done = 0;
            Matrix inv = Matrix.Transpose(Matrix.Invert(world));


            foreach (FaceGroup f in FaceGroups)
            {
                if (Form1.Manager.Names[Materials[f.Material].TextureID].Equals(texture, StringComparison.OrdinalIgnoreCase))
                {
                    int tri = f.StartFace * 3;
                    for (int j = 0; j < f.FaceCount; j++)
                    {
                        int v1 = indices[tri++] + f.StartVertex;
                        int v2 = indices[tri++] + f.StartVertex;
                        int v3 = indices[tri++] + f.StartVertex;
                        if (!AOLookUp.ContainsKey(v1))
                        {
                            AOLookUp.Add(v1, i);
                            i++;
                            Vector3 tv = Vector3.Transform(Verts[v1].Position, world);
                            Vector3 tn = Vector3.Transform(Verts[v1].Normal, inv);
                            AOValues.Add(viewer.GetRayAo(tv, tn, count));

                        }
                        if (!AOLookUp.ContainsKey(v2))
                        {
                            AOLookUp.Add(v2, i);
                            i++;
                            Vector3 tv = Vector3.Transform(Verts[v2].Position, world);
                            Vector3 tn = Vector3.Transform(Verts[v2].Normal, inv);
                            AOValues.Add(viewer.GetRayAo(tv, tn, count));

                        }
                        if (!AOLookUp.ContainsKey(v3))
                        {
                            AOLookUp.Add(v3, i);
                            i++;
                            Vector3 tv = Vector3.Transform(Verts[v3].Position, world);
                            Vector3 tn = Vector3.Transform(Verts[v3].Normal, inv);
                            AOValues.Add(viewer.GetRayAo(tv, tn, count));
                        }
                        done += 3;
                        prog = (done * 100) / total;
                        AOBuilder.Instance.SetProgress((int)prog);
                    }

                }
                else
                {
                    done += f.FaceCount * 3;
                    prog = (done * 100) / total;
                    AOBuilder.Instance.SetProgress((int)prog);
                }
            }

        }
        #endregion

        #region 3DS support
        public void Initialise(ThreeDSFile.Entity e, String dir)
        {
            Verts = new VertexPositionNormalTexture[e.vertices.GetLength(0)];
            for (int i = 0; i < e.vertices.GetLength(0); i++)
            {
                Verts[i].Position = e.vertices[i];
                Verts[i].TextureCoordinate = e.texcoords[i];
                Verts[i].Normal = e.normals[i];
            }
            indices = new short[e.indices.GetLength(0) * 3];
            int pos = 0;
            for (int i = 0; i < e.indices.GetLength(0); i++)
            {
                indices[pos++] = (short)e.indices[i].vertex1;
                indices[pos++] = (short)e.indices[i].vertex2;
                indices[pos++] = (short)e.indices[i].vertex3;
            }
            LodDistances.Add(99999);

            FaceGroup f = new FaceGroup();
            f.StartVertex = 0;
            f.StartFace = 0;
            f.VertexCount = e.vertices.GetLength(0);
            f.FaceCount = e.indices.GetLength(0);
            FaceGroups.Add(f);

            Material m = new Material(e.material.name);
            m.AlphaTestVal = 0;
            m.Ambient = e.material.Ambient[0];
            m.Diffuse = 0.9f;
            m.Colour = e.material.Diffuse;
            m.Glass = false;
            m.Shine = e.material.Specular[0];
            m.Specular = m.Shine;
            m.SpecularPow = e.material.Shininess;

            if (!String.IsNullOrEmpty(e.material.texture))
                m.TextureID = Form1.Manager.AddTexture(e.material.texture, dir);
            else
            {
                m.TextureID = Form1.Manager.AddTexture(@"null.tga", @"C:\Aircraft\3do\Plane\textures");
            }

            Materials.Add(m);
        }
        #endregion

        #region Modifiers
        List<Vector3> cache = new List<Vector3>();

        public void CacheNormals()
        {
            cache.Clear();
            for (int j = 0; j < Verts.Length; j++)
            {
                cache.Add(Verts[j].Normal);
            }
            for (int j = 0; j < Verts.Length; j++)
            {
                cache[j].Normalize();
            }
        }

        public void RecoverNormals()
        {
            for (int j = 0; j < Verts.Length; j++)
            {
                Verts[j].Normal = cache[j];
            }
        }
        public void FlipNormals(int axis)
        {
            switch (axis)
            {
                case 0: // z axis
                    {
                        for (int j = 0; j < cache.Count; j++)
                        {
                            Vector3 delta = new Vector3(0, 0, Verts[j].Position.Z);
                            delta = delta - Verts[j].Position;
                            delta.Normalize();
                            float a = Vector3.Dot(delta, cache[j]);
                            a = MathHelper.ToDegrees((float)(Math.Acos(a)));
                            if (a > 45)
                            {
                                Verts[j].Normal = -cache[j];
                            }
                            else
                            {
                                Verts[j].Normal = cache[j];
                            }
                        }
                    }
                    break;
                case 1: // y axis
                    {
                        for (int j = 0; j < cache.Count; j++)
                        {
                            Vector3 delta = new Vector3(0, Verts[j].Position.Y, 0);
                            delta = delta - Verts[j].Position;
                            delta.Normalize();
                            float a = Vector3.Dot(delta, cache[j]);
                            a = MathHelper.ToDegrees((float)Math.Abs(Math.Acos(a)));
                            if (a > 45)
                            {
                                Verts[j].Normal = -cache[j];
                            }
                            else
                            {
                                Verts[j].Normal = cache[j];
                            }
                        }
                    }
                    break;
                case 2: // X axis
                    {
                        for (int j = 0; j < cache.Count; j++)
                        {
                            Vector3 delta = new Vector3(Verts[j].Position.X, 0, 0);
                            delta = delta - Verts[j].Position;
                            delta.Normalize();
                            float a = Vector3.Dot(delta, cache[j]);
                            a = MathHelper.ToDegrees((float)Math.Abs(Math.Acos(a)));
                            if (a > 45)
                            {
                                Verts[j].Normal = -cache[j];
                            }
                            else
                            {
                                Verts[j].Normal = cache[j];
                            }
                        }
                    }
                    break;

                case 3:// ALWAYS
                    {
                        for (int j = 0; j < cache.Count; j++)
                        {
                            Verts[j].Normal = -cache[j];
                        }
                    }
                    break;

            }

        }

        public void FlipNormals(int axis, int facegroup)
        {
            FaceGroup f = FaceGroups[facegroup];
            for (int i = 0; i < f.VertexCount; i++)
            {
                int j = f.StartVertex + i;
                switch (axis)
                {
                    case 0:
                        Verts[j].Normal = Vector3.Reflect(cache[j], Vector3.UnitZ);
                        break;
                    case 1:
                        Verts[j].Normal = Vector3.Reflect(cache[j], Vector3.UnitY);
                        break;
                    case 2:
                        Verts[j].Normal = Vector3.Reflect(cache[j], Vector3.UnitX);
                        break;
                    case 3:
                        Verts[j].Normal = -cache[j];
                        break;
                }
            }



        }

        public void RecalculateNormals()
        {
            RegenerateNormals();
        }

        public void MoveUV(float dx, float dy)
        {
            if (selected_facegroup != null)
            {
                int start = selected_facegroup.StartVertex;
                for (int i = 0; i < selected_facegroup.VertexCount; i++)
                {
                    Verts[start].TextureCoordinate.X += dx;
                    Verts[start].TextureCoordinate.Y += dy;
                    start++;
                }

            }
        }

        public void RegenerateNormals()
        {
            int length = Verts.GetLength(0);
            float[] counts = new float[length];
            Vector3[] normals = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                counts[i] = 0;
                normals[i] = Vector3.Zero;
            }
            foreach (FaceGroup f in FaceGroups)
            {
                int tri = f.StartFace * 3;
                for (int j = 0; j < f.FaceCount; j++)
                {
                    int v1 = indices[tri++] + f.StartVertex;
                    int v2 = indices[tri++] + f.StartVertex;
                    int v3 = indices[tri++] + f.StartVertex;

                    Vector3 norm = calcNormal(v1, v2, v3);
                    normals[v1] += norm;
                    normals[v2] += norm;
                    normals[v3] += norm;
                    counts[v1]++;
                    counts[v2]++;
                    counts[v3]++;


                }
            }
            for (int i = 0; i < length; i++)
            {
                normals[i] /= counts[i];
                normals[i].Normalize();
                Verts[i].Normal = normals[i];
            }

        }

        Vector3 calcNormal(int v1, int v2, int v3)    			// Calculates Normal For A Quad Using 3 Points
        {
            Matrix iw = Matrix.Invert(parent.previous_matrix);

            Vector3 sv1 = Vector3.Transform(Verts[v2].Position, iw) - Vector3.Transform(Verts[v1].Position, iw);
            Vector3 sv2 = Vector3.Transform(Verts[v3].Position, iw) - Vector3.Transform(Verts[v1].Position, iw);
            Vector3 cv3 = Vector3.Cross(sv1, sv2);

            return cv3;
        }

        public void SwapTriangles()
        {
            foreach (FaceGroup f in FaceGroups)
            {
                int tri = f.StartFace * 3;
                for (int j = 0; j < f.FaceCount; j++)
                {
                    int v1 = indices[tri] + f.StartVertex;
                    int v2 = indices[tri + 1] + f.StartVertex;
                    int v3 = indices[tri + 2] + f.StartVertex;
                    if (!ClockWise(v1, v2, v3))
                    {
                        indices[tri + 1] = (short)(v3 - f.StartVertex);
                        indices[tri + 2] = (short)(v2 - f.StartVertex);
                    }
                    tri += 3;
                }
            }

        }

        bool ClockWise(int v1, int v2, int v3)
        {
            Vector3 n = calcNormal(v1, v2, v3);
            return (Math.Abs(Vector3.Dot(n, Verts[v1].Normal)) > 0.5);
        }

        public void Rotate90Z()
        {
            Matrix m = Matrix.CreateRotationZ(MathHelper.ToRadians(90));
            for (int i = 0; i < Verts.Length; i++)
            {
                Vector3 res = Vector3.Transform(Verts[i].Position, m);
                Verts[i].Position = res;
            }
        }

        public void AdjustLighting()
        {
            bool isStrelk = mesh_name.Contains("STREL");

            foreach (Material m in Materials)
            {
                m.AdjustLighting(isStrelk);
            }
        }
        #endregion

        #region Binormals and tangents
        public void GenerateBinormalsAndTangents()
        {
            BumpVerts = new BumpVertex[Verts.Length];
            for (int i = 0; i < Verts.Length; i++)
            {
                BumpVerts[i].Position = Verts[i].Position;
                BumpVerts[i].TextureCoordinate = Verts[i].TextureCoordinate;
                BumpVerts[i].Normal = Verts[i].Normal;
            }

            Vector3[] tan2 = new Vector3[Verts.Length];
            Vector3[] tan1 = new Vector3[Verts.Length];

            for (int a = 0; a < (indices.Length); a += 3)
            {
                short i1 = indices[a + 0];
                short i2 = indices[a + 1];
                short i3 = indices[a + 2];

                Vector3 v1 = Verts[i1].Position;
                Vector3 v2 = Verts[i2].Position;
                Vector3 v3 = Verts[i3].Position;

                Vector2 w1 = Verts[i1].TextureCoordinate;
                Vector2 w2 = Verts[i2].TextureCoordinate;
                Vector2 w3 = Verts[i3].TextureCoordinate;

                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;

                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;

                float r = 1.0F / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            for (int a = 0; a < Verts.Length; a++)
            {
                Vector3 n = Verts[a].Normal;
                Vector3 t = tan1[a];

                n.Normalize();
                Vector3 p = n * Vector3.Dot(t, n);
                t = t - p;
                t.Normalize();
                BumpVerts[a].Tangent = t;

                Vector4 temp = new Vector4(t.X, t.Y, t.Z, 1);

                // Calculate handedness
                temp.W = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
                BumpVerts[a].BiNormal = Vector3.Normalize(Vector3.Cross(n, t) * temp.W);
            }
        }

        public void EnableBumpMapping(int i, int t)
        {
            for (int j = 0; j < Materials.Count; j++)
            {
                if (Materials[j].TextureID == i)
                {
                    Materials[j].BumpMapped = true;
                    Materials[j].NormalTex = t;
                }
            }
        }
        #endregion

        #region FBX

        FbxVector2 getFBXUV(int i)
        {
            float x = Verts[i].TextureCoordinate.X;
            x -= (int)x;
            if (x < 0)
                x += 1;

            float y = Verts[i].TextureCoordinate.Y;
            y -= (int)y;
            if (y < 0)
                y += 1;
            y = 1 - y;

            return new FbxVector2(x, y);
        }

        public void SaveAsUE5(String dir, String Name, Matrix World)
        {
            String name = Path.Combine(dir, Name + "_" + mesh_name);
            name += ".obj";
            String matname = "mat_" + Name + "_" + mesh_name;
            Matrix rot = Matrix.CreateScale(100) * World;

            using (TextWriter writer = File.CreateText(name))
            {
                writer.WriteLine("mtllib " + matname + ".mtl");
                writer.WriteLine("");
                writer.WriteLine("");
                for (int i = 0; i < Verts.Length; i++)
                {
                    Vector3 p = Verts[i].Position;
                    p = Vector3.Transform(p, rot);

                    writer.WriteLine(String.Format("v {0} {1} {2}", p.X, p.Y, p.Z));
                    Vector3 n = Verts[i].Normal;

                    writer.WriteLine(String.Format("vn {0} {1} {2}", n.X, n.Y, n.Z));
                    writer.WriteLine(String.Format("vt {0} {1}", Verts[i].TextureCoordinate.X, Verts[i].TextureCoordinate.Y));
                }
                TextWriter material_writer = File.CreateText(dir + "//" + matname + ".mtl");
                foreach (FaceGroup f in FaceGroups)
                {
                    writer.WriteLine(String.Format("usemtl {0}", Materials[f.Material].Name));
                    writer.WriteLine("");
                    int si = f.StartFace * 3;
                    for (int i = 0; i < f.FaceCount; i++)
                    {
                        int v1, v2, v3;
                        v1 = 1 + f.StartVertex + indices[si++];
                        v2 = 1 + f.StartVertex + indices[si++];
                        v3 = 1 + f.StartVertex + indices[si++];

                        writer.WriteLine(String.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", v1, v2, v3));

                    }
                    writer.WriteLine("");

                    Material m = Materials[f.Material];                 
                    m.Saveobj(material_writer);
                   
                   
                }
                writer.Close();
                material_writer.Close();
            }
        }

        public void SaveAsUE4(String dir, Matrix world)
        {
            FbxSdkManager sdkManager = FbxSdkManager.Create();
            FbxScene scene = FbxScene.Create(sdkManager, "");
            Skill.FbxSDK.IO.FbxExporter exporter = Skill.FbxSDK.IO.FbxExporter.Create(sdkManager, "");
            int writeFileFormat = -1;
            Version version = Skill.FbxSDK.IO.FbxIO.CurrentVersion;
            if (writeFileFormat < 0 || writeFileFormat >= sdkManager.IOPluginRegistry.WriterFormatCount)
            {
                // Write in fall back format if pEmbedMedia is true
                writeFileFormat = sdkManager.IOPluginRegistry.NativeWriterFormat;
                {
                    //Try to export in ASCII if possible
                    int formatIndex, formatCount = sdkManager.IOPluginRegistry.WriterFormatCount;

                    for (formatIndex = 0; formatIndex < formatCount; formatIndex++)
                    {
                        if (sdkManager.IOPluginRegistry.WriterIsFBX(formatIndex))
                        {
                            string desc = sdkManager.IOPluginRegistry.GetWriterFormatDescription(formatIndex);

                            if (desc.Contains("ascii"))
                            {
                                writeFileFormat = formatIndex;
                                break;
                            }
                        }
                    }
                }
            }
            // Set the file format
            exporter.FileFormat = writeFileFormat;
            Skill.FbxSDK.IO.FbxStreamOptionsFbxWriter exportOptions = Skill.FbxSDK.IO.FbxStreamOptionsFbxWriter.Create(sdkManager, "");
            if (sdkManager.IOPluginRegistry.WriterIsFBX(writeFileFormat))
            {
                // Export options determine what kind of data is to be imported.
                // The default (except for the option eEXPORT_TEXTURE_AS_EMBEDDED)
                // is true, but here we set the options explictly.
                exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.MATERIAL, true);
                exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.TEXTURE, true);
                exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.EMBEDDED, false);
                exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.LINK, true);
                exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.SHAPE, false);
                exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.GOBO, false);
                exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.ANIMATION, false);
                exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.GLOBAL_SETTINGS, true);
            }
            FbxMesh mesh;
            mesh = FbxMesh.Create(sdkManager, mesh_name);
            mesh.InitControlPoints(Verts.Length);
            Matrix conv = Matrix.CreateScale(100) * Matrix.CreateRotationY(MathHelper.ToRadians(90));
            FbxVector4[] fVerts = new FbxVector4[Verts.Length];
            for (int i = 0; i < Verts.Length; i++)
            {
                Vector3 vr = Verts[i].Position;
                vr = Vector3.Transform(vr, conv);
                fVerts[i] = new FbxVector4(vr.X, vr.Y, vr.Z);
            }
            mesh.ControlPoints = fVerts;

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
                FbxVector2 fu = getFBXUV(i);
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

                FbxTexture.WrapMode xwrap = FbxTexture.WrapMode.Repeat;
                FbxTexture.WrapMode ywrap = FbxTexture.WrapMode.Repeat;
                if (!m.tfWrapX)
                    xwrap = FbxTexture.WrapMode.Clamp;
                if (!m.tfWrapY)
                    ywrap = FbxTexture.WrapMode.Clamp;

                texture.SetWrapMode(xwrap, ywrap);

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
                material.SpecularFactor = m.Shine;


                layer.Materials.DirectArray.Add(material);
            }

            #endregion

            #region Polygons
            int count = 0;
            int group = 0;
            foreach (FaceGroup f in FaceGroups)
            {
                int s = f.StartFace * 3;
                for (int i = 0; i < f.FaceCount; i++)
                {
                    mesh.BeginPolygon(f.Material, f.Material, group, true);
                    mesh.AddPolygon(indices[s] + f.StartVertex, indices[s] + f.StartVertex);
                    layer.DiffuseTextures.IndexArray.SetAt(s, f.Material);
                    layer.Materials.IndexArray.SetAt(s, f.Material);
                    s++;

                    mesh.AddPolygon(indices[s] + f.StartVertex, indices[s] + f.StartVertex);
                    layer.DiffuseTextures.IndexArray.SetAt(s, f.Material);
                    layer.Materials.IndexArray.SetAt(s, f.Material);

                    s++;
                    mesh.AddPolygon(indices[s] + f.StartVertex, indices[s] + f.StartVertex);
                    layer.DiffuseTextures.IndexArray.SetAt(s, f.Material);
                    layer.Materials.IndexArray.SetAt(s, f.Material);
                    s++;
                    mesh.EndPolygon();

                    count += 3;
                }
                group++;
            }
            #endregion

            UVDiffuseLayer.IndexArray.Count = count;

            FbxNode node = FbxNode.Create(sdkManager, mesh_name);
            Form1.Instance.fbx_nodes.Add("ROOT", node);
            node.NodeAttribute = mesh;
            node.Shading_Mode = FbxNode.ShadingMode.TextureShading;

            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            world.Decompose(out scale, out rotation, out translation);

            Vector3 lrot = FromQ2(rotation);
            FbxDouble3 loc = new FbxDouble3(translation.X, translation.Y, translation.Z);
            FbxDouble3 llrot = new FbxDouble3(lrot.X, lrot.Y, lrot.Z);
            FbxDouble3 lscale = new FbxDouble3(scale.X, scale.Y, scale.Z);

            node.Limits.ScalingLimitActive = false;
            node.Limits.RotationLimitActive = false;
            node.RotationActive = true;
            node.ScalingActive = true;

            node.LclTranslation.Set(loc);
            node.LclRotation.Set(llrot);
            node.LclScaling.Set(lscale);

            node.SetRotationOrder(FbxNode.PivotSet.SourceSet, FbxRotationOrder.EulerZXY);

            // Build the node tree.
            FbxNode rootNode = scene.RootNode;
            rootNode.AddChild(node);

            exporter.Export(scene, exportOptions);

            exportOptions.Destroy();
            sdkManager.Destroy();

            exporter = null;
            exportOptions = null;
            sdkManager = null;

        }

        public FbxNode SaveFBX(String dir, Matrix world, Node dad, Node mom)
        {
            Form1.Instance.globalSaveAsFBXDialog.Progress(mesh_name);
            FbxMesh mesh;
            String node_name;

            #region Create mesh and add verts
            if (Form1.Instance.fbx_meshes.ContainsKey(mesh_name))
            {
                mesh = Form1.Instance.fbx_meshes[mesh_name];
                node_name = mesh_name + "_inst_" + Form1.Instance.Fbx_node_count;
                Form1.Instance.Fbx_node_count++;
            }
            else
            {
                node_name = dad.Name;
                mesh = FbxMesh.Create(Form1.Instance.sdkManager, mesh_name);
                mesh.InitControlPoints(Verts.Length);

                FbxVector4[] fVerts = new FbxVector4[Verts.Length];
                for (int i = 0; i < Verts.Length; i++)
                {
                    Vector3 vr = Verts[i].Position;
                    fVerts[i] = new FbxVector4(vr.X, vr.Y, vr.Z);
                }
                mesh.ControlPoints = fVerts;

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
                    FbxVector2 fu = getFBXUV(i);
                    //Debug.WriteLine(String.Format("{0}\t\t({1},{2})", i, Verts[i].TextureCoordinate.X, Verts[i].TextureCoordinate.Y));
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
                    FbxTexture texture = FbxTexture.Create(Form1.Instance.sdkManager, "DiffuseTexture" + m.Name);
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

                    FbxTexture.WrapMode xwrap = FbxTexture.WrapMode.Repeat;
                    FbxTexture.WrapMode ywrap = FbxTexture.WrapMode.Repeat;
                    if (!m.tfWrapX)
                        xwrap = FbxTexture.WrapMode.Clamp;
                    if (!m.tfWrapY)
                        ywrap = FbxTexture.WrapMode.Clamp;

                    texture.SetWrapMode(xwrap, ywrap);

                    layer.DiffuseTextures.DirectArray.Add(texture);

                    FbxSurfacePhong material = FbxSurfacePhong.Create(Form1.Instance.sdkManager, m.Name);

                    // Generate primary and secondary colors.
                    material.EmissiveColor = new FbxDouble3(0.0, 0.0, 0.0);
                    material.AmbientColor = new FbxDouble3(m.Ambient, m.Ambient, m.Ambient);
                    material.DiffuseColor = new FbxDouble3(m.Diffuse, m.Diffuse, m.Diffuse);
                    material.SpecularColor = new FbxDouble3(m.Specular, m.Specular, m.Specular);
                    material.TransparencyFactor = m.AlphaTestVal;
                    material.Shininess = m.SpecularPow;
                    material.ShadingModel = "phong";
                    material.SpecularFactor = m.Shine;


                    layer.Materials.DirectArray.Add(material);
                }

                #endregion

                #region Polygons
                int count = 0;
                int group = 0;
                foreach (FaceGroup f in FaceGroups)
                {
                    int s = f.StartFace * 3;
                    for (int i = 0; i < f.FaceCount; i++)
                    {
                        mesh.BeginPolygon(f.Material, f.Material, group, true);
                        mesh.AddPolygon(indices[s] + f.StartVertex, indices[s] + f.StartVertex);
                        layer.DiffuseTextures.IndexArray.SetAt(s, f.Material);
                        layer.Materials.IndexArray.SetAt(s, f.Material);
                        s++;

                        mesh.AddPolygon(indices[s] + f.StartVertex, indices[s] + f.StartVertex);
                        layer.DiffuseTextures.IndexArray.SetAt(s, f.Material);
                        layer.Materials.IndexArray.SetAt(s, f.Material);

                        s++;
                        mesh.AddPolygon(indices[s] + f.StartVertex, indices[s] + f.StartVertex);
                        layer.DiffuseTextures.IndexArray.SetAt(s, f.Material);
                        layer.Materials.IndexArray.SetAt(s, f.Material);
                        s++;
                        mesh.EndPolygon();

                        count += 3;
                    }
                    group++;
                }
                #endregion

                UVDiffuseLayer.IndexArray.Count = count;
                Form1.Instance.fbx_meshes.Add(mesh_name, mesh);
                node_name = mesh_name;
            }
            #endregion

            FbxNode node = FbxNode.Create(Form1.Instance.sdkManager, node_name);
            Form1.Instance.fbx_nodes.Add(dad.Name.ToLower(), node);
            node.NodeAttribute = mesh;
            node.Shading_Mode = FbxNode.ShadingMode.TextureShading;

            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            world.Decompose(out scale, out rotation, out translation);

            Vector3 lrot = FromQ2(rotation);
            FbxDouble3 loc = new FbxDouble3(translation.X, translation.Y, translation.Z);
            FbxDouble3 llrot = new FbxDouble3(lrot.X, lrot.Y, lrot.Z);
            FbxDouble3 lscale = new FbxDouble3(scale.X, scale.Y, scale.Z);

            node.Limits.ScalingLimitActive = false;
            node.Limits.RotationLimitActive = false;
            node.RotationActive = true;
            node.ScalingActive = true;

            node.LclTranslation.Set(loc);
            node.LclRotation.Set(llrot);
            node.LclScaling.Set(lscale);

            node.SetRotationOrder(FbxNode.PivotSet.SourceSet, FbxRotationOrder.EulerZXY);

            // Build the node tree.
            FbxNode rootNode = Form1.Instance.scene.RootNode;
            if (dad != null)
            {
                if (!dad.Parent.Contains("ROOT"))
                {
                    rootNode = Form1.Instance.fbx_nodes[dad.Parent.ToLower()];
                    if (rootNode == null)
                        rootNode = Form1.Instance.scene.RootNode;
                }
            }

            if (Form1.ExportBaseMesh)
                rootNode.AddChild(node);

            #region Shadow
            // now add the shadow
            if (ShadowVertsArray != null)
            {
                #region Create mesh and add verts
                FbxMesh smesh = FbxMesh.Create(Form1.Instance.sdkManager, mesh_name + "_shadow");
                smesh.InitControlPoints(ShadowVertsArray.Length);

                FbxVector4[] sVerts = new FbxVector4[ShadowVertsArray.Length];
                for (int i = 0; i < ShadowVertsArray.Length; i++)
                {
                    Vector3 vr = ShadowVertsArray[i].Position;
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

                FbxNode snode = FbxNode.Create(Form1.Instance.sdkManager, mesh_name + "_shadow");
                snode.NodeAttribute = smesh;
                snode.Shading_Mode = FbxNode.ShadingMode.WireFrame;

                if (Form1.ExportShadows)
                    node.AddChild(snode);


            }
            #endregion

            #region Hooks
            foreach (Hook h in Hooks)
            {
                FbxNode hnode = FbxNode.Create(Form1.Instance.sdkManager, h.Name);
                hnode.Shading_Mode = FbxNode.ShadingMode.WireFrame;

                Quaternion qrot;
                Vector3 tran;
                Vector3 scl;
                h.matrix.Decompose(out scl, out qrot, out tran);

                Vector3 hlrot = FromQ2(rotation);

                hnode.Limits.ScalingLimitActive = false;
                hnode.Limits.RotationLimitActive = false;
                hnode.RotationActive = true;
                hnode.ScalingActive = true;


                hnode.SetRotationOrder(FbxNode.PivotSet.SourceSet, FbxRotationOrder.EulerZXY);


                Vector3 rtran = tran;
                hnode.LclTranslation.Set(new FbxDouble3(rtran.X, rtran.Y, rtran.Z));
                hnode.LclScaling.Set(new FbxDouble3(scl.X, scl.Y, scl.Z));
                hnode.LclRotation.Set(new FbxDouble3(hlrot.X, hlrot.Y, hlrot.Z));

                if (Form1.ExportHooks)
                    node.AddChild(hnode);
            }
            #endregion

            #region LOD's
            for (int i = 0; i < Lods.Count; i++)
            {
                if (Form1.ExportLODs)
                    node.AddChild(Lods[i].CreateFBXNode(Form1.Instance.sdkManager, mesh_name, i, dir, Matrix.Identity));

                FbxNode shadow_node = Lods[i].AddShadow(Form1.Instance.sdkManager, mesh_name, i, dir, Matrix.Identity);

                if ((shadow_node != null) && (Form1.ExportLODShadows))
                    node.AddChild(shadow_node);
            }
            #endregion

            #region Collision meshes
            if (Form1.ExportCollisionMeshes)
            {
                for (int j = 0; j < colmesh.NBlocks; j++)
                {
                    foreach (CollisionMeshPart part in colmesh.Blocks[j].Parts)
                    {
                        if (part.Faces.Count > 0)
                        {
                            node.AddChild(part.AddCollisionMeshPart(mesh_name, Form1.Instance.sdkManager, Matrix.Identity));
                        }
                    }
                }
            }
            #endregion

            return node;
        }


        /// <summary> 
        /// The function converts a Microsoft.Xna.Framework.Quaternion into a Microsoft.Xna.Framework.Vector3 
        /// </summary> 
        /// <param name="q">The Quaternion to convert</param> 
        /// <returns>An equivalent Vector3</returns> 
        /// <remarks> 
        /// This function was extrapolated by reading the work of Martin John Baker. All credit for this function goes to Martin John. 
        /// http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/index.htm 
        /// </remarks> 
        public Vector3 QuaternionToEuler(Quaternion q)
        {
            Vector3 v = new Vector3();

            v.X = (float)Math.Atan2
            (
                2 * q.Y * q.W - 2 * q.X * q.Z,
                1 - 2 * Math.Pow(q.Y, 2) - 2 * Math.Pow(q.Z, 2)
            );

            v.Y = (float)Math.Asin
            (
                2 * q.X * q.Y + 2 * q.Z * q.W
            );

            v.Z = (float)Math.Atan2
            (
                2 * q.X * q.W - 2 * q.Y * q.Z,
                1 - 2 * Math.Pow(q.X, 2) - 2 * Math.Pow(q.Z, 2)
            );

            if (q.X * q.Y + q.Z * q.W == 0.5)
            {
                v.X = (float)(2 * Math.Atan2(q.X, q.W));
                v.Z = 0;
            }

            else if (q.X * q.Y + q.Z * q.W == -0.5)
            {
                v.X = (float)(-2 * Math.Atan2(q.X, q.W));
                v.Z = 0;
            }

            return v;
        }

        Vector3 FromQ2(Quaternion q1)
        {
            float sqw = q1.W * q1.W;
            float sqx = q1.X * q1.X;
            float sqy = q1.Y * q1.Y;
            float sqz = q1.Z * q1.Z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = q1.X * q1.W - q1.Y * q1.Z;
            Vector3 v;

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.Y = (float)(2 * Math.Atan2(q1.Y, q1.X));
                v.X = MathHelper.PiOver2;
                v.Z = 0;
                return NormalizeAngles(v);
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.Y = (float)(-2 * Math.Atan2(q1.Y, q1.X));
                v.X = -MathHelper.PiOver2;
                v.Z = 0;
                return NormalizeAngles(v);
            }
            Quaternion q = new Quaternion(q1.W, q1.Z, q1.X, q1.Y);
            v.Y = (float)Math.Atan2(2 * q.X * q.W + 2 * q.Y * q.Z, 1 - 2 * (q.Z * q.Z + q.W * q.W));        // Yaw
            v.X = (float)Math.Asin(2 * (q.X * q.Z - q.W * q.Y));                                            // Pitch
            v.Z = (float)Math.Atan2(2 * q.X * q.Y + 2 * q.Z * q.W, 1 - 2 * (q.Y * q.Y + q.Z * q.Z));        // Roll
            return NormalizeAngles(v);
        }

        Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.X = NormalizeAngle(MathHelper.ToDegrees(angles.X));
            angles.Y = NormalizeAngle(MathHelper.ToDegrees(angles.Y));
            angles.Z = NormalizeAngle(MathHelper.ToDegrees(angles.Z));
            return angles;
        }

        float NormalizeAngle(float angle)
        {
            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;
            return angle;
        }
        #endregion
    }
}
