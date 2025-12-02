using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples1;
public class TestRegistry1 : AbstractRegistryModule {
    public override void ConfigureServices(IServiceCollection services)
        => services.AddTransient<Service>()
        .AddTransient<ITestService1, Service>();

    public class Service : ITestService1 { }
}
