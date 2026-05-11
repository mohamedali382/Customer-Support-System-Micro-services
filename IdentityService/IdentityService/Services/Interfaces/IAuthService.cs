using IdentityService.DTOs;

namespace IdentityService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(RegisterDto dto);

        Task<AuthResultDto?> LoginAsync(LoginDto dto);

        Task<List<UserDto>> GetAllUsersAsync();
    }
}