using System;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples4
{
    // This is to make it easy to register all the match registries in the unit tests.
    public static class TestMatchRegistries {
        public static readonly Type[] Types = [
            typeof(MatchRegistry),
            typeof(Outer.MatchRegistry),
            typeof(Outer.OtherRegistry),
            typeof(Outer.Inner.MatchRegistry),
            typeof(Outer.Inner.OtherRegistry)
        ];
    }

    public sealed class MatchRegistry : AbstractRegistryModule {
        public const string DEFAULT_MSG = "Default_Match_Msg0";
        public string CfgMsg { get; set; } = DEFAULT_MSG;

        public override void ConfigureServices(IServiceCollection services)
            => services.AddTransient(_ => new ConfigurableService(CfgMsg));
    }

    namespace Outer
    {
        public sealed class MatchRegistry : AbstractRegistryModule {
            public const string DEFAULT_MSG = "Default_Match_Msg1";
            public string CfgMsg { get; set; } = DEFAULT_MSG;

            public override void ConfigureServices(IServiceCollection services)
                => services.AddTransient(_ => new ConfigurableService(CfgMsg));
        }

        public sealed class OtherRegistry : AbstractRegistryModule {
                public const string DEFAULT_MSG = "Default_Match_Msg2";
                public string CfgMsg { get; set; } = DEFAULT_MSG;

                public override void ConfigureServices(IServiceCollection services)
                    => services.AddTransient(_ => new ConfigurableService(CfgMsg));
            }

        namespace Inner
        {
            public sealed class MatchRegistry : AbstractRegistryModule {
                public const string DEFAULT_MSG = "Default_Match_Msg3";
                public string CfgMsg { get; set; } = DEFAULT_MSG;

                public override void ConfigureServices(IServiceCollection services)
                    => services.AddTransient(_ => new ConfigurableService(CfgMsg));
            }

            public sealed class OtherRegistry : AbstractRegistryModule {
                public const string DEFAULT_MSG = "Default_Match_Msg4";
                public string CfgMsg { get; set; } = DEFAULT_MSG;

                public override void ConfigureServices(IServiceCollection services)
                    => services.AddTransient(_ => new ConfigurableService(CfgMsg));
            }
        }
    }
}
