using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Interfaces;
using SupplySafe.Api.Domain.Entities;
using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Infrastructure.AI;

/// <summary>
/// Grok (xAI) analyzer with hard timeout and local heuristic fallback.
/// Demo never blocks on external AI availability.
/// </summary>
public class GrokRiskAnalyzer : IAiRiskAnalyzer
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISupplyRiskEngine _riskEngine;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GrokRiskAnalyzer> _logger;

    private static readonly TimeSpan ExternalTimeout = TimeSpan.FromSeconds(3);

    private static readonly string[] DefaultRecommendations =
    [
        "Divert shipment to alternate route",
        "Activate secondary supplier",
        "Prioritize existing inventory"
    ];

    public GrokRiskAnalyzer(
        IHttpClientFactory httpClientFactory,
        ISupplyRiskEngine riskEngine,
        IConfiguration configuration,
        ILogger<GrokRiskAnalyzer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _riskEngine = riskEngine;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RiskAnalysisResultDto> AnalyzeAsync(
        Shipment shipment,
        InventoryItem? inventory,
        IReadOnlyList<SupplyRisk> relatedRisks,
        CancellationToken cancellationToken = default)
    {
        var apiKey = Environment.GetEnvironmentVariable("XAI_API_KEY")
                     ?? _configuration["XAI:ApiKey"];

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(ExternalTimeout);

                var grokResult = await TryGrokAsync(apiKey, shipment, inventory, relatedRisks, timeoutCts.Token);
                if (grokResult is not null)
                    return grokResult;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Grok call timed out after {Timeout}s — using local fallback", ExternalTimeout.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Grok call failed — using local fallback");
            }
        }
        else
        {
            _logger.LogInformation("XAI_API_KEY not set — using local risk fallback");
        }

        return BuildFallback(shipment, inventory, relatedRisks);
    }

    private async Task<RiskAnalysisResultDto?> TryGrokAsync(
        string apiKey,
        Shipment shipment,
        InventoryItem? inventory,
        IReadOnlyList<SupplyRisk> relatedRisks,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("xai");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var prompt = BuildPrompt(shipment, inventory, relatedRisks);
        var payload = new
        {
            model = _configuration["XAI:Model"] ?? "grok-2-latest",
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content =
                        "You are SupplySafe AI risk analyst. Reply ONLY with compact JSON keys: " +
                        "riskScore (0-100 int), severity (LOW|MEDIUM|HIGH|CRITICAL), delayDays (int), " +
                        "inventoryCoverageDays (number), projectedShortageUnits (int), estimatedFinancialImpact (number), " +
                        "predictedStockout (bool), confidence (0-100 int), recommendations (string array max 5)."
                },
                new { role = "user", content = prompt }
            }
        };

        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/v1/chat/completions", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Grok HTTP {Status}", (int)response.StatusCode);
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var message = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(message))
            return null;

        var json = ExtractJson(message);
        var parsed = JsonSerializer.Deserialize<GrokRiskPayload>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (parsed is null)
            return null;

        return new RiskAnalysisResultDto
        {
            ShipmentId = shipment.Id,
            RiskScore = Math.Clamp(parsed.RiskScore, 0, 100),
            Severity = (parsed.Severity ?? _riskEngine.ToSeverity(parsed.RiskScore).ToString()).ToUpperInvariant(),
            DelayDays = parsed.DelayDays,
            InventoryCoverageDays = parsed.InventoryCoverageDays,
            ProjectedShortageUnits = parsed.ProjectedShortageUnits,
            EstimatedFinancialImpact = parsed.EstimatedFinancialImpact,
            PredictedStockout = parsed.PredictedStockout,
            Confidence = Math.Clamp(parsed.Confidence, 0, 100),
            Recommendations = parsed.Recommendations is { Count: > 0 }
                ? parsed.Recommendations
                : [.. DefaultRecommendations]
        };
    }

    private RiskAnalysisResultDto BuildFallback(
        Shipment shipment,
        InventoryItem? inventory,
        IReadOnlyList<SupplyRisk> relatedRisks)
    {
        // Canonical demo numbers for SHP-2048
        if (shipment.Id.Equals("SHP-2048", StringComparison.OrdinalIgnoreCase))
        {
            return new RiskAnalysisResultDto
            {
                ShipmentId = "SHP-2048",
                RiskScore = 87,
                Severity = "CRITICAL",
                DelayDays = 6,
                InventoryCoverageDays = 7.1,
                ProjectedShortageUnits = 13200,
                EstimatedFinancialImpact = 84600m,
                PredictedStockout = true,
                Confidence = 91,
                Recommendations = [.. DefaultRecommendations]
            };
        }

        var coverage = inventory?.CoverageDays ?? 14;
        var weather = relatedRisks.Where(r => r.Type == RiskType.Weather).Select(r => r.Severity).DefaultIfEmpty(RiskLevel.Low).Max();
        var port = relatedRisks.Where(r => r.Type == RiskType.PortCongestion).Select(r => r.Severity).DefaultIfEmpty(RiskLevel.Low).Max();
        var geo = relatedRisks.Where(r => r.Type == RiskType.Geopolitical).Select(r => r.Severity).DefaultIfEmpty(RiskLevel.Low).Max();

        var score = _riskEngine.CalculateRiskScore(shipment.DelayDays, coverage, weather, port, geo);
        var severity = _riskEngine.ToSeverity(score);

        var shortage = 0;
        var stockout = false;
        if (inventory is not null && shipment.DelayDays > 0 && coverage < shipment.DelayDays + inventory.CoverageDays)
        {
            var gapDays = Math.Max(0, shipment.DelayDays - coverage);
            shortage = (int)Math.Round(gapDays * inventory.DailyConsumption);
            stockout = shortage > 0 || coverage < shipment.DelayDays;
        }

        var unitImpact = inventory is not null && inventory.Quantity > 0
            ? shipment.Value / Math.Max(shipment.Units, 1)
            : 4m;

        var financialImpact = Math.Round(shortage * unitImpact, 0);
        if (financialImpact <= 0 && score >= 60)
            financialImpact = Math.Round(shipment.Value * 0.08m, 0);

        return new RiskAnalysisResultDto
        {
            ShipmentId = shipment.Id,
            RiskScore = score,
            Severity = severity.ToString().ToUpperInvariant(),
            DelayDays = shipment.DelayDays,
            InventoryCoverageDays = Math.Round(coverage, 1),
            ProjectedShortageUnits = shortage,
            EstimatedFinancialImpact = financialImpact,
            PredictedStockout = stockout || score >= 80,
            Confidence = score >= 70 ? 88 : 76,
            Recommendations = [.. DefaultRecommendations]
        };
    }

    private static string BuildPrompt(
        Shipment shipment,
        InventoryItem? inventory,
        IReadOnlyList<SupplyRisk> relatedRisks)
    {
        var risks = string.Join("; ", relatedRisks.Select(r => $"{r.Type}:{r.Severity}({r.Score}) {r.Title}"));
        return
            $"Analyze supply risk for shipment {shipment.Id}. Product={shipment.Product}. " +
            $"Route={shipment.Route}. DelayDays={shipment.DelayDays}. CurrentRisk={shipment.RiskScore}. " +
            $"Units={shipment.Units}. Value={shipment.Value}. " +
            $"InventoryCoverageDays={inventory?.CoverageDays}. DailyConsumption={inventory?.DailyConsumption}. " +
            $"RelatedRisks=[{risks}]. Prefer conservative operational recommendations.";
    }

    private static string ExtractJson(string message)
    {
        var trimmed = message.Trim();
        if (trimmed.StartsWith("```"))
        {
            var start = trimmed.IndexOf('{');
            var end = trimmed.LastIndexOf('}');
            if (start >= 0 && end > start)
                return trimmed[start..(end + 1)];
        }

        var i = trimmed.IndexOf('{');
        var j = trimmed.LastIndexOf('}');
        if (i >= 0 && j > i)
            return trimmed[i..(j + 1)];

        return trimmed;
    }

    private sealed class GrokRiskPayload
    {
        public int RiskScore { get; set; }
        public string? Severity { get; set; }
        public int DelayDays { get; set; }
        public double InventoryCoverageDays { get; set; }
        public int ProjectedShortageUnits { get; set; }
        public decimal EstimatedFinancialImpact { get; set; }
        public bool PredictedStockout { get; set; }
        public int Confidence { get; set; }
        public List<string>? Recommendations { get; set; }
    }
}
