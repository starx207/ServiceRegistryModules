using System;
using Microsoft.Extensions.DependencyInjection;
using ServiceRegistryModules.Exceptions;
using Shouldly;

namespace ServiceRegistryModules.Tests;

public class ApplyRegistries_IllegalSetup_Tests
{
    [Fact]
    public void ThrowException_WhenUsingEnvironment_OfWrongType() {
        // Arrange
        var services = new ServiceCollection();
        var illegalHost = "I'm not IHostEnvironment!";

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(() => {
            services.ApplyRegistries(cfg => {
                cfg.UsingEnvironment(illegalHost);
            });
        });
        ex.Message.ShouldBe("Environment object must implement IHostEnvironment");
    }

    [Theory,
        InlineData(""),
        InlineData("  "),
        InlineData(null)]
    public void ThrowException_WhenDefiningCustomConfigSection_WithInvalidValue(string? illegalSectionName) {
        // Arrange
        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(() => {
            services.ApplyRegistries(cfg => {
                cfg.WithConfigurationsFromSection(illegalSectionName!);
            });
        });
        ex.Message.ShouldBe("'sectionKey' cannot be null or whitespace.");
    }

    [Fact]
    public void ThrowException_WhenRegisteringFromAssemblies_WithoutPassingAnyAssemblies() {
        // Arrange
        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(() => {
            services.ApplyRegistries(cfg => {
                cfg.FromAssemblies();
            });
        });
        ex.Message.ShouldBe("No assemblies given to scan");
    }

    [Fact]
    public void ThrowException_WhenRegisteringTypes_WithoutTheRequiredInterface() {
        // Arrange
        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(() => {
            services.ApplyRegistries(cfg => {
                cfg.OfTypes(typeof(bool), typeof(TestSamples4.ProductionOnlyRegistry), typeof(Exception));
            });
        });
        ex.Message.ShouldBe("The following registry types do not implement IRegistryModule: Boolean, Exception");
    }
}
