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
 
    public partial class Fox1ExportDialog : Form
    {
        public bool ExportPropAnimators = false;
        public bool ExportTextures = true;
        public bool ExportVators = false;
        public bool ExportRudders = false;
        public bool ExportAilerons = false;
        public bool ExportFlaps = false;
        public bool ExportBombBays = false;
        public int Mode = 0;

        public List<CVTAnimation> CvtAnimations = new List<CVTAnimation>();
        public List<TranslateAnimator> TranslateAnimations = new List<TranslateAnimator>();
        public List<SmoothedAngleAnimator> SmoothedAnimations = new List<SmoothedAngleAnimator>();

        public Fox1ExportDialog()
        {
            InitializeComponent();
            radioButton1.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ExportPropAnimators = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            ExportTextures = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            ExportVators = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            ExportRudders = checkBox4.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            ExportAilerons = checkBox5.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            ExportFlaps = checkBox6.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Forms.CVTAnimatorDialog ad = new Forms.CVTAnimatorDialog();
            if (ad.ShowDialog() == DialogResult.OK)
            {
                CVTAnimation cvt = new CVTAnimation();
                cvt.Control = ad.Control;
                cvt.Maximum = ad.Maximum;
                cvt.Minimum = ad.Minimum;
                cvt.Start = ad.Start;
                cvt.Finish = ad.Finish;
                cvt.Target = ad.Target;

                CvtAnimations.Add(cvt);
                listBox1.Items.Add(String.Format("CVTAnimationComponent_{0}", listBox1.Items.Count));
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Forms.TranslateAnimatorDialog td = new Forms.TranslateAnimatorDialog();
            if (td.ShowDialog() == DialogResult.OK)
            {
                TranslateAnimations.Add(td.result);
                listBox2.Items.Add(String.Format("TranslateAnimatorComponent_{0}", listBox2.Items.Count));
            }
        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            Forms.SmoothedAngleAnimatorDialog sd = new Forms.SmoothedAngleAnimatorDialog();
            if (sd.ShowDialog() == DialogResult.OK)
            {
                SmoothedAnimations.Add(sd.result);
                listBox3.Items.Add(String.Format("SmoothedAngleAnimatorComponent_{0}", listBox3.Items.Count));
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            ExportBombBays = checkBox7.Checked;
        }

        // Aircraft mode
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                Mode = 0;
        }

        // Ship mode
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                Mode = 1;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                Mode = 2;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                Mode = 3;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
                Mode = 4;
        }
    }
}
