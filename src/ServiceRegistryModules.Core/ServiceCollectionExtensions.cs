// using System;
// using System.Linq;
// using System.Reflection;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using ServiceRegistryModules.Exceptions;
// using ServiceRegistryModules.Internal;

namespace ServiceRegistryModules;
// TODO: I'm thinking this entire class should be source generated. This will allow me to add a partial method that
//       can be filled in by the source generator.
// TODO: Maybe we could use interceptors to get the calling assembly here instead of using reflection for GetCallingAssembly?
public static class ServiceCollectionExtensions {
    /// <summary>
    /// Applies the <see cref="IRegistryModule"/>s from the given assemblies.
    /// If no assemblies provided, the calling assembly will be scanned for <see cref="IRegistryModule"/> implementations to apply.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies">The assemblies to scan</param>
    /// <returns></returns>
    /// <exception cref="RegistryActivationException">
    /// When unable to instantiate an <see cref="IRegistryModule"/> implementation
    /// or when any registry configurations are invalid.
    /// </exception>
    /// <exception cref="RegistryConfigurationException">When there is a problem with the registry configuration</exception>
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries_Old(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, params System.Reflection.Assembly[] assemblies)
        => services.ApplyRegistries_Old(config => {
            if (assemblies.Length > 0) {
                config.FromAssemblies(assemblies);
            }
        }, System.Reflection.Assembly.GetCallingAssembly());

    /// <summary>
    /// Applies the <see cref="IRegistryModule"/>s according to the <see cref="FullServiceCollectionRegistryConfiguration"/>.
    /// If the configuration does not specify any registries, registry types, or registry assemblies, 
    /// the calling assembly will be scanned for <see cref="IRegistryModule"/> implementations to apply.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="registryConfiguration">Configuration for which registries to apply</param>
    /// <returns></returns>
    /// <exception cref="RegistryActivationException">
    /// When unable to instantiate an <see cref="IRegistryModule"/> implementation
    /// or when any registry configurations are invalid.
    /// </exception>
    /// <exception cref="RegistryConfigurationException">When there is a problem with the registry configuration</exception>
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries_Old(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, System.Action<FullServiceCollectionRegistryConfiguration> registryConfiguration)
        => services.ApplyRegistries_Old(registryConfiguration, System.Reflection.Assembly.GetCallingAssembly());

    /// <summary>
    /// Applies the <see cref="IRegistryModule"/>s from the given assemblies.
    /// If no assemblies provided, the calling assembly will be scanned for <see cref="IRegistryModule"/> implementations to apply.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="context">The context from which to obtain the <see cref="IHostingEnvironment"/> and <see cref="IConfiguration"/></param>
    /// <param name="assemblies">The assemblies to scan</param>
    /// <returns></returns>
    /// <exception cref="RegistryActivationException">
    /// When unable to instantiate an <see cref="IRegistryModule"/> implementation
    /// or when any registry configurations are invalid.
    /// </exception>
    /// <exception cref="RegistryConfigurationException">When there is a problem with the registry configuration</exception>
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries_Old(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, Microsoft.Extensions.Hosting.HostBuilderContext context, params System.Reflection.Assembly[] assemblies)
        => services.ApplyRegistries_Old(config => {
            if (assemblies.Length > 0) {
                config.FromAssemblies(assemblies);
            }
            config.UsingConfiguration(context.Configuration);
            config.UsingEnvironment(context.HostingEnvironment);
        }, System.Reflection.Assembly.GetCallingAssembly());

    /// <summary>
    /// Applies the <see cref="IRegistryModule"/>s according to the <see cref="ServiceCollectionRegistryConfiguration"/>.
    /// If the configuration does not specify any registries, registry types, or registry assemblies, 
    /// the calling assembly will be scanned for <see cref="IRegistryModule"/> implementations to apply.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="context">The context from which to obtain the <see cref="IHostingEnvironment"/> and <see cref="IConfiguration"/></param>
    /// <param name="registryConfiguration">Configuration for which registries to apply</param>
    /// <returns></returns>
    /// <exception cref="RegistryActivationException">
    /// When unable to instantiate an <see cref="IRegistryModule"/> implementation
    /// or when any registry configurations are invalid.
    /// </exception>
    /// <exception cref="RegistryConfigurationException">When there is a problem with the registry configuration</exception>
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries_Old(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, Microsoft.Extensions.Hosting.HostBuilderContext context, System.Action<ServiceCollectionRegistryConfiguration> registryConfiguration) 
        => services.ApplyRegistries_Old(config => {
            config.UsingConfiguration(context.Configuration);
            config.UsingEnvironment(context.HostingEnvironment);
            registryConfiguration(config);
        }, System.Reflection.Assembly.GetCallingAssembly());

    internal static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries_Old(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, System.Action<FullServiceCollectionRegistryConfiguration> registryConfiguration, System.Reflection.Assembly callingAssembly) {
        var runnerConfig = new FullServiceCollectionRegistryConfiguration();
        runnerConfig.WithDefaultAssembly(callingAssembly);
        registryConfiguration(runnerConfig);
        var options = runnerConfig.GetOptions();

        Internal.InternalServiceProvider.GetRegistryRunner().ApplyRegistries(services, options);

        return services;
    }
}
