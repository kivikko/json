namespace Kivikko.Json.Tests.Models.Model3;

public class Product : Item
{
    public bool IsValid { get; set; }
    public List<ProductItem> ProductItems { get; set; } = new();
    public int? Group { get; set; }
}