using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IL2Modder.IL2;

namespace IL2Modder
{
    public partial class GenerateSkinTexture : Form
    {
        Bitmap b = null;
        HIM him;
        MeshNode selected;
        bool down = false;
        float start_x, start_y;

        public GenerateSkinTexture(HIM him)
        {
            InitializeComponent();
            this.him = him;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void GenerateSkinTexture_Load(object sender, EventArgs e)
        {
            foreach (String s in Form1.Manager.Names)
            {
                comboBox2.Items.Add(s);
            }
            comboBox2.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int[] sizes = new int[] { 128, 256, 512, 1024, 2048, 4096 };
            int i = comboBox1.SelectedIndex;
            int size = sizes[i];
            b = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(b);
            pictureBox1.Image = b;

            g.Clear(Color.White);
            Pen p = new Pen(Color.Black);
            Brush br = new SolidBrush(Color.Gray);
            
            him.DrawSkin(g, p, br, (String)comboBox2.Items[comboBox2.SelectedIndex], size);

            p.Dispose();
            br.Dispose();
            g.Dispose();

            pictureBox1.Invalidate();
            Application.DoEvents();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "*.bmp";
            sfd.Filter = "Bitmaps (*.bmp)|*.bmp";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                b.Save(sfd.FileName);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((b != null)&&(e.Button == System.Windows.Forms.MouseButtons.Left))
            {
                down = true;
                {
                    int[] sizes = new int[] { 128, 256, 512, 1024, 2048, 4096 };
                    int i = comboBox1.SelectedIndex;
                    int size = sizes[i];

                    Graphics g = Graphics.FromImage(b);
                    g.Clear(Color.White);
                    Pen p = new Pen(Color.Black);
                    Brush br = new SolidBrush(Color.Gray);

                    him.DrawSkin(g, p, br, (String)comboBox2.Items[comboBox2.SelectedIndex], size);

                    p.Dispose();
                    br.Dispose();
                    g.Dispose();
                    pictureBox1.Invalidate();
                    Application.DoEvents();
                }
                float cx = (float)e.X;
                float cy = (float)e.Y;
                
                start_x = cx;
                start_y = cy;

                cx = (cx * b.Width) / 512.0f;
                cy = (cy * b.Height) / 512.0f;

                MeshNode mn = him.Inside(cx, cy, (String)comboBox2.Items[comboBox2.SelectedIndex], b.Width);
                if (mn != null)
                {
                    Pen r = new Pen(Color.Red);
                    Brush o = new SolidBrush(Color.Orange);
                    Graphics g = Graphics.FromImage(b);
                    mn.DrawSkinPart(g, r, o, (String)comboBox2.Items[comboBox2.SelectedIndex], b.Width);

                    o.Dispose();
                    r.Dispose();
                    g.Dispose();
                    selected = mn;
                    pictureBox1.Invalidate();
                    Application.DoEvents();
                }
                else
                {
                    selected = null;
                }


            }

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            down = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((down) && (selected != null))
            {
                float cx = (float)e.X;
                float cy = (float)e.Y;

                cx -= start_x;
                cy -= start_y;

                cx /= 512;
                cy /= 512;

                selected.MoveUV(cx, cy);
                start_x = (float)e.X;
                start_y = (float)e.Y;
                {
                    int[] sizes = new int[] { 128, 256, 512, 1024, 2048, 4096 };
                    int i = comboBox1.SelectedIndex;
                    int size = sizes[i];

                    Graphics g = Graphics.FromImage(b);
                    g.Clear(Color.White);
                    
                    Pen p = new Pen(Color.Black);
                    Brush br = new SolidBrush(Color.Gray);
                    Pen r = new Pen(Color.Red);
                    Brush o = new SolidBrush(Color.Orange);

                    him.DrawSkin(g, p, br, (String)comboBox2.Items[comboBox2.SelectedIndex], size);
                    selected.DrawSkinPart(g, r, o, (String)comboBox2.Items[comboBox2.SelectedIndex], b.Width);

                    r.Dispose();
                    o.Dispose();
                    p.Dispose();
                    br.Dispose();
                    g.Dispose();
                    pictureBox1.Invalidate();
                    Application.DoEvents();
                }
            }
        }
    }
}
