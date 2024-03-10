using System.Text;
using Kivikko.Json.Tests.Models.Model1;
using Kivikko.Json.Tests.Models.Model2;
using Kivikko.Json.Tests.Models.Model3;
using Kivikko.Json.Tests.Models.Model4;

namespace Kivikko.Json.Tests;

public class JsonUtilsTests
{
    [Test]
    public void FromJsonProductRangeTest()
    {
        var oldJson = Encoding.UTF8.GetString(Resources.TestProducts);
        var range = JsonUtils.FromJson<Range<Product>>(oldJson);
        var newJson = JsonUtils.ToJson(range, ignoreNullOrDefaultValues: false);
        
        Assert.Multiple(() =>
        {
            Assert.That(range, Is.Not.Null);
            Assert.That(newJson, Is.EqualTo(oldJson));
            Assert.That(range.Items, Has.Count.EqualTo(5));
            Assert.That(range.Items[0].ProductItems, Has.Count.EqualTo(5));
            Assert.That(range.Items[1].ProductItems, Has.Count.EqualTo(5));
            Assert.That(range.Items[2].ProductItems, Has.Count.EqualTo(5));
            Assert.That(range.Items[3].ProductItems, Has.Count.EqualTo(5));
            Assert.That(range.Items[4].ProductItems, Has.Count.EqualTo(5));
        });
    }

    [Test]
    public void ConvertProductTest()
    {
        var product     = JsonUtils.FromJson<ProductRoot>(ProductRoot.LV430880);
        var productJson = JsonUtils.ToJson(product);
        
        Assert.Multiple(() =>
        {
            Assert.That(product, Is.Not.Null);
            Assert.That(productJson, Is.EqualTo(ProductRoot.LV430880));
        });
    }

    [Test]
    public void ConvertCableTrayItemsTest()
    {
        var cableTrayItems     = JsonUtils.FromJson<CableTrayItem[]>(CableTraysDataTest.Json);
        var cableTrayItemsJson = JsonUtils.ToJson(cableTrayItems);
        
        Assert.Multiple(() =>
        {
            Assert.That(cableTrayItems, Is.Not.Null);
            Assert.That(cableTrayItemsJson, Is.EqualTo(CableTraysDataTest.Json));
        });
    }

    [Test, TestCaseSource(typeof(TestCases), nameof(TestCases.FromJsonCases))]
    public object FromJsonTest(Type type, string obj)
    {
        // var expectedObject = JsonConvert.DeserializeObject(obj, type);
        
        var actualObject = JsonUtils.FromJson(obj, type);
        
        // Console.WriteLine($"JsonConvert\n-----------\n{expectedObject}");
        // Console.WriteLine();
        
        Console.WriteLine($"JsonUtils\n---------\n{actualObject}");

        // Assert.That(actualObject, Is.EqualTo(expectedObject));
        
        return actualObject;
    }
    
    [Test, TestCaseSource(typeof(TestCases), nameof(TestCases.ToJsonCases))]
    public string ToJsonTest(object obj)
    {
        // var expectedJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        // {
            // DefaultValueHandling = DefaultValueHandling.Ignore,
            // NullValueHandling = NullValueHandling.Ignore,
        // });
        
        var actualJson = JsonUtils.ToJson(obj);
        
        // Console.WriteLine($"JsonConvert\n-----------\n{expectedJson}");
        // Console.WriteLine();
        
        Console.WriteLine($"JsonUtils\n---------\n{actualJson}");

        // Assert.That(actualJson, Is.EqualTo(expectedJson));
        
        return actualJson;
    }

