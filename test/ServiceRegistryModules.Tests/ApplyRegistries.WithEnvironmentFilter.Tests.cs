using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ServiceRegistryModules.Tests;

public class ApplyRegistries_WithEnvironmentFilter_Tests
{
    [Theory,
        InlineData("production"),
        InlineData("staging"),
        InlineData("development")]
    public void OnlyRegisterServices_FromRegistries_ThatMatchTheEnvironment_OrHaveNoFilter(string environmentName) {
        // Arrange
        var env = new TestHostEnvironment() {
            EnvironmentName = environmentName
        };

        List<string> expectedEnvironments = environmentName switch {
            "production" => [TestSamples4.ProductionOnlyRegistry.FilterDisplay, TestSamples4.ProdOrStagingRegistry.FilterDisplay],
            "staging" => [TestSamples4.StagingOnlyRegistry.FilterDisplay, TestSamples4.ProdOrStagingRegistry.FilterDisplay],
            "development" => [TestSamples4.DevelopmentOnlyRegistry.FilterDisplay],
            _ => []
        };
        expectedEnvironments.Add(TestSamples4.AnyEnvironmentRegistry.FilterDisplay);
        expectedEnvironments.Sort();

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg.OfTypes(
            typeof(TestSamples4.ProductionOnlyRegistry),
            typeof(TestSamples4.StagingOnlyRegistry),
            typeof(TestSamples4.DevelopmentOnlyRegistry),
            typeof(TestSamples4.ProdOrStagingRegistry),
            typeof(TestSamples4.AnyEnvironmentRegistry)
        ).UsingEnvironment(env));
        var provider = services.BuildServiceProvider();

        var configuredServices = provider.GetServices<RegistryServices.ConfigurableService>();
        var environments = configuredServices.Select(s => s.Message).ToList();
        environments.Sort();

        // Assert
        environments.ShouldBe(expectedEnvironments);
    }
}
