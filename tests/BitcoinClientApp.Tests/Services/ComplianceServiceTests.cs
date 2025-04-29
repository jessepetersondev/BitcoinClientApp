using BitcoinClientApp.Services;
using BtcComplianceToolkit.Models;
using NBitcoin;

namespace BitcoinClientApp.Tests.Services
{
    public class ComplianceServiceTests
    {
        private readonly Mock<IComplianceService> _mockComplianceService;

        public ComplianceServiceTests()
        {
            _mockComplianceService = new Mock<IComplianceService>();
        }

        [Fact]
        public void FindOfac_ShouldIdentifyOfacListedAddresses()
        {
            // Arrange
            // Create keys and addresses instead of parsing strings
            var ofacKey1 = new Key();
            var ofacKey2 = new Key();
            var legitKey1 = new Key();
            var legitKey2 = new Key();

            // Create a test set of OFAC addresses
            var ofacAddr1 = ofacKey1.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            var ofacAddr2 = ofacKey2.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            var ofacAddressList = new HashSet<BitcoinAddress> { ofacAddr1, ofacAddr2 };

            var legitScript = legitKey1.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main).ScriptPubKey;
            var ofacScript = ofacAddr1.ScriptPubKey;
            var anotherLegitScript = legitKey2.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main).ScriptPubKey;

            // Create test UTXOs
            var utxos = new List<Utxo>
            {
                // UTXO for a legitimate address
                new Utxo(
                    new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000001"), 0),
                    Money.Coins(1),
                    legitScript
                ),
                // UTXO for a sanctioned address
                new Utxo(
                    new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000002"), 1),
                    Money.Coins(2),
                    ofacScript
                ),
                // Another UTXO for a legitimate address
                new Utxo(
                    new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000003"), 0),
                    Money.Coins(0.5m),
                    anotherLegitScript
                )
            };
            
            // Setup the mock to return only the sanctioned UTXO
            var expectedHits = new List<Utxo>
            {
                new Utxo(
                    new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000002"), 1),
                    Money.Coins(2),
                    ofacScript
                )
            };
            
            _mockComplianceService.Setup(c => c.FindOfac(
                It.IsAny<IEnumerable<Utxo>>(), 
                It.IsAny<HashSet<BitcoinAddress>>()))
                .Returns(expectedHits);

            // Act
            var ofacHits = _mockComplianceService.Object.FindOfac(utxos, ofacAddressList).ToList();

            // Assert
            Assert.Single(ofacHits);
            var hit = ofacHits.First();
            Assert.Equal("0000000000000000000000000000000000000000000000000000000000000002", hit.Outpoint.Hash.ToString());
            Assert.Equal(1u, hit.Outpoint.N);
            Assert.Equal(Money.Coins(2), hit.Amount);
            
            // Verify method was called with correct parameters
            _mockComplianceService.Verify(c => c.FindOfac(
                It.Is<IEnumerable<Utxo>>(u => u.Count() == utxos.Count()),
                It.Is<HashSet<BitcoinAddress>>(a => a.Count == ofacAddressList.Count)), 
                Times.Once);
        }

        [Fact]
        public void HasRisky_ShouldIdentifyRiskyScripts()
        {
            // Arrange
            // Create a transaction with a potentially risky script (for example, OP_RETURN)
            var tx = Transaction.Create(Network.Main);
            
            // Add a normal P2PKH output
            var normalKey = new Key();
            var normalAddress = normalKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            tx.Outputs.Add(new TxOut(Money.Coins(1), normalAddress.ScriptPubKey));
            
            // Add an OP_RETURN output (considered risky in most compliance systems)
            var opReturnScript = TxNullDataTemplate.Instance.GenerateScriptPubKey(new byte[] { 1, 2, 3, 4 });
            tx.Outputs.Add(new TxOut(Money.Zero, opReturnScript));
            
            // Setup the mock to return true for this transaction
            _mockComplianceService.Setup(c => c.HasRisky(tx))
                .Returns(true);

            // Act
            var hasRisky = _mockComplianceService.Object.HasRisky(tx);

            // Assert
            Assert.True(hasRisky);
            
            // Verify method was called with the correct transaction
            _mockComplianceService.Verify(c => c.HasRisky(tx), Times.Once);
        }

        [Fact]
        public void Cluster_ShouldGroupUtxosByScriptType()
        {
            // Arrange
            var p2pkhKey = new Key();
            var addressP2PKH = p2pkhKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            // Create a P2SH address
            var scriptPubKey = new Script(OpcodeType.OP_HASH160, Op.GetPushOp(new byte[20]), OpcodeType.OP_EQUAL);
            var addressP2SH = scriptPubKey.GetDestinationAddress(Network.Main);
            
            var p2pkhScript = addressP2PKH.ScriptPubKey;
            var p2shScript = scriptPubKey;
            
            var utxos = new List<Utxo>
            {
                // P2PKH UTXOs
                new Utxo(
                    new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000001"), 0),
                    Money.Coins(1),
                    p2pkhScript
                ),
                new Utxo(
                    new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000002"), 0),
                    Money.Coins(0.5m),
                    p2pkhScript
                ),
                // P2SH UTXO
                new Utxo(
                    new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000003"), 0),
                    Money.Coins(2),
                    p2shScript
                )
            };
            
            // Setup expected clusters
            var expectedClusters = new Dictionary<Script, List<Utxo>>
            {
                {
                    p2pkhScript,
                    new List<Utxo>
                    {
                        new Utxo(
                            new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000001"), 0),
                            Money.Coins(1),
                            p2pkhScript
                        ),
                        new Utxo(
                            new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000002"), 0),
                            Money.Coins(0.5m),
                            p2pkhScript
                        )
                    }
                },
                {
                    p2shScript,
                    new List<Utxo>
                    {
                        new Utxo(
                            new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000003"), 0),
                            Money.Coins(2),
                            p2shScript
                        )
                    }
                }
            };
            
            _mockComplianceService.Setup(c => c.Cluster(It.IsAny<IEnumerable<Utxo>>()))
                .Returns(expectedClusters);

            // Act
            var clusters = _mockComplianceService.Object.Cluster(utxos);

            // Assert
            Assert.Equal(2, clusters.Count); // Should have 2 clusters (P2PKH and P2SH)
            
            // Find the cluster for each script type
            var p2pkhCluster = clusters.FirstOrDefault(c => c.Key.ToString() == p2pkhScript.ToString());
            var p2shCluster = clusters.FirstOrDefault(c => c.Key.ToString() == p2shScript.ToString());
            
            // Verify the P2PKH cluster has 2 UTXOs
            Assert.Equal(2, p2pkhCluster.Value.Count);
            
            // Verify the P2SH cluster has 1 UTXO
            Assert.Single(p2shCluster.Value);
            
            // Verify the UTXOs are in the correct clusters
            Assert.Contains(p2pkhCluster.Value, u => u.Outpoint.Hash.ToString() == "0000000000000000000000000000000000000000000000000000000000000001");
            Assert.Contains(p2pkhCluster.Value, u => u.Outpoint.Hash.ToString() == "0000000000000000000000000000000000000000000000000000000000000002");
            Assert.Contains(p2shCluster.Value, u => u.Outpoint.Hash.ToString() == "0000000000000000000000000000000000000000000000000000000000000003");
            
            // Verify method was called with correct parameters
            _mockComplianceService.Verify(c => c.Cluster(It.Is<IEnumerable<Utxo>>(u => u.Count() == utxos.Count())), Times.Once);
        }
    }
} 