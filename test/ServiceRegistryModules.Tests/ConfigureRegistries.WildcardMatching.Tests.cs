using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using Shouldly;
using Xunit.Abstractions;

namespace ServiceRegistryModules.Tests;
/*
    TODO (not necessarily all in this test file): Should I also test the WebApplicationBuilder variants? Not sure that adds a lot of value
*/
public class ConfigureRegistries_WildcardMatching_Tests {
    [Theory,
        MemberData(nameof(MultiMatchData))]
    public void ConfigureAllRegistriesThatMatch(MultiMatchTestCase testCase) {
        // Arrange
        var configuredMsg = new Fixture().Create<string>();
        var expectedMessages = testCase.GetExpectedMessages(configuredMsg);
        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "{{testCase.MatchPattern}}": {
                    "CfgMsg": "{{configuredMsg}}"
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg
            .OfTypes(TestSamples4.TestMatchRegistries.Types)
            .UsingConfiguration(config));
        var provider = services.BuildServiceProvider();
        var messages = provider.GetServices<ConfigurableService>()
            .Select(s => s.Message)
            .ToArray();

        // Assert
        messages.ShouldBe(expectedMessages, ignoreOrder: true);
    }

    [Theory,
        MemberData(nameof(MatchSpecificityData))]
    public void UseConfigurationWithBestMatch(MatchSpecificityTestCase testCase) {
        // Arrange
        var expectedMsg = testCase.ExpectedMessage;
        var jsonBuilder = new StringBuilder();
        jsonBuilder.AppendLine("""{""");
        jsonBuilder.AppendLine("""    "service_registries:configuration": {""");
        foreach (var (pattern, message) in testCase.ConfiguredMessages) {
            jsonBuilder.AppendLine($$"""        "{{pattern}}": {""");
            jsonBuilder.AppendLine($$"""            "CfgMsg": "{{message}}" """);
            if (testCase.IsLastPattern(pattern)) {
                jsonBuilder.AppendLine("""        }""");
            } else {
                jsonBuilder.AppendLine("""        },""");
            }
        }
        jsonBuilder.AppendLine("""    }""");
        jsonBuilder.AppendLine("""}""");
        var config = JsonConfig.Create(jsonBuilder.ToString());

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg
            .OfTypes(typeof(TestSamples4.Outer.Inner.MatchRegistry))
            .UsingConfiguration(config));
        var provider = services.BuildServiceProvider();
        var cfgService = provider.GetService<ConfigurableService>();

