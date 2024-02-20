// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Kivikko.Json.Benchmark.Model;

public class MainTestClass
{
    public TestClass? NestedClass { get; set; }
    public IEnumerable<TestClass>? NestedClasses { get; set; }
    public Dictionary<string, TestClass>? ClassesDictionary { get; set; }
}