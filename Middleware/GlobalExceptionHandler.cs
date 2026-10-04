using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhaApi.Exceptions;

namespace MinhaApi.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return false;

        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            ArgumentException or BadHttpRequestException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflito ao persistir os dados"),
            OperationCanceledException => (StatusCodes.Status500InternalServerError, "Operação cancelada"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Erro não tratado em {Path}", httpContext.Request.Path);
        else
            _logger.LogWarning(exception, "{Title} em {Path}", title, httpContext.Request.Path);

        var detail = status == StatusCodes.Status500InternalServerError && !_env.IsDevelopment()
            ? "Ocorreu um erro interno. Tente novamente mais tarde."
            : exception.Message;

        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}
