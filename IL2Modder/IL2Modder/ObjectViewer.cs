using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IL2Modder.IL2;
using IL2Modder.Particles;
using IL2Modder.Lighting;

namespace IL2Modder
{
    public struct Billboard
    {
        public const float BILLBOARD_SIZE = 128.0f;

        public Texture2D texture;
        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;
    }

    public class ObjectViewer : GraphicsDeviceControl
    {
        public static ObjectViewer Instance;
        public Vector3 eye;
        public Vector3 up;
        public Vector3 forward;
        public Vector3 lightdirection;
        public float ViewDistance;
        public HIM mesh;
        public Effect my_effect;
        public Effect cubemap;
        public Effect glass;
        public Effect shadow;
        public Effect lambert;
        public Effect particle;
        public Effect pointlight;
        public Effect billboard;
        public Effect bumpmap;
        public Effect multipass;
        public TextureCube skybox;
        public Texture2D fields;
        public Texture2D smoke;
        public Texture2D fire;
        public Texture2D shoot;
        public Texture2D cartridge;
        public Texture2D glow;
        public Texture2D ocean1;
        public Texture2D ocean2;
        public Texture2D ocean3;
        public Texture2D ocean4;
        public Texture2D smokeglow;
        public Texture2D grounddust;
        public Effect ocean_shader;
        public Model bomb;
        public Model bomb100;
        public Model tank;
        public Model rocket;
        public static Model cube;
        public static Model cube2;
        public WeaponLoadoutArray WeaponLoadouts;
        public Dictionary<String, String> Weapons = new Dictionary<string, string>();
        public int SelectedWeaponLoadout = -1;
        public static SpriteFont font;
        public const int MaxLights = 5;

        LightingManager lightManager = null;

        ShadowMap shadow_map;
        RenderTarget2D screenshot;
        public RenderTarget2D ao_target;
        public RenderTarget2D ao_worker;

        public Skybox sky = new Skybox();
        ParticleSystem engine_smoke = null;
        ParticleSystem fire_system = null;
        ParticleSystem smoke_system = null;
        ParticleSystem gun_system = null;
        ParticleSystem dump_system = null;
        ParticleSystem oil_leak = null;
        ParticleSystem bullet_system = null;
        ParticleSystem afterburner_system = null;
        ParticleSystem booster_system = null;
        public static ParticleSystem shoot_system = null;

        ParticleSettings engine_smoke_settings = new ParticleSettings();
        ParticleSettings fire_settings = new ParticleSettings();
        ParticleSettings smoke_settings = new ParticleSettings();
        ParticleSettings hit_settings = new ParticleSettings();
        ParticleSettings gun_settings = new ParticleSettings();
        ParticleSettings dump_settings = new ParticleSettings();
        ParticleSettings oil_settings = new ParticleSettings();
        ParticleSettings bullets = new ParticleSettings();
        ParticleSettings afterburner = new ParticleSettings();
        ParticleSettings booster = new ParticleSettings();


        BasicEffect ground_effect;
        BasicEffect effect;
        BasicEffect collision;
        public static SpriteBatch batch;

        bool SkyBoxActive = false;
        bool GroundPlaneActive = false;
        bool ShadowsActive = false;
        bool EngineSmokeActive = false;
        public static bool EngineFireActive = false;
        bool CollisionMeshDraw = false;
        bool FuelLeakActive = false;
        bool FuelFireActive = false;
        bool GunsActive = false;
        bool DropTanksActive = false;
        bool RocketsActive = false;
        bool BayActive = false;
        bool DrawNormals = false;
        bool DrawOcean = false;
        bool DrawFog = false;
        bool AfterburnerActive = false;
        bool BoosterActive = false;
        public bool ShipMode = false;
        public static bool Culling = false;
        public static bool BumpMapping = false;

        public static bool ShaderMode = true;
        public static bool GearActive = false;
        public static bool WingFoldActive = false;
        public static bool AirBrakeActive = false;
        public static bool Turrets = false;
        public static float AirBrake = 0;
        public static float GearAngle;
        public static float WingFoldAngle = 0;
        public static float BayAngle;
        public static float TurretAngle = 0;
        public static float TurretDirection = 0;
        public static float BumpStrength = 2;
        public static bool CockpitMode = false;
        public static bool Explode = false;
        public static float FlapAngle = 0;
        public static bool FlapsActive = false;
        public static bool AnimateCockpit = false;
        public static bool AnimateArrestor = false;
        public static float DoorAngle = 0;
        public static bool HookDisplay = false;
        public static bool ColMeshOnly = false;
        public static Matrix mView;
        public static Matrix mProjection;
        public static Vector3 curEye;
        public static Ray? ray;
        public static bool NightMode = false;
        public static String hitmesh = "";

        float time;
        float time2;
        float runtime;
        int frame = 0;

        Vector3 light_direction = new Vector3(0, 0, 1);
        List<Vector3> EngineSmokeHooks = new List<Vector3>();
        List<Vector3> FireHooks = new List<Vector3>();
        List<Vector3> AfterburnerHooks = new List<Vector3>();
        List<Vector3> BoosterHooks = new List<Vector3>();
        List<Vector3> GunHooks = new List<Vector3>();
        List<Vector3> GunDumpHooks = new List<Vector3>();
        List<Vector3> MountBombs = new List<Vector3>();
        List<Vector3> FuelLeaks = new List<Vector3>();
        List<Vector3> FuelFire = new List<Vector3>();
        List<Vector3> DropTanks = new List<Vector3>();
        List<Vector3> Rockets = new List<Vector3>();
        List<Vector3> Lights = new List<Vector3>();
        List<Vector3> InternalGuns = new List<Vector3>();
        List<Vector3> BulletDirections = new List<Vector3>();
        Dictionary<String, Mesh> WeaponMeshes = new Dictionary<string, Mesh>();

        public List<Vector3> BombHooks = new List<Vector3>();
        public static List<Vector3> BailHooks = new List<Vector3>();
        public static int pilot_count;
        public int bomb_count;
        Ocean ocean;
        int mounted_bombs = 0;
        LineDrawer ld;
        Vector4[] colours = new Vector4[ObjectViewer.MaxLights];
        Vector3[] positions = new Vector3[ObjectViewer.MaxLights];
        Vector3 Target = Vector3.Zero;
        private Billboard _billboard;

        public static FlightVariables fv = new FlightVariables();
        public static List<String> saved_materials = new List<string>();
        public static TextWriter material_writer;

