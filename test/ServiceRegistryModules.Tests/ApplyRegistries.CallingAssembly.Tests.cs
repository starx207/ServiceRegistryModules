using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using Shouldly;

namespace ServiceRegistryModules.Tests;

public class ApplyRegistries_CallingAssembly_Tests
{
    [Fact]
    public void ApplyAllRegistries_DefinedInTheCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries();
        var provider = services.BuildServiceProvider();
        var serviceTypes = provider.GetServices<ITestService1>()
            .Select(s => s.GetType())
            .OrderBy(t => t.Name)
            .ToList();

        // Assert
        serviceTypes.ShouldBe([typeof(TestService1), typeof(TestService1Alt)]);
    }

    [Fact]
    public void ApplyOnlyPublicRegistries_DefinedInTheCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg.PublicOnly());
        var provider = services.BuildServiceProvider();
        var serviceTypes = provider.GetServices<ITestService1>()
            .Select(s => s.GetType())
            .OrderBy(t => t.Name)
            .ToList();

        // Assert
        serviceTypes.ShouldBe([typeof(TestService1)]);
    }

    [Theory,
        InlineData(true),
        InlineData(false)]
    public void ApplyNestedRegistries_AccordingToPublicConfiguration(bool publicOnly) {
        // Arrange
        var services = new ServiceCollection();
        List<Type> expectedSvc2Types = [];
        List<Type> expectedSvc3Types = [typeof(TestService3)];

        if (!publicOnly) {
            expectedSvc2Types.AddRange([typeof(TestService2), typeof(TestService2Alt)]);
            expectedSvc3Types.Add(typeof(TestService3Alt));
        }

        // Act
        services.ApplyRegistries(cfg => {
            if (publicOnly) {
                cfg.PublicOnly();
            }
        });
        var provider = services.BuildServiceProvider();
        var serviceTypes2 = provider.GetServices<ITestService2>()
            .Select(s => s.GetType())
            .OrderBy(t => t.Name)
            .ToList();

        var serviceTypes3 = provider.GetServices<ITestService3>()
            .Select(s => s.GetType())
            .OrderBy(t => t.Name)
            .ToList();

        // Assert
        serviceTypes2.ShouldBe(expectedSvc2Types);
        serviceTypes3.ShouldBe(expectedSvc3Types);
    }
}