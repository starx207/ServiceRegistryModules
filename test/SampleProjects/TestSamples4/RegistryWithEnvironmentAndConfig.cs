using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples4;

public class RegistryWithEnvironmentAndConfig : AbstractRegistryModule {
    private readonly IHostEnvironment? _env;
    private readonly IConfiguration? _config;

    public RegistryWithEnvironmentAndConfig(IConfiguration config) => _config = config;

    public RegistryWithEnvironmentAndConfig(IHostEnvironment env) => _env = env;

    public RegistryWithEnvironmentAndConfig(IHostEnvironment env, IConfiguration config) {
        _env = env;
        _config = config;
    }

    public override void ConfigureServices(IServiceCollection services) {
        var elements = new List<string>();
        if (_env?.ApplicationName is { } appName) {
            elements.Add(appName);
        }
        if (_config is { } && _config["ConnectionString"] is { } connStr) {
            elements.Add(connStr);
        }
        var message = string.Join(" + ", elements);
        services.AddSingleton(_ => new ConfigurableService(message));
    }
}
