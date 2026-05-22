namespace Handmade.Infrastructure.Options;

public sealed class SmtpOptions
{
    public string FromName { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public bool UseDefaultCredentials { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}