using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using IL2Modder.AnimationForms;
using IL2Modder.Fox1Animators;

namespace IL2Modder
{
    public partial class FoxOneAnimationEditor : Form
    {
        AAGunParameters aag;
        AAGunAnimation aag_animator;

        FieldGunAnimator fga;
        FieldGunAnimation fga_form;

        ShipTurretHeadAnimation sth;
        ShipTurretHeadParameters sthp;

        ShipsGunAnimation sga;
        ShipGunParameters sgp;

        String target_name = "";

        public FoxOneAnimationEditor()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            aag_animator = new AAGunAnimation();
            ObjectViewer.Instance.mesh.animators.Add(aag_animator);

            aag = new AAGunParameters(aag_animator);
            aag.Show();

            Button b = new Button();
            b.Text = "AA Gun Animation";
            flowLayoutPanel1.Controls.Add(b);

            

        }

        /// <summary>
        /// Field gun animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            fga = new FieldGunAnimator();
            ObjectViewer.Instance.mesh.animators.Add(fga);

            fga_form = new FieldGunAnimation(fga);
            fga_form.Show();

            Button b = new Button();
            b.Text = "Field Gun Animation";
            flowLayoutPanel1.Controls.Add(b);

        }

        public void SetTarget(String name)
        {
            target_name = name;
            button1.Enabled = false;
            button4.Enabled = false;
        }

        // ok button
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        // ship turret head animation
        private void button5_Click(object sender, EventArgs e)
        {
            sth = new ShipTurretHeadAnimation(target_name);
            ObjectViewer.Instance.mesh.animators.Add(sth);

            sthp = new ShipTurretHeadParameters(sth);
            sthp.Show();

            Button b = new Button();
            b.Text = "Ship turret head";
            flowLayoutPanel1.Controls.Add(b);
        }

        /// <summary>
        /// Ship turret gun animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            sga = new ShipsGunAnimation(target_name);
            ObjectViewer.Instance.mesh.animators.Add(sga);

            sgp = new ShipGunParameters(sga);
            sgp.Show();

            Button b = new Button();
            b.Text = "Ship turret gun";
            flowLayoutPanel1.Controls.Add(b);
        }
    }
}
