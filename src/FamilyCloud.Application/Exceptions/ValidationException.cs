namespace FamilyCloud.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string msg) : base(msg) { }
    }
}