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
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        #region Storage
        public WriteableBitmap bitmap = null;
        public WriteableBitmap falseimage = null;
        public WriteableBitmap typeoverlay = null;

        public List<ShapeFile> Overlays = new List<ShapeFile>();

        Core core = new Core();
        #endregion


        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            Closing += OnWindowClosing;

            core.UpdateMainDisplay = (int state) =>
            {
                Dispatcher.BeginInvoke((Action)(() => UpdateMainDisplayImpl(state)));
            };

            core.SetStatusText = (string text) =>
            {
                Status.Text = text;
            };

            core.SetStatusText(core.SRTMdirectory);
            core.UpdateMainDisplay(0);
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
            core.SaveSettings();
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
                core.SRTMdirectory = win.Path;
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
            dialog.InitialDirectory = core.SRTMdirectory;
            dialog.Title = "Save generated type map";
            dialog.Filter = "TGA files (*.tga)|*.tga";

            if (dialog.ShowDialog() == true)
            {
                core.SaveMapT(dialog.FileName);
            }
        }

        /// <summary>
        /// Generate map t pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateMapT(object sender, RoutedEventArgs e)
        {
            core.GenerateMapT();
            var settings = core.settings;
            typeoverlay = new WriteableBitmap(settings.ImageWidth, settings.ImageHeight, 96, 96, PixelFormats.Pbgra32, null);
            typeoverlay.WritePixels(new Int32Rect(0, 0, settings.ImageWidth, settings.ImageHeight), core.types, settings.ImageWidth * 4, 0);
            core.UpdateMainDisplay(5);
        }

        void RefreshStatusBar()
        {
            StartLat.Text = String.Format("Lat : {0}", core.settings.StartLatitude);
            StartLon.Text = String.Format("Lon : {0}", core.settings.StartLongitude);
            PixelWidth.Text = String.Format("Pix X: {0}", core.settings.ImageWidth);
            PixelHeight.Text = String.Format("Pix Y: {0}", core.settings.ImageHeight);
            Scale.Text = String.Format("Pix Scale: {0}", core.settings.PixelWidthInMetres);
            UMercator.Text = String.Format("Mercator : {0}", core.settings.UseMercatorProjection);
        }

        /// <summary>
        /// Edit loaded project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditProject(object sender, RoutedEventArgs e)
        {
            Window2 win = new Window2(core.settings);
            bool? result = win.ShowDialog();
            if (result.HasValue && result.Value)
            {
                core.settings = win.settings;
                RefreshStatusBar();
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
                core.settings = win.settings;
                RefreshStatusBar();
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
                    core.settings.Save(writer);
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
                    core.settings.Load(writer);
                }
                RefreshStatusBar();
            }
        }

        /// <summary>
        /// Load project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateMap(object sender, RoutedEventArgs e)
        {
            core.GenerateMap();
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
                dialog.InitialDirectory = core.SRTMdirectory;
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
                dialog.InitialDirectory = core.SRTMdirectory;
                dialog.Title = "Save generated image";
                dialog.Filter = "TGA files (*.tga)|*.tga";

                var settings = core.settings;

                if (dialog.ShowDialog() == true)
                {
                    TGAWriter.Save(core.Pixels, settings.ImageWidth, settings.ImageHeight, dialog.FileName);
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
                dialog.InitialDirectory = core.SRTMdirectory;
                dialog.Title = "Save generated image";
                dialog.Filter = "RAW files (*.raw)|*.raw";

                if (dialog.ShowDialog() == true)
                {
                    core.RawSave(dialog.FileName);
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
                dialog.InitialDirectory = core.SRTMdirectory;
                dialog.Title = "Save generated image";
                dialog.Filter = "IL2 files (*.tga)|*.tga";

                if (dialog.ShowDialog() == true)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    core.IL2Save(dialog.FileName);
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
            core.GenerateMapFromData();
        }

        /// <summary>
        /// Canvas display handler
        /// </summary>
        /// <param name="state"></param>
        public void UpdateMainDisplayImpl(int state)
        {
            var settings = core.settings;

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
                        lock (core.RequiredFiles)
                        {
                            foreach (String s in core.RequiredFiles)
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
                        title.Content = core.downloadstatus;
                        title.Foreground = Brushes.White;
                        title.FontSize = 18;
                        title.FontWeight = FontWeights.Normal;
                        title.FontStyle = FontStyles.Normal;
                        MainDisplay.Children.Add(title);
                        Canvas.SetTop(title, y);
                        Canvas.SetLeft(title, 20);

                        y += 30;
                        title = new Label();
                        title.Content = core.download_task;
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
                        title.Content = core.downloadstatus;
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
                            bitmap.WritePixels(new Int32Rect(0, 0, settings.ImageWidth, settings.ImageHeight), core.Pixels, settings.ImageWidth * 2, 0);

                            short minValue = 32767;
                            short maxValue = -32767;
                            int length = settings.ImageHeight * settings.ImageWidth;
                            for (int i = 0; i < length; i++)
                            {
                                if (core.Pixels[i] < minValue)
                                    minValue = core.Pixels[i];
                                if (core.Pixels[i] > maxValue)
                                    maxValue = core.Pixels[i];
                            }

                            float[] scaled_pixels = new float[length];

                            for (int i = 0; i < length; i++)
                            {
                                scaled_pixels[i] = ((float)core.Pixels[i] - (float)minValue) / (float)(maxValue - minValue);

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
        /// Draw any overlay files into the map_t image
        /// </summary>
        private void DrawOverlays()
        {
            if (Overlays.Count == 0)
                return;

            if (typeoverlay == null)
                return;

            var settings = core.settings;

            #region Create a bounding region in latitude and longitude
            MercatorProjection mp = new MercatorProjection(settings.StartLatitude, settings.StartLongitude);
            Region r = mp.GetRegion(settings.ImageWidth, settings.ImageHeight, (int)settings.PixelWidthInMetres);           
            #endregion

            foreach (ShapeFile s in Overlays)
            {
                s.Draw(core.map_t, 32, r, mp, (int)settings.PixelWidthInMetres, settings.ImageWidth, settings.ImageHeight);
            }
            UInt32[] types = new UInt32[settings.ImageHeight * settings.ImageWidth];

            for (int i = 0; i < settings.ImageHeight * settings.ImageWidth; i++)
            {
                byte baset = core.map_t[i];
                if ((baset & 32) == 0)
                    types[i] = core.ColourMappingForMapT[core.map_t[i]];
                else
                    types[i] = 0xffff0000;
            }

            typeoverlay.WritePixels(new Int32Rect(0, 0, settings.ImageWidth, settings.ImageHeight), types, settings.ImageWidth * 4, 0);
            core.UpdateMainDisplay(5);
        }

    }

}
