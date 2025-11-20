using GMAO.Models.DTOs.UserDtos;

namespace GMAO.Models.DTOs.AuthDto
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; } = null!;

    }
}
