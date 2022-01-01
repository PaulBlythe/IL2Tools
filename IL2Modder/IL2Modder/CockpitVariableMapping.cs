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
    public partial class CockpitVariableMapping : Form
    {
        int selection;
        List<Control> controls = new List<Control>();

        public CockpitVariableMapping()
        {
            InitializeComponent();
            selection = -1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selection!=comboBox1.SelectedIndex)
            {
                selection = comboBox1.SelectedIndex;
                groupBox2.Controls.Clear();
                controls.Clear();

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        {
                            Label l = new Label();
                            l.Text = "Units";
                            l.Location = new Point(20, 20);
                            groupBox2.Controls.Add(l);

                            CheckBox cb1 = new CheckBox();
                            cb1.Text = "Knots";
                            cb1.Location = new Point(20, 40);
                            cb1.CheckStateChanged += new EventHandler(cb1_CheckStateChanged);
                            controls.Add(cb1);
                            groupBox2.Controls.Add(cb1);

                            cb1 = new CheckBox();
                            cb1.Text = "Mach";
                            cb1.Location = new Point(20, 60);
                            cb1.CheckStateChanged += new EventHandler(cb1_CheckStateChanged);
                            controls.Add(cb1);
                            groupBox2.Controls.Add(cb1);

                            cb1 = new CheckBox();
                            cb1.Text = "Miles per hour";
                            cb1.Location = new Point(20, 80);
                            cb1.CheckStateChanged += new EventHandler(cb1_CheckStateChanged);
                            controls.Add(cb1);
                            groupBox2.Controls.Add(cb1);

                            cb1 = new CheckBox();
                            cb1.Text = "Kilometres per hour";
                            cb1.Location = new Point(20, 100);
                            cb1.CheckStateChanged += new EventHandler(cb1_CheckStateChanged);
                            controls.Add(cb1);
                            groupBox2.Controls.Add(cb1);

                            l = new Label();
                            l.Text = "Type";
                            l.Location = new Point(20, 130);
                            groupBox2.Controls.Add(l);

                            RadioButton ra1 = new RadioButton();
                            ra1.Text = "Rotary";
                            ra1.Location = new Point(20, 150);
                            controls.Add(ra1);
                            groupBox2.Controls.Add(ra1);

                            ra1 = new RadioButton();
                            ra1.Text = "Linear";
                            ra1.Location = new Point(130, 150);
                            controls.Add(ra1);
                            groupBox2.Controls.Add(ra1);

                            GroupBox g2 = new GroupBox();
                            g2.Location = new Point(10, 180);
                            g2.Text = "Input range";
                            g2.Size = new Size(260,60);
                            controls.Add(g2);
                            groupBox2.Controls.Add(g2);

                            NumericUpDown n1 = new NumericUpDown();
                            n1.Location = new Point(20, 20);
                            n1.Minimum = 0;
                            n1.Maximum = 1000;
                            n1.Value = 0;
                            n1.Width = 100;
                            controls.Add(n1);
                            g2.Controls.Add(n1);

                            n1 = new NumericUpDown();
                            n1.Location = new Point(130, 20);
                            n1.Minimum = 0;
                            n1.Maximum = 1000;
                            n1.Value = 1000;
                            n1.Width = 100;
                            controls.Add(n1);
                            g2.Controls.Add(n1);

                            g2 = new GroupBox();
                            g2.Location = new Point(10, 250);
                            g2.Text = "Output range";
                            g2.Size = new Size(260, 60);
                            controls.Add(g2);
                            groupBox2.Controls.Add(g2);

                            n1 = new NumericUpDown();
                            n1.Location = new Point(20, 20);
                            n1.Minimum = 0;
                            n1.Maximum = 1000;
                            n1.Value = 0;
                            n1.Width = 100;
                            controls.Add(n1);
                            g2.Controls.Add(n1);

                            n1 = new NumericUpDown();
                            n1.Location = new Point(130, 20);
                            n1.Minimum = 0;
                            n1.Maximum = 1000;
                            n1.Value = 360;
                            n1.Width = 100;
                            controls.Add(n1);
                            g2.Controls.Add(n1);


                        }
                        break;
                }
            }
        }

        void cb1_CheckStateChanged(object sender, EventArgs e)
        {
            CheckBox src_cb = null;
            if (sender is CheckBox)
            {
                src_cb = (CheckBox)sender;
            }
            for (int i = 0; i < controls.Count; i++)
            {
                if ((controls[i] is CheckBox) && (controls[i] != src_cb))
                {
                    CheckBox t = (CheckBox)controls[i];
                    t.Checked = false;
                }
            }
        }


    }
}
