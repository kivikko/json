namespace Kivikko.Json.Benchmark.Model;

public static class TestFactory
{
    public static TestClass CreateObject() => new()
    {
        String = "Hello",
        NestedClass = new NestedClass
        {
            Bool = true,
            Double = Math.PI,
            Integer = 5,
            DateTime = new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Local),
            Time = new TimeSpan(days: 1, hours: 2, minutes: 3, seconds: 4, milliseconds: 5, microseconds: 6),
            StringArray = new[] { "A", "B", "C" },
            StringEnumerable = new[] { "K", "L", "M" },
            StringList = new List<string> { "X", "Y", "Z" },
            Dictionary = new Dictionary<int, string?>
            {
                [1] = "One",
                [2] = "Two",
                [3] = "Three"
            }
        },
        Classes = new[]
        {
            new NestedClass(),
            new NestedClass { Integer = null, Enum = TestEnum.Value1 },
            new NestedClass { Integer = 0, Enum = TestEnum.Value2 },
            new NestedClass { Integer = 1, Enum = TestEnum.Value3 },
        },
        ClassesDictionary = new Dictionary<int, NestedClass>
        {
            [0] = new() { Integer = 0 },
            [1] = new() { Integer = 1 },
            [2] = new() { Integer = 2 },
        }
    };
}