using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSKReporterHelper
{
    class pskContext : DbContext
    {
        public pskContext() : base()
        {

        }

        public DbSet<pskdata> Data { get; set; }
    }
}
