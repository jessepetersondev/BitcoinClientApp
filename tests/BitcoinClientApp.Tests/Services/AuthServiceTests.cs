using System.Collections.Generic;
using System.Threading.Tasks;
using BitcoinClientApp.Data;
using BitcoinClientApp.Models;
using BitcoinClientApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;
using System;
using Moq;
using Moq.EntityFrameworkCore;
using System.Threading;

namespace BitcoinClientApp.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly string _testUsername = "testuser";
        private readonly string _testPassword = "P@ssw0rd!";

        public AuthServiceTests()
        {
            // Create in-memory database with a unique name
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            _context = new ApplicationDbContext(options);
            
            // Create the service with real context
            _authService = new AuthService(_context);
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
            
            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.GetUserByUsernameAsync(_testUsername);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testUsername, result.Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ShouldReturnNullWhenUserDoesNotExist()
        {
            // Arrange - Use a clean database context

            // Act
            var result = await _authService.GetUserByUsernameAsync(_testUsername);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUserWhenUsernameDoesNotExist()
        {
            // Arrange - Start with empty database

            // Act
            var result = await _authService.CreateUserAsync(_testUsername, _testPassword);

            // Assert
            Assert.True(result);
            
            // Verify a user was added
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == _testUsername);
            Assert.NotNull(user);
            Assert.NotNull(user.PasswordHash);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnFalseWhenUsernameExists()
        {
            // Arrange
            var testUser = new User 
            { 
                Id = 1, 
                Username = _testUsername, 
                PasswordHash = "hashedPassword" 
            };
            
            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.CreateUserAsync(_testUsername, _testPassword);

            // Assert
            Assert.False(result);
            
            // Verify no additional users were added
            var userCount = await _context.Users.CountAsync(u => u.Username == _testUsername);
            Assert.Equal(1, userCount);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnTrueWhenCredentialsAreValid()
        {
            // Arrange - We need to test with a real hash
            
            // First create a user with the real password hash
            await _authService.CreateUserAsync(_testUsername, _testPassword);

            // Act
            var result = await _authService.ValidateUserAsync(_testUsername, _testPassword);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnFalseWhenUserDoesNotExist()
        {
            // Arrange - Empty database 

            // Act
            var result = await _authService.ValidateUserAsync(_testUsername, _testPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnFalseWhenPasswordIsInvalid()
        {
            // Arrange
            // First create a user with the correct password
            await _authService.CreateUserAsync(_testUsername, _testPassword);

            // Act - Try to validate with wrong password
            string wrongPassword = "WrongPassword123!";
            var result = await _authService.ValidateUserAsync(_testUsername, wrongPassword);

            // Assert
            Assert.False(result);
        }
    }
} 