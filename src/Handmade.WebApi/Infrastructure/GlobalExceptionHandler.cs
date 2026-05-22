using Ardalis.GuardClauses;
using Handmade.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ApplicationException = Handmade.Application.Common.Exceptions.ApplicationException;

namespace Handmade.WebApi.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly Dictionary<Type, Func<Exception, ProblemDetails>> _exceptionHandlers;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
        _exceptionHandlers = new Dictionary<Type, Func<Exception, ProblemDetails>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnauthorizedException), HandleUnauthorizedException},
            { typeof(DomainException), HandleDomainException},
            { typeof(ApplicationException), HandleApplicationException },
        };
    }

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        var problemDetails = _exceptionHandlers.TryGetValue(exceptionType, out Func<Exception, ProblemDetails>? func)
            ? func.Invoke(exception)
            : HandleUnhandledException(exception);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        return _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        });
    }
    
    private static ProblemDetails HandleValidationException(Exception ex)
    {
        var exception = (ValidationException)ex;

        var problemDetails = new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred",
            Detail = exception.Message,
        };

        problemDetails.Extensions.TryAdd("code", "ValidationError");

        return problemDetails;
    }

    private static ProblemDetails HandleNotFoundException(Exception ex)
    {
        var exception = (NotFoundException)ex;

        return new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "The specified resource was not found.",
            Detail = exception.Message,
        };
    }

    private static ProblemDetails HandleUnauthorizedException(Exception ex)
    {
        var exception = (UnauthorizedException)ex;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = exception.Message,
        };

        problemDetails.Extensions.TryAdd("code", exception.Code);

        return problemDetails;
    }

    private static ProblemDetails HandleDomainException(Exception ex)
    {
        var exception = (DomainException)ex;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = exception.Title,
            Detail = exception.Message,
        };

        if (!string.IsNullOrEmpty(exception.Code))
        {
            problemDetails.Extensions.TryAdd("code", exception.Code);
        }

        return problemDetails;
    }

    private ProblemDetails HandleApplicationException(Exception ex)
    {
        _logger.LogError(ex, "Something went wrong");

        var exception = (ApplicationException)ex;

        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = exception.Title,
            Detail = exception.Message
        };
    }

    private ProblemDetails HandleUnhandledException(Exception ex)
    {
        _logger.LogError(ex, "Unhandled Exception");

        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Error",
            Detail = "An error occurred while processing your request."
        };
    }
}