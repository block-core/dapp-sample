using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Blockcore.AtomicSwaps.BlockcoreDns.Toolkit
{
    public class JsonToolKit<T> where T : class
    {
        public T ConverJsonToObject(string json)
        {
            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }

        public async Task<T> DownloadAndConverJsonToObjectAsync(string Url)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    Uri uri = new Uri(Url);
                    var json = await httpClient.GetStringAsync(uri);
                    var result = JsonConvert.DeserializeObject<T>(json);
                    return result;
                }
            }
            catch
            {
                return null;
            }

        }
    }
}
