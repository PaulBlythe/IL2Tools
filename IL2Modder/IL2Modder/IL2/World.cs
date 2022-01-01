using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.IL2
{
    public class World
    {
        public Atmosphere Atm;
        public static World instance;

        public World()
        {
            Atm = new Atmosphere();
            instance = this;
        }
        public static World cur()
        {
            return instance;
        }
        public static float getTimeofDay()
        {
            return DateTime.Now.Hour * 3600.0f + DateTime.Now.Minute * 60.0f + DateTime.Now.Second;
        }
    }
}
