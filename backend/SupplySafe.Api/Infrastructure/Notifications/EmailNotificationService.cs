using System.Net;
using System.Net.Mail;
using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Interfaces;
using SupplySafe.Api.Domain.Entities;
using SupplySafe.Api.Infrastructure.Persistence;

namespace SupplySafe.Api.Infrastructure.Notifications;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly InMemoryStore _store;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        InMemoryStore store,
        IConfiguration configuration,
        ILogger<EmailNotificationService> logger)
    {
        _store = store;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SendEmailResultDto> SendAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var messageId = _store.NextMessageId();
        var recipient = string.IsNullOrWhiteSpace(request.Recipient)
            ? "operations@supplysafe.demo"
            : request.Recipient.Trim();
        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? "SUPPLYSAFE - Critical Supply Chain Risk"
            : request.Subject.Trim();
        var body = string.IsNullOrWhiteSpace(request.Body)
            ? "SupplySafe AI alert — review contingency actions."
            : request.Body;

        var sentViaSmtp = false;
        var host = _configuration["Smtp:Host"];
        if (!string.IsNullOrWhiteSpace(host))
        {
            try
            {
                sentViaSmtp = await TrySendSmtpAsync(host, recipient, subject, body, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMTP send failed — simulating email delivery");
            }
        }

        if (!sentViaSmtp)
        {
            _logger.LogInformation(
                "Simulated email {MessageId} to {Recipient} | Subject: {Subject}\n{Body}",
                messageId, recipient, subject, body);
        }

        _store.Notifications.Add(new Notification
        {
            Id = $"NTF-{messageId}",
            Recipient = recipient,
            Subject = subject,
            Body = body,
            MessageId = messageId,
            Sent = true,
            CreatedAt = DateTime.UtcNow,
            Channel = "email"
        });

        return new SendEmailResultDto
        {
            Success = true,
            Recipient = recipient,
            Subject = subject,
            MessageId = messageId
        };
    }

    public Task<SendEmailResultDto> SendCriticalIncidentAlertAsync(
        Incident incident,
        Shipment? shipment,
        RiskAnalysisResultDto? analysis,
        CancellationToken cancellationToken = default)
    {
        var shipmentId = shipment?.Id ?? incident.ShipmentId ?? "N/A";
        var product = shipment?.Product ?? "Critical product";
        var route = shipment?.Route ?? "N/A";
        var riskScore = analysis?.RiskScore ?? incident.RiskScore;
        var delay = analysis?.DelayDays ?? shipment?.DelayDays ?? 0;
        var coverage = analysis?.InventoryCoverageDays ?? 0;
        var shortage = analysis?.ProjectedShortageUnits ?? incident.AffectedUnits;

        var recommendations = (analysis?.Recommendations?.Count > 0
                ? analysis.Recommendations
                : incident.Recommendations)
            .Take(3)
            .Select(r => $"- {r}")
            .ToList();

        if (recommendations.Count == 0)
        {
            recommendations =
            [
                "- Divert shipment",
                "- Activate secondary supplier",
                "- Prioritize inventory"
            ];
        }

        var body =
            $"""
            CRITICAL SUPPLY CHAIN ALERT

            Shipment: {shipmentId}
            Product: {product}
            Route: {route}
            Risk Score: {riskScore}/100
            Expected delay: +{delay} days
            Inventory coverage: {coverage:0.0} days
            Projected shortage: {shortage:N0} units

            RECOMMENDED ACTION
            {string.Join('\n', recommendations)}

            SupplySafe AI recommends immediate intervention.
            """;

        return SendAsync(new SendEmailRequest
        {
            Recipient = "operations@supplysafe.demo",
            Subject = "🚨 SUPPLYSAFE - Critical Supply Chain Risk",
            Body = body,
            IncidentId = incident.Id
        }, cancellationToken);
    }

    private async Task<bool> TrySendSmtpAsync(
        string host,
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var from = _configuration["Smtp:From"] ?? "alerts@supplysafe.demo";
        var enableSsl = !string.Equals(_configuration["Smtp:EnableSsl"], "false", StringComparison.OrdinalIgnoreCase);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        using var message = new MailMessage(from, recipient, subject, body);
        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("SMTP email delivered to {Recipient}", recipient);
        return true;
    }
}
