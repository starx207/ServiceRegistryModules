using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using ServiceRegistryModules.Exceptions;
using Shouldly;
using Xunit.Abstractions;

namespace ServiceRegistryModules.Tests;

public class ConfigureRegistries_SetEvent_Tests {
    [Fact]
    public void InvokeConfiguredEventHandler() {
        // Arrange
        var config = JsonConfig.Create("""
        {
            "service_registries:configuration": {
                "TestRegistry2": {
                    "MyPublicEvent": "ServiceRegistryModules.Tests.ConfigureRegistries_SetEvent_Tests+Events.OnHandledEvent"
                }
            }
        }
        """);

        // Act
        TestSamples4.TestHost.ConfigureServices(config);

        // Assert
        Events.HandledEventFor.ShouldBeOfType<TestSamples1.TestRegistry2>();
        Events.HandledEventArgs.ShouldBeSameAs(TestSamples1.TestRegistry2.EventArgs);
    }

    [Fact]
    public void RetrieveAndInvokeEventHandler_FromOtherConfiguration() {
        // Arrange
        var config = JsonConfig.Create("""
        {
            "service_registries:configuration": {
                "TestRegistry2": {
                    "MyPublicEvent": {
                        "Value": "other:section:handler",
                        "Type": "config"
                    }
                }
            },
            "other:section": {
                "handler": "ServiceRegistryModules.Tests.ConfigureRegistries_SetEvent_Tests+Events.OnHandledEvent"
            }
        }
        """);

        // Act
        TestSamples4.TestHost.ConfigureServices(config);

        // Assert
        Events.HandledEventFor.ShouldBeOfType<TestSamples1.TestRegistry2>();
        Events.HandledEventArgs.ShouldBeSameAs(TestSamples1.TestRegistry2.EventArgs);
    }

