using System;
using BitcoinClientApp.Models;
using Xunit;

namespace BitcoinClientApp.Tests.Models
{
    public class WalletTests
    {
        [Fact]
        public void Wallet_PropertiesShouldBeSetCorrectly()
        {
            // Arrange
            var id = 1;
            var username = "testuser";
            var encryptedPrivateKey = "encryptedKey";
            var salt = "salt";
            var publicKey = "02c3b0ae8cc1ee271736869b98c70d78aa8bcedca62f048fec142a1377ca9951b8";
            var address = "1MRCQDprprVzd2Lc4UJmeMrBassetX1vDw";
            var createdAt = DateTime.UtcNow;

            // Act
            var wallet = new Wallet
            {
                Id = id,
                Username = username,
                EncryptedPrivateKey = encryptedPrivateKey,
                Salt = salt,
                PublicKey = publicKey,
                Address = address,
                CreatedAt = createdAt
            };

            // Assert
            Assert.Equal(id, wallet.Id);
            Assert.Equal(username, wallet.Username);
            Assert.Equal(encryptedPrivateKey, wallet.EncryptedPrivateKey);
            Assert.Equal(salt, wallet.Salt);
            Assert.Equal(publicKey, wallet.PublicKey);
            Assert.Equal(address, wallet.Address);
            Assert.Equal(createdAt, wallet.CreatedAt);
        }

        [Fact]
        public void Wallet_DefaultCreatedAtShouldBeUtcNow()
        {
            // Arrange & Act
            var before = DateTime.UtcNow.AddSeconds(-1);
            var wallet = new Wallet();
            var after = DateTime.UtcNow.AddSeconds(1);

            // Assert - by default CreatedAt should be set to a reasonable time near UtcNow
            Assert.True(wallet.CreatedAt >= before && wallet.CreatedAt <= after);
        }

        [Fact]
        public void Wallet_NewInstanceShouldHaveEmptyStrings()
        {
            // Arrange & Act
            var wallet = new Wallet();

            // Assert
            Assert.NotNull(wallet.Username);
            Assert.NotNull(wallet.EncryptedPrivateKey);
            Assert.NotNull(wallet.Salt);
            Assert.NotNull(wallet.PublicKey);
            Assert.NotNull(wallet.Address);
            
            Assert.Equal(string.Empty, wallet.Username);
            Assert.Equal(string.Empty, wallet.EncryptedPrivateKey);
            Assert.Equal(string.Empty, wallet.Salt);
            Assert.Equal(string.Empty, wallet.PublicKey);
            Assert.Equal(string.Empty, wallet.Address);
        }
    }
} 