using System;
using System.Threading.Tasks;
using Blockcore.AtomicSwaps.BlockcoreWallet;
using Blockcore.AtomicSwaps.BlockcoreWallet.Exceptions;
using Microsoft.AspNetCore.Components;


namespace Blockcore.AtomicSwaps.BlockcoreWallet
{

	public partial class BlockcoreWallet : IDisposable
	{
		private bool disposedValue;

		[Inject]
		public IBlockcoreWalletConnector BlockcoreWalletConnector { get; set; } = default!;
		public bool HasBlockcoreWallet { get; set; }
		string[] arr = new string[] { };

		public string? SignedMessage { get; set; }
		public string? SignedMessageAnyAccount { get; set; }
		public string? SignedMessageAnyAccountJson { get; set; }
		public string? PaymentRequestResult { get; set; }
		public string? DIDSupportedMethodsResult { get; set; }
		public string? DIDRequestResult { get; set; }

		protected override async Task OnInitializedAsync()
		{
			HasBlockcoreWallet = await BlockcoreWalletConnector.HasBlockcoreWallet();
		}

		public async Task SignMessageAnyAccount(string value)
		{
			var result = await BlockcoreWalletConnector.SignMessageAnyAccount(value);
			SignedMessageAnyAccount = $"Signed: {result}";
		}

		public async Task SignMessageAnyAccountJson(string value)
		{
			var result = await BlockcoreWalletConnector.SignMessageAnyAccountJson(value);
			SignedMessageAnyAccountJson = $"Signed: {result}";
		}

		public async Task PaymentRequest(string network, string amount)
		{
			var result = await BlockcoreWalletConnector.PaymentRequest(network, amount);
			PaymentRequestResult = $"{result}";
		}

		public async Task DIDSupportedMethods()
		{
			var result = await BlockcoreWalletConnector.DIDSupportedMethods();
			DIDSupportedMethodsResult = $"{result}";
		}

		public async Task DIDRequest(string[] methods)
		{
			var result = await BlockcoreWalletConnector.DIDRequest(methods);
			DIDRequestResult = $"{result}";
		}

		public async Task SignMessage(string message)
		{
			try
			{
				var result = await BlockcoreWalletConnector.SignMessage(message);
				SignedMessage = $"Signed: {result}";
			}
			catch (UserDeniedException)
			{
				SignedMessage = "User Denied";
			}
			catch (Exception ex)
			{
				SignedMessage = $"Exception: {ex}";
			}
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
