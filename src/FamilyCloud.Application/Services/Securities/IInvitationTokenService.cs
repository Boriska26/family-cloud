namespace FamilyCloud.Application.Services.Securities
{
    public interface IInvitationTokenService
    {
        string GenerateToken();

        string ComputeHash(string token);
    }
}