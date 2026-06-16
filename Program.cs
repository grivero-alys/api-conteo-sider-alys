using api_conteo_sider_alys.Clients.Sider;
using api_conteo_sider_alys.Repositories.Bundle;
using api_conteo_sider_alys.Services.BundleCodeGenerator;
using api_conteo_sider_alys.Services.SiderBundle;
using api_conteo_sider_alys.Storage.BlobVideo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IBlobVideoStorage, BlobVideoStorage>();
        services.AddSingleton<IBundleCodeGenerator, BundleCodeGenerator>();
        services.AddSingleton<IBundleRepository, BundleRepository>();
        services.AddHttpClient<ISiderClient, SiderClient>();
        services.AddSingleton<ISiderBundleService, SiderBundleService>();
    })
    .Build();

await host.RunAsync();
