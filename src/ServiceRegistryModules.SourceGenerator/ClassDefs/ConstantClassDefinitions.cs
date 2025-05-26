using System;

namespace ServiceRegistryModules.SourceGenerator;

public static class ConstantClassDefinitions {
    [Obsolete("This is a temporary class. Please use the generated class instead.")]
    public const string TEMP_RUNNER_DEF = """
    namespace ServiceRegistryModules.Internal {
        internal partial class GeneratedRegistryRunner {
            public partial void ApplyRegistries(Microsoft.Extensions.DependencyInjection.IServiceCollection services, ServiceRegistryModules.Internal.RegistryOptions options) {
                throw new System.NotImplementedException("This is a temporary class. Please use the generated class instead.");
            }
        }
    }
    """;
}
