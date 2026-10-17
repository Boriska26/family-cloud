using FamilyCloud.Application.Dtos;
using FamilyCloud.Application.Exceptions;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services.Securities;
using FamilyCloud.Domain;

namespace FamilyCloud.Application.Services;

public class LoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginService(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResultDto> Login(string login, string password, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByLoginAsync(login, cancellationToken);
        if (user == null)
            throw new InvalidCredentialsException("The login or password is entered incorrectly");

        if (user.IsBlocked)
            throw new InvalidCredentialsException("The login or password is entered incorrectly");

        if (!_passwordHasherService.VerifyPassword(password, user.PasswordHash))
            throw new InvalidCredentialsException("The login or password is entered incorrectly");

        var resultToken = _jwtTokenService.Create(new UserDto(user.Id, user.Login, user.Role));

        return resultToken;
    }
}