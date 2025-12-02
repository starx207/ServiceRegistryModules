using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ServiceRegistryModules.Tests;

internal static class JsonConfig {
#if NET7_0_OR_GREATER
    public static IConfiguration Create([StringSyntax(StringSyntaxAttribute.Json)] string json)
#else
    public static IConfiguration Create(string json)
#endif
    => new ConfigurationBuilder()
    .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
    .Build();
}
