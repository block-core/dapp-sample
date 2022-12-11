using Blockcore.Consensus;
using Blockcore.Consensus.BlockInfo;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
using Blockcore.P2P;
using NBitcoin;
using NBitcoin.BitcoinCore;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using System.Net;

namespace Blockcore.Networks.City.Networks
{
    public class CityMain : Network
    {
        public CityMain()
        {
            // START MODIFICATIONS OF GENERATED CODE
            var consensusOptions = new CityPosConsensusOptions
            {
                MaxBlockBaseSize = 1_000_000,
                MaxStandardVersion = 2,
                MaxStandardTxWeight = 100_000,
                MaxBlockSigopsCost = 20_000,
                MaxStandardTxSigopsCost = 20_000 / 5,
                WitnessScaleFactor = 4
            };

            // END MODIFICATIONS

            CoinSetup setup = CitySetup.Instance.Setup;
            NetworkSetup network = CitySetup.Instance.Main;

            NetworkType = NetworkType.Mainnet;
            DefaultConfigFilename = setup.ConfigFileName; // The default name used for the City configuration file.

            Name = network.Name;
            CoinTicker = network.CoinTicker;
            Magic = ConversionTools.ConvertToUInt32(setup.Magic);
            RootFolderName = network.RootFolderName;
            DefaultPort = network.DefaultPort;
            DefaultRPCPort = network.DefaultRPCPort;
            DefaultAPIPort = network.DefaultAPIPort;

            DefaultMaxOutboundConnections = 16;
            DefaultMaxInboundConnections = 109;
            MaxTipAge = 2 * 60 * 60;
            MinTxFee = 10000;
            MaxTxFee = Money.Coins(1).Satoshi;
            FallbackFee = 15000;
            MinRelayTxFee = 10000;
            MaxTimeOffsetSeconds = 25 * 60;
            DefaultBanTimeSeconds = 16000; // 500 (MaxReorg) * 64 (TargetSpacing) / 2 = 4 hours, 26 minutes and 40 seconds

            var consensusFactory = new PosConsensusFactory();

            GenesisTime = network.GenesisTime;
            GenesisNonce = network.GenesisNonce;
            GenesisBits = network.GenesisBits;
            GenesisVersion = network.GenesisVersion;
            GenesisReward = network.GenesisReward;

            consensusFactory.Protocol = new ConsensusProtocol()
            {
                ProtocolVersion = ProtocolVersion.FEEFILTER_VERSION,
                MinProtocolVersion = ProtocolVersion.POS_PROTOCOL_VERSION,
            };

            Consensus = new Blockcore.Consensus.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: setup.CoinType,
                hashGenesisBlock: null,
                subsidyHalvingInterval: 24333,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: null,
                bip9Deployments: null,
                bip34Hash: null,
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 500,
                defaultAssumeValid: null,
                maxMoney: long.MaxValue,
                coinbaseMaturity: 50,
                premineHeight: 2,
                premineReward: Money.Coins(setup.PremineReward),
                proofOfWorkReward: Money.Coins(setup.PoWBlockReward),
                targetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
                targetSpacing: setup.TargetSpacing,
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: setup.LastPowBlock,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(setup.PoSBlockReward),
                proofOfStakeTimestampMask: setup.ProofOfStakeTimestampMask
            );

            Consensus.PosEmptyCoinbase = true;
            Consensus.PosUseTimeFieldInKernalHash = true;

