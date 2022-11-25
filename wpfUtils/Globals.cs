using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace wpfUtils
{
    /// <summary>
    /// trying to make this a global
    /// </summary>
    public static class Globals
    {

        public static void Initialise()
        {
            GlobalLines = new List<MyColours>();
            Load();
        }

        public static void Load()
        {
            string str = File.ReadAllText(Globals.ColoursFile);

            GlobalLines = (List<MyColours>)JsonConvert.DeserializeObject<List<MyColours>>(str);

            if(GlobalLines == null)
                {
                SetDefault();
                Save();
            }
        }

        public static void Save()
        {
            JsonSerializer serializer = new JsonSerializer();

            string str = JsonConvert.SerializeObject(GlobalLines, Formatting.Indented);

            File.WriteAllText(Globals.ColoursFile, str);
        }

        public static void SetDefault()
        {
            if (GlobalLines == null) GlobalLines = new List<MyColours>();

            MyColours c = new MyColours(123, "line");
            GlobalLines.Add(c);

            MyColours backgound = new MyColours(0, "background");
            GlobalLines.Add(backgound);

            MyColours pp = new MyColours(0, "purple");
            GlobalLines.Add(pp);

            MyColours pp0 = new MyColours(0, "pink");
            GlobalLines.Add(pp0);
        }

        /// <summary>
        /// global line colours
        /// </summary>
        public static List<MyColours> GlobalLines = new List<MyColours>();

        public static string spotsFile = "mySpots.json";

        public static string ColoursFile = "myColors.json";

        public static Color NewColor(uint value)
        {
            return Color.FromArgb((byte)((value >> 24) & 0xFF),
                       (byte)((value >> 16) & 0xFF),
                       (byte)((value >> 8) & 0xFF),
                       (byte)(value & 0xFF));
        }

        public static Brush GetBrush(string wanted)
        {
            SolidColorBrush brush = new SolidColorBrush(QueryColor(wanted));
            return brush;
        }

        public static Pen GetPen(string wanted, double width)
        {
            Pen p = new Pen(GetBrush(wanted), width);
            return p;
        }


        public static Color QueryColor(string wanted)
        {
            bool exists = GlobalLines.Where(p => p.Name == wanted).Any();

            if (exists == true)
            {
                MyColours acertainperson = GlobalLines.Where(p => p.Name == wanted).First();

                return NewColor(acertainperson.Col);
            }
            else
                return Colors.Black;
        }

    }
}
