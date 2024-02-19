# Kivikko.Json

The Kivikko.Json library is designed for quick and easy conversion of .NET objects to JSON and vice versa.
This project was developed to address scenarios where the use of Newtonsoft.Json or System.Text.Json is not feasible.
For instance, it's applicable for plugins designed for programs that utilize different versions of JSON converters, which may be incompatible with each other across various software versions.

## Supported Types

The library supports the following .NET types for serialization and deserialization:
- Basic value types (`int`, `double`, `bool`, etc.)
- `string`
- Collections implementing `IEnumerable` interface (for instance: `List<T>`, `T[]`, etc.)
- Dictionaries implementing `IDictionary` interface (for instance: `Dictionary<TKey, TValue>`)
- Any custom user types (`class`, `struct`) with public properties and/or fields
- Temporal types (`DateTime`, `TimeSpan`)
- Enumerations (`enum`)

Please, be aware that the library will attempt to serialize public properties and fields of your custom types.
If a type is not listed above, it is not guaranteed to be properly serialized/deserialized by the JsonUtils library.

## Usage

``` c#
// Converting an object to a JSON string
var obj = new MyObject();
string json = JsonUtils.ToJson(obj);
```

``` c#
// Converting a JSON string to an object
string json = "{\"Property\":\"Value\"}";
MyObject obj = JsonUtils.FromJson<MyObject>(json);
```