            // TODO: Set your Base58Prefixes
            Base58Prefixes = new byte[12][];
            Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (byte)network.PubKeyAddress };
            Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (byte)network.ScriptAddress };
            Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (byte)network.SecretAddress };

            Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };

            Bech32Encoders = new Bech32Encoder[2];
            var encoder = new Bech32Encoder(network.CoinTicker.ToLowerInvariant());
            Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            DNSSeeds = network.DNS.Select(dns => new DNSSeedData(dns, dns)).ToList();
            SeedNodes = network.Nodes.Select(node => new NBitcoin.Protocol.NetworkAddress(IPAddress.Parse(node), network.DefaultPort)).ToList();

            StandardScriptsRegistry = new CityStandardScriptsRegistry();



        }
    }

    public class CityStandardScriptsRegistry : StandardScriptsRegistry
    {
        // See MAX_OP_RETURN_RELAY in stratisX, <script.h>
        public const int MaxOpReturnRelay = 83;

        // Need a network-specific version of the template list
        private readonly List<ScriptTemplate> standardTemplates = new List<ScriptTemplate>
        {
            PayToPubkeyHashTemplate.Instance,
            PayToPubkeyTemplate.Instance,
            PayToScriptHashTemplate.Instance,
            PayToMultiSigTemplate.Instance,
            new TxNullDataTemplate(MaxOpReturnRelay),
            PayToWitTemplate.Instance
        };

        public override List<ScriptTemplate> GetScriptTemplates => standardTemplates;

        public override void RegisterStandardScriptTemplate(ScriptTemplate scriptTemplate)
        {
            if (!standardTemplates.Any(template => (template.Type == scriptTemplate.Type)))
            {
                standardTemplates.Add(scriptTemplate);
            }
        }

        public override bool IsStandardTransaction(Transaction tx, Network network)
        {
            return base.IsStandardTransaction(tx, network);
        }

        public override bool AreOutputsStandard(Network network, Transaction tx)
        {
            return base.AreOutputsStandard(network, tx);
        }

        public override ScriptTemplate GetTemplateFromScriptPubKey(Script script)
        {
            return standardTemplates.FirstOrDefault(t => t.CheckScriptPubKey(script));
        }

        public override bool IsStandardScriptPubKey(Network network, Script scriptPubKey)
        {
            return base.IsStandardScriptPubKey(network, scriptPubKey);
        }

        public override bool AreInputsStandard(Network network, Transaction tx, CoinsView coinsView)
        {
            return base.AreInputsStandard(network, tx, coinsView);
        }
    }

    public class CityPosConsensusOptions : PosConsensusOptions
    {
        /// <summary>Coinstake minimal confirmations softfork activation height for mainnet.</summary>
        public const int CityCoinstakeMinConfirmationActivationHeightMainnet = 500000;

        /// <summary>Coinstake minimal confirmations softfork activation height for testnet.</summary>
        public const int CityCoinstakeMinConfirmationActivationHeightTestnet = 15000;

        public override int GetStakeMinConfirmations(int height, Network network)
        {
            if (network.Name.ToLowerInvariant().Contains("test"))
            {
                return height < CityCoinstakeMinConfirmationActivationHeightTestnet ? 10 : 20;
            }

            // The coinstake confirmation minimum should be 50 until activation at height 500K (~347 days).
            return height < CityCoinstakeMinConfirmationActivationHeightMainnet ? 50 : 500;
        }
    }

    internal class CitySetup
    {
        internal static CitySetup Instance = new CitySetup();

        internal CoinSetup Setup = new CoinSetup
        {
            FileNamePrefix = "city",
            ConfigFileName = "city.conf",
            Magic = "01-59-54-43",
            CoinType = 1926, // SLIP-0044: https://github.com/satoshilabs/slips/blob/master/slip-0044.md,
            PremineReward = 13736000000,
            PoWBlockReward = 2,
            PoSBlockReward = 20,
            LastPowBlock = 2500,
            GenesisText = "July 27, 2018, New Scientiest, Bitcoin’s roots are in anarcho-capitalism", // The New York Times, 2020-04-16
            TargetSpacing = TimeSpan.FromSeconds(64),
            ProofOfStakeTimestampMask = 0x0000000F, // 0x0000003F // 64 sec
            PoSVersion = 3
        };

        internal NetworkSetup Main = new NetworkSetup
        {
            Name = "CityMain",
            RootFolderName = "city",
            CoinTicker = "CITY",
            DefaultPort = 4333,
            DefaultRPCPort = 4334,
            DefaultAPIPort = 4335,
            DefaultSignalRPort = 4336,
            PubKeyAddress = 28, // B https://en.bitcoin.it/wiki/List_of_address_prefixes
            ScriptAddress = 88, // b
            SecretAddress = 237,
            GenesisTime = 1538481600,
            GenesisNonce = 1626464,
            GenesisBits = 0x1E0FFFFF,
            GenesisVersion = 1,
            GenesisReward = Money.Zero,
            HashGenesisBlock = "00000b0517068e602ed5279c20168cfa1e69884ee4e784909652da34c361bff2",
            HashMerkleRoot = "b3425d46594a954b141898c7eebe369c6e6a35d2dab393c1f495504d2147883b",
            DNS = new[] { "seed.city-chain.org", "seed.city-coin.org", "seed.liberstad.com", "city.seed.blockcore.net" },
            Nodes = new[] { "95.217.210.139", "195.201.16.145", "89.10.224.54" },
        };

        internal NetworkSetup Test = new NetworkSetup
        {
            Name = "CityTest",
            RootFolderName = "citytest",
            CoinTicker = "TCITY",
            DefaultPort = 24333,
            DefaultRPCPort = 24334,
            DefaultAPIPort = 24335,
            DefaultSignalRPort = 24336,
            PubKeyAddress = 66,
            ScriptAddress = 196,
            SecretAddress = 194,
            GenesisTime = 1587115303,
            GenesisNonce = 3451,
            GenesisBits = 0x1F0FFFFF,
            GenesisVersion = 1,
            GenesisReward = Money.Zero,
            HashGenesisBlock = "00090058f8a37e4190aa341ab9605d74b282f0c80983a676ac44b0689be0fae1",
            HashMerkleRoot = "88cd7db112380c4d6d4609372b04cdd56c4f82979b7c3bf8c8a764f19859961f",
            DNS = new[] { "seedtest1.city.blockcore.net", "seedtest2.city.blockcore.net", "seedtest.city.blockcore.net" },
            Nodes = new[] { "23.97.234.230", "13.73.143.193", "89.10.227.34" },
           
        };

        public bool IsPoSv3()
        {
            return Setup.PoSVersion == 3;
        }

        public bool IsPoSv4()
        {
            return Setup.PoSVersion == 4;
        }
    }
    public class ConversionTools
    {
        public static uint ConvertToUInt32(string magicText, bool reverse = false)
        {
            byte[] number = magicText.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();

            if (reverse)
            {
                Array.Reverse(number);
            }

            return BitConverter.ToUInt32(number);
        }
    }

    internal class CoinSetup
    {
        internal string FileNamePrefix;
        internal string ConfigFileName;
        internal string Magic;
        internal int CoinType;
        internal decimal PremineReward;
        internal decimal PoWBlockReward;
        internal decimal PoSBlockReward;
        internal int LastPowBlock;
        internal string GenesisText;
        internal TimeSpan TargetSpacing;
        internal uint ProofOfStakeTimestampMask;
        internal int PoSVersion;
    }

    internal class NetworkSetup
    {
        internal string Name;
        internal string RootFolderName;
        internal string CoinTicker;
        internal int DefaultPort;
        internal int DefaultRPCPort;
        internal int DefaultAPIPort;
        internal int DefaultSignalRPort;
        internal int PubKeyAddress;
        internal int ScriptAddress;
        internal int SecretAddress;
        internal uint GenesisTime;
        internal uint GenesisNonce;
        internal uint GenesisBits;
        internal int GenesisVersion;
        internal Money GenesisReward;
        internal string HashGenesisBlock;
        internal string HashMerkleRoot;
        internal string[] DNS;
        internal string[] Nodes;
    }
}