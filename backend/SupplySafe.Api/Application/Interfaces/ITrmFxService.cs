using SupplySafe.Api.Application.DTOs;

namespace SupplySafe.Api.Application.Interfaces;

public interface ITrmFxService
{
    Task<TrmSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken = default);
    decimal ToCop(decimal amountUsd, TrmSnapshotDto snapshot);
}
