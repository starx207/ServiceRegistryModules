using System;

namespace ServiceRegistryModules.SourceGenerator.ClassDefs;

public static class ServiceCollectionExtensions
{
    public static string Name => nameof(ServiceCollectionExtensions);

    public const string DEFINITION = """
    namespace ServiceRegistryModules {
        internal static class ServiceCollectionExtensions {
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
            public static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, params System.Reflection.Assembly[] assemblies)
                => services.ApplyRegistries(config => {
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
            public static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, System.Action<FullServiceCollectionRegistryConfiguration> registryConfiguration)
                => services.ApplyRegistries(registryConfiguration, System.Reflection.Assembly.GetCallingAssembly());

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
            public static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, Microsoft.Extensions.Hosting.HostBuilderContext context, params System.Reflection.Assembly[] assemblies)
                => services.ApplyRegistries(config => {
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
            public static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, Microsoft.Extensions.Hosting.HostBuilderContext context, System.Action<ServiceCollectionRegistryConfiguration> registryConfiguration) 
                => services.ApplyRegistries(config => {
                    config.UsingConfiguration(context.Configuration);
                    config.UsingEnvironment(context.HostingEnvironment);
                    registryConfiguration(config);
                }, System.Reflection.Assembly.GetCallingAssembly());

            internal static Microsoft.Extensions.DependencyInjection.IServiceCollection ApplyRegistries(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, System.Action<FullServiceCollectionRegistryConfiguration> registryConfiguration, System.Reflection.Assembly callingAssembly) {
                var runnerConfig = new FullServiceCollectionRegistryConfiguration();
                runnerConfig.WithDefaultAssembly(callingAssembly);
                registryConfiguration(runnerConfig);
                var options = runnerConfig.GetOptions();

                var genereatedRunner = new ServiceRegistryModules.Internal.GeneratedRegistryRunner();
                genereatedRunner.ApplyRegistries(services, options);

                return services;
            }
        }
    }
    """;
}
