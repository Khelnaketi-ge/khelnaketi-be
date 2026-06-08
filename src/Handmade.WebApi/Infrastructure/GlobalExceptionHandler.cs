using Ardalis.GuardClauses;
using Handmade.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            { typeof(DbUpdateException), HandleDbUpdateException },
        };
    }

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        var problemDetails = _exceptionHandlers.TryGetValue(exceptionType, out var func)
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

    private ProblemDetails HandleDbUpdateException(Exception ex)
    {
        if (TryGetUniqueConstraintName(ex, out var constraintName)
            && TryMapUniqueConstraint(constraintName, out var fieldName, out var message))
        {
            return HandleValidationException(new ValidationException(fieldName, message));
        }

        return HandleUnhandledException(ex);
    }

    private static bool TryGetUniqueConstraintName(Exception exception, out string constraintName)
    {
        if (GetStringProperty(exception, "SqlState") == "23505"
            && GetStringProperty(exception, "ConstraintName") is { Length: > 0 } foundConstraintName)
        {
            constraintName = foundConstraintName;
            return true;
        }

        if (exception.InnerException is not null)
        {
            return TryGetUniqueConstraintName(exception.InnerException, out constraintName);
        }

        constraintName = string.Empty;
        return false;
    }

    private static bool TryMapUniqueConstraint(
        string constraintName,
        out string fieldName,
        out string message)
    {
        switch (constraintName)
        {
            case "IX_Users_NormalizedEmail":
                fieldName = "Email";
                message = "Email address is already registered";
                return true;
            case "IX_Users_NormalizedPhoneNumber":
                fieldName = "PhoneNumber";
                message = "Phone number is already registered";
                return true;
            case "IX_Brands_NormalizedName":
                fieldName = "Name";
                message = "Brand name is already registered";
                return true;
            case "IX_Brands_Slug":
                fieldName = "Name";
                message = "Brand name is already registered";
                return true;
            case "IX_BrandPhoneNumbers_BrandId_NormalizedPhoneNumber":
                fieldName = "PhoneNumbers";
                message = "Phone numbers must be unique";
                return true;
            case "IX_BrandPhoneNumbers_BrandId_IsPrimary":
                fieldName = "PhoneNumbers";
                message = "Only one active primary phone number is allowed";
                return true;
            case "IX_BrandEmailAddresses_BrandId_NormalizedEmail":
                fieldName = "EmailAddresses";
                message = "Email addresses must be unique";
                return true;
            case "IX_BrandEmailAddresses_BrandId_IsPrimary":
                fieldName = "EmailAddresses";
                message = "Only one active primary email address is allowed";
                return true;
            case "IX_BrandAddresses_BrandId_IsPrimary":
                fieldName = "Addresses";
                message = "Only one active primary address is allowed";
                return true;
            case "IX_CategoryAttributes_CategoryId_ProductAttributeId":
                fieldName = "AttributeId";
                message = "Attribute is already linked to this category";
                return true;
            case "IX_CategoryTranslations_LanguageCode_Slug":
                fieldName = "Translations";
                message = "Category name already exists";
                return true;
            case "IX_CategoryTranslations_CategoryId_LanguageCode":
                fieldName = "Translations";
                message = "Translation languages must be unique";
                return true;
            case "IX_HomeCategories_CategoryId":
                fieldName = "CategoryId";
                message = "Category is already selected for home";
                return true;
            case "IX_ProductAttributes_NormalizedName":
                fieldName = "Translations";
                message = "Attribute name already exists";
                return true;
            case "IX_ProductAttributeTranslations_LanguageCode_Slug":
                fieldName = "Translations";
                message = "Attribute name already exists";
                return true;
            case "IX_ProductAttributeTranslations_ProductAttributeId_LanguageCode":
                fieldName = "Translations";
                message = "Translation languages must be unique";
                return true;
            case "IX_AttributeOptionTranslations_LanguageCode_Slug":
                fieldName = "Translations";
                message = "Option value already exists";
                return true;
            case "IX_AttributeOptionTranslations_AttributeOptionId_LanguageCode":
                fieldName = "Translations";
                message = "Translation languages must be unique";
                return true;
            case "IX_ProductTranslations_ProductId_LanguageCode":
                fieldName = "Translations";
                message = "Translation languages must be unique";
                return true;
            case "IX_ProductTranslations_LanguageCode_Slug":
                fieldName = "Translations";
                message = "Product title already exists";
                return true;
            case "IX_Products_BrandId_Sku":
                fieldName = "Sku";
                message = "This SKU already exists";
                return true;
            case "IX_ProductAttributeValues_ProductId_ProductAttributeId":
                fieldName = "AttributeValues";
                message = "Attribute values must be unique";
                return true;
            case "IX_ProductImages_ProductId_ImageId":
                fieldName = "Images";
                message = "Images must be unique";
                return true;
            case "IX_ProductImages_ProductId_IsPrimary":
                fieldName = "Images";
                message = "Only one primary image is allowed";
                return true;
            default:
                fieldName = string.Empty;
                message = string.Empty;
                return false;
        }
    }

    private static string? GetStringProperty(Exception exception, string propertyName) =>
        exception.GetType().GetProperty(propertyName)?.GetValue(exception) as string;

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
