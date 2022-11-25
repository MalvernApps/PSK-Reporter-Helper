using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpfUtils
{
    /// <summary>
    /// Interaction logic for ColourLine.xaml
    /// </summary>
    public partial class ColourLine : UserControl
    {
        public MyColours thisLine { get; set; }

        public ColourLine()
        {

        }

        public ColourLine(MyColours col)
        {
            thisLine = col;

            InitializeComponent();

            this.Loaded += ColourLine_Loaded;
        }

        public Color NewColor( uint value )
        {
            return Color.FromArgb((byte)((value >> 24) & 0xFF),
                       (byte)((value >> 16) & 0xFF),
                       (byte)((value >> 8) & 0xFF),
                       (byte)(value & 0xFF));
        }

        private  uint ToUint( Color c)
        {
            return (uint)(((c.A << 24) | (c.R << 16) | (c.G << 8) | c.B) & 0xffffffffL);
        }

        private void ColourLine_Loaded(object sender, RoutedEventArgs e)
        {
            myName.Content = thisLine.Name;
            myColour.SelectedColor = NewColor( thisLine.Col );
        }

        private void colchnaged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            thisLine.Col = ToUint( myColour.SelectedColor.Value);
        }
    }
}
