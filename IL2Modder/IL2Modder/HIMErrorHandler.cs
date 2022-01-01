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
    public partial class HIMErrorHandler : Form
    {
        public HIMErrorHandler()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void SetErrorString(String s)
        {
            textBox1.Text = s;
        }

        public void SetLineNumber(int s)
        {
            textBox2.Text = s.ToString();
        }

        public void SetErrorText(String s)
        {
            textBox3.Text = s;
        }

        public void SetFilename(String s)
        {
            textBox4.Text = s;
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
                    writer.WriteLine("Type         :-  HIM loader");
                    writer.WriteLine("File name    :-  " + textBox4.Text);
                    writer.WriteLine("Line number  :-  " + textBox2.Text);
                    writer.WriteLine("Line text    :-  " + textBox1.Text);
                    writer.WriteLine("Error        :-  ");
                    writer.WriteLine(textBox3.Text);
                    writer.WriteLine("****************************************************************************");
                    writer.Close();
                }
            }
        }
    }
}
