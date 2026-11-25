using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AbbContentEditor.Helpers
{
    /// <summary>
    /// SMTP configuration, bound from the "Smtp" section of appsettings.
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 25;
        /// <summary>None | Auto | SslOnConnect | StartTls | StartTlsWhenAvailable</summary>
        public SecureSocketOptions Security { get; set; } = SecureSocketOptions.None;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string FromEmail { get; set; } = "alexey@beliaeff.ru";
        public string FromName { get; set; } = "ABWords";
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string recipientEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }

    public class EmailKitSender : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailKitSender> _logger;

        public EmailKitSender(IOptions<SmtpSettings> settings, ILogger<EmailKitSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, _settings.Security, cancellationToken);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email '{Subject}' sent to {Email}", subject, recipientEmail);
        }
    }
}
