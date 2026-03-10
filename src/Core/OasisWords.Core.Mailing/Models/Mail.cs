namespace OasisWords.Core.Mailing.Models;

public class Mail
{
    public string ToEmail { get; set; } = string.Empty;
    public string ToFullName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string TextBody { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public List<MailAttachment> Attachments { get; set; } = new();
}

public class MailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}
