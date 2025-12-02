using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using Shouldly;

namespace ServiceRegistryModules.Tests;

public class ApplyRegistries_ExplicitAssemblies_Tests
{
    [Fact]
    public void ApplyAllRegistries_DefinedInTheGivenAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(typeof(TestSamples1.Marker).Assembly, typeof(TestSamples2.Marker).Assembly);
        var provider = services.BuildServiceProvider();
        var serviceTypes = provider.GetServices<ITestService1>()
            .Select(s => s.GetType())
            .OrderBy(t => t.FullName)
            .ToList();

        // Assert
        serviceTypes.ShouldBe([
            typeof(TestSamples1.TestRegistry1.Service), 
            typeof(TestSamples1.TestRegistry2.Service),
            typeof(TestSamples2.TestRegistry1.Service),
            typeof(TestSamples2.TestRegistry2.Service)
        ]);
    }

    [Fact]
    public void ApplyPublicRegistries_DefinedInTheGivenAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg.FromAssemblies(
                typeof(TestSamples1.Marker).Assembly,
                GetType().Assembly
            ).PublicOnly()
        );
        var provider = services.BuildServiceProvider();
        var serviceTypes = provider.GetServices<ITestService1>()
            .Select(s => s.GetType())
            .OrderBy(t => t.FullName)
            .ToList();

        // Assert
        serviceTypes.ShouldBe([
            typeof(RegistryServices.TestService1),
            typeof(TestSamples1.TestRegistry1.Service), 
            typeof(TestSamples1.TestRegistry2.Service)
        ]);
    }
}
