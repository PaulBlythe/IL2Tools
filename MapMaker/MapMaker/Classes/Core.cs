using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Net;

namespace MapMaker
{
    public delegate void UpdateMainDisplayDelegate(int state);
    public delegate void SetStatusTextDelegate(string text);

    public partial class Core
    {
        public static Core Instance;

        #region Storage
        public String SRTMdirectory = "";
        public ProjectSettings settings = new ProjectSettings();
        WebClient wc = new WebClient();

        public List<String> RequiredFiles = new List<string>();
        UnavailableFilesList UnAvailableFiles = new UnavailableFilesList();
        List<String> ziplist = new List<string>();

        public String downloadstatus = "";
        public String download_task = "";
        public bool FilesDownloaded = false;

        public short[] Pixels;
        public SRTM[,] Map;
        public LandTypeDatabase landdata = null;
        public byte[] map_t;
        public UInt32[] types;

        public UpdateMainDisplayDelegate UpdateMainDisplay;
        public SetStatusTextDelegate SetStatusText;
        #endregion


        public Core()
        {
            Instance = this;

            String p = System.AppDomain.CurrentDomain.BaseDirectory;
            p = System.IO.Path.Combine(p, "settings.txt");

            if (File.Exists(p))
            {
                using (StreamReader sr = new StreamReader(p))
                {
                    SRTMdirectory = sr.ReadLine();
                }
            }
            landdata = new LandTypeDatabase();
        }

        public void SaveSettings()
        {
            String p = System.AppDomain.CurrentDomain.BaseDirectory;
            p = System.IO.Path.Combine(p, "settings.txt");
            using (TextWriter writer = File.CreateText(p))
            {
                writer.WriteLine(SRTMdirectory);
            }
        }

        public void GenerateMap()
        {
            FilesDownloaded = false;
            UpdateMainDisplay(1);
            GetSRTMFiles();
        }

