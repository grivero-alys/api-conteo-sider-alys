using api_conteo_sider_alys.Models;

namespace api_conteo_sider_alys.Services.SiderBundle;

public interface ISiderBundleService
{
    Task<BundleResponse> CreateAsync(BundleCreationInput input, CancellationToken cancellationToken);
}
