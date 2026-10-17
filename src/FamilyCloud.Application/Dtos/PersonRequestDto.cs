namespace FamilyCloud.Application.Dtos;

public record class PersonRequestDto(
    string? FirstName,
    string? MiddleName,
    string? LastName,
    DateOnly? BirthDate);