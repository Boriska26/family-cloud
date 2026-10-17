namespace FamilyCloud.Application.Dtos;

public record class UserDto(Guid Id, string Login, Role Role);