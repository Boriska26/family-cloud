namespace FamilyCloud.Mobile.Models;

public record LoginResultDto(
    string AccessToken,
    DateTimeOffset ExpiresAt);