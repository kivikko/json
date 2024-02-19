# Kivikko.Json

The Kivikko.Json library is designed for quick and easy conversion of .NET objects to JSON and vice versa.
This project was developed to address scenarios where the use of Newtonsoft.Json or System.Text.Json is not feasible.
For instance, it's applicable for plugins designed for programs that utilize different versions of JSON converters, which may be incompatible with each other across various software versions.

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
