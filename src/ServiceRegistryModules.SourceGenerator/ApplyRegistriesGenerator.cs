using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ServiceRegistryModules.SourceGenerator.ClassDefs;

namespace ServiceRegistryModules.SourceGenerator;


[Generator(LanguageNames.CSharp)]
public class ApplyRegistriesGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // Register a callback to be invoked during the generation process
        context.RegisterPostInitializationOutput(GenerateStaticSource);
    }

    private static void GenerateStaticSource(IncrementalGeneratorPostInitializationContext context) {
        // Example of generating a simple source file
        context.AddSource($"{GeneratedRegistryRunner.Name}.g.cs", SourceText.From(GeneratedRegistryRunner.DEFINITION, Encoding.UTF8));
        context.AddSource($"{ServiceCollectionExtensions.Name}.g.cs", SourceText.From(ServiceCollectionExtensions.DEFINITION, Encoding.UTF8));
        context.AddSource($"{GeneratedRegistryRunner.Name}.Test.g.cs", SourceText.From(ConstantClassDefinitions.TEMP_RUNNER_DEF, Encoding.UTF8));
    }
}
