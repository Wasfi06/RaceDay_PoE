using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string? _requiredRole;

        public SessionAuthorizeAttribute(string? requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var userId = context.HttpContext.Items["UserId"];
            var role = context.HttpContext.Items["Role"] as string;

            // No valid session
            if (userId == null || string.IsNullOrEmpty(role))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Authentication is required."
                });

                return;
            }

            // Check required role
            if (!string.IsNullOrEmpty(_requiredRole) &&
                !string.Equals(role, _requiredRole, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new
                {
                    message = "You do not have permission to access this resource."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };

                return;
            }

            await next();
        }
    }
}
