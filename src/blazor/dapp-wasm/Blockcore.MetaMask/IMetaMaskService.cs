﻿using Blockcore.AtomicSwaps.MetaMask.Enums;
using Blockcore.AtomicSwaps.MetaMask.Extensions;
using Microsoft.JSInterop;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.MetaMask
{
    public interface IMetaMaskService
    {
        public static event Func<string, Task>? AccountChangedEvent;
        public static event Func<(long, Chain), Task>? ChainChangedEvent;

        ValueTask ConnectMetaMask();
        ValueTask DisposeAsync();
        ValueTask<dynamic> GenericRpc(string method, params dynamic[]? args);
        Task<BigInteger> GetBalance(string address, string block = "latest");
        ValueTask<string> GetSelectedAddress();
        ValueTask<(long chainId, Chain chain)> GetSelectedChain();
        ValueTask<long> GetTransactionCount();
        ValueTask<bool> HasMetaMask();
        ValueTask<bool> IsSiteConnected();
        ValueTask ListenToEvents();
        ValueTask<IJSObjectReference> LoadScripts(IJSRuntime jsRuntime);
        Task<string> RequestAccounts();
        ValueTask<string> SendTransaction(string to, BigInteger weiValue, string? data = null);
        ValueTask<string> SignTypedData(string label, string value);
        ValueTask<string> SignTypedDataV4(string typedData);

        [JSInvokable()]
        static async Task OnAccountsChanged(string selectedAccount)
        {
            if (AccountChangedEvent != null)
            {
                await AccountChangedEvent.Invoke(selectedAccount);
            }
        }

        [JSInvokable()]
        static async Task OnChainChanged(string chainhex)
        {
            if (ChainChangedEvent != null)
            {
                await ChainChangedEvent.Invoke(ChainHexToChainResponse(chainhex));
            }
        }

        protected static (long chainId, Chain chain) ChainHexToChainResponse(string chainHex)
        {
            long chainId = chainHex.HexToInt();
            return (chainId, (Chain)chainId);
        }
    }
}