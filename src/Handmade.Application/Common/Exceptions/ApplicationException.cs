namespace Handmade.Application.Common.Exceptions;

public class ApplicationException : Exception
{
    public string Title { get; } = "Something went wrong";

    public ApplicationException() : base("Application exception occurred.") { }
    public ApplicationException(string message) : base(message) { }
}