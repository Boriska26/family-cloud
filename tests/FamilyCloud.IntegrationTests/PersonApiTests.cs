using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FamilyCloud.Application.Dtos;
using FamilyCloud.Application.Services.Securities;
using FamilyCloud.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FamilyCloud.IntegrationTests;

public sealed class PersonApiTests(PersonApiFactory factory) : IClassFixture<PersonApiFactory>
{
    private static readonly PersonRequestDto CreateRequest = new("Иван", "Петрович", "Смирнов", new DateOnly(1990, 4, 12));
    private static readonly PersonUpdateDto UpdateRequest = new("Анна", "Ивановна", "Орлова", new DateOnly(1995, 6, 20));

    [Fact]
    public async Task Create_AuthenticatedUser_ReturnsCreatedAndPersistsPerson()
    {
        var user = await SeedUser();
        using var client = AuthenticatedClient(user);
        using var response = await client.PostAsJsonAsync("/api/persons", CreateRequest);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PersonDto>();
        Assert.NotNull(dto);
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal($"/api/persons/{dto.Id}", response.Headers.Location?.OriginalString);
        Assert.Equal(new PersonDto(dto.Id, CreateRequest.FirstName, CreateRequest.MiddleName,
            CreateRequest.LastName, CreateRequest.BirthDate), dto);
        await AssertStored(dto);
    }

    [Fact]
    public async Task Get_ExistingPerson_ReturnsPerson()
    {
        var user = await SeedUser();
        using var client = AuthenticatedClient(user);
        using var response = await client.GetAsync($"/api/persons/{user.PersonId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PersonDto>();
        Assert.NotNull(dto);
        Assert.Equal(user.PersonId, dto.Id);
        await AssertStored(dto);
    }

    [Fact]
    public async Task Get_MissingPerson_ReturnsNotFound()
    {
        using var client = AuthenticatedClient(await SeedUser());
        using var response = await client.GetAsync($"/api/persons/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_OwnPerson_Succeeds()
    {
        var user = await SeedUser();
        await AssertUpdate(user, user.PersonId, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_PersonWithoutUser_Succeeds()
    {
        var user = await SeedUser();
        await using var db = factory.CreateDbContext();
        var person = new Person("Без пользователя");
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        await AssertUpdate(user, person.Id, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_AnotherUsersPerson_ReturnsForbiddenAndPreservesData()
    {
        var user = await SeedUser();
        var other = await SeedUser();
        await AssertUpdate(user, other.PersonId, HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_Administrator_CanEditOwnOrAnotherUsersPerson(bool anotherUser)
    {
        var admin = await SeedUser(administrator: true);
        var target = anotherUser ? await SeedUser() : admin;
        await AssertUpdate(admin, target.PersonId, HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_BlockedUser_ReturnsForbiddenEvenForAdministrator(bool administrator)
    {
        var user = await SeedUser(administrator, blocked: true);
        await AssertUpdate(user, user.PersonId, HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task Request_WithoutJwt_ReturnsUnauthorized(string method)
    {
        var user = await SeedUser();
        using var client = factory.CreateApiClient();
        using var request = new HttpRequestMessage(new HttpMethod(method),
            method == "POST" ? "/api/persons" : $"/api/persons/{user.PersonId}");
        if (method != "GET") request.Content = JsonContent.Create(CreateRequest);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<User> SeedUser(bool administrator = false, bool blocked = false)
    {
        await using var db = factory.CreateDbContext();
        var person = new Person("Исходное", "Отчество", "Фамилия", new DateOnly(1980, 1, 2));
        var settings = new UserSetting
        {
            PersonId = person.Id, Login = $"test-{Guid.NewGuid():N}", PasswordHash = "unused-test-hash"
        };
        var user = administrator ? User.CreateAdministrator(settings) : new User(settings);
        db.Persons.Add(person);
        db.Users.Add(user);
        // The domain exposes no blocking method; set persisted state through EF for test setup.
        db.Entry(user).Property(x => x.IsBlocked).CurrentValue = blocked;
        await db.SaveChangesAsync();
        return user;
    }

    private HttpClient AuthenticatedClient(User user)
    {
        var client = factory.CreateApiClient();
        using var scope = factory.Services.CreateScope();
        var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>()
            .Create(new UserDto(user.Id, user.Login, user.Role));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return client;
    }

    private async Task AssertUpdate(User user, Guid personId, HttpStatusCode expected)
    {
        await using var db = factory.CreateDbContext();
        var before = await db.Persons.AsNoTracking().SingleAsync(x => x.Id == personId);
        using var client = AuthenticatedClient(user);
        using var response = await client.PutAsJsonAsync($"/api/persons/{personId}", UpdateRequest);
        Assert.Equal(expected, response.StatusCode);
        var expectedDto = expected == HttpStatusCode.OK
            ? new PersonDto(personId, UpdateRequest.FirstName, UpdateRequest.MiddleName, UpdateRequest.LastName, UpdateRequest.BirthDate)
            : new PersonDto(personId, before.FirstName, before.MiddleName, before.LastName, before.BirthDate);
        if (expected == HttpStatusCode.OK)
            Assert.Equal(expectedDto, await response.Content.ReadFromJsonAsync<PersonDto>());
        await AssertStored(expectedDto);
    }

    private async Task AssertStored(PersonDto expected)
    {
        await using var db = factory.CreateDbContext();
        var actual = await db.Persons.AsNoTracking().SingleAsync(x => x.Id == expected.Id);
        Assert.Equal(expected, new PersonDto(actual.Id, actual.FirstName, actual.MiddleName, actual.LastName, actual.BirthDate));
    }
}

