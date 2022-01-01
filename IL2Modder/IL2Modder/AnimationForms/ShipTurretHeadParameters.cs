using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IL2Modder.Fox1Animators;

namespace IL2Modder.AnimationForms
{
    public partial class ShipTurretHeadParameters : Form
    {
        ShipTurretHeadAnimation dad;

        public ShipTurretHeadParameters(ShipTurretHeadAnimation parent)
        {
            dad = parent;
            InitializeComponent();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            dad.Speed = (float)numericUpDown2.Value;
            dad.MinYaw = (float)numericUpDown1.Value;
            dad.MaxYaw = (float)numericUpDown3.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
