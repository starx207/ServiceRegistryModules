using Microsoft.Extensions.DependencyInjection;
using RegistryServices;

namespace ServiceRegistryModules.DefaultOptions.Tests;

public sealed class EmptyRegistry : AbstractRegistryModule {
    public static readonly EmptyRegistry Instance = new();
    public override void ConfigureServices(IServiceCollection services) { }
}

public sealed class TestService1Registry : AbstractRegistryModule {
    public override void ConfigureServices(IServiceCollection services)
        => services.AddTransient<ITestService1, TestService1>();
}

internal sealed class TestService1Registry_Internal : AbstractRegistryModule {
    public override void ConfigureServices(IServiceCollection services)
        => services.AddTransient<ITestService1, TestService1Alt>();
}

internal sealed class TestService2Registry_Wrapper {
    public sealed class TestService2Registry : AbstractRegistryModule {
        public override void ConfigureServices(IServiceCollection services)
            => services.AddTransient<ITestService2, TestService2>();
    }

    internal sealed class TestService2Registry_Internal : AbstractRegistryModule {
        public override void ConfigureServices(IServiceCollection services)
            => services.AddTransient<ITestService2, TestService2Alt>();
    }
}

public sealed class TestService3Registry_Wrapper {
    public sealed class TestService3Registry : AbstractRegistryModule {
        public override void ConfigureServices(IServiceCollection services)
            => services.AddTransient<ITestService3, TestService3>();
    }

    internal sealed class TestService3Registry_Internal : AbstractRegistryModule {
        public override void ConfigureServices(IServiceCollection services)
            => services.AddTransient<ITestService3, TestService3Alt>();
    }
}