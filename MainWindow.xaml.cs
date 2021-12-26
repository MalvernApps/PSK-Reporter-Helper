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

namespace PSKReporterHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DownloadData();

            uNzIP();
        }

        private void DownloadData()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://www.pskreporter.info/cgi-bin/pskdata.pl?TXT=1&days=7&senderCallsign=M0JFG", "data.zip");
            }

        }

        private void uNzIP()
        {
            string startPath = @".";
            string zipPath = @".\data.zip";
            string extractPath = @".\extract";

            // extract to the extract dir
            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }
    }
}
