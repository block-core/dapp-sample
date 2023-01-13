using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blockcore.AtomicSwaps.BlockcoreDns.Models;
using Microsoft.AspNetCore.Components;

namespace Blockcore.AtomicSwaps.BlockcoreDns
{
    public partial class BlockcoreDns : IDisposable
    {
        private bool disposedValue;
        [Inject]
        public IBlockcoreDnsService blockcoreDnsService { get; set; } = default!;
        public string? blockcoreDnsUrl { get; set; }
        public string? network { get; set; }
        public string? type { get; set; }
        public string textColor { get; set; } = "text-success";

        public IList<DnsServices> services { get; set; }

        protected override Task OnInitializedAsync()
        {
            blockcoreDnsUrl = blockcoreDnsService.GetDnsServiceUrl();
            return Task.CompletedTask;
        }

        public async Task GetServicesByNetwork()
        {
            services = await blockcoreDnsService.GetServicesByNetwork(network);
        }

        public async Task GetServicesByType()
        {
            services = await blockcoreDnsService.GetServicesByType(type);
        }

        public async Task GetServicesByTypeAndNetwork()
        {
            services = await blockcoreDnsService.GetServicesByTypeAndNetwork(type,network);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


}
