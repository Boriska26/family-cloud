using System.Security.Claims;
using FamilyCloud.Application;

namespace FamilyCloud.Api.Models;

public class CurrentUser : ICurrentUser
{
    private readonly Guid _userId;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
            throw new InvalidOperationException("Пользовтель не доступен");
        if (user.Identity?.IsAuthenticated != true)
            throw new InvalidOperationException("Пользовтель не доступен");

        var userId = user.FindFirst("sub")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Пользовтель не доступен");

        if (!Guid.TryParse(userId, out _userId))
            throw new InvalidOperationException("Пользовтель не доступен");
    }

    public Guid UserId => _userId;
}