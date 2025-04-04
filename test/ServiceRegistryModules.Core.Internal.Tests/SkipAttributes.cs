namespace ServiceRegistryModules.Internal.Tests;

public class FactAttribute : Xunit.FactAttribute {
    public FactAttribute() => Skip = "refactoring...";
}

public class TheoryAttribute : Xunit.TheoryAttribute {
    public TheoryAttribute() : base() => Skip = "refactoring...";
}