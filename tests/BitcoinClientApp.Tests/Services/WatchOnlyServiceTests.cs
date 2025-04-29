using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using WatchOnlyWalletToolkit.Models;
using BitcoinClientApp.Services;
using Xunit;

namespace BitcoinClientApp.Tests.Services
{
    public class WatchOnlyServiceTests
    {
        private readonly WatchOnlyService _watchOnlyService;
        
        // Valid xpub for testing (this is a test/example xpub, not a real one with funds)
        private readonly string _testXpub = "xpub6CUGRUonZSQ4TWtTMmzXdrXDtypWKiKrhko4egpiMZbpiaQL2jkwSB1icqYh2cfDfVxdx4df189oLKnC5fSwqPfgyP3hooxujYzAu3fDVmz";

        public WatchOnlyServiceTests()
        {
            _watchOnlyService = new WatchOnlyService();
        }

        [Fact]
        public void Parse_ShouldReturnExtPubKeyFromValidXpub()
        {
            // Act
            var result = _watchOnlyService.Parse(_testXpub);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExtPubKey>(result);
        }

        [Fact]
        public void Derive_ShouldDeriveCorrectNumberOfAddresses()
        {
            // Arrange
            var extPubKey = _watchOnlyService.Parse(_testXpub);
            int change = 0; // External chain
            int start = 0;
            int count = 5;

            // Act
            var addresses = _watchOnlyService.Derive(extPubKey, change, start, count).ToList();

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
        }

        [Fact]
        public void Filter_ShouldReturnUtxosBelongingToAddressesInRange()
        {
            // Arrange
            var extPubKey = _watchOnlyService.Parse(_testXpub);
            int change = 0;
            int start = 0;
            int count = 5;
            
            // Get addresses from the range
            var addresses = _watchOnlyService.Derive(extPubKey, change, start, count).ToList();
            
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

            // Act
            var filteredUtxos = _watchOnlyService.Filter(extPubKey, allUtxos, change, start, count).ToList();

            // Assert
            Assert.Equal(2, filteredUtxos.Count); // Should only include the 2 UTXOs from derived addresses
            Assert.Contains(filteredUtxos, u => u.Outpoint.Hash.ToString() == "0000000000000000000000000000000000000000000000000000000000000001");
            Assert.Contains(filteredUtxos, u => u.Outpoint.Hash.ToString() == "0000000000000000000000000000000000000000000000000000000000000003");
            Assert.DoesNotContain(filteredUtxos, u => u.Outpoint.Hash.ToString() == "0000000000000000000000000000000000000000000000000000000000000002");
        }
    }
} 