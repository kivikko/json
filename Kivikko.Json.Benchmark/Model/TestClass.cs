namespace Kivikko.Json.Benchmark.Model;

public class TestClass
{
    public bool? Bool { get; set; }
    public double? Double { get; set; }
    public int? Integer { get; set; }
    public TestEnum Enum { get; set; }
    public DateTime? DateTime { get; set; }
    public TimeSpan? Time { get; set; }
    public string[]? StringArray { get; set; }
    public IEnumerable<string>? StringEnumerable { get; set; }
    public List<string>? StringList { get; set; }
    public Dictionary<string, string>? Dictionary { get; set; }
    public TestClass? NestedClass { get; set; }
}