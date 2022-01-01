using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IL2Modder
{
    public partial class Joystick : UserControl
    {
        public float Roll;
        public float Pitch;
        public delegate void JoystickHandler();
        private bool down;

        public Joystick()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
    
            Roll = Pitch = 0;
            down = false;
        }

       

        [Category("Action")]
        [Description("Fires when the Joystick is moved.")]
        public event JoystickHandler JoystickClicked;

        protected virtual void OnJoystickClicked()
        {
            // If an event has no subscribers registerd, it will
            // evaluate to null. The test checks that the value is not
            // null, ensuring that there are subsribers before
            // calling the event itself.
            if (JoystickClicked != null)
            {
                JoystickClicked();  // Notify Subscribers
            }
        }
        

        private void Joystick_Paint(object sender, PaintEventArgs pe)
        {

            // Declare and instantiate a new pen.
            System.Drawing.Pen myPen = new System.Drawing.Pen(Color.Black);

            // Draw an aqua rectangle in the rectangle represented by the control.
            pe.Graphics.DrawRectangle(myPen, new Rectangle(this.Location, this.Size));

            float xc = (1.0f + Roll) / 2.0f;
            float yc = (1.0f + Pitch) / 2.0f;
            xc *= this.Size.Width;
            yc *= this.Size.Height;
            //xc += this.Location.X;
            //yc += this.Location.Y;

            int cx = (this.Size.Width / 2);
            int cy =  (this.Size.Height / 2);

            pe.Graphics.DrawLine(myPen, new Point(cx, cy), new Point((int)xc, (int)yc));
            pe.Graphics.DrawEllipse(myPen, new Rectangle((int)xc - 4, (int)yc - 4, 8, 8));
            myPen.Dispose();
        }

        private void Joystick_MouseClick(object sender, MouseEventArgs e)
        {
            float rx = e.X;// -Location.X;
            float ry = e.Y;// -Location.Y;

            rx /= Size.Width;
            ry /= Size.Height;
            Roll = (rx - 0.5f) * 2.0f;
            Pitch = (ry - 0.5f) * 2.0f;
            OnJoystickClicked();
            Invalidate();
        }

        private void Joystick_MouseDown(object sender, MouseEventArgs e)
        {
            down = true;

            float rx = e.X;// -Location.X;
            float ry = e.Y;// -Location.Y;

            rx /= Size.Width;
            ry /= Size.Height;
            Roll = (rx - 0.5f) * 2.0f;
            Pitch = (ry - 0.5f) * 2.0f;
            OnJoystickClicked();
            Invalidate();
        }

        private void Joystick_MouseMove(object sender, MouseEventArgs e)
        {
            if (down)
            {
                float rx = e.X;// -Location.X;
                float ry = e.Y;// -Location.Y;

                rx /= Size.Width;
                ry /= Size.Height;
                Roll = (rx - 0.5f) * 2.0f;
                Pitch = (ry - 0.5f) * 2.0f;
                OnJoystickClicked();
                Invalidate();
            }
        }

        private void Joystick_MouseLeave(object sender, EventArgs e)
        {
            down = false;
        }

        private void Joystick_MouseUp(object sender, MouseEventArgs e)
        {
            down = false;
        }
    }
}