        protected override void Initialize()
        {
            if (!IsDesignerHosted2)
            {
                eye = new Vector3(0, 0, -25);
                up = new Vector3(0, 1, 0);

                effect = new BasicEffect(GraphicsDevice);
                batch = new SpriteBatch(GraphicsDevice);
                collision = new BasicEffect(GraphicsDevice);
                collision.TextureEnabled = false;
                collision.VertexColorEnabled = true;

                mesh = null;
                ViewDistance = 0;
                my_effect = null;
                shadow_map = new ShadowMap(GraphicsDevice);

                #region Setup particle systems
                engine_smoke_settings.Duration = TimeSpan.FromSeconds(9);

                smoke_settings.Duration = TimeSpan.FromSeconds(12);
                smoke_settings.DurationRandomness = 0.2f;
                smoke_settings.MinHorizontalVelocity = -0.1f;
                smoke_settings.MaxHorizontalVelocity = 0.1f;
                smoke_settings.MaxEndSize = 4.6f;
                smoke_settings.MaxParticles = 1500;
                smoke_settings.EndVelocity = 1.0f;

                fire_settings.Duration = TimeSpan.FromSeconds(1);
                fire_settings.MaxRotateSpeed = 18;
                fire_settings.MinRotateSpeed = -18;
                fire_settings.MinStartSize = 0.2f;
                fire_settings.MaxStartSize = 1.5f;
                fire_settings.MaxHorizontalVelocity = 0.05f;
                fire_settings.MinHorizontalVelocity = -0.05f;
                fire_settings.MaxEndSize = 1.1f;
                fire_settings.BlendState = BlendState.AlphaBlend;

                hit_settings.Duration = TimeSpan.FromSeconds(1);
                hit_settings.MaxRotateSpeed = 18;
                hit_settings.MinRotateSpeed = -18;
                hit_settings.MinStartSize = 1.2f;
                hit_settings.MaxStartSize = 1.5f;
                hit_settings.MaxHorizontalVelocity = 0.05f;
                hit_settings.MinHorizontalVelocity = -0.05f;
                hit_settings.MaxEndSize = 2.1f;
                hit_settings.BlendState = BlendState.AlphaBlend;

                gun_settings.Duration = TimeSpan.FromSeconds(0.5);
                gun_settings.MaxRotateSpeed = 28;
                gun_settings.MinRotateSpeed = -28;
                gun_settings.MinStartSize = 0.2f;
                gun_settings.MaxStartSize = 0.5f;
                gun_settings.MaxHorizontalVelocity = 0.05f;
                gun_settings.MinHorizontalVelocity = -0.05f;
                gun_settings.MaxEndSize = 0.1f;
                gun_settings.BlendState = BlendState.AlphaBlend;

                dump_settings.Duration = TimeSpan.FromSeconds(4.5);
                dump_settings.MaxRotateSpeed = 28;
                dump_settings.MinRotateSpeed = -28;
                dump_settings.MinStartSize = 0.2f;
                dump_settings.MaxStartSize = 0.5f;
                dump_settings.MaxHorizontalVelocity = 0.05f;
                dump_settings.MinHorizontalVelocity = -0.05f;
                dump_settings.MaxVerticalVelocity = 0.1f;
                dump_settings.MinVerticalVelocity = -0.1f;
                dump_settings.MaxEndSize = 0.1f;
                dump_settings.BlendState = BlendState.AlphaBlend;
                dump_settings.MaxParticles = 600;

                oil_settings.Duration = TimeSpan.FromSeconds(6);
                oil_settings.DurationRandomness = 0.2f;
                oil_settings.MinHorizontalVelocity = -0.1f;
                oil_settings.MaxHorizontalVelocity = 0.1f;
                oil_settings.MinStartSize = 0.2f;
                oil_settings.MaxStartSize = 0.3f;
                oil_settings.MaxEndSize = 0.6f;
                oil_settings.EndVelocity = 1.0f;
                oil_settings.MinColor = Color.Black;
                oil_settings.MaxColor = Color.Gray;
                oil_settings.MaxParticles = 1500;

                bullets.Duration = TimeSpan.FromSeconds(0.25f);
                bullets.DurationRandomness = 0.02f;
                bullets.MinHorizontalVelocity = 0.1f;
                bullets.MaxHorizontalVelocity = 0.1f;
                bullets.MinStartSize = 1.2f;
                bullets.MaxStartSize = 1.3f;
                bullets.MaxEndSize = 1.6f;
                bullets.MaxParticles = 32;
                bullets.EndVelocity = 1.0f;
                bullets.MinColor = Color.FromNonPremultiplied(160, 100, 100, 255);
                bullets.MaxColor = Color.FromNonPremultiplied(255, 255, 255, 128);


                afterburner.Duration = TimeSpan.FromSeconds(0.85);
                afterburner.DurationRandomness = 0.22f;
                afterburner.MinHorizontalVelocity = -0.1f;
                afterburner.MaxHorizontalVelocity = 0.1f;
                afterburner.MaxStartSize = 1.26f;
                afterburner.MinStartSize = 1.15f;
                afterburner.MaxEndSize = 0.0f;
                afterburner.MaxParticles = 1432;
                afterburner.EndVelocity = 1.0f;
                //afterburner.MinColor = Color.FromNonPremultiplied(24, 50, 255, 128);
                // afterburner.MaxColor = Color.FromNonPremultiplied(24, 50, 120, 32);
                afterburner.MinColor = Color.FromNonPremultiplied(128, 50, 25, 128);
                afterburner.MaxColor = Color.FromNonPremultiplied(255, 128, 0, 32);
                afterburner.BlendState = BlendState.Additive;

                booster.Duration = TimeSpan.FromSeconds(1.85);
                booster.DurationRandomness = 0.12f;
                booster.MinHorizontalVelocity = -0.1f;
                booster.MaxHorizontalVelocity = 0.1f;
                booster.MaxStartSize = 1.2f;
                booster.MinStartSize = 0.6f;
                booster.MaxEndSize = 0.0f;
                booster.MaxParticles = 1432;
                booster.EndVelocity = 1.0f;
                booster.MinColor = Color.FromNonPremultiplied(200, 190, 190, 64);
                booster.MaxColor = Color.FromNonPremultiplied(160, 100, 100, 16);
                booster.BlendState = BlendState.Additive;
                #endregion

                time = 0;
                time2 = 0;
                bomb_count = 0;
                GearAngle = 0;
                BayAngle = 0;

                colours[0] = Color.White.ToVector4();
                colours[1] = Color.Green.ToVector4();
                colours[2] = Color.White.ToVector4();
                colours[3] = Color.Red.ToVector4();

                positions[0] = new Vector3(10, 0, 0);
                positions[1] = new Vector3(0, 10, 0);
                positions[2] = new Vector3(0, 0, 10);
                positions[3] = new Vector3(0, -10, 0);

                screenshot = new RenderTarget2D(GraphicsDevice, this.Width, this.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
                ao_target = new RenderTarget2D(GraphicsDevice, 1024, 1024, false, SurfaceFormat.Color, DepthFormat.Depth16);
                ao_worker = new RenderTarget2D(GraphicsDevice, 128, 128, false, SurfaceFormat.Color, DepthFormat.None);

                ocean = null;

                WeaponLoadouts = null;
                ObjectViewer.ray = null;
                ld = new LineDrawer(GraphicsDevice);
                
                Instance = this;
                Application.Idle += delegate { Invalidate(); };
            }
        }

        public bool IsDesignerHosted2
        {
            get
            {
                Control ctrl = this;

                while (ctrl != null)
                {
                    if ((ctrl.Site != null) && ctrl.Site.DesignMode)
                        return true;
                    ctrl = ctrl.Parent;
                }
                return false;
            }
        }

        #region Particle systems
        public void AddGunShot(Vector3 location, Vector3 direction)
        {
            if (gun_system == null)
            {
                gun_system = new ParticleSystem(gun_settings, particle, shoot);
            }

            gun_system.AddParticle(location, direction);
        }
        public void AddLargeGunShot(Vector3 location, Vector3 direction)
        {
            if (fire_system == null)
            {
                fire_system = new ParticleSystem(fire_settings, particle, fire);
            }

            fire_system.AddParticle(location, direction);
        }
        public void AddSmoke(Vector3 location, Vector3 direction)
        {
            if (engine_smoke == null)
            {
                engine_smoke = new ParticleSystem(engine_smoke_settings, particle, smoke);
            }
            engine_smoke.AddParticle(location, direction);
        }
        #endregion

        #region Tidy up
        protected override void Dispose(bool disposing)
        {
            screenshot.Dispose();
            base.Dispose(disposing);
        }

        public void Clear()
        {
            EngineSmokeHooks.Clear();
            FireHooks.Clear();
            GunHooks.Clear();
            GunDumpHooks.Clear();
            MountBombs.Clear();
            BombHooks.Clear();
            BailHooks.Clear();
            FuelLeaks.Clear();
            FuelFire.Clear();
            DropTanks.Clear();
            Rockets.Clear();

            mounted_bombs = 0;
            bomb_count = 0;

            EngineSmokeActive = false;
            EngineFireActive = false;
            FuelLeakActive = false;
            FuelFireActive = false;
            DropTanksActive = false;
            RocketsActive = false;
            if (WeaponLoadouts != null)
                WeaponLoadouts.Loads.Clear();
            SelectedWeaponLoadout = -1;
        }
        #endregion

        #region Get sets
        public float EyeX
        {
            get { return eye.X; }
            set { eye.X = value; }
        }
        public float EyeY
        {
            get { return eye.Y; }
            set { eye.Y = value; }
        }
        public float EyeZ
        {
            get { return eye.Z; }
            set { eye.Z = value; }
        }
        public Vector3 Eye
        {
            get { return eye; }
            set { eye = value; }
        }
        public Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }
        public void SetBumpStrength(float f)
        {
            BumpStrength = f;
            bumpmap.Parameters["BumpConstant"].SetValue(f);
        }
        public void SetTarget(Vector3 target)
        {
            Target = target;
        }
        #endregion

        #region Enable/Disable

        #region Hook display
        public void ToggleHooks()
        {
            if (ObjectViewer.HookDisplay)
            {
                ObjectViewer.HookDisplay = false;
            }
            else
            {
                ObjectViewer.HookDisplay = true;
            }
        }
        #endregion

        #region Sky box
        public void EnableSkybox()
        {
            sky.skyBox = cube;
            sky.skyBoxEffect = cubemap;
            sky.skyBoxTexture = skybox;
            SkyBoxActive = true;
        }
        public void DisableSkybox()
        {
            SkyBoxActive = false;
        }
        #endregion

        #region Ground plane
        public void EnableGroundPlane()
        {
            GroundPlaneActive = true;
            ground_effect = new BasicEffect(Form1.graphics);
        }
        public void DisableGroundPlane()
        {
            GroundPlaneActive = false;
        }
        #endregion

        #region Shadows
        public void EnableShadows()
        {
            ShadowsActive = true;
        }
        public void DisableShadows()
        {
            ShadowsActive = false;
        }
        #endregion

        #region Engine smoke
        public void EnableEngineSmoke()
        {
            EngineSmokeActive = true;
            if (mesh != null)
            {
                EngineSmokeHooks.Clear();
                mesh.FindHook("Smoke", ref EngineSmokeHooks);
                if (engine_smoke == null)
                {
                    engine_smoke = new ParticleSystem(engine_smoke_settings, particle, smoke);
                }
                time2 = 0;
            }
        }
        public void DisableEngineSmoke()
        {
            EngineSmokeActive = false;
        }
        #endregion