        public void GenerateMapT()
        {
            types = new UInt32[settings.ImageWidth * settings.ImageHeight];
            byte[] lt = new byte[settings.ImageHeight * settings.ImageWidth];
            List<byte> usedtypes = new List<byte>();
            double start_lat = settings.StartLatitude;
            int pos = 0;
            MercatorProjection mp = new MercatorProjection();

            for (int y = 0; y < settings.ImageHeight; y++)
            {
                double start_lon = settings.StartLongitude;
                for (int x = 0; x < settings.ImageWidth; x++)
                {
                    byte t = landdata.GetType(start_lat, start_lon);
                    if (!usedtypes.Contains(t))
                        usedtypes.Add(t);

                    lt[pos++] = t;
                    start_lon = mp.GetNewLongitude(start_lon, start_lat, settings.PixelWidthInMetres);
                }
                start_lat -= (settings.PixelWidthInMetres / 111320.0);
            }


            for (int i = 0; i < settings.ImageWidth * settings.ImageHeight; i++)
            {
                byte type = lt[i];
                short height = Pixels[i];

                switch (type)
                {
                    case 0:         // water
                        {
                            if (height <= 0)
                            {
                                lt[i] = 28;
                            }
                            else
                            {
                                lt[i] = 0;
                            }
                        }
                        break;
                    case 1:         // Jungle
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 27;
                            }
                        }
                        break;
                    case 2:         // Forest
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 24;
                            }
                        }
                        break;
                    case 3:         // snow
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 8;
                            }
                        }
                        break;

                    case 4:         // desert
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 1;
                            }
                        }
                        break;

                    case 5:         // dry scrub
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 2;
                            }
                        }
                        break;

                    case 6:         // city
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 16;
                            }
                        }
                        break;

                    case 7:         // grass
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 3;
                            }
                        }
                        break;

                    case 8:         // hill
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 4;
                            }
                        }
                        break;

                    case 9:         // farmland
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 5;
                            }
                        }
                        break;

                    case 10:         // tropical savannah
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 6;
                            }
                        }
                        break;

                    case 11:         // lake
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 30;
                            }
                        }
                        break;

                    case 12:         // swamp
                        {
                            lt[i] = 7;
                        }
                        break;

                    case 13:         // steppe
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 12;
                            }
                        }
                        break;

                    case 14:         // mixed scrub
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 13;
                            }
                        }
                        break;

                    case 15:         // desert scrub
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 14;
                            }
                        }
                        break;

                    case 16:         // mountain
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 9;
                            }
                        }
                        break;

                    case 17:         // sand scrub
                        {
                            if (height == 0)
                            {
                                lt[i] = 29;
                            }
                            else
                            {
                                lt[i] = 15;
                            }
                        }
                        break;
                }


            }
            map_t = lt;

            for (int i=0; i<settings.ImageHeight*settings.ImageWidth; i++)
            {
                types[i] = ColourMappingForMapT[lt[i]];
            }
        }

        public void SaveMapT(string path)
        {
            TGAWriter.Save(map_t, settings.ImageWidth, settings.ImageHeight, path);
        }

        public void RawSave(string path)
        {
            if (Pixels != null)
            {
                using (BinaryWriter b = new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    for (int i = 0; i < settings.ImageWidth * settings.ImageHeight; i++)
                    {
                        b.Write(Pixels[i]);
                    }
                }
            }
        }

        public void IL2Save(string path)
        {
            IL2Mapping map = new IL2Mapping();
            IL2Colour[] newPixels = new IL2Colour[settings.ImageWidth * settings.ImageHeight];

            for (int i = 0; i < settings.ImageHeight * settings.ImageWidth; i++)
            {
                newPixels[i] = map.GetColour(Pixels[i]);
            }

            TGAWriter.Save(newPixels, settings.ImageWidth, settings.ImageHeight, path);
        }

        public void GenerateMapFromData()
        {
            ThreadStart childref = new ThreadStart(BuildAll);
            Thread childThread = new Thread(childref);
            childThread.IsBackground = true;
            childThread.Start();
        }

        /// <summary>
        /// Load in all the SRTM files to create a virtual map
        /// </summary>
        private void BuildVirtualMap()
        {
            int start_lat = (int)Math.Floor(settings.StartLatitude);
            int end_lat = (int)(settings.StartLatitude + ((settings.ImageHeight * settings.PixelWidthInMetres) / 111320.0) + 0.5);
            int start_lon = (int)Math.Floor(settings.StartLongitude);

            double width_in_metres = settings.PixelWidthInMetres * settings.ImageWidth;

            double met = SRTM.GetMetresPerDegreeLongitude(start_lat);
            int width1 = (int)(width_in_metres / met);

            met = SRTM.GetMetresPerDegreeLongitude(end_lat);
            int width2 = (int)(width_in_metres / met);

            width1 = Math.Max(width1, width2);

            int LonWidth = width1 + 2;
            double height_in_metres = settings.PixelWidthInMetres * settings.ImageHeight;
            int LatHeight = (int)(height_in_metres / 111320.0) + 1;

            Map = new SRTM[LonWidth, LatHeight];

            for (int i = 0; i < LatHeight; i++)
            {
                for (int j = 0; j < LonWidth; j++)
                {
                    Map[j, i] = new SRTM(start_lat - i, start_lon + j, SRTMdirectory);
                }
            }
        }

        /// <summary>
        /// Actually build the gray scale image
        /// </summary>
        private void BuildImage()
        {
            Core parent = Core.Instance;
            MercatorProjection mp = new MercatorProjection(settings.StartLatitude, settings.StartLongitude);
            Pixels = new short[settings.ImageWidth * settings.ImageHeight];


            int pos = 0;
            for (int y = 0; y < settings.ImageHeight; y++)
            {
                parent.downloadstatus = String.Format("Line {0}", y);
                UpdateMainDisplay(3);
                double nl = mp.GetNewLatitude(settings.StartLatitude, y * settings.PixelWidthInMetres);
                Delta d = mp.GetDelta(nl, settings.StartLongitude);
                for (int x = 0; x < settings.ImageWidth; x++)
                {
                    Pixels[pos + x] = Map[d.mx, d.my].GetHeight(d, settings.PixelWidthInMetres);
                    d = mp.Step(d, settings.PixelWidthInMetres);
                }
                //Parallel.For(0, settings.ImageWidth,
                //   index => 
                //   {
                //       Pixels[pos + index] = Map[d.mx, d.my].GetHeight(d,settings.PixelWidthInMetres);
                //       d = mp.Step(d, settings.PixelWidthInMetres);
                //
                //   }
                //       );
                pos += settings.ImageWidth;
            }


        }

        /// <summary>
        /// Work out the needed SRTM files and download them
        /// </summary>
        public void GetSRTMFiles()
        {
            int start_lat = (int)Math.Floor(settings.StartLatitude);
            int start_lon = (int)Math.Floor(settings.StartLongitude);
            double met = SRTM.GetMetresPerDegreeLongitude(start_lat);
            double width_in_metres = settings.PixelWidthInMetres * settings.ImageWidth;
            int LonWidth = (int)(width_in_metres / met) + 2;
            double height_in_metres = settings.PixelWidthInMetres * settings.ImageHeight;
            int LatHeight = (int)(height_in_metres / 111320.0) + 1;

            UnAvailableFiles.SetDirectory(SRTMdirectory);

            for (int i = 0; i < LatHeight; i++)
            {
                for (int j = 0; j < LonWidth; j++)
                {
                    RequiredFiles.Add(SRTM.GetFileName(start_lat - i, start_lon + j));
                }
            }
            UpdateMainDisplay(1);

            // Remove any files we already have
            downloadstatus = "Removing existing files";
            for (int i = RequiredFiles.Count - 1; i >= 0; i--)
            {
                String fn = System.IO.Path.Combine(SRTMdirectory, RequiredFiles[i]);
                fn += ".hgt";
                if (UnAvailableFiles.Contains(RequiredFiles[i]) || File.Exists(fn))
                {
                    RequiredFiles.RemoveAt(i);
                    UpdateMainDisplay(1);
                }
            }

            downloadstatus = "Downloading files";
            UpdateMainDisplay(1);

            ThreadStart childref = new ThreadStart(DownloadFiles);
            Thread childThread = new Thread(childref);
            childThread.IsBackground = true;
            childThread.Start();
        }

        public static void BuildAll()
        {
            Core parent = Core.Instance;
            parent.UpdateMainDisplay(2);
            parent.BuildVirtualMap();
            parent.UpdateMainDisplay(3);
            parent.BuildImage();
            parent.UpdateMainDisplay(4);
        }

        #region Thread handlers

        /// <summary>
        /// Thread based file downloader, plus does all the other work
        /// </summary>
        /// 
        public static void DownloadFiles()
        {
            string SRTMLocation = "https://dds.cr.usgs.gov/srtm/version2_1/SRTM3/";
            Core parent = Core.Instance;
            int i = parent.RequiredFiles.Count - 1;
            while (i >= 0)
            {
                bool missing = false;
                String file = System.IO.Path.Combine(parent.SRTMdirectory,
                                                     parent.RequiredFiles[i]) + ".zip";
                parent.download_task = "Downloading " + file;
                parent.UpdateMainDisplay(1);

                if (!File.Exists(file))
                {
                    String url = SRTMLocation + "/Africa/" + parent.RequiredFiles[i] + ".zip";
                    if (DoesFileExist(url))
                    {
                        DownloadFile(url, file);
                    }
                    else
                    {
                        url = SRTMLocation + "/Australia/" + parent.RequiredFiles[i] + ".zip";
                        if (DoesFileExist(url))
                        {
                            DownloadFile(url, file);
                        }
                        else
                        {
                            url = SRTMLocation + "/Eurasia/" + parent.RequiredFiles[i] + ".zip";
                            if (DoesFileExist(url))
                            {
                                DownloadFile(url, file);
                            }
                            else
                            {
                                url = SRTMLocation + "/Islands/" + parent.RequiredFiles[i] + ".zip";
                                if (DoesFileExist(url))
                                {
                                    DownloadFile(url, file);
                                }
                                else
                                {
                                    url = SRTMLocation + "/North_America/" + parent.RequiredFiles[i] + ".zip";
                                    if (DoesFileExist(url))
                                    {
                                        DownloadFile(url, file);
                                    }
                                    else
                                    {
                                        url = SRTMLocation + "/South_America/" + parent.RequiredFiles[i] + ".zip";
                                        if (DoesFileExist(url))
                                        {
                                            DownloadFile(url, file);
                                        }
                                        else
                                        {
                                            missing = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Thread.Sleep(50);
                }

                if (missing)
                {
                    parent.UnAvailableFiles.Add(parent.RequiredFiles[i]);
                }

                lock (parent.RequiredFiles)
                {
                    parent.RequiredFiles.RemoveAt(i);
                }

                parent.UpdateMainDisplay(1);
                i--;
            }
            parent.download_task = "Extracting files";
            parent.UpdateMainDisplay(1);

            foreach (String s in parent.ziplist)
            {
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(s, parent.SRTMdirectory);
                }
                catch (Exception) { }

            }
            parent.download_task = "Loading SRTM files";
            parent.UpdateMainDisplay(1);
            parent.ziplist.Clear();

            parent.UpdateMainDisplay(2);
            parent.BuildVirtualMap();
            parent.UpdateMainDisplay(3);
            parent.BuildImage();
            parent.UpdateMainDisplay(4);
            parent.FilesDownloaded = true;
        }

        /// <summary>
        /// Check the file exists on the server
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool DoesFileExist(String url)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            System.Net.HttpWebRequest request = null;
            System.Net.HttpWebResponse response = null;
            request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
            request.Method = "HEAD";
            request.Timeout = 30000;
            try
            {
                response = (System.Net.HttpWebResponse)request.GetResponse();
                HttpStatusCode status = response.StatusCode;
                return (status != HttpStatusCode.NotFound);
            }
            catch (Exception)
            {
                return false;
            }


        }

        public static void DownloadFile(string url, string file)
        {
            if (File.Exists(file))
                return;

            bool done = false;
            while (!done)
            {
                Core parent = Core.Instance;
                {
                    try
                    {
                        parent.wc.DownloadFile(new System.Uri(url), file);
                        done = true;

                        lock (parent.ziplist)
                        {
                            parent.ziplist.Add(file);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "The operation has timed out")
                        {
                            done = false;
                        }
                        else
                        {
                            done = true;
                        }
                    }
                }
            }
        }

        #endregion


        #region Data tables
        LandTypeRecord[] LandTypes = new LandTypeRecord[]
        {
            new LandTypeRecord(24, 24, 128, "water"),
            new LandTypeRecord(50, 205, 49, "jungle"),
            new LandTypeRecord(33, 138, 33, "forest"),
            new LandTypeRecord(255, 255, 249, "snow"),
            new LandTypeRecord(189, 189, 189, "desert"),
            new LandTypeRecord(245, 222, 180, "dryscrub"),
            new LandTypeRecord(254, 0, 0, "city"),
            new LandTypeRecord(249, 237, 115, "grassland"),
            new LandTypeRecord(144, 187, 142, "hill"),
            new LandTypeRecord(240, 183, 104, "farmland"),
            new LandTypeRecord(255, 214, 0, "tropical savana"),
            new LandTypeRecord(70, 131, 178, "lake"),
            new LandTypeRecord(255, 214, 0, "swamp"),
            new LandTypeRecord(154, 205, 50, "steppe"),
            new LandTypeRecord(153, 249, 151, "mixed scrub"),
            new LandTypeRecord(218, 235, 157, "desert scrub"),
            new LandTypeRecord(151, 147, 84, "mountain"),
            new LandTypeRecord(188, 142, 144, "sand scrub")

        };

        public UInt32[] ColourMappingForMapT = new UInt32[]
        {
                0x80a0ff00,     // 0    Lowland near water
                0x80ffff00,     // 1    Desert
                0x8080ff00,     // 2    Dry scrub
                0x8000ff00,     // 3    Grass
                0x80008000,     // 4    Hill
                0x8000a000,     // 5    Farmland
                0x8080ff00,     // 6    Tropical savannah
                0x80009080,     // 7    Swamp
                0x80ffffff,     // 8    Snow
                0x80808080,     // 9    Mountain
                0x00000000,     // 10
                0x00000000,     // 11
                0x8040a000,     // 12   Steppe
                0x8040ff40,     // 13   Mixed scrub
                0x80808000,     // 14   Desert scrub
                0x80a0a000,     // 15   Sand scrub
                0x80404040,     // 16   City
                0x00000000,         // 17
                0x00000000,         // 18
                0x00000000,         // 19
                0x00000000,         // 20
                0x00000000,         // 21
                0x00000000,         // 22
                0x00000000,         // 23
                0x8080a020,         // 24   Forest
                0x00000000,         // 25
                0x00000000,         // 26
                0x8060ff40,         // 27   Jungle
                0x800020ff,         // 28   Sea
                0x800060ff,         // 29   Water in land 
                0x800080ff,         // 30   Lake
                0x00000000       // 31

        };
        #endregion
    }
}
