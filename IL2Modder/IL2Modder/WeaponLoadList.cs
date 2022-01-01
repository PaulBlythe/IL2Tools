using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IL2Modder.IL2;

namespace IL2Modder
{
    public partial class WeaponLoadList : Form
    {
        WeaponLoadoutArray loads;
        String[] Names;
        public ObjectViewer viewer;

        public WeaponLoadList()
        {
            InitializeComponent();
        }
        public void Setup(WeaponLoadoutArray wl, String [] names)
        {
            foreach (WeaponLoadout w in wl.Loads)
            {
                listBox1.Items.Add(w.name);
            }
            loads = wl;
            Names = names;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            viewer.SelectedWeaponLoadout = -1;
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                listBox2.Items.Clear();
                WeaponLoadout ws = loads.Loads[listBox1.SelectedIndex];
                foreach (WeaponSlot slot in ws.Weapons)
                {
                    listBox2.Items.Add(String.Format("{0} on {1}", slot.WeaponName, Names[slot.Slot]));
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                
                WeaponLoadout ws = loads.Loads[listBox1.SelectedIndex];
                foreach (WeaponSlot slot in ws.Weapons)
                {
                    String h = viewer.WeaponLoadouts.Hooks[slot.Slot];
                    if ((h.Contains("CANNON")) || (h.Contains("MGUN")) || (slot.WeaponName.Contains("Null")))
                    {
                    }
                    else
                    {
                        if (!viewer.Weapons.Keys.Contains<string>(slot.WeaponName))
                        {
                            MessageBox.Show("Missing weapon detected " + slot.WeaponName, "Loadout validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                MessageBox.Show("Looks good to me", "Loadout validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
                viewer.SelectedWeaponLoadout = listBox1.SelectedIndex;
            else
                viewer.SelectedWeaponLoadout = -1;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
