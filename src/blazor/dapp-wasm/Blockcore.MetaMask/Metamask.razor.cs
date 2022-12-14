using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Blockcore.AtomicSwaps.MetaMask;
using Blockcore.AtomicSwaps.MetaMask.Enums;
using Blockcore.AtomicSwaps.MetaMask.Exceptions;
using Blockcore.AtomicSwaps.MetaMask.Models;
using Microsoft.AspNetCore.Components;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Nethereum.Web3;

namespace Blockcore.MetaMask
{

    public partial class Metamask : IDisposable
    {
        [Inject]
        public IMetaMaskService MetaMaskService { get; set; } = default!;

        public bool HasMetaMask { get; set; }
        public string? SelectedAddress { get; set; }
        public string? SelectedChain { get; set; }
        public string? TransactionCount { get; set; }
        public string? SignedData { get; set; }
        public string? SignedDataV4 { get; set; }
        public string? FunctionResult { get; set; }
        public string? RpcResult { get; set; }
        public Chain? Chain { get; set; }

        protected override async Task OnInitializedAsync()
        {
            //Subscribe to events
            IMetaMaskService.AccountChangedEvent += MetaMaskService_AccountChangedEvent;
            IMetaMaskService.ChainChangedEvent += MetaMaskService_ChainChangedEvent;

            HasMetaMask = await MetaMaskService.HasMetaMask();
            if (HasMetaMask)
                await MetaMaskService.ListenToEvents();

            bool isSiteConnected = await MetaMaskService.IsSiteConnected();
            if (isSiteConnected)
            {
                await GetSelectedAddress();
                await GetSelectedNetwork();
            }

        }

        private async Task MetaMaskService_ChainChangedEvent((long, Chain) arg)
        {
            await GetSelectedNetwork();
            StateHasChanged();
        }

        private async Task MetaMaskService_AccountChangedEvent(string arg)
        {
            await GetSelectedAddress();
            StateHasChanged();
        }

        public async Task ConnectMetaMask()
        {
            await MetaMaskService.ConnectMetaMask();
            await GetSelectedAddress();
        }

        public async Task GetSelectedAddress()
        {
            SelectedAddress = await MetaMaskService.GetSelectedAddress();
            Console.WriteLine($"Address: {SelectedAddress}");
        }

        public async Task GetSelectedNetwork()
        {
            var chainInfo = await MetaMaskService.GetSelectedChain();
            Chain = chainInfo.chain;

            SelectedChain = $"ChainID: {chainInfo.chainId}, Name: {chainInfo.chain.ToString()}";
            Console.WriteLine($"ChainID: {chainInfo.chainId}");
        }

        public async Task GetTransactionCount()
        {
            var transactionCount = await MetaMaskService.GetTransactionCount();
            TransactionCount = $"Transaction count: {transactionCount}";
        }

        public async Task SignData(string label, string value)
        {
            try
            {
                var result = await MetaMaskService.SignTypedData("test label", "test value");
                SignedData = $"Signed: {result}";
            }
            catch (UserDeniedException)
            {
                SignedData = "User Denied";
            }
            catch (Exception ex)
            {
                SignedData = $"Exception: {ex}";
            }
        }

        public async Task SignDataV4()
        {
            try
            {
                var chainInfo = await MetaMaskService.GetSelectedChain();

                var data = new TypedDataPayload<AtomicSwaps.MetaMask.Models.Message>
                {
                    Domain = new AtomicSwaps.MetaMask.Models.Domain
                    {
                        Name = "AAA",
                        Version = "1",
                        ChainId = chainInfo.chainId
                    },
                    Types = new Dictionary<string, TypeMemberValue[]>
                    {
                        ["EIP712Domain"] = new[]
                        {
                    new TypeMemberValue { Name = "name", Type = "string" },
                    new TypeMemberValue { Name = "version", Type = "string" },
                    new TypeMemberValue { Name = "chainId", Type = "uint256" }
                },
                        ["Message"] = new[]
                        {
                    new TypeMemberValue { Name = "contents", Type = "string" }
                }
                    },
                    PrimaryType = "Message",
                    Message = new AtomicSwaps.MetaMask.Models.Message
                    {
                        contents = "Blockcore"
                    }
                };

                var result = await MetaMaskService.SignTypedDataV4(data.ToJson());

                SignedDataV4 = $"Signed: {result}";
            }
            catch (UserDeniedException)
            {
                SignedDataV4 = "User Denied";
            }
            catch (Exception ex)
            {
                SignedDataV4 = $"Exception: {ex}";
            }
        }

