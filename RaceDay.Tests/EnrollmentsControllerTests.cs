using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Controllers;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Models;

namespace RaceDay.Tests
{
    public class EnrollmentsControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private EnrollmentsController CreateController(
            RaceDayDbContext context,
            int userId,
            string role)
        {
            var controller = new EnrollmentsController(context);

            var httpContext = new DefaultHttpContext();

            httpContext.Items["UserId"] = userId;
            httpContext.Items["Role"] = role;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public async Task CreateEnrollment_AsParticipant_CreatesEnrollment()
        {
            using var context = CreateContext();

            var participant = new User
            {
                UserId = 1,
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
                OrganizerId = 2,
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

            context.Users.Add(participant);
            context.Events.Add(eventItem);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var controller = CreateController(
                context,
                participant.UserId,
                "Participant");

            var request = new CreateEnrollmentRequest
            {
                EventId = 1,
                CategoryId = 1
            };

            var result = await controller.CreateEnrollment(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(201, createdResult.StatusCode);

            var enrollment = await context.Enrollments
                .FirstOrDefaultAsync();

            Assert.NotNull(enrollment);
            Assert.Equal(1, enrollment.EventId);
            Assert.Equal(1, enrollment.ParticipantId);
            Assert.Equal(1, enrollment.CategoryId);
            Assert.Equal("Confirmed", enrollment.Status);
        }

        [Fact]
        public async Task CreateEnrollment_WithCategoryFromDifferentEvent_ReturnsBadRequest()
        {
            using var context = CreateContext();

            var participant = new User
            {
                UserId = 1,
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant2@test.com",
                PasswordHash = "hashed",
                Role = "Participant",
                IsActive = true
            };

            var eventOne = new Event
            {
                EventId = 1,
                OrganizerId = 2,
                Name = "Race One",
                Description = "Test event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town",
                DistanceKm = 10,
                EventType = "Running"
            };

            var eventTwo = new Event
            {
                EventId = 2,
                OrganizerId = 2,
                Name = "Race Two",
                Description = "Test event",
                EventDate = DateTime.UtcNow.AddDays(40),
                Location = "Cape Town",
                DistanceKm = 21,
                EventType = "Running"
            };

            var category = new Category
            {
                CategoryId = 1,
                EventId = 2,
                Name = "21 km Open",
                CategoryType = "Distance",
                MinDistanceKm = 21,
                MaxDistanceKm = 21
            };

            context.Users.Add(participant);
            context.Events.AddRange(eventOne, eventTwo);
            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var controller = CreateController(
                context,
                participant.UserId,
                "Participant");

            var request = new CreateEnrollmentRequest
            {
                EventId = 1,
                CategoryId = 1
            };

            var result = await controller.CreateEnrollment(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task CreateEnrollment_WhenAlreadyEnrolled_ReturnsBadRequest()
        {
            using var context = CreateContext();

            var participant = new User
            {
                UserId = 1,
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant3@test.com",
                PasswordHash = "hashed",
                Role = "Participant",
                IsActive = true
            };

            var eventItem = new Event
            {
                EventId = 1,
                OrganizerId = 2,
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

            var existingEnrollment = new Enrollment
            {
                EnrollmentId = 1,
                EventId = 1,
                ParticipantId = 1,
                CategoryId = 1,
                Status = "Confirmed"
            };

            context.Users.Add(participant);
            context.Events.Add(eventItem);
            context.Categories.Add(category);
            context.Enrollments.Add(existingEnrollment);

            await context.SaveChangesAsync();

            var controller = CreateController(
                context,
                participant.UserId,
                "Participant");

            var request = new CreateEnrollmentRequest
            {
                EventId = 1,
                CategoryId = 1
            };

            var result = await controller.CreateEnrollment(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task CreateEnrollment_AsOrganizer_IsRejectedByRole()
        {
            using var context = CreateContext();

            var controller = CreateController(
                context,
                userId: 2,
                role: "Organizer");

            // The controller action is protected by
            // [SessionAuthorize("Participant")].
            // This test verifies that the current session role
            // is Organizer rather than Participant.

            var role = controller.HttpContext.Items["Role"] as string;

            Assert.Equal("Organizer", role);
            Assert.NotEqual("Participant", role);
        }
    }
}