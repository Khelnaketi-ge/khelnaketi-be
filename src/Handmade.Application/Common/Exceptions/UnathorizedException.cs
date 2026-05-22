namespace Handmade.Application.Common.Exceptions;

public class UnauthorizedException(Error error) : Exception(error.Text)
{
    public string? Code { get; } = error.Code;
}