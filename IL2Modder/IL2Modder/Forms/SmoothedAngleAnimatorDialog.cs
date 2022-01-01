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
    public partial class SmoothedAngleAnimatorDialog : Form
    {
        public SmoothedAngleAnimator result = new SmoothedAngleAnimator();

        public SmoothedAngleAnimatorDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result.Scale = (float)numericUpDown1.Value;
            result.Smoothing = (float)numericUpDown2.Value;
            result.Mesh = textBox2.Text;
            result.Control = textBox1.Text;
            int plane = 0;
            if (checkBox1.Checked)
                plane += 1;
            if (checkBox2.Checked)
                plane += 2;
            if (checkBox3.Checked)
                plane += 4;

            result.Plane = plane;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
