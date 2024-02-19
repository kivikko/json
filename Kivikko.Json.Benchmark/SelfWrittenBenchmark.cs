using System.Diagnostics;
using Kivikko.Json.Benchmark.Model;

namespace Kivikko.Json.Benchmark;

public static class SelfWrittenBenchmark
{
    public static void Run(int count)
    {
        var instance     = TestFactory.CreateObject();
        var instanceJson = JsonUtils.ToJson(instance);
        var stopwatch = new Stopwatch();

        // Warm-up
        stopwatch.Start();
        
        for (var i = 0; i < 1000; i++)
        {
            System.Text.Json.JsonSerializer.Serialize(instance);
            System.Text.Json.JsonSerializer.Deserialize<TestClass>(instanceJson);
            
            Newtonsoft.Json.JsonConvert.SerializeObject(instance);
            Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass>(instanceJson);
            
            JsonUtils.ToJson(instance);
            JsonUtils.FromJson<TestClass>(instanceJson);
        }
        
        stopwatch.Stop();
        Console.WriteLine($"Warm-up time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        
        RunAction("Newtonsoft.Json Serialize",   () => Newtonsoft.Json.JsonConvert.SerializeObject(instance));
        RunAction("Newtonsoft.Json Deserialize", () => Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass>(instanceJson));
        
        RunAction("System.Text.Json Serialize",   () => System.Text.Json.JsonSerializer.Serialize(instance));
        RunAction("System.Text.Json Deserialize", () => System.Text.Json.JsonSerializer.Deserialize<TestClass>(instanceJson));
        
        RunAction("Kivikko.Json Serialize",   () => JsonUtils.ToJson(instance));
        RunAction("Kivikko.Json Deserialize", () => JsonUtils.FromJson<TestClass>(instanceJson));

        Console.WriteLine();
        
        return;

        void RunAction(string title, Action action)
        {
            stopwatch.Restart();
            for (var i = 0; i < count; i++) action();
            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine(title);
            Console.WriteLine($"Time: {stopwatch.Elapsed.TotalMilliseconds / count:f6} ms / {stopwatch.Elapsed.TotalMilliseconds:f4} ms");
        }
    }
}