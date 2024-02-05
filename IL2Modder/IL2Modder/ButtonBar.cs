using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace IL2Modder
{
    public partial class ButtonBar : Form
    {
        public static ButtonBar Instance;

        public TreeView tree;
        public String screenshot
        {
            get { return textBox3.Text; }
            set { textBox3.Text = value; }
        }
        public String textures
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }
        public String distance
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }


        public ButtonBar()
        {
            InitializeComponent();
            tree = treeView1;
            Instance = this;
        }

        private void ButtonBar_Load(object sender, EventArgs e)
        {

        }
        
        #region HIM menu
        private void loadHimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.loadHimToolStripMenuItem_Click(sender, e);
        }
 
        private void saveHimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.saveHimToolStripMenuItem_Click(sender, e);
        }

        private void newHimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.newHimToolStripMenuItem_Click(sender, e);
        }

        private void load3DSFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.load3DSFileToolStripMenuItem_Click(sender, e);
        }

        private void loadACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.loadACToolStripMenuItem_Click(sender, e);
        }

        private void loadScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.loadScriptToolStripMenuItem_Click(sender, e);
        }

        private void loadCockpitScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.loadCockpitScriptToolStripMenuItem_Click(sender, e);
        }

        private void scanMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.scanMeshToolStripMenuItem_Click(sender, e);
        }

        private void loadWeaponLoadoutsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.loadWeaponLoadoutsToolStripMenuItem_Click(sender, e);
        }
        #endregion

        #region File menu
        private void loadProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.loadProjectToolStripMenuItem_Click(sender, e);
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.saveProjectToolStripMenuItem_Click(sender, e);
        }

        private void saveProjectAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.saveProjectAsToolStripMenuItem_Click(sender, e);
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.saveAllToolStripMenuItem_Click(sender, e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.exitToolStripMenuItem_Click(sender, e);
        }


        #endregion

        #region Render menu
        private void changeSkyboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.changeSkyboxToolStripMenuItem_Click(sender, e);
        }
        
        private void screenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.screenshotToolStripMenuItem_Click(sender, e);
        }

        private void textureBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.textureBrowserToolStripMenuItem_Click(sender, e);
        }

        private void toggleLightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.toggleLightingToolStripMenuItem_Click(sender, e);
        }

        private void toggleRenderModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.toggleRenderModeToolStripMenuItem_Click(sender, e);
        }

        private void generateSkinTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.generateSkinTextureToolStripMenuItem_Click(sender,e);
        }
        #endregion

        #region Settings menu
        private void setTextureDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.setTextureDirectoryToolStripMenuItem_Click(sender, e);
        }

        private void setScreenshotDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.setScreenshotDirectoryToolStripMenuItem_Click(sender, e);
        }

        private void addWeaponToDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.addWeaponToDatabaseToolStripMenuItem_Click(sender, e);
        }
        #endregion

        #region Button bar
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Form1.Instance.toolStripButton1_Click(sender, e);
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Form1.Instance.toolStripButton2_Click(sender, e);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Form1.Instance.toolStripButton3_Click(sender, e);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Form1.Instance.toolStripButton4_Click(sender, e);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            Form1.Instance.toolStripButton5_Click(sender, e);
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            Form1.Instance.toolStripButton6_Click(sender, e);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            Form1.Instance.toolStripButton7_Click(sender, e);
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            Form1.Instance.toolStripButton8_Click(sender, e);
        }
        #endregion

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
           
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            Form1.Instance.treeView1_AfterCheck(sender, e);
        }

        #region Event buttons
        private void button1_Click(object sender, EventArgs e)
        {
            Form1.Instance.button1_Click(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1.Instance.button2_Click(sender, e);
        }
        // repair plane
        private void button3_Click(object sender, EventArgs e)
        {
            Form1.Instance.RepairPlane();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1.Instance.button4_Click(sender, e);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Form1.Instance.button17_Click(sender, e);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form1.Instance.button5_Click(sender, e);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Form1.Instance.button10_Click(sender, e);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form1.Instance.button7_Click(sender, e);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Form1.Instance.button8_Click(sender, e);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            Form1.Instance.button18_Click(sender, e);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Form1.Instance.button9_Click(sender, e);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form1.Instance.button6_Click(sender, e);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Form1.Instance.button11_Click(sender, e);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Form1.Instance.button12_Click(sender, e);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Form1.Instance.button16_Click(sender, e);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Form1.Instance.button13_Click(sender, e);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Form1.Instance.button14_Click(sender, e);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Form1.Instance.button15_Click(sender, e);
        }
        #endregion

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            Form1.Instance.hScrollBar1_ValueChanged(sender, e);
        }

        private void hScrollBar2_ValueChanged(object sender, EventArgs e)
        {
            Form1.Instance.hScrollBar2_ValueChanged(sender, e);
        }

        private void joystick1_JoystickClicked()
        {
            Form1.Instance.joystick1_JoystickClicked(joystick1);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.checkBox5_CheckedChanged(sender, e);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.checkBox4_CheckedChanged(sender, e);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.checkBox1_CheckedChanged(sender, e);
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.checkBox7_CheckStateChanged(sender, e);
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.checkBox6_CheckedChanged(sender, e);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.checkBox2_CheckedChanged(sender, e);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.checkBox3_CheckedChanged(sender, e);
        }

        private void ButtonBar_FormClosing(object sender, FormClosingEventArgs e)
        {
            String file = Form1.TexturesDirectory + "\\bb.layout";
            using (TextWriter writer = File.CreateText(file))
            {
                writer.WriteLine(String.Format("X {0}", this.Location.X));
                writer.WriteLine(String.Format("Y {0}", this.Location.Y));
                writer.Close();
            }
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            Form1.Instance.BakeAmbientOcclusion();
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.DrawNormals(checkBox8.Checked);
        }

        private void regenerateNormalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.RegenerateNormals();
        }

        private void toggleCullingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.ToggleCulling();
        }

        private void swapClockwiseTrianglesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.SwapTriangles();
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.Ocean(checkBox9.Checked);
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.Fog(checkBox10.Checked);
        }

        private void hScrollBar3_ValueChanged(object sender, EventArgs e)
        {
            float val = hScrollBar3.Value / 110.0f;
            Form1.Instance.setThrottle(val);
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.ShipMode(checkBox11.Checked);
        }

        private void saveAsDAEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.SaveAsDAE();
        }

        private void saveAsOGREToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.SaveAsOGRE();
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.AfterburnerToggle(checkBox12.Checked);
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.BoosterToggle(checkBox13.Checked);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            Form1.Instance.ArrestorToggle();
        }

        private void saveAsOBJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.SaveAsObj();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            Form1.Instance.AdjustLighting();
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            Form1.Instance.EnableBumpMapping(checkBox14.Checked);
        }

        private void hScrollBar4_ValueChanged(object sender, EventArgs e)
        {
            Form1.Instance.SetBumpLevel(hScrollBar4.Value / 10.0f);
        }

        private void saveAsFBXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.SaveAsFBX();
        }

        private void saveForFox1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void cOpyCommonTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.CopyCommonTexturesToGuruEngine();
        }

        private void exportToGuruToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.SaveToFox1();
        }

        /// <summary>
        /// Copy the paint scheme textures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyPaintschemesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.CopyPaintSchemesToGuruEngine();
        }

        /// <summary>
        /// Export collision meshes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportCollisionMeshesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.SaveCollisionMeshes();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void exportForUE4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.SaveToUE4();
        }

        /// <summary>
        /// Save for UE5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToUE5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.SaveToUE5();
        }

        private void exportHooksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.Instance.ExportHooksToText();
        }
    }
}
