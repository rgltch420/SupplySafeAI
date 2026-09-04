using System.Globalization;
using System.Text.Json;
using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Interfaces;

namespace SupplySafe.Api.Infrastructure.Fx;

/// <summary>
/// TRM / FX helper for the demo.
/// Tries open-data TRM (USD/COP) with short timeout; always falls back to burned rates
/// so the hackathon demo never depends on external network.
/// </summary>
public class TrmFxService : ITrmFxService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TrmFxService> _logger;

    // Burned demo basket (explainable in pitch): TRM-like USD + reference EUR/CNY
    private static readonly TrmSnapshotDto Burned = new()
    {
        AsOf = new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc),
        Source = "burned-demo-fallback",
        UsdCop = 4125.87m,
        EurCop = 4520.40m,
        CnyCop = 575.20m,
        UsdEur = 0.9128m,
        UsdCny = 7.1720m
    };

    public TrmFxService(IHttpClientFactory httpClientFactory, ILogger<TrmFxService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<TrmSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            var client = _httpClientFactory.CreateClient("trm");
            // datos.gov.co open dataset commonly used for TRM historical/current values
            using var response = await client.GetAsync(
                "resource/32sa-8pi3.json?$limit=1&$order=vigenciadesde DESC",
                timeout.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TRM open-data HTTP {Status} — using burned rates", (int)response.StatusCode);
                return CloneBurned("burned-demo-fallback");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: timeout.Token);
            if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
                return CloneBurned("burned-demo-fallback");

            var row = doc.RootElement[0];
            if (!TryReadDecimal(row, "valor", out var usdCop))
                return CloneBurned("burned-demo-fallback");

            // Keep EUR/CNY as derived/reference from burned cross so demo stays multi-currency
            var snapshot = CloneBurned("datos.gov.co + burned EUR/CNY refs");
            snapshot.UsdCop = Math.Round(usdCop, 2);
            snapshot.AsOf = DateTime.UtcNow;
            snapshot.EurCop = Math.Round(snapshot.UsdCop / snapshot.UsdEur, 2);
            snapshot.CnyCop = Math.Round(snapshot.UsdCop / snapshot.UsdCny, 2);
            return snapshot;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || cancellationToken.IsCancellationRequested == false)
        {
            _logger.LogWarning(ex, "TRM fetch failed — using burned rates");
            return CloneBurned("burned-demo-fallback");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("TRM fetch timed out — using burned rates");
            return CloneBurned("burned-demo-fallback");
        }
    }

    public decimal ToCop(decimal amountUsd, TrmSnapshotDto snapshot) =>
        Math.Round(amountUsd * snapshot.UsdCop, 0);

    private static TrmSnapshotDto CloneBurned(string source) => new()
    {
        AsOf = Burned.AsOf,
        Source = source,
        Note = Burned.Note,
        UsdCop = Burned.UsdCop,
        EurCop = Burned.EurCop,
        CnyCop = Burned.CnyCop,
        UsdEur = Burned.UsdEur,
        UsdCny = Burned.UsdCny
    };

    private static bool TryReadDecimal(JsonElement row, string name, out decimal value)
    {
        value = 0;
        if (!row.TryGetProperty(name, out var prop))
            return false;

        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out value))
            return true;

        if (prop.ValueKind == JsonValueKind.String &&
            decimal.TryParse(prop.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return true;

        return false;
    }
}
