using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using IL2Modder.IL2;

namespace IL2Modder
{
    public partial class Research : Form
    {
        public List<int> table3;
        public List<Table1Entry> table1;

        public Research()
        {
            InitializeComponent();
        }
        public void AddMessage(String s)
        {
            listBox1.Items.Add(s);
        }
        public void Sections(int sections)
        {
            textBox1.Text = String.Format("{0}", sections);
        }
        public void Table1(int sections)
        {
            textBox2.Text = String.Format("{0:X4}", sections);
        }
        public void Table2(int sections)
        {
            textBox3.Text = String.Format("{0:X4}", sections);
        }
        public void Table2Size(int sections)
        {
            textBox4.Text = String.Format("{0:X4}", sections);
        }
        public void AddToTable1(String test)
        {
            listBox3.Items.Add(test);
        }
        public void HexDisplay(byte[] data)
        {
            
            int i = 0;
            while (i < data.GetLength(0))
            {
                int j = data.GetLength(0) - i;
                if (j > 16)
                    j = 16;

                String res = String.Format("{0:X4}: ", i);
                String res2 = "";
                for (int k = 0; k < j; k++)
                {
                    byte d = data[i++];
                    String b = String.Format("{0:X2} ", d);
                    res = res + b;
                    if (d == 0)
                        d = (byte) '.';
                    b = String.Format("{0} ", Convert.ToChar(d));
                    res2 += b;
                }
                listBox2.Items.Add(res + " | " + res2);
            }
        }
        public void HexDisplay2(byte[] data)
        {

            int i = 0;
            while (i < data.GetLength(0))
            {
                int j = data.GetLength(0) - i;
                if (j > 16)
                    j = 16;

                String res = String.Format("{0:X4}: ", i);
                String res2 = "";
                for (int k = 0; k < j; k++)
                {
                    byte d = data[i++];
                    String b = String.Format("{0:X2} ", d);
                    res = res + b;
                    if (d == 0)
                        d = (byte)'.';
                    b = String.Format("{0} ", Convert.ToChar(d));
                    res2 += b;
                }
                listBox5.Items.Add(res + " | " + res2);
            }
        }
        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = listBox3.SelectedIndex;
            listBox4.Items.Clear();
            Table1Entry t = table1[i];
            listBox4.Items.Add(String.Format("Number of records {0}", t.NumberOfRecords));
            for (int j = 0; j < t.NumberOfRecords; j++)
            {
                listBox4.Items.Add(String.Format("Size {0}", table3[t.Guess + j]));
            }
        }
        
    }
}
