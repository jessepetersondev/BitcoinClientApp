using BitcoinClientApp.Data;
using BitcoinClientApp.Models;
using BitcoinClientApp.Services;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace BitcoinClientApp.Tests.Services
{
    public class AddressServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ICryptoService> _mockCryptoService;
        private readonly AddressService _addressService;
        private readonly string _defaultUsername = "testuser";

        public AddressServiceTests()
        {
            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            
            // Setup mock crypto service
            _mockCryptoService = new Mock<ICryptoService>();
            
            // Create address service with real context and mocked crypto service
            _addressService = new AddressService(_context, _mockCryptoService.Object);
        }

        [Fact]
        public async Task CreateNewWallet_ShouldCreateWalletAndReturnKeyAndAddress()
        {
            // Arrange
            var expectedSalt = "mockedSalt";
            var expectedEncryptedKey = "encryptedKeyValue";
            
            _mockCryptoService.Setup(c => c.GenerateSalt())
                .Returns(expectedSalt);
            
            _mockCryptoService.Setup(c => c.EncryptPrivateKey(It.IsAny<string>(), expectedSalt))
                .Returns(expectedEncryptedKey);

            // Act
            var result = await _addressService.CreateNewWallet(_defaultUsername);

            // Assert
            Assert.NotNull(result.privateKey);
            Assert.NotNull(result.address);
            Assert.IsType<Key>(result.privateKey);
            Assert.IsAssignableFrom<BitcoinAddress>(result.address);
            
            // Verify a wallet was added to the context
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Username == _defaultUsername);
            Assert.NotNull(wallet);
            Assert.Equal(expectedEncryptedKey, wallet.EncryptedPrivateKey);
            Assert.Equal(expectedSalt, wallet.Salt);
            
            // Verify the mock was called
            _mockCryptoService.Verify(c => c.GenerateSalt(), Times.Once);
            _mockCryptoService.Verify(c => c.EncryptPrivateKey(It.IsAny<string>(), expectedSalt), Times.Once);
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
            
            var testWallets = new List<Wallet>
            {
                new Wallet
                {
                    Username = _defaultUsername,
                    EncryptedPrivateKey = "encrypted1",
                    Salt = "salt1",
                    PublicKey = key1.PubKey.ToString(),
                    Address = address1.ToString(),
                    CreatedAt = DateTime.UtcNow
                },
                new Wallet
                {
                    Username = _defaultUsername,
                    EncryptedPrivateKey = "encrypted2",
                    Salt = "salt2",
                    PublicKey = key2.PubKey.ToString(),
                    Address = address2.ToString(),
                    CreatedAt = DateTime.UtcNow
                },
                new Wallet
                {
                    Username = "otheruser",
                    EncryptedPrivateKey = "encrypted3",
                    Salt = "salt3",
                    PublicKey = "02a7451395735369f2ecdfc829c0f774e88ef1303dfe5b2f04dbaab30a535dfdd3",
                    Address = "1PkE8pNy95tMSPaCHHLCZzHQKY8Ny6HjpC",
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Add test data to the in-memory database
            _context.Wallets.AddRange(testWallets);
            await _context.SaveChangesAsync();

            // Act
            var result = await _addressService.GetAddressesForUser(_defaultUsername);

            // Assert
            var addresses = result.ToList();
            Assert.Equal(2, addresses.Count);
            Assert.Contains(addresses, a => a.ToString() == address1.ToString());
            Assert.Contains(addresses, a => a.ToString() == address2.ToString());
            Assert.DoesNotContain(addresses, a => a.ToString() == "1PkE8pNy95tMSPaCHHLCZzHQKY8Ny6HjpC");
        }

        [Fact]
        public async Task GetAddressesForUser_ShouldHandleInvalidAddresses()
        {
            // Arrange
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            var testWallets = new List<Wallet>
            {
                new Wallet
                {
                    Username = _defaultUsername,
                    EncryptedPrivateKey = "encrypted1",
                    Salt = "salt1",
                    PublicKey = key.PubKey.ToString(),
                    Address = address.ToString(), // Valid address
                    CreatedAt = DateTime.UtcNow
                },
                new Wallet
                {
                    Username = _defaultUsername,
                    EncryptedPrivateKey = "encrypted2",
                    Salt = "salt2",
                    PublicKey = "invalid-public-key",
                    Address = "invalid-address", // Invalid address
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Add test data to the in-memory database
            _context.Wallets.AddRange(testWallets);
            await _context.SaveChangesAsync();

            // Act
            var result = await _addressService.GetAddressesForUser(_defaultUsername);

            // Assert
            var addresses = result.ToList();
            Assert.Single(addresses); // Only one valid address should be returned
            Assert.Contains(addresses, a => a.ToString() == address.ToString());
        }

        [Fact]
        public async Task GetPrivateKeyForAddress_ShouldReturnPrivateKeyForValidAddress()
        {
            // Arrange
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            var addressString = address.ToString();
            var wif = key.GetWif(Network.Main).ToString();
            
            var wallet = new Wallet
            {
                Username = _defaultUsername,
                EncryptedPrivateKey = "encryptedKey",
                Salt = "salt",
                PublicKey = key.PubKey.ToString(),
                Address = addressString,
                CreatedAt = DateTime.UtcNow
            };

            // Add test wallet to the database
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            
            // Setup mock to decrypt the private key
            _mockCryptoService.Setup(c => c.DecryptPrivateKey("encryptedKey", "salt"))
                .Returns(wif);

            // Act
            var result = await _addressService.GetPrivateKeyForAddress(addressString, _defaultUsername);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Key>(result);
            
            // Verify the decrypt method was called
            _mockCryptoService.Verify(c => c.DecryptPrivateKey("encryptedKey", "salt"), Times.Once);
        }

        [Fact]
        public async Task GetPrivateKeyForAddress_ShouldReturnNullForNonExistentAddress()
        {
            // Arrange
            string addressString = "1LRVYFAvjRKQnLLj1h3iUd9RZiKnzYFBzq";
            
            // Database is empty, so the address should not exist
            
            // Act
            var result = await _addressService.GetPrivateKeyForAddress(addressString, _defaultUsername);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPrivateKeyForAddress_ShouldReturnNullWhenDecryptionFails()
        {
            // Arrange
            string addressString = "1LRVYFAvjRKQnLLj1h3iUd9RZiKnzYFBzq";
            
            var wallet = new Wallet
            {
                Username = _defaultUsername,
                EncryptedPrivateKey = "encryptedKey",
                Salt = "salt",
                PublicKey = "02c3b0ae8cc1ee271736869b98c70d78aa8bcedca62f048fec142a1377ca9951b8",
                Address = addressString,
                CreatedAt = DateTime.UtcNow
            };

            // Add wallet to the database
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            
            // Setup mock to throw an exception when DecryptPrivateKey is called
            _mockCryptoService.Setup(c => c.DecryptPrivateKey("encryptedKey", "salt"))
                .Throws(new Exception("Decryption failed"));

            // Act
            var result = await _addressService.GetPrivateKeyForAddress(addressString, _defaultUsername);

            // Assert
            Assert.Null(result);
        }
    }
}
