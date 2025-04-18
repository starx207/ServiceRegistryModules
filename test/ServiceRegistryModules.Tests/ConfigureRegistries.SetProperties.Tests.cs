using System.Text.Json;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules.Exceptions;
using Shouldly;
using TestSamples4;

namespace ServiceRegistryModules.Tests;

public class ConfigureRegistries_SetProperties_Tests {
    [Theory,
        InlineData(true),
        InlineData(false)]
    public void SetAllProperties_WithPublicSetters(bool customizeConfigKey) {
        // Arrange
        var fixture = new Fixture();
        var configSectionName = customizeConfigKey
            ? fixture.Create<string>()
            : "service_registries";

        var expected = fixture.Create<ExpectedPublicProps>();

        var config = JsonConfig.Create($$"""
        {
            "{{configSectionName}}:configuration": {
                "ConfigurableRegistry1": {
                    "PublicString": "{{expected.PublicString}}",
                    "PublicInt": {{expected.PublicInt}},
                    "PublicBool": {{expected.PublicBool.ToString().ToLower()}}
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => {
            cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config);
            if (customizeConfigKey) {
                cfg.WithConfigurationsFromSection(configSectionName);
            }
        });
        var configSvc = services.BuildServiceProvider().GetRequiredService<ConfigurableService>();
        var actual = JsonSerializer.Deserialize<ExpectedPublicProps>(configSvc.Message);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Theory,
        InlineData(true),
        InlineData(false)]
    public void SetAllProperties_WithNonPublicSetters(bool customizeConfigKey) {
        // Arrange
        var fixture = new Fixture();
        var configSectionName = customizeConfigKey
            ? fixture.Create<string>()
            : "service_registries";

        var expected = fixture.Create<ExpectedNonPublicSetters>();

        var config = JsonConfig.Create($$"""
        {
            "{{configSectionName}}:configuration": {
                "ConfigurableRegistry1": {
                    "StringWithInternalSetter": "{{expected.StringWithInternalSetter}}",
                    "StringWithPrivateSetter": "{{expected.StringWithPrivateSetter}}",
                    "InternalString": "{{expected.InternalString}}",
                    "PrivateString": "{{expected.PrivateString}}"
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => {
            cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config);
            if (customizeConfigKey) {
                cfg.WithConfigurationsFromSection(configSectionName);
            }
        });
        var configSvc = services.BuildServiceProvider().GetRequiredService<ConfigurableService>();
        var actual = JsonSerializer.Deserialize<ExpectedNonPublicSetters>(configSvc.Message);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void ThrowException_WhenTryingToSetProperty_WithNoSetter() {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<ExpectedUnsettableProps>();

        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "ConfigurableRegistry1": {
                    "StringWithoutSetter": "{{expected.StringWithoutSetter}}",
                    "StringWithLambdaGetter": "{{expected.StringWithLambdaGetter}}"
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(()
            => services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config)));
        ex.Message.ShouldBe("Failed to configure ConfigurableRegistry1 because no setter found " +
        "for the following properties: StringWithoutSetter, StringWithLambdaGetter");
    }

    [Fact]
    public void NotThrow_WhenTryingToSetProperty_WithNoSetter_WhileSuppressingErrors() {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<ExpectedUnsettableProps>();

        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "ConfigurableRegistry1": {
                    "StringWithoutSetter": {
                         "Value": "{{expected.StringWithoutSetter}}",
                         "SuppressErrors": true
                    },
                    "StringWithLambdaGetter": {
                        "Value": "{{expected.StringWithLambdaGetter}}",
                        "SuppressErrors": true
                    }
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        Should.NotThrow(() =>{
            services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config));
        });
    }

    [Fact]
    public void ThrowException_WhenTryingToSetProperty_WithNonPublicSetter_UsingPublicOnly() {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<ExpectedNonPublicSetters>();

        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "ConfigurableRegistry1": {
                    "StringWithInternalSetter": "{{expected.StringWithInternalSetter}}",
                    "StringWithPrivateSetter": "{{expected.StringWithPrivateSetter}}"
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(()
            => services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config)
                .PublicOnly()));
        ex.Message.ShouldBe("Failed to configure ConfigurableRegistry1 because no public setter " +
        "found for the following properties: StringWithInternalSetter, StringWithPrivateSetter");
    }

    [Fact]
    public void NotThrow_WhenTryingToSetProperty_WithNonPublicSetter_UsingPublicOnly_AndErrorSuppression() {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<ExpectedNonPublicSetters>();

        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "ConfigurableRegistry1": {
                    "StringWithInternalSetter": {
                        "Value": "{{expected.StringWithInternalSetter}}",
                        "SuppressErrors": true
                    },
                    "StringWithPrivateSetter": {
                        "Value": "{{expected.StringWithPrivateSetter}}",
                        "SuppressErrors": true
                    }
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        Should.NotThrow(() => {
            services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config)
                .PublicOnly());
        });
    }

    [Fact]
    public void ThrowException_WhenTryingToSetNonPublicProperty_UsingPublicOnly() {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<ExpectedNonPublicSetters>();

        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "ConfigurableRegistry1": {
                    "InternalString": "{{expected.InternalString}}",
                    "PrivateString": "{{expected.PrivateString}}"
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(()
            => services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config)
                .PublicOnly()));
        ex.Message.ShouldBe("Configuration failed for the following non-existant or non-public " +
        "ConfigurableRegistry1 members: InternalString, PrivateString");
    }

    [Fact]
    public void NotThrow_WhenTryingToSetNonPublicProperty_UsingPublicOnly_AndErrorSuppression() {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<ExpectedNonPublicSetters>();

        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "ConfigurableRegistry1": {
                    "InternalString": {
                        "Value": "{{expected.InternalString}}",
                        "SuppressErrors": true
                    },
                    "PrivateString": {
                        "Value": "{{expected.PrivateString}}",
                        "SuppressErrors": true
                    }
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        Should.NotThrow(() => {
            services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config)
                .PublicOnly());
        });
    }

    [Fact]
    public void ThrowException_WhenTryingToSetProperty_WithIncompatibleType() {
        // Arrange
        var fixture = new Fixture();

        var config = JsonConfig.Create("""
        {
            "service_registries:configuration": {
                "ConfigurableRegistry1": {
                    "PublicInt": "Hello, World!"
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(()
            => services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config)));
        ex.Message.ShouldBe("Unable to set PublicInt value to configured value.");
    }

    [Fact]
    public void NotThrow_WhenTryingToSetProperty_WithIncompatibleType_WhileSuppressingErrors() {
        // Arrange
        var fixture = new Fixture();

        var config = JsonConfig.Create("""
        {
            "service_registries:configuration": {
                "ConfigurableRegistry1": {
                    "PublicInt": {
                        "Value": "Hello, World!",
                        "SuppressErrors": true
                    }
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        Should.NotThrow(() => {
            services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(ConfigurableRegistry1))
                .UsingConfiguration(config));
        });
    }

    public class ExpectedPublicProps {
        public string PublicString { get; set; } = string.Empty;
        public int PublicInt { get; set; }
        public bool PublicBool { get; set; }
    }

    public class ExpectedNonPublicSetters {
        public string StringWithInternalSetter { get; set; } = string.Empty;
        public string StringWithPrivateSetter { get; set; } = string.Empty;
        public string InternalString { get; set; } = string.Empty;
        public string PrivateString { get; set; } = string.Empty;
    }

    public class ExpectedUnsettableProps {
        public string StringWithoutSetter { get; set; } = string.Empty;
        public string StringWithLambdaGetter { get; set; } = string.Empty;
    }
}
