using Handmade.Application.Interfaces;
using Handmade.Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Handmade.Infrastructure.Services;

public sealed class EmailSender(SmtpOptions options) : IEmailSender
{
    public async Task SendEmailAsync(
        string toName, 
        string toAddress, 
        string subject,
        string textContent, 
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        
        message.From.Add(new MailboxAddress(options.FromName, options.FromAddress));
        message.To.Add(new MailboxAddress(toName, toAddress));
        message.Subject = subject;
        
        message.Body = new TextPart("plain")
        {
            Text = textContent
        };
        
        using var client = new SmtpClient();
                
        await client.ConnectAsync(
            options.Host,
            options.Port, 
            options.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None,
            cancellationToken
        );
        
        await client.AuthenticateAsync(
            options.UserName,
            options.Password,
            cancellationToken
        );

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
