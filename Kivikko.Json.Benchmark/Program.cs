using BenchmarkDotNet.Running;
using Kivikko.Json.Benchmark;
using Kivikko.Json.Benchmark.Model;

while (true)
{
    Console.WriteLine("1 - BenchmarkDotNet");
    Console.WriteLine("2 - Self-written Benchmark");
    Console.WriteLine("0 - Exit");
    
    var line = Console.ReadLine();
    Console.WriteLine();
    
    switch (line)
    {
        case "1":
            BenchmarkRunner.Run<JsonBenchmark>();
            break;
        
        case "2":
            SelfWrittenBenchmark.Run(ReadInstanceType(), count: 10000);
            break;
        
        case "0":
            return;
    }
}

TestFactory.InstanceType ReadInstanceType()
{
    WriteInstruction();
    var line = Console.ReadLine();
    return int.TryParse(line, out var i) ? (TestFactory.InstanceType)i : 0;
}

void WriteInstruction()
{
    Console.WriteLine("Choose an object to benchmark:");
    
    foreach (var type in Enum.GetValues(typeof(TestFactory.InstanceType)))
        Console.WriteLine($"{(int)type} - {type}");
}
