namespace Handmade.Application.Interfaces;

public interface ICurrentUser
{
    int? Id { get; }
    Guid? SessionId { get; }
}
