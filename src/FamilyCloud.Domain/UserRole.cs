namespace FamilyCloud.Domain
{
    public class UserRole
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public Role Role { get; private set; }
    }
}