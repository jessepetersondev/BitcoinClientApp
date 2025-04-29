using BitcoinClientApp.Models;

namespace BitcoinClientApp.Tests.Models
{
    public class UserTests
    {
        [Fact]
        public void User_PropertiesShouldBeSetCorrectly()
        {
            // Arrange
            var id = 1;
            var username = "testuser";
            var passwordHash = "hashedPassword";
            var displayName = "Test User";
            var createdAt = DateTime.UtcNow;

            // Act
            var user = new User
            {
                Id = id,
                Username = username,
                PasswordHash = passwordHash,
                DisplayName = displayName,
                CreatedAt = createdAt
            };

            // Assert
            Assert.Equal(id, user.Id);
            Assert.Equal(username, user.Username);
            Assert.Equal(passwordHash, user.PasswordHash);
            Assert.Equal(displayName, user.DisplayName);
            Assert.Equal(createdAt, user.CreatedAt);
        }

        [Fact]
        public void User_DefaultCreatedAtShouldBeUtcNow()
        {
            // Arrange & Act
            var before = DateTime.UtcNow.AddSeconds(-1);
            var user = new User();
            var after = DateTime.UtcNow.AddSeconds(1);

            // Assert - by default CreatedAt should be set to a reasonable time near UtcNow
            Assert.True(user.CreatedAt >= before && user.CreatedAt <= after);
        }

        [Fact]
        public void User_NewInstanceShouldHaveNullDisplayName()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.Null(user.DisplayName);
        }

        [Fact]
        public void User_NewInstanceShouldHaveEmptyStrings()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.NotNull(user.Username);
            Assert.NotNull(user.PasswordHash);
            
            Assert.Equal(string.Empty, user.Username);
            Assert.Equal(string.Empty, user.PasswordHash);
        }
    }
} 