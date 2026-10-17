namespace FamilyCloud.Application.Dtos;

public record LoginResultDto(string AccessToken, DateTimeOffset ExpiresAt);