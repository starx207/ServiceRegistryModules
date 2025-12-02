using System;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples2;
public class TestRegistry2 : AbstractRegistryModule {
    public event EventHandler<IServiceCollection>? OnConfigure;

    public override void ConfigureServices(IServiceCollection services) {
        OnConfigure?.Invoke(this, services);
        services.AddTransient<ITestService1, Service>();
    }

    public class Service : ITestService1 { }
}
