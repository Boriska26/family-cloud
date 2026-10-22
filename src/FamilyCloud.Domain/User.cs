namespace FamilyCloud.Domain
{
    public class User
    {
        private User() { }

        public User(UserSetting userSetting) : this(userSetting, Role.Member) { }

        private User(UserSetting userSetting, Role role)
        {
            if (userSetting is null)
                throw new ArgumentNullException(nameof(userSetting));
            if (userSetting.PersonId == Guid.Empty)
                throw new ArgumentException("personId cannot be empty");

            ArgumentException.ThrowIfNullOrWhiteSpace(userSetting.Login);
            ArgumentException.ThrowIfNullOrWhiteSpace(userSetting.PasswordHash);

            Id = Guid.NewGuid();
            IsBlocked = false;
            Login = userSetting.Login.Trim().ToLowerInvariant();
            PasswordHash = userSetting.PasswordHash;
            CreatedAt = DateTimeOffset.UtcNow;
            Role = role;
            InvitedByUserId = userSetting.InvitedByUserId;
            PersonId = userSetting.PersonId;
        }

        public Guid Id { get; private set; }

        public string Login { get; private set; } = string.Empty;

        public string PasswordHash { get; private set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; private set; }

        public Guid? InvitedByUserId { get; private set; }

        public User? InvitedByUser { get; private set; }

        public bool? CanInviteOverride { get; private set; }

        public bool IsBlocked { get; private set; }

        public Role Role { get; private set; }

        public Guid PersonId { get; private set; }

        public static User CreateAdministrator(UserSetting userSetting)
        {
            return new User(userSetting, Role.Administrator);
        }

        public bool CanInvite(DateTimeOffset now)
        {
            if (IsBlocked)
                return false;

            if (Role == Role.Administrator)
                return true;

            if (CanInviteOverride.HasValue)
                return CanInviteOverride.Value;

            return CreatedAt.AddDays(30) <= now;
        }
    }
}