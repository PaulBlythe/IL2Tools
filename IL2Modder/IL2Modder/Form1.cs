using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Globalization;

using IL2Modder.IL2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using IL2Modder.Tools;
using Skill.FbxSDK;
using IL2Modder.Forms;

namespace IL2Modder
{
    public partial class Form1 : Form
    {
        HIM current_object;
        Quaternion rotation;
        Quaternion light_rotation;

        ContentManager contentManager;
        Loading loading_dialog;
        Node copynode;
        TreeNode copysrc;
        int skybox_id = 0;

        public static Form1 Instance;
        public static TextureManager Manager;
        public static GraphicsDevice graphics;
        public static Il2Project project;
        public Vector3 eye;
        public static bool Animate;
        public static bool EnginesON;
        public static bool ShadowsOn;
        public static bool BailOut;
        public static bool ExportShadows;
        public static bool ExportBaseMesh;
        public static bool ExportLODs;
        public static bool ExportLODShadows;
        public static bool ExportHooks;
        public static bool ExportCollisionMeshes;
        public static float Flaps;
        public static float Roll;
        public static float Pitch;
        public static float Yaw;
        public static float Throttle;
        public static float Arrestor;
        public static bool Lighting;
        public static bool Screenshot;
        public static String ShotName;
        public static String TexturesDirectory = @"\\NFS1\FileStore\Aircraft\3do\TEXTURES";
        public static String ScreenShotDirectory = "";
        public static String FBXDirectory = "";
        public static Script script;
        public static CockpitScript cscript;
        public static List<String> missing_textures = new List<String>();
        public static List<String> written_effects = new List<String>();
        public static String comms;
        public static HookEdit hook_edit;
        public ButtonBar buttons;
        public FbxSdkManager sdkManager = null;
        public FbxScene scene = null;
        public Skill.FbxSDK.IO.FbxExporter exporter = null;
        public Skill.FbxSDK.IO.FbxStreamOptionsFbxWriter exportOptions = null;
        public Dictionary<String, FbxNode> fbx_nodes = new Dictionary<string, FbxNode>();
        public Dictionary<String, FbxMesh> fbx_meshes = new Dictionary<string, FbxMesh>();
        public int Fbx_node_count;
        public static float mouse_sensitivity = 2048;
        World world = new World();

        TreeView treeView1;

        int dx = 0;
        int dy = 0;
        int mouse_x, mouse_y;
        int engine_fires = 0;
        bool down = false;
        bool shoot = false;
        bool loading = false;

        public Form1()
        {
            InitializeComponent();

            Manager = new TextureManager();
            dx = dy = 0;
            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);
            eye = new Vector3(0, 0, -25);
            rotation = Quaternion.Identity;
            light_rotation = Quaternion.Identity;

            contentManager = new ContentManager(objectViewer1.Services, Application.StartupPath + "/CompiledAssets");

            Animate = false;
            Lighting = true;
            Flaps = 0;
            Roll = 0;
            Pitch = 0;
            Yaw = 0;
            EnginesON = true;
            ShadowsOn = false;
            Screenshot = false;
            BailOut = false;
            copysrc = null;
            copynode = null;
            current_object = null;
            cscript = null;
            script = null;
            Throttle = 0;
            project = new Il2Project();

            Instance = this;
            buttons = new ButtonBar();
            buttons.Show();

            treeView1 = buttons.tree;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (File.Exists("settings.bin"))
            {
                using (BinaryReader b = new BinaryReader(File.Open("settings.bin", FileMode.Open)))
                {
                    try
                    {
                        TexturesDirectory = b.ReadString();
                      
                    }
                    catch (Exception)
                    {
                        TexturesDirectory = "";
                    }
                    try
                    {
                        ScreenShotDirectory = b.ReadString();
                    }
                    catch (Exception)
                    {
                        ScreenShotDirectory = "";
                    }
                    try
                    {
                        FBXDirectory = b.ReadString();
                    }
                    catch (Exception)
                    {
                        FBXDirectory = "";
                    }
                    b.Close();
                }
                buttons.textures = TexturesDirectory;
                buttons.screenshot = ScreenShotDirectory;
            }
            if (File.Exists("weapons.txt"))
            {
                using (TextReader tr = File.OpenText("weapons.txt"))
                {
                    string line;
                    while ((line = tr.ReadLine()) != null)
                    {
                        string[] parts = line.Split(' ');
                        objectViewer1.Weapons.Add(parts[0], parts[1]);
                    }
                }
            }
            String file = Form1.TexturesDirectory + "\\bb.layout";
            if (File.Exists(file))
            {
                using (TextReader tr = File.OpenText(file))
                {
                    string line;
                    int x = 0;
                    int y = 0;
                    while ((line = tr.ReadLine()) != null)
                    {
                        string[] parts = line.Split(' ');
                        if (line.StartsWith("X"))
                            x = int.Parse(parts[1]);
                        if (line.StartsWith("Y"))
                            y = int.Parse(parts[1]);

                        //MessageBox.Show("X = " + x.ToString() + " Y = " + y.ToString(), "buttons");
                    }
                    tr.Close();
                    buttons.SetDesktopLocation(x, y);
                    buttons.Location = new System.Drawing.Point(x, y);
                }
            }

            file = Form1.TexturesDirectory + "\\ov.layout";
            if (File.Exists(file))
            {
                using (TextReader tr = File.OpenText(file))
                {
                    string line;
                    int x = 0;
                    int y = 0;
                    int w = 0;
                    int h = 0;
                    while ((line = tr.ReadLine()) != null)
                    {
                        string[] parts = line.Split(' ');
                        if (line.StartsWith("X"))
                            x = int.Parse(parts[1]);
                        if (line.StartsWith("Y"))
                            y = int.Parse(parts[1]);
                        if (line.StartsWith("W"))
                            w = int.Parse(parts[1]);
                        if (line.StartsWith("H"))
                            h = int.Parse(parts[1]);

                    }
                    tr.Close();

                    this.SetDesktopLocation(x, y);
                    this.Location = new System.Drawing.Point(x, y);
                    this.Width = w;
                    this.Height = h;

                }

            }
        }

