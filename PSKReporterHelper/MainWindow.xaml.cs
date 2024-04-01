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
using System.Threading;
using System.Windows.Threading;

namespace PSKReporterHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// update nugets
    /// /// update-package -reinstall 
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
    public static MainWindow Instance;

        private static System.Timers.Timer aTimer;

        FilterEnum filtertype = FilterEnum.fifteen;
        BandsEnum BandFilter = BandsEnum.Thrity;

        public string testLocator = "io82uc";

         public List<pskdata> UnfilteredData = new List<pskdata>();
        public List<pskdata> FilteredData = new List<pskdata>();

        public MainWindow()
        {
            InitializeComponent();

             Instance = this;

            this.Loaded += MainWindow_Loaded;
        }

    public string GetMyCallsign()
    {
      return Properties.Settings.Default.MyCallsign;
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

            AdCircledMarkerPSK(ps, 2);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            timeFilter.ItemsSource = Enum.GetValues(typeof(FilterEnum)).Cast<FilterEnum>();
            timeFilter.SelectedIndex = 3;

            bandFilter.ItemsSource = Enum.GetValues(typeof(BandsEnum)).Cast<BandsEnum>();
            bandFilter.SelectedIndex = 4;

            LoadCallsigns lcs = new LoadCallsigns();

      testcall.ItemsSource = lcs.callsigns; testcall.SelectedIndex = 5;

      testcall.SelectedIndex = 0;



						// as a red sircle
						PlotHomeLocation();

            Download();

            //SetTimer();
        }

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(10*1000*1);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            aTimer.Start();
        }

        private void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
             DispatcherPriority.Background,
             new Action(() =>
             {
                 Download();
             }));
                   // Console.WriteLine("Timer off");

        }

        private void DownloadDataFromPSKReporter()
        {
            string mycallsign = Properties.Settings.Default.MyCallsign;
			//string mycallsign = ConfigurationManager.AppSettings.Get("myCallsign");

			mycallsign = testcall.SelectedValue as string;


						using (var client = new WebClient())
            {
                string dwn = "https://www.pskreporter.info/cgi-bin/pskdata.pl?TXT=1&days=1.0&senderCallsign=" + mycallsign;
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
               //Console.WriteLine("No DATA");
                return;
            }

            foreach (pskdata c in UnfilteredData)
            {
                c.gps = MaidenheadLocator.LocatorToLatLng(c.rxlocation);
                c.distance = MaidenheadLocator.Distance(testLocator, c.rxlocation);
                //Console.WriteLine(c.mode + " " + c.MHz + " " + c.rxlocation + " " + c.distance.ToString("F1"));
                c.lat = c.gps.Lat;
                c.lng = c.gps.Long;

                if (c.mode.Contains("FT8") || c.mode.Contains("FT4"))
                    AdCircledMarkerPSK(c, 20);

                //if (c.distance > 1500)
                //    Console.WriteLine("Here");
            }
        }

        private void map_Loaded(object sender, RoutedEventArgs e)
        {
            AddLogString("App started: " + DateTime.Now.ToString() + "\r\n");
            AddLogString("App started: " + DateTime.UtcNow.ToString() + " UTC");

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

        /// <summary>
        /// from stack overflow
        /// </summary>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        /// <param name="actualValue"></param>
        /// <returns></returns>
        private Color GetColor(Int32 rangeStart /*Complete Red*/, Int32 rangeEnd /*Complete Green*/, Int32 actualValue)
        {
            if (rangeStart >= rangeEnd) return Colors.Black;

            Int32 max = rangeEnd - rangeStart; // make the scale start from 0
            Int32 value = actualValue - rangeStart; // adjust the value accordingly

            Int32 green = (255 * value) / max; // calculate green (the closer the value is to max, the greener it gets)
            Int32 red = 255 - green; // set red as inverse of green

            return Color.FromRgb((Byte)red, (Byte)green, (Byte)0);
        }

        private void AdCircledMarkerSpot( Spot s, int band)
        {
            GMap.NET.WindowsPresentation.GMapMarker marker = new GMap.NET.WindowsPresentation.GMapMarker(new GMap.NET.PointLatLng(s.gps.Lat, s.gps.Long));

            Brush col;

            col = new SolidColorBrush(GetColor((int) WSPR_min, (int) WSPR_max, (int) s.snr));

            col.Freeze();

            marker.Shape = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5,
                ToolTip = s.Tostring(),
                Visibility = Visibility.Visible,
                Fill = col,

            };

            mapView.Markers.Add(marker);
        }

        private void AdCircledMarkerPSK(pskdata ps,  int band)
        {
            GMap.NET.WindowsPresentation.GMapMarker marker = new GMap.NET.WindowsPresentation.GMapMarker(new GMap.NET.PointLatLng(ps.gps.Lat, ps.gps.Long));

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

        public void Download()
        {
            DownloadDataFromPSKReporter();

            UnZip();

            Reader();

            SetGridAsync();
        }


        private void menuDownload(object sender, RoutedEventArgs e)
        {
            Download();
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

        private void DoTimeFilter( int minutes, int band )
        {
            // remove all map markers
            mapView.Markers.Clear();

            DateTime now = DateTime.UtcNow;
            now = now.AddMinutes(-minutes);

            FilteredData = new List<pskdata>();

            foreach (pskdata ps in UnfilteredData)
            {
                if (ps.time > now && CheckFreq(ps.MHz, band) == true)
                {
                    FilteredData.Add(ps);
                    AdCircledMarkerPSK(ps, 20);
                }
            }
        }

        private bool CheckFreq( double mhz, int band )
        {if (band == -1) return true;

            double diff = Math.Abs(mhz - band);

            if (diff < 1.0) return true;

            return false; 
        }

        private void menuWSJTLoading(object sender, RoutedEventArgs e)
        {

        }


        private void F1Unchecked(object sender, RoutedEventArgs e)
        {
           
        }


        private void filterchanged(object sender, SelectionChangedEventArgs e)
        {
          
            filtertype = (FilterEnum)timeFilter.SelectedItem;
            int minutes;
            minutes = (int) filtertype;

            if ( bandFilter.SelectedItem != null )
            BandFilter = (BandsEnum) bandFilter.SelectedItem;
            int freq;
            freq = (int)BandFilter;

            DoTimeFilter(minutes, freq );

        }

        private void menuGrid(object sender, RoutedEventArgs e)
        {
            mytabctrl.SelectedIndex = 1;
            //GridDisplay gd = new GridDisplay(UnfilteredData);
            //gd.ShowDialog();
        }

        private void menuhelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("No help yet :)");
        }

        private void menuhome(object sender, RoutedEventArgs e)
        {

        }

        private void dblClick(object sender, MouseButtonEventArgs e)
        {

        }
        static private void Add( )
        {
            AddtoDatabaseAync(tmp);
        }

        static private void AddtoDatabaseAync(List<pskdata> data)
        {
            using (var ctx = new pskContext())
            {

                //Console.WriteLine(ctx.Data.Count());

                foreach (pskdata p in data)
                {

                    ctx.Data.Add(p);
                    ctx.SaveChanges();
                }                

                Console.WriteLine("thread done: " + ctx.Data.Count());
            }
        }

        static List<pskdata> tmp;
            
        private void SetGridAsync()
        {
            mygrid.ItemsSource = UnfilteredData;

            tmp = UnfilteredData;

            // Create a thread and call a background method   
           // Thread backgroundThread = new Thread(new ThreadStart( MainWindow.Add ));
            // Start thread  
           // backgroundThread.Start();
        }

        private void menuMap(object sender, RoutedEventArgs e)
        {
            mytabctrl.SelectedIndex = 0;
        }

        private void AddLogString( string lg )
        {
            mylog.Text += lg;
        }

        private void menuExperiment(object sender, RoutedEventArgs e)
        {
            // this is where I experiment

            using (StreamWriter outputFile = new StreamWriter("test.txt"))   
            {
                foreach (pskdata ps in UnfilteredData)
                    outputFile.WriteLine(ps.distance + "," + ps.snr);
                //foreach (string line in lines)
                //    outputFile.WriteLine(line);
            }
        }


        float WSPR_min, WSPR_max;

        private void closeddrop(object sender, EventArgs e)
        {
            string res = dropFreq.Text;
            int band = -1;

            switch( res )
            {
                case "All":
                    band = -1;
                    break;

                case "Twenty":
                    band = 14;
                    break;

                case "Thirty":
                    band = 10;
                    break;

                case "Seventen":
                    band = 18;
                    break;

                case "Twelse":
                    band = 24;
                    break;

                case "Ten":
                    band = 28;
                    break;

                default: break;
            }

            filtertype = (FilterEnum)timeFilter.SelectedItem;
            int minutes;
            minutes = (int)filtertype;


            int freq;
            freq = (int)band;

            DoTimeFilter(minutes, freq);
        }

        /// <summary>
        /// Get WSPR data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuWSPRDownload(object sender, RoutedEventArgs e)
        {
            WSPRWebAccess wwa = new WSPRWebAccess();
            wwa.download();

            mapView.Markers.Clear();

            WSPR_max = wwa.Spots.Max(r => r.snr);
            WSPR_min = wwa.Spots.Min(r => r.snr);

            WSPR_max = 32;

            if (WSPR_max > 5) WSPR_max = 5;

            foreach (Spot s in wwa.Spots )
            {
                s.gps = MaidenheadLocator.LocatorToLatLng(s.grid);
                s.distance = (float) MaidenheadLocator.Distance(testLocator, s.grid);
                //Console.WriteLine(c.mode + " " + c.MHz + " " + c.rxlocation + " " + c.distance.ToString("F1"));
                s.lat = s.gps.Lat;
                s.lng = s.gps.Long;

               // if (c.mode.Contains("FT8"))
               if ( s.snr < 5 )
                    AdCircledMarkerSpot(s, 20);

                //if (c.distance > 1500)
                //    Console.WriteLine("Here");
            }
        }
    }
}
