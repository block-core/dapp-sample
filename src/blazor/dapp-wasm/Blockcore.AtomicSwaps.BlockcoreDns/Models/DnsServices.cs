using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreDns.Models
{
	public class DnsServices
    {
        public string Url { get; set; }
        public List<DnsResult> DnsResults { get; set; }
    }
}
