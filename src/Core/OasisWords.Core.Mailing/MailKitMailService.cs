using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using OasisWords.Core.Mailing.Models;

namespace OasisWords.Core.Mailing;

public interface IMailService
{
    Task SendEmailAsync(Mail mail, CancellationToken cancellationToken = default);
}

public class MailKitMailService : IMailService
{
    private readonly MailSettings _mailSettings;

    public MailKitMailService(MailSettings mailSettings)
    {
        _mailSettings = mailSettings;
    }

    public async Task SendEmailAsync(Mail mail, CancellationToken cancellationToken = default)
    {
        MimeMessage message = new();
        message.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
        message.To.Add(new MailboxAddress(mail.ToFullName, mail.ToEmail));
        message.Subject = mail.Subject;

        BodyBuilder bodyBuilder = new()
        {
            HtmlBody = mail.HtmlBody,
            TextBody = mail.TextBody
        };

        foreach (MailAttachment attachment in mail.Attachments)
        {
            bodyBuilder.Attachments.Add(
                attachment.FileName,
                attachment.Data,
                ContentType.Parse(attachment.ContentType));
        }

        message.Body = bodyBuilder.ToMessageBody();

        using SmtpClient client = new();
        await client.ConnectAsync(
            _mailSettings.Server,
            _mailSettings.Port,
            _mailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
            cancellationToken);

        await client.AuthenticateAsync(
            _mailSettings.UserName,
            _mailSettings.Password,
            cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
