using System.Net.Http.Json;
using api_conteo_sider_alys.Models;

namespace api_conteo_sider_alys.Clients.Sider;

public sealed class SiderClient : ISiderClient
{
    private readonly HttpClient _httpClient;
    private readonly string? _endpoint = Environment.GetEnvironmentVariable("SiderApiEndpoint");
    private readonly string? _apiKey = Environment.GetEnvironmentVariable("SiderApiKey");

    public SiderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SendBundleAsync(BundleResponse bundle, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
        {
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = JsonContent.Create(new
            {
                bundleCode = bundle.BundleCode,
                steelDiameter = bundle.SteelDiameter,
                itemCount = bundle.ItemCount,
                countStartedAt = bundle.CountStartedAt,
                countFinishedAt = bundle.CountFinishedAt,
                countTime = bundle.CountTime,
                bundleType = bundle.BundleType,
                videoPath = bundle.VideoPath
            })
        };

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            request.Headers.Add("x-api-key", _apiKey);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return true;
    }
}