        #region Afterburner smoke
        public void EnableAfterburner()
        {
            AfterburnerActive = true;
            if (mesh != null)
            {
                AfterburnerHooks.Clear();
                mesh.FindHook("Engine3EF", ref AfterburnerHooks);
                mesh.FindHook("Engine1EF", ref AfterburnerHooks);
                mesh.FindHook("Engine2EF", ref AfterburnerHooks);
                mesh.FindHook("Engine1ES", ref AfterburnerHooks);
                mesh.FindHook("Engine2ES", ref AfterburnerHooks);
                mesh.FindHook("Engine3ES", ref AfterburnerHooks);
                if (afterburner_system == null)
                {
                    afterburner_system = new ParticleSystem(afterburner, particle, smokeglow);
                }
            }
        }
        public void DisableAfterburner()
        {
            AfterburnerActive = false;
        }
        #endregion

        #region Boosters
        public void EnableBoosters()
        {
            BoosterActive = true;
            if (mesh != null)
            {
                BoosterHooks.Clear();
                mesh.FindHook("Booster", ref BoosterHooks);
                if (booster_system == null)
                {
                    booster_system = new ParticleSystem(booster, particle, grounddust);
                }
            }
        }
        public void DisableBoosters()
        {
            BoosterActive = false;
        }
        #endregion

        #region Guns
        public void ToggleGuns()
        {
            if (GunsActive)
                GunsActive = false;
            else
            {
                if (GunHooks.Count == 0)
                {
                    if (mesh != null)
                    {
                        mesh.FindHook("MGUN", "Dump", ref GunHooks);
                        mesh.FindHook("MGUNDump", ref GunDumpHooks);
                        mesh.FindHook("CANNON", "Dump", ref GunHooks);
                        mesh.FindHook("CANNONDump", ref GunDumpHooks);
                    }
                }
                if (gun_system == null)
                {
                    dump_system = new ParticleSystem(dump_settings, particle, cartridge);
                    gun_system = new ParticleSystem(gun_settings, particle, shoot);
                    bullet_system = new ParticleSystem(bullets, particle, smoke);
                }
                GunsActive = true;
            }
        }
        #endregion

        #region Engine fires
        public void EnableEngineFire(int engines)
        {
            EngineFireActive = true;
            FireHooks.Clear();
            if (smoke_system == null)
            {
                smoke_system = new ParticleSystem(smoke_settings, particle, smoke);
            }
            if (fire_system == null)
            {
                fire_system = new ParticleSystem(fire_settings, particle, fire);
            }

            for (int i = 0; i < engines; i++)
            {
                String search = String.Format("Engine{0}EF", i + 1);
                mesh.FindHook(search, ref FireHooks);
            }
            time = 0;
        }
        public void DisableEngineFire()
        {
            EngineFireActive = false;
        }
        #endregion

        #region Collision mesh
        public void ToggleCollision()
        {
            if (CollisionMeshDraw)
            {
                CollisionMeshDraw = false;
                hitmesh = "";
            }
            else
                CollisionMeshDraw = true;
        }
        #endregion

        #region Shoot system
        public void EnableShooting()
        {
            if (shoot_system == null)
            {
                shoot_system = new ParticleSystem(hit_settings, particle, shoot);
            }
        }
        #endregion

        #region Gear
        public void ToggleGear()
        {
            if (GearActive)
                GearActive = false;
            else
            {
                GearActive = true;
                GearAngle = 0;
            }
        }
        #endregion

        #region mount bombs
        public void AddBomb()
        {
            mounted_bombs++;
            if (mounted_bombs > MountBombs.Count)
                mounted_bombs = 0;
        }
        #endregion

        #region Fuel leaks
        public void ToggleFuelLeak()
        {
            if (FuelLeakActive)
                FuelLeakActive = false;
            else
            {
                FuelLeakActive = true;
                mesh.FindHook("Leak", ref FuelLeaks);
                if (oil_leak == null)
                {
                    oil_leak = new ParticleSystem(oil_settings, particle, smoke);
                }
            }
        }
        #endregion

        #region Fuel fire
        public void ToggleFuelFire()
        {
            FuelFire.Clear();

            if (FuelFireActive)
                FuelFireActive = false;
            else
            {
                FuelFireActive = true;
                mesh.FindHook("Burn", ref FuelFire);
                if (fire_system == null)
                {
                    fire_system = new ParticleSystem(fire_settings, particle, fire);
                }
                if (smoke_system == null)
                {
                    smoke_system = new ParticleSystem(smoke_settings, particle, smoke);
                }
            }
        }
        #endregion

        #region Drop tanks
        public void ToggleDropTanks()
        {
            DropTanks.Clear();

            if (DropTanksActive)
                DropTanksActive = false;
            else
            {
                DropTanksActive = true;
                if (mesh != null)
                {
                    mesh.FindHook("ExternalDev", ref DropTanks);
                }
            }
        }
        #endregion

        #region Rockets
        public void ToggleRockets()
        {
            Rockets.Clear();

            if (RocketsActive)
                RocketsActive = false;
            else
            {
                RocketsActive = true;
                if (mesh != null)
                {
                    mesh.FindHook("ExternalRock", ref Rockets);
                }
            }
        }
        #endregion

        #region Bomb bays
        public void ToggleBombBays()
        {
            if (BayActive)
                BayActive = false;
            else
            {
                BayActive = true;
                BayAngle = 0;
            }
        }
        #endregion

        #region Wing Fold
        public void ToggleWingFold()
        {
            if (WingFoldActive)
                WingFoldActive = false;
            else
            {
                WingFoldActive = true;
                WingFoldAngle = 0;
            }
        }
        #endregion

        #region Render mode
        public void ToggleRenderMode()
        {
            if (ShaderMode)
                ShaderMode = false;
            else
                ShaderMode = true;
        }
        #endregion

        #region Flaps
        public void ToggleFlaps()
        {
            if (FlapsActive)
            {
                FlapsActive = false;
            }
            else
            {
                FlapsActive = true;
            }
        }
        #endregion

        #region Toggle cockpit animation
        public void ToggleCockpitAnimation()
        {
            if (AnimateCockpit)
            {
                AnimateCockpit = false;
            }
            else
            {
                AnimateCockpit = true;
            }
        }
        #endregion

        #region Day / Night
        public void Night(bool state)
        {
            NightMode = state;
            if (state)
            {
                if (lightManager == null)
                {
                    lightManager = new LightingManager(multipass);
                }
                else
                {
                    lightManager.Clear();
                }

                // Look for any nav lights and add them as point lights.
                for (int li = 0; li < 64; li++)
                {
                    Lights.Clear();
                    mesh.FindHook("NavLight" + li, ref Lights);
                    if (Lights.Count > 0)
                    {
                        switch (li)
                        {
                            case 0:
                                lightManager.AddPointLight(Lights[0], Color.GreenYellow);
                                break;
                            case 2:
                                lightManager.AddPointLight(Lights[0], Color.Red);
                                break;
                            default:
                                lightManager.AddPointLight(Lights[0], Color.White);
                                break;

                        }
                    }
                }
                // Look for any landing lights and add them as spot lights.

                Lights.Clear();
                List<Vector3> dirs = new List<Vector3>();
                mesh.FindHook("landinglight", ref Lights, ref dirs);
                if (Lights.Count > 0)
                {
                    for (int jj = 0; jj < Lights.Count; jj++)
                    {
                        lightManager.AddSpotLight(Lights[jj], dirs[jj], Color.White);
                    }
                }
                Debug.WriteLine(String.Format("Found {0} point lights and {1} spotlights", lightManager.pointLights.Count, lightManager.spotLights.Count));




                for (int i = 0; i < ObjectViewer.MaxLights; i++)
                {
                    colours[i] = Color.Transparent.ToVector4();
                    positions[i] = Vector3.UnitZ * 100000.0f;
                }
                int j = 0;
                for (int i = 0; i < 6; i++)
                {
                    Lights.Clear();
                    mesh.FindHook("NavLight" + i, ref Lights);

                    if (Lights.Count > 0)
                    {
                        positions[j] = Lights[0];
                        switch (i)
                        {
                            case 2:     // left wingtip
                                colours[j] = Color.GreenYellow.ToVector4();
                                break;
                            case 0:     // right wingtip
                                colours[j] = Color.Red.ToVector4();
                                break;
                            default:    // unknown
                                colours[j] = Color.White.ToVector4();
                                break;
                        }
                        j++;
                        if (j == ObjectViewer.MaxLights)
                            return;
                    }

                }
                for (int i = 0; i < 6; i++)
                {
                    mesh.FindHook("LandingLight" + i, ref Lights);

                    if (Lights.Count > 0)
                    {
                        positions[j] = Lights[0];
                        colours[j] = Color.White.ToVector4();
                        j++;
                        if (j == ObjectViewer.MaxLights)
                            return;
                    }

                }
            }
        }
        #endregion

        #region Air brakes
        public void ToggleAirBrakes()
        {
            if (AirBrakeActive)
                AirBrakeActive = false;
            else
            {
                AirBrakeActive = true;
                AirBrake = 0;
            }
        }
        #endregion

        #region Turrets
        public void ToggleTurrets()
        {
            if (ObjectViewer.Turrets)
                ObjectViewer.Turrets = false;
            else
            {
                ObjectViewer.TurretAngle = 0;
                ObjectViewer.TurretDirection = 0.5f;
                ObjectViewer.Turrets = true;
            }
        }
        #endregion

