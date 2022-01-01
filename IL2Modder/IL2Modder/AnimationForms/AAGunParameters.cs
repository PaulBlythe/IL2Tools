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
    public partial class AAGunParameters : Form
    {
        AAGunAnimation animator;

        public AAGunParameters(AAGunAnimation Animator)
        {
            animator = Animator;
            InitializeComponent();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            animator.BarrelTravel = (float) numericUpDown1.Value;
            animator.RateOfFire = (int)numericUpDown4.Value;
            animator.MaxPitch = (float)numericUpDown3.Value;
            animator.MinPitch = (float)numericUpDown2.Value;
        }
    }
}
