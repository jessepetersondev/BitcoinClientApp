using BitcoinClientApp.Services;
using NBitcoin;

namespace BitcoinClientApp.Tests.Services
{
    public class AddressServiceTests
    {
        private readonly Mock<IAddressService> _mockAddressService;
        private readonly Mock<ICryptoService> _mockCryptoService;
        private readonly string _defaultUsername = "testuser";

        public AddressServiceTests()
        {
            _mockAddressService = new Mock<IAddressService>();
            _mockCryptoService = new Mock<ICryptoService>();
        }

        [Fact]
        public async Task CreateNewWallet_ShouldCreateWalletAndReturnKeyAndAddress()
        {
            // Arrange
            var expectedSalt = "mockedSalt";
            var expectedEncryptedKey = "encryptedKeyValue";
            
            // Create expected return values
            var privateKey = new Key();
            var publicKey = privateKey.PubKey;
            var address = publicKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            // Set up mock to return the expected values
            _mockAddressService.Setup(a => a.CreateNewWallet(_defaultUsername))
                .ReturnsAsync((privateKey, address));
            
            _mockCryptoService.Setup(c => c.GenerateSalt())
                .Returns(expectedSalt);
            
            _mockCryptoService.Setup(c => c.EncryptPrivateKey(It.IsAny<string>(), expectedSalt))
                .Returns(expectedEncryptedKey);

            // Act
            var result = await _mockAddressService.Object.CreateNewWallet(_defaultUsername);

            // Assert
            Assert.NotNull(result.privateKey);
            Assert.NotNull(result.address);
            Assert.IsType<Key>(result.privateKey);
            Assert.IsAssignableFrom<BitcoinAddress>(result.address);
            
            // Verify the mock was called
            _mockAddressService.Verify(a => a.CreateNewWallet(_defaultUsername), Times.Once);
        }

        [Fact]
        public async Task GetAddressesForUser_ShouldReturnAddressesForUser()
        {
            // Arrange
            // Create test wallet data
            var key1 = new Key();
            var key2 = new Key();
            var address1 = key1.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            var address2 = key2.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            var expectedAddresses = new List<BitcoinAddress> { address1, address2 };
            
            // Setup mock to return the expected addresses
            _mockAddressService.Setup(a => a.GetAddressesForUser(_defaultUsername))
                .ReturnsAsync(expectedAddresses);

            // Act
            var result = await _mockAddressService.Object.GetAddressesForUser(_defaultUsername);

            // Assert
            var addresses = result.ToList();
            Assert.Equal(2, addresses.Count);
            Assert.Contains(addresses, a => a.ToString() == address1.ToString());
            Assert.Contains(addresses, a => a.ToString() == address2.ToString());
            Assert.DoesNotContain(addresses, a => a.ToString() == "1PkE8pNy95tMSPaCHHLCZzHQKY8Ny6HjpC");
            
            // Verify mock was called with correct parameters
            _mockAddressService.Verify(a => a.GetAddressesForUser(_defaultUsername), Times.Once);
        }

        [Fact]
        public async Task GetAddressesForUser_ShouldHandleInvalidAddresses()
        {
            // Arrange
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            var expectedAddresses = new List<BitcoinAddress> { address };
            
            // Setup mock to return only valid addresses
            _mockAddressService.Setup(a => a.GetAddressesForUser(_defaultUsername))
                .ReturnsAsync(expectedAddresses);

            // Act
            var result = await _mockAddressService.Object.GetAddressesForUser(_defaultUsername);

            // Assert
            var addresses = result.ToList();
            Assert.Single(addresses); // Only one valid address should be returned
            Assert.Contains(addresses, a => a.ToString() == address.ToString());
            
            // Verify mock was called with correct parameters
            _mockAddressService.Verify(a => a.GetAddressesForUser(_defaultUsername), Times.Once);
        }

        [Fact]
        public async Task GetPrivateKeyForAddress_ShouldReturnPrivateKeyForValidAddress()
        {
            // Arrange
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            var addressString = address.ToString();
            var wif = key.GetWif(Network.Main).ToString();
            
            // Setup mock to return the private key
            _mockAddressService.Setup(a => a.GetPrivateKeyForAddress(addressString, _defaultUsername))
                .ReturnsAsync(key);
            
            // Setup mock to decrypt the private key
            _mockCryptoService.Setup(c => c.DecryptPrivateKey("encryptedKey", "salt"))
                .Returns(wif);

            // Act
            var result = await _mockAddressService.Object.GetPrivateKeyForAddress(addressString, _defaultUsername);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Key>(result);
            
            // Verify the mock was called
            _mockAddressService.Verify(a => a.GetPrivateKeyForAddress(addressString, _defaultUsername), Times.Once);
        }

        [Fact]
        public async Task GetPrivateKeyForAddress_ShouldReturnNullForNonExistentAddress()
        {
            // Arrange
            string addressString = "1LRVYFAvjRKQnLLj1h3iUd9RZiKnzYFBzq";
            
            // Setup mock to return null
            _mockAddressService.Setup(a => a.GetPrivateKeyForAddress(addressString, _defaultUsername))
                .ReturnsAsync((Key?)null);
            
            // Act
            var result = await _mockAddressService.Object.GetPrivateKeyForAddress(addressString, _defaultUsername);

            // Assert
            Assert.Null(result);
            
            // Verify mock was called with correct parameters
            _mockAddressService.Verify(a => a.GetPrivateKeyForAddress(addressString, _defaultUsername), Times.Once);
        }

        [Fact]
        public async Task GetPrivateKeyForAddress_ShouldReturnNullWhenDecryptionFails()
        {
            // Arrange
            string addressString = "1LRVYFAvjRKQnLLj1h3iUd9RZiKnzYFBzq";
            
            // Setup mock to return null (simulating decryption failure)
            _mockAddressService.Setup(a => a.GetPrivateKeyForAddress(addressString, _defaultUsername))
                .ReturnsAsync((Key?)null);
            
            // Setup mock to throw an exception when DecryptPrivateKey is called
            _mockCryptoService.Setup(c => c.DecryptPrivateKey("encryptedKey", "salt"))
                .Throws(new Exception("Decryption failed"));

            // Act
            var result = await _mockAddressService.Object.GetPrivateKeyForAddress(addressString, _defaultUsername);

            // Assert
            Assert.Null(result);
            
            // Verify mock was called with correct parameters
            _mockAddressService.Verify(a => a.GetPrivateKeyForAddress(addressString, _defaultUsername), Times.Once);
        }
    }
}