        #region Collision mesh only
        public void ToggleColOnly()
        {
            ColMeshOnly = !ColMeshOnly;
            if (ColMeshOnly)
            {
                CollisionMeshDraw = true;
            }
            else
            {
                CollisionMeshDraw = false;
            }
        }

        #endregion

        #region Draw normals
        public void ToggleNormalDrawing(bool state)
        {
            DrawNormals = state;
        }
        #endregion

        #region Toggle culling
        public void ToggleCulling()
        {
            Culling = !Culling;
        }
        #endregion

        #region Ocean
        public void SetOcean(bool on)
        {
            DrawOcean = on;
            if (on)
            {
                if (ocean == null)
                {
                    ocean = new Ocean();
                    ocean.oceanEffect = ocean_shader;
                    ocean.OceanNormalMaps[0] = ocean1;
                    ocean.OceanNormalMaps[1] = ocean2;
                    ocean.OceanNormalMaps[2] = ocean3;
                    ocean.OceanNormalMaps[3] = ocean4;
                    ground_effect = new BasicEffect(Form1.graphics);
                }
            }
            else
            {
            }
        }
        #endregion

        #region Fog
        public void Fog(bool on)
        {
            DrawFog = on;
            if (on)
            {
                if (horizon == null)
                {
                    BuildHorizon(Form1.graphics, 950);
                }
            }
        }
        #endregion

        #region Arrestor hook
        public void ToggleArrestor()
        {
            if (AnimateArrestor)
                AnimateArrestor = false;
            else
                AnimateArrestor = true;
        }
        #endregion

        #endregion

        #region Draw methods

        public void DrawGroundPlane(Matrix view, Matrix projection)
        {
            RasterizerState stat = new RasterizerState();
            stat.CullMode = CullMode.None;
            Form1.graphics.RasterizerState = stat;


            VertexPositionTexture[] verts = new VertexPositionTexture[]{
                new VertexPositionTexture(new Vector3(-1000,-20,-1000),new Vector2(0,0)),
                new VertexPositionTexture(new Vector3( 1000,-20,-1000),new Vector2(0,1)),
                new VertexPositionTexture(new Vector3(-1000,-20, 1000),new Vector2(1,0)),
                new VertexPositionTexture(new Vector3( 1000,-20, 1000),new Vector2(1,1))

            };
            short[] ind = new short[]{
                0,1,2,
                1,2,3
            };
            ground_effect.View = view;
            ground_effect.World = Matrix.CreateRotationX(MathHelper.PiOver2);
            ground_effect.Projection = projection;
            ground_effect.DiffuseColor = Color.White.ToVector3();
            ground_effect.AmbientLightColor = Color.Gray.ToVector3();
            ground_effect.Texture = fields;
            ground_effect.TextureEnabled = true;
            ground_effect.Alpha = 1.0f;


            Form1.graphics.DepthStencilState = DepthStencilState.Default;

            foreach (EffectPass pass in ground_effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                ground_effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                    PrimitiveType.TriangleList,
                    verts,
                    0,                  // vertex buffer offset to add to each element of the index buffer
                    4,                  // number of vertices to draw
                    ind,
                    0,                  // first index element to read
                    2                   // number of primitives to draw
                    );
            }
        }

