namespace FamilyCloud.Domain
{
    public class InvitationSetting
    {
        public Guid PersonId { get; init; }

        public Guid InvitedByUserId { get; init; }

        public string TokenHash { get; init; } = string.Empty;
    }
}