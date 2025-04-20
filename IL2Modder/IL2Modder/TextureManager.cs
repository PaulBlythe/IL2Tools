using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IL2Modder.AC3D;
using System.Runtime.InteropServices;

namespace IL2Modder
{
    public class TextureImage
    {
        public byte[] imageData;	 // Image Data (Up To 32 Bits)
        public int bpp;	 // Image Color Depth In Bits Per Pixel.
        public int width;	 // Image Width
        public int height;   // Image Height

        public bool IsIMF = false;

        public void Swap()
        {
            int src = 0;

            for(int i=0; i<width*height; i++)
            {
                byte r = imageData[src];
                byte b = imageData[src + 2];
                imageData[src] = b;
                imageData[src + 2] = r;
                src += 4;
            }
        }
    }

    public class TXL
    {
        public List<Texture2D> Textures = new List<Texture2D>();
        public List<int> Hashes = new List<int>();
        public List<int> Indices = new List<int>();
        public int nFrames = 0;

        public float delta_t = 0;
        public int current_index = 0;
        public float counter = 0;

        public Texture2D GetTexture()
        {
            counter += 0.02f;
            if (counter >= delta_t)
            {
                counter -= delta_t;
                current_index++;
                if (current_index == nFrames)
                    current_index = 0;
            }
            int indx = Indices[current_index];
            return Textures[indx];
        }
    }

    public class TextureManager
    {
        public List<Texture2D> Textures = new List<Texture2D>();
        public List<String> Names = new List<string>();
        public List<int> Hashes = new List<int>();
        public List<TXL> animated_textures = new List<TXL>();

        Texture2D white = null;

        int null_count = 0;

        public TextureManager()
        {

        }

