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
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        public Double OutputLatitude;
        public Double OutputLongitude;

        public Window3()
        {
            InitializeComponent();

            wbSample.Navigate("https://www.google.com/maps");
        }

        private void NewUrl(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            url.Text = e.Uri.OriginalString;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            OutputLatitude = Double.Parse(Latitude.Text);
            OutputLongitude = Double.Parse(Longitude.Text);
            this.DialogResult = true;
        }
    }
}
