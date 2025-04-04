using System;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples1;
public class TestRegistry2 : AbstractRegistryModule {
    public static readonly EventArgs EventArgs = new EventArgs();

    public event EventHandler<EventArgs>? MyPublicEvent;

    public override void ConfigureServices(IServiceCollection services) {
        services.AddSingleton<Service>()
            .AddTransient<ITestService1, Service>();
        MyPublicEvent?.Invoke(this, EventArgs);
    }

    public class Service : ITestService1 { }
}
