namespace api_conteo_sider_alys.Models;

public sealed class BundleCreationInput
{
    public required string BundleType { get; init; }
    public required string Headquarter { get; init; }
    public required string Camera { get; init; }
    public DateTimeOffset CountedAt { get; init; }
    public required string SteelDiameter { get; init; }
    public int ItemCount { get; init; }
    public UploadedVideo? Video { get; init; }

    public string SiteCode => Headquarter;
    public string CameraCode => Camera;
    public string RebarDiameter => SteelDiameter;
}
