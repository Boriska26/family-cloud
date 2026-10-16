using System.Security.Cryptography;
using System.Text;
using FamilyCloud.Application.Services.Securities;

namespace FamilyCloud.Infrastructure.Services.Securities
{
    public class InvitationTokenService : IInvitationTokenService
    {
        public string ComputeHash(string token)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            
            byte[] computeHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(computeHashBytes);
        }

        public string GenerateToken()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(32);
            string token = Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
                
            return token;
        }
    }
}