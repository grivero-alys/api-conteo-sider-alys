using api_conteo_sider_alys.Models;

namespace api_conteo_sider_alys.Storage.BlobVideo;

public interface IBlobVideoStorage
{
    Task<string?> SaveAsync(
        UploadedVideo? video,
        string bundleCode,
        DateTimeOffset countedAt,
        CancellationToken cancellationToken);
}
