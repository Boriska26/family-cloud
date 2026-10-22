namespace FamilyCloud.Application.Exceptions
{
    public class InvalidInvitationException : Exception
    {
        public InvalidInvitationException() : base("Недействительное приглашение") { }
    }
}