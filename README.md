# Kivikko.Json

The Kivikko.Json is designed for quick and easy conversion of .NET objects to JSON and vice versa.

This project was developed to address scenarios where the use of `Newtonsoft.Json` or `System.Text.Json` is not feasible.

All serialization and deserialization logic is contained within a single file. This means you can easily copy the `JsonUtils` file into any of your existing .NET projects without the need for additional dependencies or packages.

This level of portability can be extremely useful in cases where you may want to limit the number of third-party dependencies in your project, particularly to avoid potential DLL Hell scenarios. DLL Hell can occur when multiple applications on the same system require different versions of the same dependency, which could lead to compatibility issues.

For instance, this setup is applicable for plugins designed for programs that utilize different versions of JSON converters, which may be incompatible with each other across various software versions.

Being able to copy the functionality directly into your project circumvents this issue, as the functionality will always align with the version of the project it's copied into.

## Supported Types

The `JsonUtils` supports the following .NET types for serialization and deserialization:
- Basic value types (`int`, `double`, `bool`, etc.)
- `string`
- Collections implementing `IEnumerable` interface (for instance: `List<T>`, `T[]`, etc.)
- Dictionaries implementing `IDictionary` interface (for instance: `Dictionary<TKey, TValue>`)
- Any custom user types (`class`, `struct`) with public properties and/or fields
- Temporal types (`DateTime`, `TimeSpan`)
- Enumerations (`enum`)

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
