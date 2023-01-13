using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreWallet.Exceptions
{
    public class NoBlockcoreWalletException : ApplicationException
    {
        public NoBlockcoreWalletException() : base("BlockcoreWallet is not installed.")
        {

        }
    }
}
