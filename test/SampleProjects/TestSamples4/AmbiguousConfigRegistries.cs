using System;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples4;

public class AmbiguousPropertyRegistry : AbstractRegistryModule {
    public string AmbiguousConfig { get; set; } = string.Empty;

    public override void ConfigureServices(IServiceCollection services)
        => services.AddTransient(_ => new ConfigurableService(AmbiguousConfig));
}

public class AmbiguousEventRegistry : AbstractRegistryModule {
    public static readonly EventArgs EventArgs = new EventArgs();
    public event EventHandler<EventArgs>? AmbiguousConfig;

    public override void ConfigureServices(IServiceCollection services)
        => AmbiguousConfig?.Invoke(this, EventArgs);

}
