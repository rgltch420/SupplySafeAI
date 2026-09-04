namespace SupplySafe.Api.Application.DTOs;

public class TrmSnapshotDto
{
    public DateTime AsOf { get; set; }
    public string Source { get; set; } = "burned-demo";
    public string Note { get; set; } =
        "TRM oficial (Colombia) = USD/COP certificada estilo Superintendencia/BanRep. EUR y CNY son tasas de referencia para la demo de supply chain Asia–Europa–Colombia.";

    /// <summary>Official-style TRM: Colombian pesos per 1 USD.</summary>
    public decimal UsdCop { get; set; }

    /// <summary>Reference: COP per 1 EUR.</summary>
    public decimal EurCop { get; set; }

    /// <summary>Reference: COP per 1 CNY (yuan).</summary>
    public decimal CnyCop { get; set; }

    public decimal UsdEur { get; set; }
    public decimal UsdCny { get; set; }
}
