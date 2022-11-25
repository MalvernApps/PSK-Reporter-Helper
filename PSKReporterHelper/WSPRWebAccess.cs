using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using wpfUtils;
//using wpfUtils;

namespace PSKReporterHelper
{
    class WSPRWebAccess
    {

        public List<Spot> spots = new List<Spot>();

        string downloadLimit = "1000000";

        bool RXMode = true;

        private String RXquery = @"http://db1.wspr.live/?query=SELECT * FROM wspr.rx WHERE time>'{0}' AND rx_sign='{1}' ORDER BY id LIMIT {2}";
        private String TXquery = @"http://db1.wspr.live/?query=SELECT * FROM wspr.rx WHERE time>'{0}' AND tx_sign='{1}' ORDER BY id LIMIT {2}";
        private object speechSynthesizer;

        private void Talk(string str)
        {
        }

        private string GetTime()
        {
            string t = "          ";
            DateTime d = DateTime.UtcNow;
            int diff = -24;
            d = d.AddHours(diff);

            t = String.Format("{0:yyyy-MM-dd HH:MM:ss}", d);

            return t;
        }

        private string GetQuery(string rx_callsign)
        {
            string q;

            rx_callsign = "M0JFG";
            string downloadLimit = "1000000";

            //q = string.Format( "http://db1.wspr.live/?query=SELECT * FROM wspr.rx WHERE time>'{0}' AND rx_sign='{1}' ORDER BY id LIMIT 10000", GetTime(), rx_callsign.Text);

            if (false)
                q = string.Format(RXquery, GetTime(), rx_callsign, downloadLimit);
            else
                q = string.Format(TXquery, GetTime(), rx_callsign, downloadLimit);

            return q;
        }

        private void ReadFile()
        {
            String line;
            int count = 0;

            // make sure old data is gone
            spots.Clear();
            spots = new List<Spot>();

            // Read the file and display it line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader(Globals.spotsFile);
            while ((line = file.ReadLine()) != null)
            {
                count++;
                Spot s = new Spot();
                s.str = line;
                s.Parse(false);
                spots.Add(s);

            }

            int downloadlimit = int.Parse(downloadLimit); ;

            if (downloadlimit == count)
            {
                Talk("download limit exceeded");
            }


        }

        public void download()
        {
            //if ( (bool) IsConnectedToInternet() == false )
            //{
            //    MessageBox.Show("No Internet", "ERROR", 0, MessageBoxImage.Error);
            //    return;
            //}

            WebClient webClient = new WebClient();
            webClient.DownloadFile(GetQuery("M0JFG"), Globals.spotsFile);
            webClient.Dispose();

            string dsize = new System.IO.FileInfo(Globals.spotsFile).Length.ToString();

            ReadFile();
        }


    }
}
