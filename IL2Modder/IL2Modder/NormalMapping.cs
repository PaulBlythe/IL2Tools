using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IL2Modder
{
    public partial class NormalMapping : Form
    {
        TextureManager tmain;
        Bitmap b;
        public int TexID;

        public NormalMapping(TextureManager manager)
        {
            InitializeComponent();

            tmain = manager;
        }

        /// <summary>
        /// Load the normal texture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "TGA files (*.tga)|*.tga|Bitmaps (*.bmp)|*.bmp";
            ofd.DefaultExt = "*.tga";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TexID = tmain.AddTexture(Path.GetFileName(ofd.FileName), Path.GetDirectoryName(ofd.FileName));
                Texture2D t = tmain.Textures[TexID];
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

        /// <summary>
        /// Cancel the operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        /// <summary>
        /// Generate binormals and tangents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            ObjectViewer.Instance.mesh.GenerateBinormalsAndTangents();
        }

        /// <summary>
        /// Enable / disable bump mapping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ObjectViewer.BumpMapping = checkBox1.Checked;
        }

        /// <summary>
        /// Close and apply
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }



    }
}
