namespace FamilyCloud.Application.Dtos;

public record AcceptInvitationRequestDto(
    string Token,
    string Login,
    string Password);