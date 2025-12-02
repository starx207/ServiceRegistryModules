using System;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples2;
public class TestRegistry1 : AbstractRegistryModule {
    public override void ConfigureServices(IServiceCollection services)
        => services.AddTransient<Service>()
        .AddTransient<ITestService1, Service>();

    public class Service : ITestService1 { }

    private static void TestEventHandler(object sender, EventArgs e) {
        HandledEvents.HandledEventFor = sender;
        HandledEvents.HandledEventArgs = e;
    }

    private static void TestInvalidHandler() {
        // No-op
    }
}

public static class HandledEvents {
    public static object? HandledEventFor;
    public static EventArgs? HandledEventArgs;
}