        public async Task CallSmartContractFunctionExample1()
        {
            try
            {
                string address = "0x21253c6f5E16a56031dc8d8AF0bb372bc4A4DfDA";
                BigInteger weiValue = 0;
                string data = GetEncodedFunctionCall();

                data = data[2..]; //Remove the 0x from the generated string
                var result = await MetaMaskService.SendTransaction(address, weiValue, data);
                FunctionResult = $"TX Hash: {result}";
            }
            catch (UserDeniedException)
            {
                FunctionResult = "User Denied";
            }
            catch (Exception ex)
            {
                FunctionResult = $"Exception: {ex}";
            }
        }

        public async Task CallSmartContractFunctionExample2()
        {
            try
            {
                string address = "0x722BcdA7BD1a0f8C1c9b7c0eefabE36c1f0fBF2a";
                BigInteger weiValue = 1000000000000000;
                string data = GetEncodedFunctionCallExample2();

                data = data[2..]; //Remove the 0x from the generated string
                var result = await MetaMaskService.SendTransaction(address, weiValue, data);
                FunctionResult = $"TX Hash: {result}";
            }
            catch (UserDeniedException)
            {
                FunctionResult = "User Denied";
            }
            catch (Exception ex)
            {
                FunctionResult = $"Exception: {ex}";
            }
        }

        private string GetEncodedFunctionCall()
        {
            //This example uses Nethereum.ABI to create the ABI encoded string to call a smart contract function

            //Smart contract has a function called "share"
            FunctionABI function = new FunctionABI("share", false);

            //With 4 inputs
            var inputsParameters = new[] {
                    new Parameter("address", "receiver"),
                    new Parameter("string", "appId"),
                    new Parameter("string", "shareType"),
                    new Parameter("string", "data")
                };
            function.InputParameters = inputsParameters;

            var functionCallEncoder = new FunctionCallEncoder();

            var data = functionCallEncoder.EncodeRequest(function.Sha3Signature, inputsParameters,
                "0x92B143F46C3F8B4242bA85F800579cdF73882e98",
                "Blockcore.AtomicSwaps.MetaMask",
                "Sample",
                DateTime.UtcNow.ToString());
            return data;
        }

        private string GetEncodedFunctionCallExample2()
        {
            //This example uses Nethereum.ABI to create the ABI encoded string to call a smart contract function

            //Smart contract has a function called "share"
            FunctionABI function = new FunctionABI("setColor", false);

            //With 4 inputs
            var inputsParameters = new[] {
                    new Parameter("string", "color")
                };
            function.InputParameters = inputsParameters;

            var functionCallEncoder = new FunctionCallEncoder();

            var data = functionCallEncoder.EncodeRequest(function.Sha3Signature, inputsParameters, new object[] { "green" });

            return data;
        }


        public async Task GenericRpc()
        {
            var result = await MetaMaskService.RequestAccounts();
            RpcResult = $"RPC result: {result}";
        }

        public async Task GetBalance()
        {
            var address = await MetaMaskService.GetSelectedAddress();
            var result = await MetaMaskService.GetBalance(address);
            var balance = Web3.Convert.FromWei(result);
            RpcResult = $"Balance result: {Math.Round(balance, 4)} ETH";
        }

        public void Dispose()
        {
            IMetaMaskService.AccountChangedEvent -= MetaMaskService_AccountChangedEvent;
            IMetaMaskService.ChainChangedEvent -= MetaMaskService_ChainChangedEvent;
        }
    }

}
