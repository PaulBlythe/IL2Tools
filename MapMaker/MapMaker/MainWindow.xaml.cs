using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Win32;

namespace MapMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        #region Storage
        String SRTMdirectory = "";
        ProjectSettings settings = new ProjectSettings();
        WebClient wc = new WebClient();

        List<String> RequiredFiles = new List<string>();
        List<String> ziplist = new List<string>();

        String downloadstatus = "";
        public String download_task = "";
        public bool FilesDownloaded = false;

        public short[] Pixels;
        public WriteableBitmap bitmap = null;
        public WriteableBitmap falseimage = null;
        public WriteableBitmap typeoverlay = null;

        public SRTM[,] Map;
        public LandTypeDatabase landdata = null;
        public byte[] map_t;

        public List<ShapeFile> Overlays = new List<ShapeFile>();

        #endregion


        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            Closing += OnWindowClosing;

            String p = System.AppDomain.CurrentDomain.BaseDirectory;
            p = System.IO.Path.Combine(p, "settings.txt");

            if (File.Exists(p))
            {
                using (StreamReader sr = new StreamReader(p))
                {
                    SRTMdirectory = sr.ReadLine();
                    Status.Text = SRTMdirectory;
                }
            }
            landdata = new LandTypeDatabase();
            UpdateMainDisplay(0);
        }

        private void OpenFileMenu(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Save out the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            String p = System.AppDomain.CurrentDomain.BaseDirectory;
            p = System.IO.Path.Combine(p, "settings.txt");
            using (TextWriter writer = File.CreateText(p))
            {
                writer.WriteLine(SRTMdirectory);
            }

        }

        /// <summary>
        /// Option set srtm directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Window1 win = new Window1();

            bool? result = win.ShowDialog();
            if (result.HasValue && result.Value)
            {
                SRTMdirectory = win.Path;
                Status.Text = win.Path;
            }
        }

        /// <summary>
        /// Exit pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Exit pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadShapeFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SHAPE files (*.shp)|*.shp";
            ofd.DefaultExt = "*.shp";
            if (ofd.ShowDialog() == true)
            {
                Overlays.Add(new ShapeFile(ofd.FileName));
                DrawOverlays();
            }
        }

        /// <summary>
        /// Save map T pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveMapT(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = SRTMdirectory;
            dialog.Title = "Save generated type map";
            dialog.Filter = "TGA files (*.tga)|*.tga";

            if (dialog.ShowDialog() == true)
            {
                TGAWriter.Save(map_t, settings.ImageWidth, settings.ImageHeight, dialog.FileName);
            }
        }

        /// <summary>
        /// Generate map t pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateMapT(object sender, RoutedEventArgs e)
        {
            UInt32[] types = new UInt32[settings.ImageWidth * settings.ImageHeight];
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


            typeoverlay = new WriteableBitmap(settings.ImageWidth, settings.ImageHeight, 96, 96, PixelFormats.Pbgra32, null);
            typeoverlay.WritePixels(new Int32Rect(0, 0, settings.ImageWidth, settings.ImageHeight), types, settings.ImageWidth * 4, 0);
            UpdateMainDisplay(5);
        }

        /// <summary>
        /// Edit loaded project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditProject(object sender, RoutedEventArgs e)
        {
            Window2 win = new Window2(settings);
            bool? result = win.ShowDialog();
            if (result.HasValue && result.Value)
            {
                settings = win.settings;

                StartLat.Text = String.Format("Lat : {0}", settings.StartLatitude);
                StartLon.Text = String.Format("Lon : {0}", settings.StartLongitude);
                PixelWidth.Text = String.Format("Pix X: {0}", settings.ImageWidth);
                PixelHeight.Text = String.Format("Pix Y: {0}", settings.ImageHeight);
                Scale.Text = String.Format("Pix Scale: {0}", settings.PixelWidthInMetres);
                UMercator.Text = String.Format("Mercator : {0}", settings.UseMercatorProjection);

            }
        }

        /// <summary>
        /// New project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Window2 win = new Window2();
            bool? result = win.ShowDialog();
            if (result.HasValue && result.Value)
            {
                settings = win.settings;

                StartLat.Text = String.Format("Lat : {0}", settings.StartLatitude);
                StartLon.Text = String.Format("Lon : {0}", settings.StartLongitude);
                PixelWidth.Text = String.Format("Pix X: {0}", settings.ImageWidth);
                PixelHeight.Text = String.Format("Pix Y: {0}", settings.ImageHeight);
                Scale.Text = String.Format("Pix Scale: {0}", settings.PixelWidthInMetres);
                UMercator.Text = String.Format("Mercator : {0}", settings.UseMercatorProjection);

            }
        }

        /// <summary>
        /// Save project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Project files (*.prj)|*.prj";
            sfd.DefaultExt = "*.prj";
            if (sfd.ShowDialog() == true)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(sfd.FileName, FileMode.Create)))
                {
                    settings.Save(writer);
                }
            }
        }

        /// <summary>
        /// Load project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Project files (*.prj)|*.prj";
            ofd.DefaultExt = "*.prj";
            if (ofd.ShowDialog() == true)
            {
                using (BinaryReader writer = new BinaryReader(File.Open(ofd.FileName, FileMode.Open)))
                {
                    settings.Load(writer);
                }

                StartLat.Text = String.Format("Lat : {0}", settings.StartLatitude);
                StartLon.Text = String.Format("Lon : {0}", settings.StartLongitude);
                PixelWidth.Text = String.Format("Pix X: {0}", settings.ImageWidth);
                PixelHeight.Text = String.Format("Pix Y: {0}", settings.ImageHeight);
                Scale.Text = String.Format("Pix Scale: {0}", settings.PixelWidthInMetres);
                UMercator.Text = String.Format("Mercator : {0}", settings.UseMercatorProjection);


            }
        }

        /// <summary>
        /// Load project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateMap(object sender, RoutedEventArgs e)
        {
            FilesDownloaded = false;
            UpdateMainDisplay(1);
            GetSRTMFiles();

        }

        /// <summary>
        /// Save the generated file as a PNG
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PNGSave(object sender, RoutedEventArgs e)
        {
            if (bitmap != null)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.InitialDirectory = SRTMdirectory;
                dialog.Title = "Save generated image";
                dialog.Filter = "PNG files (*.png)|*.png";

                if (dialog.ShowDialog() == true)
                {
                    using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        encoder.Save(fileStream);
                    }
                }
            }
        }

        /// <summary>
        /// Save the generated file as a tga
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TGASave(object sender, RoutedEventArgs e)
        {
            if (bitmap != null)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.InitialDirectory = SRTMdirectory;
                dialog.Title = "Save generated image";
                dialog.Filter = "TGA files (*.tga)|*.tga";

                if (dialog.ShowDialog() == true)
                {
                    TGAWriter.Save(Pixels, settings.ImageWidth, settings.ImageHeight, dialog.FileName);
                }
            }
        }

        /// <summary>
        /// Save the generated file as a raw file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RAWSave(object sender, RoutedEventArgs e)
        {
            if (bitmap != null)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.InitialDirectory = SRTMdirectory;
                dialog.Title = "Save generated image";
                dialog.Filter = "RAW files (*.raw)|*.raw";

                if (dialog.ShowDialog() == true)
                {

                    using (BinaryWriter b = new BinaryWriter(File.Open(dialog.FileName, FileMode.Create)))
                    {
                        for (int i = 0; i < settings.ImageWidth * settings.ImageHeight; i++)
                        {
                            b.Write(Pixels[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save the generated file as a raw file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IL2Save(object sender, RoutedEventArgs e)
        {
            if (bitmap != null)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.InitialDirectory = SRTMdirectory;
                dialog.Title = "Save generated image";
                dialog.Filter = "IL2 files (*.tga)|*.tga";

                if (dialog.ShowDialog() == true)
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    IL2Mapping map = new IL2Mapping();
                    IL2Colour[] newPixels = new IL2Colour[settings.ImageWidth * settings.ImageHeight];

                    for (int i = 0; i < settings.ImageHeight * settings.ImageWidth; i++)
                    {
                        newPixels[i] = map.GetColour(Pixels[i]);
                    }

                    TGAWriter.Save(newPixels, settings.ImageWidth, settings.ImageHeight, dialog.FileName);

                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// Load project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateMapFromData(object sender, RoutedEventArgs e)
        {
            ThreadStart childref = new ThreadStart(BuildAll);
            Thread childThread = new Thread(childref);
            childThread.IsBackground = true;
            childThread.Start();
        }

        /// <summary>
        /// Canvas display handler
        /// </summary>
        /// <param name="state"></param>
        public void UpdateMainDisplay(int state)
        {
            MainDisplay.Children.Clear();

            switch (state)
            {
                #region Start up screen
                case 0:
                    {
                        Label title = new Label();
                        title.Content = "IL2 Map Maker";
                        title.Foreground = Brushes.White;
                        title.FontSize = 64;
                        title.FontWeight = FontWeights.Bold;
                        title.FontStyle = FontStyles.Oblique;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, 200);
                        Canvas.SetLeft(title, 700);

                        title = new Label();
                        title.Content = "A gift for slibenli";
                        title.Foreground = Brushes.White;
                        title.FontSize = 48;
                        title.FontWeight = FontWeights.Bold;
                        title.FontStyle = FontStyles.Oblique;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, 300);
                        Canvas.SetLeft(title, 740);

                        title = new Label();
                        title.Content = "from Stainless";
                        title.Foreground = Brushes.White;
                        title.FontSize = 48;
                        title.FontWeight = FontWeights.Bold;
                        title.FontStyle = FontStyles.Oblique;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, 400);
                        Canvas.SetLeft(title, 760);

                    }
                    break;
                #endregion

                #region Get SRTM data
                case 1:
                    {
                        Label title = new Label();
                        title.Content = "Checking for needed SRTM files";
                        title.Foreground = Brushes.White;
                        title.FontSize = 18;
                        title.FontWeight = FontWeights.Normal;
                        title.FontStyle = FontStyles.Normal;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, 20);
                        Canvas.SetLeft(title, 20);

                        int y = 50;
                        int x = 20;
                        lock (RequiredFiles)
                        {
                            foreach (String s in RequiredFiles)
                            {
                                title = new Label();
                                title.Content = s;
                                title.Foreground = Brushes.White;
                                title.FontSize = 18;
                                title.FontWeight = FontWeights.Normal;
                                title.FontStyle = FontStyles.Normal;
                                MainDisplay.Children.Add(title);
                                Canvas.SetTop(title, y);
                                Canvas.SetLeft(title, x);

                                x += 200;
                                if (x > 1800)
                                {
                                    x = 20;
                                    y += 20;
                                }

                            }
                        }
                        y += 30;
                        title = new Label();
                        title.Content = downloadstatus;
                        title.Foreground = Brushes.White;
                        title.FontSize = 18;
                        title.FontWeight = FontWeights.Normal;
                        title.FontStyle = FontStyles.Normal;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, y);
                        Canvas.SetLeft(title, 20);

                        y += 30;
                        title = new Label();
                        title.Content = download_task;
                        title.Foreground = Brushes.White;
                        title.FontSize = 18;
                        title.FontWeight = FontWeights.Normal;
                        title.FontStyle = FontStyles.Normal;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, y);
                        Canvas.SetLeft(title, 20);

                    }
                    break;
                #endregion

                #region Create virtual map
                case 2:
                    {
                        Label title = new Label();
                        title.Content = "Creating virtual map";
                        title.Foreground = Brushes.White;
                        title.FontSize = 18;
                        title.FontWeight = FontWeights.Normal;
                        title.FontStyle = FontStyles.Normal;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, 20);
                        Canvas.SetLeft(title, 20);
                    }
                    break;
                #endregion

                #region Create image
                case 3:
                    {
                        Label title = new Label();
                        title.Content = "Creating actual map";
                        title.Foreground = Brushes.White;
                        title.FontSize = 18;
                        title.FontWeight = FontWeights.Normal;
                        title.FontStyle = FontStyles.Normal;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, 20);
                        Canvas.SetLeft(title, 20);

                        title = new Label();
                        title.Content = downloadstatus;
                        title.Foreground = Brushes.White;
                        title.FontSize = 18;
                        title.FontWeight = FontWeights.Normal;
                        title.FontStyle = FontStyles.Normal;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, 50);
                        Canvas.SetLeft(title, 20);
                    }
                    break;
                #endregion

                #region Display image
                case 4:
                    {
                        if (bitmap == null)
                        {
                            bitmap = new WriteableBitmap(settings.ImageWidth, settings.ImageHeight, 96, 96, PixelFormats.Gray16, null);
                            bitmap.WritePixels(new Int32Rect(0, 0, settings.ImageWidth, settings.ImageHeight), Pixels, settings.ImageWidth * 2, 0);

                            short minValue = 32767;
                            short maxValue = -32767;
                            int length = settings.ImageHeight * settings.ImageWidth;
                            for (int i = 0; i < length; i++)
                            {
                                if (Pixels[i] < minValue)
                                    minValue = Pixels[i];
                                if (Pixels[i] > maxValue)
                                    maxValue = Pixels[i];
                            }

                            float[] scaled_pixels = new float[length];

                            for (int i = 0; i < length; i++)
                            {
                                scaled_pixels[i] = ((float)Pixels[i] - (float)minValue) / (float)(maxValue - minValue);

                            }

                            falseimage = new WriteableBitmap(settings.ImageWidth, settings.ImageHeight, 96, 96, PixelFormats.Gray32Float, null);
                            falseimage.WritePixels(new Int32Rect(0, 0, settings.ImageWidth, settings.ImageHeight), scaled_pixels, settings.ImageWidth * 4, 0);
                        }

                        var sbitmap = new TransformedBitmap(falseimage, new ScaleTransform(800.0f / falseimage.PixelWidth, 800.0f / falseimage.PixelHeight));

                        Image img = new Image();
                        img.Source = sbitmap;
                        MainDisplay.Children.Add(img);

                        Canvas.SetTop(img, 20);
                        Canvas.SetLeft(img, 420);
                    }
                    break;
                #endregion

                #region display overlay
                case 5:
                    {
                        var sbitmap = new TransformedBitmap(falseimage, new ScaleTransform(800.0f / falseimage.PixelWidth, 800.0f / falseimage.PixelHeight));

                        Image img = new Image();
                        img.Source = sbitmap;
                        MainDisplay.Children.Add(img);

                        Canvas.SetTop(img, 20);
                        Canvas.SetLeft(img, 420);

                        sbitmap = new TransformedBitmap(typeoverlay, new ScaleTransform(800.0f / typeoverlay.PixelWidth, 800.0f / typeoverlay.PixelHeight));

                        img = new Image();
                        img.Source = sbitmap;
                        MainDisplay.Children.Add(img);

                        Canvas.SetTop(img, 20);
                        Canvas.SetLeft(img, 420);
                    }
                    break;
                    #endregion

            }
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
            MainWindow parent = MainWindow.Instance;
            MercatorProjection mp = new MercatorProjection(settings.StartLatitude, settings.StartLongitude);
            Pixels = new short[settings.ImageWidth * settings.ImageHeight];


            int pos = 0;
            for (int y = 0; y < settings.ImageHeight; y++)
            {
                parent.downloadstatus = String.Format("Line {0}", y);
                parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(3)));
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
        private void GetSRTMFiles()
        {
            int start_lat = (int)Math.Floor(settings.StartLatitude);
            int start_lon = (int)Math.Floor(settings.StartLongitude);
            double met = SRTM.GetMetresPerDegreeLongitude(start_lat);
            double width_in_metres = settings.PixelWidthInMetres * settings.ImageWidth;
            int LonWidth = (int)(width_in_metres / met) + 2;
            double height_in_metres = settings.PixelWidthInMetres * settings.ImageHeight;
            int LatHeight = (int)(height_in_metres / 111320.0) + 1;


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
                if (File.Exists(fn))
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
            MainWindow parent = MainWindow.Instance;
            parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(2)));
            parent.BuildVirtualMap();
            parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(3)));
            parent.BuildImage();
            parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(4)));
        }

        /// <summary>
        /// Draw any overlay files into the map_t image
        /// </summary>
        private void DrawOverlays()
        {
            if (Overlays.Count == 0)
                return;

            if (typeoverlay == null)
                return;

            #region Create a bounding region in latitude and longitude
            MercatorProjection mp = new MercatorProjection(settings.StartLatitude, settings.StartLongitude);
            Region r = mp.GetRegion(settings.ImageWidth, settings.ImageHeight, (int)settings.PixelWidthInMetres);           
            #endregion

            foreach (ShapeFile s in Overlays)
            {
                s.Draw(map_t, 32, r, mp, (int)settings.PixelWidthInMetres, settings.ImageWidth, settings.ImageHeight);
            }
            UInt32[] types = new UInt32[settings.ImageHeight * settings.ImageWidth];

            for (int i = 0; i < settings.ImageHeight * settings.ImageWidth; i++)
            {
                byte baset = map_t[i];
                if ((baset & 32) == 0)
                    types[i] = ColourMappingForMapT[map_t[i]];
                else
                    types[i] = 0xffff0000;
            }

            typeoverlay.WritePixels(new Int32Rect(0, 0, settings.ImageWidth, settings.ImageHeight), types, settings.ImageWidth * 4, 0);
            UpdateMainDisplay(5);
        }

        #region Thread handlers

        /// <summary>
        /// Thread based file downloader, plus does all the other work
        /// </summary>
        /// 
        public static void DownloadFiles()
        {
            string SRTMLocation = "https://dds.cr.usgs.gov/srtm/version2_1/SRTM3/";
            MainWindow parent = MainWindow.Instance;
            int i = parent.RequiredFiles.Count - 1;
            while (i >= 0)
            {
                bool missing = false;
                String file = parent.SRTMdirectory + @"\" + parent.RequiredFiles[i] + ".zip";
                parent.download_task = "Downloading " + file;
                parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(1)));

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
                }
                if (missing)
                {

                }
                else
                {
                    lock (parent.RequiredFiles)
                    {
                        parent.RequiredFiles.RemoveAt(i);
                        parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(1)));
                    }
                    Thread.Sleep(50);
                }
                i--;
            }
            parent.download_task = "Extracting files";
            parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(1)));

            foreach (String s in parent.ziplist)
            {
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(s, parent.SRTMdirectory);
                }
                catch (Exception) { }

            }
            parent.download_task = "Loading SRTM files";
            parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(1)));
            parent.ziplist.Clear();

            parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(2)));
            parent.BuildVirtualMap();
            parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(3)));
            parent.BuildImage();
            parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(4)));
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
                MainWindow parent = MainWindow.Instance;
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

        UInt32[] ColourMappingForMapT = new UInt32[]
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
                0x00000000,     // 17
                0x00000000,     // 18
                0x00000000,     // 19
                0x00000000,     // 20
                0x00000000,     // 21
                0x00000000,     // 22
                0x00000000,     // 23
                0x8080a020,     // 24   Forest
                0x00000000,     // 25
                0x00000000,     // 26
                0x8060ff40,     // 27   Jungle
                0x800020ff,     // 28   Sea
                0x800060ff,     // 29   Water in land 
                0x800080ff,     // 30   Lake
                0x00000000      // 31

        };
        #endregion
    }
}
