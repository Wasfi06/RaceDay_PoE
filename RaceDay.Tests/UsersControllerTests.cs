using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Controllers;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Models;
using Moq;
using RaceDay.Services;

namespace RaceDay.Tests
{
    public class UsersControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private UsersController CreateController(
            RaceDayDbContext context,
            int userId,
            string role)
        {
            var blobStorageMock = new Mock<IBlobStorageService>();

            var controller = new UsersController(
                context,
                blobStorageMock.Object);

            var httpContext = new DefaultHttpContext();

            httpContext.Items["UserId"] = userId;
            httpContext.Items["Role"] = role;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        private async Task SeedUser(RaceDayDbContext context)
        {
            var user = new User
            {
                UserId = 1,
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@test.com",
                PasswordHash = "hashed-password",
                Role = "Participant",
                Phone = "0123456789",
                ProfileImageUrl = null,
                IsActive = true
            };

            context.Users.Add(user);

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetMyProfile_ReturnsCurrentUserProfile()
        {
            using var context = CreateContext();

            await SeedUser(context);

            var controller = CreateController(
                context,
                userId: 1,
                role: "Participant");

            var result = await controller.GetMyProfile();

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateMyProfile_UpdatesCurrentUser()
        {
            using var context = CreateContext();

            await SeedUser(context);

            var controller = CreateController(
                context,
                userId: 1,
                role: "Participant");

            var request = new UpdateProfileRequest
            {
                FirstName = "Updated",
                LastName = "Participant",
                Phone = "0821234567",
                ProfileImageUrl = "https://example.com/profile.jpg"
            };

            var result = await controller.UpdateMyProfile(request);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(200, okResult.StatusCode);

            var updatedUser = await context.Users
                .FirstAsync(u => u.UserId == 1);

            Assert.Equal("Updated", updatedUser.FirstName);
            Assert.Equal("Participant", updatedUser.LastName);
            Assert.Equal("0821234567", updatedUser.Phone);
            Assert.Equal(
                "https://example.com/profile.jpg",
                updatedUser.ProfileImageUrl);
        }

        [Fact]
        public async Task GetMyProfile_WhenUserDoesNotExist_ReturnsNotFound()
        {
            using var context = CreateContext();

            var controller = CreateController(
                context,
                userId: 999,
                role: "Participant");

            var result = await controller.GetMyProfile();

            var notFoundResult =
                Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}