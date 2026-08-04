using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AbbContentEditor.Helpers
{
    public class EmailKitSender
    {
        public async Task SendEmailAsync(string recipientEmail, string text)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sender Name", "alexey@beliaeff.ru"));
            message.To.Add(new MailboxAddress("Recipient Name", recipientEmail));
            message.Subject = "Test Email from .NET";

            message.Body = new TextPart("html")
            {
                Text = text
            };
            
            using var client = new SmtpClient();

            // Option A: If connecting to a local MTA (Postfix/Sendmail on localhost)
            await client.ConnectAsync("127.0.0.1", 25, SecureSocketOptions.None);

            // Option B: If connecting to an external SMTP server (e.g., port 587 with STARTTLS)
            // await client.ConnectAsync("smtp.yourprovider.com", 587, SecureSocketOptions.StartTls);
            // await client.AuthenticateAsync("smtp_username", "smtp_password");

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
