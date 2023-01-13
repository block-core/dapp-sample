using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreDns.Models
{
    public class DnsResult
    {
        public string Domain { get; set; }
        public string Symbol { get; set; }
        public string Service { get; set; }
        public int Ttl { get; set; }
        public bool Online { get; set; }
    }
}
