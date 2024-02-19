using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kivikko.Json;

public static class JsonUtils
{
    public static string ToJson(object obj)
    {
        if (obj is null) return "null";
        var type = obj.GetType();
        if (type.IsValueType) return ToString(obj);
        
        switch (obj)
        {
            case string str: return $"\"{str}\"";
            case IDictionary dictionary: return ToString(dictionary);
            case IEnumerable enumerable: return ToString(enumerable);
        }
        
        var jsonProperties = type
            .GetProperties()
            .Where(x => x.CanWrite)
            .Select(p => new { p.Name, p.PropertyType, Value = p.GetValue(obj) })
            .Where(p => p.Value is not null && !IsDefaultValue(p.PropertyType, p.Value))
            .Select(p =>
            {
                var x = $"\"{p.Name}\":{(p.PropertyType.IsPrimitive ? ToString(p.Value) : ToJson(p.Value))}";
                Console.WriteLine(x);
                return x;
            });

        Console.WriteLine();
        
        return $"{{{string.Join(",", jsonProperties)}}}";
    }
    
    // ReSharper disable once UnusedMember.Local
    private static bool IsDefaultValue(object obj) => IsDefaultValue(obj.GetType(), obj);
    private static bool IsDefaultValue(Type type, object obj)
    {
        if (obj is null) return true;
        if (type.IsValueType && obj.Equals(Activator.CreateInstance(type))) return true;
        return type == typeof(string) && string.IsNullOrEmpty((string)obj);
    }
    
    private static string ToString(object obj) => obj switch
    {
        bool     b => b.ToString().ToLower(),
        decimal  d => d.ToString(CultureInfo.InvariantCulture),
        double   d => d.ToString(CultureInfo.InvariantCulture),
        float    f => f.ToString(CultureInfo.InvariantCulture),
        Enum     e => Convert.ToInt32(e).ToString(),
        DateTime { Kind: DateTimeKind.Local }       d => $"\"{d:yyyy-MM-ddTHH:mm:sszzz}\"",
        DateTime { Kind: DateTimeKind.Unspecified } d => $"\"{d:yyyy-MM-ddTHH:mm:ss}\"",
        DateTime { Kind: DateTimeKind.Utc }         d => $"\"{d:yyyy-MM-ddTHH:mm:ss}Z\"",
        TimeSpan t => $"\"{t:d\\.hh\\:mm\\:ss\\.fffffff}\"",
        ITuple   t => ToString(t),
        _ => obj.ToString()
    };

    private static string ToString(IDictionary dictionary) => dictionary
        .Keys.Cast<object>()
        .Aggregate("{", (current, key) => current + $"\"{key}\":{ToJson(dictionary[key])},").TrimEnd(',') + "}";

    private static string ToString(IEnumerable enumerable) => enumerable
        .Cast<object>()
        .Aggregate("[", (current, x) => current + ToJson(x) + ",").TrimEnd(',') + "]";

    private static string ToString(ITuple tuple)
    {
        var toString = new string[tuple.Length];
        
        for (var i = 0; i < tuple.Length; i++)
            toString[i] = $"\"Item{i + 1}\":{ToJson(tuple[i])}";
        
        return $"{{{string.Join(",", toString)}}}";
    }

    public static T FromJsonOrNew<T>(string json)
        where T : class, new() => !string.IsNullOrWhiteSpace(json)
        ? FromJson<T>(json)
        : new T();

    public static T FromJson<T>(string json) =>
        FromJson(json, typeof(T)) is T obj ? obj : default;

    public static object FromJson(string json, Type type) =>
        json is null ? default :
        type.IsValueType || type == typeof(string) ? GetNullableValueFromJson(json, type) :
        type.GetInterfaces().Any(x => x == typeof(IDictionary)) ? GetDictionaryFromJson(json, type) :
        type.GetInterfaces().Any(x => x == typeof(IEnumerable)) ? GetEnumerableFromJson(json, type) :
        GetObjectFromJson(json, type);

    private static object GetNullableValueFromJson(string json, Type type) => type.IsNullableType()
        ? json is not (null or "null" or "")
            ? GetValueFromJson(json, type.GenericTypeArguments[0])
            : default
        : GetValueFromJson(json, type);

    private static object GetValueFromJson(string json, Type type) =>
        ParseDictionary.TryGetValue(type, out var valueFunc) ? valueFunc(json) :
        typeof(Enum) == type          ? GetEnumFromJson(json, type) :
        typeof(Enum) == type.BaseType ? GetEnumFromJson(json, type) :
        typeof(DateTime) == type      ? GetDateTimeFromJson(json) :
        typeof(TimeSpan) == type      ? GetTimeSpanFromJson(json) :
        GetObjectFromJson(json, type);

