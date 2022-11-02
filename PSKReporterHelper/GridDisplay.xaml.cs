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

namespace PSKReporterHelper
{
    /// <summary>
    /// Interaction logic for GridDisplay.xaml
    /// </summary>
    public partial class GridDisplay : Window
    {
        List<pskdata> data = new List<pskdata>();

        public GridDisplay( List<pskdata> inData)
        {
            InitializeComponent();

            data = inData;

            this.Loaded += GridDisplay_Loaded;
        }

        private void GridDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            mygrid.ItemsSource = data;
        }

        private void dblClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
