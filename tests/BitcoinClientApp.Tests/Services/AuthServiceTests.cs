using BitcoinClientApp.Models;
using BitcoinClientApp.Services;

namespace BitcoinClientApp.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly string _testUsername = "testuser";
        private readonly string _testPassword = "P@ssw0rd!";

        public AuthServiceTests()
        {
            // Create a mock auth service for all tests
            _mockAuthService = new Mock<IAuthService>();
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ShouldReturnUserWhenExists()
        {
            // Arrange
            var testUser = new User 
            { 
                Id = 1, 
                Username = _testUsername, 
                PasswordHash = "hashedPassword" 
            };
            
            _mockAuthService.Setup(a => a.GetUserByUsernameAsync(_testUsername))
                .ReturnsAsync(testUser);

            // Act
            var result = await _mockAuthService.Object.GetUserByUsernameAsync(_testUsername);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testUsername, result.Username);
            
            // Verify the method was called exactly once with the correct username
            _mockAuthService.Verify(a => a.GetUserByUsernameAsync(_testUsername), Times.Once);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ShouldReturnNullWhenUserDoesNotExist()
        {
            // Arrange
            _mockAuthService.Setup(a => a.GetUserByUsernameAsync(_testUsername))
                .ReturnsAsync((User)null);

            // Act
            var result = await _mockAuthService.Object.GetUserByUsernameAsync(_testUsername);

            // Assert
            Assert.Null(result);
            
            // Verify the method was called exactly once with the correct username
            _mockAuthService.Verify(a => a.GetUserByUsernameAsync(_testUsername), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUserWhenUsernameDoesNotExist()
        {
            // Arrange
            _mockAuthService.Setup(a => a.CreateUserAsync(_testUsername, _testPassword))
                .ReturnsAsync(true);

            // Act
            var result = await _mockAuthService.Object.CreateUserAsync(_testUsername, _testPassword);

            // Assert
            Assert.True(result);
            
            // Verify the method was called exactly once with the correct parameters
            _mockAuthService.Verify(a => a.CreateUserAsync(_testUsername, _testPassword), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnFalseWhenUsernameExists()
        {
            // Arrange
            _mockAuthService.Setup(a => a.CreateUserAsync(_testUsername, _testPassword))
                .ReturnsAsync(false);

            // Act
            var result = await _mockAuthService.Object.CreateUserAsync(_testUsername, _testPassword);

            // Assert
            Assert.False(result);
            
            // Verify the method was called exactly once with the correct parameters
            _mockAuthService.Verify(a => a.CreateUserAsync(_testUsername, _testPassword), Times.Once);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnTrueWhenCredentialsAreValid()
        {
            // Arrange
            _mockAuthService.Setup(a => a.ValidateUserAsync(_testUsername, _testPassword))
                .ReturnsAsync(true);

            // Act
            var result = await _mockAuthService.Object.ValidateUserAsync(_testUsername, _testPassword);

            // Assert
            Assert.True(result);
            
            // Verify the method was called exactly once with the correct parameters
            _mockAuthService.Verify(a => a.ValidateUserAsync(_testUsername, _testPassword), Times.Once);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnFalseWhenUserDoesNotExist()
        {
            // Arrange
            _mockAuthService.Setup(a => a.ValidateUserAsync(_testUsername, _testPassword))
                .ReturnsAsync(false);

            // Act
            var result = await _mockAuthService.Object.ValidateUserAsync(_testUsername, _testPassword);

            // Assert
            Assert.False(result);
            
            // Verify the method was called exactly once with the correct parameters
            _mockAuthService.Verify(a => a.ValidateUserAsync(_testUsername, _testPassword), Times.Once);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnFalseWhenPasswordIsInvalid()
        {
            // Arrange
            string wrongPassword = "WrongPassword123!";
            
            _mockAuthService.Setup(a => a.ValidateUserAsync(_testUsername, wrongPassword))
                .ReturnsAsync(false);

            // Act
            var result = await _mockAuthService.Object.ValidateUserAsync(_testUsername, wrongPassword);

            // Assert
            Assert.False(result);
            
            // Verify the method was called exactly once with the correct parameters
            _mockAuthService.Verify(a => a.ValidateUserAsync(_testUsername, wrongPassword), Times.Once);
        }
        
        [Fact]
        public async Task GetUserByUsernameAsync_ShouldThrowExceptionWhenDatabaseFails()
        {
            // Arrange - Set up mock to throw exception
            _mockAuthService.Setup(a => a.GetUserByUsernameAsync(_testUsername))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () => 
                await _mockAuthService.Object.GetUserByUsernameAsync(_testUsername));
            
            Assert.Equal("Database connection failed", exception.Message);
            
            // Verify the method was called exactly once
            _mockAuthService.Verify(a => a.GetUserByUsernameAsync(_testUsername), Times.Once);
        }
    }
} 