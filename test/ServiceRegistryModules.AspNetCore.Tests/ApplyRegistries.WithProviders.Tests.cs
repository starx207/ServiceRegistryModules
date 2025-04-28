using System.Collections.Generic;
using AutoFixture;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ServiceRegistryModules.AspNetCore.Tests;

public class ApplyRegistries_WithProviders_Tests {
    [Fact]
    public void UseCorrectEnvironment_WhenProvidedInDifferentWays() {
        // Arrange
        var fixture = new Fixture();
        var appName = fixture.Create<string>();

        var wab = WebApplication.CreateBuilder(new WebApplicationOptions {
            ApplicationName = appName
        });
        var env2 = new TestHostEnvironment {
            ApplicationName = fixture.Create<string>()
        };

        // Act
        wab.ApplyRegistries(cfg => {
            cfg.OfTypes(typeof(TestSamples4.RegistryWithEnvironmentAndConfig));
            cfg.UsingProviders(env2);
        });
        var provider = wab.Services.BuildServiceProvider();
        var service = provider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        service?.Message.ShouldBe(appName);
    }

    [Fact]
    public void UseCorrectConfiguration_WhenProvidedInDifferentWays() {
        // Arrange
        var fixture = new Fixture();
        var appName = fixture.Create<string>();
        var connStr = fixture.Create<string>();

        var wab = WebApplication.CreateBuilder(new WebApplicationOptions {
            ApplicationName = appName
        });
        wab.Configuration.AddInMemoryCollection(new Dictionary<string, string?>() {
            { "ConnectionString", connStr }
        });

        var config2 = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>() {
                { "ConnectionString", fixture.Create<string>() }
            })
            .Build();

        // Act
        wab.ApplyRegistries(cfg => {
            cfg.OfTypes(typeof(TestSamples4.RegistryWithEnvironmentAndConfig));
            cfg.UsingProviders(config2);
        });
        var provider = wab.Services.BuildServiceProvider();
        var service = provider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        service?.Message.ShouldBe($"{appName} + {connStr}");
    }

    [Fact]
    public void PassConfigurationAndEnvironment_FromWebAppBuilder() {
        // Arrange
        var fixture = new Fixture();
        var connStr = fixture.Create<string>();
        var appName = fixture.Create<string>();
        var expectedMsg = $"{appName} + {connStr}";
        
        var wab = WebApplication.CreateBuilder(new WebApplicationOptions {
            ApplicationName = appName
        });
        wab.Configuration.AddInMemoryCollection(new Dictionary<string, string?>() {
            { "ConnectionString", connStr }
        });

        // Act
        wab.ApplyRegistries(cfg
            => cfg.OfTypes(typeof(TestSamples4.RegistryWithEnvironmentAndConfig)));
        var provider = wab.Services.BuildServiceProvider();
        var service = provider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        service?.Message.ShouldBe(expectedMsg);
    }
}
