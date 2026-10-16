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

        public CreateInvitationService(
            IUnitOfWork unitOfWork,
            IInvitationTokenService invitationTokenService,
            IInvitationRepository invitationRepository,
            IUserRepository userRepository,
            IPersonRepository personRepository
            )
        {
            _unitOfWork = unitOfWork;
            _invitationTokenService = invitationTokenService;
            _invitationRepository = invitationRepository;
            _userRepository = userRepository;
            _personRepository = personRepository;
        }

        public async Task<string?> CreateAsync(
            Guid invitedByUserId,
            Guid personId,
            CancellationToken cancellationToken)
        {
            User? user = await _userRepository.GetByIdAsync(invitedByUserId, cancellationToken);
            if (user == null)
            {
                //TODO Поменять на собственный Exception
                return null;
            }

            Person? person = await _personRepository.GetByIdAsync(personId, cancellationToken);
            if (person == null)
            {
                //TODO Поменять на собственный Exception
                return null;
            }

            if (await _userRepository.ExistsByPersonIdAsync(personId, cancellationToken))
            {
                //TODO Поменять на собственный Exception
                return null;
            }

            if (!user.CanInvite(DateTimeOffset.UtcNow))
            {
                //TODO Поменять на собственный Exception
                return null;
            }

            string token = _invitationTokenService.GenerateToken();
            string tokenHash = _invitationTokenService.ComputeHash(token);
            InvitationSetting invitationSetting = new InvitationSetting
            {
                PersonId = personId,
                TokenHash = tokenHash,
                InvitedByUserId = invitedByUserId
            };

            Invitation invitation = new Invitation(invitationSetting);
            _invitationRepository.Add(invitation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return token;
        }
    }
}