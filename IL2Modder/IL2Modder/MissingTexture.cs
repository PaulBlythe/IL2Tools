using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace IL2Modder
{
    public partial class MissingTexture : Form
    {
        String file;
        String extension;

        public MissingTexture(String name)
        {
            InitializeComponent();
            file = name;
            label1.Text = name;
            extension = Path.GetExtension(name);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = file;
            ofd.Filter = String.Format("Graphics files (*{0})|*{0}", extension);
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Form1.project.SearchPaths.Add(Path.GetDirectoryName(ofd.FileName));
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Ignore;
            this.Close();
        }
    }
}
