namespace IL2Modder
{
    partial class Joystick
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Joystick
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Name = "Joystick";
            this.Size = new System.Drawing.Size(70, 68);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Joystick_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Joystick_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Joystick_MouseDown);
            this.MouseLeave += new System.EventHandler(this.Joystick_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Joystick_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Joystick_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
