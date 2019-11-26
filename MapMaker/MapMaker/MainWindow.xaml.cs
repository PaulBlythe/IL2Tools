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
        public SRTM[,] Map;
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
                        for (int i=0; i<settings.ImageWidth*settings.ImageHeight; i++)
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

                    for (int i=0; i<settings.ImageHeight * settings.ImageWidth; i++)
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
                               scaled_pixels[i] = ((float)Pixels[i] - (float)minValue) / (float)(maxValue-minValue);

                            }

                            falseimage = new WriteableBitmap(settings.ImageWidth, settings.ImageHeight, 96, 96, PixelFormats.Gray32Float, null);
                            falseimage.WritePixels(new Int32Rect(0, 0, settings.ImageWidth, settings.ImageHeight), scaled_pixels, settings.ImageWidth * 4, 0);
                        }

                        var sbitmap = new TransformedBitmap(falseimage, new ScaleTransform( 800.0f / falseimage.PixelWidth, 800.0f / falseimage.PixelHeight));

                        Image img = new Image();
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
            for (int y=0; y<settings.ImageHeight; y++)
            {
                parent.downloadstatus = String.Format("Line {0}", y);
                parent.Dispatcher.BeginInvoke((Action)(() => parent.UpdateMainDisplay(3)));
                double nl = mp.GetNewLatitude(settings.StartLatitude, y * settings.PixelWidthInMetres);
                Delta d = mp.GetDelta(nl, settings.StartLongitude);
                for (int x=0; x<settings.ImageWidth; x++)
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


            for (int i = 0; i < LatHeight ; i++)
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

    }
}
