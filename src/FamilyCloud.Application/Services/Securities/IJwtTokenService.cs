using FamilyCloud.Application.Dtos;

namespace FamilyCloud.Application.Services.Securities;

public interface IJwtTokenService
{
    LoginResultDto Create(UserDto userDto);
}