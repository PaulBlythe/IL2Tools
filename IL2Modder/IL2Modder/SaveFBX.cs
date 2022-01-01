using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace IL2Modder
{
    public partial class SaveFBX : Form
    {
        public bool ExportShadows;
        public bool ExportBaseMesh;
        public bool ExportLODs;
        public bool ExportLODShadows;
        public bool ExportHooks;
        public bool ExportCollisionMeshes;

        public SaveFBX()
        {
            InitializeComponent();

            textBox1.Text = Form1.FBXDirectory;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "FBX files (*.fbx)|*.fbx";
            sfd.DefaultExt = ".fbx";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = sfd.FileName;
                Form1.FBXDirectory = Path.GetDirectoryName(sfd.FileName);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExportBaseMesh = checkBox6.Checked;
            ExportHooks = checkBox5.Checked;
            ExportLODs = checkBox3.Checked;
            ExportLODShadows = checkBox4.Checked;
            ExportShadows = checkBox2.Checked;
            ExportCollisionMeshes = checkBox7.Checked;

            Form1.Instance.OnSaveAsFBX(textBox1.Text, Path.GetDirectoryName(textBox1.Text),checkBox1.Checked);

            Form1.Instance.exporter.Export(Form1.Instance.scene, Form1.Instance.exportOptions);

            Form1.Instance.exportOptions.Destroy();
            Form1.Instance.sdkManager.Destroy();

            Form1.Instance.exporter = null;
            Form1.Instance.exportOptions = null;
            Form1.Instance.sdkManager = null;

            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        public void Progress(string file)
        {
            textBox2.Text = file;
            progressBar1.Value++;
            this.Invalidate();

            Application.DoEvents();
        }

        // export shadows
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
        }
    }
}