        private void DrawShadows(Matrix view, Matrix projection)
        {
            if (ShadowsActive)
            {
                VertexPositionTexture[] sverts = new VertexPositionTexture[]{
                    new VertexPositionTexture(new Vector3(-35,-20,-35),new Vector2(1,0)),
                    new VertexPositionTexture(new Vector3( 35,-20,-35),new Vector2(0,0)),
                    new VertexPositionTexture(new Vector3(-35,-20, 35),new Vector2(1,1)),
                    new VertexPositionTexture(new Vector3( 35,-20, 35),new Vector2(0,1))
            
                };
                short[] sind = new short[]{
                    0,1,2,
                    1,2,3
                };
                Form1.graphics.DepthStencilState = DepthStencilState.None;
                ground_effect.Texture = shadow_map.shadowMapTexture;
                ground_effect.Alpha = 0.5f;
                ground_effect.View = view;
                ground_effect.World = Matrix.CreateRotationX(MathHelper.PiOver2);
                ground_effect.Projection = projection;
                ground_effect.DiffuseColor = Color.White.ToVector3();
                ground_effect.AmbientLightColor = Color.Gray.ToVector3();
                ground_effect.TextureEnabled = true;
                ground_effect.Alpha = 1.0f;
            
                foreach (EffectPass pass in ground_effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    ground_effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                        PrimitiveType.TriangleList,
                        sverts,
                        0,                  // vertex buffer offset to add to each element of the index buffer
                        4,                  // number of vertices to draw
                        sind,
                        0,                  // first index element to read
                        2                   // number of primitives to draw
                        );
                }
            
            }

            
        }

        protected override void Draw()
        {
            if (Form1.Screenshot)
            {
                screenshot = new RenderTarget2D(GraphicsDevice, this.Width, this.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);

                GraphicsDevice.SetRenderTarget(screenshot);
            }

            #region Animate tracks

            #region Animate landing gear
            if ((GearActive) && (GearAngle < 1))
            {
                GearAngle += 0.001f;
            }
            if ((!GearActive) && (GearAngle > 0))
            {
                GearAngle -= 0.001f;
                GearAngle = Math.Max(GearAngle, 0);
            }
            #endregion

            #region Animate bomb bays
            if ((BayActive) && (BayAngle < MathHelper.PiOver2))
            {
                BayAngle += 0.01f;
            }
            if ((!BayActive) && (BayAngle > 0))
            {
                BayAngle -= 0.01f;
                BayAngle = Math.Max(BayAngle, 0);
                if (BayAngle == 0)
                {
                    BayActive = false;
                }
            }
            #endregion

            #region Animate wing fold
            if ((WingFoldActive) && (WingFoldAngle < 1))
            {
                WingFoldAngle += 0.01f;
            }
            if ((!WingFoldActive) && (WingFoldAngle > 0))
            {
                WingFoldAngle -= 0.01f;
                WingFoldAngle = Math.Max(WingFoldAngle, 0);
                if (WingFoldAngle == 0)
                {
                    WingFoldActive = false;
                }
            }
            #endregion

            #region Animate Flaps
            if ((FlapsActive) && (FlapAngle < 60))
            {
                FlapAngle += 1;
            }
            else
            {
                if (FlapAngle > 0)
                {
                    FlapAngle--;
                    FlapAngle = Math.Max(FlapAngle, 0);
                }
            }
            #endregion

            #region Animate cockpit
            if (Form1.Animate)
            {
                fv.Animate();
            }
            #endregion

            #region Animate cockpit door
            if (AnimateCockpit)
            {
                if (DoorAngle < 1)
                    DoorAngle += 0.04f;
            }
            else
            {
                if (DoorAngle > 0)
                    DoorAngle -= 0.04f;
            }
            #endregion

            #region Animate air brakes
            if ((AirBrakeActive) && (AirBrake < 1))
            {
                AirBrake += 0.01f;
            }
            if ((!AirBrakeActive) && (AirBrake > 0))
            {
                AirBrake -= 0.01f;
                AirBrake = Math.Max(AirBrake, 0);
                if (AirBrake == 0)
                {
                    AirBrakeActive = false;
                }
            }
            #endregion

            #region Animate turrets
            if (Turrets)
            {
                TurretAngle += TurretDirection;
                if ((TurretAngle > 180) || (TurretAngle < -180))
                {
                    TurretDirection = -TurretDirection;
                    TurretAngle += TurretDirection;
                }
            }
            #endregion

            #region Animate arrestor hook
            if ((AnimateArrestor) && (Form1.Arrestor < 1))
            {
                Form1.Arrestor += 0.016f;
                Form1.Arrestor = Math.Min(1, Form1.Arrestor);
            }
            if ((!AnimateArrestor) && (Form1.Arrestor > 0))
            {
                Form1.Arrestor -= 0.016f;
                Form1.Arrestor = Math.Max(0, Form1.Arrestor);
            }
            #endregion

            #endregion

            time += 0.016f;
            time2 += 0.016f;
            frame++;
            frame &= 7;
            pilot_count = 0;
            Matrix view = Matrix.Identity;

            if (CockpitMode)
            {
                view = Matrix.CreateLookAt(eye, eye + forward, up);
            }
            else
            {
                view = Matrix.CreateLookAt(eye, Target, up);
            }
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1800.0f);
            ObjectViewer.curEye = eye;
            if ((BailHooks.Count == 0) && (mesh != null))
            {
                mesh.FindHook("ExternalBail", ref BailHooks);
                mesh.FindHook("BombSpawn", ref BombHooks);
                mesh.FindHook("ExternalBomb", ref MountBombs);
            }

            if (ShadowsActive)
            {
                shadow_map.Update(light_direction, view * projection, shadow);
                shadow_map.Begin();
                if (mesh != null)
                {
                    mesh.DrawShadow(shadow, ViewDistance);
                }
                shadow_map.End();
            }
            ObjectViewer.mView = view;
            ObjectViewer.mProjection = projection;

            GraphicsDevice.Clear(Color.Blue);


            if (SkyBoxActive)
            {
                sky.skyBoxTexture = skybox;
                sky.Draw(view, projection, eye);
            }
            if (GroundPlaneActive)
            {
                DrawGroundPlane(view, projection);
                if (ShadowsActive)
                {
                    DrawShadows(view, projection);
                }

            }
            if (DrawOcean)
            {
                ocean.Draw(runtime, Eye, view, skybox, projection, ShipMode);
                if (ShadowsActive)
                {
                    DrawShadows(view, projection);
                }

                runtime += 0.02f;
            }

            if (DrawFog)
            {
                DrawHorizon(projection, view);
            }

            #region Draw mesh
            if ((mesh != null) && (!ColMeshOnly))
            {
                if (ShaderMode)
                {
                    if (NightMode)
                    {
                        pointlight.Parameters["View"].SetValue(view);
                        pointlight.Parameters["Projection"].SetValue(projection);
                        pointlight.Parameters["ViewVector"].SetValue(eye);
                        pointlight.Parameters["diffuseColor"].SetValue(colours);
                        pointlight.Parameters["lightPosition"].SetValue(positions);

                        Form1.graphics.DepthStencilState = DepthStencilState.Default;
                        mesh.Draw(pointlight, ViewDistance, false);
                        mesh.Draw(pointlight, ViewDistance, true);

                    }
                    else
                    {
                        my_effect.Parameters["View"].SetValue(view);
                        my_effect.Parameters["Projection"].SetValue(projection);
                        my_effect.Parameters["ViewVector"].SetValue(eye);
                        my_effect.Parameters["DiffuseLightDirection"].SetValue(lightdirection);

                        Form1.graphics.DepthStencilState = DepthStencilState.Default;
                        mesh.Draw(my_effect, ViewDistance, false);
                        mesh.Draw(my_effect, ViewDistance, true);
                        if (BumpMapping)
                        {
                            bumpmap.Parameters["View"].SetValue(view);
                            bumpmap.Parameters["Projection"].SetValue(projection);
                            bumpmap.Parameters["ViewVector"].SetValue(eye);
                            mesh.DrawBumped(ViewDistance, Matrix.Identity, false);
                            mesh.DrawBumped(ViewDistance, Matrix.Identity, true);
                        }
                    }
                }
                else
                {
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = Matrix.Identity;

                    effect.TextureEnabled = true;

                    Form1.graphics.DepthStencilState = DepthStencilState.Default;
                    mesh.Draw(effect, ViewDistance);

                }

                Form1.graphics.BlendState = BlendState.AlphaBlend;
                Form1.graphics.DepthStencilState = DepthStencilState.DepthRead;

                glass.Parameters["cubeMap"].SetValue(skybox);
                glass.Parameters["EyePosition"].SetValue(eye);
                mesh.DrawGlass(glass, ViewDistance, view * projection);
            }
            #endregion

            #region Engine smoke display
            if (EngineSmokeActive)
            {
                if (time2 > 0.25)
                {
                    time2 -= 0.25f;
                    foreach (Vector3 h in EngineSmokeHooks)
                    {

                        engine_smoke.AddParticle(h, -4.0f * Vector3.UnitX);
                    }
                }

            }
            #endregion

            #region Afterburner
            if (AfterburnerActive)
            {
                foreach (Vector3 h in AfterburnerHooks)
                {
                    afterburner_system.AddParticle(h, -Vector3.UnitX * 6);
                }
            }
            #endregion

            #region Booster
            if (BoosterActive)
            {
                foreach (Vector3 h in BoosterHooks)
                {
                    booster_system.AddParticle(h, -Vector3.UnitX * 6);
                }
            }
            #endregion

            #region Engine fire display
            if (EngineFireActive)
            {
                if (time > 0.125)
                {
                    time -= 0.125f;
                    foreach (Vector3 h in FireHooks)
                    {
                        smoke_system.AddParticle(h, -4.0f * Vector3.UnitX);
                        fire_system.AddParticle(h, -2.4f * Vector3.UnitX);
                    }
                }
            }
            #endregion

            #region Fuel fire display
            if (FuelFireActive)
            {
                if ((frame & 3) == 0)
                {
                    foreach (Vector3 h in FuelFire)
                    {
                        smoke_system.AddParticle(h, -4.0f * Vector3.UnitX);
                        fire_system.AddParticle(h, -2.4f * Vector3.UnitX);
                    }
                }
            }
            #endregion

            #region Guns display
            if (GunsActive)
            {
                foreach (Vector3 v in GunHooks)
                {
                    gun_system.AddParticle(v, Vector3.Zero);

                }
                for (int i = 0; i < InternalGuns.Count; i++)
                {
                    gun_system.AddParticle(InternalGuns[i], Vector3.Zero);
                    bullet_system.AddParticle(InternalGuns[i], -30.0f * BulletDirections[i]);
                }
                if (frame == 0)
                {
                    foreach (Vector3 v in GunDumpHooks)
                    {
                        dump_system.AddParticle(v, new Vector3(-5, 0, -1));
                    }
                }
            }
            #endregion

            #region Oil leak
            if (FuelLeakActive)
            {
                foreach (Vector3 v in FuelLeaks)
                {
                    oil_leak.AddParticle(v, -4.0f * Vector3.UnitX);
                }
            }
            #endregion

            #region Particle systems
            if (fire_system != null)
            {
                fire_system.SetCamera(view, projection);
                fire_system.Update(0.016f);
                fire_system.Draw();
            }
            if (smoke_system != null)
            {
                smoke_system.SetCamera(view, projection);
                smoke_system.Update(0.016f);
                smoke_system.Draw();
            }
            if (engine_smoke != null)
            {
                engine_smoke.SetCamera(view, projection);
                engine_smoke.Update(0.016f);
                engine_smoke.Draw();
            }
            if (shoot_system != null)
            {
                shoot_system.SetCamera(view, projection);
                shoot_system.Update(0.016f);
                shoot_system.Draw();
            }
            if (dump_system != null)
            {
                dump_system.SetCamera(view, projection);
                dump_system.Update(0.008f);
                dump_system.Draw();
            }
            if (gun_system != null)
            {
                gun_system.SetCamera(view, projection);
                gun_system.Update(0.016f);
                gun_system.Draw();
            }
            if (oil_leak != null)
            {
                oil_leak.SetCamera(view, projection);
                oil_leak.Update(0.016f);
                oil_leak.Draw();
            }
            if (bullet_system != null)
            {
                bullet_system.SetCamera(view, projection);
                bullet_system.Update(0.016f);
                bullet_system.Draw();
            }
            if (afterburner_system != null)
            {
                afterburner_system.SetCamera(view, projection);
                afterburner_system.Update(0.016f);
                afterburner_system.Draw();
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
            }
            if (booster_system != null)
            {
                booster_system.SetCamera(view, projection);
                booster_system.Update(0.016f);
                booster_system.Draw();
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
            }
            #endregion

            #region Bombs
            if (bomb_count > 0)
            {
                Form1.graphics.DepthStencilState = DepthStencilState.Default;
                for (int i = 0; i < bomb_count; i++)
                {
                    DrawModel(bomb, view, projection, 1.0f, 0, BombHooks[i]);
                }
            }
            if (mounted_bombs > 0)
            {
                Form1.graphics.DepthStencilState = DepthStencilState.Default;
                for (int i = 0; i < mounted_bombs; i++)
                {
                    DrawModel(bomb100, view, projection, 0.75f, 0, MountBombs[i]);
                }
            }
            #endregion

            #region Drop tanks
            if (DropTanksActive)
            {
                Form1.graphics.DepthStencilState = DepthStencilState.Default;
                foreach (Vector3 v in DropTanks)
                {
                    DrawModel(tank, view, projection, 1.00f, 0, v);
                }
            }
            #endregion

            #region Rockets
            if (RocketsActive)
            {
                Form1.graphics.DepthStencilState = DepthStencilState.Default;
                foreach (Vector3 v in Rockets)
                {
                    DrawModel(rocket, view, projection, 1.00f, 0, v);
                }
            }
            #endregion

            #region Collision mesh
            if ((CollisionMeshDraw) && (mesh != null))
            {
                Form1.graphics.DepthStencilState = DepthStencilState.Default;
                RasterizerState originalState = Form1.graphics.RasterizerState;
                RasterizerState rasterizerState = new RasterizerState();
                rasterizerState.FillMode = FillMode.WireFrame;
                Form1.graphics.RasterizerState = rasterizerState;
                collision.View = view;
                collision.Projection = projection;
                mesh.DrawCollisionMesh(collision);
                Form1.graphics.RasterizerState = originalState;
            }
            #endregion

            #region Weapons
            if (SelectedWeaponLoadout >= 0)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                foreach (WeaponSlot ws in WeaponLoadouts.Loads[SelectedWeaponLoadout].Weapons)
                {
                    if ((WeaponLoadouts.Hooks[ws.Slot].StartsWith("_Ext")) || (WeaponLoadouts.Hooks[ws.Slot].Contains("BombSpawn")))
                    {
                        if (ws.WeaponName.Contains("Null"))
                        {
                        }
                        else
                        {
                            Mesh nmesh = WeaponMeshes[ws.WeaponName];
                            effect.View = view;
                            effect.Projection = projection;
                            effect.World = Matrix.Identity;
                            effect.TextureEnabled = true;

                            List<Vector3> hl = new List<Vector3>();
                            mesh.FindHook(WeaponLoadouts.Hooks[ws.Slot], ref hl);
                            Form1.graphics.DepthStencilState = DepthStencilState.Default;
                            nmesh.Draw(effect, ViewDistance, Matrix.CreateTranslation(hl[0]));
                        }
                    }
                }
            }
            #endregion

            #region DrawNormals
            if (DrawNormals)
            {
                BasicEffect teffect = new BasicEffect(GraphicsDevice);
                teffect.Projection = projection;
                teffect.World = Matrix.Identity;
                teffect.View = view;
                teffect.TextureEnabled = false;
                teffect.VertexColorEnabled = true;
                teffect.AmbientLightColor = Color.White.ToVector3();
                teffect.DiffuseColor = Color.White.ToVector3();
                teffect.Alpha = 1;
                teffect.LightingEnabled = false;
                mesh.DrawNormals(teffect);
            }
            #endregion


            if (NightMode)
            {
                DrawFlare(view, projection);
            }
            if (hitmesh != "")
            {
                batch.Begin();
                batch.DrawString(font, hitmesh, Vector2.UnitX * 10, Color.White);
                batch.End();
            }



            ObjectViewer.ray = null;

            if (Form1.Screenshot)
            {
                GraphicsDevice.SetRenderTarget(null); // finished with render target
                int i = 0;
                String fname;
                do
                {
                    i++;
                    fname = Form1.ScreenShotDirectory + "\\" + Form1.ShotName + String.Format("_{0}.png", i);
                } while (File.Exists(fname));

                using (FileStream fs = new FileStream(fname, FileMode.OpenOrCreate))
                {
                    screenshot.SaveAsPng(fs, screenshot.Width, screenshot.Height); // save render target to disk
                }
                Form1.Screenshot = false;
            }

        }

        private void DrawShadowMap()
        {
            Rectangle rect = new Rectangle();

            rect.X = 0;
            rect.Y = GraphicsDevice.Viewport.Height - 256;
            rect.Width = 256;
            rect.Height = 256;

            ObjectViewer.batch.Begin();
            ObjectViewer.batch.Draw(shadow_map.shadowMapTexture, rect, Color.White);
            ObjectViewer.batch.End();
        }

        private void DrawModel(Model mod, Matrix view, Matrix projection, float scale, float rotation, Vector3 position)
        {
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[mod.Bones.Count];
            mod.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in mod.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index]
                        * Matrix.CreateScale(scale)
                        * Matrix.CreateRotationY(rotation)
                        //* Matrix.CreateRotationZ(MathHelper.PiOver2)
                        * Matrix.CreateTranslation(position);
                    effect.View = view;
                    effect.Projection = projection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }

        private void DrawFlare(Matrix View, Matrix Projection)
        {
            Vector3 zAxis = new Vector3(View.M13, View.M23, View.M33);
            Vector3 viewDir;
            Vector3.Negate(ref zAxis, out viewDir);
            for (int i = 0; i < ObjectViewer.MaxLights; i++)
            {
                InitBillboard(1, 1, positions[i], Color.FromNonPremultiplied(colours[i]));
                DrawBillboard(viewDir, View, Projection);
            }
        }

        private void InitBillboard(float width, float height, Vector3 position, Color color)
        {
            float x = width * 0.5f;
            float y = height * 0.5f;

            Vector3 upperLeft = new Vector3(-x, 0, y);//, 0.0f);
            Vector3 upperRight = new Vector3(x, 0, y);//, 0.0f);
            Vector3 lowerLeft = new Vector3(-x, 0, -y);//, 0.0f);
            Vector3 lowerRight = new Vector3(x, 0, -y);//, 0.0f);
            Vector3 nupperLeft = new Vector3(-x, y, 0.0f);
            Vector3 nupperRight = new Vector3(x, y, 0.0f);
            Vector3 nlowerLeft = new Vector3(-x, -y, 0.0f);
            Vector3 nlowerRight = new Vector3(x, -y, 0.0f);

            VertexPositionColorTexture[] vertices =
            {
                new VertexPositionColorTexture(position + upperLeft, color,  new Vector2(0.0f, 0.0f)),  // 0
                new VertexPositionColorTexture(position + upperRight, color, new Vector2(1.0f, 0.0f)),  // 1
                new VertexPositionColorTexture(position + lowerLeft, color, new Vector2(0.0f, 1.0f)),  // 2
                new VertexPositionColorTexture(position + lowerRight,color, new Vector2(1.0f, 1.0f)),  // 3

                new VertexPositionColorTexture(position + nupperLeft,color,  new Vector2(0.0f, 0.0f)),  // 4
                new VertexPositionColorTexture(position + nupperRight,color, new Vector2(1.0f, 0.0f)),  // 5
                new VertexPositionColorTexture(position + nlowerLeft, color, new Vector2(0.0f, 1.0f)),  // 6
                new VertexPositionColorTexture(position + nlowerRight,color, new Vector2(1.0f, 1.0f)),  // 7
            };

            _billboard.vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
            _billboard.vertexBuffer.SetData(vertices);

            short[] indices =
            {
                0, 1, 2,
                2, 1, 3,
                4, 5, 6,
                6, 5, 7
            };

            _billboard.indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            _billboard.indexBuffer.SetData(indices);
        }

        private void DrawBillboard(Vector3 viewDir, Matrix view, Matrix projection)
        {
            GraphicsDevice.SetVertexBuffer(_billboard.vertexBuffer);
            GraphicsDevice.Indices = _billboard.indexBuffer;

            RasterizerState prevRasterizerState = GraphicsDevice.RasterizerState;
            BlendState prevBlendState = GraphicsDevice.BlendState;

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Create a matrix that will rotate the billboard so that it will
            // always face the camera.

            Matrix billboardMatrix = Matrix.CreateBillboard(Vector3.Zero, eye, Vector3.Up, null);// Matrix.CreateConstrainedBillboard(Vector3.Zero, eye, Vector3.Up, null, null);//-eye, Vector3.UnitZ);
            billboard.CurrentTechnique = billboard.Techniques["Albedo"];
            billboard.Parameters["world"].SetValue(Matrix.Identity);
            billboard.Parameters["view"].SetValue(view);
            billboard.Parameters["projection"].SetValue(projection);
            billboard.Parameters["colorMap"].SetValue(glow);

            foreach (EffectPass pass in billboard.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0, _billboard.vertexBuffer.VertexCount, 0, 4);
            }

            GraphicsDevice.SetVertexBuffer(null);
            GraphicsDevice.Indices = null;

            GraphicsDevice.BlendState = prevBlendState;
            GraphicsDevice.RasterizerState = prevRasterizerState;
        }

        #endregion

        #region Tests
        public void Shoot(int x, int y)
        {
            ObjectViewer.hitmesh = "";
            Matrix view = Matrix.CreateLookAt(eye, Vector3.Zero, up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1800.0f);
            if (mesh != null)
            {
                mesh.Shoot(x, y, projection, view);
            }

        }
        public Ray CreateRay(int x, int y)
        {
            Matrix view = Matrix.CreateLookAt(eye, Vector3.Zero, up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1800.0f);
            Viewport vp = Form1.graphics.Viewport;

            Vector3 pos1 = vp.Unproject(new Vector3(x, y, 0), projection, view, Matrix.Identity);
            Vector3 pos2 = vp.Unproject(new Vector3(x, y, 1), projection, view, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);

            return new Ray(pos1, dir);
        }

        public MeshNode FindMeshNode(String name)
        {
            MeshNode res = (MeshNode)mesh.FindNode(name);
            return res;
        }
        #endregion

        #region Weapon loadouts
        public void PrepareLoadout()
        {
            WeaponMeshes.Clear();
            InternalGuns.Clear();
            BulletDirections.Clear();
            if (SelectedWeaponLoadout >= 0)
            {
                foreach (WeaponSlot ws in WeaponLoadouts.Loads[SelectedWeaponLoadout].Weapons)
                {
                    if ((WeaponLoadouts.Hooks[ws.Slot].Contains("Ext")) || (WeaponLoadouts.Hooks[ws.Slot].Contains("BombSpawn")))
                    {
                        if (!ws.WeaponName.Contains("Null"))
                        {
                            if (!WeaponMeshes.ContainsKey(ws.WeaponName))
                            {
                                Mesh m = new Mesh(Weapons[ws.WeaponName]);
                                WeaponMeshes.Add(ws.WeaponName, m);
                            }
                        }
                    }
                    if (WeaponLoadouts.Hooks[ws.Slot].Contains("CANNON"))
                    {
                        if (!ws.WeaponName.Contains("Null"))
                        {
                            List<Vector3> hl = new List<Vector3>();
                            mesh.FindHook(WeaponLoadouts.Hooks[ws.Slot], ref hl);
                            InternalGuns.Add(hl[0]);
                            Hook bd = mesh.FindHook(WeaponLoadouts.Hooks[ws.Slot]);
                            BulletDirections.Add(bd.matrix.Up);

                        }
                    }
                }
            }
        }
        #endregion

        #region Ambient Occlusion
        public float GetAo(Vector3 pos, Vector3 direction, Vector3 up)
        {
            float res = 1.0f;
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 0.01f, 1.0f);
            BasicEffect be = new BasicEffect(GraphicsDevice);
            be.Projection = projection;
            be.World = Matrix.Identity;
            be.AmbientLightColor = Color.White.ToVector3();
            be.DiffuseColor = Color.White.ToVector3();
            be.TextureEnabled = false;
            byte[] textureData = new byte[4 * 128 * 128];

            pos += direction * 0.01f;

            Vector3 right = Vector3.Cross(direction, Vector3.Right);
            Vector3 nup = Vector3.Cross(right, direction);

            Matrix view = Matrix.CreateLookAt(pos, pos + (direction * 10), nup);
            be.View = view;
            GraphicsDevice.SetRenderTarget(ao_worker);
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            RasterizerState ras = new RasterizerState();
            ras.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = ras;
            mesh.Draw(be, 0);
            GraphicsDevice.SetRenderTarget(null);

            ao_worker.GetData<byte>(textureData);

            float c = 0;
            for (int j = 0; j < 4 * 128 * 128; j += 4)
            {
                if (textureData[j] > 5)
                {
                    c++;
                }
            }
            res = c / (128.0f * 128.0f);

            return res;
        }

        public float GetRayAo(Vector3 pos, Vector3 normal, int count)
        {
            return mesh.GetAORay(pos, normal, count);
        }

        int bakesize;
        String baketexture;

        public void BakeAmbientVertexCamera(String texture, int size)
        {
            bakesize = size;
            baketexture = texture;

            mesh.BuildAoListVertexCamera(texture, this);

            ao_target.Dispose();
            ao_target = new RenderTarget2D(GraphicsDevice, size, size, false, SurfaceFormat.Color, DepthFormat.None);
            GraphicsDevice.SetRenderTarget(ao_target);
            GraphicsDevice.Clear(Color.White);

            BasicEffect effect = new BasicEffect(GraphicsDevice);
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, size, size, 0, 0, 1);
            effect.World = Matrix.Identity;
            effect.View = Matrix.Identity;
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
            effect.AmbientLightColor = Color.White.ToVector3();
            effect.DiffuseColor = Color.White.ToVector3();
            effect.Alpha = 1;
            effect.LightingEnabled = false;


            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            RasterizerState ras = new RasterizerState();
            ras.CullMode = CullMode.None;

            GraphicsDevice.RasterizerState = ras;

            mesh.DrawAO(effect, size, texture);

            GraphicsDevice.SetRenderTarget(null); // finished with render target


            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void BakeAmbientMultiVertex(String texture, int size)
        {
            bakesize = size;
            baketexture = texture;

            mesh.BuildAoListMultiVertexCamera(texture, this);

            ao_target.Dispose();
            ao_target = new RenderTarget2D(GraphicsDevice, size, size, false, SurfaceFormat.Color, DepthFormat.None);
            GraphicsDevice.SetRenderTarget(ao_target);
            GraphicsDevice.Clear(Color.White);

            BasicEffect effect = new BasicEffect(GraphicsDevice);
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, size, size, 0, 0, 1);
            effect.World = Matrix.Identity;
            effect.View = Matrix.Identity;
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
            effect.AmbientLightColor = Color.White.ToVector3();
            effect.DiffuseColor = Color.White.ToVector3();
            effect.Alpha = 1;
            effect.LightingEnabled = false;


            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            RasterizerState ras = new RasterizerState();
            ras.CullMode = CullMode.None;

            GraphicsDevice.RasterizerState = ras;

            mesh.DrawAO(effect, size, texture);

            GraphicsDevice.SetRenderTarget(null); // finished with render target


            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void BakeAmbientVertexRay(String texture, int size, int count)
        {
            bakesize = size;
            baketexture = texture;

            mesh.BuildAoListiVertexRay(texture, this, count);

            ao_target.Dispose();
            ao_target = new RenderTarget2D(GraphicsDevice, size, size, false, SurfaceFormat.Color, DepthFormat.None);
            GraphicsDevice.SetRenderTarget(ao_target);
            GraphicsDevice.Clear(Color.White);

            BasicEffect effect = new BasicEffect(GraphicsDevice);
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, size, size, 0, 0, 1);
            effect.World = Matrix.Identity;
            effect.View = Matrix.Identity;
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
            effect.AmbientLightColor = Color.White.ToVector3();
            effect.DiffuseColor = Color.White.ToVector3();
            effect.Alpha = 1;
            effect.LightingEnabled = false;


            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            RasterizerState ras = new RasterizerState();
            ras.CullMode = CullMode.None;

            GraphicsDevice.RasterizerState = ras;

            mesh.DrawAO(effect, size, texture);

            GraphicsDevice.SetRenderTarget(null); // finished with render target


            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void UpdateBaking(Mesh tmesh)
        {
            GraphicsDevice.SetRenderTarget(ao_target);
            BasicEffect effect = new BasicEffect(GraphicsDevice);
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, bakesize, bakesize, 0, 0, 1);
            effect.World = Matrix.Identity;
            effect.View = Matrix.Identity;
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
            effect.AmbientLightColor = Color.White.ToVector3();
            effect.DiffuseColor = Color.White.ToVector3();
            effect.Alpha = 1;
            effect.LightingEnabled = false;


            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            RasterizerState ras = new RasterizerState();
            ras.CullMode = CullMode.None;

            GraphicsDevice.RasterizerState = ras;

            tmesh.DrawAo(effect, bakesize, baketexture);

            GraphicsDevice.SetRenderTarget(null); // finished with render target


            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            AOBuilder.Instance.UpdateForm();

        }

        public void BakeAmbientOcclusion(AOBuilder builder, int maxrays, int size)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<float> ao_values = new List<float>();

            mesh.BuildAoList(ref verts, ref normals);
            for (int i = 0; i < verts.Count; i++)
            {
                ao_values.Add(GetAo(verts[i], normals[i], Vector3.Up));
            }

            ao_target.Dispose();
            ao_target = new RenderTarget2D(GraphicsDevice, size, size, false, SurfaceFormat.Color, DepthFormat.None);
            GraphicsDevice.SetRenderTarget(ao_target);
            GraphicsDevice.Clear(Color.White);

            BasicEffect effect = new BasicEffect(GraphicsDevice);
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, size, size, 0, 0, 1);
            effect.World = Matrix.Identity;
            effect.View = Matrix.Identity;
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
            effect.AmbientLightColor = Color.White.ToVector3();
            effect.DiffuseColor = Color.White.ToVector3();
            effect.Alpha = 1;
            effect.LightingEnabled = false;


            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            RasterizerState ras = new RasterizerState();
            ras.CullMode = CullMode.None;

            GraphicsDevice.RasterizerState = ras;

            mesh.DrawAo(effect, size, ref ao_values);

            GraphicsDevice.SetRenderTarget(null); // finished with render target


            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public List<Triangle> GetTris(String texture)
        {
            List<Triangle> tris = new List<Triangle>();

            return tris;
        }
        #endregion

        #region Adjustments

        public void RegenerateNormals()
        {
            mesh.RegenerateNormals();
        }

        public void SwapTriangles()
        {
            mesh.SwapTriangles();
        }
        #endregion

        #region heirMesh support
        public bool Exists(String name)
        {
            if (mesh.FindNode(name) != null)
                return true;

            return false;
        }

        public void hideTree(String name)
        {
            Node n = mesh.FindNode(name);
            if (n != null)
            {
                n.Hidden = true;
                foreach (Node n2 in n.children)
                {
                    n2.Hidden = true;
                    hide(n2);
                }

            }
        }

        private void hide(Node n)
        {
            foreach (Node n2 in n.children)
            {
                n2.Hidden = true;
                hide(n2);
            }
        }

        public void chunkVisible(String name, bool visible)
        {
            Node n = mesh.FindNode(name);
            if (n != null)
            {
                n.Hidden = !visible;
            }
        }
        #endregion

        #region Horizon
        VertexPositionColor[] horizon;
        BasicEffect horizon_effect;

        private void BuildHorizon(GraphicsDevice device, float radius)
        {
            float h = 75;
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            for (int i = 2; i < 362; i += 2)
            {
                float start_angle = MathHelper.ToRadians(i - 2);
                float end_angle = MathHelper.ToRadians(i);
                float x1, z1;
                float x2, z2;

                x1 = (float)(radius * Math.Sin(start_angle));
                z1 = (float)(radius * Math.Cos(start_angle));
                x2 = (float)(radius * Math.Sin(end_angle));
                z2 = (float)(radius * Math.Cos(end_angle));

                #region Triangle 1
                VertexPositionColor vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 0);
                vpc.Position = new Vector3(x1, 20 - h, z1);
                verts.Add(vpc);

                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 250);
                vpc.Position = new Vector3(x1, -20, z1);
                verts.Add(vpc);

                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 0);
                vpc.Position = new Vector3(x2, 20 - h, z2);
                verts.Add(vpc);
                #endregion

                #region Triangle 2
                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 0);
                vpc.Position = new Vector3(x1, h - 20, z1);
                verts.Add(vpc);

                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 250);
                vpc.Position = new Vector3(x1, -20, z1);
                verts.Add(vpc);

                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 0);
                vpc.Position = new Vector3(x2, h - 20, z2);
                verts.Add(vpc);
                #endregion

                #region Triangle 3
                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 250);
                vpc.Position = new Vector3(x2, -20, z2);
                verts.Add(vpc);

                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 0);
                vpc.Position = new Vector3(x2, 20 - h, z2);
                verts.Add(vpc);

                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 250);
                vpc.Position = new Vector3(x1, -20, z1);
                verts.Add(vpc);
                #endregion

                #region Triangle 4
                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 250);
                vpc.Position = new Vector3(x2, -20, z2);
                verts.Add(vpc);

                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 0);
                vpc.Position = new Vector3(x2, h - 20, z2);
                verts.Add(vpc);

                vpc = new VertexPositionColor();
                vpc.Color = Color.FromNonPremultiplied(255, 255, 255, 250);
                vpc.Position = new Vector3(x1, -20, z1);
                verts.Add(vpc);
                #endregion

            }
            horizon = verts.ToArray();
            horizon_effect = new BasicEffect(device);
            horizon_effect.DiffuseColor = Color.White.ToVector3();
            horizon_effect.VertexColorEnabled = true;
            horizon_effect.TextureEnabled = false;
            horizon_effect.World = Matrix.CreateRotationX(MathHelper.ToRadians(90));
            horizon_effect.PreferPerPixelLighting = true;

        }

        private void DrawHorizon(Matrix projection, Matrix view)
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            horizon_effect.Projection = projection;
            horizon_effect.View = view;
            if (ShipMode)
                horizon_effect.World = Matrix.CreateTranslation(new Vector3(0, 20, 0)) * Matrix.CreateRotationX(MathHelper.ToRadians(90));
            else
                horizon_effect.World = Matrix.CreateRotationX(MathHelper.ToRadians(90));
            Form1.graphics.DepthStencilState = DepthStencilState.None;

            foreach (EffectPass pass in horizon_effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                horizon_effect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>
                    (PrimitiveType.TriangleList,
                     horizon,
                     0,
                     horizon.Length / 3);
            }
        }

        #endregion

        #region DAE
        public void SaveDAE()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "DAE files *.dae |*.dae";
            sfd.DefaultExt = "*.dae";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (TextWriter writer = File.CreateText(sfd.FileName))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\">");
                    writer.WriteLine("\t<asset>");
                    writer.WriteLine("\t\t<contributor>");
                    writer.WriteLine("\t\t\t<author>IL2 Modder</author>");
                    writer.WriteLine("\t\t\t<authoring_tool>Blender 2.58.1 r38094M</authoring_tool>");
                    writer.WriteLine("\t\t</contributor>");
                    writer.WriteLine(String.Format("\t\t<created>{0}</created>", DateTime.Now.ToLongDateString()));
                    writer.WriteLine(String.Format("\t\t<modified>{0}</modified>", DateTime.Now.ToLongDateString()));
                    writer.WriteLine("\t\t<unit name=\"meter\" meter=\"1\"/>");
                    writer.WriteLine("\t\t<up_axis>Z_UP</up_axis>");
                    writer.WriteLine("\t</asset>");

                    writer.WriteLine("\t<library_cameras>");
                    writer.WriteLine("\t\t<camera id=\"Camera-camera\" name=\"Camera\">");
                    writer.WriteLine("\t\t\t<optics>");
                    writer.WriteLine("\t\t\t\t<technique_common>");
                    writer.WriteLine("\t\t\t\t\t<perspective>");
                    writer.WriteLine("\t\t\t\t\t\t<xfov>49.13434</xfov>");
                    writer.WriteLine("\t\t\t\t\t\t<aspect_ratio>1</aspect_ratio>");
                    writer.WriteLine("\t\t\t\t\t\t<znear>0.1</znear>");
                    writer.WriteLine("\t\t\t\t\t\t<zfar>6000</zfar>");
                    writer.WriteLine("\t\t\t\t\t</perspective>");
                    writer.WriteLine("\t\t\t\t</technique_common>");
                    writer.WriteLine("\t\t\t</optics>");
                    writer.WriteLine("\t\t</camera>");
                    writer.WriteLine("\t</library_cameras>");

                    writer.WriteLine("\t<library_lights>");
                    writer.WriteLine("\t\t<light id=\"Lamp_001-light\" name=\"Lamp.001\">");
                    writer.WriteLine("\t\t\t<technique_common>");
                    writer.WriteLine("\t\t\t\t<ambient>");
                    writer.WriteLine("\t\t\t\t\t<color>0.5511385 0.5956185 0.6800001</color>");
                    writer.WriteLine("\t\t\t\t</ambient>");
                    writer.WriteLine("\t\t\t</technique_common>");
                    writer.WriteLine("\t\t</light>");
                    writer.WriteLine("\t</library_lights>");

                    writer.WriteLine("\t<library_effects>");
                    Form1.written_effects.Clear();
                    mesh.SaveDAEEffects(writer, true);
                    writer.WriteLine("\t</library_effects>");

                    writer.WriteLine("\t<library_materials>");
                    Form1.written_effects.Clear();
                    mesh.SaveDAEEffects(writer, false);
                    writer.WriteLine("\t</library_materials>");

                    writer.WriteLine("\t<library_geometries>");
                    mesh.SaveDAEMesh(writer);
                    writer.WriteLine("\t<library_geometries>");

                    writer.WriteLine("\t<library_visual_scenes>");
                    writer.WriteLine("\t\t<visual_scene id=\"Scene\" name=\"Scene\">");
                    writer.WriteLine("\t\t\t<node id=\"Lamp_002\" type=\"NODE\">");
                    writer.WriteLine("\t\t\t<translate sid=\"location\">40.81436 24.00965 5.903865</translate>");
                    writer.WriteLine("\t\t\t<rotate sid=\"rotationZ\">0 0 1 -251.9589</rotate>");
                    writer.WriteLine("\t\t\t<rotate sid=\"rotationY\">0 1 0 25.96984</rotate>");
                    writer.WriteLine("\t\t\t<rotate sid=\"rotationX\">1 0 0 76.67831</rotate>");
                    writer.WriteLine("\t\t\t<scale sid=\"scale\">2.344286 2.344287 2.344287</scale>");
                    writer.WriteLine("\t\t\t<instance_light url=\"#Lamp_002-light\"/>");
                    writer.WriteLine("\t\t\t</node>");

                    mesh.SaveDAEStruture(writer);

                    writer.WriteLine("\t\t</visual_scene>");
                    writer.WriteLine("\t</library_visual_scenes>");


                    writer.WriteLine("\t<scene>");
                    writer.WriteLine("\t\t<instance_visual_scene url=\"#Scene\"/>");
                    writer.WriteLine("\t</scene>");
                    writer.WriteLine("</COLLADA>");
                }
            }

        }
        #endregion

        #region OGRE
        public void SaveAsOGRE(String dir)
        {
            mesh.SaveOGRE(dir);
        }
        #endregion

        #region OBJ
        public void SaveAsObj(String directory)
        {
            saved_materials.Clear();
            material_writer = File.CreateText(directory + "//il2mat.mtl");
            mesh.SaveAsObj(directory);
            material_writer.Close();
        }
        public void SaveAsObj2(String directory, String name)
        {
            String locs = Path.Combine(directory, "locs.txt");
            TextWriter tw = File.CreateText(locs);
            saved_materials.Clear();
            material_writer = File.CreateText(directory + "//il2mat.mtl");
            mesh.SaveAsObj2(directory, name, tw);
            material_writer.Close();
            tw.Close();
        }
        #endregion

        #region FBX
        public void SaveAsFBX(String directory)
        {
            mesh.SaveAsFBX(directory);
        }

        public void SaveAsUE4(String directory)
        {
            mesh.SaveAsUE4(directory);
        }
        public void SaveAsUE5(String directory)
        {
            mesh.SaveAsUE5(directory);
        }

        #endregion

        #region FOX1
        public String SaveToFox1()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = @"E:\Research\XNA\MultiThreadedRenderer\MultiThreadedRenderer\Content\HIM\Aircraft\British\WWII\Fighters\Spitfire9c";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Fox1ExportDialog fd = new Fox1ExportDialog();
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    mesh.SaveToFox1(fbd.SelectedPath, fd);
                    if (fd.ExportTextures)
                        return fbd.SelectedPath;
                }
            }
            return "Cancel";
        }

        public void ExportCollisionMeshes(String dir)
        {
            mesh.ExportCollisionMeshes(dir);
        }

        public void ExportHooks(TextWriter t)
        {
            mesh.ExportHooks(t);
        }

        #endregion

        #region Modifiers
        public void AdjustLighting()
        {
            mesh.AdjustLighting();
        }

        public void AddCollisionMesh(MeshNode mn)
        {
            Forms.AddCollisionMesh acm = new Forms.AddCollisionMesh(mn);
            if (acm.ShowDialog() == DialogResult.OK)
            {

            }
        }

        #endregion

    }
}
