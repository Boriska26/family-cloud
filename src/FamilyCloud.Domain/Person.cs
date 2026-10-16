namespace FamilyCloud.Domain
{
    public class Person
    {
        private Person(){}

        public Person(
            string? firstName = null,
            string? middleName = null,
            string? lastName = null,
            DateOnly? birthDate = null)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            BirthDate = birthDate;
        }

        public Guid Id { get; private set; }

        public string? FirstName { get; private set; }

        public string? MiddleName { get;private  set; }

        public string? LastName { get;private  set; }

        public DateOnly? BirthDate { get;private  set; }
    }
}