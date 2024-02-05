using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;

using IL2Modder.AC3D;
using IL2Modder.Fox1Animators;

using Skill.FbxSDK;

namespace IL2Modder.IL2
{
    enum LoadingMode
    {
        Start,
        Root,
        Node,
        LoadingModes
    };
    public class HIM
    {
        public RootNode root;
        LoadingMode mode;
        List<AircraftActions> actions = new List<AircraftActions>();
        List<TurretLimits> turret_limits = new List<TurretLimits>();
        Random rand = new Random();

        public List<Animator> animators = new List<Animator>();
        String Name;
        public int nProps;
        public int nRudders;
        public int nFlaps;

        public HIM()
        {
            nProps = 0;
            nRudders = 0;
            nFlaps = 0;
            root = new RootNode();
            root.VisibiltySphere = 10;
            CollisionNode cn = new CollisionNode("CollisionObject sphere 12.0 0.0 0.0 0.0");
            root.Colliders.Add(cn);
            for (int i = 0; i < 8; i++)
            {
                turret_limits.Add(new TurretLimits());
            }
        }

        public HIM(String filename)
        {
            int line_number = 0;
            int repeat_count = 0;
            string line = "";

            mode = LoadingMode.Start;
            Node node = null;
            String dir = Path.GetDirectoryName(filename);
            int lp = dir.LastIndexOf('\\');
            Name = dir.Substring(lp + 1);

            for (int i = 0; i < 8; i++)
            {
                turret_limits.Add(new TurretLimits());
            }
            using (TextReader reader = File.OpenText(filename))
            {

                try
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        line_number++;

                        bool used = false;
                        repeat_count = 0;
                        if ((line.StartsWith("//")) || (line.StartsWith(";")) || (line.Length < 1) || (line.StartsWith("#")))
                        {
                            used = true;
                        }
                        while (!used)
                        {
                            repeat_count++;
                            if (repeat_count == 20)
                            {
                                throw new Exception("Infinite loop detected in HIM file");
                            }
                            string temp = line.Replace('\t', ' ');
                            temp = temp.Replace('/', ' ');
                            temp = filter(temp);

                            string[] parts = temp.Split(' ');
                            switch (mode)
                            {
                                case LoadingMode.Start:
                                    if (parts[0].Equals("[_ROOT_]"))
                                    {
                                        root = new RootNode();
                                        mode = LoadingMode.Root;
                                        used = true;
                                    }
                                    else
                                    {
                                        if (parts[0].StartsWith("["))
                                        {
                                            node = new Node(parts[0]);
                                            mode = LoadingMode.Node;
                                            used = true;
                                        }
                                    }
                                    break;

                                #region Root node
                                case LoadingMode.Root:
                                    if (parts[0].Equals("VisibilitySphere"))
                                    {
                                        root.VisibiltySphere = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                        used = true;
                                    }
                                    if (parts[0].Equals("CollisionObject"))
                                    {
                                        CollisionNode cn = new CollisionNode(line);
                                        root.Colliders.Add(cn);
                                        used = true;
                                    }
                                    if (parts[0].StartsWith("["))
                                    {
                                        mode = LoadingMode.Start;
                                    }
                                    break;
                                #endregion

                                #region Node mode
                                case LoadingMode.Node:
                                    if (parts[0].Equals("Mesh"))
                                    {
                                        MeshNode mn = new MeshNode(parts[1], dir);
                                        node.children.Add(mn);
                                        used = true;
                                        if ((parts[1].StartsWith("Prop")) && (!parts[1].Contains("Rot")) && (parts[1].EndsWith("D0")))
                                        {
                                            nProps++;
                                        }
                                        if ((parts[1].StartsWith("Rudder")) && (parts[1].EndsWith("D0")))
                                        {
                                            nRudders++;
                                        }
                                        if ((parts[1].StartsWith("Flap")) && (parts[1].EndsWith("D0")))
                                        {
                                            nFlaps++;
                                        }
                                    }
                                    if (parts[0].Equals("Parent"))
                                    {
                                        Node dad = FindNode(parts[1]);
                                        if (dad == null)
                                        {
                                            throw new Exception("Cannot find parent " + parts[1]);
                                        }
                                        dad.children.Add(node);
                                        node.Parent = parts[1];
                                        node.Hidden = dad.Hidden;
                                        used = true;
                                    }
                                    if (parts[0].Equals("Attaching"))
                                    {
                                        node.world.M11 = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M12 = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M13 = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M21 = float.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M22 = float.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M23 = float.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M31 = float.Parse(parts[7], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M32 = float.Parse(parts[8], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M33 = float.Parse(parts[9], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M41 = float.Parse(parts[10], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M42 = float.Parse(parts[11], System.Globalization.CultureInfo.InvariantCulture);
                                        node.world.M43 = float.Parse(parts[12], System.Globalization.CultureInfo.InvariantCulture);
                                        node.base_matrix = node.world;
                                        used = true;
                                    }
                                    if (parts[0].Equals("Hidden", StringComparison.OrdinalIgnoreCase))
                                    {
                                        node.Hidden = true;
                                        node.originalHidden = true;
                                        if (node.children.Count == 1)
                                        {
                                            if (node.children[0] is MeshNode)
                                            {
                                                node.children[0].Hidden = true;

                                            }
                                        }
                                        used = true;
                                    }
                                    if (parts[0].Equals("invisible", StringComparison.OrdinalIgnoreCase))
                                    {
                                        node.Hidden = true;
                                        node.originalHidden = true;
                                        used = true;
                                    }
                                    if (parts[0].Equals("Separable", StringComparison.OrdinalIgnoreCase))
                                    {
                                        node.Seperable = true;
                                        used = true;
                                    }
                                    if (parts[0].Equals("CollisionObject", StringComparison.OrdinalIgnoreCase))
                                    {
                                        CollisionNode cn = new CollisionNode(line);
                                        node.Colliders.Add(cn);
                                        used = true;
                                    }
                                    if (parts[0].StartsWith("["))
                                    {
                                        mode = LoadingMode.Start;
                                    }
                                    if (parts[0].Length == 0)
                                        used = true;
                                    break;
                                    #endregion

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    HIMErrorHandler err = new HIMErrorHandler();
                    err.SetLineNumber(line_number);
                    err.SetErrorText(e.ToString());
                    err.SetErrorString(line);
                    err.SetFilename(filename);

                    err.ShowDialog();
                }
            }
        }

        #region Search methods

        public Node FindNode(String name)
        {
            if (name.Equals("_ROOT_", StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }
            foreach (Node n in root.children)
            {
                if (n.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return n;
                Node n3 = FindNode(n, name);
                if (n3 != null)
                    return n3;
            }
            return null;
        }

        public Hook FindHook(String name)
        {
            Hook h;
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode m = (MeshNode)n;
                    h = m.FindHook(name);
                    if (h != null)
                    {
                        return h;
                    }

                }
                h = FindHook(n, name);
                if (h != null)
                    return h;
            }
            return null;
        }

        public Hook FindHook(Node n, String name)
        {
            Hook h;
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode m = (MeshNode)n2;
                    h = m.FindHook(name);
                    if (h != null)
                    {
                        return h;
                    }

                }
                h = FindHook(n2, name);
                if (h != null)
                    return h;
            }
            return null;
        }

        public Node FindNode(Node n, String Name)
        {
            foreach (Node n2 in n.children)
            {
                if (n2.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
                    return n2;
                Node n3 = FindNode(n2, Name);
                if (n3 != null)
                    return n3;
            }
            return null;
        }

        public void FindHook(String name, ref List<Vector3> hooks)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode m = (MeshNode)n;
                    m.FindHook(name, ref hooks, n.world);
                }
                FindHook(n, name, ref hooks, n.world);
            }
        }

        public void FindHook(String name, ref List<Vector3> hooks, ref List<Vector3> directions)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode m = (MeshNode)n;
                    m.FindHook(name, ref hooks, ref directions, n.world);
                }
                FindHook(n, name, ref hooks, ref directions, n.world);
            }
        }

        public void FindHook(String name, ref List<Vector3> hooks, ref List<Vector3> directions, Node host)
        {
            foreach (Node n in root.children)
            {
                if (n == host)
                {
                    if (n is MeshNode)
                    {
                        MeshNode m = (MeshNode)n;
                        m.FindHook(name, ref hooks, ref directions, n.world);
                    }
                }
                FindHook(n, name, ref hooks, ref directions, n.world, host);
            }
        }

        private void FindHook(Node n, string name, ref List<Vector3> hooks, ref List<Vector3> directions, Matrix pos, Node host)
        {
            foreach (Node n2 in n.children)
            {
                if (n == host)
                {
                    if (n2 is MeshNode)
                    {
                        MeshNode m = (MeshNode)n2;
                        m.FindHook(name, ref hooks, ref directions, m.world * pos);
                    }
                }
                FindHook(n2, name, ref hooks, ref directions, n2.world * pos, host);
            }
        }

        public Hook RaytraceHook(Ray ray)
        {
            Hook h;
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode m = (MeshNode)n;
                    h = m.RaytraceHook(ray);
                    if (h != null)
                    {
                        return h;
                    }

                }
                h = RaytraceHook(n, ray);
                if (h != null)
                    return h;
            }
            return null;
        }

        public Hook RaytraceHook(Node n2, Ray ray)
        {
            Hook h;
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode m = (MeshNode)n;
                    h = m.RaytraceHook(ray);
                    if (h != null)
                    {
                        return h;
                    }

                }
                h = RaytraceHook(n, ray);
                if (h != null)
                    return h;
            }
            return null;
        }

        private void FindHook(Node n, string name, ref List<Vector3> hooks, ref List<Vector3> directions, Matrix pos)
        {
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode m = (MeshNode)n2;
                    m.FindHook(name, ref hooks, ref directions, m.world * pos);
                }
                FindHook(n2, name, ref hooks, ref directions, n2.world * pos);
            }
        }

        private void FindHook(Node n, string name, ref List<Vector3> hooks, Matrix pos)
        {
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode m = (MeshNode)n2;
                    m.FindHook(name, ref hooks, m.world * pos);
                }
                FindHook(n2, name, ref hooks, n2.world * pos);
            }
        }

        public void FindHook(String name, String ignore, ref List<Vector3> hooks)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode m = (MeshNode)n;
                    m.FindHook(name, ignore, ref hooks, n.world);
                }
                FindHook(n, name, ignore, ref hooks, n.world);
            }
        }

        private void FindHook(Node n, string name, String ignore, ref List<Vector3> hooks, Matrix pos)
        {
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode m = (MeshNode)n2;
                    m.FindHook(name, ignore, ref hooks, m.world * pos);
                }
                FindHook(n2, name, ignore, ref hooks, n2.world * pos);
            }
        }

        public MeshNode Inside(float x, float y, String texture, float size)
        {
            MeshNode res;
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    res = mn.Inside(x, y, texture, size);
                    if (res != null)
                    {
                        return res;
                    }
                }
                else
                {
                    res = Inside(n, x, y, texture, size);
                    if (res != null)
                        return res;
                }
            }
            return null;
        }

        private MeshNode Inside(Node n, float x, float y, String texture, float size)
        {
            MeshNode res;
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    res = mn.Inside(x, y, texture, size);
                    if (res != null)
                    {
                        return res;
                    }
                }
                else
                {
                    res = Inside(n2, x, y, texture, size);
                    if (res != null)
                    {
                        return res;
                    }
                }
            }
            return null;
        }

        public Vector3 GetLocation(String name)
        {
            String test = name.ToLowerInvariant();
            foreach (Node n in root.children)
            {
                String test2 = n.Name.ToLowerInvariant();
                if (test == test2)
                {
                    return n.world.Translation;
                }
                Vector3 res = GetLocation(n, name, n.world);
                if (res != Vector3.Zero)
                    return res;
            }
            return Vector3.Zero;
        }

        public Vector3 GetLocation(Node n2, String name, Matrix world)
        {
            String test = name.ToLowerInvariant();
            foreach (Node n in n2.children)
            {
                String test2 = n.Name.ToLowerInvariant();
                if (test == test2)
                {
                    return Vector3.Transform(n.world.Translation, world);
                }
                Vector3 res = GetLocation(n, name, n.world * world);
                if (res != Vector3.Zero)
                    return res;
            }
            return Vector3.Zero;
        }
        #endregion

        #region Draw methods

        public void DrawSkin(Graphics g, Pen p, Brush b, String texture, float size)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawSkin(g, p, b, texture, size);
                }
                else
                {
                    DrawSkin(n, g, p, b, texture, size);
                }
            }
        }

        public void DrawSkin(Node n, Graphics g, Pen p, Brush b, String texture, float size)
        {
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    mn.DrawSkin(g, p, b, texture, size);
                }
                else
                {
                    DrawSkin(n2, g, p, b, texture, size);
                }
            }
        }

        public void Draw(BasicEffect effect, float distance)
        {
            Matrix world = Matrix.Identity;
            foreach (Node n in root.children)
            {
                world = n.world;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                        mn.Draw(effect, distance, world);
                }
                Draw(n, effect, distance, world);
            }
        }

        public void Draw(Effect effect, float distance, bool sort)
        {
            #region Fox one add ons
            foreach (Animator aa in animators)
            {
                aa.Update(this, 0.01f);
            }
            #endregion

            #region script based aircraft animation
            if (Form1.script != null)
            {
                actions.Clear();
                Form1.script.update();

                float t;
                Form1.script.moveElevator(Form1.Pitch);
                Form1.script.moveAileron(Form1.Roll);
                Form1.script.moveArrestorHook(Form1.Arrestor);
                Form1.script.moveFan(Form1.Throttle);
                Form1.script.moveRudder(Form1.Yaw);

                if (ObjectViewer.GearAngle > 0)
                {
                    t = ObjectViewer.GearAngle;
                    Form1.script.moveGear(t);
                }
                if (ObjectViewer.BayAngle > 0)
                {
                    t = ObjectViewer.BayAngle / MathHelper.PiOver2;
                    Form1.script.moveBayDoor(t);
                }
                if (ObjectViewer.WingFoldAngle > 0)
                {
                    Form1.script.moveWingFold(ObjectViewer.WingFoldAngle);
                }
                if (ObjectViewer.FlapAngle > 0)
                {
                    Form1.script.moveFlaps(ObjectViewer.FlapAngle / 60.0f);
                }
                if (ObjectViewer.DoorAngle > 0)
                {
                    Form1.script.moveCockpitDoor(ObjectViewer.DoorAngle);
                }
                if (ObjectViewer.AirBrake > 0)
                {
                    Form1.script.moveAirBrake(ObjectViewer.AirBrake);
                }
                if (ObjectViewer.Turrets)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float[] values = new float[2];
                        values[0] = ObjectViewer.TurretAngle;
                        values[1] = 0;
                        Form1.script.turretAngles(i, values);

                        turret_limits[i].Min = values[0];
                        turret_limits[i].Max = values[1];

                    }
                }
                actions = Form1.script.GetResults();
            }
            #endregion

            #region Animated cockpit
            if (Form1.cscript != null)
            {
                actions.Clear();
                Form1.cscript.reflectWorldToInstruments(0.5f, ObjectViewer.fv);
                actions = Form1.cscript.GetResults();
            }
            #endregion

            Matrix world = Matrix.Identity;

            foreach (Node n in root.children)
            {
                String test = n.Name.ToLower();

                world = n.world;
                if ((ObjectViewer.Explode) && (n.Seperable))
                {
                    Vector3 rp = new Vector3(world.M41, world.M42, world.M43);
                    rp *= distance * 0.01f;

                    world = Matrix.CreateTranslation(rp) * world;
                }
                world = AdjustMatrix(world, test);
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                    {

                        if (ObjectViewer.Explode)
                            mn.Draw(effect, 0, world, sort);
                        else
                            mn.Draw(effect, distance, world, sort);
                    }
                }

                Draw(n, effect, distance, world, sort);
            }
        }

        public void DrawBumped(float distance, Matrix vp, bool sort)
        {

            Matrix world = Matrix.Identity;

            foreach (Node n in root.children)
            {
                String test = n.Name.ToLower();

                world = n.world;
                if ((ObjectViewer.Explode) && (n.Seperable))
                {
                    Vector3 rp = new Vector3(world.M41, world.M42, world.M43);
                    rp *= distance * 0.01f;

                    world = Matrix.CreateTranslation(rp) * world;
                }
                world = AdjustMatrix(world, test);
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                    {
                        if (ObjectViewer.Explode)
                            mn.DrawBumped(ObjectViewer.Instance.bumpmap, 0, world, sort);
                        else
                            mn.DrawBumped(ObjectViewer.Instance.bumpmap, distance, world, sort);
                    }
                }

                DrawBumped(n, distance, world, sort);
            }
        }

        public void DrawBumped(Node n, float distance, Matrix vp, bool sort)
        {
            Matrix adj = Matrix.Identity;

            foreach (Node n2 in n.children)
            {
                String test = n2.Name.ToLower();
                adj = Matrix.Identity;
                if ((ObjectViewer.Explode) && (n2.Seperable))
                {
                    adj = n2.world * n.world * vp;
                    Vector3 l = new Vector3(adj.M41, adj.M42, 0);
                    l *= distance * 0.01f;
                    adj = Matrix.CreateTranslation(l);
                }

                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    foreach (AircraftActions a in actions)
                    {
                        if (test.Equals(a.name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (a.type == 1)
                            {
                                mn.Hidden = a.visible;
                            }
                        }
                    }
                    if (!mn.Hidden)
                    {
                        if (ObjectViewer.Explode)
                            mn.DrawBumped(ObjectViewer.Instance.bumpmap, 0, adj * mn.world * vp, sort);
                        else
                            mn.DrawBumped(ObjectViewer.Instance.bumpmap, distance, adj * mn.world * vp, sort);
                    }
                }
                else
                {
                    adj = AdjustMatrix(adj, test);

                    DrawBumped(n2, distance, adj * n2.world * vp, sort);
                }
            }
        }

        public void DrawGlass(Effect effect, float distance, Matrix vp)
        {
            RasterizerState stat = new RasterizerState();
            stat.CullMode = CullMode.None;
            stat.DepthBias = 2 / 400000.0f;
            Form1.graphics.BlendState = BlendState.AlphaBlend;
            Form1.graphics.RasterizerState = stat;

            Matrix world = Matrix.Identity;
            foreach (Node n in root.children)
            {
                String test = n.Name.ToLower();
                world = n.world;
                world = AdjustMatrix(world, test);
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                        mn.DrawGlass(effect, distance, world, vp);
                }
                DrawGlass(n, effect, distance, world, vp);
            }
        }

        public void DrawShadow(Effect effect, float distance)
        {
            Matrix world = Matrix.Identity;
            foreach (Node n in root.children)
            {
                world = n.world;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                        mn.DrawShadow(effect, distance, world);
                }
                DrawShadow(n, effect, distance, world);
            }
        }

        void Draw(Node n, BasicEffect effect, float distance, Matrix world)
        {
            Matrix adj = Matrix.Identity;
            foreach (Node n2 in n.children)
            {
                adj = Matrix.Identity;
                if ((ObjectViewer.Explode) && (n2.Seperable))
                {
                    Vector3 rp = new Vector3(world.M41, world.M42, world.M43);
                    float l = rp.Length();
                    l *= l;
                    l *= 0.5f;
                    rp.Normalize();
                    rp *= l;
                    adj = Matrix.CreateTranslation(rp);
                }
                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    if (!mn.Hidden)
                        mn.Draw(effect, distance, n.world * adj * world);
                }
                Draw(n2, effect, distance, n.world * adj * world);
            }
        }

        void Draw(Node n, Effect effect, float distance, Matrix world, bool sort)
        {
            Matrix adj = Matrix.Identity;

            foreach (Node n2 in n.children)
            {
                String test = n2.Name.ToLower();
                adj = Matrix.Identity;
                if ((ObjectViewer.Explode) && (n2.Seperable))
                {
                    adj = n2.world * n.world * world;
                    Vector3 l = new Vector3(adj.M41, adj.M42, 0);
                    l *= distance * 0.01f;
                    adj = Matrix.CreateTranslation(l);
                }

                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    foreach (AircraftActions a in actions)
                    {
                        if (test.Equals(a.name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (a.type == 1)
                            {
                                mn.Hidden = a.visible;
                            }
                        }
                    }
                    //if (!mn.Hidden)
                    {
                        if (ObjectViewer.Explode)
                            mn.Draw(effect, 0, adj * mn.world * world, sort);
                        else
                            mn.Draw(effect, distance, adj * mn.world * world, sort);
                    }
                }
                else
                {
                    adj = AdjustMatrix(adj, test);

                    Draw(n2, effect, distance, adj * n2.world * world, sort);
                }
            }
        }

        Matrix AdjustMatrix(Matrix adj, String test)
        {
            if (Form1.script == null)
            {
                if (test.Contains("bay01"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.BayAngle) * adj;
                }
                if (test.Contains("bay1"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.BayAngle) * adj;
                }
                if (test.Contains("bay2"))
                {
                    adj = Matrix.CreateRotationY(-ObjectViewer.BayAngle) * adj;
                }
                if (test.Contains("bay04"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.BayAngle) * adj;
                }
                if (test.Contains("bay07"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.BayAngle) * adj;
                }
                if (test.Contains("bay10"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.BayAngle) * adj;
                }
                if (test.Contains("gearl3"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearr3"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearc3"))
                {
                    adj = Matrix.CreateRotationY(-ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearc4"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearr4"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearl4"))
                {
                    adj = Matrix.CreateRotationY(-ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearc5"))
                {
                    adj = Matrix.CreateRotationY(-ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearl6"))
                {
                    adj = Matrix.CreateRotationY(-ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearl10"))
                {
                    adj = Matrix.CreateRotationY(-ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearl11"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearr6"))
                {
                    adj = Matrix.CreateRotationY(-ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearr10"))
                {
                    adj = Matrix.CreateRotationY(ObjectViewer.GearAngle) * adj;
                }
                if (test.Contains("gearr11"))
                {
                    adj = Matrix.CreateRotationY(-ObjectViewer.GearAngle) * adj;
                }
            }
            else
            {

                foreach (AircraftActions a in actions)
                {
                    if (a.name.Equals(test, StringComparison.OrdinalIgnoreCase))
                    {
                        if (a.type == 0)
                        {
                            //adj *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(-a.p), MathHelper.ToRadians(-a.r), MathHelper.ToRadians(a.y));

                            adj *= Matrix.CreateRotationY(MathHelper.ToRadians(-a.p));
                            adj *= Matrix.CreateRotationX(MathHelper.ToRadians(-a.r));
                            adj *= Matrix.CreateRotationZ(MathHelper.ToRadians(a.y));

                        }
                        if (a.type == 2)
                            adj *= Matrix.CreateTranslation(a.y, a.p, a.r);
                    }
                }
                if (test.StartsWith("turret"))
                {
                    char c = test.ElementAt(6);
                    try
                    {
                        char t = test.ElementAt(7);
                        if ((t == 'a') || (t == 'A'))
                        {
                            int ci = c - '0';
                            ci--;

                            adj = Matrix.CreateRotationY(MathHelper.ToRadians(turret_limits[ci].Min)) * adj;
                        }
                    }
                    catch (Exception) { }

                }

            }
            //if (Form1.cscript != null)
            //{
            //    foreach (AircraftActions a in actions)
            //    {
            //        if (a.name.Equals(test, StringComparison.OrdinalIgnoreCase))
            //        {
            //            if (a.type == 0)
            //                adj = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(-a.p), MathHelper.ToRadians(-a.r), MathHelper.ToRadians(a.y));
            //            if (a.type == 2)
            //                adj = Matrix.CreateTranslation(a.y, a.p, a.r);
            //        }
            //    }
            //}
            if (Form1.BailOut)
            {
                if (test.StartsWith("pilot"))
                {
                    char n = test.ElementAt(5);
                    int bp = (int)(n - '1');
                    if (bp < ObjectViewer.BailHooks.Count)
                        adj = Matrix.CreateTranslation(ObjectViewer.BailHooks[bp]) * adj;
                }
            }
            return adj;
        }

        void DrawGlass(Node n, Effect effect, float distance, Matrix world, Matrix vp)
        {
            foreach (Node n2 in n.children)
            {

                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    if (!mn.Hidden)
                        mn.DrawGlass(effect, distance, mn.world * world, vp);
                }
                else
                {
                    Matrix adj = Matrix.Identity;
                    String test = n2.Name.ToLower();
                    adj = AdjustMatrix(adj, test);
                    DrawGlass(n2, effect, distance, adj * n2.world * world, vp);
                }
            }
        }

        void DrawShadow(Node n, Effect effect, float distance, Matrix world)
        {
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    if (!mn.Hidden)
                        mn.DrawShadow(effect, distance, mn.world * world);
                }
                DrawShadow(n2, effect, distance, n2.world * world);
            }
        }

        public void DrawCollisionMesh(BasicEffect be)
        {
            Matrix world = Matrix.Identity;
            foreach (Node n in root.children)
            {
                world = n.world;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                        mn.DrawCollisionMesh(be, world);
                }
                DrawCollisionMesh(n, be, world);
            }
        }

        void DrawCollisionMesh(Node n, BasicEffect be, Matrix World)
        {
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    if (!mn.Hidden)
                        mn.DrawCollisionMesh(be, mn.world * World);
                }
                DrawCollisionMesh(n2, be, n2.world * World);
            }
        }

        public void DrawAo(BasicEffect effect, int size, ref List<float> ao_values)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawAo(effect, size, ref ao_values);
                }
                else
                {
                    DrawAo(n, effect, size, ref ao_values);
                }
            }
        }

        void DrawAo(Node n2, BasicEffect effect, int size, ref List<float> ao_values)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawAo(effect, size, ref ao_values);
                }
                else
                {
                    DrawAo(n, effect, size, ref ao_values);
                }
            }
        }

        public void DrawAO(BasicEffect effect, int size, String texture)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawAo(effect, size, texture);
                }
                else
                {
                    DrawAO(n, effect, size, texture);
                }
            }
        }

        public void DrawAO(Node n2, BasicEffect effect, int size, String texture)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawAo(effect, size, texture);
                }
                else
                {
                    DrawAO(n, effect, size, texture);
                }
            }
        }

        public void DrawNormals(BasicEffect effect)
        {
            Matrix world = Matrix.Identity;
            foreach (Node n in root.children)
            {
                world = n.world;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawNormals(effect, world);
                }
                else
                {
                    DrawNormals(n, effect, world);
                }
            }
        }

        public void DrawNormals(Node n2, BasicEffect effect, Matrix world)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.DrawNormals(effect, mn.world * world);
                }
                else
                {
                    DrawNormals(n, effect, n.world * world);
                }
            }
        }
        #endregion

        #region Tests
        public void Shoot(int x, int y, Matrix projection, Matrix view)
        {
            Matrix world = Matrix.Identity;
            foreach (Node n in root.children)
            {
                world = n.world;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                    {
                        mn.Shoot(x, y, projection, view, world);
                    }
                }
                Shoot(n, x, y, projection, view, world);

            }
        }

        public void Shoot(Node n, int x, int y, Matrix projection, Matrix view, Matrix world)
        {
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    if (!mn.Hidden)
                    {
                        mn.Shoot(x, y, projection, view, mn.world * world);
                    }
                }
                else
                {
                    Shoot(n2, x, y, projection, view, n2.world * world);
                }
            }
        }

        public bool Blocked(Ray r)
        {
            Matrix world = Matrix.Identity;
            foreach (Node n in root.children)
            {
                world = n.world;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (mn.Blocked(r, world))
                        return true;
                }
                else
                {
                    if (Blocked(n, r, world))
                        return true;
                }
            }
            return false;
        }

        public bool Blocked(Node n2, Ray r, Matrix world)
        {

            foreach (Node n in n2.children)
            {
                world = n.world;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (mn.Blocked(r, mn.world * world))
                        return true;
                }
                else
                {
                    if (Blocked(n, r, n.world * world))
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region Helpers
        String filter(String s)
        {
            String result = "";
            char[] next = new char[1];
            int a = 0;
            char old = 'n';
            while (a < s.Length)
            {
                next[0] = s.ElementAt(a);
                if (next[0] != ' ')
                {
                    result = result + new String(next);
                }
                else
                {
                    if (old != ' ')
                        result = result + new String(next);

                }
                old = next[0];
                a++;
            }
            return result;
        }

        public void AddRoot(Node n)
        {
            root.children.Add(n);
        }

        public void ResetDamage()
        {
            foreach (Node n in root.children)
            {
                n.Damage = 0;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    String t = mn.Name.Substring(mn.Name.Length - 1);
                    try
                    {
                        int dl = int.Parse(t);
                        if (dl > 0)
                        {
                            mn.Hidden = true;
                        }
                    }
                    catch (Exception) { }
                }
                ResetDamage(n);
            }
        }

        public void ResetDamage(Node nroot)
        {
            foreach (Node n in nroot.children)
            {
                n.Damage = 0;
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    String t = mn.Name.Substring(mn.Name.Length - 1);
                    try
                    {
                        int dl = 0;
                        int.TryParse(t, out dl);

                        if (dl > 0)
                        {
                            mn.Hidden = true;
                        }
                    }
                    catch (Exception) { }
                }
                ResetDamage(n);
            }
        }


        #endregion

        #region Ambient Occlusion
        public void BuildAoList(ref List<Vector3> verts, ref List<Vector3> normals)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.BuildAoList(n.world, ref verts, ref normals);
                }
                else
                {
                    BuildAoList(n, n.world, ref verts, ref normals);
                }
            }
        }

        public void BuildAoList(Node n2, Matrix world, ref List<Vector3> verts, ref List<Vector3> normals)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.BuildAoList(mn.world * world, ref verts, ref normals);
                }
                else
                {
                    BuildAoList(n, n.world * world, ref verts, ref normals);
                }
            }
        }

        public void BuildAoListVertexCamera(String texture, ObjectViewer viewer)
        {

            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                        mn.BuildAoListVertexCamera(texture, Matrix.Identity, viewer);
                }
                else
                {
                    BuildAoListVertexCamera(n, Matrix.Identity, texture, viewer);
                }
            }
        }

        public void BuildAoListVertexCamera(Node n2, Matrix world, String texture, ObjectViewer viewer)
        {

            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                        mn.BuildAoListVertexCamera(texture, world, viewer);
                }
                else
                {
                    BuildAoListVertexCamera(n, n.world * world, texture, viewer);
                }
            }
        }

        public void BuildAoListMultiVertexCamera(String texture, ObjectViewer viewer)
        {

            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.BuildAoListMultiVertexCamera(texture, n.world, viewer);
                }
                else
                {
                    BuildAoListMultiVertexCamera(n, n.world, texture, viewer);
                }
            }
        }

        public void BuildAoListMultiVertexCamera(Node n2, Matrix world, String texture, ObjectViewer viewer)
        {

            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.BuildAoListMultiVertexCamera(texture, n.world * world, viewer);
                }
                else
                {
                    BuildAoListMultiVertexCamera(n, n.world * world, texture, viewer);
                }
            }
        }

        public void BuildAoListiVertexRay(String texture, ObjectViewer viewer, int count)
        {

            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                        mn.BuildAoListVertexRay(texture, Matrix.Identity, viewer, count);

                }
                else
                {
                    BuildAoListVertexRay(n, Matrix.Identity, texture, viewer, count);
                }
            }
        }

        public void BuildAoListVertexRay(Node n2, Matrix world, String texture, ObjectViewer viewer, int count)
        {

            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (!mn.Hidden)
                        mn.BuildAoListVertexRay(texture, world, viewer, count);
                }
                else
                {
                    BuildAoListVertexRay(n, n.world * world, texture, viewer, count);
                }
            }
        }

        float GetRand()
        {
            double r = (rand.NextDouble() - 0.5) * 2;
            return (float)r;
        }

        public float GetAORay(Vector3 pos, Vector3 normal, int count)
        {
            float res = 0;
            for (int i = 0; i < count; i++)
            {
                Ray ray = new Ray();
                ray.Position = pos;
                float h = GetRand() * MathHelper.Pi;
                float p = GetRand() * MathHelper.PiOver4;

                Matrix m = Matrix.CreateFromYawPitchRoll(h, p, 0);
                ray.Direction = Vector3.Transform(normal, m);
                ray.Direction.Normalize();
                ray.Position += ray.Direction * 0.01f;      //Stop it intersecting with itself

                if (CheckCollide(ray, 1))
                {
                    res += 1;
                }

            }

            return  ((float)res / (float)count);
        }

        public bool CheckCollide(Ray r)
        {
            bool result = false;
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (mn.QuickCheck(r, mn.world))
                    {
                        return true;
                    }
                }
                else
                {
                    if (CheckCollide(n, r, n.world))
                    {
                        return true;
                    }
                }
            }
            return result;
        }

        public bool CheckCollide(Ray r, float d)
        {
            bool result = false;
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (mn.QuickCheck(r, mn.world,d))
                    {
                        return true;
                    }
                }
                else
                {
                    if (CheckCollide(n, r, n.world, d))
                    {
                        return true;
                    }
                }
            }
            return result;
        }

        private bool CheckCollide(Node n2, Ray r, Matrix world)
        {
            bool result = false;
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (mn.QuickCheck(r, mn.world * world))
                    {
                        return true;
                    }
                }
                else
                {
                    if (CheckCollide(n, r, n.world * world))
                    {
                        return true;
                    }
                }
            }
            return result;
        }

        private bool CheckCollide(Node n2, Ray r, Matrix world, float d)
        {
            bool result = false;
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    if (mn.QuickCheck(r, mn.world * world, d))
                    {
                        return true;
                    }
                }
                else
                {
                    if (CheckCollide(n, r, n.world * world, d))
                    {
                        return true;
                    }
                }
            }
            return result;
        }

        #endregion

        #region Serialisers
        public void Serialise(String name)
        {
            String dir = Path.GetDirectoryName(name);
            using (TextWriter writer = File.CreateText(name))
            {
                writer.WriteLine("[_ROOT_]");
                writer.WriteLine(String.Format("VisibilitySphere {0}", root.VisibiltySphere));
                if (root.Colliders.Count > 0)
                {
                    if (root.Colliders[0].nType == CollisionNodeType.Sphere)
                    {
                        writer.WriteLine(String.Format("CollisionObject sphere {0} {1} {2} {3}",
                                         root.Colliders[0].Sphere.Radius,
                                         root.Colliders[0].Sphere.Center.X,
                                         root.Colliders[0].Sphere.Center.Y,
                                         root.Colliders[0].Sphere.Center.Z));
                    }
                    else
                    {
                        throw new Exception("Unhandled collision node type " + root.Colliders[0].nType);
                    }
                }
                foreach (Node n in root.children)
                {
                    if (n is MeshNode)
                    {
                        MeshNode mn = (MeshNode)n;
                        mn.SaveMaterials(dir);
                        String mname = mn.mesh.mesh_name;
                        mname = Path.Combine(dir, mname);
                        mname += ".msh";
                        using (TextWriter w = File.CreateText(mname))
                        {
                            mn.Serialize(w);
                            w.Close();
                        }
                    }
                    else
                    {

                        writer.WriteLine(String.Format("[{0}]", n.Name));
                        MeshNode mn = (MeshNode)n.children[0];
                        writer.WriteLine(String.Format("Mesh {0}", mn.mesh.mesh_name));
                        writer.WriteLine("Parent _ROOT_");
                        if (n.Hidden)
                            writer.WriteLine("Hidden");
                        if (n.Seperable)
                            writer.WriteLine("Separable");

                        writer.WriteLine(String.Format("Attaching {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                                        n.world.M11, n.world.M12, n.world.M13,
                                        n.world.M21, n.world.M22, n.world.M23,
                                        n.world.M31, n.world.M32, n.world.M33,
                                        n.world.M41, n.world.M42, n.world.M43));

                    }
                    foreach (CollisionNode cn in n.Colliders)
                    {
                        cn.Serialise(writer);
                    }

                    Serialise(n, writer, dir);
                }
                writer.Close();
            }
        }

        private void Serialise(Node nr, TextWriter writer, String dir)
        {
            foreach (Node n in nr.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.SaveMaterials(dir);

                    String name = mn.mesh.mesh_name;
                    name = Path.Combine(dir, name);
                    name += ".msh";
                    using (TextWriter w = File.CreateText(name))
                    {
                        mn.Serialize(w);
                        w.Close();
                    }
                }
                else
                {
                    writer.WriteLine(String.Format("[{0}]", n.Name));
                    MeshNode mn = (MeshNode)n.children[0];
                    writer.WriteLine(String.Format("Mesh {0}", mn.mesh.mesh_name));
                    writer.WriteLine(String.Format("Parent {0}", nr.Name));
                    if (n.Hidden)
                        writer.WriteLine("Hidden");
                    if (n.Seperable)
                        writer.WriteLine("Separable");

                    writer.WriteLine(String.Format("Attaching {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                                    n.world.M11, n.world.M12, n.world.M13,
                                    n.world.M21, n.world.M22, n.world.M23,
                                    n.world.M31, n.world.M32, n.world.M33,
                                    n.world.M41, n.world.M42, n.world.M43));


                    foreach (CollisionNode cn in n.Colliders)
                    {
                        cn.Serialise(writer);
                    }

                    Serialise(n, writer, dir);
                }
            }
        }

        public void SerialiseHim(String name)
        {
            String dir = Path.GetDirectoryName(name);
            using (TextWriter writer = File.CreateText(name))
            {
                writer.WriteLine("[_ROOT_]");
                writer.WriteLine(String.Format("VisibilitySphere {0}", root.VisibiltySphere));
                if (root.Colliders[0].nType == CollisionNodeType.Sphere)
                {
                    writer.WriteLine(String.Format("CollisionObject sphere {0} {1} {2} {3}",
                                     root.Colliders[0].Sphere.Radius,
                                     root.Colliders[0].Sphere.Center.X,
                                     root.Colliders[0].Sphere.Center.Y,
                                     root.Colliders[0].Sphere.Center.Z));
                }
                else
                {
                    throw new Exception("Unhandled collision node type " + root.Colliders[0].nType);
                }
                foreach (Node n in root.children)
                {
                    if (n is MeshNode)
                    {

                    }
                    else
                    {

                        writer.WriteLine(String.Format("[{0}]", n.Name));
                        MeshNode mn = (MeshNode)n.children[0];
                        writer.WriteLine(String.Format("Mesh {0}", mn.mesh.mesh_name));
                        writer.WriteLine("Parent _ROOT_");
                        if (n.Hidden)
                            writer.WriteLine("Hidden");
                        if (n.Seperable)
                            writer.WriteLine("Separable");

                        writer.WriteLine(String.Format("Attaching {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                                        n.world.M11, n.world.M12, n.world.M13,
                                        n.world.M21, n.world.M22, n.world.M23,
                                        n.world.M31, n.world.M32, n.world.M33,
                                        n.world.M41, n.world.M42, n.world.M43));

                    }
                    foreach (CollisionNode cn in n.Colliders)
                    {
                        cn.Serialise(writer);
                    }

                    SerialiseH(n, writer, dir);
                }
                writer.Close();
            }
        }

        private void SerialiseH(Node nr, TextWriter writer, String dir)
        {
            foreach (Node n in nr.children)
            {
                if (n is MeshNode)
                {

                }
                else
                {
                    writer.WriteLine(String.Format("[{0}]", n.Name));
                    MeshNode mn = (MeshNode)n.children[0];
                    writer.WriteLine(String.Format("Mesh {0}", mn.mesh.mesh_name));
                    writer.WriteLine(String.Format("Parent {0}", nr.Name));
                    if (n.Hidden)
                        writer.WriteLine("Hidden");
                    if (n.Seperable)
                        writer.WriteLine("Separable");

                    writer.WriteLine(String.Format("Attaching {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                                    n.world.M11, n.world.M12, n.world.M13,
                                    n.world.M21, n.world.M22, n.world.M23,
                                    n.world.M31, n.world.M32, n.world.M33,
                                    n.world.M41, n.world.M42, n.world.M43));


                    foreach (CollisionNode cn in n.Colliders)
                    {
                        cn.Serialise(writer);
                    }

                    Serialise(n, writer, dir);
                }
            }
        }

        #region DAE
        public void SaveDAEEffects(TextWriter writer, bool effect)
        {
            foreach (Node n in root.children)
            {
                SaveDAEEffects(n, writer, effect);
            }
        }

        private void SaveDAEEffects(Node n2, TextWriter tw, bool effect)
        {
            if (n2 is MeshNode)
            {
                MeshNode mn = (MeshNode)n2;
                mn.mesh.SaveDAEEffects(tw, effect);
            }
            foreach (Node n in n2.children)
            {
                SaveDAEEffects(n, tw, effect);
            }
        }

        public void SaveDAEMesh(TextWriter tw)
        {
            foreach (Node n in root.children)
            {
                SaveDAEMesh(n, tw);
            }
        }

        public void SaveDAEMesh(Node n2, TextWriter tw)
        {
            if (n2 is MeshNode)
            {
                MeshNode mn = (MeshNode)n2;
                mn.mesh.SaveDAEMesh(tw);
            }
            foreach (Node n in n2.children)
            {
                SaveDAEMesh(n, tw);
            }
        }

        public void SaveDAEStruture(TextWriter tw)
        {
            foreach (Node n in root.children)
            {
                SaveDAEStructure(tw, n, Matrix.Identity);
            }
        }

        private void SaveDAEStructure(TextWriter tw, Node n, Matrix m)
        {
            Matrix mn = n.world * m;
            if (n is MeshNode)
            {
                Vector3 scale, translation;
                Quaternion quat;

                MeshNode men = (MeshNode)n;
                mn.Decompose(out scale, out quat, out translation);
                tw.WriteLine(String.Format("\t<node id=\"{0}\" type=\"NODE\">", men.Name));
                tw.WriteLine(String.Format("\t\t<translate sid=\"location\">{0} {1} {2}</translate>", translation.X, translation.Y, translation.Z));
                tw.WriteLine(String.Format("\t\t<scale sid=\"scale\">{0} {1} {2}</scale>", scale.X, scale.Y, scale.Z));
                tw.WriteLine(String.Format("\t\t<instance_geometry url=\"#{0}-mesh\">", men.Name));
                tw.WriteLine("\t\t<bind_material>");
                tw.WriteLine("\t\t\t<technique_common>");
                tw.WriteLine("\t\t\t\t<instance_material symbol=\"nose1\" target=\"#nose-material\">");
                tw.WriteLine("\t\t\t\t\t<bind_vertex_input semantic=\"UVTex\" input_semantic=\"TEXCOORD\" input_set=\"0\"/>");
                tw.WriteLine("\t\t\t\t</instance_material>");
                tw.WriteLine("\t\t\t</technique_common>");
                tw.WriteLine("\t\t\t</bind_material>");
                tw.WriteLine("\t\t</instance_geometry>");
                tw.WriteLine("\t</node>");

            }
            foreach (Node n2 in n.children)
            {
                SaveDAEStructure(tw, n2, mn);
            }

        }
        #endregion

        #region OGRE
        public void SaveOGRE(String dir)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveOGRE(dir);
                }
                SaveOGRE(n, dir);
            }
        }
        private void SaveOGRE(Node n2, String dir)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveOGRE(dir);
                }
                SaveOGRE(n, dir);
            }
        }
        #endregion

        #region Obj
        public void SaveAsObj(String dir)
        {

            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveAsOBJ(dir, "il2mat");
                }
                SaveAsObj(n, dir);
            }
        }
        public void SaveAsObj(Node n2, String dir)
        {

            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveAsOBJ(dir, "il2mat");
                }
                SaveAsObj(n, dir);
            }
        }
        #endregion

        public void SaveAsUE4(string dir)
        {
            Matrix world = Matrix.Identity;
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveAsUE4(dir, world);
                }
                world = n.world;
                SaveAsUE4(dir, n, world);
            }
        }
        public void SaveAsUE4(string dir, Node n2, Matrix world)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveAsUE4(dir, world);
                }

            }
        }

        public void SaveAsUE5(string dir)
        {
            Matrix world = Matrix.Identity;
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveAsUE5(dir, Name, world);
                }
                SaveAsUE5(dir, n, n.world);
            }

        }

        public void SaveAsUE5(string dir, Node n2, Matrix world)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveAsUE5(dir, Name, world);
                }
                SaveAsUE5(dir, n, n.world);
            }
        }

        #region FBX
        public void SaveAsFBX(String dir)
        {
            Matrix world = Matrix.Identity;

            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    n.fbx_node = mn.mesh.SaveFBX(dir, n.world, null, n);// * world,null);
                }
            }
            foreach (Node n in root.children)
            {
                SaveAsFBX(n, dir, n.world);
            }
        }
        public void SaveAsFBX(Node n2, String dir, Matrix world)
        {

            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    n.fbx_node = mn.mesh.SaveFBX(dir, n2.world, n2, n);// * world, n2);

                }

            }
            foreach (Node n in n2.children)
            {
                SaveAsFBX(n, dir, n.world * world);
            }
        }
        #endregion


        public void SaveToFox1(String dir, Fox1ExportDialog options)
        {
            String filename = Path.Combine(dir, Name + ".gameobject");
            if (File.Exists(filename))
            {
                String backup = filename.Replace(".gameobject", ".bak");
                if (File.Exists(backup))
                {
                    File.Delete(backup);
                }
                File.Move(filename, backup);
            }


            TextWriter gameobject = new StreamWriter(filename);
            Matrix world = Matrix.Identity;

            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveToFox1(dir, n, gameobject);
                }

            }
            foreach (Node n in root.children)
            {
                SaveToFox1(n, dir, gameobject,options.Mode); 
            }

            gameobject.WriteLine("WorldTransform_1");
            gameobject.WriteLine("WorldTransform");
            gameobject.WriteLine("1");
            gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
            gameobject.WriteLine("1");
            gameobject.WriteLine("Consumers#;");
            gameobject.WriteLine("0");
            switch (options.Mode)
            {
                case 0:
                    {
                        gameobject.WriteLine("AircraftComponent_1");
                        gameobject.WriteLine("AircraftComponent");
                        gameobject.WriteLine("2");
                        gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                        gameobject.WriteLine("Transform#WorldTransform#WorldTransform_1:Consumers");
                        gameobject.WriteLine("1");
                        gameobject.WriteLine("Meshes#CF_D0:Aircraft,;");
                        gameobject.WriteLine("0");
                    }
                    break;

                case 1:
                    {
                        gameobject.WriteLine("AircraftComponent_1");
                        gameobject.WriteLine("AircraftComponent");
                        gameobject.WriteLine("2");
                        gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                        gameobject.WriteLine("Transform#WorldTransform#WorldTransform_1:Consumers");
                        gameobject.WriteLine(root.children.Count);
                        foreach (Node n in root.children)
                        {
                            gameobject.WriteLine(String.Format("MultiMeshComponent#Meshes#{0}:Aircraft,;", n.Name));
                        }

                        gameobject.WriteLine("0");
                    }
                    break;
                case 4:
                    {
                        gameobject.WriteLine("AircraftComponent_1");
                        gameobject.WriteLine("AircraftComponent");
                        gameobject.WriteLine("2");
                        gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                        gameobject.WriteLine("Transform#WorldTransform#WorldTransform_1:Consumers");
                        gameobject.WriteLine("0");          // TODO need to add all top level meshes
                        gameobject.WriteLine("0");
                    }
                    break;
            }


            if (options.ExportPropAnimators)
            {
                for (int i = 0; i < nProps; i++)
                {
                    gameobject.WriteLine(String.Format("PropellorAnimatorComponent_{0}", i + 1));
                    gameobject.WriteLine("PropellorAnimatorComponent");
                    gameobject.WriteLine("1");
                    gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                    gameobject.WriteLine("0");
                    gameobject.WriteLine("2");
                    gameobject.WriteLine(String.Format("EngineNumber Int {0}", i + 1));
                    gameobject.WriteLine(String.Format("PropellorNumber Int {0}", i + 1));
                }
            }

            if (options.ExportVators)
            {
                gameobject.WriteLine("ElevatorAnimatorComponent_1");
                gameobject.WriteLine("ElevatorAnimatorComponent");
                gameobject.WriteLine("1");
                gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                gameobject.WriteLine("0");
                gameobject.WriteLine("2");
                gameobject.WriteLine("ElevatorNumber Int 0");
                gameobject.WriteLine("Scale Float 30");
            }
            if (options.ExportAilerons)
            {
                gameobject.WriteLine("AileronsAnimatorComponent_1");
                gameobject.WriteLine("AileronsAnimatorComponent");
                gameobject.WriteLine("1");
                gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                gameobject.WriteLine("0");
                gameobject.WriteLine("2");
                gameobject.WriteLine("AileronNumber Int 0");
                gameobject.WriteLine("Scale Float 30");
            }
            if (options.ExportRudders)
            {
                for (int i = 0; i < nRudders; i++)
                {
                    gameobject.WriteLine(String.Format("RudderAnimatorComponent_{0}", i + 1));
                    gameobject.WriteLine("RudderAnimatorComponent");
                    gameobject.WriteLine("1");
                    gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                    gameobject.WriteLine("0");
                    gameobject.WriteLine("2");
                    gameobject.WriteLine(String.Format("RudderNumber Int {0}", i + 1));
                    gameobject.WriteLine("Scale Float 30");
                }
            }
            if (options.ExportFlaps)
            {
                int todo = nFlaps;
                int index = 1;
                int done = 1;
                while (todo > 0)
                {
                    String flapname = String.Format("Flap{0:00}_D0", index);
                    Node n = FindNode(flapname);
                    if (n != null)
                    {
                        gameobject.WriteLine(String.Format("FlapAnimatorComponent_{0}", done));
                        gameobject.WriteLine("FlapAnimatorComponent");
                        gameobject.WriteLine("1");
                        gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                        gameobject.WriteLine("0");
                        gameobject.WriteLine("2");
                        gameobject.WriteLine(String.Format("FlapNumber Int {0}", index));
                        gameobject.WriteLine("Scale Float 85");
                        todo--;
                        done++;
                    }
                    index++;
                }


            }
            if (options.CvtAnimations.Count > 0)
            {
                for (int i = 0; i < options.CvtAnimations.Count; i++)
                {
                    gameobject.WriteLine(String.Format("CVTAnimatorComponent_{0}", i + 1));
                    gameobject.WriteLine("CVTAnimatorComponent");
                    gameobject.WriteLine("1");
                    gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                    gameobject.WriteLine("0");
                    gameobject.WriteLine("6");
                    gameobject.WriteLine(String.Format("Maximum Float {0}", options.CvtAnimations[i].Maximum));
                    gameobject.WriteLine(String.Format("Minimum Float {0}", options.CvtAnimations[i].Minimum));
                    gameobject.WriteLine(String.Format("Start Float {0}", options.CvtAnimations[i].Start));
                    gameobject.WriteLine(String.Format("Finish Float {0}", options.CvtAnimations[i].Finish));
                    gameobject.WriteLine("TargetMesh String " + options.CvtAnimations[i].Target);
                    gameobject.WriteLine("ControlValue String " + options.CvtAnimations[i].Control);
                }
            }
            if (options.TranslateAnimations.Count > 0)
            {
                for (int i = 0; i < options.TranslateAnimations.Count; i++)
                {
                    gameobject.WriteLine(String.Format("TranslateAnimatorComponent_{0}", i + 1));
                    gameobject.WriteLine("TranslateAnimatorComponent");
                    gameobject.WriteLine("1");
                    gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                    gameobject.WriteLine("0");
                    gameobject.WriteLine("4");
                    gameobject.WriteLine(String.Format("Scale Float {0}", options.TranslateAnimations[i].Scale));
                    gameobject.WriteLine("Mesh String " + options.TranslateAnimations[i].Mesh);
                    gameobject.WriteLine("Control String " + options.TranslateAnimations[i].Control);
                    gameobject.WriteLine("Plane Int " + options.TranslateAnimations[i].Plane.ToString());
                }
            }
            if (options.SmoothedAnimations.Count > 0)
            {
                for (int i = 0; i < options.SmoothedAnimations.Count; i++)
                {
                    gameobject.WriteLine(String.Format("SmoothedAngleAnimatorComponent_{0}", i + 1));
                    gameobject.WriteLine("SmoothedAngleAnimatorComponent");
                    gameobject.WriteLine("1");
                    gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                    gameobject.WriteLine("0");
                    gameobject.WriteLine("5");
                    gameobject.WriteLine(String.Format("Scale Float {0}", options.SmoothedAnimations[i].Scale));
                    gameobject.WriteLine(String.Format("Smoothing Float {0}", options.SmoothedAnimations[i].Smoothing));
                    gameobject.WriteLine("Mesh String " + options.SmoothedAnimations[i].Mesh);
                    gameobject.WriteLine("Control String " + options.SmoothedAnimations[i].Control);
                    gameobject.WriteLine("Plane Int " + options.SmoothedAnimations[i].Plane.ToString());
                }
            }
            switch (options.Mode)
            {
                case 4:
                case 0:
                    {
                        gameobject.WriteLine("AircraftStateComponent_1");
                        gameobject.WriteLine("AircraftStateComponent");
                        gameobject.WriteLine("1");
                        gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                        gameobject.WriteLine("0");
                        gameobject.WriteLine("0");
                    }
                    break;

                case 1:
                    {
                        gameobject.WriteLine("ShipStateComponent_1");
                        gameobject.WriteLine("ShipStateComponent");
                        gameobject.WriteLine("1");
                        gameobject.WriteLine("Root#Root#" + Name + ":GameComponents");
                        gameobject.WriteLine("0");
                        gameobject.WriteLine("0");
                    }
                    break;

            }
            gameobject.Close();
        }

        private void SaveToFox1(Node n2, String dir, TextWriter g, int mode)
        {
            bool save = n2 is MeshNode;
            bool hooklist = false;
           
            if (!save)
            {
                int cnt = 3;
                if (n2.children.Count > 0)
                {
                    if (n2.children[0] is MeshNode)
                    {
                        MeshNode mn = n2.children[0] as MeshNode;
                        if (mn.mesh.Hooks.Count > 0)
                        {
                            cnt++;
                            hooklist = true;
                        }
                    }
                }
                if (mode == 3)
                    cnt--;
                g.WriteLine(n2.Name);
                g.WriteLine("MultiMeshComponent");
                g.WriteLine(cnt.ToString());
                g.WriteLine(String.Format("Root#Root#{0}:GameComponents", Name));
                if (mode !=3)
                 g.WriteLine("AircraftComponent#Aircraft#AircraftComponent_1:Meshes");
                g.WriteLine("CollisionMeshComponent#Collision#" + n2.Name + "collision");
                if (hooklist)
                {
                    g.WriteLine("HookListComponent#Hooks#Mesh_" + n2.Name + "Hooks");
                }

                g.WriteLine("1");
                g.Write("Children#");
            }
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.mesh.SaveToFox1(dir, n2, g);

                }
                else
                {
                    g.Write(n.Name);
                    g.Write(":Father,");
                }

            }
            if (!save)
            {
                g.WriteLine(";");
                g.WriteLine("2");
                g.Write("Filename String ");
                g.WriteLine(n2.Name);
                g.WriteLine("Hidden Bool " + n2.Hidden.ToString());

                g.WriteLine(n2.Name + "collision");
                g.WriteLine("CollisionMeshComponent");
                g.WriteLine("1");
                g.WriteLine("Root#Root#" + Name + ":GameComponents");
                g.WriteLine("0");
                g.WriteLine("1");
                g.WriteLine("Filename String " + Path.Combine(dir, n2.Name + ".collision"));


            }
            else
            {
                MeshNode mn = n2 as MeshNode;
                if (mn.mesh.Hooks.Count > 0)
                {
                    g.WriteLine(n2.Name + "Hooks");
                    g.WriteLine("HookListComponent");
                    g.WriteLine("1");
                    g.WriteLine("Root#Root#" + Name + ":GameComponents");
                    g.WriteLine("0");
                    g.WriteLine(mn.mesh.Hooks.Count.ToString());

                    foreach (Hook h in mn.mesh.Hooks)
                    {
                        String nn = h.Name.Replace(' ', '_');

                        g.WriteLine(String.Format("Entry String {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                                nn,
                                h.matrix.M11.ToString(), h.matrix.M12.ToString(), h.matrix.M13.ToString(),
                                h.matrix.M21.ToString(), h.matrix.M22.ToString(), h.matrix.M23.ToString(),
                                h.matrix.M31.ToString(), h.matrix.M32.ToString(), h.matrix.M33.ToString(),
                                h.matrix.M41.ToString(), h.matrix.M42.ToString(), h.matrix.M43.ToString()
                                ));
                    }
                }
            }

            foreach (Node n in n2.children)
            {
                SaveToFox1(n, dir, g, mode);
            }
        }

        public void ExportCollisionMeshes(String dir)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.ExportCollision(dir, n);
                }

            }
            foreach (Node n in root.children)
            {
                ExportCollisionMeshes(n, dir);
            }
        }

        public void ExportHooks(TextWriter t)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.ExportHooks(t);
                }

            }
            foreach (Node n in root.children)
            {
                ExportHooks(n, t);
            }
        }

        void ExportHooks(Node n2, TextWriter t)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.ExportHooks(t);
                }

            }
            foreach (Node n in n2.children)
            {
                ExportHooks(n, t);
            }
        }

        void ExportCollisionMeshes(Node n2, String dir)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.ExportCollision(dir, n2);
                }

            }
            foreach (Node n in n2.children)
            {
                ExportCollisionMeshes(n, dir);
            }
        }

        #endregion

        #region Modifiers
        public void ResetHidden()
        {
            ResetHidden(root);
        }

        void ResetHidden(Node n)
        {
            if (n is MeshNode)
            {
                MeshNode mn = (MeshNode)n;
                mn.ResetHidden();
            }
            foreach (Node n2 in n.children)
            {
                ResetHidden(n2);
            }
        }

        public void RegenerateNormals()
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.RegenerateNormals();
                }
                RegenerateNormals(n);
            }
        }
        void RegenerateNormals(Node n2)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.RegenerateNormals();
                }
                RegenerateNormals(n);
            }
        }

        public void SwapTriangles()
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.SwapTriangles();
                }
                SwapTriangles(n);
            }
        }

        public void SwapTriangles(Node n2)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.SwapTriangles();
                }
                SwapTriangles(n);
            }
        }

        public void AdjustLighting()
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.AdjustLighting();
                }
                AdjustLighting(n);
            }
        }

        public void AdjustLighting(Node n2)
        {
            foreach (Node n in n2.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.AdjustLighting();
                }
                AdjustLighting(n);
            }
        }
        #endregion

        #region Loaders
        public void LoadAC(String name, String dir)
        {
            using (TextReader reader = File.OpenText(name))
            {
                List<Material> materials = new List<Material>();
                Form1.Manager.Dispose();
                Form1.Manager.AddWhite();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    char[] seps = new char[] { ' ', '\t', '\a' };
                    string[] parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        switch (parts[0])
                        {
                            case "AC3Db":
                                {
                                    // well it's the right format
                                }
                                break;
                            case "OBJECT":
                                {
                                    if (root.children.Count == 0)
                                    {
                                        // it is the root node
                                        line = reader.ReadLine();
                                        parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                                        if (parts[0].Equals("kids"))
                                        {
                                            int kids = int.Parse(parts[1]);
                                            LoadACObject(reader, root, kids, materials, dir);
                                        }
                                        else
                                        {
                                            throw new Exception("Malformed AC file ");
                                        }
                                    }
                                }
                                break;
                            case "MATERIAL":
                                {
                                    Material m = new Material(parts[1]);
                                    materials.Add(m);
                                    m.tfDoubleSided = true;
                                    m.tfWrapX = true;
                                    m.tfWrapY = true;
                                    m.Diffuse = 1;
                                    m.AlphaTestVal = 0;
                                    int i = 2;
                                    while (i < parts.GetLength(0))
                                    {
                                        if ((parts[i].Equals("rgb")) || (parts[i].Equals("png")))
                                        {
                                            m.Colour[0] = float.Parse(parts[i + 1], System.Globalization.CultureInfo.InvariantCulture);
                                            m.Colour[1] = float.Parse(parts[i + 2], System.Globalization.CultureInfo.InvariantCulture);
                                            m.Colour[2] = float.Parse(parts[i + 3], System.Globalization.CultureInfo.InvariantCulture);
                                            i += 4;
                                        }
                                        if (parts[i].Equals("amb"))
                                        {
                                            m.Ambient = float.Parse(parts[i + 1], System.Globalization.CultureInfo.InvariantCulture);
                                            i += 4;
                                        }
                                        if (parts[i].Equals("emis"))
                                        {
                                            i += 4;
                                        }
                                        if (parts[i].Equals("spec"))
                                        {
                                            m.Specular = float.Parse(parts[i + 1], System.Globalization.CultureInfo.InvariantCulture);
                                            i += 4;
                                        }
                                        if (parts[i].Equals("shi"))
                                        {
                                            m.Shine = 0.5f;
                                            m.SpecularPow = float.Parse(parts[i + 1], System.Globalization.CultureInfo.InvariantCulture);
                                            i += 2;
                                        }
                                        if (parts[i].Equals("trans"))
                                        {
                                            float alpha = float.Parse(parts[i + 1], System.Globalization.CultureInfo.InvariantCulture);
                                            if (alpha > 0)
                                            {
                                                m.Sort = true;
                                                m.Glass = true;
                                            }
                                            i += 2;
                                        }

                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load in a 3D mesh in AC3D AC format
        /// </summary>
        /// <param name="reader">File</param>
        /// <param name="parent">Tree node of parent</param>
        /// <param name="Count">Number of objects to load</param>
        /// <param name="materials">Materials defined in header</param>
        /// <param name="dir">Holding directory</param>
        void LoadACObject(TextReader reader, Node parent, int Count, List<Material> materials, String dir)
        {
            String line;
            Node cur = null;
            List<Vector3> vertices = new List<Vector3>();
            List<List<ushort>> facegroups = new List<List<ushort>>();
            List<AC3DTriangle> tris = new List<AC3DTriangle>();
            List<Vector2> uvs = new List<Vector2>();
            List<string> names = new List<string>();
            Vector2 size = new Vector2(1, 1);
            char[] seps = new char[] { ' ', '\t', '\a' };
            int texID = 0;
            String texture_name = "";

            foreach (Material m in materials)
            {
                facegroups.Add(new List<ushort>());
            }

            for (int i = 0; i < Count; i++)
            {
                line = reader.ReadLine();
                string[] parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                if (parts[0].Equals("OBJECT"))
                {
                    cur = new Node();

                    cur.world = Matrix.CreateRotationX(MathHelper.ToRadians(90));
                    parent.children.Add(cur);
                    vertices.Clear();
                    foreach (List<ushort> kl in facegroups)
                    {
                        kl.Clear();
                    }
                    tris.Clear();

                    while (!parts[0].Equals("kids"))
                    {
                        line = reader.ReadLine();
                        parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                        if (parts[0].Equals("name"))
                        {
                            if (names.Contains(parts[1]))
                            {
                                parts[1] += names.Count();
                            }
                            names.Add(parts[1]);
                            cur.Name = parts[1].Replace('"', '_');

                        }
                        if (parts[0].Equals("loc"))
                        {
                            cur.world.M41 = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                            cur.world.M42 = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                            cur.world.M43 = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);

                        }
                        if (parts[0].Equals("data"))
                        {
                            cur.Name = reader.ReadLine();
                        }
                        if (parts[0].Equals("texture"))
                        {
                            parts[1] = parts[1].TrimStart('"');
                            parts[1] = parts[1].TrimEnd('"');
                            texID = Form1.Manager.AddTexture(parts[1], dir);
                            size = Form1.Manager.Size(texID);
                            texture_name = parts[1];
                        }
                        if (parts[0].Equals("numvert"))
                        {
                            int nverts = int.Parse(parts[1]);

                            for (int ii = 0; ii < nverts; ii++)
                            {
                                line = reader.ReadLine();
                                parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                                Vector3 v = new Vector3();
                                v.X = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                                v.Y = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                v.Z = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                                vertices.Add(v);

                            }
                        }
                        if (parts[0].Equals("numsurf"))
                        {
                            int nsurf = int.Parse(parts[1]);
                            for (int ii = 0; ii < nsurf; ii++)
                            {
                                line = reader.ReadLine();   // skip SURF

                                line = reader.ReadLine();   // mat
                                parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                                int mat = int.Parse(parts[1]);

                                line = reader.ReadLine();   // refs
                                parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                                int refs = int.Parse(parts[1]);

                                switch (refs)
                                {
                                    case 2:
                                        {
                                            line = reader.ReadLine();
                                            line = reader.ReadLine();
                                        }
                                        break;
                                    case 3:
                                        {
                                            AC3DTriangle t = new AC3DTriangle();

                                            for (int j = 0; j < 3; j++)
                                            {
                                                line = reader.ReadLine();
                                                parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                                                ushort us = ushort.Parse(parts[0]);
                                                float u = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                                float v = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);

                                                t.UV[j] = new Vector2(u, v);
                                                t.Position[j] = vertices[us];
                                                t.Material = mat;
                                                tris.Add(t);
                                            }
                                        }
                                        break;
                                    case 4:
                                        {
                                            AC3DTriangle t = new AC3DTriangle();
                                            AC3DTriangle t2 = new AC3DTriangle();
                                            Vector2[] luvs = new Vector2[4];
                                            ushort[] inf = new ushort[4];
                                            for (int j = 0; j < 4; j++)
                                            {
                                                line = reader.ReadLine();
                                                parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                                                inf[j] = ushort.Parse(parts[0]);
                                                float u = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                                float v = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);

                                                luvs[j].X = u;
                                                luvs[j].Y = v;

                                            }
                                            t.Material = mat;
                                            t.Position[0] = vertices[inf[0]];
                                            t.UV[0] = luvs[0];
                                            t.Position[1] = vertices[inf[1]];
                                            t.UV[1] = luvs[1];
                                            t.Position[2] = vertices[inf[2]];
                                            t.UV[2] = luvs[2];
                                            tris.Add(t);

                                            t2.Material = mat;
                                            t2.Position[0] = vertices[inf[0]];
                                            t2.UV[0] = luvs[0];
                                            t2.Position[1] = vertices[inf[2]];
                                            t2.UV[1] = luvs[2];
                                            t2.Position[2] = vertices[inf[3]];
                                            t2.UV[2] = luvs[3];
                                            tris.Add(t2);

                                        }
                                        break;
                                    default:
                                        {
                                            List<ushort> indices = new List<ushort>();
                                            List<Vector2> coords = new List<Vector2>();
                                            for (int r = 0; r < refs; r++)
                                            {

                                                line = reader.ReadLine();
                                                parts = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                                                indices.Add(ushort.Parse(parts[0]));
                                                float u = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                                float v = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                                                coords.Add(new Vector2(u, v));
                                            }
                                            for (int r = 2; r < refs; r++)
                                            {
                                                AC3DTriangle t = new AC3DTriangle();
                                                t.Material = mat;
                                                t.Position[0] = vertices[indices[r - 2]];
                                                t.UV[0] = coords[r - 2];
                                                t.Position[1] = vertices[indices[r - 1]];
                                                t.UV[1] = coords[r - 1];
                                                t.Position[2] = vertices[indices[r]];
                                                t.UV[2] = coords[r];
                                                tris.Add(t);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    MeshNode mn = new MeshNode(cur.Name);
                    mn.mesh.mesh_name = "Mesh_" + cur.Name;
                    mn.mesh.Materials.Clear();
                    mn.mesh.LodDistances.Add(150000);
                    for (int ii = 0; ii < materials.Count; ii++)
                    {
                        mn.mesh.Materials.Add(new Material(materials[ii]));
                        mn.mesh.Materials[ii].TextureID = texID;
                        mn.mesh.Materials[ii].tname = texture_name;
                    }

                    vertices.Clear();
                    uvs.Clear();
                    foreach (AC3DTriangle t in tris)
                    {
                        for (int ii = 0; ii < 3; ii++)
                        {
                            facegroups[t.Material].Add((ushort)vertices.Count);
                            vertices.Add(t.Position[ii]);
                            uvs.Add(t.UV[ii]);
                        }
                    }

                    int cmat = 0;
                    int cface = 0;
                    int icount = 0;
                    foreach (List<ushort> ll in facegroups)
                    {
                        icount += ll.Count;
                    }
                    mn.mesh.indices = new short[icount];
                    icount = 0;
                    foreach (List<ushort> ll in facegroups)
                    {
                        FaceGroup f = new FaceGroup();
                        f.FaceCount = ll.Count / 3;
                        f.StartFace = cface;
                        f.StartVertex = 0;
                        f.VertexCount = vertices.Count;
                        f.Material = cmat;
                        mn.mesh.FaceGroups.Add(f);

                        cmat++;
                        cface += ll.Count / 3;
                        for (int l = 0; l < ll.Count; l++)
                        {
                            mn.mesh.indices[icount++] = (short)ll[l];
                        }
                    }
                    mn.mesh.VertexCount = vertices.Count;
                    mn.mesh.FaceCount = tris.Count;

                    #region Generate normals
                    Vector3[] normals = new Vector3[vertices.Count];
                    Vector3[] temps = new Vector3[mn.mesh.indices.Length];
                    for (int ii = 0; ii < mn.mesh.indices.Length; ii += 3)
                    {

                        Vector3 v1 = vertices[mn.mesh.indices[ii + 0]] - vertices[mn.mesh.indices[ii + 1]];
                        Vector3 v2 = vertices[mn.mesh.indices[ii + 1]] - vertices[mn.mesh.indices[ii + 2]];

                        normals[mn.mesh.indices[ii]] = Vector3.Cross(v1, v2);
                        normals[mn.mesh.indices[ii + 1]] = normals[mn.mesh.indices[ii]];
                        normals[mn.mesh.indices[ii + 2]] = normals[mn.mesh.indices[ii]];
                    }

                    #endregion

                    mn.mesh.Verts = new VertexPositionNormalTexture[vertices.Count];
                    for (int ii = 0; ii < vertices.Count; ii++)
                    {
                        mn.mesh.Verts[ii].Position = vertices[ii];
                        mn.mesh.Verts[ii].Normal = normals[ii];
                        mn.mesh.Verts[ii].TextureCoordinate = uvs[ii];
                    }
                    cur.children.Add(mn);
                    int nkids = int.Parse(parts[1]);
                    if (nkids > 0)
                    {
                        LoadACObject(reader, cur, nkids, materials, dir);
                    }
                }
                else
                {
                    throw new Exception("Malformed AC file");
                }
            }
        }
        #endregion

        #region Binormals and tangents
        public void GenerateBinormalsAndTangents()
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.GenerateBinormalsAndTangents();
                }
                foreach (Node n2 in n.children)
                {
                    GenerateBinormalsAndTangents(n2);
                }
            }
        }

        private void GenerateBinormalsAndTangents(Node n)
        {
            if (n is MeshNode)
            {
                MeshNode mn = (MeshNode)n;
                mn.GenerateBinormalsAndTangents();
            }
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    mn.GenerateBinormalsAndTangents();
                }
                foreach (Node n3 in n2.children)
                {
                    GenerateBinormalsAndTangents(n3);
                }
            }
        }

        public void EnableBumpMapping(int i, int t)
        {
            foreach (Node n in root.children)
            {
                if (n is MeshNode)
                {
                    MeshNode mn = (MeshNode)n;
                    mn.EnableBumpMapping(i, t);
                }
                foreach (Node n2 in n.children)
                {
                    EnableBumpMapping(n2, i, t);
                }
            }
        }
        public void EnableBumpMapping(Node n, int i, int t)
        {
            if (n is MeshNode)
            {
                MeshNode mn = (MeshNode)n;
                mn.EnableBumpMapping(i, t);
            }
            foreach (Node n2 in n.children)
            {
                if (n2 is MeshNode)
                {
                    MeshNode mn = (MeshNode)n2;
                    mn.EnableBumpMapping(i, t);
                }
                foreach (Node n3 in n2.children)
                {
                    EnableBumpMapping(n3, i, t);
                }
            }
        }
        #endregion
    }
}
