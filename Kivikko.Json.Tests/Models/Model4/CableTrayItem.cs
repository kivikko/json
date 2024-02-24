namespace Kivikko.Json.Tests.Models.Model4;

public class CableTrayItem : ProductItem
{
    public CableTrayShapeProxy Shape { get; set; }
    public string Thickness { get; set; }
    public double Offset { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
}