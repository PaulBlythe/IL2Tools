using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapMaker
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public ProjectSettings settings;

        public Window2()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cancel pressd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Okay pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            settings = new ProjectSettings();
            settings.StartLatitude = double.Parse(StartLatitude.Text);
            settings.StartLongitude = double.Parse(StartLongitude.Text);
            settings.PixelWidthInMetres = double.Parse(CellSize.Text);
            settings.UseMercatorProjection = (bool)Mercator.IsChecked;
            settings.ImageHeight = int.Parse(PixelHeight.Text);
            settings.ImageWidth = int.Parse(PixelWidth.Text);


            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Pick start location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Window3 win = new Window3();
            bool? result = win.ShowDialog();
            if (result.HasValue && result.Value)
            {
                StartLatitude.Text = win.OutputLatitude.ToString();
                StartLongitude.Text = win.OutputLongitude.ToString();
            }
        }
    }
}
