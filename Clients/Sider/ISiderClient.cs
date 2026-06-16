using api_conteo_sider_alys.Models;

namespace api_conteo_sider_alys.Clients.Sider;

public interface ISiderClient
{
    Task<bool> SendBundleAsync(BundleResponse bundle, CancellationToken cancellationToken);
}
