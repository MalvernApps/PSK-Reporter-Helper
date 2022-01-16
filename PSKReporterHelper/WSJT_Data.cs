using maidenhead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSKReporterHelper
{
    class WSJT_Data
    {
        /// <summary>
        ///  GPS coordinate
        /// </summary>
        public LatLng gps { get; set; }

        /// <summary>
        /// maidenhead locator
        /// </summary>
        public string Locator { get; set; }

        /// <summary>
        /// tramsmitter callsign
        /// </summary>
        public string Callsign { get; set; }
        
        /// <summary>
        /// distance away
        /// </summary>
        public double distance { get; set; }
    }
}
