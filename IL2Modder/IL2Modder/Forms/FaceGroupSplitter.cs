using IL2Modder.IL2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IL2Modder.Forms
{
    public partial class FaceGroupSplitter : Form
    {
        Mesh host;

        public FaceGroupSplitter(Mesh mesh)
        {
            InitializeComponent();

            foreach(FaceGroup f in mesh.FaceGroups)
            {
                listBox1.Items.Add(f.Material.ToString());
            }
            host = mesh;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            host.SelectedFaceGroup = null;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            host.SelectedFaceGroup = null;
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex>=0)
            {
                host.SelectedFaceGroup = host.FaceGroups[listBox1.SelectedIndex];
            }
        }
    }
}
