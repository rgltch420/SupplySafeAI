namespace SupplySafe.Api.Domain.Entities;

public class Notification
{
    public string Id { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public bool Sent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Channel { get; set; } = "email";
}
