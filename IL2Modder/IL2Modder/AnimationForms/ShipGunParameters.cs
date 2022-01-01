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
    public partial class ShipGunParameters : Form
    {
        ShipsGunAnimation dad;

        public ShipGunParameters(ShipsGunAnimation parent)
        {
            InitializeComponent();
            dad = parent;
        }

        /// <summary>
        /// Add animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            dad.MinPitch     = (float)numericUpDown1.Value;
            dad.MaxPitch     = (float)numericUpDown2.Value;
            dad.Speed        = (float)numericUpDown3.Value;
            dad.RateOfFire   = (float)numericUpDown4.Value;
            dad.BarrelTravel = (float)numericUpDown5.Value;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dad.GunType = comboBox1.SelectedIndex;
        }

    }
}
