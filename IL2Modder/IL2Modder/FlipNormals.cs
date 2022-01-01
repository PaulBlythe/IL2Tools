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
    public partial class FlipNormals : Form
    {
        Mesh target;

        public FlipNormals(Mesh mesh)
        {
            InitializeComponent();
            target = mesh;
            mesh.CacheNormals();

            listBox1.Items.Add("All face groups");
            foreach (FaceGroup f in mesh.FaceGroups)
            {
                listBox1.Items.Add(mesh.Materials[f.Material].Name);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            target.RecoverNormals();
            target.SelectedFaceGroup = null;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            target.SelectedFaceGroup = null;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                if (listBox1.SelectedIndex <= 0)
                    target.FlipNormals(0);
                else
                {
                    target.FlipNormals(0, listBox1.SelectedIndex - 1);
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
                checkBox3.Checked = false;
                if (listBox1.SelectedIndex <= 0)
                    target.FlipNormals(1);
                else
                {
                    target.FlipNormals(1, listBox1.SelectedIndex - 1);
                }
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                checkBox2.Checked = false;
                checkBox1.Checked = false;
                if (listBox1.SelectedIndex <= 0)
                    target.FlipNormals(2);
                else
                {
                    target.FlipNormals(2, listBox1.SelectedIndex - 1);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex <= 0)
                target.FlipNormals(3);
            else
            {
                target.FlipNormals(3, listBox1.SelectedIndex - 1);
            }
        }

        /// <summary>
        /// Recalculate all the normals
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            target.RecalculateNormals();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            target.SelectedFaceGroup = null;
            if (listBox1.SelectedIndex>0)
            {
                target.SelectedFaceGroup = target.FaceGroups[listBox1.SelectedIndex - 1];
            }
        }
    }
}
