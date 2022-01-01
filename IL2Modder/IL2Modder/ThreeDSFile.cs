using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using IL2Modder.IL2;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder
{
    public class ThreeDSFile
    {
        #region classes

        class ThreeDSChunk
        {
            public ushort ID;
            public uint Length;
            public int BytesRead;

            public ThreeDSChunk(BinaryReader reader)
            {
                // 2 byte ID
                ID = reader.ReadUInt16();

                // 4 byte length
                Length = reader.ReadUInt32();

                // = 6
                BytesRead = 6;
            }
        }

        public struct Triangle
        {
            public int vertex1;
            public int vertex2;
            public int vertex3;

            public Triangle(int v1, int v2, int v3)
            {
                vertex1 = v1;
                vertex2 = v2;
                vertex3 = v3;
            }

            public override string ToString()
            {
                return String.Format("v1: {0} v2: {1} v3: {2}", vertex1, vertex2, vertex3);
            }
        }

        public class nMaterial
        {
            // Set Default values
            public float[] Ambient = new float[] { 0.5f, 0.5f, 0.5f };
            public float[] Diffuse = new float[] { 0.5f, 0.5f, 0.5f };
            public float[] Specular = new float[] { 0.5f, 0.5f, 0.5f };
            public int Shininess = 50;
            public String name;
            public String texture;

            public nMaterial() { }
        }

        public struct MaterialFaces
        {
            public nMaterial Material;
            public UInt16[] Faces;
        }

        public class Entity
        {
            public nMaterial material = new nMaterial();

            // The stored vertices 
            public Vector3[] vertices;

            // The calculated normals
            public Vector3[] normals;

            // The indices of the triangles which point to vertices
            public Triangle[] indices;

            // The coordinates which map the texture onto the entity
            public Vector2[] texcoords;

            public List<MaterialFaces> materialFaces = new List<MaterialFaces>();

            public bool normalized = false;

            public short id = -1;

            public void CalculateNormals()
            {
                if (indices == null) return;

                normals = new Vector3[vertices.Length];

                Vector3[] temps = new Vector3[indices.Length];

                for (int ii = 0; ii < indices.Length; ii++)
                {
                    Triangle tr = indices[ii];

                    Vector3 v1 = vertices[tr.vertex1] - vertices[tr.vertex2];
                    Vector3 v2 = vertices[tr.vertex2] - vertices[tr.vertex3];

                    temps[ii] = Vector3.Cross(v1, v2);
                }

                for (int ii = 0; ii < vertices.Length; ii++)
                {
                    Vector3 v = new Vector3();
                    float shared = 0;

                    for (int jj = 0; jj < indices.Length; jj++)
                    {
                        Triangle tr = indices[jj];
                        if (tr.vertex1 == ii || tr.vertex2 == ii || tr.vertex3 == ii)
                        {
                            v += temps[jj];
                            shared++;
                        }
                    }
                    v = v / shared;
                    v.Normalize();
                    normals[ii] = v;
                }
                normalized = true;
            }
        }
        #endregion

        #region Enums

        enum Groups
        {
            C_PRIMARY = 0x4D4D,
            C_OBJECTINFO = 0x3D3D,
            C_VERSION = 0x0002,
            C_EDITKEYFRAME = 0xB000,
            C_MATERIAL = 0xAFFF,
            C_MATNAME = 0xA000,
            C_MATAMBIENT = 0xA010,
            C_MATDIFFUSE = 0xA020,
            C_MATSPECULAR = 0xA030,
            C_MATSHININESS = 0xA040,
            C_MATMAP = 0xA200,
            C_MATMAPFILE = 0xA300,
            C_OBJECT = 0x4000,
            C_OBJECT_MESH = 0x4100,
            C_OBJECT_VERTICES = 0x4110,
            C_OBJECT_FACES = 0x4120,
            C_OBJECT_MATERIAL = 0x4130,
            C_OBJECT_UV = 0x4140,
            C_Hierarchy_position = 0xB030
        }

        #endregion		

        string base_dir;
        BinaryReader reader;
        double maxX, maxY, maxZ, minX, minY, minZ;
        int version = -1;

        public List<Entity> entities = new List<Entity>();
        public List<String> names = new List<String>();
        public Dictionary<string, nMaterial> materials = new Dictionary<string, nMaterial>();
        

        public ThreeDSFile(String file_name)
        {
            if (!File.Exists(file_name))
            {
                throw new ArgumentException("3ds file could not be found", "file_name");
            }
            base_dir =  new FileInfo ( file_name ).DirectoryName + "/";
			
			maxX = maxY = maxZ = double.MinValue;
			minX = minY = minZ = double.MaxValue;
		
			FileStream file = null;
			try
			{
				// create a binary stream from this file
				file  = new FileStream(file_name, FileMode.Open, FileAccess.Read); 
				reader = new BinaryReader ( file );
				reader.BaseStream.Seek (0, SeekOrigin.Begin); 

				// 3ds files are in chunks
				// read the first one
				ThreeDSChunk chunk = new ThreeDSChunk ( reader );
				
				if ( chunk.ID != (short) Groups.C_PRIMARY )
				{
					throw new FormatException ( "Not a proper 3DS file." );
				}

				// recursively process chunks
				ProcessChunk ( chunk );
			}
			finally
			{
				// close up everything
				if (reader != null) reader.Close ();
				if (file != null) file.Close ();
			}			
		}
     
        void ProcessChunk(ThreeDSChunk chunk)
        {
            // process chunks until there are none left
            while (chunk.BytesRead < chunk.Length)
            {
                // grab a chunk
                ThreeDSChunk child = new ThreeDSChunk(reader);

                // process based on ID
                switch ((Groups)child.ID)
                {
                    case Groups.C_VERSION:
                        version = reader.ReadInt32();
                        child.BytesRead += 4;
                        break;

                    case Groups.C_OBJECTINFO:

                        // not sure whats up with this chunk
                        //SkipChunk ( obj_chunk );
                        //child.BytesRead += obj_chunk.BytesRead;
                        //ProcessChunk ( child );

                        // blender 3ds export (others?) uses this
                        // in the hierarchy of objects and materials
                        // so lets process the next (child) chunk

                        break;

                    case Groups.C_MATERIAL:
                        ProcessMaterialChunk(child);
                        break;

                    case Groups.C_OBJECT:

                        string name = ProcessString(child);
                        names.Add(name);
                        Entity e = ProcessObjectChunk(child);
                        e.CalculateNormals();
                        entities.Add(e);
                        break;

                    case Groups.C_Hierarchy_position:
                        entities[entities.Count-1].id = ProcessHeirachyPositionChunk(child);
                        break;

                    default:
                        SkipChunk(child);
                        break;

                }

                chunk.BytesRead += child.BytesRead;
            }
        }

        Entity ProcessObjectChunk(ThreeDSChunk chunk)
        {
            return ProcessObjectChunk(chunk, new Entity());
        }

        void ProcessMaterialChunk(ThreeDSChunk chunk)
        {
            string name = string.Empty;
            nMaterial m = new nMaterial();

            while (chunk.BytesRead < chunk.Length)
            {
                ThreeDSChunk child = new ThreeDSChunk(reader);

                switch ((Groups)child.ID)
                {
                    case Groups.C_MATNAME:

                        m.name = ProcessString(child);
                        name = m.name;
                        break;

                    case Groups.C_MATAMBIENT:

                        m.Ambient = ProcessColorChunk(child);
                        break;

                    case Groups.C_MATDIFFUSE:

                        m.Diffuse = ProcessColorChunk(child);
                        break;

                    case Groups.C_MATSPECULAR:

                        m.Specular = ProcessColorChunk(child);
                        break;

                    case Groups.C_MATSHININESS:

                        m.Shininess = ProcessPercentageChunk(child);

                        break;

                    case Groups.C_MATMAP:

                        ProcessPercentageChunk(child);

                        ProcessTexMapChunk(child, m);

                        break;

                    default:

                        SkipChunk(child);
                        break;

                }
                chunk.BytesRead += child.BytesRead;
            }
            materials.Add(name, m);
        }

        void ProcessTexMapChunk(ThreeDSChunk chunk, nMaterial m)
        {
            while (chunk.BytesRead < chunk.Length)
            {
                ThreeDSChunk child = new ThreeDSChunk(reader);
                switch ((Groups)child.ID)
                {
                    case Groups.C_MATMAPFILE:
                        string name = ProcessString(child);
                        m.texture = name;
                        break;

                    default:

                        SkipChunk(child);
                        break;

                }
                chunk.BytesRead += child.BytesRead;
            }
        }

        float[] ProcessColorChunk(ThreeDSChunk chunk)
        {
            ThreeDSChunk child = new ThreeDSChunk(reader);
            float[] c = new float[] { (float)reader.ReadByte() / 256, (float)reader.ReadByte() / 256, (float)reader.ReadByte() / 256 };
            //Console.WriteLine ( "R {0} G {1} B {2}", c.R, c.B, c.G );
            chunk.BytesRead += (int)child.Length;
            return c;
        }

        short ProcessHeirachyPositionChunk(ThreeDSChunk chunk)
        {
            ThreeDSChunk child = new ThreeDSChunk(reader);
            short res = reader.ReadInt16();
            child.BytesRead += 2;
            chunk.BytesRead += child.BytesRead;
            Console.WriteLine("Chunk ID = " + res);
            return res;
        }

        int ProcessPercentageChunk(ThreeDSChunk chunk)
        {
            ThreeDSChunk child = new ThreeDSChunk(reader);
            int per = reader.ReadUInt16();
            child.BytesRead += 2;
            chunk.BytesRead += child.BytesRead;
            return per;
        }

        Entity ProcessObjectChunk(ThreeDSChunk chunk, Entity e)
        {
            while (chunk.BytesRead < chunk.Length)
            {
                ThreeDSChunk child = new ThreeDSChunk(reader);

                switch ((Groups)child.ID)
                {
                    case Groups.C_OBJECT_MESH:
                        ProcessObjectChunk(child, e);
                        break;

                    case Groups.C_OBJECT_VERTICES:
                        e.vertices = ReadVertices(child);
                        break;

                    case Groups.C_OBJECT_FACES:
                        e.indices = ReadIndices(child);
                        if (child.BytesRead < child.Length)
                            ProcessObjectChunk(child, e);
                        break;

                    case Groups.C_OBJECT_MATERIAL:

                        string name2 = ProcessString(child);

                        nMaterial mat;
                        if (materials.TryGetValue(name2, out mat))
                        {
                            e.material = mat;

                            MaterialFaces m = new MaterialFaces();
                            m.Material = mat;

                            int nfaces = reader.ReadUInt16();
                            child.BytesRead += 2;
                            
                            m.Faces = new UInt16[nfaces];

                            for (int ii = 0; ii < nfaces; ii++)
                            {
                                m.Faces[ii] = reader.ReadUInt16();
                                child.BytesRead += 2;
                            }

                            e.materialFaces.Add(m);
                        }
                        else
                        {
                            Console.WriteLine(" Warning: Material '{0}' not found. ", name2);
                            //throw new Exception ( "Material not found!" );

                            SkipChunk(child);
                        }

                        break;

                    case Groups.C_OBJECT_UV:

                        int cnt = reader.ReadUInt16();
                        child.BytesRead += 2;

                        //Console.WriteLine("	TexCoords: {0}", cnt);
                        e.texcoords = new Vector2[cnt];
                        for (int ii = 0; ii < cnt; ii++)
                            e.texcoords[ii] = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                        child.BytesRead += (cnt * (4 * 2));

                        break;

                    default:

                        SkipChunk(child);
                        break;

                }
                chunk.BytesRead += child.BytesRead;
                //Console.WriteLine ( "	ID: {0} Length: {1} Read: {2}", chunk.ID.ToString("x"), chunk.Length , chunk.BytesRead );
            }
            return e;
        }

        void SkipChunk(ThreeDSChunk chunk)
        {
            int length = (int)chunk.Length - chunk.BytesRead;
            reader.ReadBytes(length);
            chunk.BytesRead += length;
        }

        string ProcessString(ThreeDSChunk chunk)
        {
            StringBuilder sb = new StringBuilder();

            byte b = reader.ReadByte();
            int idx = 0;
            while (b != 0)
            {
                sb.Append((char)b);
                b = reader.ReadByte();
                idx++;
            }
            chunk.BytesRead += idx + 1;

            return sb.ToString();
        }

        Vector3[] ReadVertices(ThreeDSChunk chunk)
        {
            ushort numVerts = reader.ReadUInt16();
            chunk.BytesRead += 2;
            //Console.WriteLine("	Vertices: {0}", numVerts);
            Vector3[] verts = new Vector3[numVerts];

            for (int ii = 0; ii < verts.Length; ii++)
            {
                float f1 = reader.ReadSingle();
                float f2 = reader.ReadSingle();
                float f3 = reader.ReadSingle();

                Vector3 v = new Vector3(f1, f3, -f2);

                // track the boundaries of this model
                if (v.X > maxX) maxX = v.X;
                if (v.Y > maxY) maxY = v.Y;
                if (v.Z > maxZ) maxZ = v.Z;

                if (v.X < minX) minX = v.X;
                if (v.Y < minY) minY = v.Y;
                if (v.Z < minZ) minZ = v.Z;

                verts[ii] = v;
            }

            chunk.BytesRead += verts.Length * (3 * 4);
            return verts;
        }

        Triangle[] ReadIndices(ThreeDSChunk chunk)
        {
            ushort numIdcs = reader.ReadUInt16();
            chunk.BytesRead += 2;
            Triangle[] idcs = new Triangle[numIdcs];

            for (int ii = 0; ii < idcs.Length; ii++)
            {
                idcs[ii] = new Triangle(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());
                //Console.WriteLine ( idcs [ii] );

                // flags
                reader.ReadUInt16();
            }
            chunk.BytesRead += (2 * 4) * idcs.Length;
            //Console.WriteLine ( "b {0} l {1}", chunk.BytesRead, chunk.Length);

            //chunk.BytesRead = (int) chunk.Length;
            //SkipChunk ( chunk );

            return idcs;
        }

    }
}
