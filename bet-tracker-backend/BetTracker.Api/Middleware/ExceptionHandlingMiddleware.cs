using System.Net;
using BetTracker.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace BetTracker.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

     public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private  async Task HandleAsync(HttpContext context, Exception exception)
    {
        var(status, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            ForbiddenException => (HttpStatusCode.Forbidden, "Forbidden"),
            DomainRuleException => (HttpStatusCode.BadRequest, "Invalid operation"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        var isUnexpected = status == HttpStatusCode.InternalServerError;

        if (isUnexpected)
        {
            _logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
        }

        var detail = isUnexpected && !_environment.IsDevelopment()
            ? "An unexpected error occurred."
            : exception.Message;

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = status == HttpStatusCode.InternalServerError? "An unexpected error occurred." : exception.Message,
            Instance = context.Request.Path
        };
        if (isUnexpected && _environment.IsDevelopment())
        {
            problem.Extensions["exceptionType"] = exception.GetType().FullName;
            problem.Extensions["innerException"] = exception.InnerException?.Message;
            problem.Extensions["stackTrace"] = exception.StackTrace;
        }
        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }

}