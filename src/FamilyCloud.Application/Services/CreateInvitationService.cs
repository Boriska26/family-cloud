using FamilyCloud.Application.Exceptions;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services.Securities;
using FamilyCloud.Domain;

namespace FamilyCloud.Application.Services
{
    public class CreateInvitationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvitationTokenService _invitationTokenService;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ICurrentUser _currentUser;

        public CreateInvitationService(
            IUnitOfWork unitOfWork,
            IInvitationTokenService invitationTokenService,
            IInvitationRepository invitationRepository,
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IPersonRepository personRepository
            )
        {
            _unitOfWork = unitOfWork;
            _invitationTokenService = invitationTokenService;
            _invitationRepository = invitationRepository;
            _userRepository = userRepository;
            _personRepository = personRepository;
            _currentUser = currentUser;
        }

        public async Task<string> CreateAsync(
            Guid personId,
            CancellationToken cancellationToken)
        {
            User? user = await _userRepository.GetByIdAsync(_currentUser.UserId, cancellationToken);
            if (user is null || !user.CanInvite(DateTimeOffset.UtcNow))
            {
                throw new ForbiddenException();
            }

            Person? person = await _personRepository.GetByIdAsync(personId, cancellationToken);
            if (person == null)
            {
                throw new NotFoundException("Персона не найдена");
            }

            if (await _userRepository.ExistsByPersonIdAsync(personId, cancellationToken))
            {
                throw new ConflictException("У персоны уже есть учетная запись");
            }

            string token = _invitationTokenService.GenerateToken();
            string tokenHash = _invitationTokenService.ComputeHash(token);
            InvitationSetting invitationSetting = new InvitationSetting
            {
                PersonId = personId,
                TokenHash = tokenHash,
                InvitedByUserId = user.Id
            };

            Invitation invitation = new Invitation(invitationSetting);
            _invitationRepository.Add(invitation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return token;
        }
    }
}