    private static Dictionary<Type, Func<string, object>> _parseDictionary;
    private static Dictionary<Type, Func<string, object>> ParseDictionary => _parseDictionary ??= new Dictionary<Type, Func<string, object>>
    {
        [typeof(bool)]     = json => bool   .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(byte)]     = json => byte   .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(sbyte)]    = json => sbyte  .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(char)]     = json => char   .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(decimal)]  = json => decimal.TryParse(json.Trim('"'), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : default,
        [typeof(double)]   = json => double .TryParse(json.Trim('"'), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : default,
        [typeof(float)]    = json => float  .TryParse(json.Trim('"'), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : default,
        [typeof(int)]      = json => int    .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(uint)]     = json => uint   .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(long)]     = json => long   .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(ulong)]    = json => ulong  .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(short)]    = json => short  .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(ushort)]   = json => ushort .TryParse(json.Trim('"'), out var value) ? value : default,
        [typeof(string)]   = json => json is null or "null" or "" ? null : json.Trim('"'),
    };

    private static IDictionary GetDictionaryFromJson(string json, Type type)
    {
        var dictionary = Activator.CreateInstance(type) as IDictionary;
        
        if (dictionary is null || string.IsNullOrWhiteSpace(json))
            return dictionary;
        
        var keyType   = type.GenericTypeArguments.ElementAtOrDefault(0);
        var valueType = type.GenericTypeArguments.ElementAtOrDefault(1);
        var values    = GetValues(json);
        
        foreach (var x in values)
        {
            var key   = FromJson(x.Key,   keyType);
            var value = FromJson(x.Value, valueType);
            dictionary.Add(key, value);
        }

        return dictionary;
    }
    
    private static object GetEnumFromJson(string json, Type type) =>
        Enum.ToObject(type, int.TryParse(json.Trim('"'), out var value) ? value : 0);

    private static object GetDateTimeFromJson(string json)
    {
        var str = json.Trim('"');
        var dateTime = DateTime.TryParse(json.Trim('"'), out var value) ? value : new DateTime();
        return str.EndsWith("Z", StringComparison.InvariantCultureIgnoreCase)
            ? dateTime.ToUniversalTime()
            : dateTime;
    }

    private static object GetTimeSpanFromJson(string json) =>
        TimeSpan.TryParse(json.Trim('"'), out var value) ? value : new TimeSpan();

    private static object GetObjectFromJson(string json, Type type)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        var values   = GetValues(json);
        var instance = Activator.CreateInstance(type);
        
        foreach (var valuePair in values)
        {
            Type propertyType;
            Action<object> setValue;

            if (type.GetProperty(valuePair.Key, BindingFlags.Public | BindingFlags.Instance) is { } property)
            {
                propertyType = property.PropertyType;
                setValue = propertyValue => property.SetValue(instance, propertyValue);
            }

            else if (type.GetField(valuePair.Key, BindingFlags.Public | BindingFlags.Instance) is { } filed)
            {
                propertyType = filed.FieldType;
                setValue = propertyValue => filed.SetValue(instance, propertyValue);
            }

            else
            {
                continue;
            }
            
            var value = FromJson(valuePair.Value, propertyType);

            if (propertyType.IsArray &&
                propertyType.GetElementType() is { } arrayElementType &&
                value is Array valueArray)
            {
                var typedArray = Array.CreateInstance(arrayElementType, valueArray.Length);
                valueArray.CopyTo(typedArray, 0);
                setValue(typedArray);
            }
            
            else if (
                propertyType.IsGenericType &&
                propertyType.GetInterfaces().Any(i => i == typeof(IDictionary)) &&
                value is IDictionary valueDictionary)
            {
                var genericArgs = propertyType.GetGenericArguments();
                var keyType   = genericArgs[0];
                var valueType = genericArgs[1];
                var typedDictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
                foreach (DictionaryEntry entry in valueDictionary) typedDictionary.Add(entry.Key, entry.Value);
                setValue(typedDictionary);
            }
            
            else if (
                propertyType.IsGenericType &&
                propertyType.GetInterfaces().Any(i => i == typeof(IEnumerable)) &&
                propertyType.GetGenericArguments()[0] is { } listElementType &&
                value is ICollection valueCollection)
            {
                var typedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listElementType));
                foreach (var v in valueCollection) typedList.Add(v);
                setValue(typedList);
            }
            
            else
            {
                setValue(value);
            }
        }
        
        return instance;
    }

    private static IEnumerable GetEnumerableFromJson(string json, Type type)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<object>();
        
        var list = new List<object>();
        var stringValue = string.Empty;
        var genericType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments.FirstOrDefault();
        var inQuotes = false;
        var brackets = 0;
        
        for (var i = 1; i < json.Length - 1; i++)
        {
            var c = json[i];
            
            switch (c)
            {
                case '"' when json[i - 1] is not '\\': inQuotes = !inQuotes; stringValue += c; break;
                case '{' when !inQuotes: brackets++; stringValue += c; break;
                case '}' when !inQuotes: brackets--; stringValue += c; break;
                case '[' when !inQuotes: brackets++; stringValue += c; break;
                case ']' when !inQuotes: brackets--; stringValue += c; break;
                
                case ',' when !inQuotes && brackets is 0:
                    list.Add(FromJson(stringValue, genericType));
                    stringValue = string.Empty;
                    break;
                
                default:
                    stringValue += c;
                    break;
            }
        }
        
        if (!string.IsNullOrWhiteSpace(stringValue) && !inQuotes && brackets is 0)
            list.Add(FromJson(stringValue, genericType));
        
        return type.BaseType == typeof(Array) ? list.ToArray() : list;
    }

    private static Dictionary<string, string> GetValues(string json)
    {
        var values = new Dictionary<string, string>();
        var key    = string.Empty;
        var value  = string.Empty;
        var isKeyReading    = false;
        var isValueReading  = false;
        var isValueInQuotes = false;
        var brackets = 0;
        
        for (var i = 1; i < json.Length - 1; i++)
        {
            var c = json[i];
            
            switch (c)
            {
                case '[' when isValueReading: value += c; brackets++; break;
                case ']' when isValueReading: value += c; brackets--; break;
                case '{' when isValueReading: value += c; brackets++; break;
                case '}' when isValueReading: value += c; brackets--; break;
                
                case '"' when json[i - 1] is not '\\':
                    switch (isKeyReading, isValueReading)
                    {
                        case (false, false): isKeyReading = true;  break;
                        case (true, false):  isKeyReading = false; break;
                        case (false, true):  value += c; isValueInQuotes = !isValueInQuotes; break;
                    }
                    break;
                
                case ':' when !isValueReading && brackets is 0:
                    isValueReading = true;
                    break;
                
                case ',' when isValueReading && brackets is 0 && !isValueInQuotes:
                    isValueReading = false;
                    values.Add(key, value);
                    key   = string.Empty;
                    value = string.Empty;
                    break;
                
                default:
                    if (isKeyReading) key += c;
                    else if (isValueReading) value += c;
                    break;
            }
        }
        
        if (!string.IsNullOrWhiteSpace(key))
            values.Add(key, value);
        
        return values;
    }

    public static bool IsNullableType(this Type type) => type.IsGenericType(typeof(Nullable<>));
    public static bool IsGenericType(this Type type, Type genericType) => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
    
    public static bool TryLoad<T>(string path, out T value)
        where T : class, new() =>
        TryLoad(path, Encoding.Default, out value);

    public static bool TryLoad<T>(string path, Encoding encoding, out T value)
        where T : class, new()
    {
        if (!File.Exists(path))
        {
            value = new T();
            return false;
        }
        
        var json = File.ReadAllText(path, encoding);
        var deserializeObject = FromJson<T>(json);
        value = deserializeObject ?? new T();
        
        return deserializeObject is not null;
    }
    
    public static void Save(string path, object obj) =>
        Save(path, ToJson(obj), Encoding.Default);
    
    public static void Save(string path, object obj, Encoding encoding) =>
        WriteAllTextIfDifferent(path, ToJson(obj), encoding);
    
    private static void WriteAllTextIfDifferent(string path, string content, Encoding encoding)
    {
        if (File.Exists(path))
        {
            var newContentHashCode = content.GetHashCode();
            var oldContentHashCode = File.ReadAllText(path, encoding).GetHashCode();
            
            if (newContentHashCode == oldContentHashCode)
                return;
        }
        
        CreateDirectoryIfNotExist(path);
        File.WriteAllText(path, content, encoding);
    }

    private static void CreateDirectoryIfNotExist(string path)
    {
        var directory = Path.GetDirectoryName(path);

        if (directory is not null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }
}