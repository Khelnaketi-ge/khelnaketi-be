namespace Handmade.Application.Interfaces;

public interface ICurrentRequest
{
    string? IpAddress { get; }
    string? UserAgent { get; }
}
