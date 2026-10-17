using System.Security.Claims;
using FamilyCloud.Application.Dtos;
using FamilyCloud.Application.Services.Securities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FamilyCloud.Infrastructure.Services.Securities;

public class JwtTokenService : IJwtTokenService
{
    private readonly ILogger<JwtTokenService> _logger;
    private readonly JwtTokenOptions _jwtTokenOptions;

    public JwtTokenService(IOptions<JwtTokenOptions> JwtOptions, ILogger<JwtTokenService> logger)
    {
        _logger = logger;
        _jwtTokenOptions = JwtOptions.Value;
    }

    public LoginResultDto Create(UserDto userDto)
    {
        try
        {
            byte[] keyBytes = Convert.FromBase64String(_jwtTokenOptions.SigningKey);

            if (keyBytes.Length < 32)
                throw new ArgumentException("Ключ меньше 32 байт");

            var tokenHanddler = new JsonWebTokenHandler();

            string secretString = _jwtTokenOptions.SigningKey;

            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(secretString));
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(15);
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = JwtTokenOptions.Issuer,
                Audience = JwtTokenOptions.Audience,
                IssuedAt = now,
                NotBefore = now,
                Expires = expires,
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, userDto.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, userDto.Login),
                    new Claim(ClaimTypes.Role, userDto.Role.ToString()),
                ]),
                
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            };

            string token = tokenHanddler.CreateToken(descriptor);
            DateTimeOffset expiresAt = new DateTimeOffset(expires);

            return new LoginResultDto(token, expiresAt);
        }
        catch (FormatException exp)
        {
            _logger.LogError(exp, "Не верный формат ключа");
            throw;
        }
    }
}