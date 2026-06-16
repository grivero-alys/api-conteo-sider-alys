namespace api_conteo_sider_alys.Models;

public static class BundleTypes
{
    public const int IndividualId = 1;
    public const int GroupedId = 2;

    public const string Individual = "INDIVIDUAL";
    public const string Grouped = "AGRUPADO";

    public static int GetId(string bundleType)
    {
        return bundleType == Grouped ? GroupedId : IndividualId;
    }
}
