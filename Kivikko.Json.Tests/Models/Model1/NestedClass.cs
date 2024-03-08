namespace Kivikko.Json.Tests.Models.Model1;

public class NestedClass
{
    public bool? Bool { get; set; }
    public double? Double { get; set; }
    public int? Integer { get; set; }
    public TestEnum Enum { get; set; }
    public DateTime? DateTime { get; set; }
    public TimeSpan? Time { get; set; }
    public HashSet<int>? HashSet { get; set; }
    public string[]? StringArray { get; set; }
    public IEnumerable<string>? StringEnumerable { get; set; }
    public List<string>? StringList { get; set; }
    public Dictionary<int, string?>? Dictionary { get; set; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((NestedClass)obj);
    }
    private bool Equals(NestedClass other)
    {
        if (Bool != other.Bool) return false;
        if (!Nullable.Equals(Double, other.Double)) return false;
        if (Integer != other.Integer) return false;
        if (Enum != other.Enum) return false;
        if (!Nullable.Equals(DateTime, other.DateTime)) return false;
        if (!Nullable.Equals(Time, other.Time)) return false;
        if (StringArray is null && other.StringArray is null) return true;
        if (StringEnumerable is null && other.StringEnumerable is null) return true;
        if (StringList is null && other.StringList is null) return true;
        if (Dictionary is null && other.Dictionary is null) return true;
        if (StringArray is null || other.StringArray is null || !StringArray.SequenceEqual(other.StringArray)) return false;
        if (StringEnumerable is null || other.StringEnumerable is null || !StringEnumerable.SequenceEqual(other.StringEnumerable)) return false;
        if (StringList is null || other.StringList is null || !StringList.SequenceEqual(other.StringList)) return false;
        if (Dictionary is null || other.Dictionary is null || !Dictionary.SequenceEqual(other.Dictionary)) return false;
        return true;
    }

    // ReSharper disable NonReadonlyMemberInGetHashCode
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Bool);
        hashCode.Add(Double);
        hashCode.Add(Integer);
        hashCode.Add((int)Enum);
        hashCode.Add(DateTime);
        hashCode.Add(Time);
        hashCode.Add(StringArray);
        hashCode.Add(StringEnumerable);
        hashCode.Add(StringList);
        hashCode.Add(Dictionary);
        return hashCode.ToHashCode();
    }
}