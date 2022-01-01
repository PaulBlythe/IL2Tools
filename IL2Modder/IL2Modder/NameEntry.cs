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
    public partial class NameEntry : Form
    {
        public NameEntry(String text)
        {
            InitializeComponent();
            textBox1.Text = text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.comms = textBox2.Text;
            this.Close();
        }
    }
}
