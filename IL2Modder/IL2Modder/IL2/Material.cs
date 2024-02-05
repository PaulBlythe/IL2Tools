using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder.IL2
{
    public class Material
    {
        public String Name;
        public bool tfDoubleSided;
        public bool Sort;
        public bool Glass;
        public float Ambient;
        public float Diffuse;
        public float Specular;
        public float SpecularPow;
        public float Shine;
        public float[] Colour = new float[4];
        public bool tfWrapX;
        public bool tfWrapY;
        public bool tfMinLinear;
        public bool tfMagLinear;
        public bool tfBlend;
        public bool tfBlendAdd;
        public bool tfNoTexture;
        public float tfDepthOffset;
        public bool tfNoWriteZ;
        public int TextureID;
        public float AlphaTestVal;
        public bool tfTranspBorder;
        public bool tfTestA;
        public bool tfTestZ;
        public bool BumpMapped;
        public int NormalTex;
        public String tname = "";

        public Material(String name)
        {
            tfDoubleSided = false;
            Sort = false;
            Glass = false;
            tfWrapX = true;
            tfWrapY = true;
            tfMinLinear = true;
            tfMagLinear = true;
            tfBlend = false;
            tfBlendAdd = false;
            tfNoTexture = false;
            tfNoWriteZ = false;
            tfDepthOffset = 0;
            AlphaTestVal = 1;
            tfTranspBorder = false;
            BumpMapped = false;
            Name = name;
        }

        public Material(String name, String dir)
        {
            char[] seperators = new char[] { ' ', '\t' };
            tfDoubleSided = false;
            Sort = false;
            Glass = false;
            tfWrapX = true;
            tfWrapY = true;
            tfMinLinear = true;
            tfMagLinear = true;
            tfBlend = false;
            tfBlendAdd = false;
            tfNoTexture = false;
            tfNoWriteZ = false;
            tfDepthOffset = 0;
            AlphaTestVal = 1;
            tfTranspBorder = false;
            BumpMapped = false;
            Name = name;

            name = name.TrimEnd(' ');
            String filename = dir + "\\" + name + ".mat";

            using (TextReader reader = File.OpenText(filename))
            {
                string line;
                string[] parts;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("//"))
                        line = "";

                    parts = line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                    if (line.Contains("tfDoubleSide"))
                    {
                        if (parts.GetLength(0) > 1)
                            tfDoubleSided = parts[1].Equals("1");
                        else
                            tfDoubleSided = true;
                    }
                    if (line.Contains("tfShouldSort"))
                    {
                        Sort = parts[1].Equals("1");
                    }
                    if (line.Contains("tfTranspBorder"))
                    {
                        tfTranspBorder = parts[1].Equals("1");
                    }
                    if (line.Contains("Ambient"))
                    {
                        Ambient = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("Diffuse"))
                    {
                        Diffuse = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        if (Diffuse > 1)
                            Diffuse = 1;
                    }
                    if ((line.Contains("Specular")) && (!line.Contains("SpecularPow")))
                    {
                        Specular = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("SpecularPow"))
                    {
                        if (parts.Length > 1)
                            SpecularPow = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        else
                            SpecularPow = 0;
                    }
                    if (line.Contains("Shine"))
                    {
                        Shine = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("TextureName"))
                    {
                        if (parts.GetLongLength(0) == 2)
                        {
                            if (!parts[1].EndsWith("txl"))
                            {
                                TextureID = Form1.Manager.AddTexture(parts[1], Path.GetDirectoryName(filename));
                                tname = parts[1];
                                tname = tname.ToLowerInvariant();
                                if (tname.Contains("glass"))
                                    Glass = true;
                            }
                        }

                    }
                    if (line.Contains("AlphaTestVal"))
                    {
                        AlphaTestVal = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("tfTestA"))
                    {
                        tfTestA = parts[1].Equals("1");
                    }
                    if (line.Contains("tfWrapX"))
                    {
                        tfWrapX = parts[1].Equals("1");
                    }
                    if (line.Contains("tfWrapY"))
                    {
                        tfWrapY = parts[1].Equals("1");
                    }
                    if (line.Contains("tfMinLinear"))
                    {
                        tfMinLinear = parts[1].Equals("1");
                    }
                    if ((line.Contains("tfBlend")) && (!line.Contains("tfBlendAdd")))
                    {
                        tfBlend = parts[1].Equals("1");
                    }
                    if (line.Contains("tfBlendAdd"))
                    {
                        tfBlendAdd = parts[1].Equals("1");
                    }
                    if (line.Contains("tfNoWriteZ"))
                    {
                        tfNoWriteZ = parts[1].Equals("1");
                    }
                    if (line.Contains("tfDepthOffset"))
                    {
                        tfDepthOffset = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("ColorScale"))
                    {
                        Colour[0] = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        Colour[1] = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                        Colour[2] = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("BasedOn"))
                    {
                        Load(parts[1], dir);
                    }
                }
                reader.Close();
                if (tfTranspBorder)
                {
                    Texture2D tex = Form1.Manager.GetTexture(TextureID);
                    Color[] data = new Color[tex.Width * tex.Height];
                    tex.GetData<Color>(data);
                    for (int y = 0; y < tex.Height; y++)
                    {
                        data[y * tex.Width] = Color.Transparent;
                        data[(y * tex.Width) + (tex.Width - 1)] = Color.Transparent;
                    }
                    for (int y = 0; y < tex.Width; y++)
                    {
                        data[y] = Color.Transparent;
                        data[y + (tex.Width * (tex.Height - 1))] = Color.Transparent;
                    }
                    tex.SetData<Color>(data);
                    tfBlend = true;
                }
            }
        }

        public Material(Material other)
        {
            Name = other.Name + "_clone";
            tfDoubleSided = other.tfDoubleSided;
            Sort = other.Sort;
            Glass = other.Glass;
            Ambient = other.Ambient;
            Diffuse = other.Diffuse;
            Specular = other.Specular;
            SpecularPow = other.SpecularPow;
            Shine = other.Shine;
            Colour[0] = other.Colour[0];
            Colour[1] = other.Colour[1];
            Colour[2] = other.Colour[2];
            Colour[3] = other.Colour[3];
            tfWrapX = other.tfWrapX;
            tfWrapY = other.tfWrapY;
            tfMinLinear = other.tfMinLinear;
            tfMagLinear = other.tfMagLinear;
            tfBlend = other.tfBlend;
            tfBlendAdd = other.tfBlendAdd;
            tfNoTexture = other.tfNoTexture;
            tfDepthOffset = other.tfDepthOffset;
            tfNoWriteZ = other.tfNoWriteZ;
            TextureID = other.TextureID;
            AlphaTestVal = other.AlphaTestVal;
            tfTranspBorder = other.tfTranspBorder;
            tfTestA = other.tfTestA;
            tfTestZ = other.tfTestZ;
            BumpMapped = false;
        }

        bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public void Apply(BasicEffect be)
        {
            RasterizerState stat = new RasterizerState();
            if (tfDoubleSided)
                stat.CullMode = CullMode.None;
            else
                stat.CullMode = CullMode.CullClockwiseFace;

            if (ObjectViewer.Culling)
                stat.CullMode = CullMode.CullClockwiseFace;

            stat.DepthBias = -tfDepthOffset / 200000.0f;

            Form1.graphics.BlendState = BlendState.Opaque;

            if (tfBlend)
            {
                Form1.graphics.BlendState = BlendState.AlphaBlend;
            }
            if (tfBlendAdd)
            {
                Form1.graphics.BlendState = BlendState.Additive;
            }


            SamplerState ss = new SamplerState();

            ss.AddressU = TextureAddressMode.Clamp;
            ss.AddressV = TextureAddressMode.Clamp;

            Form1.graphics.DepthStencilState = DepthStencilState.Default;
            if (tfNoWriteZ)
            {
                Form1.graphics.DepthStencilState = DepthStencilState.DepthRead;
            }

            Form1.graphics.RasterizerState = stat;

            be.AmbientLightColor = new Vector3(Ambient, Ambient, Ambient);
            Vector3 col = new Vector3(Colour[0], Colour[1], Colour[2]);
            col *= Diffuse;
            be.DiffuseColor = col;
            be.SpecularColor = new Vector3(Specular, Specular, Specular);
            be.Texture = Form1.Manager.GetTexture(TextureID);

            if ((tfWrapX) && (IsPowerOfTwo(be.Texture.Width)) && (IsPowerOfTwo(be.Texture.Height)))
            {
                ss.AddressU = TextureAddressMode.Wrap;
            }
            if ((tfWrapY) && (IsPowerOfTwo(be.Texture.Width)) && (IsPowerOfTwo(be.Texture.Height)))
            {
                ss.AddressV = TextureAddressMode.Wrap;
            }
            Form1.graphics.SamplerStates[0] = ss;
        }

        public void Apply(Effect be, bool damaged)
        {
            Texture2D tex = Form1.Manager.GetTexture(TextureID);

            RasterizerState stat = new RasterizerState();
            if (tfDoubleSided)
                stat.CullMode = CullMode.None;
            else
                stat.CullMode = CullMode.CullClockwiseFace;

            if (ObjectViewer.Culling)
                stat.CullMode = CullMode.CullCounterClockwiseFace;

            stat.DepthBias = -tfDepthOffset / 10000.0f;

            #region Blendstate
            Form1.graphics.BlendState = BlendState.Opaque;

            if (tfBlend)
            {
                Form1.graphics.BlendState = BlendState.AlphaBlend;
            }
            if (tfBlendAdd)
            {
                Form1.graphics.BlendState = BlendState.Additive;
            }
            #endregion

            #region DepthStencil
            Form1.graphics.DepthStencilState = DepthStencilState.Default;
            if (tfNoWriteZ)
            {
                Form1.graphics.DepthStencilState = DepthStencilState.DepthRead;
            }

            #endregion

            SamplerState ss = new SamplerState();
            ss.AddressU = TextureAddressMode.Clamp;
            ss.AddressV = TextureAddressMode.Clamp;
            ss.Filter = TextureFilter.Anisotropic;
            if (tfMinLinear)
                ss.Filter = TextureFilter.Linear;

            if ((tfWrapX) && (IsPowerOfTwo(tex.Width)) && (IsPowerOfTwo(tex.Height)))
            {
                ss.AddressU = TextureAddressMode.Wrap;
            }
            if ((tfWrapY) && (IsPowerOfTwo(tex.Width)) && (IsPowerOfTwo(tex.Height)))
            {
                ss.AddressV = TextureAddressMode.Wrap;
            }


            if (Form1.Lighting)
                be.Parameters["AmbientIntensity"].SetValue(Ambient);
            else
                be.Parameters["AmbientIntensity"].SetValue(1.0f);

            be.Parameters["DiffuseIntensity"].SetValue(Diffuse);
            be.Parameters["SpecularColor"].SetValue(new Vector4(Specular, Specular, Specular, 1));
            if (SpecularPow > 0)
                be.Parameters["Shininess"].SetValue(SpecularPow);
            else
                be.Parameters["Shininess"].SetValue(1);

            be.Parameters["ModelTexture"].SetValue(tex);
            be.Parameters["AlphaCut"].SetValue(AlphaTestVal);
            be.Parameters["SpecularIntensity"].SetValue(Shine);
            Vector4 col = new Vector4(Colour[0], Colour[1], Colour[2], 1.0f);
            col *= Diffuse;
            be.Parameters["DiffuseColor"].SetValue(col);
            be.Parameters["testA"].SetValue(tfTestA);

            if (damaged)
            {
                be.Parameters["testA"].SetValue(true);

            }

            Form1.graphics.SamplerStates[0] = ss;
            Form1.graphics.RasterizerState = stat;


        }

        public void ApplyBumped(Effect be)
        {
            Texture2D tex = Form1.Manager.GetTexture(TextureID);
            Texture2D ntex = Form1.Manager.GetTexture(NormalTex);

            RasterizerState stat = new RasterizerState();
            if (tfDoubleSided)
                stat.CullMode = CullMode.None;
            else
                stat.CullMode = CullMode.CullClockwiseFace;

            if (ObjectViewer.Culling)
                stat.CullMode = CullMode.CullCounterClockwiseFace;

            stat.DepthBias = -tfDepthOffset / 10000.0f;

            #region Blendstate
            Form1.graphics.BlendState = BlendState.Opaque;

            if (tfBlend)
            {
                Form1.graphics.BlendState = BlendState.AlphaBlend;
            }
            if (tfBlendAdd)
            {
                Form1.graphics.BlendState = BlendState.Additive;
            }
            #endregion

            #region DepthStencil
            Form1.graphics.DepthStencilState = DepthStencilState.Default;
            if (tfNoWriteZ)
            {
                Form1.graphics.DepthStencilState = DepthStencilState.DepthRead;
            }

            #endregion

            SamplerState ss = new SamplerState();
            ss.AddressU = TextureAddressMode.Clamp;
            ss.AddressV = TextureAddressMode.Clamp;
            ss.Filter = TextureFilter.Anisotropic;
            if (tfMinLinear)
                ss.Filter = TextureFilter.Linear;

            if ((tfWrapX) && (IsPowerOfTwo(tex.Width)) && (IsPowerOfTwo(tex.Height)))
            {
                ss.AddressU = TextureAddressMode.Wrap;
            }
            if ((tfWrapY) && (IsPowerOfTwo(tex.Width)) && (IsPowerOfTwo(tex.Height)))
            {
                ss.AddressV = TextureAddressMode.Wrap;
            }


            if (Form1.Lighting)
                be.Parameters["AmbientIntensity"].SetValue(Ambient);
            else
                be.Parameters["AmbientIntensity"].SetValue(1.0f);

            be.Parameters["DiffuseIntensity"].SetValue(Diffuse);
            be.Parameters["SpecularColor"].SetValue(new Vector4(Specular, Specular, Specular, 1));
            if (SpecularPow > 0)
                be.Parameters["Shininess"].SetValue(SpecularPow);
            else
                be.Parameters["Shininess"].SetValue(1);

            be.Parameters["ModelTexture"].SetValue(tex);
            be.Parameters["AlphaCut"].SetValue(AlphaTestVal);
            be.Parameters["SpecularIntensity"].SetValue(Shine);
            Vector4 col = new Vector4(Colour[0], Colour[1], Colour[2], 1.0f);
            col *= Diffuse;
            be.Parameters["DiffuseColor"].SetValue(col);
            be.Parameters["testA"].SetValue(tfTestA);

            Form1.graphics.SamplerStates[0] = ss;
            Form1.graphics.RasterizerState = stat;

            be.Parameters["NormalMap"].SetValue(ntex);
        }

        public void Load(String name, String dir)
        {
            char[] seperators = new char[] { ' ', '\t' };
            String filename = dir + "/" + name;
            using (TextReader reader = File.OpenText(filename))
            {
                string line;
                string[] parts;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("//"))
                        line = "";
                    parts = line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                    if (line.Contains("tfDoubleSide"))
                    {
                        if (parts.GetLength(0) > 1)
                            tfDoubleSided = parts[1].Equals("1");
                    }
                    if (line.Contains("tfShouldSort"))
                    {
                        Sort = parts[1].Equals("1");
                    }
                    if (line.Contains("Ambient"))
                    {
                        Ambient = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("Diffuse"))
                    {
                        Diffuse = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        if (Diffuse > 1)
                            Diffuse = 1;
                    }
                    if ((line.Contains("Specular")) && (!line.Contains("SpecularPow")))
                    {
                        Specular = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("SpecularPow"))
                    {
                        if (parts.Length > 1)
                            SpecularPow = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        else
                            SpecularPow = 0;
                    }
                    if (line.Contains("Shine"))
                    {
                        Shine = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("TextureName"))
                    {
                        if (parts.GetLength(0) > 1)
                        {
                            TextureID = Form1.Manager.AddTexture(parts[1], dir);
                            tname = parts[1];
                        }
                    }
                    if (line.Contains("AlphaTestVal"))
                    {
                        AlphaTestVal = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("tfTestA"))
                    {
                        tfTestA = parts[1].Equals("1");
                    }
                    if (line.Contains("tfWrapX"))
                    {
                        tfWrapX = parts[1].Equals("1");
                    }
                    if (line.Contains("tfWrapY"))
                    {
                        tfWrapY = parts[1].Equals("1");
                    }
                    if (line.Contains("tfMinLinear"))
                    {
                        tfMinLinear = parts[1].Equals("1");
                    }
                    if ((line.Contains("tfBlend")) && (!line.Contains("tfBlendAdd")))
                    {
                        tfBlend = parts[1].Equals("1");
                    }
                    if (line.Contains("tfBlendAdd"))
                    {
                        tfBlendAdd = parts[1].Equals("1");
                    }
                    if (line.Contains("tfNoWriteZ"))
                    {
                        tfNoWriteZ = parts[1].Equals("1");
                    }
                    if (line.Contains("tfTestA"))
                    {
                        tfTestA = parts[1].Equals("1");
                    }
                    if (line.Contains("tfTestZ"))
                    {
                        tfTestZ = parts[1].Equals("1");
                    }
                    if (line.Contains("tfDepthOffset"))
                    {
                        tfDepthOffset = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("tfTranspBorder"))
                    {
                        tfTranspBorder = parts[1].Equals("1");
                    }
                    if (line.Contains("ColorScale"))
                    {
                        Colour[0] = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        Colour[1] = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                        Colour[2] = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (line.Contains("BasedOn"))
                    {
                        Load(parts[1], dir);
                    }
                }
                reader.Close();
            }
        }

        public void Serialise(String dir)
        {
            String toname = Name.Replace('"', '_');
            String filename = Path.Combine(dir, toname);
            filename += ".mat";
            using (TextWriter writer = File.CreateText(filename))
            {
                writer.WriteLine("[ClassInfo]");
                writer.WriteLine("  ClassName TMaterial");
                writer.WriteLine("[General]");
                if (tfDoubleSided)
                    writer.WriteLine("  tfDoubleSide 1");
                else
                    writer.WriteLine("  tfDoubleSide 0");

                if (Sort)
                    writer.WriteLine("  tfShouldSort 1");
                else
                    writer.WriteLine("  tfShouldSort 0");

                writer.WriteLine("  tfDropShadow 0");
                writer.WriteLine("  tfGameTimer 1");

                writer.WriteLine("[LightParams]");
                writer.WriteLine(String.Format("  Ambient {0}", Ambient));
                writer.WriteLine(String.Format("  Diffuse {0}", Diffuse));
                writer.WriteLine(String.Format("  Specular {0}", Specular));
                writer.WriteLine(String.Format("  SpecularPow {0}", SpecularPow));
                writer.WriteLine(String.Format("  Shine {0}", Shine));

                writer.WriteLine("[Layer0]");
                writer.WriteLine(String.Format("  TextureName {0}", tname));
                writer.WriteLine("  PaletteName");
                writer.WriteLine("  Frame 0.0");
                writer.WriteLine("  VisibleDistanceNear 0.0");
                writer.WriteLine("  VisibleDistanceFar 10000.0");
                writer.WriteLine("  TextureCoordScale 0.0 0.0 1.0 1.0");
                writer.WriteLine("  ColorScale 1.0 1.0 1.0 1.0");
                writer.WriteLine(String.Format("  AlphaTestVal {0}", AlphaTestVal));

                if (tfWrapX)
                    writer.WriteLine("  tfWrapX 1");
                else
                    writer.WriteLine("  tfWrapX 0");

                if (tfWrapY)
                    writer.WriteLine("  tfWrapY 1");
                else
                    writer.WriteLine("  tfWrapY 0");

                if (tfMinLinear)
                    writer.WriteLine("  tfMinLinear 1");
                else
                    writer.WriteLine("  tfMinLinear 0");

                if (tfMagLinear)
                    writer.WriteLine("  tfMagLinear 1");
                else
                    writer.WriteLine("  tfMagLinear 0");

                writer.WriteLine("  tfMipMap 0");

                if (tfBlend)
                    writer.WriteLine("  tfBlend 1");
                else
                    writer.WriteLine("  tfBlend 0");

                if (tfBlendAdd)
                    writer.WriteLine("  tfBlendAdd 1");
                else
                    writer.WriteLine("  tfBlendAdd 0");

                if (tfTestA)
                    writer.WriteLine("  tfTestA 1");
                else
                    writer.WriteLine("  tfTestA 0");

                if (tfTestZ)
                    writer.WriteLine("  tfTestZ 1");
                else
                    writer.WriteLine("  tfTestZ 0");

                writer.WriteLine("  tfUpDateClear 0");
                writer.WriteLine("  tfModulate 1");
                writer.WriteLine("  tfNoTexture 0");
                writer.WriteLine("  tfAnimatePalette 0");
                writer.WriteLine("  tfAnimateSkippedFrames 0");

                if (tfNoWriteZ)
                    writer.WriteLine("  tfNoWriteZ 1");
                else
                    writer.WriteLine("  tfNoWriteZ 0");

                writer.WriteLine(String.Format("  tfDepthOffset {0}", tfDepthOffset));

                if (tfTranspBorder)
                    writer.WriteLine("  tfTranspBorder 1");
                else
                    writer.WriteLine("  tfTranspBorder 0");

                writer.WriteLine("  tfTestZEqual 0");
                writer.Close();
            }
        }

        public void SaveDAEEffects(TextWriter writer, bool effect)
        {
            if (!Form1.written_effects.Contains(Name))
            {
                if (effect)
                {
                    writer.WriteLine(String.Format("\t\t<effect id=\"{0}-effect\">", Name));
                    writer.WriteLine("\t\t\t<profile_COMMON>");
                    writer.WriteLine("\t\t\t\t<technique sid=\"common\">");
                    writer.WriteLine("\t\t\t\t\t<phong>");
                    writer.WriteLine("\t\t\t\t\t<emission>");
                    writer.WriteLine("\t\t\t\t\t\t<color>0 0 0 1</color>");
                    writer.WriteLine("\t\t\t\t\t</emission>");
                    writer.WriteLine("\t\t\t\t\t<ambient>");
                    writer.WriteLine(String.Format("\t\t\t\t\t\t<color>{0} {0} {0} 1</color>", Ambient));
                    writer.WriteLine("\t\t\t\t\t</ambient>");
                    writer.WriteLine("\t\t\t\t\t<diffuse>");
                    writer.WriteLine(String.Format("\t\t\t\t\t\t<color>{0} {0} {0} 1</color>", Diffuse));
                    writer.WriteLine("\t\t\t\t\t</diffuse>");
                    writer.WriteLine("\t\t\t\t\t<specular>");
                    writer.WriteLine(String.Format("\t\t\t\t\t\t<color>{0} {0} {0} 1</color>", Specular));
                    writer.WriteLine("\t\t\t\t\t</specular>");
                    writer.WriteLine("\t\t\t\t\t<shininess>");
                    writer.WriteLine(String.Format("\t\t\t\t\t\t<float>{0}</float>", Shine));
                    writer.WriteLine("\t\t\t\t\t</shininess>");
                    writer.WriteLine("\t\t\t\t\t<index_of_refraction>");
                    writer.WriteLine("\t\t\t\t\t\t<float>1</float>");
                    writer.WriteLine("\t\t\t\t\t</index_of_refraction>");
                    writer.WriteLine("\t\t\t\t</phong>");
                    writer.WriteLine("\t\t\t</technique>");
                    writer.WriteLine("\t\t\t</profile_COMMON>");
                    writer.WriteLine("\t\t\t<extra>");
                    writer.WriteLine("\t\t\t\t<technique profile=\"IL2\">");
                    writer.WriteLine(String.Format("\t\t\t\t\t<double_sided>{0}</double_sided>", tfDoubleSided));
                    writer.WriteLine("\t\t\t\t</technique>");
                    writer.WriteLine("\t\t\t</extra>");
                    writer.WriteLine("\t\t</effect>");
                }
                else
                {
                    writer.WriteLine(String.Format("\t\t<material id=\"{0}-material\" name=\"{0}\">", Name));
                    writer.WriteLine(String.Format("\t\t<instance_effect url=\"#{0}-effect\"/>", Name));
                    writer.WriteLine("\t\t</material>");
                }
                Form1.written_effects.Add(Name);
            }
        }

        public void Saveobj(TextWriter writer)
        {
            writer.WriteLine("newmtl " + Name);
            writer.WriteLine("\tNs " + Shine);
            writer.WriteLine(String.Format("\tKs {0} {1} {2}", Specular, Specular, Specular));
            writer.WriteLine(String.Format("\tKa {0} {1} {2}", Ambient, Ambient, Ambient));
            writer.WriteLine(String.Format("\tKd {0} {1} {2}", Diffuse, Diffuse, Diffuse));
            if (Glass)
                writer.WriteLine("\tillum 6");
            else
                writer.WriteLine("\tillum 1");
            writer.WriteLine("\td " + AlphaTestVal);
            writer.WriteLine("\tmap_Kd " + tname.Replace(".tga",".png"));
            writer.WriteLine("");

        }

        public void AdjustLighting(bool isstrelk)
        {
            //if (!Glass)
            //{
            //    Ambient = 0.4f;
            //    Diffuse = 0.9f;
            //    Specular = 1.0f;
            //}
            if (Name.Contains("Propellor"))
            {
                tfBlend = true;
                Sort = true;
                AlphaTestVal = 0.1f;
                tfTestA = true;
            }
            if (Name.Contains("Gloss"))
            {
                Shine = 0.4f;
                SpecularPow = 4.0f;
            }
            if (Name.Contains("Glass") || Name.Contains("glass"))
            {
                tfTestA = true;
            }
            if (Name.Contains("Overlay"))
            {
                tfTestA = true;
                //tfBlend = true;
                Sort = true;
                tfNoWriteZ = true;
            }
            if ((isstrelk) || (tname.Contains("prib")) || (tname.Contains("text")))
            {
                Sort = true;
                tfTestA = true;
                AlphaTestVal = 0.1f;
            }
        }

        public void SaveToFox1(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(tfDoubleSided);
            bw.Write(Sort);
            bw.Write(Glass);
            bw.Write(Ambient);
            bw.Write(Diffuse);
            bw.Write(Specular);
            bw.Write(SpecularPow);
            bw.Write(Shine);
            bw.Write(tfWrapX);
            bw.Write(tfWrapY);
            bw.Write(tfMinLinear);
            bw.Write(tfMagLinear);
            bw.Write(tfBlend);
            bw.Write(tfBlendAdd);
            bw.Write(tfNoTexture);
            bw.Write(tfDepthOffset);
            bw.Write(tfNoWriteZ);
            bw.Write(AlphaTestVal);
            bw.Write(tfTranspBorder);
            bw.Write(tfTestA);
            bw.Write(tfTestZ);
            String tex = Path.GetFileNameWithoutExtension(tname);

            if (tname.StartsWith(".."))
            {
                tex = "$" + Path.GetFileNameWithoutExtension(tname);
            }

            bw.Write(tex);
            bw.Write(Colour[0]);
            bw.Write(Colour[1]);
            bw.Write(Colour[2]);
            bw.Write(Colour[3]);

        }


    }
}
