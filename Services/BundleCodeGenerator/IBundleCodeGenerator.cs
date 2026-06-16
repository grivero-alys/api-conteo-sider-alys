namespace api_conteo_sider_alys.Services.BundleCodeGenerator;

public interface IBundleCodeGenerator
{
    string Generate(string companyCode, string siteCode, string cameraCode, string bundleType);
}
