using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Controllers;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Models;

namespace RaceDay.Tests
{
    public class ResultsControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private ResultsController CreateController(
            RaceDayDbContext context,
            int userId,
            string role)
        {
            var controller = new ResultsController(context);

            var httpContext = new DefaultHttpContext();

            httpContext.Items["UserId"] = userId;
            httpContext.Items["Role"] = role;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        private async Task SeedTestData(RaceDayDbContext context)
        {
            var organizer = new User
            {
                UserId = 1,
                FirstName = "Test",
                LastName = "Organizer",
                Email = "organizer@test.com",
                PasswordHash = "hashed",
                Role = "Organizer",
                IsActive = true
            };

            var participant = new User
            {
                UserId = 2,
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@test.com",
                PasswordHash = "hashed",
                Role = "Participant",
                IsActive = true
            };

            var eventItem = new Event
            {
                EventId = 1,
                OrganizerId = 1,
                Name = "Test Race",
                Description = "Test event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town",
                DistanceKm = 10,
                EventType = "Running"
            };

            var category = new Category
            {
                CategoryId = 1,
                EventId = 1,
                Name = "10 km Open",
                CategoryType = "Distance",
                MinDistanceKm = 10,
                MaxDistanceKm = 10
            };

            var enrollment = new Enrollment
            {
                EnrollmentId = 1,
                EventId = 1,
                ParticipantId = 2,
                CategoryId = 1,
                Status = "Confirmed"
            };

            context.Users.AddRange(organizer, participant);
            context.Events.Add(eventItem);
            context.Categories.Add(category);
            context.Enrollments.Add(enrollment);

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateResult_AsEventOrganizer_CreatesResult()
        {
            using var context = CreateContext();

            await SeedTestData(context);

            var controller = CreateController(
                context,
                userId: 1,
                role: "Organizer");

            var request = new CreateResultRequest
            {
                EnrollmentId = 1,
                FinishTime = new TimeSpan(0, 52, 30),
                FinishPosition = 12,
                IsPublished = true
            };

            var result = await controller.CreateResult(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(201, createdResult.StatusCode);

            var savedResult = await context.Results
                .FirstOrDefaultAsync();

            Assert.NotNull(savedResult);
            Assert.Equal(1, savedResult.EnrollmentId);
            Assert.Equal(new TimeSpan(0, 52, 30), savedResult.FinishTime);
            Assert.Equal(12, savedResult.FinishPosition);
            Assert.True(savedResult.IsPublished);
            Assert.NotNull(savedResult.PublishedAt);
        }

        [Fact]
        public async Task CreateResult_WithInvalidPosition_ReturnsBadRequest()
        {
            using var context = CreateContext();

            await SeedTestData(context);

            var controller = CreateController(
                context,
                userId: 1,
                role: "Organizer");

            var request = new CreateResultRequest
            {
                EnrollmentId = 1,
                FinishTime = new TimeSpan(0, 52, 30),
                FinishPosition = 0,
                IsPublished = true
            };

            var result = await controller.CreateResult(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task GetMyResults_AsParticipant_ReturnsOwnResult()
        {
            using var context = CreateContext();

            await SeedTestData(context);

            context.Results.Add(new Result
            {
                ResultId = 1,
                EnrollmentId = 1,
                FinishTime = new TimeSpan(0, 52, 30),
                FinishPosition = 12,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var controller = CreateController(
                context,
                userId: 2,
                role: "Participant");

            var result = await controller.GetMyResults();

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetResult_AsDifferentParticipant_ReturnsNotFound()
        {
            using var context = CreateContext();

            await SeedTestData(context);

            var secondParticipant = new User
            {
                UserId = 3,
                FirstName = "Second",
                LastName = "Participant",
                Email = "participant2@test.com",
                PasswordHash = "hashed",
                Role = "Participant",
                IsActive = true
            };

            context.Users.Add(secondParticipant);

            context.Results.Add(new Result
            {
                ResultId = 1,
                EnrollmentId = 1,
                FinishTime = new TimeSpan(0, 52, 30),
                FinishPosition = 12,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var controller = CreateController(
                context,
                userId: 3,
                role: "Participant");

            var result = await controller.GetResult(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}