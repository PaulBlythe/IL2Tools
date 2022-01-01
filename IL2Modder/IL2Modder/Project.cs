using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IL2Modder
{
    public class Il2Project
    {
        public List<String> SearchPaths = new List<String>();
        public String HIM;
        public String Path = null;

        public void Serialise(String path)
        {
            using (TextWriter writer = File.CreateText(path))
            {
                writer.WriteLine(SearchPaths.Count);
                foreach (String s in SearchPaths)
                {
                    writer.WriteLine(s);
                }
                writer.WriteLine(HIM);
                writer.Close();
            }
        }
        public void DeSerialise(String path)
        {
            using (TextReader reader = File.OpenText(path))
            {
                int npaths = int.Parse(reader.ReadLine());
                for (int i = 0; i < npaths; i++)
                {
                    SearchPaths.Add(reader.ReadLine());
                }
                HIM = reader.ReadLine();
            }
        }

    }
}
