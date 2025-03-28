﻿using System;
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
        new JsonReader().ReadFromJson<T>(json.ToCharArray());

    public static object FromJson(string json, Type type) =>
        new JsonReader().ReadFromJson(json.ToCharArray(), type);

    public static string ToJson(object obj, bool ignoreNullOrDefaultValues = true) =>
        new JsonWriter(ignoreNullOrDefaultValues).WriteToJson(obj);

    public static bool TryLoad<T>(string path, out T value)
        where T : class =>
        TryLoad(path, Encoding.Default, out value);

    public static bool TryLoad<T>(string path, Encoding encoding, out T value)
        where T : class
    {
        if (!File.Exists(path))
        {
            value = default;
            return false;
        }
        
        var json = File.ReadAllText(path, encoding);
        var deserializeObject = FromJson<T>(json);
        value = deserializeObject;
        
        return deserializeObject is not null;
    }
    
    public static void Save(string path, object obj, bool ignoreNullOrDefaultValues = true) =>
        Save(path, obj, Encoding.Default, ignoreNullOrDefaultValues);
    
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

        public T ReadFromJson<T>(char[] json) =>
            ReadFromJson(json, typeof(T)) is T fromJson ? fromJson : default;

        public object ReadFromJson(char[] json, Type type)
        {
            _index = 0;
            return FromJsonPrivate(json, type);
        }

        private object FromJsonPrivate(char[] json, Type type)
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

        private object GetPrimitiveFromJson(char[] json, Type type)
        {
            var inQuotes = false;
            var slash = false;
            while (_index < json.Length)
            {
                var c = json[_index];
                switch (c, inQuotes, slash)
                {
                    case ('"',  _,     false): inQuotes = !inQuotes; break;
                    case ('"',  _,     _    ): slash = false; break;
                    case (']',  false, _    ): goto quit;
                    case ('}',  false, _    ): goto quit;
                    case (',',  false, _    ): goto quit;
                    case (':',  false, _    ): goto quit;
                    case (' ',  false, _    ): goto next;
                    case ('\n', false, _    ): goto next;
                    case ('\r', false, _    ): goto next;
                    case ('\t', false, _    ): goto next;
                    case ('\\', _,     false): slash = true; goto next;
                    case ('n',  _,     true ): slash = false; _stringBuilder.Append("\\"); break; 
                    case ('r',  _,     true ): slash = false; _stringBuilder.Append("\\"); break; 
                    case ('t',  _,     true ): slash = false; _stringBuilder.Append("\\"); break; 
                    case (_,    _,     true ): slash = false; break;
                }
                _stringBuilder.Append(c);
                next:
                _index++;
            }
            quit:
            var stringValue = _stringBuilder.ToString();
            _stringBuilder.Clear();
            return GetNullableValueFromJson(stringValue, type);
        }

        private object GetObjectFromJson(char[] json, Type type)
        {
            if (json.Length is 0)
                return default;

            var instance = Activator.CreateInstance(type);
            var state = State.Default;
            
            string memberName = null;
            
            while (_index < json.Length)
            {
               var c = json[_index];
                switch (c, state)
                {
                    case ('{',   State.Default  ): _index++; state = State.WaitName; continue;
                    case (',',   State.Default  ): _index++; state = State.WaitName; continue;
                    case ('}',   State.WaitName ): _index++; return instance;
                    case (' ',   State.WaitName ): _index++; continue;
                    case ('\n',  State.WaitName ): _index++; continue;
                    case ('\r',  State.WaitName ): _index++; continue;
                    case ('\t',  State.WaitName ): _index++; continue;
                    case ('"',   State.WaitName ): _index++; state = State.ReadName;   continue;
                    case (_,     State.WaitName ): _index++; state = State.ReadName; _stringBuilder.Append(c); continue;
                    case ('"',   State.ReadName ): _index++; state = State.Default;    continue;
                    case (':',   State.ReadName ): _index++; ReadValue();              continue;
                    case (_,     State.ReadName ): _index++; _stringBuilder.Append(c); continue;
                    case (':',   State.Default  ): _index++; ReadValue();              continue;
                    case ('}',   State.Default  ): _index++; return instance;
                    case (_, not State.ReadValue): _index++; continue;
                    case (_,     State.ReadValue) when memberName is not null:
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
                            Reset();
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

                        else if (propertyType.IsGenericType)
                        {
                            var interfaces = propertyType.GetInterfaces();

                            if (interfaces.Any(i => i == typeof(IDictionary)) &&
                                value is IDictionary valueDictionary)
                            {
                                var genericArgs = propertyType.GetGenericArguments();
                                var keyType = genericArgs[0];
                                var valueType = genericArgs[1];
                                var typedDictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
                                foreach (DictionaryEntry entry in valueDictionary) typedDictionary.Add(entry.Key, entry.Value);
                                setValue(typedDictionary);
                            }

                            else if (
                                interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>)) &&
                                propertyType.GetGenericArguments()[0] is { } setElementType &&
                                value is ICollection setValueCollection)
                            {
                                var typedSet = Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(setElementType), setValueCollection);
                                setValue(typedSet);
                            }

                            else if (
                                interfaces.Any(i => i == typeof(IEnumerable)) &&
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

                        else
                        {
                            setValue(value);
                        }

                        Reset();
                        continue;
                    }
                    default:
                        _index++;
                        continue;
                }
            }
            
            return instance;

            void ReadValue()
            {
                state = State.ReadValue;
                if (_stringBuilder.Length is 0) return;
                memberName = _stringBuilder.ToString();
                _stringBuilder.Clear();
            }

            void Reset()
            {
                state = State.Default;
                memberName = null;
                _stringBuilder.Clear();
            }
        }
        
        private IDictionary GetDictionaryFromJson(char[] json, Type type)
        {
            var dictionary = (IDictionary)Activator.CreateInstance(type);
                
            if (json.Length is 0)
                return dictionary;

            if (IsNull(json, _index))
                return dictionary;
            
            var genericTypeArguments = type.GenericTypeArguments.Length switch
            {
                < 2 => type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.Name == nameof(IDictionary) + "`2")?.GenericTypeArguments,
                _ => type.GenericTypeArguments
            };

            var keyType   = genericTypeArguments?.ElementAtOrDefault(0);
            var valueType = genericTypeArguments?.ElementAtOrDefault(1);

            object key = null;
                
            while (_index < json.Length)
            {
                switch (json[_index], key)
                {
                    case ('{', null): _index++; continue;
                    case ('"', null): key = FromJsonPrivate(json, keyType); break;
                    case (':', _): _index++; continue;
                    case (',', _): _index++; continue;
                    case ('}', _): _index++; return dictionary;
                    case (_, not null): dictionary.Add(key, FromJsonPrivate(json, valueType)); key = null; continue;
                    default: _index++; continue;
                }
            }

            return dictionary;
        }

        private IEnumerable GetEnumerableFromJson(char[] json, Type type)
        {
            var list = new List<object>();
            var elementType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments.FirstOrDefault();
            
            if (elementType is null)
                return Array.Empty<object>();

            if (IsNull(json, _index))
                return Array.Empty<object>();
            
            var inBrackets = false;
            
            while (_index < json.Length)
            {
                switch (json[_index], isInBreckets: inBrackets)
                {
                    case (' ',  _    ): _index++; continue;
                    case ('\n', _    ): _index++; continue;
                    case ('\r', _    ): _index++; continue;
                    case ('\t', _    ): _index++; continue;
                    case ('[',  false): _index++; inBrackets = true; continue;
                    case (',',  _    ): _index++; continue;
                    case (']',  _    ): _index++; goto quit;
                    case ('"',  false) when IsNull(json.Skip(_index + 1).Take(4)): _index++; goto quit;
                    case ('n',  false) when IsNull(json.Skip(_index).Take(4)):     _index++; goto quit;
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

        private static bool IsNull(char[] json, int index) =>
            index < json.Length + 3 &&
            json[index] is 'n' &&
            IsNull(json.Skip(index).Take(4));

        private static bool IsNull(IEnumerable<char> c)
        {
            var a = c.ToArray();
            return a.Length > 3 && a[0] is 'n' && a[1] is 'u' && a[2] is 'l' && a[3] is 'l';
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
            [typeof(string)]   = json => json is null or "null" or "" ? null : TrimQuotesOneTime(json),
            [typeof(Guid)]     = json => Guid   .TryParse(json.Trim(Quote), out var value) ? value : default,
        };

        private static string TrimQuotesOneTime(string value)
        {
            if (value.Length < 2)
                return value;

            var startChar = value[0];
            var endChar = value[value.Length - 1];

            return (startChar, endChar) switch
            {
                ('"', '"') => value.Substring(1, value.Length - 2),
                ('"',  _ ) => value.Substring(1),
                ( _ , '"') => value.Substring(0, value.Length - 1),
                _ => value
            };
        }
        
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
        private enum State { Default, WaitName, ReadName, ReadValue }
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
                case string str: return StringToJson(str);
                case IDictionary dictionary: return new JsonWriter(_ignoreNullOrDefaultValues).ToJson(dictionary);
                case IEnumerable enumerable: return new JsonWriter(_ignoreNullOrDefaultValues).ToJson(enumerable);
            }

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
            }

            if (_stringBuilder[_stringBuilder.Length - 1] == ',')
                _stringBuilder.Remove(_stringBuilder.Length - 1, 1);
            
            _stringBuilder.Append("}");

            return _stringBuilder.ToString();
        }

        private string StringToJson(string str)
        {
            _stringBuilder.Append("\"");
            foreach (var c in str)
            {
                switch (c)
                {
                    case '\\': _stringBuilder.Append(@"\\");  break;
                    case '\"': _stringBuilder.Append("\\\""); break;
                    case '\n': _stringBuilder.Append("\\n");  break;
                    case '\r': _stringBuilder.Append("\\r");  break;
                    case '\t': _stringBuilder.Append("\\t");  break;
                    default:   _stringBuilder.Append(c);      break;
                }
            }
            _stringBuilder.Append("\"");
            return _stringBuilder.ToString();
        }

        private static bool IsDefaultValue(Type type, object obj)
        {
            if (obj is null) return true;
            if (type.IsValueType && obj.Equals(Activator.CreateInstance(type))) return true;
            if (type == typeof(string) && string.IsNullOrEmpty((string)obj)) return true;
            if (obj is IEnumerable enumerable && !enumerable.OfType<object>().Any()) return true;
            return false;
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
            if (_stringBuilder[_stringBuilder.Length - 1] == ',')
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
            if (_stringBuilder[_stringBuilder.Length - 1] == ',')
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
            if (_stringBuilder[_stringBuilder.Length - 1] == ',')
                _stringBuilder.Remove(_stringBuilder.Length - 1, 1);
            _stringBuilder.Append("}");
            return _stringBuilder.ToString();
        }
    }
}