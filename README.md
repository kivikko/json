# Kivikko.Json

The Kivikko.Json is designed for quick and easy conversion of .NET objects to JSON and vice versa.

This project was developed to address scenarios where the use of [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) or [System.Text.Json](https://www.nuget.org/packages/System.Text.Json) is not feasible.

All serialization and deserialization logic is contained within a single file. This means you can easily copy the [JsonUtils.cs](https://github.com/kivikko/Json/blob/main/Kivikko.Json/JsonUtils.cs) file into any of your existing .NET projects without the need for additional dependencies or packages.

This level of portability can be extremely useful in cases where you may want to limit the number of third-party dependencies in your project, particularly to avoid potential DLL Hell scenarios. DLL Hell can occur when multiple applications on the same system require different versions of the same dependency, which could lead to compatibility issues.

For instance, this setup is applicable for plugins designed for programs that utilize different versions of JSON converters, which may be incompatible with each other across various software versions. Or if another plugin with an incompatible (outdated) version of JSON converter is already installed in the software.

Being able to copy the functionality directly into your project circumvents this issue, as the functionality will always align with the version of the project it's copied into.

## Supported Types

The `JsonUtils` supports the following .NET types for serialization and deserialization:
- Basic value types (`int`, `double`, `bool`, etc.)
- `string`
- `GUID`
- Enumerations (`enum`)
- Temporal types (`DateTime`, `TimeSpan`)
- Tuples (for instance: `(T1,T2)`)
- Collections implementing `IEnumerable` interface (for instance: `List<T>`, `T[]`, etc.)
- Dictionaries implementing `IDictionary` interface (for instance: `Dictionary<TKey, TValue>`)
- `HashSet<T>`
- Any custom user types (`class`, `struct`) with public properties and/or fields

Please, be aware that the library will attempt to serialize public properties and fields of your custom types.
If a type is not listed above, it is not guaranteed to be properly serialized/deserialized by the `JsonUtils` class.

## Usage

#### Serialization
```csharp
// Converting an object to a JSON string
var obj = new MyObject();
var json = JsonUtils.ToJson(obj);
```

#### Deserialization
```csharp
// Converting a JSON string to an object
var json = "{\"Property\":\"Value\"}";
var obj = JsonUtils.FromJson<MyObject>(json);
```

### File I/O
The `JsonUtils` also supports saving and loading directly to and from files.

#### Loading
```csharp
// Tries to load a JSON string from a file and attempts to deserialize it to an object
if (JsonUtils.TryLoad(path, out MyObject myObject))
{
    // The file was successfully loaded and the object was deserialized
}
```

#### Saving
```csharp
// Save your object directly to a file as JSON
JsonUtils.Save(path, myObject);
```
## Performance

While `JsonUtils` provides a standalone and easily integratable JSON serialization and deserialization solution, it is important to note that due to its simplicity, there is a trade-off with performance.

In benchmark tests, the performance of `JsonUtils` was approximately 2 times slower than that of [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) and 2.5 times slower than that of [System.Text.Json](https://www.nuget.org/packages/System.Text.Json).

[BenchmarkDotNet](https://www.nuget.org/packages/BenchmarkDotNet) `v0.13.12`, Windows 11 (10.0.22621.3155/22H2/2022Update/SunValley2)

Intel Core i7-10510U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores

.NET SDK 7.0.100

[Host]     : .NET 7.0.16 (7.0.1624.6629), X64 RyuJIT AVX2

DefaultJob : .NET 7.0.16 (7.0.1624.6629), X64 RyuJIT AVX2

| Method                        | Mean      |  Ratio | Error    | StdDev   |
|------------------------------ |----------:|-------:|---------:|---------:|
| SystemTextJsonSerialization   |  16.41 ms |      1 | 0.146 ms | 0.129 ms |
| NewtonsoftJsonSerialization   |  28.85 ms |   1.76 | 0.527 ms | 0.586 ms |
| JsonUtilsSerialization        |  46.98 ms |   2.86 | 0.932 ms | 1.179 ms |
| SystemTextJsonDeserialization |  51.04 ms |      1 | 0.994 ms | 1.458 ms |
| NewtonsoftJsonDeserialization |  58.06 ms |   1.14 | 1.126 ms | 1.157 ms |
| JsonUtilsDeserialization      | 113.66 ms |   2.23 | 2.205 ms | 2.708 ms |


###### Legends
- Mean   : Arithmetic mean of all measurements
- Ratio  : The ratio of the current mean to the fastest mean
- Error  : Half of 99.9% confidence interval
- StdDev : Standard deviation of all measurements
- 1 ms   : 1 Millisecond (0.001 sec)

## Dependencies

This project uses the following NuGet packages in the Test and Benchmark projects:

- [BenchmarkDotNet](https://www.nuget.org/packages/BenchmarkDotNet)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)
- [Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
- [NUnit](https://www.nuget.org/packages/NUnit)
- [System.Text.Json](https://www.nuget.org/packages/System.Text.Json)

All packages listed are distributed under their own licenses.

## License

This project is licensed under the terms of the MIT license. See [LICENSE](LICENSE) for more details.
