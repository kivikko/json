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
    public static T FromJsonOrNew<T>(string json)
        where T : class, new() => !string.IsNullOrWhiteSpace(json)
        ? FromJson<T>(json)
        : new T();

    public static T FromJson<T>(string json) =>
        new JsonReader().ReadFromJson<T>(json);

    public static object FromJson(string json, Type type) =>
        new JsonReader().ReadFromJson(json, type);

    public static string ToJson(object obj, bool ignoreNullOrDefaultValues = true) =>
        new JsonWriter(ignoreNullOrDefaultValues).WriteToJson(obj);

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
    
    public static void Save(string path, object obj, bool ignoreNullOrDefaultValues = true) =>
        Save(path, ToJson(obj, ignoreNullOrDefaultValues), Encoding.Default);
    
    public static void Save(string path, object obj, Encoding encoding, bool ignoreNullOrDefaultValues = true) =>
        WriteAllTextIfDifferent(path, ToJson(obj, ignoreNullOrDefaultValues), encoding);
    
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

    private class JsonReader
    {
        private static readonly char[] Quote = { '"' };
        private readonly StringBuilder _stringBuilder = new();
        private int _index;

        public T ReadFromJson<T>(string json) =>
            ReadFromJson(json, typeof(T)) is T fromJson ? fromJson : default;

        public object ReadFromJson(string json, Type type)
        {
            _index = 0;
            return FromJsonPrivate(json, type);
        }

        private object FromJsonPrivate(string json, Type type)
        {
            if (type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(Guid) ||
                type == typeof(DateTime) ||
                type == typeof(TimeSpan) ||
                IsNullableType(type))
                return GetPrimitiveFromJson(json, type);
            
            if (type.IsValueType)
                return GetObjectFromJson(json, type);
            
            var interfaces = type.GetInterfaces();
            
            if (interfaces.Any(x => x == typeof(IDictionary)))
                return GetDictionaryFromJson(json, type);
                
            if (interfaces.Any(x => x == typeof(IEnumerable)))
                return GetEnumerableFromJson(json, type);
            
            return GetObjectFromJson(json, type);
        }

        private object GetPrimitiveFromJson(string json, Type type)
        {
            var inQuotes = false;
            
            while (_index < json.Length)
            {
                switch (json[_index])
                {
                    case '"': inQuotes = !inQuotes; break;
                    case ']' when !inQuotes: goto quit;
                    case '}' when !inQuotes: goto quit;
                    case ',' when !inQuotes: goto quit;
                    case ':' when !inQuotes: goto quit;
                }
                _stringBuilder.Append(json[_index]);
                _index++;
            }
            quit:
            var stringValue = _stringBuilder.ToString();
            _stringBuilder.Clear();
            return GetNullableValueFromJson(stringValue, type);
        }

        private object GetObjectFromJson(string json, Type type)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            var instance = Activator.CreateInstance(type);
            var isNameReading = false;
            var isValueReading = false;
            
            string memberName = null;
            
            while (_index < json.Length)
            {
                switch (json[_index])
                {
                    case '{' when !isValueReading: _index++; continue;
                    case '"' when !isValueReading && memberName is null: _index++; isNameReading = !isNameReading; continue;
                    case ':': _index++; isValueReading = true; continue;
                    case ',': _index++; continue;
                    case '}': _index++; return instance;
                        
                    default:
                        if (isNameReading)
                        {
                            _stringBuilder.Append(json[_index]);
                            _index++;
                        }
                        else if (_stringBuilder.Length > 0)
                        {
                            memberName = _stringBuilder.ToString();
                            _stringBuilder.Clear();
                        }
                        else if (memberName is not null)
                        {
                            Type propertyType;
                            Action<object> setValue;

                            if (type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance) is { } property)
                            {
                                propertyType = property.PropertyType;
                                setValue = propertyValue => property.SetValue(instance, propertyValue);
                            }

                            else if (type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance) is { } filed)
                            {
                                propertyType = filed.FieldType;
                                setValue = propertyValue => filed.SetValue(instance, propertyValue);
                            }

                            else
                            {
                                _index++;
                                continue;
                            }
                            
                            var value = FromJsonPrivate(json, propertyType);

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
                            
                            memberName = null;
                            isValueReading = false;
                        }
                        else
                        {
                            _index++;
                        }
                        continue;
                }
            }
            
            return instance;
        }
        
        private IDictionary GetDictionaryFromJson(string json, Type type)
        {
            var dictionary = (IDictionary)Activator.CreateInstance(type);
                
            if (string.IsNullOrWhiteSpace(json))
                return dictionary;
            
            var keyType   = type.GenericTypeArguments.ElementAtOrDefault(0);
            var valueType = type.GenericTypeArguments.ElementAtOrDefault(1);
            
            object key = null;
                
            while (_index < json.Length)
            {
                switch (json[_index])
                {
                    case '{': _index++; continue;
                    case '"' when key is null: key = FromJsonPrivate(json, keyType); break;
                    case ':': _index++; continue;
                    case ',': _index++; continue;
                    case '}': _index++; return dictionary;
                        
                    default:
                        if (key is not null)
                        {
                            dictionary.Add(key, FromJsonPrivate(json, valueType));
                            key = null;
                        }
                        else
                        {
                            _index++;
                        }
                        continue;
                }
            }

            return dictionary;
        }

        private IEnumerable GetEnumerableFromJson(string json, Type type)
        {
            var list = new List<object>();
            var elementType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments.FirstOrDefault();
            
            if (elementType is null)
                return Array.Empty<object>();
            
            while (_index < json.Length)
            {
                switch (json[_index])
                {
                    case '[': _index++; continue;
                    case ',': _index++; continue;
                    case ']': _index++; goto quit;
                    default: list.Add(FromJsonPrivate(json, elementType)); break;
                }
            }
            quit:
            
            if (type.IsArray)
            {
                var typedArray = Array.CreateInstance(elementType, list.Count);
                list.ToArray().CopyTo(typedArray, 0);
                return typedArray;
            }
            
            if (type.IsGenericType &&
                type.GetInterfaces().Any(i => i == typeof(IEnumerable)))
            {
                var typedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                foreach (var v in list) typedList.Add(v);
                return typedList;
            }

            return list;
        }

        private static object GetNullableValueFromJson(string json, Type type) => IsNullableType(type)
            ? json is not (null or "null" or "")
                ? GetValueFromJson(json, type.GenericTypeArguments[0])
                : default
            : GetValueFromJson(json, type);

        private static object GetValueFromJson(string json, Type type) =>
            ParseDictionary.TryGetValue(type, out var valueFunc) ? valueFunc(json) :
            type.IsEnum              ? GetEnumFromJson(json, type) :
            typeof(DateTime) == type ? GetDateTimeFromJson(json) :
            typeof(TimeSpan) == type ? GetTimeSpanFromJson(json) :
            Activator.CreateInstance(type);

        private static Dictionary<Type, Func<string, object>> _parseDictionary;
        private static Dictionary<Type, Func<string, object>> ParseDictionary => _parseDictionary ??= new Dictionary<Type, Func<string, object>>
        {
            [typeof(bool)]     = json => bool   .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(byte)]     = json => byte   .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(sbyte)]    = json => sbyte  .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(char)]     = json => char   .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(decimal)]  = json => decimal.TryParse(json.Trim(Quote), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : default,
            [typeof(double)]   = json => double .TryParse(json.Trim(Quote), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : default,
            [typeof(float)]    = json => float  .TryParse(json.Trim(Quote), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : default,
            [typeof(int)]      = json => int    .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(uint)]     = json => uint   .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(long)]     = json => long   .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(ulong)]    = json => ulong  .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(short)]    = json => short  .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(ushort)]   = json => ushort .TryParse(json.Trim(Quote), out var value) ? value : default,
            [typeof(string)]   = json => json is null or "null" or "" ? null : json.Trim(Quote),
            [typeof(Guid)]     = json => Guid   .TryParse(json.Trim(Quote), out var value) ? value : default,
        };
        
        private static object GetEnumFromJson(string json, Type type) =>
            Enum.ToObject(type, int.TryParse(json.Trim(Quote), out var value) ? value : 0);

        private static DateTime GetDateTimeFromJson(string json)
        {
            var str = json.Trim(Quote);
            var dateTime = DateTime.TryParse(json.Trim(Quote), out var value) ? value : new DateTime();
            return str.EndsWith("Z", StringComparison.InvariantCultureIgnoreCase)
                ? dateTime.ToUniversalTime()
                : dateTime;
        }

        private static TimeSpan GetTimeSpanFromJson(string json) =>
            TimeSpan.TryParse(json.Trim(Quote), out var value) ? value : new TimeSpan();
        
        private static bool IsNullableType(Type type) => IsGenericType(type, typeof(Nullable<>));
        private static bool IsGenericType(Type type, Type genericType) => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
    }

    private class JsonWriter
    {
        private readonly bool _ignoreNullOrDefaultValues;
        private readonly StringBuilder _stringBuilder = new();

        public JsonWriter(bool ignoreNullOrDefaultValues)
        {
            _ignoreNullOrDefaultValues = ignoreNullOrDefaultValues;
        }

        public string WriteToJson(object obj)
        {
            if (obj is null) return "null";
            var type = obj.GetType();
            if (type.IsValueType) return ToString(obj);
            
            switch (obj)
            {
                case string str: return $"\"{str}\"";
                case IDictionary dictionary: return new JsonWriter(_ignoreNullOrDefaultValues).ToJson(dictionary);
                case IEnumerable enumerable: return new JsonWriter(_ignoreNullOrDefaultValues).ToJson(enumerable);
            }

            var hasContent = false;

            _stringBuilder.Append("{");
            
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite) continue;
                var value = property.GetValue(obj);
                if (_ignoreNullOrDefaultValues && (value is null || IsDefaultValue(property.PropertyType, value))) continue;
                _stringBuilder.Append("\"");
                _stringBuilder.Append(property.Name);
                _stringBuilder.Append("\":");
                _stringBuilder.Append(new JsonWriter(_ignoreNullOrDefaultValues).WriteToJson(value));
                _stringBuilder.Append(",");
                hasContent = true;
            }

            if (hasContent)
                _stringBuilder.Remove(_stringBuilder.Length - 1, 1);
            
            _stringBuilder.Append("}");

            return _stringBuilder.ToString();
        }
        
        private static bool IsDefaultValue(Type type, object obj)
        {
            if (obj is null) return true;
            if (type.IsValueType && obj.Equals(Activator.CreateInstance(type))) return true;
            return type == typeof(string) && string.IsNullOrEmpty((string)obj);
        }
        
        private string ToString(object obj) => obj switch
        {
            bool     b => b.ToString().ToLower(),
            decimal  d => d.ToString(CultureInfo.InvariantCulture),
            double   d => d.ToString(CultureInfo.InvariantCulture),
            float    f => f.ToString(CultureInfo.InvariantCulture),
            Guid     g => $"\"{g.ToString()}\"",
            Enum     e => Convert.ToInt32(e).ToString(),
            DateTime { Kind: DateTimeKind.Local }       d => $"\"{d:yyyy-MM-ddTHH:mm:sszzz}\"",
            DateTime { Kind: DateTimeKind.Unspecified } d => $"\"{d:yyyy-MM-ddTHH:mm:ss}\"",
            DateTime { Kind: DateTimeKind.Utc }         d => $"\"{d:yyyy-MM-ddTHH:mm:ss}Z\"",
            TimeSpan t => $"\"{t:d\\.hh\\:mm\\:ss\\.fffffff}\"",
            ITuple   t => ToJson(t),
            null => "null",
            _    => obj.ToString()
        };

        private string ToJson(IDictionary dictionary)
        {
            _stringBuilder.Append("{");
            foreach (DictionaryEntry entry in dictionary)
            {
                _stringBuilder.Append("\"");
                _stringBuilder.Append(entry.Key);
                _stringBuilder.Append("\":");
                _stringBuilder.Append(new JsonWriter(_ignoreNullOrDefaultValues).WriteToJson(entry.Value));
                _stringBuilder.Append(",");
            }
            _stringBuilder.Remove(_stringBuilder.Length - 1, 1);
            _stringBuilder.Append("}");
            return _stringBuilder.ToString();
        }

        private string ToJson(IEnumerable enumerable)
        {
            _stringBuilder.Append("[");
            foreach (var obj in enumerable)
            {
                _stringBuilder.Append(new JsonWriter(_ignoreNullOrDefaultValues).WriteToJson(obj));
                _stringBuilder.Append(",");
            }
            _stringBuilder.Remove(_stringBuilder.Length - 1, 1);
            _stringBuilder.Append("]");
            return _stringBuilder.ToString();
        }

        private string ToJson(ITuple tuple)
        {
            _stringBuilder.Append("{");
            for (var i = 0; i < tuple.Length; i++)
            {
                _stringBuilder.Append("\"Item");
                _stringBuilder.Append(i + 1);
                _stringBuilder.Append("\":");
                _stringBuilder.Append(new JsonWriter(_ignoreNullOrDefaultValues).WriteToJson(tuple[i]));
                _stringBuilder.Append(",");
            }
            _stringBuilder.Remove(_stringBuilder.Length - 1, 1);
            _stringBuilder.Append("}");
            return _stringBuilder.ToString();
        }
    }
}