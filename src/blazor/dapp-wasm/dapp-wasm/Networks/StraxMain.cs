using System.Drawing;
using Blockcore.Consensus;
using Blockcore.Consensus.BlockInfo;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
using NBitcoin;
using NBitcoin.DataEncoders;
using System.Text;

namespace Blockcore.Networks.Strax
{
    public class StraxMain : Network
    {
        public StraxMain()
        {
            this.Name = "StraxMain";
            this.NetworkType = NetworkType.Mainnet;
            this.Magic = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("StrX"));
            this.MinTxFee = 10000;
            this.MaxTxFee = Money.Coins(1).Satoshi;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.CoinTicker = "STRAX";

            var consensusFactory = new StraxConsensusFactory();

            var consensusOptions = new PosConsensusOptions
            {
                MaxBlockBaseSize = 1_000_000,
                MaxBlockSerializedSize = 4_000_000,
                MaxStandardVersion = 2,
                MaxStandardTxWeight = 150_000,
                MaxBlockSigopsCost = 20_000,
                MaxStandardTxSigopsCost = 20_000 / 5,
                WitnessScaleFactor = 4
            };

            this.Consensus = new Consensus.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 105105, // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
                hashGenesisBlock: null,
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: null,
                bip9Deployments: null,
                bip34Hash: null,
                minerConfirmationWindow: 2016,
                maxReorgLength: 500,
                defaultAssumeValid: null, // TODO: Set this once some checkpoint candidates have elapsed
                maxMoney: long.MaxValue,
                coinbaseMaturity: 50,
                premineHeight: 2,
                premineReward: Money.Coins(124987850),
                proofOfWorkReward: Money.Coins(18),
                targetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60),
                targetSpacing: TimeSpan.FromSeconds(45),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: null,
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 675,
                proofOfStakeLimit: null,
                proofOfStakeLimitV2: null,
                proofOfStakeReward: Money.Coins(18),
                proofOfStakeTimestampMask: 0x0000000F // 16 sec
            );

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { 75 }; // X
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { 140 }; // y
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (75 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };

            this.Bech32Encoders = new Bech32Encoder[2];
            var encoder = new Bech32Encoder("strax");
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.StandardScriptsRegistry = new StraxStandardScriptsRegistry();
        }
    }

    public class StraxTransaction : Transaction
    {
        public StraxTransaction() : base()
        {
        }

        public StraxTransaction(string hex, ConsensusFactory consensusFactory) : this()
        {
            this.FromBytes(Encoders.Hex.DecodeData(hex), consensusFactory);
        }

        public StraxTransaction(byte[] bytes) : this()
        {
            this.FromBytes(bytes);
        }

        public override bool IsProtocolTransaction()
        {
            return this.IsCoinStake || this.IsCoinBase;
        }
    }

    public class StraxPosBlockHeader : PosBlockHeader
    {
        // Indicates that the header contains additional fields.
        // The first field is a uint "Size" field to indicate the serialized size of additional fields.
        public const int ExtendedHeaderBit = 0x10000000;

        // Determines whether this object should serialize the new fields associated with smart contracts.
        public bool HasSmartContractFields => (this.version & ExtendedHeaderBit) != 0;

        /// <inheritdoc />
        public override int CurrentVersion => 7;

        private ushort extendedHeaderSize => (ushort)(hashStateRootSize + receiptRootSize + this.logsBloom.GetCompressedSize());

        /// <summary>
        /// Root of the state trie after execution of this block. 
        /// </summary>
        private uint256 hashStateRoot;
        public uint256 HashStateRoot { get { return this.hashStateRoot; } set { this.hashStateRoot = value; } }
        private static int hashStateRootSize = 32;

        /// <summary>
        /// Root of the receipt trie after execution of this block.
        /// </summary>
        private uint256 receiptRoot;
        public uint256 ReceiptRoot { get { return this.receiptRoot; } set { this.receiptRoot = value; } }
        private static int receiptRootSize = 32;

        /// <summary>
        /// Bitwise-OR of all the blooms generated from all of the smart contract transactions in the block.
        /// </summary>
        private Bloom logsBloom;
        public Bloom LogsBloom { get { return this.logsBloom; } set { this.logsBloom = value; } }

        public StraxPosBlockHeader()
        {
            this.hashStateRoot = 0;
            this.receiptRoot = 0;
            this.logsBloom = new Bloom();
        }

        #region IBitcoinSerializable Members

        public override void ReadWrite(BitcoinStream stream)
        {
            base.ReadWrite(stream);
            if (this.HasSmartContractFields)
            {
                stream.ReadWrite(ref this.hashStateRoot);
                stream.ReadWrite(ref this.receiptRoot);
                stream.ReadWriteCompressed(ref this.logsBloom);
            }
        }

        #endregion

        /// <summary>Populates stream with items that will be used during hash calculation.</summary>
        protected override void ReadWriteHashingStream(BitcoinStream stream)
        {
            base.ReadWriteHashingStream(stream);
            if (this.HasSmartContractFields)
            {
                stream.ReadWrite(ref this.hashStateRoot);
                stream.ReadWrite(ref this.receiptRoot);
                stream.ReadWriteCompressed(ref this.logsBloom);
            }
        }

        /// <summary>Gets the total header size - including the <see cref="Size"/> - in bytes.</summary>
        public override long HeaderSize => this.HasSmartContractFields ? Size + this.extendedHeaderSize : Size;
    }


    public class StraxConsensusFactory : PosConsensusFactory
    {
        /// <inheritdoc />
        public override T TryCreateNew<T>()
        {
            if (this.IsProvenBlockHeader<T>())
                return (T)(object)this.CreateProvenBlockHeader();

            return base.TryCreateNew<T>();
        }

        /// <inheritdoc />
        public override Block CreateBlock()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new PosBlock(this.CreateBlockHeader());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <inheritdoc />
        public override BlockHeader CreateBlockHeader()
        {
            return new StraxPosBlockHeader();
        }

        /// <inheritdoc />
        public override Transaction CreateTransaction()
        {
            return new StraxTransaction();
        }

        /// <inheritdoc />
        public override Transaction CreateTransaction(string hex)
        {
            return new StraxTransaction(hex, this);
        }

        /// <inheritdoc />
        public override Transaction CreateTransaction(byte[] bytes)
        {
            return new StraxTransaction(bytes);
        }
    }

    public class StraxStandardScriptsRegistry : StandardScriptsRegistry
    {
        public const int MaxOpReturnRelay = 83;

        // Need a network-specific version of the template list
        private static readonly List<ScriptTemplate> standardTemplates = new List<ScriptTemplate>
        {
            new PayToPubkeyHashTemplate(),
            new PayToPubkeyTemplate(),
            new PayToScriptHashTemplate(),
            new PayToMultiSigTemplate(),
            new TxNullDataTemplate(MaxOpReturnRelay),
            new PayToWitTemplate()
        };

        public override List<ScriptTemplate> GetScriptTemplates => standardTemplates;
    }
}