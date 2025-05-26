using System;

namespace ServiceRegistryModules.SourceGenerator.ClassDefs;

public static class GeneratedRegistryRunner
{
    public static string Name => nameof(GeneratedRegistryRunner);

    public const string DEFINITION = """
    namespace ServiceRegistryModules.Internal {
        internal partial class GeneratedRegistryRunner {
            /// <summary>
            /// Applies the <see cref="IRegistryModule"/>s according to the <see cref="FullServiceCollectionRegistryConfiguration"/>.
            /// </summary>
            /// <param name="services"></param>
            /// <param name="options">Configuration for which registries to apply</param>
            /// <returns></returns>
            public partial void ApplyRegistries(Microsoft.Extensions.DependencyInjection.IServiceCollection services, ServiceRegistryModules.Internal.RegistryOptions options);
        }
    }
    """;
}
