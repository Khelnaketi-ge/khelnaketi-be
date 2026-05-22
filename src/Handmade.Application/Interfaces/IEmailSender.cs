namespace Handmade.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(
        string toName,
        string toAddress,
        string subject,
        string textContent,
        CancellationToken cancellationToken = default
    );
}
