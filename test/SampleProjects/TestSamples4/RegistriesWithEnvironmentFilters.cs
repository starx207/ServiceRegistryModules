using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples4;

public class DevelopmentOnlyRegistry : IRegistryModule {
    public static IReadOnlyCollection<string> EnvironmentFilters => ["development"];
    public static string FilterDisplay => string.Join(" or ", EnvironmentFilters);

    public IReadOnlyCollection<string> TargetEnvironments => EnvironmentFilters;

    public void ConfigureServices(IServiceCollection services)
        => services.AddTransient(_ => new ConfigurableService(FilterDisplay));
}

public class StagingOnlyRegistry : IRegistryModule {
    public static IReadOnlyCollection<string> EnvironmentFilters => ["staging"];
    public static string FilterDisplay => string.Join(" or ", EnvironmentFilters);

    public IReadOnlyCollection<string> TargetEnvironments => EnvironmentFilters;

    public void ConfigureServices(IServiceCollection services)
        => services.AddTransient(_ => new ConfigurableService(FilterDisplay));
}

public class ProductionOnlyRegistry : AbstractRegistryModule {
    public static IReadOnlyCollection<string> EnvironmentFilters => ["production"];
    public static string FilterDisplay => string.Join(" or ", EnvironmentFilters);

    public override IReadOnlyCollection<string> TargetEnvironments => EnvironmentFilters;

    public override void ConfigureServices(IServiceCollection services)
        => services.AddTransient(_ => new ConfigurableService(FilterDisplay));
}

public class ProdOrStagingRegistry : AbstractRegistryModule {
    public static IReadOnlyCollection<string> EnvironmentFilters => ["production", "staging"];
    public static string FilterDisplay => string.Join(" or ", EnvironmentFilters);

    public override IReadOnlyCollection<string> TargetEnvironments => EnvironmentFilters;

    public override void ConfigureServices(IServiceCollection services)
        => services.AddTransient(_ => new ConfigurableService(FilterDisplay));
}

public class AnyEnvironmentRegistry : AbstractRegistryModule {
    public static IReadOnlyCollection<string> EnvironmentFilters => ["anything at all!"];
    public static string FilterDisplay => string.Join(" or ", EnvironmentFilters);

    public override void ConfigureServices(IServiceCollection services)
        => services.AddTransient(_ => new ConfigurableService(FilterDisplay));
}
