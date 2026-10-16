namespace FamilyCloud.Domain
{
    public class UserSetting
    {
        public Guid PersonId { get; init; }

        public string PasswordHash { get; init; } = string.Empty;

        public Guid? InvitedByUserId { get; init; }

        public string Login { get; init; } = string.Empty;
    }
}