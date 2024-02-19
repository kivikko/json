// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Kivikko.Json.Benchmark.Model;

public class TestClass
{
    public NestedClass? NestedClass { get; set; }
    public IEnumerable<NestedClass>? Classes { get; set; }
    public Dictionary<int, NestedClass>? ClassesDictionary { get; set; }
    public string? String { get; set; }
}