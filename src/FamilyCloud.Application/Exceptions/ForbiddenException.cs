namespace FamilyCloud.Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("Недостаточно прав для выполнения операции")
    {
    }
}