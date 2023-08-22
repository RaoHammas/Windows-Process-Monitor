namespace MonitorApp.Domain.Models;

public class EmailMetadata
{
    public string ToAddress { get; set; }
    public string Subject { get; set; }
    public string? Body { get; set; }
    public string? AttachmentPath { get; set; }

    public EmailMetadata()
    {
    }
}