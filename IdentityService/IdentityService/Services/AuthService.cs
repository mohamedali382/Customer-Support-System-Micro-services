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

        public async Task<string?> RegisterAsync(RegisterDto dto)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists) return null;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role 
            };

            var hasher = new PasswordHasher<ApplicationUser>();
            user.PasswordHash = hasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.Id.ToString();   // ✅ return the ID
        }

        public async Task<AuthResultDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return null;

            var hasher = new PasswordHasher<ApplicationUser>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            var token = _jwtService.GenerateToken(user);

        
            return new AuthResultDto
            {
                Token = token,
                Id = user.Id.ToString(),
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber ?? ""
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "User")
                .Select(u => new UserDto
                {
                    Id = u.Id.ToString(),
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber ?? ""
                })
                .ToListAsync();
        }
    }
}