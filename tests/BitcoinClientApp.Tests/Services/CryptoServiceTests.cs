using BitcoinClientApp.Services;

namespace BitcoinClientApp.Tests.Services
{
    public class CryptoServiceTests
    {
        private readonly CryptoService _cryptoService;

        public CryptoServiceTests()
        {
            _cryptoService = new CryptoService();
        }

        [Fact]
        public void GenerateSalt_ShouldReturnRandomSalt()
        {
            // Act
            var salt1 = _cryptoService.GenerateSalt();
            var salt2 = _cryptoService.GenerateSalt();

            // Assert
            Assert.NotNull(salt1);
            Assert.NotNull(salt2);
            Assert.NotEqual(salt1, salt2); // Different salts should be generated each time
            
            // Salt should be a valid Base64 string
            Assert.True(IsValidBase64(salt1), "Salt is not a valid Base64 string");
            
            // Generated salt should have sufficient length for security
            byte[] decodedSalt = Convert.FromBase64String(salt1);
            Assert.True(decodedSalt.Length >= 16, "Salt is not long enough for security");
        }

        [Fact]
        public void EncryptDecrypt_ShouldRoundTrip()
        {
            // Arrange
            string originalText = "ThisIsATestPrivateKey123!";
            string salt = _cryptoService.GenerateSalt();

            // Act
            string encrypted = _cryptoService.EncryptPrivateKey(originalText, salt);
            string decrypted = _cryptoService.DecryptPrivateKey(encrypted, salt);

            // Assert
            Assert.NotEqual(originalText, encrypted); // Encrypted value should be different
            Assert.Equal(originalText, decrypted); // Decryption should restore original value
        }

        [Fact]
        public void EncryptPrivateKey_ShouldProduceDifferentOutputsForSameInputWithDifferentSalts()
        {
            // Arrange
            string originalText = "MyPrivateKeyToEncrypt";
            string salt1 = _cryptoService.GenerateSalt();
            string salt2 = _cryptoService.GenerateSalt();

            // Act
            string encrypted1 = _cryptoService.EncryptPrivateKey(originalText, salt1);
            string encrypted2 = _cryptoService.EncryptPrivateKey(originalText, salt2);

            // Assert
            Assert.NotEqual(encrypted1, encrypted2);
        }

        [Fact]
        public void DecryptPrivateKey_ShouldThrowExceptionWithWrongSalt()
        {
            // Arrange
            string originalText = "SecretPrivateKey";
            string correctSalt = _cryptoService.GenerateSalt();
            string wrongSalt = _cryptoService.GenerateSalt();
            string encrypted = _cryptoService.EncryptPrivateKey(originalText, correctSalt);

            // Act & Assert
            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => 
                _cryptoService.DecryptPrivateKey(encrypted, wrongSalt));
        }

        private bool IsValidBase64(string base64String)
        {
            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 