        public void Dispose()
        {
            Textures.Clear();
            Names.Clear();
            Hashes.Clear();
        }
        public void ReplaceTexture(String name, int slot)
        {
            if (name.EndsWith("tga", StringComparison.OrdinalIgnoreCase))
            {
                TextureImage t = LoadTGA(name);
                if (t == null)
                    throw new Exception("Invalid texture " + name);

                Texture2D tex = new Texture2D(Form1.graphics, t.width, t.height);
                tex.SetData<byte>(t.imageData);
                Textures[slot] = tex;
            }

            if (name.EndsWith("png",StringComparison.OrdinalIgnoreCase))
            {
                Image ib = Image.FromFile(name);
                Bitmap b = (Bitmap)ib;
                // get source bitmap pixel format size
                int Depth = System.Drawing.Bitmap.GetPixelFormatSize(b.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 24 and 32 bpp images are supported.");
                }
                int Width = b.Width;
                int Height = b.Height;
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Width, Height);

                // Lock bitmap and return bitmap data
                BitmapData bitmapData = b.LockBits(rect, ImageLockMode.ReadWrite, b.PixelFormat);
                int step = Depth / 8;
                byte[] Pixels = new byte[Width * Height * step];
                IntPtr Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
                b.UnlockBits(bitmapData);

                Texture2D tex = new Texture2D(Form1.graphics, Width, Height);
                if (Depth == 24)
                {
                    byte[] npixels = new byte[Width * Height * 4];
                    int re = 0;
                    int wr = 0;
                    for (int i = 0; i < Width * Height; i++)
                    {
                        npixels[wr++] = Pixels[re++];
                        npixels[wr++] = Pixels[re++];
                        npixels[wr++] = Pixels[re++];
                        npixels[wr++] = 255;

                    }
                    tex.SetData<byte>(npixels);
                    Textures[slot] = tex;
                }
                else if (Depth == 8)
                {
                    byte[] npixels = new byte[Width * Height * 4];
                    int pos = 0;
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            System.Drawing.Color c = b.GetPixel(x, y);
                            npixels[pos++] = c.B;
                            npixels[pos++] = c.G;
                            npixels[pos++] = c.R;
                            npixels[pos++] = c.A;
                        }
                    }
                    tex.SetData<byte>(npixels);
                    Textures[slot] = tex;
                }
                else
                {
                    tex.SetData<byte>(Pixels);
                    Textures[slot] = tex;
                }
            }

            if (name.EndsWith("bmp", StringComparison.OrdinalIgnoreCase))
            {
                Image ib = Bitmap.FromFile(name);
                Bitmap b = (Bitmap)ib;

                // get source bitmap pixel format size
                int Depth = System.Drawing.Bitmap.GetPixelFormatSize(b.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 24 and 32 bpp images are supported.");
                }
                int Width = b.Width;
                int Height = b.Height;
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Width, Height);

                // Lock bitmap and return bitmap data
                BitmapData bitmapData = b.LockBits(rect, ImageLockMode.ReadWrite, b.PixelFormat);
                int step = Depth / 8;
                byte[] Pixels = new byte[Width * Height * step];
                IntPtr Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
                b.UnlockBits(bitmapData);

                Texture2D tex = new Texture2D(Form1.graphics, Width, Height);
                if (Depth == 24)
                {
                    byte[] npixels = new byte[Width * Height * 4];
                    int re = 0;
                    int wr = 0;
                    for (int i = 0; i < Width * Height; i++)
                    {
                        npixels[wr++] = Pixels[re++];
                        npixels[wr++] = Pixels[re++];
                        npixels[wr++] = Pixels[re++];
                        npixels[wr++] = 255;

                    }
                    tex.SetData<byte>(npixels);
                    Textures[slot] = tex;
                }
                else if (Depth == 8)
                {
                    byte[] npixels = new byte[Width * Height * 4];
                    int pos = 0;
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            System.Drawing.Color c = b.GetPixel(x, y);
                            npixels[pos++] = c.B;
                            npixels[pos++] = c.G;
                            npixels[pos++] = c.R;
                            npixels[pos++] = c.A;
                        }
                    }
                    tex.SetData<byte>(npixels);
                    Textures[slot] = tex;
                }
                else
                {
                    tex.SetData<byte>(Pixels);
                    Textures[slot] = tex;
                }

            }
        }

        public int AddTexture(String name, String dir)
        {
            String hash;

            if (name == null)
            {
                hash = Path.Combine(dir, null_count.ToString());
                Names.Add(null_count.ToString());
                Texture2D ntex = new Texture2D(Form1.graphics, 1, 1);
                Textures.Add(ntex);
                null_count++;
                return Textures.Count - 1;
            }
            else
            {
                hash = Path.Combine(dir, name);
            }
            int hc = hash.GetHashCode();

            String file;

            if (Hashes.Contains(hc))
            {
                return Hashes.IndexOf(hc);
            }
            if (name.StartsWith(".."))
            {
                if ((name.Contains("textures")) || (name.Contains("TEXTURES")) || (name.Contains("Textures")))
                    file = Form1.TexturesDirectory + "\\" + Path.GetFileName(name);
                else
                    file = Path.Combine(dir, name);
            }
            else
            {
                file = dir + "\\summer\\" + name;
                if (!File.Exists(file))
                {
                    file = dir + "\\" + name;
                    if (!File.Exists(file))
                    {
                        if (name.EndsWith("tga"))
                        {
                            file = Path.GetFileNameWithoutExtension(name);
                            file = Path.GetDirectoryName(name) + file + ".tgb";
                            if (!File.Exists(file))
                            {
                                file = "";
                                foreach (String s in Form1.project.SearchPaths)
                                {
                                    String nfile = s + "/" + Path.GetFileName(name);
                                    if (File.Exists(nfile))
                                    {
                                        file = nfile;
                                    }
                                }

                                if (file == "")
                                {
                                    MissingTexture mt = new MissingTexture(Path.GetFileName(name));
                                    DialogResult r = mt.ShowDialog();
                                    if (r == DialogResult.OK)
                                    {
                                        foreach (String s in Form1.project.SearchPaths)
                                        {
                                            String nfile = s + "/" + Path.GetFileName(name);
                                            if (File.Exists(nfile))
                                            {
                                                file = nfile;
                                            }
                                        }
                                    }
                                    else if (r == DialogResult.Ignore)
                                    {
                                        Names.Add(name);
                                        Hashes.Add(hc);
                                        Texture2D ntex = new Texture2D(Form1.graphics, 1, 1);
                                        Textures.Add(ntex);
                                        return Textures.Count - 1;
                                    }
                                }
                            }

                        }

                    }
                }
            }
            TextureImage t = null;
            String type = Path.GetExtension(file);
            switch (type)
            {
                case ".TGA":
                case ".tga":
                case ".tgb":
                    {
                        t = LoadTGA(file);
                        if (t == null)
                            throw new Exception("Invalid texture " + name);
                    }
                    break;
                case ".txl":
                    {
                        LoadTXL(file);
                        return - animated_textures.Count;
                    }

                default:
                    {
                        t = LoadBMP(file);
                        if (t == null)
                            throw new Exception("Invalid texture " + name);
                    }
                    break;
            }


            Hashes.Add(hc);
            Names.Add(name);
            Texture2D tex = new Texture2D(Form1.graphics, t.width, t.height);
            tex.SetData<byte>(t.imageData);
            Textures.Add(tex);
            return Textures.Count - 1;

        }

        public Texture2D GetTexture(int id)
        {
            if (Textures.Count == 0)
            {
                if (white == null)
                {
                    white = new Texture2D(Form1.graphics, 1, 1);
                    byte[] tes = new byte[] { 255, 255, 255, 255 };
                    white.SetData<byte>(tes);
                }
                return white;
            }
            if (id<0)
            {
                int anm = (- id) -1 ;
                return animated_textures[anm].GetTexture();
            }
            return Textures[id];
        }

        bool Compare(byte[] array1, byte[] array2, int count)
        {
            // if continue the 2 lenghts are the same
            for (int i = 0; i < count; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private byte Average(byte a, byte b)
        {
            int x = ((int)a);
            int y = ((int)b);
            return (byte)((x + y) / 2);
        }

        private int paethPredictor(int a, int b, int c)
        {
            a = a & 255;
            b = b & 255;
            c = c & 255;
            int p = (a + b - c);
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);

            if ((pa <= pb) && (pa <= pc))
                return a;

            if (pb <= pc)
                return b;

            return c;
        }

        void LoadTXL(string filename)
        {
            TXL txl = new TXL();
            String dir = Path.GetDirectoryName(filename);

            using (TextReader tr = File.OpenText(filename))
            {
                char[] seps = new char[] { ' ', '\t', '\a' };
                string line;
                int mode = 0;

                while ((line = tr.ReadLine()) != null)
                {
                    switch (mode)
                    {
                        case 0:
                            {
                                string[] parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length > 0)
                                {
                                    if (parts[0] == "nFrames")
                                        txl.nFrames = int.Parse(parts[1]);
                                    if (parts[0] == "FPS")
                                        txl.delta_t = 1.0f / float.Parse(parts[1]);
                                    if (parts[0] == "[Images]")
                                        mode++;
                                }
                            }
                            break;
                        case 1:
                            {
                                if (line.Length > 1)
                                {
                                    if (line.StartsWith("["))
                                        mode++;
                                    else
                                    {
                                        int hash = line.GetHashCode();
                                        if (txl.Hashes.Contains(hash))
                                        {
                                            int indx = txl.Hashes.IndexOf(hash);
                                            txl.Indices.Add(indx);
                                        }else
                                        {
                                            String file = Path.Combine(dir, line);
                                            TextureImage ti = LoadTGA(file);
                                            txl.Hashes.Add(hash);
                                            txl.Indices.Add(txl.Hashes.Count - 1);
                                            Texture2D tex = new Texture2D(Form1.graphics, ti.width, ti.height);
                                            tex.SetData<byte>(ti.imageData);
                                            txl.Textures.Add(tex);
                                        }

                                    }
                                }
                            }
                            break;
                    }
                }
            }
            animated_textures.Add(txl);
        }

        TextureImage LoadIMF(FileStream file)
        {
            int width;
            int height;
            int imageSize;
            byte[] data;
            byte[] alpha;
            byte flag;

            TextureImage texture = new TextureImage();
            texture.IsIMF = true;
            file.Position = 7;
            using (BinaryReader br = new BinaryReader(file))
            {
                flag = br.ReadByte();
                width = (int)br.ReadUInt16();
                height = (int)br.ReadUInt16();

                imageSize = width * height * 4;
                data = new byte[imageSize];
                byte[] mode = br.ReadBytes(height);
                alpha = new byte[width * height];
                byte[] dalpha = new byte[width * height * 3];

                int pos = 0;
                int pos2 = 0;
                int prev = width * 4;
                byte r = 0;
                byte g = 0;
                byte b = 0;
                byte a = 0;

                for (int y = 0; y < height; y++)
                {
                    if (mode[y] < 4)
                    {
                        r = g = b = 0;
                    }
                    for (int x = 0; x < width; x++)
                    {
                        #region Decode
                        switch (mode[y])
                        {
                            // none
                            case 0:
                                r = br.ReadByte();
                                g = br.ReadByte();
                                b = br.ReadByte();
                                a = 255;
                                break;

                            // sub
                            case 1:
                                r += br.ReadByte();
                                g += br.ReadByte();
                                b += br.ReadByte();
                                a = 255;
                                break;

                            // up
                            case 2:
                                b = data[pos - prev];
                                g = data[pos - prev + 1];
                                r = data[pos - prev + 2];

                                r += br.ReadByte();
                                g += br.ReadByte();
                                b += br.ReadByte();
                                a = 255;
                                break;

                            // average
                            case 3:
                                r = Average(r, data[pos - prev + 2]);
                                g = Average(g, data[pos - prev + 1]);
                                b = Average(b, data[pos - prev]);

                                r += br.ReadByte();
                                g += br.ReadByte();
                                b += br.ReadByte();
                                a = 255;
                                break;

                            // paeth
                            case 4:

                                if (x > 0)
                                {
                                    r = (byte)paethPredictor(r, data[pos - prev + 2], data[pos - prev - 2]);
                                    g = (byte)paethPredictor(g, data[pos - prev + 1], data[pos - prev - 3]);
                                    b = (byte)paethPredictor(b, data[pos - prev], data[pos - prev - 4]);
                                }
                                else
                                {
                                    r = (byte)paethPredictor(data[pos - prev + 2], data[pos - prev + 2], 0);//data[pos - prev - 2]);
                                    g = (byte)paethPredictor(data[pos - prev + 1], data[pos - prev + 1], 0);//data[pos - prev - 3]);
                                    b = (byte)paethPredictor(data[pos - prev], data[pos - prev], 0);//data[pos - prev - 4]);
                                }

                                r += br.ReadByte();
                                g += br.ReadByte();
                                b += br.ReadByte();
                                a = 255;
                                break;

                        }
                        #endregion

                        data[pos++] = b;
                        data[pos++] = g;
                        data[pos++] = r;
                        data[pos++] = a;

                    }
                }
                if (flag > 0)
                {
                    pos = 0;
                    mode = br.ReadBytes(height);
                    for (int y = 0; y < height; y++)
                    {
                        if (mode[y] < 4)
                        {
                            r = 0;
                        }
                        for (int x = 0; x < width; x++)
                        {
                            switch (mode[y])
                            {
                                case 0:
                                    r = br.ReadByte();
                                    break;
                                case 1:
                                    r += br.ReadByte();
                                    break;
                                case 2:
                                    r = alpha[pos - width];
                                    r += br.ReadByte();
                                    break;
                                case 3:
                                    if (y == 0)
                                    {
                                        r = Average(r, 0);
                                    }
                                    else
                                    {
                                        r = Average(r, alpha[pos - width]);
                                    }
                                    r += br.ReadByte();
                                    break;
                                case 4:
                                    {
                                        if (x > 0)
                                        {
                                            r = (byte)paethPredictor(r, alpha[pos - width], alpha[pos - width - 1]);
                                            r += br.ReadByte();
                                        }
                                        else
                                        {
                                            if (y > 1)
                                                r = (byte)paethPredictor(alpha[pos - width], alpha[pos - width], alpha[pos - width - 1]);
                                            else
                                                r = (byte)paethPredictor(alpha[pos - width], alpha[pos - width], 0);

                                            r += br.ReadByte();
                                        }
                                    }
                                    break;

                            }
                            alpha[pos++] = r;
                            dalpha[pos2++] = r;
                            dalpha[pos2++] = r;
                            dalpha[pos2++] = r;

                        }
                    }
                }
            }
            texture.width = width;
            texture.height = height;
            texture.imageData = new byte[width * height * 4];
            int i = 0;
            while (i < imageSize)
            {
                texture.imageData[i] = data[i];
                i++;
            }
            if (flag > 0)
            {
                i = 3;
                int j = 0;
                while (i < imageSize)
                {
                    texture.imageData[i] = alpha[j++];
                    i += 4;
                }
            }
            return texture;
        }

        TextureImage LoadTGA(string filename)
        {
            byte[] uTGACompare = { 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 };	 // Uncompressed TGA Header
            byte[] uTGACompare2 = { 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 };	 // Uncompressed TGA Header
            byte[] cTGACompare = { 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0 };	 // Compressed TGA Header
            byte[] iTGACompare = { 73, 77, 70 };                             // IMF
            byte[] TGACompare = new byte[12];	 // Used To Compare TGA Header

            if (!File.Exists(filename))
            {
                Form1.missing_textures.Add(filename);
                TextureImage texture = new TextureImage();
                texture.bpp = 32;
                texture.height = 1;
                texture.width = 1;
                texture.imageData = new byte[4];
                texture.imageData[0] = texture.imageData[1] = texture.imageData[2] = texture.imageData[3] = 0xff;
                return texture;
            }
            FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);	// Open The TGA File
            file.Read(TGACompare, 0, 12);
            if (Compare(TGACompare, iTGACompare, 3))
            {
                TextureImage ta =  LoadIMF(file);
                ta.Swap();
                file.Close();
                return ta;
            }
            file.Close();

            Tga tga = new Tga(filename);
            TextureImage ti = new TextureImage();
            ti.bpp = 32;
            ti.height = tga.ImageHeight;
            ti.width = tga.ImageWidth;
            ti.imageData = tga.ImageData;
            //ti.Swap();
            return ti;

        }

        /// <summary>
        /// This function load an uncompressed TGA
        /// </summary>
        TextureImage LoadUncompressedTGA(FileStream file, string name)
        {
            byte[] header = new byte[6];	    // First 6 Useful Bytes From The Header
            int bytesPerPixel;	                // Holds Number Of Bytes Per Pixel Used In The TGA File
            int imageSize;	                    // Used To Store The Image Size When Setting Aside Ram
            int temp;	                        // Temporary Variable
            TextureImage texture = new TextureImage();
            texture.width = -1;

            if (file == null || file.Read(header, 0, 6) != 6)
            {
                if (file == null)
                    return null;
                else
                {
                    file.Close();
                    return null;
                }
            }
            texture.width = header[1] * 256 + header[0];	 // Determine The TGA Width	(highbyte*256+lowbyte)
            texture.height = header[3] * 256 + header[2];	 // Determine The TGA Height	(highbyte*256+lowbyte)

            if (texture.width <= 0 || texture.height <= 0 || (header[4] != 24 && header[4] != 32))	 // Is The TGA 24 or 32 Bit?
            {

                file.Close();
                return null;
            }
            texture.bpp = header[4];	 // Grab The TGA's Bits Per Pixel (24 or 32)
            bytesPerPixel = texture.bpp / 8;	 // Divide By 8 To Get The Bytes Per Pixel
            imageSize = texture.width * texture.height * bytesPerPixel;	// Calculate The Memory Required For The TGA Data 
            texture.imageData = new byte[imageSize];	 // Reserve Memory To Hold The TGA Data
            if (imageSize == 0 || file.Read(texture.imageData, 0, imageSize) != imageSize)
            {
                if (texture.imageData != null)
                    texture.imageData = null;

                file.Close();
                return null;
            }
            for (int i = 0; i < imageSize; i += bytesPerPixel)	 // Loop Through The Image Data
            {	 // Swaps The 1st And 3rd Bytes ('R'ed and 'B'lue)
                temp = texture.imageData[i];	 // Temporarily Store The Value At Image Data 'i'
                texture.imageData[i] = texture.imageData[i + 2];	 // Set The 1st Byte To The Value Of The 3rd Byte
                texture.imageData[i + 2] = (byte)temp;	 // Set The 3rd Byte To The Value In 'temp' (1st Byte Value)
            }

            file.Close();

            // fucking xna 4 doesn't support RGB textures
            if (bytesPerPixel == 3)
            {
                byte[] copy = new byte[4 * texture.width * texture.height];
                int rp = 0;
                int wp = 0;
                for (int y = 0; y < texture.height; y++)
                {
                    for (int x = 0; x < texture.width; x++)
                    {
                        int r = texture.imageData[rp];
                        r += texture.imageData[rp + 1];
                        r += texture.imageData[rp + 2];
                        r /= 3;
                        copy[wp++] = texture.imageData[rp++];
                        copy[wp++] = texture.imageData[rp++];
                        copy[wp++] = texture.imageData[rp++];
                        copy[wp++] = (byte)r;
                    }
                }

                texture.imageData = copy;

            }
            return texture;
        }

        TextureImage LoadCompressedTGA(FileStream file)
        {
            byte[] header = new byte[6];	    // First 6 Useful Bytes From The Header
            int bytesPerPixel;	                // Holds Number Of Bytes Per Pixel Used In The TGA File
            int imageSize;	                    // Used To Store The Image Size When Setting Aside Ram
            int temp;	                        // Temporary Variable
            TextureImage texture = new TextureImage();
            texture.width = -1;

            if (file == null || file.Read(header, 0, 6) != 6)
            {
                if (file == null)
                    return null;
                else
                {
                    file.Close();
                    return null;
                }
            }
            texture.width = header[1] * 256 + header[0];	 // Determine The TGA Width	(highbyte*256+lowbyte)
            texture.height = header[3] * 256 + header[2];	 // Determine The TGA Height	(highbyte*256+lowbyte)

            if (texture.width <= 0 || texture.height <= 0 || (header[4] != 24 && header[4] != 32))	 // Is The TGA 24 or 32 Bit?
            {
                file.Close();
                return null;
            }
            texture.bpp = header[4];	                    // Grab The TGA's Bits Per Pixel (24 or 32)
            bytesPerPixel = texture.bpp / 8;	            // Divide By 8 To Get The Bytes Per Pixel
            imageSize = texture.width * texture.height * bytesPerPixel;	// Calculate The Memory Required For The TGA Data 
            texture.imageData = new byte[imageSize];        // Reserve Memory To Hold The TGA Data

            int pos = 0;
            byte[] re = new byte[1];
            byte[] pixel = new byte[bytesPerPixel];
            while (pos < imageSize)                                                  // Start Loop
            {
                byte[] chunkheader = new byte[1];              // Variable To Store The Value Of The Id Chunk
                file.Read(chunkheader, 0, 1);
                if (chunkheader[0] < 128)                        // If The Chunk Is A 'RAW' Chunk
                {
                    chunkheader[0]++;                           // Add 1 To The Value To Get Total Number Of Raw Pixels
                    int n = chunkheader[0] * bytesPerPixel;
                    while (n > 0)
                    {
                        file.Read(re, 0, 1);
                        texture.imageData[pos++] = re[0];
                        n--;
                    }
                }
                else                                            // If It's An RLE Header
                {
                    chunkheader[0] -= 127;                      // Subtract 127 To Get Rid Of The ID Bit
                    file.Read(pixel, 0, bytesPerPixel);
                    for (short counter = 0; counter < chunkheader[0]; counter++)
                    {
                        for (int i = 0; i < bytesPerPixel; i++)
                            texture.imageData[pos++] = pixel[i];
                    }
                }

            }
            file.Close();
            for (int i = 0; i < imageSize; i += bytesPerPixel)	 // Loop Through The Image Data
            {	 // Swaps The 1st And 3rd Bytes ('R'ed and 'B'lue)
                temp = texture.imageData[i];	 // Temporarily Store The Value At Image Data 'i'
                texture.imageData[i] = texture.imageData[i + 2];	 // Set The 1st Byte To The Value Of The 3rd Byte
                texture.imageData[i + 2] = (byte)temp;	 // Set The 3rd Byte To The Value In 'temp' (1st Byte Value)
            }
            // fucking xna 4 doesn't support RGB textures
            if (bytesPerPixel == 3)
            {
                byte[] copy = new byte[4 * texture.width * texture.height];
                int rp = 0;
                int wp = 0;
                for (int y = 0; y < texture.height; y++)
                {
                    for (int x = 0; x < texture.width; x++)
                    {
                        copy[wp++] = texture.imageData[rp++];
                        copy[wp++] = texture.imageData[rp++];
                        copy[wp++] = texture.imageData[rp++];
                        copy[wp++] = 255;
                    }
                }

                texture.imageData = copy;

            }
            return texture;
        }

        TextureImage LoadBMP(String name)
        {
            if (name.EndsWith("rgb"))
                return LoadRGB(name);


            Image ib = Bitmap.FromFile(name);
            Bitmap b = (Bitmap)ib;

            // get source bitmap pixel format size
            int Depth = System.Drawing.Bitmap.GetPixelFormatSize(b.PixelFormat);

            // Check if bpp (Bits Per Pixel) is 8, 24, or 32
            if (Depth != 8 && Depth != 24 && Depth != 32)
            {
                throw new ArgumentException("Only 24 and 32 bpp images are supported.");
            }
            int Width = b.Width;
            int Height = b.Height;
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Width, Height);

            // Lock bitmap and return bitmap data
            BitmapData bitmapData = b.LockBits(rect, ImageLockMode.ReadWrite, b.PixelFormat);
            int step = Depth / 8;
            byte[] Pixels = new byte[Width * Height * step];
            IntPtr Iptr = bitmapData.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
            b.UnlockBits(bitmapData);

            TextureImage ti = new TextureImage();
            ti.width = Width;
            ti.height = Height;
            ti.bpp = 4;

            if (Depth == 24)
            {
                ti.imageData = new byte[Width * Height * 4];
                int re = 0;
                int wr = 0;
                for (int i = 0; i < Width * Height; i++)
                {
                    ti.imageData[wr++] = Pixels[re++];
                    ti.imageData[wr++] = Pixels[re++];
                    ti.imageData[wr++] = Pixels[re++];
                    ti.imageData[wr++] = 255;

                }

            }
            else if (Depth == 8)
            {
                ti.imageData = new byte[Width * Height * 4];
                int pos = 0;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        System.Drawing.Color c = b.GetPixel(x, y);
                        ti.imageData[pos++] = c.B;
                        ti.imageData[pos++] = c.G;
                        ti.imageData[pos++] = c.R;
                        ti.imageData[pos++] = c.A;
                    }
                }

            }
            else
            {
                ti.imageData = Pixels;

            }
            return ti;
        }

        public TextureImage LoadRGB(string name)
        {
            SGI_RGB rgb = new SGI_RGB();
            return rgb.LoadFile(name);
        }

        public Vector2 Size(int ID)
        {
            Vector2 res = new Vector2();
            res.X = Textures[ID].Width;
            res.Y = Textures[ID].Height;
            return res;
        }

        public int AddWhite()
        {
            if (white == null)
            {
                white = new Texture2D(Form1.graphics, 1, 1);
                byte[] tes = new byte[] { 255, 255, 255, 255 };
                white.SetData<byte>(tes);

            }
            String w = "white";
            Textures.Add(white);
            Names.Add(w);
            Hashes.Add(w.GetHashCode());
            return Textures.Count - 1;
        }

        public void Serialise(String dir)
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                Texture2D t = Textures[i];
                byte[] textureData = new byte[4 * t.Width * t.Height];
                t.GetData<byte>(textureData);

                for (int j = 0; j < 4 * t.Width * t.Height; j += 4)
                {
                    byte r = textureData[j];
                    byte g = textureData[j + 2];
                    textureData[j + 2] = r;
                    textureData[j] = g;
                }

                Bitmap b = new Bitmap(t.Width, t.Height, PixelFormat.Format32bppArgb);

                System.Drawing.Imaging.BitmapData bmpData = b.LockBits(
                   new System.Drawing.Rectangle(0, 0, t.Width, t.Height),
                   System.Drawing.Imaging.ImageLockMode.WriteOnly,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb
                 );

                IntPtr safePtr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
                b.UnlockBits(bmpData);

                b.Save(dir + "\\" + Path.GetFileNameWithoutExtension(Names[i]) + ".png", ImageFormat.Png);
            }
        }
        public void Serialise(String dir, String name)
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                Texture2D t = Textures[i];
                byte[] textureData = new byte[4 * t.Width * t.Height];
                t.GetData<byte>(textureData);

                for (int j = 0; j < 4 * t.Width * t.Height; j += 4)
                {
                    byte r = textureData[j];
                    byte g = textureData[j + 2];
                    textureData[j + 2] = r;
                    textureData[j] = g;
                }

                Bitmap b = new Bitmap(t.Width, t.Height, PixelFormat.Format32bppArgb);

                System.Drawing.Imaging.BitmapData bmpData = b.LockBits(
                   new System.Drawing.Rectangle(0, 0, t.Width, t.Height),
                   System.Drawing.Imaging.ImageLockMode.WriteOnly,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb
                 );

                IntPtr safePtr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
                b.UnlockBits(bmpData);

                b.Save(dir + "\\" + name + "_" + Path.GetFileNameWithoutExtension(Names[i]) + ".png", ImageFormat.Png);
            }
        }

        public void SaveTexture(String src, String dest)
        {
            TextureImage t = null;
            String type = Path.GetExtension(src);
            switch (type)
            {
                case ".tga":
                case ".tgb":
                    {
                        t = LoadTGA(src);
                        if (t == null)
                            throw new Exception("Invalid texture " + src);
                    }
                    break;
                case ".txl":
                    {
                        LoadTXL(src);
                    }
                    break;

                case ".bmp":
                    {
                        t = LoadBMP(src);
                        if (t == null)
                            throw new Exception("Invalid texture " + src);
                    }
                    break;
            }

            if (t != null)
            {
                if (!t.IsIMF)
                    t.Swap();

                Bitmap b = new Bitmap(t.width, t.height, PixelFormat.Format32bppArgb);

                System.Drawing.Imaging.BitmapData bmpData = b.LockBits(new System.Drawing.Rectangle(0, 0, t.width, t.height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                IntPtr safePtr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(t.imageData, 0, safePtr, t.imageData.Length);
                b.UnlockBits(bmpData);

                b.Save(dest, ImageFormat.Png);

            }
        }
    }
}
