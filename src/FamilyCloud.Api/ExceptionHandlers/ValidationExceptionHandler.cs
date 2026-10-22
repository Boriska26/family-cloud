using FamilyCloud.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace FamilyCloud.Api.ExceptionHandlers;

public sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException)
            return false;

        await Results.Problem(statusCode: StatusCodes.Status400BadRequest,
            title: "Ошибка проверки данных", detail: exception.Message).ExecuteAsync(httpContext);
        return true;
    }
}
