using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO.Compression;
using System.IO;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using maidenhead;
using GMap.NET;

namespace PSKReporterHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string testLocator = "io82uc";

        public MainWindow()
        {
            InitializeComponent();

            DownloadDataFromPSKReporter();

            UnZip();

            Reader();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AdCircledMarker(52, -2, "hello home", 2 );
        }

        private void DownloadDataFromPSKReporter()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://www.pskreporter.info/cgi-bin/pskdata.pl?TXT=1&days=7&senderCallsign=M0JFG", "data.zip");
            }

        }
        
        private void UnZip()
        {
            //string startPath = @".";
            string zipPath = @".\data.zip";
            string extractPath = @".\extract";

            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }


            // extract to the extract dir
            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }

        private void Reader()
        {
            List<Model> data = new List<Model>();

            using (var reader = new StreamReader(@"extract\psk_data.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ModelClassMap>();
                var records = csv.GetRecords<Model>();
                data = records.ToList();

                Console.WriteLine("hi");

                foreach (Model c in data)
                {
                    c.gps = MaidenheadLocator.LocatorToLatLng( c.receiverLocator );
                    c.distance = MaidenheadLocator.Distance(testLocator, c.receiverLocator);
                    Console.WriteLine(c.mode + " " + c.MHz + " " + c.receiverLocator + " " + c.distance.ToString("F1"));

                    if ( c.mode == "FT8")
                    AdCircledMarker(c.gps.Lat, c.gps.Long, c.distance.ToString("F1") + " " + c.receiverCallsign, 2);

                    if (c.distance > 1500)
                        Console.WriteLine("Here");
                }
            } 

           
        }

        public class Model
        {
            public int sNR { get; set; }
            public string mode { get; set; }
            public double MHz { get; set; }
            public DateTime rxTime { get; set; }
            public string senderDXCC { get; set; }
            public int flowStartSeconds { get; set; }
            public string senderCallsign { get; set; }
            public string senderLocator { get; set; }
            public string receiverCallsign { get; set; }
            public string receiverLocator { get; set; }
            public string receiverAntennaInformation { get; set; }
            public int senderDXCCADIF { get; set; }
            public string submode { get; set; }

            public LatLng gps { get; set; }
            public double distance { get; set; }

            public double bearing { get; set; }
        }

        public class ModelClassMap : ClassMap<Model>
        {
            public ModelClassMap()
            {
                Map(m => m.sNR).Name("sNR");
                Map(m => m.mode).Name("mode");
                Map(m => m.MHz).Name("MHz");
                Map(m => m.rxTime).Name("rxTime");
                Map(m => m.senderDXCC).Name("senderDXCC");
                Map(m => m.flowStartSeconds).Name("flowStartSeconds");
                Map(m => m.senderCallsign).Name("senderCallsign");
                Map(m => m.senderLocator).Name("senderLocator");
                Map(m => m.receiverCallsign).Name("receiverCallsign");
                Map(m => m.receiverLocator).Name("receiverLocator");
                Map(m => m.receiverAntennaInformation).Name("receiverAntennaInformation");
                Map(m => m.senderDXCCADIF).Name("senderDXCCADIF");
                Map(m => m.submode).Name("submode");
            }
        }

        private void map_Loaded(object sender, RoutedEventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            // choose your provider here
            mapView.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            mapView.MinZoom = 1;
            mapView.MaxZoom = 24;
            mapView.Zoom = 2;

            mapView.Manager.Mode = AccessMode.ServerAndCache;
            mapView.CacheLocation = "map cache";

            // lets the map use the mousewheel to zoom
            mapView.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            mapView.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            mapView.DragButton = MouseButton.Left;

            PointLatLng cen = new PointLatLng(52.108154, -2.296509);
            mapView.Zoom = 7;
            mapView.Position = cen;
        }

        private void AdCircledMarker(double lat, double lng, string str, int band)
        {
            GMap.NET.WindowsPresentation.GMapMarker marker = new GMap.NET.WindowsPresentation.GMapMarker(new GMap.NET.PointLatLng(lat, lng));

            Brush col;

            switch (band)
            {
                case 2:
                    col = Brushes.RoyalBlue;
                    break;

                case 20:
                    col = Brushes.Violet;
                    break;

                case 21:
                    col = Brushes.BlueViolet;
                    break;

                case 28:
                    col = Brushes.Black;
                    break;

                case 18:
                    col = Brushes.Blue;
                    break;

                case 10:
                    col = Brushes.Yellow;
                    break;

                case 7:
                    col = Brushes.White;
                    break;

                default:
                    col = Brushes.Red;
                    break;
            }

            col.Freeze();

            marker.Shape = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5,
                ToolTip = str,
                Visibility = Visibility.Visible,
                Fill = col,

            };

            mapView.Markers.Add(marker);
        }
    }
}
