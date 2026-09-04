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
        var defaultRecipient = Environment.GetEnvironmentVariable("SUPPLYSAFE_NOTIFY_EMAIL")
                               ?? _configuration["Demo:OperationsEmail"]
                               ?? "operations@supplysafe.demo";

        var messageId = _store.NextMessageId();
        var recipient = ResolveRecipient(request.Recipient, defaultRecipient);
        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? "SUPPLYSAFE - Critical Supply Chain Risk"
            : request.Subject.Trim();
        var body = string.IsNullOrWhiteSpace(request.Body)
            ? "SupplySafe AI alert — review contingency actions."
            : request.Body;

        var sentViaSmtp = false;
        // Prefer env so App Password never needs to live in committed files
        var host = Environment.GetEnvironmentVariable("Smtp__Host")
                   ?? _configuration["Smtp:Host"];
        var smtpUser = Environment.GetEnvironmentVariable("Smtp__Username")
                       ?? _configuration["Smtp:Username"];
        var smtpPass = Environment.GetEnvironmentVariable("SUPPLYSAFE_SMTP_PASSWORD")
                       ?? Environment.GetEnvironmentVariable("Smtp__Password")
                       ?? _configuration["Smtp:Password"];

        if (IsPlaceholderSmtpPassword(smtpPass))
        {
            _logger.LogWarning(
                "SUPPLYSAFE_SMTP_PASSWORD looks like a placeholder, not a real Gmail App Password. Simulating send to {Recipient}.",
                recipient);
            smtpPass = null;
        }

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(smtpPass))
        {
            try
            {
                sentViaSmtp = await TrySendSmtpAsync(host, recipient, subject, body, smtpUser, smtpPass, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMTP send failed — simulating email delivery");
            }
        }
        else if (!string.IsNullOrWhiteSpace(host) && string.IsNullOrWhiteSpace(smtpPass))
        {
            _logger.LogWarning(
                "SMTP host configured but no password. Set SUPPLYSAFE_SMTP_PASSWORD (Gmail App Password). Simulating send.");
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
            Recipient = Environment.GetEnvironmentVariable("SUPPLYSAFE_NOTIFY_EMAIL")
                        ?? _configuration["Demo:OperationsEmail"]
                        ?? "operations@supplysafe.demo",
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
        string? username,
        string? password,
        CancellationToken cancellationToken)
    {
        var port = int.TryParse(
            Environment.GetEnvironmentVariable("Smtp__Port") ?? _configuration["Smtp:Port"],
            out var p) ? p : 587;
        var from = Environment.GetEnvironmentVariable("Smtp__From")
                   ?? _configuration["Smtp:From"]
                   ?? username
                   ?? "alerts@supplysafe.demo";
        var enableSsl = !string.Equals(
            Environment.GetEnvironmentVariable("Smtp__EnableSsl") ?? _configuration["Smtp:EnableSsl"],
            "false",
            StringComparison.OrdinalIgnoreCase);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        using var message = new System.Net.Mail.MailMessage(from, recipient, subject, body);
        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("SMTP email delivered to {Recipient}", recipient);
        return true;
    }

    private static string ResolveRecipient(string? requested, string defaultRecipient)
    {
        if (string.IsNullOrWhiteSpace(requested))
            return defaultRecipient;

        var trimmed = requested.Trim();
        // Legacy demo address should not override configured Ops inbox
        if (trimmed.Equals("operations@supplysafe.demo", StringComparison.OrdinalIgnoreCase))
            return defaultRecipient;

        return trimmed;
    }

    private static bool IsPlaceholderSmtpPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var p = password.Trim();
        return p.Contains("las_16_letras", StringComparison.OrdinalIgnoreCase)
               || p.Contains("pega_aqui", StringComparison.OrdinalIgnoreCase)
               || p.Equals("changeme", StringComparison.OrdinalIgnoreCase)
               || p.Equals("password", StringComparison.OrdinalIgnoreCase);
    }
}
