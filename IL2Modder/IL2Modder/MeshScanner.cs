using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace IL2Modder
{
    public partial class MeshScanner : Form
    {
        String[] error_scans = new string[] 
        {
            "[NFrames",
            "0 0 0.00",
            "[sfVertices"
        };
        String[] error_reports = new string[]
        {
            "Line contains '[NFrames n]' which is a bug. Remove the '[' and ']' from around the NFrames.\r\n" ,
            "Spotted a '0 0 0.00'. If this is in a face group it is a bug. Replace it with '0 0 0'\r\n",
            "Mesh contains a sfVertices array\r\n"
        };

        public MeshScanner()
        {
            InitializeComponent();
        }
        public void Scan(String file)
        {
            String dir = Path.GetDirectoryName(file);
            bool Binary = false;
            using (BinaryReader b = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                short test = b.ReadInt16();
                if (test == 16897)
                    Binary = true;
                b.Close();

                if (Binary)
                {
                    textBox1.Text+="Mesh is binary, currently unsupported";
                    return;
                }
                using (TextReader reader = File.OpenText(file))
                {
                    string line;
                    int count=0;
                    
                    while ((line = reader.ReadLine()) != null)
                    {
                        for (int i = 0; i < error_scans.GetLength(0); i++)
                        {
                            if (line.Contains(error_scans[i]))
                            {
                                textBox1.Text += error_reports[i];
                                count++;
                            }
                        }
                    }
                    reader.Close();
                    if (count == 0)
                    {
                        textBox1.Text+="Looks good to me.\r\n";
                    }
                }
            }
        }
    }
}
