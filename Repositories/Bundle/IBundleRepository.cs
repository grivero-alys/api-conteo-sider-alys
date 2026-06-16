using api_conteo_sider_alys.Models;

namespace api_conteo_sider_alys.Repositories.Bundle;

public interface IBundleRepository
{
    Task<int> CreateAsync(
        BundleCreationInput input,
        string bundleCode,
        string? videoPath,
        CancellationToken cancellationToken);
}
