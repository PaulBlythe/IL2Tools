using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder
{
    public partial class TextureBrowser : Form
    {
        TextureManager tmain;
        Bitmap b;

        public TextureBrowser(TextureManager manager)
        {
            InitializeComponent();
            tmain = manager;
            foreach (String s in manager.Names)
            {
                listBox1.Items.Add(s);
            }
        }

        /// <summary>
        /// Invert image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Texture2D t = tmain.Textures[listBox1.SelectedIndex];
            byte[] textureData = new byte[4 * t.Width * t.Height];
            t.GetData<byte>(textureData);

            int linelength = t.Width * 4;

            for (int y = 0; y < t.Height/2; y++)
            {
                int rp = (t.Height - 1) - y;
                rp *= linelength;

                int wp = (y * linelength);

                for (int x = 0; x < linelength; x++)
                {
                    byte tp = textureData[rp + x];
                    textureData[rp + x] = textureData[wp + x];
                    textureData[wp + x] = tp;
                    
                }
            }
            Form1.graphics.Textures[0] = null;
            Form1.graphics.Textures[1] = null;
            t.SetData<byte>(textureData);          
            b = new Bitmap(t.Width, t.Height, PixelFormat.Format32bppArgb);
           
            System.Drawing.Imaging.BitmapData bmpData = b.LockBits(
               new System.Drawing.Rectangle(0, 0, t.Width, t.Height),
               System.Drawing.Imaging.ImageLockMode.WriteOnly,
               System.Drawing.Imaging.PixelFormat.Format32bppArgb
             );
            for (int i = 0; i < 4 * t.Width * t.Height; i += 4)
            {
                byte r = textureData[i];
                byte g = textureData[i + 2];
                textureData[i + 2] = r;
                textureData[i] = g;
            }
            IntPtr safePtr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            b.UnlockBits(bmpData);

            pictureBox1.Image = b;
            pictureBox1.Invalidate();
            Application.DoEvents();
        }

        /// <summary>
        /// Close the dialog box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Selection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
                return;

            Texture2D t = tmain.Textures[listBox1.SelectedIndex];
            b = new Bitmap(t.Width, t.Height,PixelFormat.Format32bppArgb);
            byte[] textureData = new byte[4 * t.Width * t.Height];
            t.GetData<byte>(textureData);

            System.Drawing.Imaging.BitmapData bmpData = b.LockBits(
               new System.Drawing.Rectangle(0, 0, t.Width, t.Height),
               System.Drawing.Imaging.ImageLockMode.WriteOnly,
               System.Drawing.Imaging.PixelFormat.Format32bppArgb
             );
            for (int i = 0; i < 4 * t.Width * t.Height; i += 4)
            {
                byte r = textureData[i];
                byte g = textureData[i + 2];
                textureData[i + 2] = r;
                textureData[i] = g;
            }
            IntPtr safePtr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            b.UnlockBits(bmpData);

            pictureBox1.Image = b;
            pictureBox1.Invalidate();
            Application.DoEvents();
        }

        /// <summary>
        /// Replace the texture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "PNG files (*.png)|*.png|TGA files (*.tga)|*.tga|Bitmaps (*.bmp)|*.bmp";
                ofd.DefaultExt = "*.tga";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tmain.ReplaceTexture(ofd.FileName, listBox1.SelectedIndex);

                    Texture2D t = tmain.Textures[listBox1.SelectedIndex];
                    b = new Bitmap(t.Width, t.Height, PixelFormat.Format32bppArgb);
                    byte[] textureData = new byte[4 * t.Width * t.Height];
                    t.GetData<byte>(textureData);

                    System.Drawing.Imaging.BitmapData bmpData = b.LockBits(
                       new System.Drawing.Rectangle(0, 0, t.Width, t.Height),
                       System.Drawing.Imaging.ImageLockMode.WriteOnly,
                       System.Drawing.Imaging.PixelFormat.Format32bppArgb
                     );
                    for (int i = 0; i < 4 * t.Width * t.Height; i += 4)
                    {
                        byte r = textureData[i];
                        byte g = textureData[i + 2];
                        textureData[i + 2] = r;
                        textureData[i] = g;
                    }
                    IntPtr safePtr = bmpData.Scan0;
                    System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
                    b.UnlockBits(bmpData);

                    pictureBox1.Image = b;
                    pictureBox1.Invalidate();
                    Application.DoEvents();
                }
            }
        }

        /// <summary>
        /// Fake alpha
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                Texture2D t = tmain.Textures[listBox1.SelectedIndex];
                byte[] textureData = new byte[4 * t.Width * t.Height];
                t.GetData<byte>(textureData);

                for (int i = 0; i < textureData.Length; i += 4)
                {
                    int value = textureData[i];
                    value += textureData[i + 1];
                    value += textureData[i + 2];
                    value /= 3;
                    textureData[i + 3] = (byte)value;
                }
                Form1.graphics.Textures[0] = null;
                t.SetData<byte>(textureData);
                b = new Bitmap(t.Width, t.Height, PixelFormat.Format32bppArgb);

                System.Drawing.Imaging.BitmapData bmpData = b.LockBits(
                   new System.Drawing.Rectangle(0, 0, t.Width, t.Height),
                   System.Drawing.Imaging.ImageLockMode.WriteOnly,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb
                 );
                for (int i = 0; i < 4 * t.Width * t.Height; i += 4)
                {
                    byte r = textureData[i];
                    byte g = textureData[i + 2];
                    textureData[i + 2] = r;
                    textureData[i] = g;
                }
                IntPtr safePtr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
                b.UnlockBits(bmpData);

                pictureBox1.Image = b;
                pictureBox1.Invalidate();
                Application.DoEvents();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                Texture2D t = tmain.Textures[listBox1.SelectedIndex];
                byte[] textureData = new byte[4 * t.Width * t.Height];
                t.GetData<byte>(textureData);

                for (int i = 0; i < 4 * t.Width * t.Height; i += 4)
                {
                    byte r = textureData[i];
                    byte g = textureData[i + 2];
                    textureData[i + 2] = r;
                    textureData[i] = g;
                }

                Form1.graphics.Textures[0] = null;

                tmain.Textures[listBox1.SelectedIndex].SetData<byte>(textureData);

                b = new Bitmap(t.Width, t.Height, PixelFormat.Format32bppArgb);

                System.Drawing.Imaging.BitmapData bmpData = b.LockBits(
                   new System.Drawing.Rectangle(0, 0, t.Width, t.Height),
                   System.Drawing.Imaging.ImageLockMode.WriteOnly,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb
                 );
                for (int i = 0; i < 4 * t.Width * t.Height; i += 4)
                {
                    byte r = textureData[i];
                    byte g = textureData[i + 2];
                    textureData[i + 2] = r;
                    textureData[i] = g;
                }
                IntPtr safePtr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
                b.UnlockBits(bmpData);
                
               
                pictureBox1.Image = b;
                pictureBox1.Invalidate();
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Alpha to 255
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                Texture2D t = tmain.Textures[listBox1.SelectedIndex];
                byte[] textureData = new byte[4 * t.Width * t.Height];
                t.GetData<byte>(textureData);

                for (int i = 0; i < textureData.Length; i += 4)
                {
                    int tc = textureData[i] + textureData[i + 1] + textureData[i + 2];
                    if (tc > 6)
                        textureData[i + 3] = 255;
                }
                Form1.graphics.Textures[0] = null;
                t.SetData<byte>(textureData);
                b = new Bitmap(t.Width, t.Height, PixelFormat.Format32bppArgb);

                System.Drawing.Imaging.BitmapData bmpData = b.LockBits(
                   new System.Drawing.Rectangle(0, 0, t.Width, t.Height),
                   System.Drawing.Imaging.ImageLockMode.WriteOnly,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb
                 );
                for (int i = 0; i < 4 * t.Width * t.Height; i += 4)
                {
                    byte r = textureData[i];
                    byte g = textureData[i + 2];
                    textureData[i + 2] = r;
                    textureData[i] = g;
                }
                IntPtr safePtr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
                b.UnlockBits(bmpData);

                pictureBox1.Image = b;
                pictureBox1.Invalidate();
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Bump map interface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            NormalMapping nm = new NormalMapping(tmain);
            if (nm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ObjectViewer.Instance.mesh.EnableBumpMapping(listBox1.SelectedIndex, nm.TexID);
            }
        }

        /// <summary>
        /// NULL a texture (set all pixels to 0,0,0,0)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                Texture2D t = tmain.Textures[listBox1.SelectedIndex];
                byte[] textureData = new byte[4 * t.Width * t.Height];
                t.GetData<byte>(textureData);

                for (int i = 0; i < textureData.Length; i += 4)
                {
                    textureData[i] = textureData[i + 1] = textureData[i + 2] = textureData[i + 3] = 0;
                }
                Form1.graphics.Textures[0] = null;
                t.SetData<byte>(textureData);
                b = new Bitmap(t.Width, t.Height, PixelFormat.Format32bppArgb);

                System.Drawing.Imaging.BitmapData bmpData = b.LockBits(
                   new System.Drawing.Rectangle(0, 0, t.Width, t.Height),
                   System.Drawing.Imaging.ImageLockMode.WriteOnly,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb
                 );
                for (int i = 0; i < 4 * t.Width * t.Height; i += 4)
                {
                    byte r = textureData[i];
                    byte g = textureData[i + 2];
                    textureData[i + 2] = r;
                    textureData[i] = g;
                }
                IntPtr safePtr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
                b.UnlockBits(bmpData);

                pictureBox1.Image = b;
                pictureBox1.Invalidate();
                Application.DoEvents();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmaps (*.png)|*.png";
            sfd.DefaultExt = ".png";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (listBox1.SelectedIndex >= 0)
                {
                    Texture2D t = tmain.Textures[listBox1.SelectedIndex];
                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate))
                    {
                        t.SaveAsPng(fs, t.Width, t.Height); // save render target to disk
                    }
                }
            }
        }
    }
}
