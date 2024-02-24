namespace Kivikko.Json.Tests.Models.Model3;

public class Range { }

public class Range<T> : Range
{
    public int Version { get; set; }
    public List<T> Items { get; set; } = new();
}