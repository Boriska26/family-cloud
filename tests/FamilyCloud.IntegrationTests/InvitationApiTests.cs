using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FamilyCloud.Application.Dtos;
using FamilyCloud.Application.Services.Securities;
using FamilyCloud.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace FamilyCloud.IntegrationTests;

public sealed class InvitationApiTests(PersonApiFactory factory) : IClassFixture<PersonApiFactory>
{
    private const string Password = "integration-password";

    [Theory]
    [InlineData(true, false, false, 200)]
    [InlineData(false, true, false, 200)]
    [InlineData(false, false, false, 403)]
    [InlineData(true, true, true, 403)]
    [InlineData(false, true, true, 403)]
    public async Task Create_ChecksPermission(bool admin, bool canInvite, bool blocked, int status)
    {
        var author = await SeedUser(admin, canInvite, blocked);
        var person = await SeedPerson();
        using var client = Client(author);
        using var response = await client.PostAsJsonAsync("/api/invitations", new { personId = person.Id });
        Assert.Equal((HttpStatusCode)status, response.StatusCode);
        await using var db = factory.CreateDbContext();
        if (status == 200)
        {
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.NotNull(result);
            var invitation = await db.Invitations.SingleAsync(x => x.PersonId == person.Id);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(result.Token))), invitation.TokenHash);
            Assert.NotEqual(result.Token, invitation.TokenHash);
            Assert.Equal(author.Id, invitation.InvitedByUserId);
        }
        else
        {
            await AssertProblem(response, status);
            Assert.False(await db.Invitations.AnyAsync(x => x.PersonId == person.Id));
        }
    }

    [Theory]
    [InlineData("missing", 404)]
    [InlineData("registered", 409)]
    [InlineData("anonymous", 401)]
    public async Task Create_RejectsInvalidRequest(string scenario, int status)
    {
        var author = await SeedUser();
        using var client = scenario == "anonymous" ? factory.CreateApiClient() : Client(author);
        using var response = await client.PostAsJsonAsync("/api/invitations",
            new { personId = scenario == "registered" ? author.PersonId : Guid.NewGuid() });
        Assert.Equal((HttpStatusCode)status, response.StatusCode);
        if (status != 401) await AssertProblem(response, status);
    }

    [Fact]
    public async Task Accept_Anonymous_CreatesMemberAndAllowsLogin_ThenRejectsReuse()
    {
        var (token, person, author) = await CreateInvitation();
        var login = $"member-{Guid.NewGuid():N}";
        using var client = factory.CreateApiClient();
        using var response = await Accept(client, token, login, Password);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var db = factory.CreateDbContext();
        var user = await db.Users.SingleAsync(x => x.PersonId == person.Id);
        Assert.Equal(Role.Member, user.Role);
        Assert.Equal(login, user.Login);
        Assert.Equal(author.Id, user.InvitedByUserId);
        Assert.NotEqual(Password, user.PasswordHash);
        using var scope = factory.Services.CreateScope();
        Assert.True(scope.ServiceProvider.GetRequiredService<IPasswordHasherService>().VerifyPassword(Password, user.PasswordHash));
        Assert.NotNull((await db.Invitations.SingleAsync(x => x.PersonId == person.Id)).UsedAt);
        using var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { login, password = Password });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var credentials = await loginResponse.Content.ReadFromJsonAsync<LoginResultDto>();
        Assert.NotNull(credentials);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentials.AccessToken);
        using var me = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        using var reuse = await Accept(client, token, login + "-other", Password);
        await AssertProblem(reuse, 400);
    }

    [Theory]
    [InlineData("unknown", 400)]
    [InlineData("expired", 400)]
    [InlineData("revoked", 400)]
    [InlineData("occupied", 409)]
    [InlineData("short-password", 400)]
    [InlineData("empty-login", 400)]
    [InlineData("registered-person", 409)]
    public async Task Accept_InvalidRequest_DoesNotCreateUserOrConsumeInvitation(string scenario, int status)
    {
        var (token, person, author) = await CreateInvitation();
        var login = $"new-{Guid.NewGuid():N}";
        await using (var db = factory.CreateDbContext())
        {
            var invitation = await db.Invitations.SingleAsync(x => x.PersonId == person.Id);
            if (scenario == "expired") db.Entry(invitation).Property(x => x.ExpiresAt).CurrentValue = DateTimeOffset.UtcNow.AddMinutes(-1);
            if (scenario == "revoked") invitation.Revoke(DateTimeOffset.UtcNow);
            if (scenario == "registered-person") db.Users.Add(new User(new UserSetting { PersonId = person.Id, Login = login + "-existing", PasswordHash = "unused" }));
            await db.SaveChangesAsync();
        }
        using var client = factory.CreateApiClient();
        using var response = await Accept(client, scenario == "unknown" ? "unknown-token" : token,
            scenario == "occupied" ? author.Login : scenario == "empty-login" ? " " : login,
            scenario == "short-password" ? "short" : Password);
        await AssertProblem(response, status);
        await using var check = factory.CreateDbContext();
        Assert.False(await check.Users.AnyAsync(x => x.Login == login));
        Assert.Equal(scenario == "registered-person" ? 1 : 0, await check.Users.CountAsync(x => x.PersonId == person.Id));
        Assert.Null((await check.Invitations.SingleAsync(x => x.PersonId == person.Id)).UsedAt);
    }

    [Fact]
    public async Task Accept_DatabaseFailure_RollsBackRegistrationAndReturnsSafeProblem()
    {
        var (token, person, _) = await CreateInvitation();
        // Login is limited to 100 characters by PostgreSQL. Fail during SaveChanges,
        // after both the new User and UsedAt have been staged in the same unit of work.
        var login = new string('x', 101);
        using var client = factory.CreateApiClient();
        using var response = await Accept(client, token, login, Password);
        await AssertProblem(response, 500);
        var problem = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(problem);
        Assert.Equal("Произошла внутренняя ошибка сервера", problem.Detail);
        Assert.All(problem.Extensions.Keys, key => Assert.Equal("traceId", key));
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(token, body);
        Assert.DoesNotContain(Password, body);
        Assert.DoesNotContain("stackTrace", body, StringComparison.OrdinalIgnoreCase);
        await using var db = factory.CreateDbContext();
        Assert.False(await db.Users.AnyAsync(x => x.PersonId == person.Id));
        Assert.Null((await db.Invitations.SingleAsync(x => x.PersonId == person.Id)).UsedAt);
        using var retry = await Accept(client, token, $"retry-{Guid.NewGuid():N}", Password);
        Assert.Equal(HttpStatusCode.NoContent, retry.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PostgreSql_RejectsDuplicateLoginOrPerson(bool duplicateLogin)
    {
        var existing = await SeedUser();
        var person = await SeedPerson();
        await using var db = factory.CreateDbContext();
        db.Users.Add(new User(new UserSetting {
            PersonId = duplicateLogin ? person.Id : existing.PersonId,
            Login = duplicateLogin ? existing.Login : $"unique-{Guid.NewGuid():N}", PasswordHash = "unused"
        }));
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        var postgres = Assert.IsType<PostgresException>(error.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
        Assert.Equal(duplicateLogin ? "IX_Users_Login" : "IX_Users_PersonId", postgres.ConstraintName);
    }

    private async Task<(string Token, Person Person, User Author)> CreateInvitation()
    {
        var author = await SeedUser();
        var person = await SeedPerson();
        using var client = Client(author);
        using var response = await client.PostAsJsonAsync("/api/invitations", new { personId = person.Id });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(result);
        return (result.Token, person, author);
    }

    private async Task<Person> SeedPerson()
    {
        var person = new Person("Приглашённый");
        await using var db = factory.CreateDbContext();
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        return person;
    }

    private async Task<User> SeedUser(bool admin = true, bool canInvite = false, bool blocked = false)
    {
        var person = await SeedPerson();
        var settings = new UserSetting { PersonId = person.Id, Login = $"author-{Guid.NewGuid():N}", PasswordHash = "unused" };
        var user = admin ? User.CreateAdministrator(settings) : new User(settings);
        await using var db = factory.CreateDbContext();
        db.Users.Add(user);
        db.Entry(user).Property(x => x.CanInviteOverride).CurrentValue = canInvite;
        db.Entry(user).Property(x => x.IsBlocked).CurrentValue = blocked;
        await db.SaveChangesAsync();
        return user;
    }

    private HttpClient Client(User user)
    {
        var client = factory.CreateApiClient();
        using var scope = factory.Services.CreateScope();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(new UserDto(user.Id, user.Login, user.Role));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.AccessToken);
        return client;
    }

    private static Task<HttpResponseMessage> Accept(HttpClient client, string token, string login, string password) =>
        client.PostAsJsonAsync("/api/invitations/accept", new { token, login, password });

    private static async Task AssertProblem(HttpResponseMessage response, int status)
    {
        Assert.Equal((HttpStatusCode)status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(problem);
        Assert.Equal(status, problem.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
    }

    private sealed record TokenResponse(string Token);
}
