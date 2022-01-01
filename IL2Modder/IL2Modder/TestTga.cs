using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IL2Modder
{
    public partial class TestTga : Form
    {
        Bitmap bitmap;
        Tga tga;

        public TestTga()
        {
            InitializeComponent();
        }

        // load
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "*.tga";
            ofd.Filter = "TGA files (*.tga)|*.tga";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tga = new Tga(ofd.FileName);
                bitmap = new Bitmap(tga.ImageWidth, tga.ImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(
                                    new System.Drawing.Rectangle(0, 0, tga.ImageWidth, tga.ImageHeight),
                                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                            );
                for (int i = 0; i < 4 * tga.ImageWidth * tga.ImageHeight; i += 4)
                {
                    byte r = tga.ImageData[i];
                    byte g = tga.ImageData[i + 2];
                    tga.ImageData[i + 2] = r;
                    tga.ImageData[i] = g;
                }
                IntPtr safePtr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(tga.ImageData, 0, safePtr, tga.ImageData.Length);
                bitmap.UnlockBits(bmpData);


                textBox1.Text = tga.ImageType.ToString();
                textBox2.Text = tga.ImageWidth.ToString();
                textBox3.Text = tga.ImageHeight.ToString();
                textBox4.Text = tga.PixelDepth.ToString();
                textBox5.Text = tga.XOrigin.ToString();
                textBox6.Text = tga.YOrigin.ToString();

                pictureBox1.Image = bitmap;
                pictureBox1.Invalidate();
                Application.DoEvents();

            }
        }
    }
}
