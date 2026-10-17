using Microsoft.Extensions.Options;

namespace FamilyCloud.Infrastructure.Services.Securities;

public class JwtTokenOptionsValidator : IValidateOptions<JwtTokenOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtTokenOptions options)
    {
        try
        {
            byte[] keyBytes = Convert.FromBase64String(options.SigningKey);

            if (keyBytes.Length < 32)
                return ValidateOptionsResult.Fail("Ключ имеет размер меньше 32 байт");

            return ValidateOptionsResult.Success;
        }
        catch (FormatException)
        {
            return ValidateOptionsResult.Fail("JWT SigningKey должен быть строкой Base64");
        }
    }
}