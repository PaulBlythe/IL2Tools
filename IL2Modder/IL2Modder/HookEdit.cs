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
    public partial class HookEdit : Form
    {
        Hook hook;
        float y, p, r;
        bool loading = false;

        public HookEdit()
        {
            InitializeComponent();
        }
        public void SetHook(Hook h)
        {
            hook = h;
            numericUpDown1.Value = (decimal)h.matrix.M41;
            numericUpDown2.Value = (decimal)h.matrix.M42;
            numericUpDown3.Value = (decimal)h.matrix.M43;

            Matrix m = h.matrix;
            m.M41 = 0;
            m.M42 = 0;
            m.M43 = 0;

            Quaternion q = Quaternion.CreateFromRotationMatrix(h.matrix);
            QuaternionToYawPitchRoll(q, out y, out p, out r);
            loading = true;
            numericUpDown4.Value = (decimal)y;
            numericUpDown5.Value = (decimal)p;
            numericUpDown6.Value = (decimal)r;
            loading = false;


        }
        void QuaternionToYawPitchRoll(Quaternion q, out float yaw, out float pitch, out float roll)
        {

            const float Epsilon = 0.0009765625f;
            const float Threshold = 0.5f - Epsilon;

            float XY = q.X * q.Y;
            float ZW = q.Z * q.W;

            float TEST = XY + ZW;

            if (TEST < -Threshold || TEST > Threshold)
            {

                int sign = Math.Sign(TEST);

                yaw = sign * 2 * (float)Math.Atan2(q.X, q.W);

                pitch = sign * MathHelper.PiOver2;

                roll = 0;

            }
            else
            {

                float XX = q.X * q.X;
                float XZ = q.X * q.Z;
                float XW = q.X * q.W;

                float YY = q.Y * q.Y;
                float YW = q.Y * q.W;
                float YZ = q.Y * q.Z;

                float ZZ = q.Z * q.Z;

                yaw = (float)Math.Atan2(2 * YW - 2 * XZ, 1 - 2 * YY - 2 * ZZ);

                pitch = (float)Math.Atan2(2 * XW - 2 * YZ, 1 - 2 * XX - 2 * ZZ);

                roll = (float)Math.Asin(2 * TEST);

            }
        } 

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            hook.matrix.M41 = (float)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            hook.matrix.M42 = (float)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            hook.matrix.M43 = (float)numericUpDown3.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                y = (float)numericUpDown4.Value;
                Vector3 pos = new Vector3(hook.matrix.M41, hook.matrix.M42, hook.matrix.M43);
                hook.matrix = Matrix.CreateFromYawPitchRoll(y, p, r) * Matrix.CreateTranslation(pos);
            }
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                p = (float)numericUpDown5.Value;
                Vector3 pos = new Vector3(hook.matrix.M41, hook.matrix.M42, hook.matrix.M43);
                hook.matrix = Matrix.CreateFromYawPitchRoll(y, p, r) * Matrix.CreateTranslation(pos);
            }
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                r = (float)numericUpDown6.Value;
                Vector3 pos = new Vector3(hook.matrix.M41, hook.matrix.M42, hook.matrix.M43);
                hook.matrix = Matrix.CreateFromYawPitchRoll(y, p, r) * Matrix.CreateTranslation(pos);
            }
        }
    }
}
