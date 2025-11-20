using GMAO.Models.DTOs.AuthDto;
using GMAO.Models.DTOs.UserDtos;

namespace GMAO.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken);
    }
}
