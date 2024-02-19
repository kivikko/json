namespace Kivikko.Json.Tests;

public class JsonUtilsTests
{
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
            yield return new TestCaseData(typeof(string), "").Returns(null);
            yield return new TestCaseData(typeof(string), "null").Returns(null);
            yield return new TestCaseData(typeof(DateTime), "\"2024-01-11T14:15:16+05:00\"").Returns(new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Local));
            yield return new TestCaseData(typeof(DateTime), "\"2024-01-11T14:15:16\"").Returns(new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Unspecified));
            yield return new TestCaseData(typeof(DateTime), "\"2024-01-11T14:15:16Z\"").Returns(new DateTime(2024, 01, 11, 14, 15, 16, DateTimeKind.Utc));
            yield return new TestCaseData(typeof(TimeSpan), "\"1.02:03:04.0050060\"").Returns(new TimeSpan(1, 2, 3, 4, 5, 6));
            yield return new TestCaseData(typeof(int[]), "[1,2]").Returns(new[] { 1, 2 });
            yield return new TestCaseData(typeof(IEnumerable<int>), "[1,2]").Returns(new[] { 1, 2 });
            yield return new TestCaseData(typeof(List<int>), "[1,2]").Returns(new List<int> { 1, 2 });
            yield return new TestCaseData(typeof(List<List<int>>), "[[1,2],[3,4]]").Returns(new List<List<int>> { new() { 1, 2 }, new() { 3, 4} });
            yield return new TestCaseData(typeof(int[][]), "[[1,2],[3,4]]").Returns(new[] { new[] { 1, 2 }, new[] { 3, 4 } });
            yield return new TestCaseData(typeof((string, int)), "{\"Item1\":\"A\",\"Item2\":1}").Returns(("A", 1));
            yield return new TestCaseData(typeof(Dictionary<int, int>), "{\"1\":2,\"2\":3}").Returns(new Dictionary<int, int> { [1] = 2, [2] = 3 });
            yield return new TestCaseData(typeof(Dictionary<string, int>), "{\"A\":1,\"B\":2}").Returns(new Dictionary<string, int> { ["A"] = 1, ["B"] = 2 });
            yield return new TestCaseData(typeof(Dictionary<int, string>), "{\"1\":\"A\",\"2\":\"B\"}").Returns(new Dictionary<int, string> { [1] = "A", [2] = "B" });
            yield return new TestCaseData(typeof(Dictionary<int, string>), "{\"1\":\"A\",\"2\":\"B\"}").Returns(new Dictionary<int, string> { [1] = "A", [2] = "B" });
            yield return new TestCaseData(typeof(Dictionary<int, Dictionary<int, int>>), "{\"1\":{\"2\":3,\"3\":4},\"2\":{\"3\":4}}").Returns(new Dictionary<int, Dictionary<int, int>> { [1] = new() { [2] = 3, [3] = 4 }, [2] = new() { [3] = 4 } });
            yield return new TestCaseData(typeof(Class1), "{\"Enum\":1,\"Integer\":5}").Returns(new Class1 { Enum = Enum1.Value3, Integer = 5});
            yield return new TestCaseData(typeof(Class2), "{\"Classes\":[{},{\"Enum\":-1},{\"Integer\":0},{\"Integer\":1,\"Enum\":1}]}").Returns(
                new Class2
                {
                    Classes = new[]
                    {
                        new Class1(),
                        new Class1 { Integer = null, Enum = Enum1.Value1 },
                        new Class1 { Integer = 0,    Enum = Enum1.Value2 },
                        new Class1 { Integer = 1,    Enum = Enum1.Value3 },
                    }
                });
            yield return new TestCaseData(typeof(Class2), "{\"Class1\":{\"Bool\":true,\"Double\":3.141592653589793,\"Integer\":5,\"DateTime\":\"2024-01-11T14:15:16+05:00\",\"Time\":\"1.02:03:04.0050060\",\"StringArray\":[\"A\",\"B\",\"C\"],\"StringEnumerable\":[\"K\",\"L\",\"M\"],\"StringList\":[\"X\",\"Y\",\"Z\"],\"Dictionary\":{\"1\":\"One\",\"2\":\"Two\",\"3\":\"Three\"}},\"Classes\":[{},{\"Enum\":-1},{\"Integer\":0},{\"Integer\":1,\"Enum\":1}],\"ClassesDictionary\":{\"0\":{\"Integer\":0},\"1\":{\"Integer\":1},\"2\":{\"Integer\":2}},\"String\":\"Hello\"}").Returns(
                new Class2
                {
                    String = "Hello",
                    Class1 = new Class1
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
                        new Class1(),
                        new Class1 { Integer = null, Enum = Enum1.Value1 },
                        new Class1 { Integer = 0,    Enum = Enum1.Value2 },
                        new Class1 { Integer = 1,    Enum = Enum1.Value3 },
                    },
                    ClassesDictionary = new Dictionary<int, Class1>
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
            yield return new TestCaseData(Enum1.Value1).Returns("-1");
            yield return new TestCaseData(Enum1.Value2).Returns("0");
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
            yield return new TestCaseData(new Class1()).Returns("{}");
            yield return new TestCaseData(new Class2
            {
                String = "Hello",
                Class1 = new Class1
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
                    new Class1(),
                    new Class1 { Integer = null, Enum = Enum1.Value1 },
                    new Class1 { Integer = 0,    Enum = Enum1.Value2 },
                    new Class1 { Integer = 1,    Enum = Enum1.Value3 },
                },
                ClassesDictionary = new Dictionary<int, Class1>
                {
                    [0] = new() { Integer = 0 },
                    [1] = new() { Integer = 1 },
                    [2] = new() { Integer = 2 },
                }
            }).Returns("{\"Class1\":{\"Bool\":true,\"Double\":3.141592653589793,\"Integer\":5,\"DateTime\":\"2024-01-11T14:15:16+05:00\",\"Time\":\"1.02:03:04.0050060\",\"StringArray\":[\"A\",\"B\",\"C\"],\"StringEnumerable\":[\"K\",\"L\",\"M\"],\"StringList\":[\"X\",\"Y\",\"Z\"],\"Dictionary\":{\"1\":\"One\",\"2\":\"Two\",\"3\":\"Three\"}},\"Classes\":[{},{\"Enum\":-1},{\"Integer\":0},{\"Integer\":1,\"Enum\":1}],\"ClassesDictionary\":{\"0\":{\"Integer\":0},\"1\":{\"Integer\":1},\"2\":{\"Integer\":2}},\"String\":\"Hello\"}");
        }
    }
    
    public class Class1
    {
        public bool? Bool { get; set; }
        public double? Double { get; set; }
        public int? Integer { get; set; }
        public Enum1 Enum { get; set; }
        public DateTime? DateTime { get; set; }
        public TimeSpan? Time { get; set; }
        public string[]? StringArray { get; set; }
        public IEnumerable<string>? StringEnumerable { get; set; }
        public List<string>? StringList { get; set; }
        public Dictionary<int, string?>? Dictionary { get; set; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Class1)obj);
        }
        private bool Equals(Class1 other)
        {
            if (Bool != other.Bool) return false;
            if (!Nullable.Equals(Double, other.Double)) return false;
            if (Integer != other.Integer) return false;
            if (Enum != other.Enum) return false;
            if (!Nullable.Equals(DateTime, other.DateTime)) return false;
            if (!Nullable.Equals(Time, other.Time)) return false;
            if (StringArray is null && other.StringArray is null) return true;
            if (StringEnumerable is null && other.StringEnumerable is null) return true;
            if (StringList is null && other.StringList is null) return true;
            if (Dictionary is null && other.Dictionary is null) return true;
            if (StringArray is null || other.StringArray is null || !StringArray.SequenceEqual(other.StringArray)) return false;
            if (StringEnumerable is null || other.StringEnumerable is null || !StringEnumerable.SequenceEqual(other.StringEnumerable)) return false;
            if (StringList is null || other.StringList is null || !StringList.SequenceEqual(other.StringList)) return false;
            if (Dictionary is null || other.Dictionary is null || !Dictionary.SequenceEqual(other.Dictionary)) return false;
            return true;
        }

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Bool);
            hashCode.Add(Double);
            hashCode.Add(Integer);
            hashCode.Add((int)Enum);
            hashCode.Add(DateTime);
            hashCode.Add(Time);
            hashCode.Add(StringArray);
            hashCode.Add(StringEnumerable);
            hashCode.Add(StringList);
            hashCode.Add(Dictionary);
            return hashCode.ToHashCode();
        }
    }
    
    public class Class2
    {
        public Class1? Class1 { get; set; }
        public IEnumerable<Class1>? Classes { get; set; }
        public Dictionary<int, Class1>? ClassesDictionary { get; set; }
        public string? String { get; set; }
        
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Class2)obj);
        }
        private bool Equals(Class2 other)
        {
            if (!Equals(Class1, other.Class1)) return false;
            if (Classes is null && other.Classes is null) return true;
            if (ClassesDictionary is null && other.ClassesDictionary is null) return true;
            if (Classes is null || other.Classes is null || !Classes.SequenceEqual(other.Classes)) return false;
            if (ClassesDictionary is null || other.ClassesDictionary is null || !ClassesDictionary.SequenceEqual(other.ClassesDictionary)) return false;
            return String == other.String;
        }

        public override int GetHashCode() => HashCode.Combine(Class1, Classes, ClassesDictionary, String);
    }

    public enum Enum1
    {
        Value1 = -1,
        Value2,
        Value3
    }
}