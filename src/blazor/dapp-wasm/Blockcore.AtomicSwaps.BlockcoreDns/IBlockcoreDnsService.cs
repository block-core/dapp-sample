
using Blockcore.AtomicSwaps.BlockcoreDns.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreDns
{
    public interface IBlockcoreDnsService
    {
        string GetDnsServiceUrl();
        ValueTask<IList<NsResult>> GetNsServices(string url);
        ValueTask<IList<DnsServices>> GetServicesByType(string type);
        ValueTask<IList<DnsServices>> GetServicesByNetwork(string network);
        ValueTask<IList<DnsServices>> GetServicesByTypeAndNetwork(string type, string network);
    }
}