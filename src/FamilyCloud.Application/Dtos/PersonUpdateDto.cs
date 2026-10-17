namespace FamilyCloud.Application.Dtos;

public record PersonUpdateDto(string? FirstName, string? MiddleName, string? LastName, DateOnly? BirthDate);