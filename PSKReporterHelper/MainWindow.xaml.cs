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
using System.Configuration;

using System.IO.Compression;
using System.IO;
using System.Globalization;
using maidenhead;
using GMap.NET;
using Microsoft.Win32;

namespace PSKReporterHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FilterEnum filtertype = FilterEnum.fifteen;
        public string testLocator = "io82uc";

        public List<pskdata> UnfilteredData = new List<pskdata>();
        public List<pskdata> FilteredData = new List<pskdata>();

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private void PlotHomeLocation()
        {
            // center onto the home position
            string myLocator = ConfigurationManager.AppSettings.Get("myLocation");
            LatLng coord = MaidenheadLocator.LocatorToLatLng(myLocator);

            pskdata ps = new pskdata();
            ps.txlocation = "IO82uc";
            ps.lng = -2;
            ps.snr = 1000;
            ps.distance = 0;
            ps.rxCallsign = "My HOME";

            ps.gps = coord;
            ps.lat = coord.Lat;
            ps.lng = coord.Long;

            AdCircledMarker(ps, 2);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // as a red sircle
            PlotHomeLocation();
            //pskdata ps = new pskdata();
            //ps.txlocation = "IO82uc";
            //ps.lng = -2;
            //ps.snr = 1000;
            //ps.distance = 0;

            //ps.gps = MaidenheadLocator.LocatorToLatLng(ps.txlocation);
            //ps.lat = ps.gps.Lat;
            //ps.lng = ps.gps.Long;

            //AdCircledMarker(ps, 2);
        }

        private void DownloadDataFromPSKReporter()
        {
            string mycallsign = ConfigurationManager.AppSettings.Get("myCallsign");

            using (var client = new WebClient())
            {
                string dwn = "https://www.pskreporter.info/cgi-bin/pskdata.pl?TXT=1&days=0.5&senderCallsign=" + mycallsign;
                client.DownloadFile(dwn, "data.zip");
                //client.DownloadFile("https://www.pskreporter.info/cgi-bin/pskdata.pl?TXT=1&hours=12&senderCallsign=M0JFG", "data.zip");
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

        private void ParsePSKFile()
        {
            bool first = true;
            foreach (string line in System.IO.File.ReadLines(@"extract\psk_data.csv"))
            {
                // System.Console.WriteLine(line);
                pskdata ps = new pskdata();
                if (first == false)
                    ps.parseline(line);

                first = false;

                if (ps.rxlocation != null && (ps.txlocation.Length < 7) && (ps.rxlocation.Length < 7))
                    UnfilteredData.Add(ps);
            }
        }

        private void Reader()
        {

            ParsePSKFile();
            if (UnfilteredData.Count == 0)
            {
                Console.WriteLine("No DATA");
                return;
            }

            foreach (pskdata c in UnfilteredData)
            {
                c.gps = MaidenheadLocator.LocatorToLatLng(c.rxlocation);
                c.distance = MaidenheadLocator.Distance(testLocator, c.rxlocation);
                Console.WriteLine(c.mode + " " + c.MHz + " " + c.rxlocation + " " + c.distance.ToString("F1"));
                c.lat = c.gps.Lat;
                c.lng = c.gps.Long;

                if (c.mode.Contains("FT8"))
                    AdCircledMarker(c , 20);

                if (c.distance > 1500)
                    Console.WriteLine("Here");
            }
        }

        private void map_Loaded(object sender, RoutedEventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            // choose your provider here
            //mapView.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            mapView.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
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

            // center onto the home position
            string myLocator = ConfigurationManager.AppSettings.Get("myLocation");

            LatLng coord = MaidenheadLocator.LocatorToLatLng(myLocator);

            PointLatLng cen = new PointLatLng(coord.Lat, coord.Long);
            mapView.Zoom = 7;
            mapView.Position = cen;

            mapView.Zoom = 2;
        }

        private Color GetColor(Int32 rangeStart /*Complete Red*/, Int32 rangeEnd /*Complete Green*/, Int32 actualValue)
        {
            if (rangeStart >= rangeEnd) return Colors.Black;

            Int32 max = rangeEnd - rangeStart; // make the scale start from 0
            Int32 value = actualValue - rangeStart; // adjust the value accordingly

            Int32 green = (255 * value) / max; // calculate green (the closer the value is to max, the greener it gets)
            Int32 red = 255 - green; // set red as inverse of green

            return Color.FromRgb((Byte)red, (Byte)green, (Byte)0);
        }

        private void AdCircledMarker(pskdata ps,  int band)
        {
            GMap.NET.WindowsPresentation.GMapMarker marker = new GMap.NET.WindowsPresentation.GMapMarker(new GMap.NET.PointLatLng(ps.lat, ps.lng));

            Brush col;

            col = new SolidColorBrush(GetColor(-25, 25, ps.snr));

            col.Freeze();

            marker.Shape = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5,
                ToolTip = ps.Tostring(),
                Visibility = Visibility.Visible,
                Fill = col,

            };

            mapView.Markers.Add(marker);
        }


        private void menuDownload(object sender, RoutedEventArgs e)
        {
            DownloadDataFromPSKReporter();

            UnZip();

            Reader();
        }

        private string getTxtFilename(string Title)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = Title;
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Text files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            //  "."; //  Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        //private void menuWSJTLoading(object sender, RoutedEventArgs e)
        //{
        //    string filename;

        //    filename = getTxtFilename("Looking for a All.TXT");

        //    List<WSJT_Data> allData = new List<WSJT_Data>();

        //    if (filename != null)
        //    {
        //        // we need to process the file
        //        const Int32 BufferSize = 128;
        //        using (var fileStream = File.OpenRead(filename))
        //        {
        //            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        //            {
        //                String line;
        //                while ((line = streamReader.ReadLine()) != null)
        //                {
        //                    if (line.Contains("CQ"))
        //                    {
        //                        Console.WriteLine(line);
        //                        string[] split;
        //                        split = line.Split(' ');

        //                        WSJT_Data data = new WSJT_Data();
        //                        // MaidenheadLocator.LocatorToLatLng(split[split.Count - 1]);
        //                        int count = split.Length;

        //                        data.Locator = split[count - 1];
        //                        if (data.Locator.Length == 4)
        //                        {
        //                            try
        //                            {

        //                                data.gps = MaidenheadLocator.LocatorToLatLng(data.Locator);
        //                                data.distance = MaidenheadLocator.Distance(testLocator, data.Locator);
        //                                data.Callsign = split[count - 2];
        //                                allData.Add(data);
        //                            }
        //                            catch (Exception ex)
        //                            {

        //                            }

        //                        }
        //                    }
        //                }
        //                // Process line
        //            }
        //        }
        //    }

        //    // now we can add to the map
        //    foreach (var c in allData)
        //    {
        //        AdCircledMarker(c, 2);
        //    }
        //}

        private void menuPSKFileLoading(object sender, RoutedEventArgs e)
        {
            if (getTxtFilename("Looking for a psk_data.CSV") != null)
            {
                // we need to process the file
                string str = testCallsign.Text.ToUpper();

                foreach( pskdata ps in UnfilteredData )
                {
                    if ( ps.rxCallsign == str )
                    {
                        result.Content = ps.Tostring();
                        return;
                    }
                }
            }
        }

        private void QueryCallsign(object sender, RoutedEventArgs e)
        {
            if (UnfilteredData.Count == 0)
            {

                result.Content = "No DATA TO WORK WITH!, DOWNLOAD SOME";
                return;
            }

            // we need to process the file
            string str = testCallsign.Text.ToUpper();

                foreach (pskdata ps in UnfilteredData)
                {
                    if (ps.rxCallsign.Contains(str) )
                    {
                        result.Content = "PROABABLY YES " + ps.Tostring();
                        return;
                    }
                }

            result.Content = "NO RESULT!";


        }

        private void DoTimeFilter( int minutes )
        {
            // remove all map markers
            mapView.Markers.Clear();

            DateTime now = DateTime.Now;
            now = now.AddMinutes(-minutes);

            FilteredData = new List<pskdata>();

            foreach (pskdata ps in UnfilteredData)
            {
                if (ps.time > now)
                {
                    FilteredData.Add(ps);
                    AdCircledMarker(ps, 20);
                }
            }
        }

        private void menulast15(object sender, RoutedEventArgs e)
        {
            DoTimeFilter(15);
        }

        private void menulast30(object sender, RoutedEventArgs e)
        {
            DoTimeFilter(30);
        }

        private void menulast60(object sender, RoutedEventArgs e)
        {
            DoTimeFilter(60);
        }

        private void menushowall(object sender, RoutedEventArgs e)
        {
            DoTimeFilter(1200);
        }

        private void menuWSJTLoading(object sender, RoutedEventArgs e)
        {

        }


        private void filterCheckedF1(object sender, RoutedEventArgs e)
        {
            filtertype = FilterEnum.fifteen;

            MessageBox.Show("Feature Not yet working");

            try
            {
                if (F2 == null) return;
                if (F3 == null) return;
                if (F4 == null) return;

                F2.IsChecked = false;
                F3.IsChecked = false;
                F4.IsChecked = false;
            }
            catch( Exception ex )
            {

            }
        }

        private void filterCheckedF2(object sender, RoutedEventArgs e)
        {
            filtertype = FilterEnum.thirty;

            MessageBox.Show("Feature Not yet working");

            try
            {
                if (F1 == null) return;
                if (F3 == null) return;
                if (F4 == null) return;

                F1.IsChecked = false;
                F3.IsChecked = false;
                F4.IsChecked = false;
            }
            catch (Exception ex)
            {

            }
        }

        private void filterCheckedF3(object sender, RoutedEventArgs e)
        {
            filtertype = FilterEnum.hour;

            MessageBox.Show("Feature Not yet working");

            try
            {
                if (F1 == null) return;
                if (F2 == null) return;
                if (F4 == null) return;

                F1.IsChecked = false;
                F2.IsChecked = false;
                F4.IsChecked = false;
            }
            catch (Exception ex)
            {

            }
        }

        private void filterCheckedF4(object sender, RoutedEventArgs e)
        {
            filtertype = FilterEnum.lastday;

            MessageBox.Show("Feature Not yet working");

            try
            {
                if (F1 == null) return;
                if (F2 == null) return;
                if (F3 == null) return;

                F1.IsChecked = false;
                F2.IsChecked = false;
                F3.IsChecked = false;
            }
            catch (Exception ex)
            {

            }
        }

        private void F1Unchecked(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
