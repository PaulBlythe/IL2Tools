using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.IL2
{
    public class WeaponSlot
    {
        public String WeaponName;
        public int Slot;

        public WeaponSlot(int Slot, String Name)
        {
            this.Slot = Slot;
            this.WeaponName = Name;
        }
    }
    public class WeaponLoadout
    {
        public String name;
        public List<WeaponSlot> Weapons = new List<WeaponSlot>();

        public WeaponLoadout(String Name)
        {
            name = Name;
        }

        public void Add(int slot, String name, int Count)
        {
            WeaponSlot w = new WeaponSlot(slot, name);
            Weapons.Add(w);
        }
    }
}
