using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RegistryServices;
using ServiceRegistryModules;

namespace TestSamples4;

public class ConfigurableRegistry1 : AbstractRegistryModule {
    public string PublicString { get; set; } = string.Empty;
    public int PublicInt { get; set; }
    public bool PublicBool { get; set; }

    public string StringWithInternalSetter { get; internal set; } = string.Empty;
    public string StringWithPrivateSetter { get; private set; } = string.Empty;
    public string StringWithoutSetter { get; } = string.Empty;
    public string StringWithLambdaGetter => string.Empty;
    internal string InternalString { get; set; } = string.Empty;
    private string PrivateString { get; set; } = string.Empty;

    public override void ConfigureServices(IServiceCollection services) {
        dynamic me = new ExpandoObject();
        me.PublicString = PublicString;
        me.PublicInt = PublicInt;
        me.PublicBool = PublicBool;
        me.StringWithInternalSetter = StringWithInternalSetter;
        me.StringWithPrivateSetter = StringWithPrivateSetter;
        me.StringWithoutSetter = StringWithoutSetter;
        me.StringWithLambdaGetter = StringWithLambdaGetter;
        me.InternalString = InternalString;
        me.PrivateString = PrivateString;

        var meAsJson = JsonSerializer.Serialize(me);
        services.AddSingleton(new ConfigurableService(meAsJson));
    }

}
