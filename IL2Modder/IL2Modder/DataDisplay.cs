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
    public partial class DataDisplay : Form
    {
        public DataDisplay()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public void AddData(String s)
        {
            listBox1.Items.Add(s);
        }
    }
}