    [Fact]
    public void ThrowWhenRetreivingHandler_FromOtherConfiguration_ThatIsNotSupplied() {
        // Arrange
        var config = JsonConfig.Create("""
        {
            "service_registries:configuration": {
                "TestRegistry2": {
                    "MyPublicEvent": {
                        "Value": "other:handler",
                        "Type": "config"
                    }
                }
            },
            "other:section": {
                "handler": "ServiceRegistryModules.Tests.ConfigureRegistries_SetEvent_Tests+Events.OnHandledEvent"
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(() => {
            services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(TestSamples1.TestRegistry2))
                .UsingConfiguration(config));
        });
        ex.Message.ShouldBe("Unable to resolve configuration key for 'other:handler'");
    }

    [Fact]
    public void NotThrowWhenRetreivingHandler_FromOtherConfiguration_ThatIsNotSupplied_WithErrorSuppression() {
        // Arrange
        var config = JsonConfig.Create("""
        {
            "service_registries:configuration": {
                "TestRegistry2": {
                    "MyPublicEvent": {
                        "Value": "other:handler",
                        "Type": "config",
                        "SuppressErrors": true
                    }
                }
            },
            "other:section": {
                "handler": "ServiceRegistryModules.Tests.ConfigureRegistries_SetEvent_Tests+Events.OnHandledEvent"
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        Should.NotThrow(() => {
            services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(TestSamples1.TestRegistry2))
                .UsingConfiguration(config));
        });
    }

    [Fact]
    public void InvokeEventHandler_FromUnreferencedAssembly() {
        // Arrange
        var config = JsonConfig.Create("""
        {
            "service_registries:configuration": {
                "TestRegistry1": {
                    "MyPublicEvent": {
                        "Value": "ServiceRegistryModules.Tests.ConfigureRegistries_SetEvent_Tests+Events.OnHandledEvent",
                        "HintPath": "../../../../SampleProjects/UnreferencedTestSamples/bin/Debug/netstandard2.1/UnreferencedTestSamples.dll"
                    }
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg
            .OfTypes(typeof(TestSamples2.TestRegistry2))
            .UsingConfiguration(config));
        var provider = services.BuildServiceProvider();
        var cfgService = provider.GetService<RegistryServices.ConfigurableService>();

        // Assert
        cfgService?.Message.ShouldBe("From UnreferencedTestSamples");
    }

    [Theory, MemberData(nameof(ExceptionData), "{1} when {0}")]
    public void ThrowForEvent(EventExceptionTestCase scenario) {
        // Arrange
        var formattedErrorMsg = string.Format(scenario.ExpectedErrMsg, "MyPublicEvent");
        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "TestRegistry2": {
                    "MyPublicEvent": "{{scenario.HandlerName}}"
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act/Assert
        var ex = Should.Throw<RegistryConfigurationException>(() => {
            services.ApplyRegistries(cfg => cfg
                .OfTypes(typeof(TestSamples1.TestRegistry2))
                .UsingConfiguration(config));
        });
        ex.ShouldSatisfyAllConditions(
            e => e.Message.ShouldBe(formattedErrorMsg),
            e => {
                if (scenario.InnerExTypeName is not null) {
                    e.InnerException?.GetType().FullName.ShouldBe(scenario.InnerExTypeName);
                } else {
                    e.InnerException.ShouldBeNull();
                }
            }
        );
    }

    [Theory, MemberData(nameof(ExceptionData), "{1} when {0} but error suppression is on")]
    public void NotThrowForEvent(EventExceptionTestCase scenario) {
        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "TestRegistry2": {
                    "MyPublicEvent": {
                        "Value":  "{{scenario.HandlerName}}",
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
                .OfTypes(typeof(TestSamples1.TestRegistry2))
                .UsingConfiguration(config));
        });
    }

    #region Test Data
    public static TheoryData<EventExceptionTestCase> ExceptionData(string displayFormat) => new() {
        new EventExceptionTestCase(displayFormat,
            condition: "event handler assembly cannot be found",
            handlerName: "Some.Bogus.Assembly.TestEventHandler",
            expectedErrMsg: "'{0}' event handler could not be loaded from assembly 'Some.Bogus'.",
            innerExType: typeof(FileNotFoundException)),

        new EventExceptionTestCase(displayFormat,
            condition: "event handler declaring type cannot be found",
            handlerName: $"{typeof(TestSamples2.TestRegistry1).FullName}_Oops.TestEventHandler",
            expectedErrMsg: $"'{{0}}' event handler could not be found in type '{nameof(TestSamples2.TestRegistry1)}_Oops'.",
            innerExType: typeof(TypeLoadException)),

        new EventExceptionTestCase(displayFormat,
            condition: "event handler method cannot be found",
            handlerName: $"{typeof(TestSamples2.TestRegistry1).FullName}.TestEventHandler_Oops",
            expectedErrMsg: $"'{{0}}' event handler could not be set from method 'TestEventHandler_Oops'. No such static method found."),

        new EventExceptionTestCase(displayFormat,
            condition: "event handler method incompatible with delegate",
            handlerName: $"{typeof(TestSamples2.TestRegistry1).FullName}.TestInvalidHandler",
            expectedErrMsg: $"'{nameof(TestSamples2.TestRegistry1)}.TestInvalidHandler' is not a compatible event handler for '{{0}}'.",
            innerExType: typeof(ArgumentException)),

        new EventExceptionTestCase(displayFormat,
            condition: "configured handler does not include the assembly name",
            handlerName: $"{nameof(TestSamples2.TestRegistry1)}.TestEventHandler",
            expectedErrMsg: $"Invalid handler name ({nameof(TestSamples2.TestRegistry1)}.TestEventHandler). Please use the fully qualified handler name."),

        new EventExceptionTestCase(displayFormat,
            condition: "configured handler does not include the type name",
            handlerName: "TestEventHandler",
            expectedErrMsg: "Invalid handler name (TestEventHandler). Please use the fully qualified handler name.")
    };
    #endregion

    #region Test Classes
    public class EventExceptionTestCase : IXunitSerializable {
        public string DisplayFormat { get; private set; }
        public string Condition { get; private set; }
        public string HandlerName { get; private set; }
        public string ExpectedErrMsg { get; private set; }
        public string? InnerExTypeName { get; private set; }
        public string? InnerExTypeShortName { get; private set; }

        public EventExceptionTestCase() {
            DisplayFormat = string.Empty;
            Condition = string.Empty;
            HandlerName = string.Empty;
            ExpectedErrMsg = string.Empty;
        }

        public EventExceptionTestCase(string displayFormat, string condition, string handlerName, string expectedErrMsg, Type? innerExType = null) {
            DisplayFormat = displayFormat;
            Condition = condition;
            HandlerName = handlerName;
            ExpectedErrMsg = expectedErrMsg;
            InnerExTypeName = innerExType?.FullName;
            InnerExTypeShortName = innerExType?.Name;
        }

        public override string ToString() => string.Format(DisplayFormat, Condition, InnerExTypeShortName ?? nameof(RegistryConfigurationException)).Trim();
        public void Deserialize(IXunitSerializationInfo info) {
            DisplayFormat = info.GetValue<string>(nameof(DisplayFormat));
            Condition = info.GetValue<string>(nameof(Condition));
            HandlerName = info.GetValue<string>(nameof(HandlerName));
            ExpectedErrMsg = info.GetValue<string>(nameof(ExpectedErrMsg));
            InnerExTypeName = info.GetValue<string?>(nameof(InnerExTypeName));
            InnerExTypeShortName = info.GetValue<string?>(nameof(InnerExTypeShortName));
        }
        public void Serialize(IXunitSerializationInfo info) {
            info.AddValue(nameof(DisplayFormat), DisplayFormat);
            info.AddValue(nameof(Condition), Condition);
            info.AddValue(nameof(HandlerName), HandlerName);
            info.AddValue(nameof(ExpectedErrMsg), ExpectedErrMsg);
            info.AddValue(nameof(InnerExTypeName), InnerExTypeName);
            info.AddValue(nameof(InnerExTypeShortName), InnerExTypeShortName);
        }
    }

    public static class Events {
        public static object? HandledEventFor = null;
        public static EventArgs? HandledEventArgs = null;

        public static void OnHandledEvent(object sender, EventArgs e) {
            HandledEventFor = sender;
            HandledEventArgs = e;
        }
    }
    #endregion
}
