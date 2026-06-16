using api_conteo_sider_alys.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace api_conteo_sider_alys.Storage.BlobVideo;

public sealed class BlobVideoStorage : IBlobVideoStorage
{
    private readonly string? _connectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
    private readonly string _containerName = Environment.GetEnvironmentVariable("BlobStorageContainerName") ?? "bundle-videos";
    private readonly string _enterprise = Environment.GetEnvironmentVariable("Enterprise") ?? "sider";

    public async Task<string?> SaveAsync(
        UploadedVideo? video,
        string bundleCode,
        DateTimeOffset countedAt,
        CancellationToken cancellationToken)
    {
        if (video is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("BlobStorageConnectionString is required when a video payload is sent.");
        }

        var containerClient = new BlobContainerClient(_connectionString, _containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var fileName = $"{SanitizeSegment(bundleCode)}.mp4";
        var blobName = string.Join(
            "/",
            SanitizeSegment(_enterprise),
            countedAt.ToString("yyyy"),
            countedAt.ToString("MM"),
            countedAt.ToString("dd"),
            fileName);

        var blobClient = containerClient.GetBlobClient(blobName);
        video.Content.Position = 0;

        await blobClient.UploadAsync(
            video.Content,
            new BlobHttpHeaders
            {
                ContentType = string.IsNullOrWhiteSpace(video.ContentType) ? "video/mp4" : video.ContentType
            },
            cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    private static string SanitizeSegment(string value)
    {
        var sanitized = new string(value
            .Trim()
            .Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-')
            .ToArray());

        return string.Join("-", sanitized.Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
