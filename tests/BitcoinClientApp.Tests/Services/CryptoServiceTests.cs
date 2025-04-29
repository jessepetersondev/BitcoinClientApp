using BitcoinClientApp.Services;
using System.Security.Cryptography;

namespace BitcoinClientApp.Tests.Services
{
    public class CryptoServiceTests
    {
        private readonly Mock<ICryptoService> _mockCryptoService;

        public CryptoServiceTests()
        {
            _mockCryptoService = new Mock<ICryptoService>();
        }

        [Fact]
        public void GenerateSalt_ShouldReturnRandomSalt()
        {
            // Arrange
            string salt1 = "IXZIVYKFl7jYKMGgSALUmuU+9FvqcMGCyfnZbOFsVPo="; // Sample Base64 salt
            string salt2 = "xK6FhtY99A/9lCCM+TyuZLbG2s9XncGmMk4yJuu21Z8="; // Different sample Base64 salt
            
            _mockCryptoService.SetupSequence(c => c.GenerateSalt())
                .Returns(salt1)
                .Returns(salt2);

            // Act
            var result1 = _mockCryptoService.Object.GenerateSalt();
            var result2 = _mockCryptoService.Object.GenerateSalt();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotEqual(result1, result2); // Different salts should be generated each time
            
            // Salt should be a valid Base64 string
            Assert.True(IsValidBase64(result1), "Salt is not a valid Base64 string");
            
            // Generated salt should have sufficient length for security
            byte[] decodedSalt = Convert.FromBase64String(result1);
            Assert.True(decodedSalt.Length >= 16, "Salt is not long enough for security");
            
            // Verify method was called twice
            _mockCryptoService.Verify(c => c.GenerateSalt(), Times.Exactly(2));
        }

        [Fact]
        public void EncryptDecrypt_ShouldRoundTrip()
        {
            // Arrange
            string originalText = "ThisIsATestPrivateKey123!";
            string salt = "IXZIVYKFl7jYKMGgSALUmuU+9FvqcMGCyfnZbOFsVPo=";
            string encrypted = "EncryptedDataHere12345==";
            
            _mockCryptoService.Setup(c => c.GenerateSalt())
                .Returns(salt);
                
            _mockCryptoService.Setup(c => c.EncryptPrivateKey(originalText, salt))
                .Returns(encrypted);
                
            _mockCryptoService.Setup(c => c.DecryptPrivateKey(encrypted, salt))
                .Returns(originalText);

            // Act
            string saltResult = _mockCryptoService.Object.GenerateSalt();
            string encryptedResult = _mockCryptoService.Object.EncryptPrivateKey(originalText, saltResult);
            string decryptedResult = _mockCryptoService.Object.DecryptPrivateKey(encryptedResult, saltResult);

            // Assert
            Assert.NotEqual(originalText, encryptedResult); // Encrypted value should be different
            Assert.Equal(originalText, decryptedResult); // Decryption should restore original value
            
            // Verify methods were called with correct parameters
            _mockCryptoService.Verify(c => c.GenerateSalt(), Times.Once);
            _mockCryptoService.Verify(c => c.EncryptPrivateKey(originalText, salt), Times.Once);
            _mockCryptoService.Verify(c => c.DecryptPrivateKey(encrypted, salt), Times.Once);
        }

        [Fact]
        public void EncryptPrivateKey_ShouldProduceDifferentOutputsForSameInputWithDifferentSalts()
        {
            // Arrange
            string originalText = "MyPrivateKeyToEncrypt";
            string salt1 = "IXZIVYKFl7jYKMGgSALUmuU+9FvqcMGCyfnZbOFsVPo=";
            string salt2 = "xK6FhtY99A/9lCCM+TyuZLbG2s9XncGmMk4yJuu21Z8=";
            string encrypted1 = "EncryptedData1==";
            string encrypted2 = "EncryptedData2=="; // Different from encrypted1
            
            _mockCryptoService.Setup(c => c.GenerateSalt())
                .Returns(salt1)
                .Callback(() => _mockCryptoService.Setup(c => c.GenerateSalt()).Returns(salt2));
                
            _mockCryptoService.Setup(c => c.EncryptPrivateKey(originalText, salt1))
                .Returns(encrypted1);
                
            _mockCryptoService.Setup(c => c.EncryptPrivateKey(originalText, salt2))
                .Returns(encrypted2);

            // Act
            string saltResult1 = _mockCryptoService.Object.GenerateSalt();
            string encryptedResult1 = _mockCryptoService.Object.EncryptPrivateKey(originalText, saltResult1);
            
            string saltResult2 = _mockCryptoService.Object.GenerateSalt();
            string encryptedResult2 = _mockCryptoService.Object.EncryptPrivateKey(originalText, saltResult2);

            // Assert
            Assert.NotEqual(encryptedResult1, encryptedResult2);
            
            // Verify methods were called with correct parameters
            _mockCryptoService.Verify(c => c.GenerateSalt(), Times.Exactly(2));
            _mockCryptoService.Verify(c => c.EncryptPrivateKey(originalText, salt1), Times.Once);
            _mockCryptoService.Verify(c => c.EncryptPrivateKey(originalText, salt2), Times.Once);
        }

        [Fact]
        public void DecryptPrivateKey_ShouldThrowExceptionWithWrongSalt()
        {
            // Arrange
            string originalText = "SecretPrivateKey";
            string correctSalt = "IXZIVYKFl7jYKMGgSALUmuU+9FvqcMGCyfnZbOFsVPo=";
            string wrongSalt = "xK6FhtY99A/9lCCM+TyuZLbG2s9XncGmMk4yJuu21Z8=";
            string encrypted = "EncryptedSecretData==";
            
            _mockCryptoService.Setup(c => c.GenerateSalt())
                .Returns(correctSalt);
                
            _mockCryptoService.Setup(c => c.EncryptPrivateKey(originalText, correctSalt))
                .Returns(encrypted);
                
            _mockCryptoService.Setup(c => c.DecryptPrivateKey(encrypted, wrongSalt))
                .Throws(new CryptographicException("Invalid key or authentication tag."));

            // Act & Assert
            string saltResult = _mockCryptoService.Object.GenerateSalt();
            string encryptedResult = _mockCryptoService.Object.EncryptPrivateKey(originalText, saltResult);
            
            Assert.Throws<CryptographicException>(() => 
                _mockCryptoService.Object.DecryptPrivateKey(encryptedResult, wrongSalt));
            
            // Verify methods were called with correct parameters
            _mockCryptoService.Verify(c => c.GenerateSalt(), Times.Once);
            _mockCryptoService.Verify(c => c.EncryptPrivateKey(originalText, correctSalt), Times.Once);
            _mockCryptoService.Verify(c => c.DecryptPrivateKey(encrypted, wrongSalt), Times.Once);
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