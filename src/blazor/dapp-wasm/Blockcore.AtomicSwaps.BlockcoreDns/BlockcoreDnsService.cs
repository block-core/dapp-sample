using Blockcore.AtomicSwaps.BlockcoreDns.Models;
using Blockcore.AtomicSwaps.BlockcoreDns.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreDns
{
    public class BlockcoreDnsService : IBlockcoreDnsService
    {
        private readonly string dnsServiceUrl = "https://chains.blockcore.net/services/DNS.json";

        public async ValueTask<IList<NsResult>> GetNsServices(string url)
        {
            var services = await new JsonToolKit<List<NsResult>>().DownloadAndConverJsonToObjectAsync(url);
            return services;
        }

        public async ValueTask<IList<DnsServices>> GetServicesByNetwork(string network)
        {
            if (string.IsNullOrEmpty(network))
            {
                return null;
            }
            var nsServices = await GetNsServices(dnsServiceUrl);
            if (nsServices.Any())
            {
                List<DnsServices> dnsservices = new List<DnsServices>();
                foreach (var ns in nsServices)
                {
                    List<DnsResult> dnsResult = new List<DnsResult>();
                    var result = await new JsonToolKit<List<DnsResult>>().DownloadAndConverJsonToObjectAsync(ns.Url + "/api/dns/services/symbol/"+ network);
                    if (result.Any())
                    {
                        dnsResult.AddRange(result);                   
                        dnsservices.Add(new DnsServices { Url = ns.Url, DnsResults = dnsResult });
                    }
                }
                return dnsservices;
            }
            return null;
        }

        public async ValueTask<IList<DnsServices>> GetServicesByType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }
            var nsServices = await GetNsServices(dnsServiceUrl);
            if (nsServices.Any())
            {
                List<DnsServices> dnsservices = new List<DnsServices>();
                foreach (var ns in nsServices)
                {
                    List<DnsResult> dnsResult = new List<DnsResult>();
                    var result = await new JsonToolKit<List<DnsResult>>().DownloadAndConverJsonToObjectAsync(ns.Url + "/api/dns/services/service/" + type);
                    if (result.Any())
                    {
                        dnsResult.AddRange(result);                    
                        dnsservices.Add(new DnsServices { Url = ns.Url, DnsResults = dnsResult });
                    }
                }
                return dnsservices;
            }
            return null;
        }

        public async ValueTask<IList<DnsServices>> GetServicesByTypeAndNetwork(string type, string network)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(network))
            {
                return null;
            }

            var nsServices = await GetNsServices(dnsServiceUrl);
            if (nsServices.Any())
            {
                List<DnsServices> dnsservices = new List<DnsServices>();
                foreach (var ns in nsServices)
                {
                    List<DnsResult> dnsResult = new List<DnsResult>();
                    var result = await new JsonToolKit<List<DnsResult>>().DownloadAndConverJsonToObjectAsync(ns.Url + "/api/dns/services/symbol/"+ network + "/service/" + type);
                    if (result.Any())
                    {
                        dnsResult.AddRange(result);
                        dnsservices.Add(new DnsServices { Url = ns.Url, DnsResults = dnsResult });
                    }
                }
                return dnsservices;
            }
            return null;
        }

        public string GetDnsServiceUrl()
        {
            return dnsServiceUrl;
        }

    }
}
