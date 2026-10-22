using FamilyCloud.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace FamilyCloud.Api.ExceptionHandlers;

public sealed class InvalidInvitationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not InvalidInvitationException)
            return false;

        await Results.Problem(statusCode: StatusCodes.Status400BadRequest,
            title: "Недействительное приглашение", detail: exception.Message).ExecuteAsync(httpContext);
        return true;
    }
}
