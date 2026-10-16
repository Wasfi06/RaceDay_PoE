using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Models;

namespace RaceDay.Services
{
    public class AuthService
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(RaceDayDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(
            RegisterRequest request)
        {
            // Check that the role is valid
            if (request.Role != "Organizer" &&
                request.Role != "Participant")
            {
                return (false, "Role must be Organizer or Participant.", null);
            }

            // Check whether the email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return (false, "An account with this email already exists.", null);
            }

            // Create the user
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = request.Role,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Hash the password
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Registration successful.", user);
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(
            LoginRequest request)
        {
            // Find the user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return (false, "Invalid email or password.", null);
            }

            // Check whether the account is active
            if (!user.IsActive)
            {
                return (false, "This account is inactive.", null);
            }

            // Verify the password against the stored hash
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return (false, "Invalid email or password.", null);
            }

            return (true, "Login successful.", user);
        }
    }
}
