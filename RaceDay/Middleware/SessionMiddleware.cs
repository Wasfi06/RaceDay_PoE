using RaceDay.Services;

namespace RaceDay.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            SessionService sessionService)
        {
            // Read the session ID from the request header
            var sessionHeader = context.Request.Headers["X-Session-Id"]
                .FirstOrDefault();

            if (Guid.TryParse(sessionHeader, out var sessionId))
            {
                var session = await sessionService.GetValidSessionAsync(sessionId);

                if (session != null)
                {
                    // Store authenticated user information
                    context.Items["UserId"] = session.UserId;
                    context.Items["Role"] = session.RoleSnapshot;
                    context.Items["SessionId"] = session.SessionId;
                }
            }

            await _next(context);
        }
    }
}
