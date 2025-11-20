using GMAO.Data;
using GMAO.Helpers;
using GMAO.Models.DTOs.AuthDto;
using GMAO.Models.DTOs.UserDtos;
using GMAO.Models.Entities;
using GMAO.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GMAO.Services.Implementation
{
    public class AuthService : IAuthService
    {

        private readonly ApplicationDbContext _context;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AppSettings _appSettings;
        public AuthService(ApplicationDbContext context, JwtTokenService jwtTokenService, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _appSettings = appSettings.Value;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password.");
            }
            if(!user.IsActive)
            {
                throw new Exception("User account is inactive.");
            }
            var token = _jwtTokenService.GenerateToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_appSettings.RefreshTokenExpirationDays);
            await _context.SaveChangesAsync();
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(_appSettings.TokenExpirationMinutes),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive
                }
            };


        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                throw new Exception("Invalid token.");
            }
            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(Guid.Parse(userId!));
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                throw new Exception("Invalid refresh token.");
            }
            var newToken = _jwtTokenService.GenerateToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
            
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_appSettings.RefreshTokenExpirationDays);
            await _context.SaveChangesAsync();
            return new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(_appSettings.TokenExpirationMinutes),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive
                }
            };

        }

        public async  Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {

            if( await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new Exception("User with this email already exists.");
            }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var user = new User {  
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = registerDto.Role,
                  IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive
            };

        }
    }
}
