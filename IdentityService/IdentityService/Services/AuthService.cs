using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Entities;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;

        private readonly JwtService _jwtService;

        public AuthService(
            AuthDbContext context,
            JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (exists)
                return false;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role
            };

            var hasher =
                new PasswordHasher<ApplicationUser>();

            user.PasswordHash =
                hasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == dto.Email
                );

            if (user == null)
                return null;

            var hasher = new PasswordHasher<ApplicationUser>();

            var result =
                hasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    dto.Password
                );

            if (result ==
                PasswordVerificationResult.Failed)
                return null;

            return _jwtService.GenerateToken(user);
        }
    }
}