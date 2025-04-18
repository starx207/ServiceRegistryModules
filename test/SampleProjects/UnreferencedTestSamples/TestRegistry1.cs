using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace UnreferencedTestSamples;
public class TestRegistry1 : AbstractRegistryModule {
    public override void ConfigureServices(IServiceCollection services)
        => services.AddTransient<ITestService1, Service>();

    public class Service : ITestService1 { }

    public static void OnConfigureServicesHandler(object sender, IServiceCollection services)
        => services.AddSingleton<Service>()
        .AddSingleton(_ => new ConfigurableService($"From {nameof(UnreferencedTestSamples)}"));

}
