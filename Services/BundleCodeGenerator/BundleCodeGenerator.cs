namespace api_conteo_sider_alys.Services.BundleCodeGenerator;

public sealed class BundleCodeGenerator : IBundleCodeGenerator
{
    private const string LimaTimeZoneId = "America/Lima";

    public string Generate(string companyCode, string siteCode, string cameraCode, string bundleType)
    {
        var limaNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, GetLimaTimeZone());

        return string.Join(
            "-",
            Normalize(companyCode),
            Normalize(siteCode),
            Normalize(cameraCode),
            Normalize(bundleType),
            limaNow.ToString("yyyyMMddHHmmss"));
    }

    private static TimeZoneInfo GetLimaTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(LimaTimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        }
    }

    private static string Normalize(string value)
    {
        var normalized = new string(value
            .Trim()
            .ToUpperInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());

        return string.Join("-", normalized.Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
