using System.Collections.Generic;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceRegistryModules.Exceptions;
using Shouldly;

namespace ServiceRegistryModules.Tests;

/*
    TODO:
        // Actually, I don't know that these are necessary. I misread the code when reviewing coverage.
        // I could still do these tests, but I don't really know what the results wil be.
        -----Test adding a provider of a base type and of a derived type from the same base
        -----Test adding 2 providers that derive from the same base type
*/
public class ApplyRegistries_WithProviders_Tests {
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
    public void UseCorrectProvider_WhenMultipleProvidersOfSameType_ArePresent() {
        // Arrange
        var fixture = new Fixture();
        var string1 = fixture.Create<string>();
        var string2 = fixture.Create<string>();
        var expectedMsg = string1; // string1 will be "provided" first, so it should be used
        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => {
            cfg.OfTypes(typeof(TestSamples4.RegistryWithPrimitiveProviders));
            cfg.UsingProviders(string1, false, string2);
        });
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        service?.Message.ShouldBe(expectedMsg);
    }

    [Theory,
        InlineData(true),
        InlineData(false)]
    public void UseCorrectEnvironment_WhenProvidedInDifferentWays(bool mostSpecificFirst) {
        // Arrange
        var fixture = new Fixture();
        var appName = fixture.Create<string>();

        var env1 = new TestHostEnvironment {
            ApplicationName = appName
        };
        var env2 = new TestHostEnvironment {
            ApplicationName = fixture.Create<string>()
        };

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => {
            cfg.OfTypes(typeof(TestSamples4.RegistryWithEnvironmentAndConfig));
            if (mostSpecificFirst) {
                cfg.UsingEnvironment(env2).UsingProviders(env1);
            } else {
                cfg.UsingProviders(env2).UsingEnvironment(env1);
            }
        });
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        service?.Message.ShouldBe(appName);
    }

    [Theory,
        InlineData(true),
        InlineData(false)]
    public void UseCorrectConfiguration_WhenProvidedInDifferentWays(bool mostSpecificFirst) {
        // Arrange
        var fixture = new Fixture();
        var connStr = fixture.Create<string>();

        var config1 = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>() {
                { "ConnectionString", connStr }
            })
            .Build();

        var config2 = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>() {
                { "ConnectionString", fixture.Create<string>() }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => {
            cfg.OfTypes(typeof(TestSamples4.RegistryWithEnvironmentAndConfig));
            if (mostSpecificFirst) {
                cfg.UsingConfiguration(config2).UsingProviders(config1);
            } else {
                cfg.UsingProviders(config2).UsingConfiguration(config1);
            }
        });
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        service?.Message.ShouldBe(connStr);
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
