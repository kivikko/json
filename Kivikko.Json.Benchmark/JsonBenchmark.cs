using BenchmarkDotNet.Attributes;
using Kivikko.Json.Benchmark.Model;

namespace Kivikko.Json.Benchmark;

public class JsonBenchmark
{
    private TestClass? _instance;
    private string _instanceJson = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _instance     = TestFactory.CreateObject();
        _instanceJson = JsonUtils.ToJson(_instance);
    }

    [Benchmark] public void NewtonsoftJsonSerialization()   => Newtonsoft.Json.JsonConvert.SerializeObject(_instance);
    [Benchmark] public void NewtonsoftJsonDeserialization() => Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass>(_instanceJson);
    
    [Benchmark] public void SystemTextJsonSerialization()   => System.Text.Json.JsonSerializer.Serialize(_instance);
    [Benchmark] public void SystemTextJsonDeserialization() => System.Text.Json.JsonSerializer.Deserialize<TestClass>(_instanceJson);
    
    [Benchmark] public void JsonUtilsSerialization()   => JsonUtils.ToJson(_instance);
    [Benchmark] public void JsonUtilsDeserialization() => JsonUtils.FromJson<TestClass>(_instanceJson);
}