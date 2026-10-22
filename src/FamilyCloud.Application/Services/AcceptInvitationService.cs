using FamilyCloud.Application.Dtos;
using FamilyCloud.Application.Exceptions;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services.Securities;
using FamilyCloud.Domain;

namespace FamilyCloud.Application.Services;

public class AcceptInvitationService
{
    private readonly IInvitationTokenService _invitationTokenService;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptInvitationService(
        IInvitationTokenService invitationTokenService,
        IInvitationRepository invitationRepository,
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        IUnitOfWork unitOfWork)
    {
        _invitationTokenService = invitationTokenService;
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _unitOfWork = unitOfWork;
    }

    public async Task AcceptAsync(AcceptInvitationRequestDto acceptInvitationRequestDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(acceptInvitationRequestDto.Login))
            throw new ValidationException("Пустой логин");
        var login = acceptInvitationRequestDto.Login.Trim().ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;
        string tokenHash = _invitationTokenService.ComputeHash(acceptInvitationRequestDto.Token);
        var invitation = await _invitationRepository.GetTrackingByTokenHashAsync(tokenHash, cancellationToken);
        if (invitation == null)
            throw new InvalidInvitationException();
        if (!invitation.IsValid(now))
            throw new InvalidInvitationException();
        if (await _userRepository.ExistsByPersonIdAsync(invitation.PersonId, cancellationToken))
            throw new ConflictException("У персоны уже есть учетная запись");
        if (await _userRepository.GetByLoginAsync(login, cancellationToken) != null)
            throw new ConflictException("Логин занят");
        if (string.IsNullOrWhiteSpace(acceptInvitationRequestDto.Password) || acceptInvitationRequestDto.Password.Length < 8)
            throw new ValidationException("Слишком слабый пароль");

        string passwordHash = _passwordHasherService.HashPassword(acceptInvitationRequestDto.Password);
        UserSetting userSetting = new UserSetting
        {
            InvitedByUserId = invitation.InvitedByUserId,
            Login = login,
            PasswordHash = passwordHash,
            PersonId = invitation.PersonId
        };
        User user = new User(userSetting);
        invitation.MarkAsUsed(now);
        _userRepository.Add(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}