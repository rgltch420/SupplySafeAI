namespace SupplySafe.Api.Application.DTOs;

public class IngestEmailOrderRequest
{
    public string From { get; set; } = "procurement@cliente-demo.com";
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
