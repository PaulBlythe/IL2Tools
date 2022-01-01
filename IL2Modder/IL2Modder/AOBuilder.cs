using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Imaging;

namespace IL2Modder
{
    public partial class AOBuilder : Form
    {
        public static AOBuilder Instance;
        ObjectViewer objectViewer1;
        TextureManager manager;
        Bitmap b;

        public AOBuilder(ObjectViewer ov, TextureManager Manager)
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox3.SelectedIndex = 2;
            objectViewer1 = ov;
            foreach (String s in Manager.Names)
            {
                comboBox2.Items.Add(s);
            }
            manager = Manager;
            Instance = this;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int size = comboBox3.SelectedIndex;
            size += 7;
            size = (int)Math.Pow(2,size);
            String target = (string)comboBox2.Items[comboBox2.SelectedIndex];

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    {
                        objectViewer1.BakeAmbientVertexCamera(target, size);
                        Texture2D t = objectViewer1.ao_target;
                        b = new Bitmap(size, size, PixelFormat.Format32bppArgb);
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
                    break;
                case 1:
                    {
                        objectViewer1.BakeAmbientMultiVertex(target, size);

                        Texture2D t = objectViewer1.ao_target;
                        b = new Bitmap(size, size, PixelFormat.Format32bppArgb);
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
                    break;

                case 2:
                    {
                        String tex = comboBox2.SelectedText;
                        int rays = (int)numericUpDown2.Value;
                        objectViewer1.BakeAmbientVertexRay(target, size, rays);

                        Texture2D t = objectViewer1.ao_target;
                        b = new Bitmap(size, size, PixelFormat.Format32bppArgb);
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
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".bmp";
            sfd.Filter = "Bitmaps (*.bmp)|*.bmp";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                b.Save(sfd.FileName);
            }
        }

        public void UpdateForm()
        {
            Texture2D t = objectViewer1.ao_target;
            b = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
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

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        public void SetNode(String node)
        {
            toolStripStatusLabel1.Text = node;
        }
        public void SetProgress(int per)
        {
            toolStripProgressBar1.Value = per;
            statusStrip1.Invalidate();
            Application.DoEvents();
        }
    }
}
