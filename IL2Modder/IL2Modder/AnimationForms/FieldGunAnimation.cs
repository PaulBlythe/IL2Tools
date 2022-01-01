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
    public partial class FieldGunAnimation : Form
    {
        FieldGunAnimator Animator;

        public FieldGunAnimation(FieldGunAnimator animator)
        {
            Animator = animator;
            InitializeComponent();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            Animator.Sweep = (float)numericUpDown5.Value;
            Animator.BarrelTravel = (float)numericUpDown1.Value;
            Animator.MinPitch = (float)numericUpDown2.Value;
            Animator.MaxPitch = (float)numericUpDown3.Value;
            Animator.RateOfFire = (int)numericUpDown4.Value;
        }
    }
}
