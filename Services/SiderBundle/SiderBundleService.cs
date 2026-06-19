using api_conteo_sider_alys.Clients.Sider;
using api_conteo_sider_alys.Models;
using api_conteo_sider_alys.Repositories.Bundle;
using api_conteo_sider_alys.Services.BundleCodeGenerator;
using api_conteo_sider_alys.Storage.BlobVideo;

namespace api_conteo_sider_alys.Services.SiderBundle;

public sealed class SiderBundleService : ISiderBundleService
{
    private readonly string _enterprise = Environment.GetEnvironmentVariable("Enterprise") ?? "sider";
    private readonly IBlobVideoStorage _blobVideoStorage;
    private readonly IBundleCodeGenerator _bundleCodeGenerator;
    private readonly IBundleRepository _bundleRepository;
    private readonly ISiderClient _siderClient;

    public SiderBundleService(
        IBlobVideoStorage blobVideoStorage,
        IBundleCodeGenerator bundleCodeGenerator,
        IBundleRepository bundleRepository,
        ISiderClient siderClient)
    {
        _blobVideoStorage = blobVideoStorage;
        _bundleCodeGenerator = bundleCodeGenerator;
        _bundleRepository = bundleRepository;
        _siderClient = siderClient;
    }

    public async Task<BundleResponse> CreateAsync(BundleCreationInput input, CancellationToken cancellationToken)
    {
        var bundleCode = _bundleCodeGenerator.Generate(
            _enterprise,
            input.Headquarter,
            input.Camera,
            input.BundleType);

        var videoPath = await _blobVideoStorage.SaveAsync(
            input.Video,
            bundleCode,
            cancellationToken);

        var bundleId = await _bundleRepository.CreateAsync(input, bundleCode, videoPath, cancellationToken);

        var response = new BundleResponse
        {
            BundleId = bundleId,
            BundleCode = bundleCode,
            BundleType = input.BundleType,
            SteelDiameter = input.SteelDiameter,
            ItemCount = input.ItemCount,
            CountStartedAt = input.CountStartedAt,
            CountFinishedAt = input.CountFinishedAt,
            CountTime = input.CountTime,
            VideoPath = videoPath
        };

        var sentToSider = await _siderClient.SendBundleAsync(response, cancellationToken);
        return response.WithSentToSider(sentToSider);
    }
}

file static class BundleResponseExtensions
{
    public static BundleResponse WithSentToSider(this BundleResponse response, bool sentToSider)
    {
        return new BundleResponse
        {
            BundleId = response.BundleId,
            BundleCode = response.BundleCode,
            BundleType = response.BundleType,
            SteelDiameter = response.SteelDiameter,
            ItemCount = response.ItemCount,
            CountStartedAt = response.CountStartedAt,
            CountFinishedAt = response.CountFinishedAt,
            CountTime = response.CountTime,
            VideoPath = response.VideoPath,
            SentToSider = sentToSider
        };
    }
}
