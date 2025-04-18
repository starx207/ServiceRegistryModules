using System.Linq;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules.DefaultOptions.Tests;
using ServiceRegistryModules.Exceptions;
using Shouldly;

namespace ServiceRegistryModules.Tests;

public class ConfigureRegistries_AddRemove_Tests {
    [Theory,
        InlineData(true),
        InlineData(false)]
    public void RemoveLoadedRegistry(bool customizeConfigSection) {
        // Arrange
        var cfgSection = customizeConfigSection ?
            new Fixture().Create<string>()
            : "service_registries";

        // TestRegistry1 will be loaded.
        // TestRegistry2 will be skipped.
        // TestRegistry3 does not exist, but should not cause an error.
        var configuration = JsonConfig.Create($$"""
        {
            "{{cfgSection}}:skip": [
                "TestSamples1.TestRegistry2",
                "TestSamples1.TestRegistry3"
            ]
        }
        """);
        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => {
            cfg.FromAssemblyOf<TestSamples1.Marker>()
                .UsingConfiguration(configuration);
            if (customizeConfigSection) {
                cfg.WithConfigurationsFromSection(cfgSection);
            }
        });
        var provider = services.BuildServiceProvider();
        var serviceTypes = provider.GetServices<ITestService1>()
            .Select(svc => svc.GetType());

        // Assert
        serviceTypes.ShouldBe([typeof(TestSamples1.TestRegistry1.Service)]);
    }

    [Theory,
        InlineData(true),
        InlineData(false)]
    public void AddRegistryFromALoadedAssembly(bool customizeConfigSection) {
        // Arrange
        var cfgSection = customizeConfigSection ?
            new Fixture().Create<string>()
            : "service_registries";

        var configuration = JsonConfig.Create($$"""
        {
            "{{cfgSection}}:add": [
                "TestSamples1.TestRegistry2"
            ]
        }
        """);
        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => {
            cfg.OfTypes(
                typeof(TestSamples1.TestRegistry1)
            ).UsingConfiguration(configuration);
            if (customizeConfigSection) {
                cfg.WithConfigurationsFromSection(cfgSection);
            }
        });
        var provider = services.BuildServiceProvider();
        var serviceTypes = provider.GetServices<ITestService1>()
            .Select(svc => svc.GetType())
            .OrderBy(t => t.Name)
            .ToList();

        // Assert
        serviceTypes.ShouldBe([
            typeof(TestSamples1.TestRegistry1.Service),
            typeof(TestSamples1.TestRegistry2.Service)
        ]);
    }

    [Fact]
    public void AddRegistryFromUnreferencedAssembly_UsingAHintPath() {
        // Arrange
        var configuration = JsonConfig.Create("""
        {
            "service_registries:add": [
                {
                    "FullName": "UnreferencedTestSamples.TestRegistry1",
                    "HintPath": "../../../../SampleProjects/UnreferencedTestSamples/bin/Debug/netstandard2.1/UnreferencedTestSamples.dll"
                }
            ]
        }
        """);
        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg
            .From(EmptyRegistry.Instance)
            .UsingConfiguration(configuration));
        var provider = services.BuildServiceProvider();
        var serviceTypes = provider.GetServices<ITestService1>()
            .Select(svc => svc.GetType().FullName)
            .ToList();

        // Assert
        serviceTypes.ShouldBe([
            "UnreferencedTestSamples.TestRegistry1+Service"
        ]);
    }

    [Theory,
        InlineData(true),
        InlineData(false)]
    public void ThrowException_WhenAddingUnreferencedRegistry_WithoutHint(bool simpleAdd) {
        // Arrange
        var registryName = "UnreferencedTestSamples.TestRegistry1";
        // Create config with no hint path or incorrect hint path
        var configuration = simpleAdd
            ? JsonConfig.Create($$"""
                {
                    "service_registries:add": [
                        "{{registryName}}"
                    ]
                }
                """)
            : JsonConfig.Create($$"""
                {
                    "service_registries:add": [
                        {
                            "FullName": "{{registryName}}",
                            "HintPath": "../bin/Debug/netstandard2.1/UnreferencedTestSamples.dll"
                        }
                    ]
                }
                """);
        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(() => services.ApplyRegistries(cfg => cfg
            .From(EmptyRegistry.Instance)
            .UsingConfiguration(configuration)));
        ex.Message.ShouldBe($"Unable to find additional configured registries: {registryName}");
    }

    [Theory,
        InlineData(true),
        InlineData(false)]
    public void NotThrowException_WhenAddingUnreferencedRegistry_WithErrorSuppression(bool noHintPath) {
        // Arrange
        var registryName = "UnreferencedTestSamples.TestRegistry1";
        // Create config with no hint path or incorrect hint path
        var configuration = noHintPath
            ? JsonConfig.Create($$"""
                {
                    "service_registries:add": [
                        {
                            "FullName": "{{registryName}}",
                            "SuppressErrors": true
                        }
                    ]
                }
                """)
            : JsonConfig.Create($$"""
                {
                    "service_registries:add": [
                        {
                            "FullName": "{{registryName}}",
                            "SuppressErrors": true,
                            "HintPath": "../bin/Debug/netstandard2.1/UnreferencedTestSamples.dll"
                        }
                    ]
                }
                """);
        var services = new ServiceCollection();

        // Act/Assert
        Should.NotThrow(() => services.ApplyRegistries(cfg => cfg
            .From(EmptyRegistry.Instance)
            .UsingConfiguration(configuration)));
    }
}
