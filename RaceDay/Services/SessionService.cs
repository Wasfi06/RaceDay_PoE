using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Services
{
    public class SessionService
    {
        private readonly RaceDayDbContext _context;

        public SessionService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<Session> CreateSessionAsync(User user)
        {
            var session = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = user.UserId,
                RoleSnapshot = user.Role,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                RevokedAt = null
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<Session?> GetValidSessionAsync(Guid sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
            {
                return null;
            }

            if (session.RevokedAt != null)
            {
                return null;
            }

            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                return null;
            }

            if (!session.User.IsActive)
            {
                return null;
            }

            return session;
        }

        public async Task<bool> RevokeSessionAsync(Guid sessionId)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
            {
                return false;
            }

            session.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
