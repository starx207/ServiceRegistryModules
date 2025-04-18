using System.Collections.Generic;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceRegistryModules.Exceptions;
using Shouldly;

namespace ServiceRegistryModules.Tests;

/*
    TODO (not necessarily all in this test file):
        -Test exception when UsingEnvironment with non IHostEnvironment type
        -Test exception when WithConfigurationsFromSection is null or whitespace
        -Test exception when FromAssemblies doesn't pass any assemblies
        -Test exception when OfTypes passes a type that doesn't implement IRegistryModule
        -Test removing explicitly constructed registry via config file
        -Test adding multiple providers of the same type
        -Test adding a provider of a base type and of a derived type from the same base
        -Test adding 2 providers that derive from the same base type
        -Test configuring registry value from another configuration key
        => test exception when the key is not found (unless supressing errors)
        -Test configuring something that is ambiguous between event and property

    TODO: Should I also test the WebApplicationBuilder variants? Not sure that adds a lot of value
*/
public class ApplyRegistries_WithProviders_Tests
{
    [Theory,
        InlineData(true, "Hello, World"),
        InlineData(false, "Happy Birthday")]
    public void PassProviderValues_ToRegistryOnActivation(bool negate, string message) {
        // Arrange
        var expectedMsg = message;
        if (negate) {
            expectedMsg += " (negate)";
        }
        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => {
            cfg.OfTypes(typeof(TestSamples4.RegistryWithPrimitiveProviders));
            cfg.UsingProviders(message, negate);
        });
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        service?.Message.ShouldBe(expectedMsg);
    }

    [Fact]
    public void PassConfigurationAndEnvironment_FromHostBuilder() {
        // Arrange
        var fixture = new Fixture();
        var connStr = fixture.Create<string>();
        var appName = fixture.Create<string>();
        var expectedMsg = $"{appName} + {connStr}";

        var hbc = new HostBuilderContext(new Dictionary<object, object>()) {
            HostingEnvironment = new TestHostEnvironment {
                ApplicationName = appName
            },
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>() {
                    { "ConnectionString", connStr }
                })
                .Build()
        };

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(hbc, cfg
            => cfg.OfTypes(typeof(TestSamples4.RegistryWithEnvironmentAndConfig)));
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        service?.Message.ShouldBe(expectedMsg);
    }

    [Theory,
        InlineData(true, false),
        InlineData(false, true)]
    public void UseBestFitConstructor_WhenNotAllProvidersPresent(bool includeConfig, bool includeEnv) {
        // Arrange
        var fixture = new Fixture();
        var connStr = fixture.Create<string>();
        var appName = fixture.Create<string>();
        var elements = new List<string>();
        object provider = null!;
        if (includeEnv) {
            elements.Add(appName);
            provider = new TestHostEnvironment() { ApplicationName = appName };
        }
        if (includeConfig) {
            elements.Add(connStr);
            provider = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>() {
                    { "ConnectionString", connStr }
                })
                .Build();
        }
        var expectedMsg = string.Join(" + ", elements);

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg
            .OfTypes(typeof(TestSamples4.RegistryWithEnvironmentAndConfig))
            .UsingProviders(provider));
        var svcProvider = services.BuildServiceProvider();
        var service = svcProvider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        service?.Message.ShouldBe(expectedMsg);
    }

    [Fact]
    public void ThrowExcpetion_WhenNoConstructorFound_ThatCanBeSatisfiedWithTheGivenProviders() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var registrationAction = () => {
            services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(TestSamples4.RegistryWithEnvironmentAndConfig))
                .UsingProviders("Hello, World!", false));
        };

        // Assert
        var ex = Should.Throw<RegistryActivationException>(registrationAction);
        ex.Message.ShouldBe("Unable to activate IRegistryModule of type 'RegistryWithEnvironmentAndConfig' " +
            "-- no suitable constructor found. " +
            "Allowable constructor parameters are: System.String, System.Boolean");
    }
}
