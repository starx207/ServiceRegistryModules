using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ServiceRegistryModules.Tests;

internal static class JsonConfig {
    public static IConfiguration Create([StringSyntax(StringSyntaxAttribute.Json)] string json)
    => new ConfigurationBuilder()
    .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
    .Build();
}
