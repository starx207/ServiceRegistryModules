using System;

namespace RegistryServices;

public class ConfigurableService {
    public ConfigurableService(string message) => Message = message;
    public string Message { get; }
}
