namespace api_conteo_sider_alys.Models;

public sealed class UploadedVideo
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required MemoryStream Content { get; init; }
}
