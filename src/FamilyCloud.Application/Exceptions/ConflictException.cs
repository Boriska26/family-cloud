namespace FamilyCloud.Application.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string msg)
        : base(msg)
    {
    }
}