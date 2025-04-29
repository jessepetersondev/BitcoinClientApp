using System;
using BitcoinClientApp.Data;
using BitcoinClientApp.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BitcoinClientApp.Tests.Data
{
    public class ApplicationDbContextTests
    {
        [Fact]
        public void ApplicationDbContext_ShouldHaveUsersDbSet()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var users = context.Users;

            // Assert
            Assert.NotNull(users);
            Assert.IsAssignableFrom<DbSet<User>>(users);
        }

        [Fact]
        public void ApplicationDbContext_ShouldHaveWalletsDbSet()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var wallets = context.Wallets;

            // Assert
            Assert.NotNull(wallets);
            Assert.IsAssignableFrom<DbSet<Wallet>>(wallets);
        }

        [Fact]
        public void ApplicationDbContext_CanAddAndRetrieveUser()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            var username = "testuser";
            var passwordHash = "hash123";

            // Act - Add a user
            using (var context = new ApplicationDbContext(options))
            {
                var user = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    DisplayName = "Test User",
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(user);
                context.SaveChanges();
            }

            // Act - Retrieve the user
            using (var context = new ApplicationDbContext(options))
            {
                var user = context.Users.FirstOrDefaultAsync(u => u.Username == username).Result;

                // Assert
                Assert.NotNull(user);
                Assert.Equal(username, user.Username);
                Assert.Equal(passwordHash, user.PasswordHash);
            }
        }

        [Fact]
        public void ApplicationDbContext_CanAddAndRetrieveWallet()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            var username = "walletowner";
            var address = "1MRCQDprprVzd2Lc4UJmeMrBassetX1vDw";

            // Act - Add a wallet
            using (var context = new ApplicationDbContext(options))
            {
                var wallet = new Wallet
                {
                    Username = username,
                    EncryptedPrivateKey = "encryptedKey",
                    Salt = "salt",
                    PublicKey = "02c3b0ae8cc1ee271736869b98c70d78aa8bcedca62f048fec142a1377ca9951b8",
                    Address = address,
                    CreatedAt = DateTime.UtcNow
                };

                context.Wallets.Add(wallet);
                context.SaveChanges();
            }

            // Act - Retrieve the wallet
            using (var context = new ApplicationDbContext(options))
            {
                var wallet = context.Wallets.FirstOrDefaultAsync(w => w.Address == address).Result;

                // Assert
                Assert.NotNull(wallet);
                Assert.Equal(username, wallet.Username);
                Assert.Equal(address, wallet.Address);
            }
        }
    }
} 