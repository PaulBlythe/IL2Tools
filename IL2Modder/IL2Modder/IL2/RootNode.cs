using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.IL2
{
    public class RootNode:Node
    {
        public float VisibiltySphere;

        public RootNode()
        {
            Name = "_ROOT_";
            Type = NodeType.Root;
            VisibiltySphere = 100;
        }
    }
}
