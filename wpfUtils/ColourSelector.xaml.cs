using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace wpfUtils
{
    /// <summary>
    /// Interaction logic for ColourSelector.xaml
    /// </summary>
    public partial class ColourSelector : Window
    {
        /// <summary>
        /// stuff goes in here
        /// </summary>

        // we are setting this to a global for easier access.
        List<MyColours> lines;  // = new List<MyColours>();
        int COUNT = 0;

        public ColourSelector(Window win)
        {
            // use the global storage area.
            lines = Globals.GlobalLines;

            InitializeComponent();

            //SetDefault();
            //Save();

            this.Closing += ColourSelector_Closing;

            Load();

            Color col = QueryColor("Pink");


            AddControls();

            Save();         
        }

        private void ColourSelector_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        public Color NewColor(uint value)
        {
            return Color.FromArgb((byte)((value >> 24) & 0xFF),
                       (byte)((value >> 16) & 0xFF),
                       (byte)((value >> 8) & 0xFF),
                       (byte)(value & 0xFF));
        }

        public Brush GetBrush( string wanted)
        {
            SolidColorBrush brush = new SolidColorBrush(QueryColor(wanted));
            return brush;
        }

        public Pen GetPen( string wanted, double width )
        {
            Pen p = new Pen(GetBrush(wanted), width);
            return p;
        }
            

        public Color QueryColor( string wanted )
        {
            bool exists = lines.Where(p => p.Name == wanted).Any();

            if (exists == true)
            {
                MyColours acertainperson = lines.Where(p => p.Name == wanted).First();

                return NewColor(acertainperson.Col);
            }
            else
                return Colors.Black;
        }

        public void AddControls()
        {
            foreach (MyColours myc in lines)
            { 
                MyColours c = new MyColours(myc.Col, myc.Name);
                ColourLine cl = new ColourLine(c);
                stacker.Children.Add(cl);
            }
        }

        public void SetDefault()
        {
            MyColours c = new MyColours( 123, "line");
            lines.Add(c);

            MyColours backgound = new MyColours( 0 , "backgound");
            lines.Add(backgound);

            MyColours pp = new MyColours(0, "purple");
            lines.Add(pp);

            MyColours pp0 = new MyColours(0, "pink");
            lines.Add(pp0);
        }

        public void Save()
        {
            JsonSerializer serializer = new JsonSerializer();

            string str =  JsonConvert.SerializeObject(lines, Formatting.Indented);

            File.WriteAllText(Globals.ColoursFile, str);
        }

        public void Load()
        {
            string str = File.ReadAllText(Globals.ColoursFile);

            Globals.GlobalLines = (List<MyColours>) JsonConvert.DeserializeObject< List<MyColours>>(str);
        }

        private void btnSave(object sender, RoutedEventArgs e)
        {
            // we now need to harvest the data
            Globals.GlobalLines = new List<MyColours>();
            //lines = new List<MyColours>();

            foreach (object o in stacker.Children)
            {
                try
                {
                    ColourLine c = (ColourLine)o;
                    if (c.thisLine != null)
                        Globals.GlobalLines.Add(c.thisLine);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                }
            }

            Save();
        }

        private void btnCancel(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            //this.Close();
        }

        private void btnAdd(object sender, RoutedEventArgs e)
        {
            MyColours c = new MyColours( 0, newcolor.Text );
            ColourLine cl = new ColourLine(c);
            stacker.Children.Add(cl);

        }
    }
}
