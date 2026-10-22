using FamilyCloud.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCloud.Api.ExceptionHandlers;

public class ForbiddenExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ForbiddenExceptionHandler> _logger;

    public ForbiddenExceptionHandler(ILogger<ForbiddenExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ForbiddenException)
            return false;

        _logger.LogError(exception, "Ошибка при обработке HTTP-запроса");
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Недостаточно прав",
            Detail = "На выполнение операции недостаточно прав"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await Results.Problem(problemDetails).ExecuteAsync(httpContext);

        return true;
    }
}