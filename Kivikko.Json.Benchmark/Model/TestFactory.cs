namespace Kivikko.Json.Benchmark.Model;

public static class TestFactory
{
    public enum InstanceType
    {
        ArrayInt = 1,
        ArrayString,
        DictionaryIntString,
        DeepTestClass,
        MainTestObject,
        ArrayTestClass,
        ArrayMainTestObject,
    }

    private static readonly Random Random = new();
    
    public static object? CreateInstance(InstanceType instanceType) => instanceType switch
    {
        InstanceType.ArrayInt            => CreateArray<int>(1000),
        InstanceType.ArrayString         => CreateArray<string>(1000),
        InstanceType.DictionaryIntString => CreateDictionary<int, string>(1000),
        InstanceType.DeepTestClass       => CreateDeepTestClass(),
        InstanceType.MainTestObject      => CreateMainTestObject(),
        InstanceType.ArrayTestClass      => CreateArray<TestClass>(100),
        InstanceType.ArrayMainTestObject => CreateArray<MainTestClass>(10),
        _ => null
    };

    private static TestClass? CreateDeepTestClass()
    {
        Console.WriteLine("Input depth:");
        var line = Console.ReadLine();
        return int.TryParse(line, out var depth) ? CreateDeepTestClass(depth) : default;
    }
    
    private static TestClass CreateDeepTestClass(int depth)
    {
        var testClass = CreateTestClass(size: 2);
        if (depth > 0) testClass.NestedClass = CreateDeepTestClass(depth - 1);
        return testClass;
    }

    private static MainTestClass CreateMainTestObject() => new()
    {
        NestedClass = CreateTestClass(size: 100),
        NestedClasses = CreateArray<TestClass>(count: 20),
        ClassesDictionary = CreateDictionary<string, TestClass>(count: 20)
    };

    private static TestClass CreateTestClass(int size = 10) => new()
    {
        Bool = Random.Next(2) % 2 is 0,
        Double = Random.Next(1000) / Math.PI,
        Integer = Random.Next(1000),
        DateTime = DateTime.Now,
        Time = new TimeSpan(days: Random.Next(100), hours: Random.Next(24), minutes: Random.Next(60), seconds: Random.Next(60), milliseconds: Random.Next(1000), microseconds: Random.Next(1000)),
        StringArray = CreateArray<string>(size),
        StringEnumerable = CreateArray<string>(size),
        Dictionary = CreateDictionary<string, string>(size / 2),
    };

    private static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(int count)
        where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>();
        for (var i = 0; i < count; i++)
        {
            if (!dictionary.TryAdd(New<TKey>(), New<TValue>()))
                i--;
        }
        return dictionary;
    }

    private static T[] CreateArray<T>(int count)
    {
        var array = new T[count];
        for (var i = 0; i < count; i++) array[i] = New<T>();
        return array;
    }

    private static T New<T>() => (T)NewObject<T>();
    private static object NewObject<T>()
    {
        var type = typeof(T);
        
        if (type == typeof(int))
            return Random.Next(10000);
        
        if (type == typeof(string))
            return Guid.NewGuid().ToString();
        
        if (type == typeof(double))
            return Random.Next(10000) / 100;
        
        if (type == typeof(Guid))
            return Guid.NewGuid();

        if (type == typeof(TestClass))
            return CreateTestClass();

        if (type == typeof(MainTestClass))
            return CreateMainTestObject();

        throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }
}