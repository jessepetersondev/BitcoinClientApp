using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using WatchOnlyWalletToolkit.Models;
using BitcoinClientApp.Services;
using Xunit;
using Moq;

namespace BitcoinClientApp.Tests.Services
{
    public class WatchOnlyServiceTests
    {
        private readonly Mock<IWatchOnlyService> _mockWatchOnlyService;
        
        // Valid xpub for testing (this is a test/example xpub, not a real one with funds)
        private readonly string _testXpub = "xpub6CUGRUonZSQ4TWtTMmzXdrXDtypWKiKrhko4egpiMZbpiaQL2jkwSB1icqYh2cfDfVxdx4df189oLKnC5fSwqPfgyP3hooxujYzAu3fDVmz";

        public WatchOnlyServiceTests()
        {
            _mockWatchOnlyService = new Mock<IWatchOnlyService>();
        }

        [Fact]
        public void Parse_ShouldReturnExtPubKeyFromValidXpub()
        {
            // Arrange
            var expectedExtPubKey = ExtPubKey.Parse(_testXpub, Network.Main);
            
            // Setup mock
            _mockWatchOnlyService.Setup(w => w.Parse(_testXpub))
                .Returns(expectedExtPubKey);
                
            // Act
            var result = _mockWatchOnlyService.Object.Parse(_testXpub);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExtPubKey>(result);
            
            // Verify mock was called with correct parameters
            _mockWatchOnlyService.Verify(w => w.Parse(_testXpub), Times.Once);
        }

        [Fact]
        public void Derive_ShouldDeriveCorrectNumberOfAddresses()
        {
            // Arrange
            var extPubKey = ExtPubKey.Parse(_testXpub, Network.Main);
            int change = 0; // External chain
            int start = 0;
            int count = 5;
            
            // Create expected addresses
            var expectedAddresses = new List<BitcoinAddress>();
            for (int i = 0; i < count; i++)
            {
                var childPubKey = extPubKey.Derive((uint)change).Derive((uint)(start + i)).PubKey;
                expectedAddresses.Add(childPubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main));
            }

            // Setup mock
            _mockWatchOnlyService.Setup(w => w.Derive(extPubKey, change, start, count))
                .Returns(expectedAddresses);

            // Act
            var addresses = _mockWatchOnlyService.Object.Derive(extPubKey, change, start, count).ToList();

            // Assert
            Assert.NotNull(addresses);
            Assert.Equal(count, addresses.Count);
            
            // Each address should be a valid Bitcoin address
            foreach (var address in addresses)
            {
                Assert.IsAssignableFrom<BitcoinAddress>(address);
                // Verify address is not null which implies it's valid
                Assert.NotNull(address);
            }
            
            // Addresses should be different
            Assert.Equal(count, addresses.Distinct().Count());
            
            // Verify mock was called with correct parameters
            _mockWatchOnlyService.Verify(w => w.Derive(extPubKey, change, start, count), Times.Once);
        }

        [Fact]
        public void Filter_ShouldReturnUtxosBelongingToAddressesInRange()
        {
            // Arrange
            var extPubKey = ExtPubKey.Parse(_testXpub, Network.Main);
            int change = 0;
            int start = 0;
            int count = 5;
            
            // Create addresses for the range
            var addresses = new List<BitcoinAddress>();
            for (int i = 0; i < count; i++)
            {
                var childPubKey = extPubKey.Derive((uint)change).Derive((uint)(start + i)).PubKey;
                addresses.Add(childPubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main));
            }
            
            // Create test UTXOs - some belonging to derived addresses, some not
            var allUtxos = new List<Utxo>();
            
            var outpoint1 = new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000001"), 0);
            var outpoint2 = new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000002"), 0);
            var outpoint3 = new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000003"), 0);
            
            // Add a UTXO for the first derived address
            allUtxos.Add(new Utxo(
                outpoint1, 
                Money.Coins(1), 
                addresses[0].ScriptPubKey
            ));
            
            // Add a UTXO for an address outside our range
            var key = new Key();
            var outsideAddress = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            allUtxos.Add(new Utxo(
                outpoint2,
                Money.Coins(2),
                outsideAddress.ScriptPubKey
            ));
            
            // Add another UTXO for a derived address
            allUtxos.Add(new Utxo(
                outpoint3,
                Money.Coins(3),
                addresses[2].ScriptPubKey
            ));

            // Expected filtered UTXOs
            var expectedFilteredUtxos = new List<Utxo>
            {
                allUtxos[0], // The one with outpoint1
                allUtxos[2]  // The one with outpoint3
            };
            
            // Setup mocks
            _mockWatchOnlyService.Setup(w => w.Filter(extPubKey, allUtxos, change, start, count))
                .Returns(expectedFilteredUtxos);

            // Act
            var filteredUtxos = _mockWatchOnlyService.Object.Filter(extPubKey, allUtxos, change, start, count).ToList();

            // Assert
            Assert.Equal(2, filteredUtxos.Count); // Should only include the 2 UTXOs from derived addresses
            Assert.Contains(filteredUtxos, u => u.Outpoint.Hash.ToString() == "0000000000000000000000000000000000000000000000000000000000000001");
            Assert.Contains(filteredUtxos, u => u.Outpoint.Hash.ToString() == "0000000000000000000000000000000000000000000000000000000000000003");
            Assert.DoesNotContain(filteredUtxos, u => u.Outpoint.Hash.ToString() == "0000000000000000000000000000000000000000000000000000000000000002");
            
            // Verify mock was called with correct parameters
            _mockWatchOnlyService.Verify(w => w.Filter(extPubKey, allUtxos, change, start, count), Times.Once);
        }
    }
} 