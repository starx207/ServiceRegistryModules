using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples4;

public class RegistryWithPrimitiveProviders : AbstractRegistryModule
{
    private readonly bool _negate;
    private readonly string _configuredMessage;

    public RegistryWithPrimitiveProviders(bool negate, string configuredMessage) {
        _negate = negate;
        _configuredMessage = configuredMessage;
    }

    public override void ConfigureServices(IServiceCollection services) {
        var message = _configuredMessage;
        if (_negate) {
            message += " (negate)";
        }

        services.AddSingleton(_ => new ConfigurableService(message));
    }
}
