using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace IL2Modder
{
    public partial class MSHLoaderErrorHandler : Form
    {
        public MSHLoaderErrorHandler()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void SetFilename(String s)
        {
            textBox1.Text = s;
        }

        public void SetMode(int mode)
        {
            if (mode == 0)
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox2.Checked = true;
            }
        }

        public void SetErrorText(String s)
        {
            textBox3.Text = s;
        }

        public void SetData(String s)
        {
            textBox4.Text = s;
        }

        public void SetErrorDescription(String s)
        {
            textBox2.Text = s;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".txt";
            sfd.Filter = "Text files (*.txt)|*.txt";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (TextWriter writer = File.CreateText(sfd.FileName))
                {
                    writer.WriteLine("****************************************************************************");
                    writer.WriteLine("****    IL2 modder error report                                         ****");
                    writer.WriteLine("****************************************************************************");
                    writer.WriteLine("Type         :-  MSH loader");

                    if (checkBox1.Checked)
                        writer.WriteLine("Mode         :-  Text");
                    else
                        writer.WriteLine("Mode         :-  Binary");
                    
                    writer.WriteLine("File name    :-  " + textBox1.Text);
                    writer.WriteLine("Line number  :-  " + textBox2.Text);
                    writer.WriteLine("Line text    :-  " + textBox4.Text);
                    writer.WriteLine("Error        :-  ");
                    writer.WriteLine(textBox3.Text);
                    writer.WriteLine("****************************************************************************");
                    writer.Close();
                }
            }
        }
    }
}
