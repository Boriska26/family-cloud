namespace FamilyCloud.Domain
{
    public class Person
    {
        private Person() { }

        public Person(
            string? firstName = null,
            string? middleName = null,
            string? lastName = null,
            DateOnly? birthDate = null)
        {
            Id = Guid.NewGuid();
            FirstName = string.IsNullOrWhiteSpace(firstName)
                ? "Неизвестный" : firstName;
            MiddleName = middleName;
            LastName = lastName;
            BirthDate = birthDate;
        }

        public Guid Id { get; private set; }

        public string? FirstName { get; private set; }

        public string? MiddleName { get; private set; }

        public string? LastName { get; private set; }

        public DateOnly? BirthDate { get; private set; }

        public void Update(string? firstName, string? middleName, string? lastName, DateOnly? birthDate)
        {
            FirstName = string.IsNullOrWhiteSpace(firstName) ? "Незнакомец" : firstName.Trim();
            MiddleName = middleName?.Trim();
            LastName = lastName?.Trim();
            BirthDate = birthDate;
        }
    }
}