        void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            float dz = (float)e.Delta;
            if (ObjectViewer.CockpitMode)
            {
                objectViewer1.Eye += objectViewer1.Forward * dz / 1000.0f;
            }
            else
            {
                eye.Z += dz / 100.0f;
                Matrix m = Matrix.CreateFromQuaternion(rotation);
                objectViewer1.Eye = Vector3.Transform(eye, m);
            }
        }

        public void loadHimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objectViewer1.my_effect == null)
            {
                loading_dialog = new Loading();
                loading_dialog.Show();
                Application.DoEvents();
                //LoadShader();
                LoadPrebuiltContent();
                loading_dialog.Close();
                Form1.graphics = objectViewer1.GraphicsDevice;
                objectViewer1.lightdirection = new Vector3(0, 0.7f, 0.6f);
            }
            if (current_object != null)
            {
                objectViewer1.mesh = null;
                objectViewer1.Clear();
                current_object = null;
                Manager.Dispose();
                treeView1.Nodes.Clear();
                script = null;
            }
            OpenFileDialog ofd = new OpenFileDialog();
            missing_textures.Clear();
            ofd.DefaultExt = "*.him";
            ofd.Filter = "HIM files (*.him)|*.him";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                loading = true;
                graphics = objectViewer1.GraphicsDevice;
                if (project != null)
                    project.HIM = ofd.FileName;
                current_object = new HIM(ofd.FileName);
                ShotName = Path.GetDirectoryName(ofd.FileName) + ".png";
                ShotName = Path.GetFileNameWithoutExtension(ShotName);
                this.Text = current_object.Name;
                TreeNode tn = new TreeNode("_ROOT_");
                tn.Tag = null;
                tn.Checked = true;
                treeView1.Nodes.Add(tn);
                foreach (Node n in current_object.root.children)
                {
                    TreeNode tn2 = new TreeNode(n.Name);
                    tn2.ContextMenuStrip = contextMenuStrip1;
                    tn2.Tag = n;
                    tn2.Checked = !n.Hidden;

                    tn.Nodes.Add(tn2);
                    addNodes(tn2, n);
                }
                objectViewer1.mesh = current_object;
                UpdateChecks(tn);

                if (missing_textures.Count > 0)
                {
                    MissingTextures mt = new MissingTextures();
                    mt.AddList(missing_textures);
                    mt.ShowDialog();
                }
                loading = false;
                objectViewer1.SetTarget(Vector3.Zero);
            }
        }

        private void UpdateChecks(TreeNode n)
        {
            if (n.Tag != null)
            {
                Node nd = (Node)n.Tag;
                n.Checked = !nd.Hidden;
            }
            if ((n.Nodes.Count > 0) && (n.Nodes[0].Tag is MeshNode))
            {
                MeshNode mn = (MeshNode)n.Nodes[0].Tag;
                n.Checked = !mn.Hidden;
            }
            foreach (TreeNode n2 in n.Nodes)
            {
                UpdateChecks(n2);
            }
        }

        private void addNodes(TreeNode tn, Node n)
        {
            foreach (Node n2 in n.children)
            {
                TreeNode tn2 = new TreeNode(n2.Name);
                tn2.ContextMenuStrip = contextMenuStrip1;
                tn2.Tag = n2;
                tn2.Checked = !n2.Hidden;
                if (n.Hidden)
                    tn2.Checked = false;

                tn.Nodes.Add(tn2);
                addNodes(tn2, n2);
            }
        }

        public void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (loading)
                return;

            TreeNode n = e.Node;
            Node n2 = (Node)n.Tag;
            if (n2 != null)
                n2.Hidden = !n.Checked;

            foreach (TreeNode n3 in n.Nodes)
            {
                n3.Checked = n.Checked;
                Node n4 = (Node)n3.Tag;
                if (n4 != null)
                    n4.Hidden = !n.Checked;
                after(n3, !n.Checked);
            }
        }

        private void after(TreeNode n, bool Hidden)
        {
            foreach (TreeNode n2 in n.Nodes)
            {
                Node n3 = (Node)n2.Tag;
                if (n3 != null)
                    n3.Hidden = Hidden;
                n2.Checked = !Hidden;
                after(n2, Hidden);
            }
        }


        #region Content loading
        //********************************************************************************
        //* Load all the content                                                         *
        //********************************************************************************
        private void LoadPrebuiltContent()
        {
            contentManager.Unload();
            contentManager.RootDirectory = Application.StartupPath + "/CompiledAssets";
            float step = 100.0f / 27.0f;
            objectViewer1.my_effect = contentManager.Load<Effect>("Effect");
            loading_dialog.SetProgress((int)(step * 1));
            objectViewer1.skybox = contentManager.Load<TextureCube>("Texture3D");
            loading_dialog.SetProgress((int)(step * 2));
            ObjectViewer.cube = contentManager.Load<Model>("Model");
            ObjectViewer.cube2 = contentManager.Load<Model>("cube");
            loading_dialog.SetProgress((int)(step * 3));
            objectViewer1.cubemap = contentManager.Load<Effect>("CubeEffect");
            loading_dialog.SetProgress((int)(step * 4));
            objectViewer1.glass = contentManager.Load<Effect>("Glass");
            loading_dialog.SetProgress((int)(step * 5));
            objectViewer1.fields = contentManager.Load<Texture2D>("FieldTexture");
            loading_dialog.SetProgress((int)(step * 6));
            objectViewer1.shadow = contentManager.Load<Effect>("ShadowMap");
            loading_dialog.SetProgress((int)(step * 7));
            objectViewer1.lambert = contentManager.Load<Effect>("Lambert");
            loading_dialog.SetProgress((int)(step * 8));
            objectViewer1.particle = contentManager.Load<Effect>("Particle");
            objectViewer1.pointlight = contentManager.Load<Effect>("PointLights");
            objectViewer1.billboard = contentManager.Load<Effect>("Albedo");
            loading_dialog.SetProgress((int)(step * 9));
            objectViewer1.smoke = contentManager.Load<Texture2D>("Smoke");
            objectViewer1.glow = contentManager.Load<Texture2D>("glow");
            loading_dialog.SetProgress((int)(step * 10));
            objectViewer1.fire = contentManager.Load<Texture2D>("Fire");
            loading_dialog.SetProgress((int)(step * 11));
            objectViewer1.bomb = contentManager.Load<Model>("Bomb");
            loading_dialog.SetProgress((int)(step * 12));
            objectViewer1.cartridge = contentManager.Load<Texture2D>("cartridge");
            loading_dialog.SetProgress((int)(step * 13));
            objectViewer1.bomb100 = contentManager.Load<Model>("bomb100");
            loading_dialog.SetProgress((int)(step * 14));
            objectViewer1.tank = contentManager.Load<Model>("tank");
            loading_dialog.SetProgress((int)(step * 15));
            objectViewer1.rocket = contentManager.Load<Model>("rocket");
            loading_dialog.SetProgress((int)(step * 16));
            ObjectViewer.font = contentManager.Load<SpriteFont>("Big");
            loading_dialog.SetProgress((int)(step * 17));
            objectViewer1.shoot = contentManager.Load<Texture2D>("shoot");
            loading_dialog.SetProgress((int)(step * 18));
            objectViewer1.ocean1 = contentManager.Load<Texture2D>("Ocean1_N");
            loading_dialog.SetProgress((int)(step * 19));
            objectViewer1.ocean2 = contentManager.Load<Texture2D>("Ocean2_N");
            loading_dialog.SetProgress((int)(step * 20));
            objectViewer1.ocean3 = contentManager.Load<Texture2D>("Ocean3_N");
            loading_dialog.SetProgress((int)(step * 21));
            objectViewer1.ocean4 = contentManager.Load<Texture2D>("Ocean4_N");
            loading_dialog.SetProgress((int)(step * 22));
            objectViewer1.smokeglow = contentManager.Load<Texture2D>("SmokeGlow");
            loading_dialog.SetProgress((int)(step * 23));
            objectViewer1.grounddust = contentManager.Load<Texture2D>("GroundDust");
            loading_dialog.SetProgress((int)(step * 24));
            objectViewer1.ocean_shader = contentManager.Load<Effect>("OceanShader");
            loading_dialog.SetProgress((int)(step * 25));
            objectViewer1.bumpmap = contentManager.Load<Effect>("Bump");
            loading_dialog.SetProgress((int)(step * 26));
            //objectViewer1.multipass = contentManager.Load<Effect>("MultiPassLight");
            //objectViewer1.PostLoad();
            loading_dialog.SetProgress(100);
        }

        private void LoadShader()
        {
#if BUILDER

            contentManager.Unload();
            string path = Application.StartupPath;// Directory.GetCurrentDirectory();
            float step = 100.0f / 34.0f;

            // Tell the ContentBuilder what to build.
            contentBuilder.Clear();
            contentBuilder.Add(path + "/Assets/Shader.fx", "Effect", null, "EffectProcessor");
            loading_dialog.SetProgress((int)step);
            contentBuilder.Add(path + "/Assets/BlueSky512.dds", "Texture3D", null, "TextureProcessor");
            loading_dialog.SetProgress((int)step*2);
            contentBuilder.Add(path + "/Assets/cube.fbx", "Model", null, "ModelProcessor");
            loading_dialog.SetProgress((int)(step*3));
            contentBuilder.Add(path + "/Assets/CubeMap.fx", "CubeEffect", null, "EffectProcessor");
            loading_dialog.SetProgress((int)(step*4));
            contentBuilder.Add(path + "/Assets/Glass.fx", "GlassEffect", null, "EffectProcessor");
            loading_dialog.SetProgress((int)(step*5));
            contentBuilder.Add(path + "/Assets/Fields.png", "FieldTexture", null, "TextureProcessor");
            loading_dialog.SetProgress((int)(step*6));
            contentBuilder.Add(path + "/Assets/ShadowMap.fx", "ShadowMap", null, "EffectProcessor");
            loading_dialog.SetProgress((int)(step*7));
            contentBuilder.Add(path + "/Assets/Lambert.fx", "Lambert", null, "EffectProcessor");
            loading_dialog.SetProgress((int)(step*8));
            contentBuilder.Add(path + "/Assets/Particle.fx", "Particle", null, "EffectProcessor");
            loading_dialog.SetProgress((int)(step*9));
            contentBuilder.Add(path + "/Assets/smoke.png", "Smoke", null, "TextureProcessor");
            loading_dialog.SetProgress((int)(step*10));
            contentBuilder.Add(path + "/Assets/fire.png", "Fire", null, "TextureProcessor");
            loading_dialog.SetProgress((int)(step*11));
            contentBuilder.Add(path + "/Assets/32bomb-model.x", "Bomb", null, "ModelProcessor");
            loading_dialog.SetProgress((int)(step*12));
            contentBuilder.Add(path + "/Assets/shoot.png", "shoot", null, "TextureProcessor");
            loading_dialog.SetProgress((int)(step * 13));
            contentBuilder.Add(path + "/Assets/cartridge.png", "cartridge", null, "TextureProcessor");
            loading_dialog.SetProgress((int)(step * 14));
            contentBuilder.Add(path + "/Assets/bomb.x", "bomb100", null, "ModelProcessor");
            loading_dialog.SetProgress((int)(step * 15));
            contentBuilder.Add(path + "/Assets/tank.x", "tank", null, "ModelProcessor");
            loading_dialog.SetProgress((int)(step * 16));
            contentBuilder.Add(path + "/Assets/rocket.x", "rocket", null, "ModelProcessor");
            loading_dialog.SetProgress((int)(step * 17));

            // Build this new data.
            string buildError = contentBuilder.Build();
            
            if (string.IsNullOrEmpty(buildError))
            {
                objectViewer1.my_effect = contentManager.Load<Effect>("Effect");
                loading_dialog.SetProgress((int)(step * 18));
                objectViewer1.skybox = contentManager.Load<TextureCube>("Texture3D");
                loading_dialog.SetProgress((int)(step * 19));
                objectViewer1.cube = contentManager.Load<Model>("Model");
                loading_dialog.SetProgress((int)(step * 20));
                objectViewer1.cubemap = contentManager.Load<Effect>("CubeEffect");
                loading_dialog.SetProgress((int)(step * 21));
                objectViewer1.glass = contentManager.Load<Effect>("GlassEffect");
                loading_dialog.SetProgress((int)(step * 22));
                objectViewer1.fields = contentManager.Load<Texture2D>("FieldTexture");
                loading_dialog.SetProgress((int)(step * 23));
                objectViewer1.shadow = contentManager.Load<Effect>("ShadowMap");
                loading_dialog.SetProgress((int)(step * 24));
                objectViewer1.lambert = contentManager.Load<Effect>("Lambert");
                loading_dialog.SetProgress((int)(step * 25));
                objectViewer1.particle = contentManager.Load<Effect>("Particle");
                loading_dialog.SetProgress((int)(step * 26));
                objectViewer1.smoke = contentManager.Load<Texture2D>("Smoke");
                loading_dialog.SetProgress((int)(step * 27));
                objectViewer1.fire = contentManager.Load<Texture2D>("Fire");
                loading_dialog.SetProgress((int)(step * 28));
                objectViewer1.bomb = contentManager.Load<Model>("Bomb");
                loading_dialog.SetProgress((int)(step * 29));
                objectViewer1.cartridge = contentManager.Load<Texture2D>("cartridge");
                loading_dialog.SetProgress((int)(step * 30));
                objectViewer1.bomb100 = contentManager.Load<Model>("bomb100");
                loading_dialog.SetProgress((int)(step * 31));
                objectViewer1.tank = contentManager.Load<Model>("tank");
                loading_dialog.SetProgress((int)(step * 32));
                objectViewer1.rocket = contentManager.Load<Model>("rocket");
                loading_dialog.SetProgress((int)(step * 33));
                objectViewer1.shoot = contentManager.Load<Texture2D>("shoot");
                loading_dialog.SetProgress(100);

                
            }
            else
            {
                // If the build failed, display an error message.
                MessageBox.Show(buildError, "Error");
            }
#endif

        }
        #endregion

        // change view distance
        public void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            HScrollBar s = (HScrollBar)sender;
            objectViewer1.ViewDistance = (float)s.Value;
            buttons.distance = String.Format("{0}", s.Value);
        }

        // toggle skybox
        public void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox s = (CheckBox)sender;
            if (s.Checked)
                objectViewer1.EnableSkybox();
            else
                objectViewer1.DisableSkybox();
        }

        // research dialog
        private void compareBinaryToTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".msh";
            ofd.Filter = "Mesh files (*.msh)|*.msh";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BinaryMesh bm = new BinaryMesh(ofd.FileName);
                Mesh mesh_binary = new Mesh(ofd.FileName);
                String path = Path.GetDirectoryName(ofd.FileName);
                path = path.TrimEnd(new char[] { 'O', 'l', 'd', '/' });
                mesh_binary.ReadBinary(ofd.FileName, path);
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Mesh tm = new Mesh(ofd.FileName);
                    Research r1 = new Research();
                    r1.Show();
                    r1.Sections(bm.contents.Count);
                    r1.Table1(bm.Table1start);
                    r1.Table2(bm.Table2start);
                    r1.table3 = bm.table3;
                    r1.table1 = bm.table1;
                    r1.Table2Size(bm.Table2start - bm.header5);

                    r1.HexDisplay2(bm.table2);
                    int ii = 0;
                    foreach (Table1Entry t in bm.table1)
                    {
                        r1.AddToTable1(String.Format("{0:X4},{1:X4},{2:X4},{3:X4}    : {4}", t.Type, t.Guess, t.NumberOfRecords, t.SomethingElse, bm.contents[ii++]));
                    }
                    foreach (String s in bm.contents)
                    {
                        r1.AddMessage(s);
                    }

                    float[] v1 = new float[3] { tm.Verts[0].Position.X, tm.Verts[0].Position.Y, tm.Verts[0].Position.Z };

                    Int32[] lods = new Int32[tm.LodDistances.Count];
                    for (int i = 0; i < tm.LodDistances.Count; i++)
                    {
                        lods[i] = (Int32)tm.LodDistances[i];
                    }

                    r1.HexDisplay(bm.byte_buffer);
                }
            }

        }

        // toggle animation
        public void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            Animate = ((CheckBox)sender).Checked;
        }

        // toggle ground plane
        public void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
                objectViewer1.EnableGroundPlane();
            else
                objectViewer1.DisableGroundPlane();
        }

        // toggle flaps
        public void button8_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleFlaps();
        }

        // joystick input
        public void joystick1_JoystickClicked(Joystick joystick1)
        {
            Roll = joystick1.Roll;
            Pitch = joystick1.Pitch;

        }

        // rudder input
        public void hScrollBar2_ValueChanged(object sender, EventArgs e)
        {
            HScrollBar s = (HScrollBar)sender;
            float val = s.Value - 45;
            val /= 45;
            Yaw = MathHelper.ToRadians(val * 40);
        }

        // toggle guns
        public void button9_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleGuns();
        }

        // copy node to local store
        public void copyNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                copysrc = treeView1.SelectedNode;
                copynode = (Node)treeView1.SelectedNode.Tag;
            }
        }

        // paste node
        public void pasteNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeView1.SelectedNode;
            if (tn != null)
            {
                Node n = (Node)tn.Tag;
                Node newn = Node.CopyNode(copynode);
                n.children.Add(newn);

                TreeNode tn2 = new TreeNode("pasted_" + copysrc.Text);
                tn2.Checked = copysrc.Checked;
                tn2.Tag = newn;
                tn2.ContextMenuStrip = contextMenuStrip1;

                int i = 0;
                foreach (TreeNode tn3 in copysrc.Nodes)
                {
                    TreeNode tn4 = new TreeNode("pasted_" + tn3.Text);
                    tn4.Checked = tn3.Checked;
                    tn4.ContextMenuStrip = contextMenuStrip1;
                    tn4.Tag = n.children[i++];

                    tn2.Nodes.Add(tn4);
                    // THIS IS A BUG
                }
                tn.Nodes.Add(tn2);
            }
        }

        // alter the world matrix for a node
        private void adjustMatrixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                copysrc = treeView1.SelectedNode;
                AdjustMatrix am = new AdjustMatrix();
                am.node = (Node)copysrc.Tag;
                if (am.node != null)
                    am.Show(); 
            }
        }

        // toggle shadows
        public void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            ShadowsOn = ((CheckBox)sender).Checked;
            if (ShadowsOn)
                objectViewer1.EnableShadows();
            else
                objectViewer1.DisableShadows();
        }

        // toggle engine smoke
        public void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
                objectViewer1.EnableEngineSmoke();
            else
                objectViewer1.DisableEngineSmoke();
        }

        // engine fire
        public void button10_Click(object sender, EventArgs e)
        {
            engine_fires++;
            if (engine_fires > 3)
            {
                engine_fires = 0;
                objectViewer1.DisableEngineFire();
            }
            else
            {
                objectViewer1.EnableEngineFire(engine_fires);
            }
        }

        // toggle bail out
        public void button1_Click(object sender, EventArgs e)
        {
            if (BailOut)
                BailOut = false;
            else
                BailOut = true;
        }

        // add bomb
        public void button5_Click(object sender, EventArgs e)
        {
            if (objectViewer1.mesh != null)
            {
                objectViewer1.bomb_count++;
                if (objectViewer1.bomb_count > objectViewer1.BombHooks.Count)
                {
                    objectViewer1.bomb_count = 0;
                }
            }
        }

        // explode mesh
        public void button7_Click(object sender, EventArgs e)
        {
            if (ObjectViewer.Explode)
                ObjectViewer.Explode = false;
            else
                ObjectViewer.Explode = true;
        }

        // toggle damage meshes
        public void toolStripButton1_Click(object sender, EventArgs e)
        {
            String[] checks = new String[] { "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9" };
            ToggleVisible(treeView1.Nodes[0], checks);
        }

        // if name ends with anything in the array, toggle it's visibilty flag
        public void ToggleVisible(TreeNode tn, String[] checks)
        {
            foreach (String s in checks)
            {
                if (tn.Text.EndsWith(s))
                {
                    Node nd = (Node)tn.Tag;
                    tn.Checked = !tn.Checked;
                    if (nd != null)
                        nd.Hidden = !tn.Checked;
                }
            }
            foreach (TreeNode tn2 in tn.Nodes)
            {
                ToggleVisible(tn2, checks);
            }
        }

        public void toolStripButton2_Click(object sender, EventArgs e)
        {
            String[] checks = new String[] { "CAP", "cap" };
            ToggleVisible(treeView1.Nodes[0], checks);
        }

        public void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            System.Media.SystemSounds.Question.Play();
        }

        public void dumpAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node tn = (Node)treeView1.SelectedNode.Tag;
                if (tn is MeshNode)
                {
                    MeshNode mn = (MeshNode)tn;
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Mesh files *.msh |*.msh";
                    sfd.DefaultExt = "*.msh";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        using (TextWriter writer = File.CreateText(sfd.FileName))
                        {
                            mn.Serialize(writer);
                            writer.Close();
                        }

                    }

                }
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        // toggle display of collision mesh
        public void toolStripButton3_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleCollision();
        }

        // toggle shoot
        public void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            shoot = ((CheckBox)sender).Checked;
            objectViewer1.ToggleCollision();
            objectViewer1.EnableShooting();
        }

        // setup the textures directory
        public void setTextureDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Point the app at the 'Textures' directory in IL2";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                buttons.textures = fbd.SelectedPath;
                TexturesDirectory = fbd.SelectedPath;
                using (BinaryWriter b = new BinaryWriter(File.Open("settings.bin", FileMode.Create)))
                {
                    b.Write(TexturesDirectory);
                    b.Write(ScreenShotDirectory);
                    b.Write(FBXDirectory);
                    b.Close();
                }
            }
        }

        // texture browser
        public void textureBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextureBrowser b = new TextureBrowser(Manager);
            b.ShowDialog();
        }

        // toggle landing gear
        public void button4_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleGear();
        }

        // edit material
        public void editMaterialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node tn = (Node)treeView1.SelectedNode.Tag;
                if (tn is MeshNode)
                {
                    MaterialEditor me = new MaterialEditor((MeshNode)tn);
                    me.ShowDialog();
                }
                else
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }
            }
        }

        // show hooks
        public void showHooksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node tn = (Node)treeView1.SelectedNode.Tag;
                if (tn is MeshNode)
                {
                    DataDisplay me = new DataDisplay();

                    MeshNode mn = (MeshNode)tn;
                    foreach (Hook h in mn.mesh.Hooks)
                    {
                        me.AddData(h.Name);
                    }
                    me.ShowDialog();
                }
                else
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }
            }
        }

        // add bomb
        public void button6_Click(object sender, EventArgs e)
        {
            objectViewer1.AddBomb();
        }

        // fuel leak
        public void button11_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleFuelLeak();
        }

        // fuel fire
        public void button12_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleFuelFire();
        }

        // drop tanks
        public void button13_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleDropTanks();
        }

        // Rockets
        public void button14_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleRockets();
        }

        // Bomb bays
        public void button15_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleBombBays();
        }

        #region Script handling
        void SkipFunction(TextReader r)
        {
            int bcount = 1;
            String line = r.ReadLine();
            while (!line.Contains("{"))
                line = r.ReadLine();
            while (bcount > 0)
            {
                line = r.ReadLine();
                if (line.Contains("{"))
                    bcount++;
                if (line.Contains("}"))
                    bcount--;

            }
        }

        // Load script
        public void loadScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String[] needed = new string[] 
            {
                "moveGear",
                "moveRudder",
                "doMurderPilot",
                "moveBayDoor",
                "moveWingFold",
                "moveFlap",
                "moveCockpitDoor",
                "moveAirBrake",
                "turretAngles",
                "onLoad",
                "moveAileron",
                "moveArrestorHook",
                "moveFan"
            };
            bool[] Checks = new bool[] 
            {
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false
            };
            String[] defaults = new string[]
            {
                "     public void moveGear(float f){}\r\n",
                "     public void moveRudder(float f){}\r\n",
                "     public void doMurderPilot(int i){}\r\n",
                "     public void moveBayDoor(float f){}\r\n",
                "     public void moveWingFold(float f){}\r\n",
                "     public void moveFlap(float f){}\r\n",
                "     public void moveCockpitDoor(float f){}\r\n",
                "     public void moveAirBrake(float f){}\r\n",
                "     public bool turretAngles(int i, float[]f){return false;}\r\n",
                "     public void onLoad(){}\r\n",
                "     public void moveAileron(float f){};\r\n",
                "     public void moveArrestorHook(float f){};\r\n",
                "     public void moveFan(float f){}\r\n"
            };

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".java";
            ofd.Filter = "Java source code (*.java)|*.java|C# source code (*.cs)|*.cs";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ScriptGenerator sg = new ScriptGenerator();
                if (ofd.FileName.EndsWith(".cs"))
                {
                    using (TextReader reader = File.OpenText(ofd.FileName))
                    {
                        String re = reader.ReadToEnd();
                        reader.Close();
                        sg.SetText(re);
                        sg.Compile();
                        sg.ShowDialog();
                        script = sg.script;
                        script.PrepareScript();
                    }
                }
                else
                {
                    using (TextReader reader = File.OpenText(ofd.FileName))
                    {
                        String re = "";
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("package"))
                            {
                                //line = "namespace IL2Modder.IL2\r\n{\r\n";
                                line = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Text;\r\nusing IL2Modder.IL2;\r\n";
                            }
                            else if (line.Contains("import"))
                            {
                                line = "";
                            }
                            line = line.Replace("abstract", "");
                            line = line.Replace("final", "");

                            if (line.Contains("extends"))
                            {
                                re = re + "    :AircraftInterface\r\n";
                                re = re + "{\r\n";
                                re = re + "  List<IL2Modder.IL2.AircraftActions>actions = new List<IL2Modder.IL2.AircraftActions>();\r\n\r\n";
                                re = re + "  private void SetAngles(String name, float t0, float t1, float t2)\r\n";
                                re = re + "  {\r\n";
                                re = re + "     IL2Modder.IL2.AircraftActions a = new IL2Modder.IL2.AircraftActions();\r\n";
                                re = re + "     a.name = name;\r\n";
                                re = re + "     a.y = t0;\r\n";
                                re = re + "     a.p = t1;\r\n";
                                re = re + "     a.r = t2;\r\n";
                                re = re + "     a.type = 0;\r\n";
                                re = re + "     actions.Add(a);\r\n";
                                re = re + "  }\r\n";
                                re = re + "  private void SetVisible(String name, bool value)\r\n";
                                re = re + "  {\r\n";
                                re = re + "     IL2Modder.IL2.AircraftActions a = new IL2Modder.IL2.AircraftActions();\r\n";
                                re = re + "     a.name = name;\r\n";
                                re = re + "     a.type = 1;\r\n";
                                re = re + "     a.visible = value;\r\n";
                                re = re + "     actions.Add(a);\r\n";
                                re = re + "  }\r\n";
                                re = re + "  private void SetLocate(String name, float t0, float t1, float t2)\r\n";
                                re = re + "  {\r\n";
                                re = re + "     IL2Modder.IL2.AircraftActions a = new IL2Modder.IL2.AircraftActions();\r\n";
                                re = re + "     a.name = name;\r\n";
                                re = re + "     a.type = 2;\r\n";
                                re = re + "     a.y = t0;\r\n";
                                re = re + "     a.p = t1;\r\n";
                                re = re + "     a.r = t2;\r\n";
                                re = re + "     actions.Add(a);\r\n";
                                re = re + "  }\r\n";
                                re = re + "  public void update()\r\n";
                                re = re + "  {\r\n";
                                re = re + "     actions.Clear();\r\n";
                                re = re + "  }\r\n";
                                re = re + "  public List<IL2Modder.IL2.AircraftActions> GetResults()\r\n";
                                re = re + "  {\r\n";
                                re = re + "     return actions;\r\n";
                                re = re + "  }\r\n";
                                line = "";
                                reader.ReadLine();
                            }
                            if (line.Contains("/*"))
                            {
                                int i = 0;
                                bool done = false;
                                while (!done)
                                {
                                    if ((line.ElementAt(i) == '*') && (line.ElementAt(i + 1) == '/'))
                                    {
                                        done = true;
                                    }

                                    i++;
                                    if (i == line.Length)
                                    {
                                        i = 0;
                                        line = reader.ReadLine();
                                    }

                                }

                                line = line.Remove(0, i + 1);
                            }
                            if (line.Contains("static"))
                            {
                                String test = line.Replace(" ", "");
                                if (test.Equals("static"))
                                {
                                    SkipFunction(reader);
                                    line = "";
                                }
                                else
                                {
                                    line = line.Replace("static", "");
                                }
                            }
                            if (line.Contains("[]"))
                            {
                                line = line.Replace("=", "= new float[]");
                            }
                            if (line.Contains("resetYPRmodifier();"))
                            {
                                line = "";
                            }
                            if (line.Contains("hierMesh().chunkSetLocate"))
                            {
                                line = "";
                            }
                            if (line.Contains("super."))
                            {
                                line = "";
                            }
                            if (line.Contains("Aircraft.xyz"))
                            {
                                line = "";
                            }
                            if (line.Contains("hitBone"))
                            {
                                SkipFunction(reader);
                                line = "";
                            }
                            if (line.Contains("msgExplosion"))
                            {
                                SkipFunction(reader);
                                line = "";
                            }
                            if (line.Contains("doKillPilot"))
                            {
                                SkipFunction(reader);
                                line = "";
                            }
                            if (line.Contains("doHitMeATank"))
                            {
                                SkipFunction(reader);
                                line = "";
                            }
                            if (line.Contains("rareAction"))
                            {
                                SkipFunction(reader);
                                line = "";
                            }
                            if (line.Contains("cutFM"))
                            {
                                SkipFunction(reader);
                                line = "";
                            }
                            if (line.Contains("update"))
                            {
                                SkipFunction(reader);
                                line = "";
                            }
                            if (line.Contains("gearDamageFX"))
                            {
                                SkipFunction(reader);
                                line = "";
                            }
                            if (line.Contains("Class "))
                                line = "";
                            if (line.Contains("Property.set"))
                                line = "";

                            for (int s = 0; s < needed.GetLength(0); s++)
                            {
                                if (line.Contains(needed[s]))
                                    Checks[s] = true;
                            }

                            line = line.Replace(".length", ".GetLength(0)");
                            line = line.Replace("protected", "public");
                            line = line.Replace("hierMesh().chunkSetAngles", "SetAngles");
                            line = line.Replace("hierMesh().chunkVisible", "SetVisible");
                            line = line.Replace("hierMesh()", "\"\"");
                            line = line.Replace("HierMesh ", "String ");
                            line = line.Replace("hiermesh.chunkSetAngles", "SetAngles");
                            line = line.Replace("paramHierMesh.chunkSetAngles", "SetAngles");
                            line = line.Replace("Math.max", "Math.Max");
                            line = line.Replace("Math.abs", "Math.Abs");
                            line = line.Replace("Math.min", "Math.Min");
                            line = line.Replace("Math.sin", "Math.Sin");
                            line = line.Replace("boolean", "Boolean");
                            if (!line.Equals(""))
                                re = re + line + "\r\n";


                        }
                        for (int s = 0; s < needed.GetLength(0); s++)
                        {
                            if (!Checks[s])
                            {
                                re = re + defaults[s];
                            }
                        }

                        sg.SetText(re);
                        sg.Compile();
                        sg.ShowDialog();
                        script = sg.script;
                        script.PrepareScript();
                    }
                }
            }
        }
        #endregion

        #region Project handling
        // new project
        public void loadProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "IL2 Projects (*.ilp)|*.ilp";
            ofd.CheckFileExists = false;
            ofd.DefaultExt = ".ilp";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                project = new Il2Project();
                project.Path = ofd.FileName;
            }
        }
        // load project
        public void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "IL2 Projects (*.ilp)|*.ilp";
            ofd.CheckFileExists = true;
            ofd.DefaultExt = ".ilp";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                project = new Il2Project();
                project.Path = ofd.FileName;
                project.DeSerialise(ofd.FileName);
                graphics = objectViewer1.GraphicsDevice;
                if (objectViewer1.my_effect == null)
                {
                    loading_dialog = new Loading();
                    loading_dialog.Show();
                    Application.DoEvents();
                    //LoadShader();
                    LoadPrebuiltContent();
                    loading_dialog.Close();
                }
                if (current_object != null)
                {
                    objectViewer1.mesh = null;
                    objectViewer1.Clear();
                    current_object = null;
                    Manager.Dispose();
                    treeView1.Nodes.Clear();
                }
                current_object = new HIM(project.HIM);
                TreeNode tn = new TreeNode("_ROOT_");
                tn.Tag = null;
                tn.Checked = true;
                treeView1.Nodes.Add(tn);
                foreach (Node n in current_object.root.children)
                {
                    TreeNode tn2 = new TreeNode(n.Name);
                    tn2.ContextMenuStrip = contextMenuStrip1;
                    tn2.Tag = n;
                    tn2.Checked = !n.Hidden;

                    tn.Nodes.Add(tn2);
                    addNodes(tn2, n);
                }
                objectViewer1.mesh = current_object;
                UpdateChecks(tn);

            }
        }
        // save project
        public void saveProjectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (project.Path != null)
            {
                project.Serialise(project.Path);
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "IL2 Projects (*.ilp)|*.ilp";
                sfd.DefaultExt = ".ilp";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    project.Path = sfd.FileName;
                    project.Serialise(sfd.FileName);

                }
            }
        }
        // save project as
        public void saveProjectAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "IL2 Projects (*.ilp)|*.ilp";
            sfd.DefaultExt = ".ilp";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                project.Path = sfd.FileName;
                project.Serialise(sfd.FileName);
            }
        }
        #endregion

        // toggle lighting
        public void toggleLightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Lighting)
                Lighting = false;
            else
                Lighting = true;
        }

        // screen shot
        public void screenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Screenshot = true;
        }

        // scan mesh
        public void scanMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".msh";
            ofd.Filter = "Meshes (*.msh)|*.msh";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MeshScanner ms = new MeshScanner();
                ms.Show();
                ms.Scan(ofd.FileName);
            }

        }

        // wing fold
        public void button2_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleWingFold();
        }

        // render mode
        public void toggleRenderModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleRenderMode();
        }

        // test tga viewer
        public void testTGALoaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestTga tt = new TestTga();
            tt.ShowDialog();
        }

        #region Camera
        private void objectViewer1_MouseDown_1(object sender, MouseEventArgs e)
        {
            dx = e.X;
            dy = e.Y;
            down = true;
            if (shoot)
            {
                objectViewer1.Shoot(e.X, e.Y);
            }
        }

        private void objectViewer1_MouseUp_1(object sender, MouseEventArgs e)
        {
            down = false;
            mouse_x = e.X;
            mouse_y = e.Y;
        }

        private void objectViewer1_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (down)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (shoot)
                    {
                        objectViewer1.Shoot(e.X, e.Y);
                    }
                    else
                    {
                        if (ObjectViewer.CockpitMode)
                        {
                            float pitch = (e.Y - dy) / 256.0f;
                            float yaw = (e.X - dx) / 256.0f;

                            Matrix m = Matrix.CreateFromYawPitchRoll(pitch * MathHelper.PiOver2, 0, yaw * MathHelper.Pi);
                            objectViewer1.forward = Vector3.Transform(Vector3.UnitX, m);
                        }
                        else
                        {
                            float pitch = (e.Y - dy) /mouse_sensitivity;
                            float yaw = (e.X - dx) / mouse_sensitivity;

                            Quaternion pq = Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch);
                            Quaternion yq = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
                            rotation = pq * rotation;
                            rotation = yq * rotation;
                            rotation.Normalize();

                            Matrix m = Matrix.CreateFromQuaternion(rotation);
                            objectViewer1.Eye = Vector3.Transform(eye, m);
                            objectViewer1.up = Vector3.Transform(Vector3.UnitY, m);
                        }
                    }
                }
                if (e.Button == MouseButtons.Right)
                {
                    if ((!shoot) && (!ObjectViewer.CockpitMode))
                    {
                        float yaw = (e.X - dx) / mouse_sensitivity;

                        Quaternion yq = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, yaw);

                        rotation = yq * rotation;
                        rotation.Normalize();

                        Matrix m = Matrix.CreateFromQuaternion(rotation);
                        objectViewer1.Eye = Vector3.Transform(eye, m);
                        objectViewer1.up = Vector3.Transform(Vector3.UnitY, m);
                    }
                    if (ObjectViewer.CockpitMode)
                    {
                        float dex = (e.X - dx) / mouse_sensitivity;
                        float dey = (e.Y - dy) / mouse_sensitivity;
                        objectViewer1.EyeY += dex;
                        objectViewer1.EyeZ += dey;
                    }
                }
                if (e.Button == MouseButtons.Middle)
                {
                    float pitch = (e.Y - dy) / mouse_sensitivity;
                    float yaw = (e.X - dx) / mouse_sensitivity;

                    Quaternion pq = Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch);
                    Quaternion yq = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
                    light_rotation = pq * light_rotation;
                    light_rotation = yq * light_rotation;
                    light_rotation.Normalize();

                    Matrix m = Matrix.CreateFromQuaternion(light_rotation);
                    objectViewer1.lightdirection = Vector3.Transform(new Vector3(0,0.7f,0.6f), m);
                }
            }
        }

        private void objectViewer1_MouseEnter_1(object sender, EventArgs e)
        {
            objectViewer1.Focus();
        }

        // cockpit view
        public void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (ObjectViewer.CockpitMode)
            {
                ObjectViewer.CockpitMode = false;

                objectViewer1.Eye = new Vector3(0, 0, -25);
                objectViewer1.up = new Vector3(0, 1, 0);
            }
            else
            {
                List<Vector3> c_hooks = new List<Vector3>();
                current_object.FindHook("CAMERA", "CAMERAAIM", ref c_hooks);
                if (c_hooks.Count > 0)
                    objectViewer1.Eye = c_hooks[0];
                else
                    objectViewer1.Eye = Vector3.Zero;

                objectViewer1.forward = Vector3.UnitX;
                objectViewer1.up = Vector3.UnitZ;
                ObjectViewer.CockpitMode = true;
            }
        }
        #endregion

        public void loadCockpitScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".java";
            ofd.Filter = "Java source code (*.java)|*.java|C# source code (*.cs)|*.cs";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ScriptGenerator sg = new ScriptGenerator();
                sg.cockpit = true;
                if (ofd.FileName.EndsWith(".cs"))
                {
                    using (TextReader reader = File.OpenText(ofd.FileName))
                    {
                        String re = reader.ReadToEnd();
                        reader.Close();
                        sg.SetText(re);
                        sg.Compile();
                        sg.ShowDialog();
                        cscript = sg.cscript;
                        cscript.PrepareScript();
                    }
                }
                else
                {
                }

            }
        }

        // toggle cockpit animation
        public void button16_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleCockpitAnimation();
        }

        // change sky box
        public void changeSkyboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            skybox_id++;
            if (skybox_id > 3)
                skybox_id = 0;

            objectViewer1.sky.rotation_x = 0;
            objectViewer1.sky.rotation_y = 0;
            objectViewer1.sky.rotation_z = 0;

            switch (skybox_id)
            {
                case 0:
                    objectViewer1.skybox = contentManager.Load<TextureCube>("Texture3D");
                    break;
                case 1:
                    objectViewer1.skybox = contentManager.Load<TextureCube>("Sunset");
                    objectViewer1.sky.rotation_x = 90;
                    break;
                case 2:
                    objectViewer1.skybox = contentManager.Load<TextureCube>("Grass");
                    objectViewer1.sky.rotation_x = -90;
                    //objectViewer1.sky.rotation_y = MathHelper.PiOver2;
                    break;
                case 3:
                    objectViewer1.skybox = contentManager.Load<TextureCube>("night");
                    break;
            }
        }

        bool CheckEnd(String line)
        {
            int i = 0;
            while (true)
            {
                String test = line.Substring(i, 1);
                if (test.Equals("}"))
                    return true;
                if (test.Equals("\""))
                    return false;
                i++;
            }
        }

        // load weapon loadouts
        public void loadWeaponLoadoutsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".java";
            ofd.Filter = "Java source code (*.java)|*.java";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                objectViewer1.WeaponLoadouts = new WeaponLoadoutArray();

                using (TextReader reader = File.OpenText(ofd.FileName))
                {
                    string line;
                    int mode = 0;
                    WeaponLoadout wl = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        switch (mode)
                        {
                            case 0:
                                {
                                    #region Read hook array
                                    if (line.Contains("weaponHooksRegister"))
                                    {

                                        int start = line.IndexOf('"');
                                        line = line.Remove(0, start + 1);
                                        bool done = false;
                                        List<String> hooks = new List<string>();
                                        String hook = "";

                                        while (!done)
                                        {
                                            // start by reading the first hook 
                                            String cur = line.Substring(0, 1);
                                            line = line.Remove(0, 1);
                                            if (cur == "\"")
                                            {
                                                // we have finished the current string
                                                hooks.Add(hook);
                                                hook = "";
                                                done = CheckEnd(line);
                                                if (!done)
                                                {
                                                    start = line.IndexOf('"');
                                                    line = line.Remove(0, start + 1);
                                                }
                                                else
                                                {
                                                    objectViewer1.WeaponLoadouts.Hooks = hooks.ToArray<String>();
                                                    hooks.Clear();
                                                    mode++;
                                                }

                                            }
                                            else
                                            {
                                                hook += cur;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                break;
                            case 1:
                                {
                                    #region Look for first weapon loadout
                                    if ((line.Contains("String")) && (line.Contains("=")))
                                    {
                                        int start = line.IndexOf('"');
                                        line = line.Remove(0, start + 1);
                                        start = line.IndexOf('"');
                                        wl = new WeaponLoadout(line.Substring(0, start));
                                        objectViewer1.WeaponLoadouts.Loads.Add(wl);
                                        mode++;
                                    }
                                    if ((line.Contains("weaponsRegister")) && (line.Contains("String[]")))
                                    {
                                        // alternate form 
                                        mode = 4;
                                        goto case 4;
                                    }
                                    #endregion
                                }
                                break;
                            case 2:
                                {
                                    #region Look for any active weapon slots or the end of the current one
                                    if ((line.Contains("Aircraft._WeaponSlot")) && (!line.Contains("Aircraft._WeaponSlot[")))
                                    {
                                        // we have a weapon slot
                                        int start = line.IndexOf('[');
                                        line = line.Remove(0, start + 1);
                                        start = line.IndexOf(']');
                                        String s = line.Substring(0, start);
                                        int slot = int.Parse(s);
                                        start = line.IndexOf('"');
                                        line = line.Remove(0, start + 1);
                                        start = line.IndexOf('"');
                                        s = line.Substring(0, start);
                                        if (!s.Contains("Null"))
                                            wl.Add(slot, s, 1);
                                    }
                                    if (line.Contains("Finger.Int"))
                                    {
                                        // we have finished this one
                                        mode++;
                                    }
                                    #endregion
                                }
                                break;
                            case 3:
                                {
                                    #region look for the next weapon loadout
                                    if (line.Contains("\";"))
                                    {
                                        int start = line.IndexOf('"');
                                        line = line.Remove(0, start + 1);
                                        start = line.IndexOf('"');
                                        String s = line.Substring(0, start);
                                        wl = new WeaponLoadout(s);
                                        objectViewer1.WeaponLoadouts.Loads.Add(wl);
                                        mode = 2;
                                    }
                                    if (line.Contains("catch"))
                                    {
                                        reader.Close();
                                        return;
                                    }
                                    #endregion
                                }
                                break;

                            case 4:
                                {
                                    List<String> tokens = StringTokenizer.Tokenise(line);

                                    if (tokens.Count > 0)
                                    {
                                        if (tokens[0] == "}")
                                        {
                                            reader.Close();
                                            return;
                                        }

                                        int i = 0;
                                        int j = 0;
                                        while (tokens[i] != "{")
                                        {
                                            i++;
                                        }
                                        wl = new WeaponLoadout(tokens[3]);
                                        i++;
                                        while (tokens[i] != "}")
                                        {
                                            if (tokens[i] == "null")
                                            {
                                            }
                                            else
                                            {
                                                String[] bitz = tokens[i].Split(' ');
                                                if (bitz.Length > 1)
                                                {
                                                    wl.Add(j, bitz[0], int.Parse(bitz[1]));
                                                }
                                                else
                                                {
                                                    wl.Add(j, bitz[0], 1);
                                                }
                                            }
                                            j++;
                                            i++;
                                        }
                                        objectViewer1.WeaponLoadouts.Loads.Add(wl);
                                    }


                                }
                                break;
                        }

                    }
                    reader.Close();
                }
            }
        }

        //choose weapon loadout
        public void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (objectViewer1.WeaponLoadouts == null)
                return;

            WeaponLoadList wll = new WeaponLoadList();
            wll.viewer = objectViewer1;
            wll.Setup(objectViewer1.WeaponLoadouts, objectViewer1.WeaponLoadouts.Hooks);
            if (wll.ShowDialog() == DialogResult.OK)
            {
                objectViewer1.PrepareLoadout();
            }

        }

        // add weapon 
        public void addWeaponToDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Mesh files (*.msh)|*.msh";
            ofd.DefaultExt = "*.msh";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NameEntry ne = new NameEntry(ofd.FileName);
                ne.ShowDialog();
                try
                {
                    objectViewer1.Weapons.Add(Form1.comms, ofd.FileName);
                }
                catch (Exception) { }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            String file = Application.StartupPath + "\\weapons.txt";
            using (TextWriter writer = File.CreateText(file))
            {
                Dictionary<string, string>.KeyCollection keys = objectViewer1.Weapons.Keys;
                foreach (String s in keys)
                {
                    writer.WriteLine(String.Format("{0} {1}", s, objectViewer1.Weapons[s]));
                }
                writer.Close();
            }
            if (Form1.TexturesDirectory != null)
            {
                if (this.Location.X > 0)
                {
                    file = Form1.TexturesDirectory + "\\ov.layout";
                    using (TextWriter writer = File.CreateText(file))
                    {
                        writer.WriteLine(String.Format("X {0}", this.Location.X));
                        writer.WriteLine(String.Format("Y {0}", this.Location.Y));
                        writer.WriteLine(String.Format("W {0}", this.Width));
                        writer.WriteLine(String.Format("H {0}", this.Height));
                        writer.Close();
                    }
                }
                using (BinaryWriter b = new BinaryWriter(File.Open("settings.bin", FileMode.Create)))
                {
                    b.Write(TexturesDirectory);
                    b.Write(ScreenShotDirectory);
                    b.Write(FBXDirectory);
                    b.Close();
                }
            }
        }

        // load a 3ds file
        public void load3DSFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objectViewer1.my_effect == null)
            {
                loading_dialog = new Loading();
                loading_dialog.Show();
                Application.DoEvents();
                LoadPrebuiltContent();
                loading_dialog.Close();
            }
            if (current_object != null)
            {
                objectViewer1.mesh = null;
                objectViewer1.Clear();
                current_object = null;
                Manager.Dispose();
                treeView1.Nodes.Clear();
            }
            OpenFileDialog ofd = new OpenFileDialog();
            missing_textures.Clear();
            ofd.DefaultExt = "*.3ds";
            ofd.Filter = "3DS files (*.3ds)|*.3ds";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                graphics = objectViewer1.GraphicsDevice;
                ThreeDSFile ds = new ThreeDSFile(ofd.FileName);
                TreeNode tn = new TreeNode("_ROOT_");
                tn.Tag = null;
                tn.Checked = true;
                treeView1.Nodes.Add(tn);
                HIM h = new HIM();

                for (int i = 0; i < ds.entities.Count; i++)
                {
                    TreeNode tn2 = new TreeNode(ds.names[i]);
                    tn2.Checked = true;
                    tn.Nodes.Add(tn2);
                    MeshNode mn = new MeshNode(ds.names[i]);
                    tn2.Tag = mn;
                    mn.mesh.Initialise(ds.entities[i], Path.GetDirectoryName(ofd.FileName) + "\\");
                    h.root.children.Add(mn);
                }
                objectViewer1.mesh = h;
            }

        }

        // replace a node
        private void replaceNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node tn = (Node)treeView1.SelectedNode.Tag;
                if (tn is MeshNode)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.DefaultExt = "*.msh";
                    ofd.Filter = "Mesh files (*.msh)|*.msh|3DS files (*.3ds)|*.3ds";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        MeshNode mn = (MeshNode)tn;
                        if (ofd.FileName.EndsWith("msh", StringComparison.OrdinalIgnoreCase))
                            mn.mesh = new Mesh(ofd.FileName);
                        else
                        {
                            ThreeDSFile ds = new ThreeDSFile(ofd.FileName);
                            if (ds.entities.Count > 1)
                            {
                                MessageBox.Show("Dude this 3ds file has more than one sub mesh, I can't handle that", "User error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                Mesh m = new Mesh();
                                m.Initialise(ds.entities[0], Path.GetDirectoryName(ofd.FileName));
                                mn.mesh = m;
                            }
                        }
                    }
                }
            }
        }

        // save all
        public void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "HIM files (*.him)|*.him";
            sfd.DefaultExt = "*.him";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                current_object.Serialise(sfd.FileName);
                Manager.Serialise(Path.GetDirectoryName(sfd.FileName));
            }
        }

        public void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // save him
        public void saveHimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "HIM files (*.him)|*.him";
            sfd.DefaultExt = "*.him";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                current_object.SerialiseHim(sfd.FileName);

            }
        }

        // generate skin texture
        public void generateSkinTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateSkinTexture gst = new GenerateSkinTexture(current_object);
            if (gst.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }

        }

        // new him
        public void newHimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objectViewer1.my_effect == null)
            {
                loading_dialog = new Loading();
                loading_dialog.Show();
                Application.DoEvents();
                //LoadShader();
                LoadPrebuiltContent();
                loading_dialog.Close();
                Form1.graphics = objectViewer1.GraphicsDevice;
            }
            if (current_object != null)
            {
                objectViewer1.mesh = null;
                objectViewer1.Clear();
                current_object = null;
                Manager.Dispose();
                treeView1.Nodes.Clear();
            }
            TreeNode tn = new TreeNode("_ROOT_");
            tn.Tag = null;
            tn.Checked = true;
            tn.ContextMenuStrip = contextMenuStrip1;
            treeView1.Nodes.Add(tn);
            current_object = new HIM();
            objectViewer1.mesh = current_object;
        }

        // add mesh node
        private void addMshNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = "*.msh";
                ofd.Filter = "Meshes (*.msh)|*.msh";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    String name = Path.GetFileNameWithoutExtension(ofd.FileName);

                    Node n = new Node();
                    n.Name = name;
                    n.Hidden = false;
                    n.world = Matrix.Identity;


                    TreeNode tn = new TreeNode(name);
                    tn.Checked = true;
                    tn.ContextMenuStrip = contextMenuStrip1;
                    tn.Tag = n;
                    treeView1.SelectedNode.Nodes.Add(tn);

                    Node parent = (Node)treeView1.SelectedNode.Tag;
                    if (parent == null)
                    {
                        current_object.root.children.Add(n);
                    }
                    else
                    {
                        parent.children.Add(n);
                    }
                    MeshNode mn = new MeshNode(name, Path.GetDirectoryName(ofd.FileName));
                    n.children.Add(mn);

                    TreeNode tn2 = new TreeNode("Mesh_" + name);
                    tn2.Checked = true;
                    tn2.ContextMenuStrip = contextMenuStrip1;
                    tn.Nodes.Add(tn2);
                    tn2.Tag = mn;

                    treeView1.Invalidate();
                    Application.DoEvents();
                }
            }
        }

        // reset visibility
        public void toolStripButton6_Click(object sender, EventArgs e)
        {
            loading = true;
            current_object.ResetHidden();
            ResetH(treeView1.Nodes[0]);
            UpdateChecks(treeView1.Nodes[0]);
            loading = false;

        }

        private void ResetH(TreeNode n)
        {
            Node n2 = (Node)n.Tag;
            if (n2 != null)
                n.Checked = !n2.Hidden;

            foreach (TreeNode n3 in n.Nodes)
            {
                ResetH(n3);
            }
        }

        // toggle hooks
        public void toolStripButton7_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleHooks();
        }

        // click event
        private void objectViewer1_Click(object sender, EventArgs e)
        {

        }

        private void objectViewer1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ObjectViewer.HookDisplay)
            {
                ObjectViewer.ray = objectViewer1.CreateRay(e.X, e.Y);
                Hook n = objectViewer1.mesh.RaytraceHook((Ray)ObjectViewer.ray);
                if (n != null)
                {
                    HookEdit he = new HookEdit();
                    he.SetHook(n);
                    he.ShowDialog();
                }
            }
        }

        public void checkBox7_CheckStateChanged(object sender, EventArgs e)
        {
            objectViewer1.Night(((CheckBox)sender).Checked);
        }

        // load AC
        public void loadACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objectViewer1.my_effect == null)
            {
                loading_dialog = new Loading();
                loading_dialog.Show();
                Application.DoEvents();
                LoadPrebuiltContent();
                loading_dialog.Close();
                Form1.graphics = objectViewer1.GraphicsDevice;
            }
            if (current_object != null)
            {
                objectViewer1.mesh = null;
                objectViewer1.Clear();
                current_object = null;
                Manager.Dispose();
                treeView1.Nodes.Clear();
            }
            OpenFileDialog ofd = new OpenFileDialog();
            missing_textures.Clear();

            ofd.DefaultExt = "*.ac";
            ofd.Filter = "AC3D files (*.ac)|*.ac";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TreeNode tn = new TreeNode("_ROOT_");
                tn.Tag = null;
                tn.Checked = true;
                treeView1.Nodes.Add(tn);
                current_object = new HIM();
                current_object.LoadAC(ofd.FileName, Path.GetDirectoryName(ofd.FileName));

                foreach (Node n in current_object.root.children)
                {
                    TreeNode tn2 = new TreeNode(n.Name);
                    tn2.ContextMenuStrip = contextMenuStrip1;
                    tn2.Tag = n;
                    tn2.Checked = !n.Hidden;

                    tn.Nodes.Add(tn2);
                    addNodes(tn2, n);
                }

                objectViewer1.mesh = current_object;
                UpdateChecks(tn);

            }
        }

        // toggle air brakes
        public void button17_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleAirBrakes();
        }

        // turrets
        public void button18_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleTurrets();
        }

        // set screen shot directory
        public void setScreenshotDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Point the app at the 'Textures' directory in IL2";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                buttons.screenshot = fbd.SelectedPath;
                ScreenShotDirectory = fbd.SelectedPath;
                using (BinaryWriter b = new BinaryWriter(File.Open("settings.bin", FileMode.Create)))
                {
                    b.Write(TexturesDirectory);
                    b.Write(ScreenShotDirectory);
                    b.Write(FBXDirectory);
                    b.Close();
                }
            }
        }

        // collision mesh only
        public void toolStripButton8_Click(object sender, EventArgs e)
        {
            objectViewer1.ToggleColOnly();
        }

        public void RegisterHooks(String[] Hooks)
        {
            objectViewer1.WeaponLoadouts = new WeaponLoadoutArray();
            objectViewer1.WeaponLoadouts.Hooks = Hooks;
        }

        public void weaponsRegister(String name, String[] weapons)
        {
            WeaponLoadout wl = new WeaponLoadout(name);
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    string[] parts = weapons[i].Split(' ');
                    int cnt = 1;
                    if (parts.Length > 1)
                        cnt = int.Parse(parts[1]);
                    wl.Add(i, parts[0], cnt);
                }
            }
            objectViewer1.WeaponLoadouts.Loads.Add(wl);
        }

        public void RepairPlane()
        {
            objectViewer1.mesh.ResetDamage();
        }

        public void BakeAmbientOcclusion()
        {
            AOBuilder aob = new AOBuilder(objectViewer1, Manager);
            aob.Show();
        }

        public void DrawNormals(bool state)
        {
            objectViewer1.ToggleNormalDrawing(state);
        }

        public void RegenerateNormals()
        {
            objectViewer1.RegenerateNormals();
        }

        public void ToggleCulling()
        {
            objectViewer1.ToggleCulling();
        }

        public void SwapTriangles()
        {
            objectViewer1.SwapTriangles();
        }

        public void Ocean(bool on)
        {
            objectViewer1.SetOcean(on);
        }

        public void Fog(bool on)
        {
            objectViewer1.Fog(on);
        }

        #region heirMesh support
        public bool checkChunk(String name)
        {
            return objectViewer1.Exists(name);
        }

        public void hideTree(String name)
        {
            objectViewer1.hideTree(name);
        }

        public void chunkVisible(String name, bool visible)
        {
            objectViewer1.chunkVisible(name, visible);
        }

        public void setThrottle(float value)
        {
            Throttle = value;
        }
        #endregion

        private void rotate90ZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node n = (Node)treeView1.SelectedNode.Tag;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.Rotate90Z();
                }
            }
        }

        public void ShipMode(bool state)
        {
            objectViewer1.ShipMode = state;
        }

        public void SaveAsDAE()
        {
            objectViewer1.SaveDAE();
        }

        public void SaveAsOGRE()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                objectViewer1.SaveAsOGRE(fbd.SelectedPath);
            }
        }

        public void AfterburnerToggle(bool state)
        {
            if (state)
                objectViewer1.EnableAfterburner();
            else
                objectViewer1.DisableAfterburner();
        }

        public void BoosterToggle(bool state)
        {
            if (state)
                objectViewer1.EnableBoosters();
            else
                objectViewer1.DisableBoosters();
        }

        public void ArrestorToggle()
        {
            objectViewer1.ToggleArrestor();
        }

        public void SaveToFox1()
        {
            String directory = objectViewer1.SaveToFox1();
            if (directory != "Cancel")
                Manager.Serialise(directory);
        }

        

        public void SaveAsObj()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                objectViewer1.SaveAsObj(fbd.SelectedPath);
            }
        }
        public void SaveAsObj2()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                objectViewer1.SaveAsObj2(fbd.SelectedPath,current_object.Name);
                Manager.Serialise(fbd.SelectedPath, current_object.Name);
            }
        }

        private void extractEulersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node n = (Node)treeView1.SelectedNode.Tag;


                Vector3 scale;
                Vector3 trans;
                Quaternion q;
                n.world.Decompose(out scale, out q, out trans);
                Console.WriteLine("Scale " + scale.ToString());
                Console.WriteLine("Trans " + trans.ToString());
                var yaw = Math.Atan2(2.0 * (q.Y * q.Z + q.W * q.X), q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);
                var pitch = Math.Asin(-2.0 * (q.X * q.Z - q.W * q.Y));
                var roll = Math.Atan2(2.0 * (q.X * q.Y + q.W * q.Z), q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);
                Console.WriteLine("Yaw " + MathHelper.ToDegrees((float)yaw).ToString());
                Console.WriteLine("Pitch " + MathHelper.ToDegrees((float)pitch).ToString());
                Console.WriteLine("Roll " + MathHelper.ToDegrees((float)roll).ToString());


            }
        }

        private void flipNormalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node n = (Node)treeView1.SelectedNode.Tag;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.FlipNormals();
                }
            }
        }

        public void AdjustLighting()
        {
            objectViewer1.AdjustLighting();
        }

        public void EnableBumpMapping(bool val)
        {
            ObjectViewer.BumpMapping = val;
        }

        public void SetBumpLevel(float val)
        {
            objectViewer1.SetBumpStrength(val);
        }

        private void mapToVariableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node n = (Node)treeView1.SelectedNode.Tag;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    CockpitVariableMapping cvm = new CockpitVariableMapping();
                    if (cvm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {

                    }
                }
            }
        }

        public SaveFBX globalSaveAsFBXDialog;
        public String FBXMode;

        public void SaveAsFBX()
        {
            globalSaveAsFBXDialog = new SaveFBX();
            globalSaveAsFBXDialog.ShowDialog();
        }

        public void SaveToUE4()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                objectViewer1.SaveAsUE4(fbd.SelectedPath);
                //Manager.Serialise(fbd.SelectedPath);
            }
        }

        public void SaveToUE5()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                objectViewer1.SaveAsUE5(fbd.SelectedPath);
                Manager.Serialise(fbd.SelectedPath);
            }
        }

        public void OnSaveAsFBX(string filename, string directory, bool binarymode)
        {
            fbx_nodes.Clear();
            fbx_meshes.Clear();
            Fbx_node_count = 0;

            ExportShadows = globalSaveAsFBXDialog.ExportShadows;
            ExportBaseMesh = globalSaveAsFBXDialog.ExportBaseMesh;
            ExportLODs = globalSaveAsFBXDialog.ExportLODs;
            ExportLODShadows = globalSaveAsFBXDialog.ExportLODShadows;
            ExportHooks = globalSaveAsFBXDialog.ExportHooks;
            ExportCollisionMeshes = globalSaveAsFBXDialog.ExportCollisionMeshes;

            if (binarymode)
                FBXMode = "binary";
            else
                FBXMode = "ascii";

            Form1.Instance.sdkManager = FbxSdkManager.Create();
            if (Form1.Instance.sdkManager == null)
            {
                Console.Write("Unable to create the FBX SDK manager");
                Form1.Instance.sdkManager = null;
                Form1.Instance.scene = null;
                return;
            }
            // Create the scene object. The scene will hold the data
            // in the file to be imported.           
            Form1.Instance.scene = FbxScene.Create(Form1.Instance.sdkManager, "");
            if (Form1.Instance.scene == null)
            {
                Console.Write("Unable to create the Scene");
                Form1.Instance.sdkManager = null;
                Form1.Instance.scene = null;
                return;
            }

            // Create an exporter.
            Form1.Instance.exporter = Skill.FbxSDK.IO.FbxExporter.Create(Form1.Instance.sdkManager, "");

            
            // Initialize the exporter by providing a filename.
            if (Form1.Instance.exporter.Initialize(filename) == false)
            {
                Console.Write("Call to FbxExporter.Initialize() failed.");
                Console.Write(string.Format("Error returned: {0}", Form1.Instance.exporter.LastErrorString));
                return;
            }
            int writeFileFormat = -1;
            Version version = Skill.FbxSDK.IO.FbxIO.CurrentVersion;
            if (writeFileFormat < 0 || writeFileFormat >= Form1.Instance.sdkManager.IOPluginRegistry.WriterFormatCount)
            {
                // Write in fall back format if pEmbedMedia is true
                writeFileFormat = Form1.Instance.sdkManager.IOPluginRegistry.NativeWriterFormat;
                {
                    //Try to export in ASCII if possible
                    int formatIndex, formatCount = Form1.Instance.sdkManager.IOPluginRegistry.WriterFormatCount;

                    for (formatIndex = 0; formatIndex < formatCount; formatIndex++)
                    {
                        if (Form1.Instance.sdkManager.IOPluginRegistry.WriterIsFBX(formatIndex))
                        {
                            string desc = Form1.Instance.sdkManager.IOPluginRegistry.GetWriterFormatDescription(formatIndex);

                            if (desc.Contains(Form1.Instance.FBXMode))
                            {
                                writeFileFormat = formatIndex;
                                break;
                            }
                        }
                    }
                }
            }
            // Set the file format
            Form1.Instance.exporter.FileFormat = writeFileFormat;
            Form1.Instance.exportOptions = Skill.FbxSDK.IO.FbxStreamOptionsFbxWriter.Create(Form1.Instance.sdkManager, "");

            if (Form1.Instance.sdkManager.IOPluginRegistry.WriterIsFBX(writeFileFormat))
            {
                // Export options determine what kind of data is to be imported.
                // The default (except for the option eEXPORT_TEXTURE_AS_EMBEDDED)
                // is true, but here we set the options explictly.
                Form1.Instance.exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.MATERIAL, true);
                Form1.Instance.exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.TEXTURE, true);
                Form1.Instance.exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.EMBEDDED, false);
                Form1.Instance.exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.LINK, true);
                Form1.Instance.exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.SHAPE, false);
                Form1.Instance.exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.GOBO, false);
                Form1.Instance.exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.ANIMATION, false);
                Form1.Instance.exportOptions.SetOption(Skill.FbxSDK.IO.FbxStreamOptionsFbx.GLOBAL_SETTINGS, true);
            }
            
            objectViewer1.SaveAsFBX(directory);
            Manager.Serialise(directory);
        }

        // add fox 1 animation to mesh part
        private void addAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node n = (Node)treeView1.SelectedNode.Tag;
                FoxOneAnimationEditor fae = new FoxOneAnimationEditor();
                fae.SetTarget(n.Name);
                fae.Show();
            }
        }

        // focus camera on a node
        private void focusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node n = (Node)treeView1.SelectedNode.Tag;
                if (n != null)
                {
                    Vector3 target = objectViewer1.mesh.GetLocation(n.Name);
                    objectViewer1.SetTarget(target);
                }
            }
        }

        /// <summary>
        /// Copies all the textures from 3d0/Aircraft/TEXTURES to the GuruEngine CommonTextures directory.
        /// This is really only to save me a load of time and shouldn't be needed by anyone else, but what the hey.
        /// </summary>
        public void CopyCommonTexturesToGuruEngine()
        {
            String destdir = @"E:\Research\XNA\MultiThreadedRenderer\MultiThreadedRenderer\Content\HIM\CommonTextures\";
            DirectoryInfo d = new DirectoryInfo(TexturesDirectory);
            FileInfo[] Files = d.GetFiles("*.*"); 
            foreach (FileInfo file in Files)
            {
                String src = Path.Combine(TexturesDirectory, file.Name);
                String fileName = Path.GetFileNameWithoutExtension(file.Name);
                fileName += ".png";
                String dest = Path.Combine(destdir, fileName);
                Manager.SaveTexture(src, dest);
            }
        }

        public void CopyPaintSchemesToGuruEngine()
        {
            String destdir = @"E:\Research\XNA\MultiThreadedRenderer\MultiThreadedRenderer\Content\HIM\PaintSchemes\";
            String srcdir = @"E:\Aircraft\PaintSchemes";

            SavePaintSchemes(srcdir, destdir);

        }

        void SavePaintSchemes(String src, String dest)
        {
            string[] subdirs = Directory.GetDirectories(src);
            for (int i=0; i<subdirs.Length; i++)
            {
                string nextsrc = subdirs[i];
                string nextdest = Path.Combine(dest, Path.GetFileName(subdirs[i]));
                if (!Directory.Exists(nextdest))
                {
                    Directory.CreateDirectory(nextdest);
                }
                SavePaintSchemes(nextsrc, nextdest);

            }
            string[] files = Directory.GetFiles(src, "*.tga");
            for (int i=0; i<files.Length; i++)
            {
                string srcfile = Path.Combine(src, files[i]);
                string destfile = Path.Combine(dest, Path.GetFileNameWithoutExtension(files[i]));
                destfile += ".png";
                Manager.SaveTexture(srcfile, destfile);
            }


        }

        /// <summary>
        /// Add collision mesh context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addCollisionMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node n = (Node)treeView1.SelectedNode.Tag;
                if (n != null)
                {
                    if (n is MeshNode)
                    {
                        objectViewer1.AddCollisionMesh((MeshNode)n);
                    }
                    else
                    {
                        MessageBox.Show("Please select a mesh node i.e. one whose name starts with 'Mesh_'", "User error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
        }

        /// <summary>
        /// Split off face group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitOffFacegroupToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (treeView1.SelectedNode != null)
            {
                Node n = (Node)treeView1.SelectedNode.Tag;
                if (n != null)
                {
                    if (n is MeshNode)
                    {
                        MeshNode mn = (MeshNode)n;
                        FaceGroupSplitter fgs = new FaceGroupSplitter(mn.mesh);
                        if (fgs.ShowDialog() == DialogResult.OK)
                        {

                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a mesh node i.e. one whose name starts with 'Mesh_'", "User error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
           
        }

        public void SaveCollisionMeshes()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = @"E:\Research\XNA\MultiThreadedRenderer\MultiThreadedRenderer\Content\HIM\";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                objectViewer1.ExportCollisionMeshes(fbd.SelectedPath);
            }
        }

        public void ExportHooksToText()
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.DefaultExt = "*.txt";
            ofd.Filter = "Text files (*.txt)|*.txt";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (TextWriter writer = File.CreateText(ofd.FileName))
                {
                    objectViewer1.ExportHooks(writer);
                }
            }
        }


    }
}