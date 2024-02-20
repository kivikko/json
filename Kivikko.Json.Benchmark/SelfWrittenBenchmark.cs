using System.Diagnostics;
using Kivikko.Json.Benchmark.Model;

namespace Kivikko.Json.Benchmark;

public static class SelfWrittenBenchmark
{
    public static void Run(TestFactory.InstanceType factoryInstanceType, int count)
    {
        Console.WriteLine();
        Console.WriteLine(factoryInstanceType);
        Console.WriteLine(new string('-', factoryInstanceType.ToString().Length));
        Console.WriteLine($"count: {count}");
        
        var instance = TestFactory.CreateInstance(factoryInstanceType);
        if (instance is null) return;
        var instanceJson = JsonUtils.ToJson(instance);
        var instanceType = instance.GetType();
        var stopwatch = new Stopwatch();

        // Warm-up
        stopwatch.Start();
        
        for (var i = 0; i < 1000; i++)
        {
            System.Text.Json.JsonSerializer.Serialize(instance);
            System.Text.Json.JsonSerializer.Deserialize(instanceJson, instanceType);
            
            Newtonsoft.Json.JsonConvert.SerializeObject(instance);
            Newtonsoft.Json.JsonConvert.DeserializeObject(instanceJson, instanceType);
            
            JsonUtils.ToJson(instance);
            JsonUtils.FromJson(instanceJson, instanceType);
        }
        
        stopwatch.Stop();
        
        Console.WriteLine($"Warm-up time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        
        var systemSerialization   = RunAction("System.Text.Json Serialize",   () => System.Text.Json.JsonSerializer.Serialize(instance));
        var systemDeserialization = RunAction("System.Text.Json Deserialize", () => System.Text.Json.JsonSerializer.Deserialize(instanceJson, instanceType));
        
        var newtonsoftSerialization   = RunAction("Newtonsoft.Json Serialize",   () => Newtonsoft.Json.JsonConvert.SerializeObject(instance));
        var newtonsoftDeserialization = RunAction("Newtonsoft.Json Deserialize", () => Newtonsoft.Json.JsonConvert.DeserializeObject(instanceJson, instanceType));
        
        var kivikkoSerialization   = RunAction("Kivikko.Json Serialize",   () => JsonUtils.ToJson(instance));
        var kivikkoDeserialization = RunAction("Kivikko.Json Deserialize", () => JsonUtils.FromJson(instanceJson, instanceType));

        var b1 = (newtonsoftSerialization   / systemSerialization)  .ToString("F2");
        var b2 = (newtonsoftDeserialization / systemDeserialization).ToString("F2");
        var c1 = (kivikkoSerialization   / newtonsoftSerialization)  .ToString("F2");
        var c2 = (kivikkoDeserialization / newtonsoftDeserialization).ToString("F2");
        var d1 = (kivikkoSerialization   / systemSerialization)  .ToString("F2");
        var d2 = (kivikkoDeserialization / systemDeserialization).ToString("F2");
        
        Console.WriteLine();
        Console.WriteLine("                | A |   B   |   C   |   D");
        Console.WriteLine("----------------|---|-------|-------|-------");
        Console.WriteLine($"Serialization   | 1 | {Format(b1)} | {Format(c1)} | {Format(d1)}");
        Console.WriteLine($"Deserialization | 1 | {Format(b2)} | {Format(c2)} | {Format(d2)}");
        Console.WriteLine();
        Console.WriteLine("A - System.Text.Json");
        Console.WriteLine("B - Newtonsoft.Json / System.Text.Json");
        Console.WriteLine("C - Kivikko.Json / Newtonsoft.Json");
        Console.WriteLine("D - Kivikko.Json / System.Text.Json");
        Console.WriteLine();
        
        return;

        TimeSpan RunAction(string title, Action action)
        {
            stopwatch.Restart();
            for (var i = 0; i < count; i++) action();
            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine(title);
            Console.WriteLine($"Time: {stopwatch.Elapsed.TotalMilliseconds / count:f6} ms / {stopwatch.Elapsed.TotalMilliseconds:f4} ms");
            return stopwatch.Elapsed;
        }
    }

    private static string Format(string x) => $"{new string(' ', Math.Max(0, 5 - x.Length))}{x}";
}