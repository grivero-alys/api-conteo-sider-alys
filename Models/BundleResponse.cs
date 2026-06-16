namespace api_conteo_sider_alys.Models;

public sealed class BundleResponse
{
    public int BundleId { get; init; }
    public required string BundleCode { get; init; }
    public required string BundleType { get; init; }
    public required string SteelDiameter { get; init; }
    public int ItemCount { get; init; }
    public DateTimeOffset CountedAt { get; init; }
    public string? VideoPath { get; init; }
    public bool SentToSider { get; init; }
}
