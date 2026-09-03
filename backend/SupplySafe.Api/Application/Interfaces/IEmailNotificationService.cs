using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Domain.Entities;

namespace SupplySafe.Api.Application.Interfaces;

public interface IEmailNotificationService
{
    Task<SendEmailResultDto> SendAsync(SendEmailRequest request, CancellationToken cancellationToken = default);
    Task<SendEmailResultDto> SendCriticalIncidentAlertAsync(
        Incident incident,
        Shipment? shipment,
        RiskAnalysisResultDto? analysis,
        CancellationToken cancellationToken = default);
}
