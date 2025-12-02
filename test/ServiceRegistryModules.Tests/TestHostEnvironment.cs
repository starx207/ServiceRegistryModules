using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace ServiceRegistryModules.Tests;

public class TestHostEnvironment : IHostEnvironment {
    public string EnvironmentName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ContentRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = new TestFileProvider();
}

public class TestFileProvider : IFileProvider {
    public IDirectoryContents GetDirectoryContents(string subpath) => throw new NotImplementedException();
    public IFileInfo GetFileInfo(string subpath) => throw new NotImplementedException();
    public IChangeToken Watch(string filter) => throw new NotImplementedException();
}
