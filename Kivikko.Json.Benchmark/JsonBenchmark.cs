using BenchmarkDotNet.Attributes;
using Kivikko.Json.Benchmark.Model;

namespace Kivikko.Json.Benchmark;

public class JsonBenchmark
{
    private object? _instance;
    private Type?   _instanceType;
    private string  _instanceJson = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _instance     = TestFactory.CreateInstance(TestFactory.InstanceType.ProductArray);
        _instanceJson = JsonUtils.ToJson(_instance);
        _instanceType = _instance.GetType();
    }
    
    [Benchmark] public void SystemTextJsonSerialization()   => System.Text.Json.JsonSerializer.Serialize(_instance);
    [Benchmark] public void SystemTextJsonDeserialization() => System.Text.Json.JsonSerializer.Deserialize(_instanceJson, _instanceType);
    
    [Benchmark] public void NewtonsoftJsonSerialization()   => Newtonsoft.Json.JsonConvert.SerializeObject(_instance);
    [Benchmark] public void NewtonsoftJsonDeserialization() => Newtonsoft.Json.JsonConvert.DeserializeObject(_instanceJson, _instanceType);
    
    [Benchmark] public void JsonUtilsSerialization()   => JsonUtils.ToJson(_instance);
    [Benchmark] public void JsonUtilsDeserialization() => JsonUtils.FromJson(_instanceJson, _instanceType);
}