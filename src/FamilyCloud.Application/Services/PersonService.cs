using System.Security.Cryptography.X509Certificates;
using FamilyCloud.Application.Dtos;
using FamilyCloud.Application.Exceptions;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Domain;

namespace FamilyCloud.Application.Services;

public class PersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public PersonService(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ICurrentUser currentUser
        )
    {
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<PersonDto> CreateAsync(PersonRequestDto personRequestDto, CancellationToken cancellationToken)
    {
        Person person = new Person(
            personRequestDto.FirstName,
            personRequestDto.MiddleName,
            personRequestDto.LastName,
            personRequestDto.BirthDate);

        _personRepository.Add(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PersonDto(person.Id, person.FirstName, person.MiddleName, person.LastName, person.BirthDate);
    }

    public async Task<PersonDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var person = await _personRepository.GetByIdAsync(id, cancellationToken);

        return person == null ? null
            : new PersonDto(person.Id, person.FirstName, person.MiddleName, person.LastName, person.BirthDate);
    }

    public async Task<PersonDto> UpdateAsync(Guid id, PersonUpdateDto personUpdateDto, CancellationToken cancellationToken)
    {
        var person = await _personRepository.GetTrackedByIdAsync(id, cancellationToken);
        if (person is null)
            throw new NotFoundException("Текущая персона не найдена");

        var user = await _userRepository.GetByIdAsync(_currentUser.UserId, cancellationToken);

        if (user is null)
            throw new ForbiddenException();
        if (user.IsBlocked)
            throw new ForbiddenException();

        bool canUpdate = user.Role == Role.Administrator
            || user.PersonId == id
            || !await _userRepository.ExistsByPersonIdAsync(id, cancellationToken);

        if (!canUpdate)
            throw new ForbiddenException();

        person.Update(personUpdateDto.FirstName, personUpdateDto.MiddleName, personUpdateDto.LastName, personUpdateDto.BirthDate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PersonDto(
            person.Id,
            person.FirstName,
            person.MiddleName,
            person.LastName,
            person.BirthDate
        );
    }
}