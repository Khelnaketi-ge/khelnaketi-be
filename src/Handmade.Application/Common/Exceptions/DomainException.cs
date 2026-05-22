namespace Handmade.Application.Common.Exceptions;

public class DomainException : Exception
{
    public string Title { get; } = "An error occurred";
    public string? Code { get; }
    
    public DomainException() : base("Domain exception occurred.") { }
    public DomainException(string message) : base(message) { }
    public DomainException(string message, string? code) : base(message) { Code = code; }
    public DomainException(Error error) : base(error.Text) { Code = error.Code; }
}