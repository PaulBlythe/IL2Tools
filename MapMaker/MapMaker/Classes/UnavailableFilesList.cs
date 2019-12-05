using System;
using System.IO;
using System.Diagnostics;

namespace MapMaker
{

    public class UnavailableFilesList
    {
        private string Dir = "";

        public void SetDirectory(string path)
        {
            Dir = System.IO.Path.Combine(path, "missing-files");
            Directory.CreateDirectory(Dir);
        }

        public bool Contains(string name)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(Dir));
            return File.Exists(System.IO.Path.Combine(Dir, name));
        }

        public void Add(string name)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(Dir));
            File.Create(System.IO.Path.Combine(Dir, name));
        }
    }

}
