# Kivikko.Json

The Kivikko.Json is designed for quick and easy conversion of .NET objects to JSON and vice versa.

This project was developed to address scenarios where the use of `Newtonsoft.Json` or `System.Text.Json` is not feasible.

For instance, it's applicable for plugins designed for programs that utilize different versions of JSON converters, which may be incompatible with each other across various software versions.

All serialization and deserialization logic is contained within a single file. This means you can easily copy the `JsonUtils` file into any of your existing .NET projects without the need for additional dependencies or packages.

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