        // Assert
        cfgService?.Message.ShouldBe(expectedMsg);
    }

    [Fact]
    public void ConfigureEvents_WithSameNameAsOtherProperties_WhenTypeExplicitlyGiven() {
        // Arrange
        var config = JsonConfig.Create("""
        {
            "service_registries:configuration": {
                "*Ambiguous*Registry": {
                    "AmbiguousConfig": {
                        "Value": "ServiceRegistryModules.Tests.ConfigureRegistries_WildcardMatching_Tests+Events.OnHandledEvent",
                        "Type": "event"
                    }
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg
            .OfTypes(
                typeof(TestSamples4.AmbiguousEventRegistry),
                typeof(TestSamples4.AmbiguousPropertyRegistry)
            ).UsingConfiguration(config));

        // Assert
        Events.HandledEventFor.ShouldBeOfType<TestSamples4.AmbiguousEventRegistry>();
        Events.HandledEventArgs.ShouldBeSameAs(TestSamples4.AmbiguousEventRegistry.EventArgs);
    }

    [Fact]
    public void ConfigureProperties_WithSameNameAsOtherEvents_WhenTypeExplicitlyGiven() {
        // Arrange
        var expected = new Fixture().Create<string>();
        var config = JsonConfig.Create($$"""
        {
            "service_registries:configuration": {
                "*Ambiguous*Registry": {
                    "AmbiguousConfig": {
                        "Value": "{{expected}}",
                        "Type": "property"
                    }
                }
            }
        }
        """);

        var services = new ServiceCollection();

        // Act
        services.ApplyRegistries(cfg => cfg
            .OfTypes(
                typeof(TestSamples4.AmbiguousEventRegistry),
                typeof(TestSamples4.AmbiguousPropertyRegistry)
            ).UsingConfiguration(config));
        var provider = services.BuildServiceProvider();
        var cfgService = provider.GetService<ConfigurableService>();

        // Assert
        cfgService?.Message.ShouldBe(expected);
    }

    #region Test Data
    public static TheoryData<MatchSpecificityTestCase> MatchSpecificityData => new TheoryData<MatchSpecificityTestCase> {
        new("Fully Qualifed Name Match", 6, 
            /* Priority 7 */"TestSamples*.*.*.MatchRegistry",
            /* Priority 6 */"*Outer.Inner.*Registry",
            /* Priority 5 */"TestSamples4.*.*.MatchRegistry",
            /* Priority 4 */"TestSamples4.Outer*.MatchRegistry",
            /* Priority 3 */"TestSamples4.Outer.Inner.*Registry",
            /* Priority 2 */"MatchRegistry",
            /* Priority 1 */"TestSamples4.Outer.Inner.MatchRegistry"),
        new("Simple type name match", 5, 
            /* Priority 6 */"TestSamples*.*.*.MatchRegistry",
            /* Priority 5 */"*Outer.Inner.*Registry",
            /* Priority 4 */"TestSamples4.*.*.MatchRegistry",
            /* Priority 3 */"TestSamples4.Outer*.MatchRegistry",
            /* Priority 2 */"TestSamples4.Outer.Inner.*Registry",
            /* Priority 1 */"MatchRegistry"),
        new("Single wildcard with tiebreaker", 4, 
            /* Priority 5 */"TestSamples*.*.*.MatchRegistry",
            /* Priority 4 */"*Outer.Inner.*Registry",
            /* Priority 3 */"TestSamples4.*.*.MatchRegistry",
            /* Priority 2 */"TestSamples4.Outer*.MatchRegistry",
            /* Priority 1 */"TestSamples4.Outer.Inner.*Registry"),
        new("Single wildcard", 3, 
            /* Priority 4 */"TestSamples*.*.*.MatchRegistry",
            /* Priority 3 */"*Outer.Inner.*Registry",
            /* Priority 2 */"TestSamples4.*.*.MatchRegistry",
            /* Priority 1 */"TestSamples4.Outer*.MatchRegistry"),
        new("Single wildcard for entire namespace", 3, 
            /* Priority 4 */"*.MatchRegistry",
            /* Priority 3 */"TestSamples*.*.*.MatchRegistry",
            /* Priority 2 */"*Outer.Inner.*Registry",
            /* Priority 1 */"TestSamples4.*.*.MatchRegistry"),
        new("Multiple wildcard with tiebreaker", 2, 
            /* Priority 3 */"TestSamples*.*.*.MatchRegistry",
            /* Priority 2 */"*Outer.Inner.*Registry",
            /* Priority 1 */"TestSamples4.*.*.MatchRegistry"),
        new("Multiple wildcard", 1, 
            /* Priority 2 */"*Outer.Inner.*Registry",
            /* Priority 1 */"TestSamples*.*.*.MatchRegistry"),
        new("Same length, different wildcard count", 1, 
            /* Priority 2 */"TestSamples*.*.*.MatchRegistry",
            /* Priority 1 */"TestSamples4.*.*.MatchRegistry"),
        new("Wildcard at start and end", 0, "*.Outer.Inner.*"),
        new("Wildcard at start", 0, "*.MatchRegistry"),
        new("Wildcard at end", 0, "TestSamples4.*"),
        new("Start/end patterns without match", 2,
            "TestSamples5.Outer.*.Match*",
            "*Samples4.Outer.*.NoMatchRegistry",
            "*.*Registry"),
    };

    public static TheoryData<MultiMatchTestCase> MultiMatchData => new TheoryData<MultiMatchTestCase> {
        new("TestSamples4.Outer.Inner.MatchRegistry",
            TestSamples4.MatchRegistry.DEFAULT_MSG,
            TestSamples4.Outer.MatchRegistry.DEFAULT_MSG,
            TestSamples4.Outer.OtherRegistry.DEFAULT_MSG,
            TestSamples4.Outer.Inner.OtherRegistry.DEFAULT_MSG),
        new("MatchRegistry",
            TestSamples4.Outer.OtherRegistry.DEFAULT_MSG,
            TestSamples4.Outer.Inner.OtherRegistry.DEFAULT_MSG),
        new("TestSamples4.Outer.*MatchRegistry",
            TestSamples4.MatchRegistry.DEFAULT_MSG,
            TestSamples4.Outer.OtherRegistry.DEFAULT_MSG,
            TestSamples4.Outer.Inner.OtherRegistry.DEFAULT_MSG),
        new("TestSamples4.*.MatchRegistry",
            TestSamples4.MatchRegistry.DEFAULT_MSG,
            TestSamples4.Outer.OtherRegistry.DEFAULT_MSG,
            TestSamples4.Outer.Inner.OtherRegistry.DEFAULT_MSG),
        new("TestSamples4.Outer.*.*Registry",
            TestSamples4.MatchRegistry.DEFAULT_MSG,
            TestSamples4.Outer.MatchRegistry.DEFAULT_MSG,
            TestSamples4.Outer.OtherRegistry.DEFAULT_MSG),
        new("TestSamples4.*")
    };
    #endregion

    #region Test Classes
    public static class Events {
        public static object? HandledEventFor = null;
        public static EventArgs? HandledEventArgs = null;

        public static void OnHandledEvent(object sender, EventArgs e) {
            HandledEventFor = sender;
            HandledEventArgs = e;
        }
    }

    public class MatchSpecificityTestCase : IXunitSerializable {
        public string DisplayName { get; private set; }
        public (string Pattern, string Message)[] ConfiguredMessages { get; private set; }
        public int ExpectedMessageIndex { get; private set; }
        public string ExpectedMessage => ConfiguredMessages[ExpectedMessageIndex].Message;

        public MatchSpecificityTestCase() {
            DisplayName = "Empty Test Case";
            ConfiguredMessages = Array.Empty<(string, string)>();
            ExpectedMessageIndex = 0;
        }

        public MatchSpecificityTestCase(string displayName, int expectedMessageIndex, params string[] patterns) : this() {
            DisplayName = displayName;
            ExpectedMessageIndex = expectedMessageIndex;
            var fixture = new Fixture();
            ConfiguredMessages = [.. patterns.Select(p => (p, fixture.Create<string>()))];
        }

        public bool IsLastPattern(string pattern) => ConfiguredMessages[^1].Pattern == pattern;

        public void Deserialize(IXunitSerializationInfo info) {
            DisplayName = info.GetValue<string>(nameof(DisplayName));
            ExpectedMessageIndex = info.GetValue<int>(nameof(ExpectedMessageIndex));
            var patterns = info.GetValue<string[]>("patterns");
            var messages = info.GetValue<string[]>("messages");
            ConfiguredMessages = [.. Enumerable.Range(0, patterns.Length).Select(i => (patterns[i], messages[i]))];
        }

        public void Serialize(IXunitSerializationInfo info) {
            info.AddValue(nameof(DisplayName), DisplayName);
            info.AddValue(nameof(ExpectedMessageIndex), ExpectedMessageIndex);
            string[] patterns = [.. ConfiguredMessages.Select(m => m.Pattern)];
            string[] messages = [.. ConfiguredMessages.Select(m => m.Message)];
            info.AddValue(nameof(patterns), patterns);
            info.AddValue(nameof(messages), messages);
        }

        public override string ToString() => DisplayName;
    }

    public class MultiMatchTestCase : IXunitSerializable {
        private string[] _messages;
        public string MatchPattern { get; private set; }

        public MultiMatchTestCase() {
            MatchPattern = string.Empty;
            _messages = [.. Enumerable.Repeat(
                "{0}",
                TestSamples4.TestMatchRegistries.Types.Length
            )];
        }

        public MultiMatchTestCase(string matchPattern, params string[] unchangedMessages) : this() {
            MatchPattern = matchPattern;
            for (var i = 0; i < unchangedMessages.Length; i++) {
                _messages[i] = unchangedMessages[i];
            }
        }

        public string[] GetExpectedMessages(string configuredMsg)
            => [.. _messages.Select(msg => string.Format(msg, configuredMsg))];

        public void Deserialize(IXunitSerializationInfo info) {
            MatchPattern = info.GetValue<string>(nameof(MatchPattern));
            _messages = info.GetValue<string[]>(nameof(_messages));
        }

        public void Serialize(IXunitSerializationInfo info) {
            info.AddValue(nameof(MatchPattern), MatchPattern);
            info.AddValue(nameof(_messages), _messages);
        }

        public override string ToString() => MatchPattern;
    }
    #endregion
}
