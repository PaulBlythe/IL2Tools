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
    public partial class MissingTextures : Form
    {
        public MissingTextures()
        {
            InitializeComponent();
        }
        public void AddList(List<String> files)
        {
            foreach (String s in files)
            {
                listBox1.Items.Add(s);
            }
        }
    }
}
