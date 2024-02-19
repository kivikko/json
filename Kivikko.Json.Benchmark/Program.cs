using BenchmarkDotNet.Running;
using Kivikko.Json.Benchmark;

while (true)
{
    Console.WriteLine("1 - BenchmarkDotNet");
    Console.WriteLine("2 - Self-written Benchmark");
    Console.WriteLine("0 - Exit");
    
    var key = Console.ReadKey();
    Console.WriteLine();
    
    switch (key.Key)
    {
        case ConsoleKey.D1 or ConsoleKey.NumPad1:
            BenchmarkRunner.Run<JsonBenchmark>();
            break;
        
        case ConsoleKey.D2 or ConsoleKey.NumPad2:
            SelfWrittenBenchmark.Run(100000);
            break;
        
        case ConsoleKey.D0 or ConsoleKey.NumPad0:
            return;
    }
}