    private class TestCases
    {
        public static IEnumerable<TestCaseData> FromJsonCases()
        {
            yield return new TestCaseData(typeof(bool), "true").Returns(true);
            yield return new TestCaseData(typeof(bool?), "true").Returns(true);
            yield return new TestCaseData(typeof(bool?), "").Returns(null);
            yield return new TestCaseData(typeof(bool?), "null").Returns(null);
            yield return new TestCaseData(typeof(double), "1.2").Returns(1.2);
            yield return new TestCaseData(typeof(int), "1").Returns(1);
            yield return new TestCaseData(typeof(string), "\"Hello\"").Returns("Hello");
            yield return new TestCaseData(typeof(string), "\"\\\" \\\\ Hello \\\\ \n\t World \\\"\"").Returns("\" \\ Hello \\ \n\t World \"");
            yield return new TestCaseData(typeof(string), "").Returns(null);
            yield return new TestCaseData(typeof(string), "null").Returns(null);
            yield return new TestCaseData(typeof(Guid), "da603ef1-6f9f-4303-bc09-3feb947d3ee3").Returns(Guid.Parse("da603ef1-6f9f-4303-bc09-3feb947d3ee3"));
            yield return new TestCaseData(typeof(Guid), "\"97ab4bef-8089-4bac-a37e-959e86895758\"").Returns(Guid.Parse("97ab4bef-8089-4bac-a37e-959e86895758"));
            yield return new TestCaseData(typeof(DateTime), "\"2024-01-11T14:15:16+05:00\"").Returns(new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Local));
            yield return new TestCaseData(typeof(DateTime), "\"2024-01-11T14:15:16\"").Returns(new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Unspecified));
            yield return new TestCaseData(typeof(DateTime), "\"2024-01-11T14:15:16Z\"").Returns(new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Utc));
            yield return new TestCaseData(typeof(TimeSpan), "\"1.02:03:04.0050060\"").Returns(new TimeSpan(1, 2, 3, 4, 5, 6));
            yield return new TestCaseData(typeof(int[]), "[1,2]").Returns(new[] { 1, 2 });
            yield return new TestCaseData(typeof(IEnumerable<int>), "[1,2]").Returns(new[] { 1, 2 });
            yield return new TestCaseData(typeof(List<int>), "[1,2]").Returns(new List<int> { 1, 2 });
            yield return new TestCaseData(typeof(List<List<int>>), "[[1,2],[3,4]]").Returns(new List<List<int>> { new() { 1, 2 }, new() { 3, 4} });
            yield return new TestCaseData(typeof(HashSet<int>), "[1,2]").Returns(new HashSet<int> { 1, 2 });
            yield return new TestCaseData(typeof(int[][]), "[[1,2],[3,4]]").Returns(new[] { new[] { 1, 2 }, new[] { 3, 4 } });
            yield return new TestCaseData(typeof((string, int)), "{\"Item1\":\"A\",\"Item2\":1}").Returns(("A", 1));
            yield return new TestCaseData(typeof(Dictionary<int, int>), "{\"1\":2,\"2\":3}").Returns(new Dictionary<int, int> { [1] = 2, [2] = 3 });
            yield return new TestCaseData(typeof(Dictionary<string, int>), "{\"A\":1,\"B\":2}").Returns(new Dictionary<string, int> { ["A"] = 1, ["B"] = 2 });
            yield return new TestCaseData(typeof(Dictionary<int, string>), "{\"1\":\"A\",\"2\":\"B\"}").Returns(new Dictionary<int, string> { [1] = "A", [2] = "B" });
            yield return new TestCaseData(typeof(Dictionary<int, string>), "{\"1\":\"A\",\"2\":\"B\"}").Returns(new Dictionary<int, string> { [1] = "A", [2] = "B" });
            yield return new TestCaseData(typeof(Dictionary<int, Dictionary<int, int>>), "{\"1\":{\"2\":3,\"3\":4},\"2\":{\"3\":4}}").Returns(new Dictionary<int, Dictionary<int, int>> { [1] = new() { [2] = 3, [3] = 4 }, [2] = new() { [3] = 4 } });
            yield return new TestCaseData(typeof(NestedClass), "{Enum:1,Integer:5}").Returns(new NestedClass { Enum = TestEnum.Value3, Integer = 5});
            yield return new TestCaseData(typeof(NestedClass), "{\"Enum\":1,\"Integer\":5}").Returns(new NestedClass { Enum = TestEnum.Value3, Integer = 5});
            yield return new TestCaseData(typeof(TestClass), "{\"NestedClass\":{\"Bool\":true,\"Double\":3.141592653589793,\"Integer\":5,\"DateTime\":\"2024-01-11T14:15:16+05:00\",\"Time\":\"1.02:03:04.0050060\",\"HashSet\":[1,2,3],\"StringArray\":[\"A\",\"B\",\"C\"],\"StringEnumerable\":[\"K\",\"L\",\"M\"],\"StringList\":[\"X\",\"Y\",\"Z\"],\"Dictionary\":{\"1\":\"One\",\"2\":\"Two\",\"3\":\"Three\"}},\"Classes\":[{},{\"Enum\":-1},{\"Integer\":0},{\"Integer\":1,\"Enum\":1}],\"ClassesDictionary\":{\"0\":{\"Integer\":0},\"1\":{\"Integer\":1},\"2\":{\"Integer\":2}},\"String\":\"Hello\"}").Returns(
                new TestClass
                {
                    String = "Hello",
                    NestedClass = new NestedClass
                    {
                        Bool = true,
                        Double = Math.PI,
                        Integer = 5,
                        DateTime = new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Local),
                        Time = new TimeSpan(days: 1, hours: 2, minutes: 3, seconds: 4, milliseconds: 5, microseconds: 6),
                        HashSet = new HashSet<int> { 1, 2, 3 },
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
                        new NestedClass { Integer = 0,    Enum = TestEnum.Value2 },
                        new NestedClass { Integer = 1,    Enum = TestEnum.Value3 },
                    },
                    ClassesDictionary = new Dictionary<int, NestedClass>
                    {
                        [0] = new() { Integer = 0 },
                        [1] = new() { Integer = 1 },
                        [2] = new() { Integer = 2 },
                    }
                });
        }
        
        public static IEnumerable<TestCaseData> ToJsonCases()
        {
            yield return new TestCaseData(true).Returns("true");
            yield return new TestCaseData(1).Returns("1");
            yield return new TestCaseData((Text: "Hello", Value: 1)).Returns("{\"Item1\":\"Hello\",\"Item2\":1}");
            yield return new TestCaseData(("Hello", new List<int> { 1, 2 })).Returns("{\"Item1\":\"Hello\",\"Item2\":[1,2]}");
            yield return new TestCaseData(1.2).Returns("1.2");
            yield return new TestCaseData("Hello").Returns("\"Hello\"");
            yield return new TestCaseData("\"Hello\"").Returns("\"\\\"Hello\\\"\"");
            yield return new TestCaseData("\"'Hello'\" \\ \n\t World").Returns("\"\\\"'Hello'\\\" \\\\ \\n\\t World\"");
            yield return new TestCaseData(Guid.Parse("da603ef1-6f9f-4303-bc09-3feb947d3ee3")).Returns("\"da603ef1-6f9f-4303-bc09-3feb947d3ee3\"");
            yield return new TestCaseData(TestEnum.Value1).Returns("-1");
            yield return new TestCaseData(TestEnum.Value2).Returns("0");
            yield return new TestCaseData(new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Local)).Returns("\"2024-01-11T14:15:16+05:00\"");
            yield return new TestCaseData(new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Unspecified)).Returns("\"2024-01-11T14:15:16\"");
            yield return new TestCaseData(new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Utc)).Returns("\"2024-01-11T14:15:16Z\"");
            yield return new TestCaseData(new TimeSpan(1, 2, 3, 4, 5, 6)).Returns("\"1.02:03:04.0050060\"");
            yield return new TestCaseData(new[] { 1, 2 }).Returns("[1,2]");
            yield return new TestCaseData(new List<(string S, int I)> { ("A", 1), ("B", 2) }).Returns("[{\"Item1\":\"A\",\"Item2\":1},{\"Item1\":\"B\",\"Item2\":2}]");
            yield return new TestCaseData(new List<List<int>> { new() { 1, 2 }, new() { 3, 4 } }).Returns("[[1,2],[3,4]]");
            yield return new TestCaseData(new Dictionary<int, int> { [1] = 2, [2] = 3 }).Returns("{\"1\":2,\"2\":3}");
            yield return new TestCaseData(new Dictionary<int, Dictionary<int, int>> { [1] = new() { [2] = 3, [3] = 4 }, [2] = new() { [3] = 4 } }).Returns("{\"1\":{\"2\":3,\"3\":4},\"2\":{\"3\":4}}");
            yield return new TestCaseData(new Dictionary<int, string> { [1] = "A", [2] = "B" }).Returns("{\"1\":\"A\",\"2\":\"B\"}");
            yield return new TestCaseData(new Dictionary<string, string> { ["a"] = "\"A\"" }).Returns("{\"a\":\"\\\"A\\\"\"}");
            yield return new TestCaseData(new NestedClass()).Returns("{}");
            yield return new TestCaseData(new TestClass
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
                Classes = new []
                {
                    new NestedClass(),
                    new NestedClass { Integer = null, Enum = TestEnum.Value1 },
                    new NestedClass { Integer = 0,    Enum = TestEnum.Value2 },
                    new NestedClass { Integer = 1,    Enum = TestEnum.Value3 },
                },
                ClassesDictionary = new Dictionary<int, NestedClass>
                {
                    [0] = new() { Integer = 0 },
                    [1] = new() { Integer = 1 },
                    [2] = new() { Integer = 2 },
                }
            }).Returns("{\"NestedClass\":{\"Bool\":true,\"Double\":3.141592653589793,\"Integer\":5,\"DateTime\":\"2024-01-11T14:15:16+05:00\",\"Time\":\"1.02:03:04.0050060\",\"StringArray\":[\"A\",\"B\",\"C\"],\"StringEnumerable\":[\"K\",\"L\",\"M\"],\"StringList\":[\"X\",\"Y\",\"Z\"],\"Dictionary\":{\"1\":\"One\",\"2\":\"Two\",\"3\":\"Three\"}},\"Classes\":[{},{\"Enum\":-1},{\"Integer\":0},{\"Integer\":1,\"Enum\":1}],\"ClassesDictionary\":{\"0\":{\"Integer\":0},\"1\":{\"Integer\":1},\"2\":{\"Integer\":2}},\"String\":\"Hello\"}");
        }
    }
}