using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IL2Modder.IL2
{
    public class FaceGroup
    {
        public int StartVertex;
        public int VertexCount;
        public int StartFace;
        public int FaceCount;
        public int Material;

        public void Write(BinaryWriter b)
        {
            b.Write(StartVertex);
            b.Write(VertexCount);
            b.Write(StartFace);
            b.Write(FaceCount);
            b.Write(Material);
        }
    }
}
