using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCloud.Api.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка при обработке HTTP-запроса");

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Непредвиденная ошибка",
            Detail = "Произошла внутренняя ошибка сервера"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await Results.Problem(problemDetails).ExecuteAsync(httpContext);

        return true;
    }
}