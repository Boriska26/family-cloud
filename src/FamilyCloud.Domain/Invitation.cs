using System.Runtime.CompilerServices;

namespace FamilyCloud.Domain
{
    public class Invitation
    {
        private Invitation() { }

        public Invitation(InvitationSetting invitationSetting)
        {
            if (invitationSetting == null)
                throw new ArgumentNullException(nameof(invitationSetting));
            if (invitationSetting.InvitedByUserId == Guid.Empty)
                throw new ArgumentException("InvitedUserId cannot be empty");
            if (invitationSetting.PersonId == Guid.Empty)
                throw new ArgumentException("PersonId cannot be empty");

            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(invitationSetting.TokenHash));

            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
            ExpiresAt = CreatedAt.AddMinutes(10);
            InvitedByUserId = invitationSetting.InvitedByUserId;
            PersonId = invitationSetting.PersonId;
            TokenHash = invitationSetting.TokenHash;
        }

        public Guid Id { get; private set; }

        public Guid PersonId { get; private set; }

        public Guid InvitedByUserId { get; private set; }

        public string TokenHash { get; private set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        public DateTimeOffset? UsedAt { get; private set; }

        public DateTimeOffset? RevokedAt { get; private set; }

        public bool IsValid(DateTimeOffset now)
        {
            return UsedAt == null && RevokedAt == null && now < ExpiresAt;
        }

        public void Revoke(DateTimeOffset now)
        {
            if (UsedAt != null || RevokedAt != null)
                return;

            RevokedAt = now;
        }

        public void MarkAsUsed(DateTimeOffset now)
        {
            if (!IsValid(now))
                throw new InvalidOperationException("Invitation is no longer valid");

            UsedAt = now;
        }
    }
}