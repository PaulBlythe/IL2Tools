using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using IL2Modder.IL2;

namespace IL2Modder
{
    public partial class MaterialEditor : Form
    {
        MeshNode parent;
        Material m = null;
        Bitmap b;

        public MaterialEditor(MeshNode mn)
        {
            parent = mn;
            InitializeComponent();

            foreach (Material m in mn.mesh.Materials)
            {
                comboBox1.Items.Add(m.Name);
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            m = parent.mesh.Materials[comboBox1.SelectedIndex];

            Texture2D t = Form1.Manager.GetTexture(m.TextureID);
            byte[] textureData = new byte[4 * t.Width * t.Height];
            t.GetData<byte>(textureData);
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

            checkBox1.Checked = m.Sort;
            checkBox2.Checked = m.tfWrapX;
            checkBox3.Checked = m.tfWrapY;
            checkBox4.Checked = m.tfMinLinear;
            checkBox5.Checked = m.tfMagLinear;
            checkBox6.Checked = m.tfBlend;
            checkBox7.Checked = m.tfBlendAdd;
            checkBox8.Checked = m.tfNoWriteZ;
            checkBox9.Checked = m.tfTranspBorder;
            checkBox10.Checked = m.tfDoubleSided;
            checkBox11.Checked = m.tfTestA;
            checkBox12.Checked = m.Glass;

            numericUpDown1.Value = (decimal)m.Ambient;
            numericUpDown2.Value = (decimal)m.Diffuse;
            numericUpDown3.Value = (decimal)m.Specular;
            numericUpDown4.Value = (decimal)m.SpecularPow;
            numericUpDown5.Value = (decimal)m.AlphaTestVal;

            numericUpDown6.Value = (decimal)m.Colour[0];
            numericUpDown7.Value = (decimal)m.Colour[1];
            numericUpDown8.Value = (decimal)m.Colour[2];
            numericUpDown9.Value = (decimal)m.Shine;

        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.AlphaTestVal = (float)numericUpDown5.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.SpecularPow = (float)numericUpDown4.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.Specular = (float)numericUpDown3.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.Diffuse = (float)numericUpDown2.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.Ambient = (float)numericUpDown1.Value;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.Sort = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfWrapX = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfWrapY = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfMinLinear = checkBox4.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfMagLinear = checkBox5.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfBlend = checkBox6.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfBlendAdd = checkBox7.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfNoWriteZ = checkBox8.Checked;
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfTranspBorder = checkBox9.Checked;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.Colour[0] = (float)numericUpDown6.Value;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.Colour[1] = (float)numericUpDown7.Value;
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.Colour[2] = (float)numericUpDown8.Value;
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfDoubleSided = checkBox10.Checked;
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.Shine = (float)numericUpDown9.Value;
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.tfTestA = checkBox11.Checked;
        }

        /// <summary>
        /// Change texture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (m != null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "All files |*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    m.TextureID = Form1.Manager.AddTexture(Path.GetFileName(ofd.FileName), Path.GetDirectoryName(ofd.FileName));

                    Texture2D t = Form1.Manager.GetTexture(m.TextureID);
                    byte[] textureData = new byte[4 * t.Width * t.Height];
                    t.GetData<byte>(textureData);
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
                }
            }
                
        }

        // Glass button
        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            if (m != null)
                m.Glass = checkBox12.Checked;
        }
    }
}
