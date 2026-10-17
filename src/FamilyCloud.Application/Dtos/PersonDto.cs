namespace FamilyCloud.Application.Dtos;

public record PersonDto(
    Guid Id,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    DateOnly? BirthDate);