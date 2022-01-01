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
    public partial class CVTAnimatorDialog : Form
    {
        public float Minimum;
        public float Maximum;
        public float Start;
        public float Finish;

        public String Target;
        public String Control;
        
        public CVTAnimatorDialog()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Minimum = (float)numericUpDown1.Value;
            Maximum = (float)numericUpDown2.Value;
            Start = (float)numericUpDown3.Value;
            Finish = (float)numericUpDown4.Value;

            Target = textBox1.Text;
            Control = textBox2.Text;
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
