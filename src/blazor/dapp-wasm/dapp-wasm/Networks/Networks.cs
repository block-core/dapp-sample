using Blockcore.Networks.Bitcoin;
using Blockcore.Networks.Strax;

namespace Blockcore.Networks
{
    public static class Networks
    {
      public static Dictionary<string, Network> NetworkItems { get; } = new()
      {
        { "STRAX", Blockcore.Networks.Networks.Strax.Mainnet() },
        { "CITY", Blockcore.Networks.Networks.City.Mainnet() },
        { "BTC", Blockcore.Networks.Networks.Bitcoin.Mainnet() },
      };

    public static NetworksSelector Bitcoin
        {
            get
            {
                return new NetworksSelector(() => new BitcoinMain(), () => null, () => null);
            }
        }

        public static NetworksSelector Strax
        {
            get
            {
                return new NetworksSelector(() => new StraxMain(), () => null, () => null);
            }
        }

        public static NetworksSelector City
        {
            get
            {
                return new NetworksSelector(() => new City.Networks.CityMain(), () => null, () => null);
            }
        }
    }
}
