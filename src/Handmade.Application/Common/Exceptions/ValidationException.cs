using FluentValidation.Results;

namespace Handmade.Application.Common.Exceptions;

public class ValidationException : Exception
{
    private ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string propertyName, string error) : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            [propertyName] = [error]
        };
    }

    public IDictionary<string, string[]> Errors { get; }
}
