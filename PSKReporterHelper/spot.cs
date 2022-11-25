using maidenhead;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSKReporterHelper
{
    public class Spot
    {
        public Spot()
        {

        }

        public Spot(Spot oldData)
        {
            id = oldData.id;
            time = oldData.time;
            distance = oldData.distance;
            rx_sign = oldData.rx_sign;
            tx_sign = oldData.tx_sign;
            snr = oldData.snr;
            Latitude = oldData.Latitude;
            Longitude = oldData.Longitude;
            xaxis = oldData.xaxis;
            band = oldData.band;

        }

        public long id { get; set; }

        public double Power { get; set; }

        public DateTime time { get; set; }
        public string rx_sign { get; set; }
        public string tx_sign { get; set; }

        public string grid { get; set; }

        public float distance { get; set; }

        public float snr { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double xaxis { get; set; }

        public double tester { get; set; }

        public int band { get; set; }

        public LatLng gps { get; set; }

        public double lat { get; set; }
        public double lng { get; set; }

        [Browsable(false)]
        public string str;

        public void Parse(bool isRX)
        {
            string[] split;

            split = str.Split('\t');

            id = long.Parse(split[0]);

            rx_sign = split[3];
            tx_sign = split[7];

            if (isRX == true)
                grid = split[10];
            else
                grid = split[6];

            Power = double.Parse(split[15]);

            if (isRX == true)
            {
                Latitude = double.Parse(split[8]);
                Longitude = double.Parse(split[9]);
            }
            else
            {
                Latitude = double.Parse(split[4]);
                Longitude = double.Parse(split[5]);
            }

            distance = float.Parse(split[11]);
            snr = float.Parse(split[16]);

            time = DateTime.Parse(split[1]);

            band = int.Parse(split[2]);

            xaxis = ((double)(time.ToOADate())) * 24.0;

        }

        public string Tostring()
        {
            string str;

            str = distance.ToString("F1") + "KM  RxCallsign:" + rx_sign + " SNR:" + snr + " " + distance.ToString("F2") + " KM";

            return str;

        }
    }
}
