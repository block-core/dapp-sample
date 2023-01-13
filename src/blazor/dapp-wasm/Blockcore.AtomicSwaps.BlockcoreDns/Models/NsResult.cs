using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreDns.Models
{
	public class NsResult
	{
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
