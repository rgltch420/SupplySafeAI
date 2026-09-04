namespace SupplySafe.Api.Domain.Enums;

public enum OrderStatus
{
    ReceivedFromEmail,
    Validating,
    LinkedToShipment,
    AtRisk,
    ContingencyRecommended,
    Processing,
    Completed,
    Rejected
}
