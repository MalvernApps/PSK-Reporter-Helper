using maidenhead;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSKReporterHelper
{
    /// <summary>
    /// needs to handle
    /// "sNR","mode","MHz","rxTime","senderDXCC","flowStartSeconds","senderCallsign","senderLocator","receiverCallsign","receiverLocator","receiverAntennaInformation","senderDXCCADIF","submode"
    /// </summary>
    public class pskdata
    {
        public int ID { get; set; }


        public int snr { get; set; }

        public double MHz { get; set; }

        public double distance { get; set; }

        public double lat { get; set; }

        public double lng { get; set; }

        public string mode { get; set; }

        public string txCallsign { get; set; }

        public string rxCallsign { get; set; }

        public string rxlocation { get; set; }

        public string txlocation { get; set; }

        public string antenna { get; set; }

        public DateTime time { get; set; }

        public LatLng gps { get; set;  }

        public void parseline( string line )
        {
            string[] split = line.Split(',');

            snr = int.Parse(split[0]);

            mode = split[1];

            string s = split[2].Replace("\"", "");
            MHz = double.Parse(s);

            // time = DateTime.Parse(split[3], "YYY-MM-DD hh:mm:ss");
            string str = split[3].Replace("\"", "");
            time = DateTime.ParseExact(str , "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            txCallsign = split[6];
            txlocation = split[7].Replace("\"", ""); ;
            rxCallsign = split[8];
            rxlocation = split[9].Replace("\"", ""); ;

            antenna = split[10];
        }

        public string Tostring()
        {
            string str;

            str = distance.ToString("F1") + "KM  RxCallsign:" + rxCallsign + " SNR:" + snr + " " + rxlocation + " " + MHz;

            return str;

        }
    }
}
