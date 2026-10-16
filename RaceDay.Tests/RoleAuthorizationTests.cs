using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Controllers;
using RaceDay.Data;
using RaceDay.Middleware;

namespace RaceDay.Tests
{
    public class RoleAuthorizationTests
    {
        [Fact]
        public void ParticipantRole_IsNotOrganizer()
        {
            var context = new DefaultHttpContext();

            context.Items["UserId"] = 1;
            context.Items["Role"] = "Participant";

            var role = context.Items["Role"] as string;

            Assert.Equal("Participant", role);
            Assert.NotEqual("Organizer", role);
        }

        [Fact]
        public void OrganizerRole_IsNotParticipant()
        {
            var context = new DefaultHttpContext();

            context.Items["UserId"] = 2;
            context.Items["Role"] = "Organizer";

            var role = context.Items["Role"] as string;

            Assert.Equal("Organizer", role);
            Assert.NotEqual("Participant", role);
        }

        [Fact]
        public async Task MissingSession_ReturnsUnauthorized()
        {
            var context = new DefaultHttpContext();

            var controller = new TestAuthController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            var attribute = new SessionAuthorizeAttribute();

            var actionContext = new ActionContext(
                context,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());

            var executingContext = new ActionExecutingContext(
                actionContext,
                new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller);

            var executed = false;

            await attribute.OnActionExecutionAsync(
                executingContext,
                () =>
                {
                    executed = true;

                    return Task.FromResult<ActionExecutedContext>(
                        new ActionExecutedContext(
                            actionContext,
                            new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>(),
                            controller));
                });

            Assert.False(executed);
            Assert.IsType<UnauthorizedObjectResult>(executingContext.Result);
        }

        [Fact]
        public async Task ParticipantAccessingOrganizerEndpoint_ReturnsForbidden()
        {
            var context = new DefaultHttpContext();

            context.Items["UserId"] = 1;
            context.Items["Role"] = "Participant";

            var controller = new TestAuthController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            var attribute = new SessionAuthorizeAttribute("Organizer");

            var actionContext = new ActionContext(
                context,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());

            var executingContext = new ActionExecutingContext(
                actionContext,
                new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller);

            var executed = false;

            await attribute.OnActionExecutionAsync(
                executingContext,
                () =>
                {
                    executed = true;

                    return Task.FromResult<ActionExecutedContext>(
                        new ActionExecutedContext(
                            actionContext,
                            new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>(),
                            controller));
                });

            Assert.False(executed);

            var result = Assert.IsType<ObjectResult>(
                executingContext.Result);

            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task OrganizerAccessingParticipantEndpoint_ReturnsForbidden()
        {
            var context = new DefaultHttpContext();

            context.Items["UserId"] = 2;
            context.Items["Role"] = "Organizer";

            var controller = new TestAuthController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            var attribute = new SessionAuthorizeAttribute("Participant");

            var actionContext = new ActionContext(
                context,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());

            var executingContext = new ActionExecutingContext(
                actionContext,
                new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller);

            var executed = false;

            await attribute.OnActionExecutionAsync(
                executingContext,
                () =>
                {
                    executed = true;

                    return Task.FromResult<ActionExecutedContext>(
                        new ActionExecutedContext(
                            actionContext,
                            new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>(),
                            controller));
                });

            Assert.False(executed);

            var result = Assert.IsType<ObjectResult>(
                executingContext.Result);

            Assert.Equal(403, result.StatusCode);
        }
    }
}