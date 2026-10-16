using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Services;

namespace RaceDay.Tests
{
    public class AuthServiceTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_WithValidDetails_CreatesUser()
        {
            using var context = CreateContext();
            var service = new AuthService(context);

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "newuser@test.com",
                Password = "TestPassword123!",
                Role = "Participant",
                Phone = "0123456789"
            };

            var result = await service.RegisterAsync(request);

            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal("newuser@test.com", result.User.Email);
            Assert.Equal("Participant", result.User.Role);

            // Password must not be stored as plain text
            Assert.NotEqual("TestPassword123!", result.User.PasswordHash);
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidRole_ReturnsFailure()
        {
            using var context = CreateContext();
            var service = new AuthService(context);

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "invalidrole@test.com",
                Password = "TestPassword123!",
                Role = "Admin"
            };

            var result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Null(result.User);

            Assert.Equal(
                "Role must be Organizer or Participant.",
                result.Message);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ReturnsFailure()
        {
            using var context = CreateContext();
            var service = new AuthService(context);

            var firstRequest = new RegisterRequest
            {
                FirstName = "First",
                LastName = "User",
                Email = "duplicate@test.com",
                Password = "TestPassword123!",
                Role = "Participant"
            };

            var secondRequest = new RegisterRequest
            {
                FirstName = "Second",
                LastName = "User",
                Email = "duplicate@test.com",
                Password = "AnotherPassword123!",
                Role = "Organizer"
            };

            var firstResult = await service.RegisterAsync(firstRequest);
            var secondResult = await service.RegisterAsync(secondRequest);

            Assert.True(firstResult.Success);
            Assert.False(secondResult.Success);
            Assert.Null(secondResult.User);

            Assert.Equal(
                "An account with this email already exists.",
                secondResult.Message);
        }
        [Fact]
        public async Task LoginAsync_WithCorrectPassword_ReturnsSuccess()
        {
            using var context = CreateContext();
            var service = new AuthService(context);

            var registerRequest = new RegisterRequest
            {
                FirstName = "Login",
                LastName = "Test",
                Email = "login@test.com",
                Password = "CorrectPassword123!",
                Role = "Participant"
            };

            await service.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = "login@test.com",
                Password = "CorrectPassword123!"
            };

            var result = await service.LoginAsync(loginRequest);

            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal("login@test.com", result.User.Email);
            Assert.Equal("Participant", result.User.Role);
        }
        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
        {
            using var context = CreateContext();
            var service = new AuthService(context);

            var registerRequest = new RegisterRequest
            {
                FirstName = "Login",
                LastName = "Test",
                Email = "wrongpassword@test.com",
                Password = "CorrectPassword123!",
                Role = "Participant"
            };

            await service.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = "wrongpassword@test.com",
                Password = "WrongPassword123!"
            };

            var result = await service.LoginAsync(loginRequest);

            Assert.False(result.Success);
            Assert.Null(result.User);
            Assert.Equal("Invalid email or password.", result.Message);
        }
        [Fact]
        public async Task LoginAsync_WithInactiveAccount_ReturnsFailure()
        {
            using var context = CreateContext();
            var service = new AuthService(context);

            var registerRequest = new RegisterRequest
            {
                FirstName = "Inactive",
                LastName = "User",
                Email = "inactive@test.com",
                Password = "CorrectPassword123!",
                Role = "Participant"
            };

            var registerResult = await service.RegisterAsync(registerRequest);

            registerResult.User!.IsActive = false;
            await context.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Email = "inactive@test.com",
                Password = "CorrectPassword123!"
            };

            var result = await service.LoginAsync(loginRequest);

            Assert.False(result.Success);
            Assert.Null(result.User);
            Assert.Equal("This account is inactive.", result.Message);
        }
    }
}
