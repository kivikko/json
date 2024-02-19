// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Kivikko.Json.Tests.Model;

public class TestClass
{
    public NestedClass? NestedClass { get; set; }
    public IEnumerable<NestedClass>? Classes { get; set; }
    public Dictionary<int, NestedClass>? ClassesDictionary { get; set; }
    public string? String { get; set; }
        
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((TestClass)obj);
    }
    private bool Equals(TestClass other)
    {
        if (!Equals(NestedClass, other.NestedClass)) return false;
        if (Classes is null && other.Classes is null) return true;
        if (ClassesDictionary is null && other.ClassesDictionary is null) return true;
        if (Classes is null || other.Classes is null || !Classes.SequenceEqual(other.Classes)) return false;
        if (ClassesDictionary is null || other.ClassesDictionary is null || !ClassesDictionary.SequenceEqual(other.ClassesDictionary)) return false;
        return String == other.String;
    }

    public override int GetHashCode() => HashCode.Combine(NestedClass, Classes, ClassesDictionary, String);
}