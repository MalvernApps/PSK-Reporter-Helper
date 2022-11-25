using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfUtils
{
    public class MyColours
    {
        public MyColours()
        {

        }

        public MyColours(uint col, string name)
        {
            Col = col;
            Name = name;
        }

        public uint Col { get;  set; }

        public string  Name { get;  set; }
    }
}
