using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services.Securities;
using FamilyCloud.Domain;

namespace FamilyCloud.Application.Services;

public class CreateFirstAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFirstAdminService(
        IUserRepository userRepository,
        IPersonRepository personRepository,
        IPasswordHasherService passwordHasherService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _personRepository = personRepository;
        _passwordHasherService = passwordHasherService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CreateAsync(string login, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return false;

        await using var trannsaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _unitOfWork.AcquireBootstrapLockAsync(cancellationToken);

        bool existAdmin = await _userRepository.HasAdministratorAsync(cancellationToken);
        if (existAdmin)
            return false;

        Person person = new Person("ADMINCHANGEME");
        _personRepository.Add(person);

        User user = User.CreateAdministrator(new UserSetting
        {
            Login = login,
            PasswordHash = _passwordHasherService.HashPassword(password),
            PersonId = person.Id
        });
        _userRepository.Add(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await trannsaction.CommitAsync(cancellationToken);

        return true;
    }
}