public class JwtTokenOptions
{
    public const string Issuer = nameof(FamilyCloud);
    public const string Audience = nameof(FamilyCloud);

    public string SigningKey { get; set; } = string.Empty;
}