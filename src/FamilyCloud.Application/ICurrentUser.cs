namespace FamilyCloud.Application;

public interface ICurrentUser
{
    Guid UserId { get; }
}