using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2Modder;

namespace IL2Modder.IL2
{
    public class heirMesh
    {

        public static bool chunkFindCheck(String name)
        {
            return Form1.Instance.checkChunk(name);
        }

        public static void hideSubTrees(String name)
        {
            Form1.Instance.hideTree(name);
        }

        public static void chunkVisible(String name, bool visible)
        {
            Form1.Instance.chunkVisible(name, visible);
        }

        public static float getPowerControl()
        {
            return Form1.Throttle;
        }
    }
}
