using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IL2Modder.IL2;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace IL2Modder
{
    public partial class AdjustMatrix : Form
    {
        private Node _node;
        private Matrix original;

        public Node node
        {
            get {return _node;}
            set {
                _node = value;
                setup();
            }
        }

        public AdjustMatrix()
        {
            InitializeComponent();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            _node.world = original;
            this.DialogResult = DialogResult.Cancel;
        }
        private void setup()
        {
            textBox1.Text = String.Format("{0}", _node.world.M11);
            textBox2.Text = String.Format("{0}", _node.world.M12);
            textBox3.Text = String.Format("{0}", _node.world.M13);
            textBox4.Text = String.Format("{0}", _node.world.M21);
            textBox5.Text = String.Format("{0}", _node.world.M22);
            textBox6.Text = String.Format("{0}", _node.world.M23);
            textBox7.Text = String.Format("{0}", _node.world.M31);
            textBox8.Text = String.Format("{0}", _node.world.M32);
            textBox9.Text = String.Format("{0}", _node.world.M33);
            textBox10.Text = String.Format("{0}", _node.world.M41);
            textBox11.Text = String.Format("{0}", _node.world.M42);
            textBox12.Text = String.Format("{0}", _node.world.M43);

            original = _node.world;
        }
        private void reset()
        {
            textBox1.Text = String.Format("{0}", _node.world.M11);
            textBox2.Text = String.Format("{0}", _node.world.M12);
            textBox3.Text = String.Format("{0}", _node.world.M13);
            textBox4.Text = String.Format("{0}", _node.world.M21);
            textBox5.Text = String.Format("{0}", _node.world.M22);
            textBox6.Text = String.Format("{0}", _node.world.M23);
            textBox7.Text = String.Format("{0}", _node.world.M31);
            textBox8.Text = String.Format("{0}", _node.world.M32);
            textBox9.Text = String.Format("{0}", _node.world.M33);
            textBox10.Text = String.Format("{0}", _node.world.M41);
            textBox11.Text = String.Format("{0}", _node.world.M42);
            textBox12.Text = String.Format("{0}", _node.world.M43);

        }
        // forward
        private void button2_Click(object sender, EventArgs e)
        {
            float ss = (float) numericUpDown1.Value;
            _node.world.M42 -= ss;
            textBox11.Text = String.Format("{0}", _node.world.M42);
        }

        // up
        private void button5_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown1.Value;
            _node.world.M43 += ss;
            textBox12.Text = String.Format("{0}", _node.world.M43);
        }

        // down
        private void button6_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown1.Value;
            _node.world.M43 -= ss;
            textBox12.Text = String.Format("{0}", _node.world.M43);
        }

        // back
        private void button1_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown1.Value;
            _node.world.M42 += ss;
            textBox11.Text = String.Format("{0}", _node.world.M42);
        }
        // left
        private void button3_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown1.Value;
            _node.world.M41 += ss;
            textBox10.Text = String.Format("{0}", _node.world.M41);
        }
        // right
        private void button4_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown1.Value;
            _node.world.M41 -= ss;
            textBox10.Text = String.Format("{0}", _node.world.M41);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown2.Value;
            ss = MathHelper.ToRadians(ss);

            Matrix m = Matrix.CreateRotationY(ss);
            _node.world *= m;
            reset();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown2.Value;
            ss = MathHelper.ToRadians(-ss);

            Matrix m = Matrix.CreateRotationY(ss);
            _node.world *= m;
            reset();
        }

        // Identity
        private void button15_Click(object sender, EventArgs e)
        {
            _node.world = Matrix.Identity;
            reset();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown2.Value;
            ss = MathHelper.ToRadians(ss);

            Matrix m = Matrix.CreateRotationZ(ss);
            _node.world *= m;
            reset();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown2.Value;
            ss = MathHelper.ToRadians(-ss);

            Matrix m = Matrix.CreateRotationZ(ss);
            _node.world *= m;
            reset();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown2.Value;
            ss = MathHelper.ToRadians(-ss);

            Matrix m = Matrix.CreateRotationX(ss);
            _node.world *= m;
            reset();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            float ss = (float)numericUpDown2.Value;
            ss = MathHelper.ToRadians(ss);

            Matrix m = Matrix.CreateRotationX(ss);
            _node.world *= m;
            reset();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Matrix m = Matrix.CreateScale((float)numericUpDown1.Value);
            _node.world = m * _node.world; 
            reset();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Matrix m = Matrix.CreateScale( 1.0f / (float)numericUpDown1.Value);
            _node.world = m * _node.world;
            reset();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox1.Text);
                _node.world.M11 = val;
            }
            catch (Exception) { }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox2.Text);
                _node.world.M12 = val;
            }
            catch (Exception) { }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox3.Text);
                _node.world.M13 = val;
            }
            catch (Exception) { }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox4.Text);
                _node.world.M21 = val;
            }
            catch (Exception) { }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox5.Text);
                _node.world.M22 = val;
            }
            catch (Exception) { }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox6.Text);
                _node.world.M23 = val;
            }
            catch (Exception) { }
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox7.Text);
                _node.world.M31 = val;
            }
            catch (Exception) { }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox8.Text);
                _node.world.M32 = val;
            }
            catch (Exception) { }
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox9.Text);
                _node.world.M33 = val;
            }
            catch (Exception) { }
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox10.Text);
                _node.world.M41 = val;
            }
            catch (Exception) { }
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox11.Text);
                _node.world.M42 = val;
            }
            catch (Exception) { }
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float val = float.Parse(textBox12.Text);
                _node.world.M43 = val;
            }
            catch (Exception) { }
        }
    }
}
